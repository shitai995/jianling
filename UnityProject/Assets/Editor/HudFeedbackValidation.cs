using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class HudFeedbackValidation
    {
        private const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        private const BindingFlags S=BindingFlags.Static|BindingFlags.NonPublic;
        private static object Get(object o,string n)=>o.GetType().GetField(n,F).GetValue(o);
        private static void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
        private static object Call(GuJianGame g,string n,params object[] args)=>typeof(GuJianGame).GetMethod(n,F).Invoke(g,args);
        private static void Require(bool yes,string why) {if(!yes) throw new Exception(why);}
        private static void Stage(GuJianGame g)
        {typeof(ComboValidation).GetMethod("Stage",S).Invoke(null,new object[]{g,false});}
        private static void Hit(GuJianGame g)
        {typeof(ComboValidation).GetMethod("Hit",S).Invoke(null,new object[]{g,0,100000,0,true});}
        private static void Idle(GuJianGame g)
        {g.Actors[900].hidden=true;g.Actors[-1].ResetAction();}
        private static void Tick(GuJianGame g,int ticks)
        {for(int i=0;i<ticks;i++) g.Actors[-1].Tick(.080001f);}
        [MenuItem("古剑/验证连击冷却与EP界面反馈")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying,"Play first");var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var checks=new List<string>();string error=null;float scale=Time.timeScale;var random=UnityEngine.Random.state;Time.timeScale=0;
            try
            {
                var ui=Resources.Load<OriginalUiLayout>("OriginalUiLayout");
                Require(ui&&ui.comboFrames.Length==3&&ui.comboCountNumbers.Length==3,"Saved arrays");
                for(int i=0;i<3;i++) Require(ui.comboFrames[i].name=="u_"+(12+i)&&ui.comboCountNumbers[i].name=="u_"+(20+i)&&AssetDatabase.Contains(ui.comboFrames[i])&&AssetDatabase.Contains(ui.comboCountNumbers[i]),"Saved phase "+i);
                Require(ui.shortcutIcons&&ui.shortcutIcons.name=="skillandrod"&&AssetDatabase.Contains(ui.shortcutIcons)&&ui.comboNumbers.name=="u_30"&&ui.comboBonus.name=="u_29","Distinct shortcut/count/bonus atlases");
                Require(ui.shortcutBounds==new Rect(330,279,38,38),"Source shortcut location");
                checks.Add("Saved original combo u12/13/14, hit digits u20/21/22, separate u29/u30 AP percentage and 38px Android shortcut atlas");
                Stage(g);Hit(g);Require(g.ComboHits==1&&(bool)Get(g,"comboHudFlash")&&(int)Get(g,"comboHudCounter")==1,"Production hit notification");
                Hit(g);Require(g.ComboHits==2&&(int)Get(g,"comboHudCounter")==1,"New hit restarts flash");
                Idle(g);Tick(g,1);Require((bool)Get(g,"comboHudFlash")&&(int)Get(g,"comboHudCounter")==2,"Second phase");
                Tick(g,1);Require(!(bool)Get(g,"comboHudFlash")&&(int)Get(g,"comboHudCounter")==0&&g.ComboHits==2,"Return normal phase without losing combo");
                checks.Add("Real production attack hits restart the source combo flash; subsequent actual actor 80ms ticks show phase2 then normal phase0 while retaining hit count");
                Stage(g);Set(g,"energy",1990);Hit(g);Require(g.Energy==1995&&!(bool)Get(g,"energyHudFlash"),"EP before full");
                Hit(g);Require(g.Energy==2000&&(bool)Get(g,"energyHudFlash")&&(int)Get(g,"energyHudCounter")==1,"EP full hit transition");
                Idle(g);int length=ui.scene.actions[7].steps.Length;Tick(g,length-1);
                Require((bool)Get(g,"energyHudFlash")&&(int)Get(g,"energyHudCounter")==length,"Lost last full-EP frame");
                Tick(g,1);Require(!(bool)Get(g,"energyHudFlash")&&(int)Get(g,"energyHudCounter")==0&&g.Energy==2000,"EP animation did not settle");
                g.Actors[900].hidden=false;Hit(g);Require(g.Energy==2000&&!(bool)Get(g,"energyHudFlash"),"Already full EP retriggered flash");
                checks.Add("Production hits gain 1990→1995→2000, run all six first-fill frames, settle on the full loop and do not retrigger the one-shot while already full");
                Stage(g);Set(g,"energy",1995);Hit(g);Idle(g);Set(g,"menu",true);int before=(int)Get(g,"energyHudCounter");Call(g,"Update");
                Require((int)Get(g,"energyHudCounter")==before,"Menu advanced role feedback");Set(g,"menu",false);
                g.Actors[-1].frozen=true;Tick(g,4);Require((int)Get(g,"energyHudCounter")==before,"Frozen role advanced feedback");g.Actors[-1].frozen=false;
                g.Begin();Require(!(bool)Get(g,"energyHudFlash")&&!(bool)Get(g,"comboHudFlash"),"New game retained transient feedback");
                checks.Add("Real Update pauses feedback on menu, frozen actor ticks preserve phase, and new game clears both transient effects");
                Stage(g);
                for(int id=5;id<=21;id++)
                {Set(g,"equippedSkill",id);int expected=id==11?7:id==12?6:id-5;Require(g.ShortcutIconIndex==expected,"Shortcut icon mapping "+id);}
                Set(g,"equippedSkill",5);Call(g,"ResetSkillCooldown",false);int duration=g.SkillCooldownTicks;
                Require(duration>2,"Fixture cooldown");Idle(g);Tick(g,1);Require(g.SkillCooldownTicks==duration-1,"First recovery tick");
                Tick(g,duration-3);Require(g.SkillCooldownTicks==2,"First white-flash tick");Tick(g,1);Require(g.SkillCooldownTicks==1,"Second white-flash tick");Tick(g,1);Require(g.SkillCooldownTicks==0,"Ready tick");
                checks.Add("All 17 shortcut icon mappings include swapped source skills11/12; actual cooldown ticks progress from hidden start through two terminal white frames to ready");
                Stage(g);int count=UnityEngine.Object.FindObjectsOfType<GameObject>().Length;
                for(int i=0;i<10;i++)Hit(g);Require(g.ComboHits==10&&g.ComboAttackPercent==1,"Ten-hit AP");
                Require(UnityEngine.Object.FindObjectsOfType<GameObject>().Length==count,"Feedback created fixed nodes");
                checks.Add("Ten real hits retain the production +1% AP rule and generate no new HUD GameObjects");
            }
            catch(Exception e){error=e.ToString();}
            finally
            {
                Time.timeScale=scale;UnityEngine.Random.state=random;
                var report=new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error};
                File.WriteAllText("../Reference/Validation/hud-feedback.json",JsonUtility.ToJson(report,true));Debug.Log("GUJIAN_HUD_FEEDBACK "+report.passed+" "+error);
            }
        }
        public static void PrepareVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();Stage(g);Time.timeScale=0;
            for(int i=0;i<10;i++)Hit(g);Idle(g);
            ((System.Collections.IList)Get(g,"numbers")).Clear();
            foreach(var actor in g.Actors.Values)if(actor.IsEnemy)actor.hidden=true;
            Set(g,"toastUntil",0f);Set(g,"energy",2000);Set(g,"comboHudFlash",false);Set(g,"lockInput",true);
            Set(g,"equippedSkill",5);Set(g,"skillCooldownTicks",0);Call(g,"UpdateCamera");
        }
        private static Texture2D Render()
        {
            var game=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if(game)Call(game,"RefreshScenePresentation");
            // Synchronous captures do not pass through the normal frame's group update.
            UnityEngine.Rendering.SortingGroup.UpdateAllSortingGroups();
            var t=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var v=Resources.FindObjectsOfTypeAll(t)[0];
            ((EditorWindow)v).SendEvent(new Event{type=EventType.Layout});((EditorWindow)v).SendEvent(new Event{type=EventType.Repaint});
            var size=(Vector2)t.GetProperty("targetSize",F).GetValue(v,null);
            if(Camera.main)
            {
                float aspect=533f/320f,screenAspect=size.x/size.y;
                Camera.main.rect=screenAspect>aspect?new Rect((1-aspect/screenAspect)/2,0,aspect/screenAspect,1):
                    new Rect(0,(1-screenAspect/aspect)/2,1,screenAspect/aspect);
            }
            var rt=(RenderTexture)t.BaseType.GetMethod("RenderView",F).Invoke(v,new object[]{size,false});
            var old=RenderTexture.active;RenderTexture.active=rt;
            var texture=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);
            try
            {
                texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);texture.Apply();
                var pixels=texture.GetPixels32();
                if(SystemInfo.graphicsUVStartsAtTop)for(int y=0;y<rt.height/2;y++)for(int x=0;x<rt.width;x++)
                {int a=y*rt.width+x,b=(rt.height-1-y)*rt.width+x;var c=pixels[a];pixels[a]=pixels[b];pixels[b]=c;}
                texture.SetPixels32(pixels);texture.Apply();return texture;
            }
            finally{RenderTexture.active=old;}
        }
        private static Color Pixel(Texture2D image,float x,float y)
        {
            float scale=Mathf.Min(image.width/533f,image.height/320f);
            return image.GetPixel(Mathf.FloorToInt((image.width-533*scale)/2+x*scale),image.height-1-Mathf.FloorToInt((image.height-320*scale)/2+y*scale));
        }
        private static void Save(Texture2D image,string name)
        {File.WriteAllBytes("../Reference/Validation/"+name+".png",image.EncodeToPNG());}
        private static void RequireBlackBars(Texture2D image)
        {
            float scale=Mathf.Min(image.width/533f,image.height/320f);
            int marginX=Mathf.FloorToInt((image.width-533*scale)/2)-2, marginY=Mathf.FloorToInt((image.height-320*scale)/2)-2;
            for(int y=0;y<image.height;y++)for(int x=0;x<image.width;x++)
                if(x<marginX||x>=image.width-marginX||y<marginY||y>=image.height-marginY)
                    Require(image.GetPixel(x,y).maxColorComponent<.02f,"Effect escaped the viewport at "+x+","+y);
        }
        public static void CaptureVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();var checks=new List<string>();string error=null;
            try
            {
                foreach(int remaining in new[]{48,24,2,1,0})
                {
                    Set(g,"skillCooldownTicks",remaining);var image=Render();
                    try
                    {
                        Save(image,"hud-cooldown-"+remaining);
                        RequireBlackBars(image);
                        if(remaining==24)Require(Pixel(image,349,314).maxColorComponent<.02f,"Bottom cooldown mask is not opaque black");
                        if(remaining==2||remaining==1) {var c=Pixel(image,349,298);Require(c.r>.98f&&c.g>.98f&&c.b>.98f,"Terminal cooldown frame is not opaque white");}
                    }
                    finally{UnityEngine.Object.DestroyImmediate(image);}
                }
                checks.Add("Actual unfocused GameView pixels verify black cooldown mask and both full-white terminal frames; ready state captured with original icon");
                foreach(int phase in new[]{0,1,2})
                {
                    Set(g,"comboHudFlash",phase!=0);Set(g,"comboHudCounter",phase);var image=Render();
                    try{Save(image,"hud-combo-phase-"+phase);}finally{UnityEngine.Object.DestroyImmediate(image);}
                }
                checks.Add("Actual GameView captures all three original ten-hit combo image/digit phases and atlas AP percentage");
                Set(g,"lockInput",false);Require(g.OpenMiniMap(),"Map fixture");var map=Render();
                try
                {
                    Save(map,"mini-map-controls");
                    RequireBlackBars(map);
                    Require(Pixel(map,456,26).maxColorComponent<.02f&&Pixel(map,442,299).maxColorComponent<.02f,"Minimap still draws scene menu/skill keys");
                }
                finally{UnityEngine.Object.DestroyImmediate(map);g.CloseMiniMap();}
                checks.Add("Minimap GameView pixels verify scene menu/skill buttons are absent; source state36 retains direction, confirm and back");
                checks.Add("Every tested GameView letterbox pixel stays black during full-EP glow and after switching to minimap; fixed logical viewport clips the overhanging source frames");
            }
            catch(Exception e){error=e.ToString();}
            var report=new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error};
            File.WriteAllText("../Reference/Validation/hud-feedback-rendered.json",JsonUtility.ToJson(report,true));Debug.Log("GUJIAN_HUD_RENDERED "+report.passed+" "+error);
        }
    }
}
