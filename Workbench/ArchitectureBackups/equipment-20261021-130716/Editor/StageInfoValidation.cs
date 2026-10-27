using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class StageInfoValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private const BindingFlags S = BindingFlags.Static | BindingFlags.NonPublic;
        private static object Get(GuJianGame g, string n) => typeof(GuJianGame).GetField(n,F).GetValue(g);
        private static void Set(GuJianGame g, string n, object v) => typeof(GuJianGame).GetField(n,F).SetValue(g,v);
        private static object Call(GuJianGame g, string n, params object[] a) => typeof(GuJianGame).GetMethod(n,F).Invoke(g,a);
        private static int I(GuJianGame g, string n) => (int)Get(g,"stageInfo"+n);
        private static bool Visible(GuJianGame g) => (bool)Get(g,"stageInfoVisible");
        private static void Require(bool yes, string message) { if (!yes) throw new Exception(message); }
        private static void Tick(GuJianGame g, int count) { for (int i=0;i<count;i++) Call(g,"TickStageInfo",.080001f); }
        private static void Stage(GuJianGame g)
        {
            g.Begin();g.Machine.Clear();g.Machine.Values["event"]=-999;g.Machine.Enqueue("200");g.Machine.Tick();
            foreach(var actor in g.Actors.Values) {actor.locked=true;if(actor.IsEnemy)actor.hidden=true;}
            Set(g,"lockInput",true);Call(g,"UpdateCamera");
        }
        private static void Report(string name,List<string> checks,string error)
        {
            var report=new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error};
            File.WriteAllText("../Reference/Validation/"+name+".json",JsonUtility.ToJson(report,true));
            Debug.Log("GUJIAN_"+name+" "+report.passed+" "+error);
        }
        [MenuItem("古剑/验证场景名滑入与停留")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying,"Play first");var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var checks=new List<string>();string error=null;float scale=Time.timeScale;Time.timeScale=0;
            try
            {
                var ui=Resources.Load<OriginalUiLayout>("OriginalUiLayout");
                Require(ui&&AssetDatabase.Contains(ui)&&ui.stageNameFontSize==18&&ui.stageNameFontHeight==24,"Saved font settings");
                Stage(g);Require(g.StageName=="剑冢入口"&&Visible(g)&&I(g,"Counter")==0,"Original stage200 x4C command");
                Require(I(g,"Width")>0&&I(g,"X")==((533-I(g,"Width"))>>1),"Measured centered text");
                checks.Add("Original stage200 script triggers x4C with the saved 18px font / editable 24px rise metric and centered measured title");
                g.Machine.Command(0x49,new object[]{"retained notification"});string toast=(string)Get(g,"toast");
                g.Machine.Command(0x4C,new object[0]);Require((string)Get(g,"toast")==toast&&!g.Machine.Running&&!g.DialogueVisible,"x4C is nonblocking and must not overwrite another notification");
                int initial=-10-((533+I(g,"Width"))>>1);
                Require(I(g,"Line1")==initial&&I(g,"Line2")==initial+I(g,"Width")+20&&I(g,"OffsetY")==0,"Source initial geometry");
                int count=UnityEngine.Object.FindObjectsOfType<GameObject>().Length;
                Tick(g,1);Require(I(g,"Line1")==initial+30&&I(g,"OffsetY")==0,"30px line entry");
                int slideTicks=1;while(I(g,"Line2")<533){Tick(g,1);slideTicks++;Require(slideTicks<100,"Slide did not finish");}
                Require(I(g,"OffsetY")==0,"Rise began before source line2 boundary check");
                Tick(g,1);Require(I(g,"Line1")==I(g,"X")-10&&I(g,"Line2")==533&&I(g,"OffsetY")==1,"Clamped lines and first 1px rise");
                checks.Add("Both source lines enter by 30px per 80ms tick; overshoot is clamped on the next tick before the first 1px text rise");
                Tick(g,ui.stageNameFontHeight-1);Require(I(g,"OffsetY")==24&&I(g,"Counter")==0,"Final rise frame");
                Tick(g,24);Require(Visible(g)&&I(g,"Counter")==24,"All 24 hold ticks");Tick(g,1);
                Require(!Visible(g)&&I(g,"Counter")==0&&I(g,"OffsetY")==0,"One-shot completion");
                Require(UnityEngine.Object.FindObjectsOfType<GameObject>().Length==count,"Presentation generated fixed objects");
                checks.Add("Title rises exactly 24 single-pixel ticks, holds all 24 ticks then disappears, without creating HUD GameObjects");
                g.Machine.Command(0x4C,new object[0]);Call(g,"TickStageInfo",.031f);Require(I(g,"Line1")==initial,"Fractional tick ran early");
                Call(g,"TickStageInfo",.04901f);Require(I(g,"Line1")==initial+30,"Fractional accumulation lost");
                g.Machine.Command(0x4C,new object[0]);Call(g,"TickStageInfo",.24001f);Require(I(g,"Line1")==initial+90,"Batched elapsed ticks");
                g.Machine.Command(0x4C,new object[0]);Require(I(g,"Line1")==initial&&I(g,"OffsetY")==0&&(float)Get(g,"stageInfoElapsed")==0,"Restart retained previous phase");
                checks.Add("Fractional elapsed time accumulates to 80ms, delayed frames catch up, and repeated x4C restarts all phases");
                g.Begin();g.Machine.Clear();Require(!Visible(g),"New game retained stage overlay");
                g.Machine.Command(0x21,new object[]{"未请求显示的场景"});Require(!Visible(g),"Loading a title implicitly displayed it");
                checks.Add("Begin clears transient presentation; x21 changes the stage name without implicitly requesting an overlay");
            }
            catch(Exception e){error=e.ToString();}
            finally{Time.timeScale=scale;Report("stage-info",checks,error);}
        }

        private static GuJianGame liveGame;
        private static int livePhase, liveBefore;
        private static double liveNext, liveDeadline;
        private static float liveScale;
        private static List<string> liveChecks;
        public static void RunLive()
        {
            Require(EditorApplication.isPlaying&&liveGame==null,"Play first; only one live validation at a time");
            liveGame=UnityEngine.Object.FindObjectOfType<GuJianGame>();liveScale=Time.timeScale;Time.timeScale=0;
            Stage(liveGame);Set(liveGame,"menu",true);liveBefore=I(liveGame,"Line1");
            livePhase=0;liveChecks=new List<string>();liveNext=EditorApplication.timeSinceStartup+.35;
            liveDeadline=EditorApplication.timeSinceStartup+12;Time.timeScale=1;EditorApplication.update+=LiveUpdate;
        }
        private static void LiveUpdate()
        {
            try
            {
                Require(EditorApplication.isPlaying&&liveGame,"Editor stopped during live test");
                double now=EditorApplication.timeSinceStartup;
                Require(now<liveDeadline,"Live stage title timed out");if(now<liveNext)return;
                if(livePhase==0)
                {
                    Require((bool)Get(liveGame,"menu")&&I(liveGame,"Line1")>liveBefore,"Real menu Update paused global stage feedback");
                    liveChecks.Add("Actual background Unity Update advances stage lines while the game menu is open");
                    Set(liveGame,"menu",false);liveGame.Machine.Command(0x29,new object[]{"场景名演出验证"});
                    liveGame.Machine.Command(0x4C,new object[0]);liveBefore=I(liveGame,"Line1");liveNext=now+.35;livePhase++;
                }
                else if(livePhase==1)
                {
                    Require(liveGame.DialogueVisible&&I(liveGame,"Line1")>liveBefore,"Dialogue paused global stage feedback");
                    liveChecks.Add("Actual dialogue Update continues stage presentation while actor gameplay is paused");
                    liveGame.ContinueDialogue();liveGame.Machine.Command(0x4C,new object[0]);Time.timeScale=0;
                    liveBefore=I(liveGame,"Line1");liveNext=now+.25;livePhase++;
                }
                else if(livePhase==2)
                {
                    Require(I(liveGame,"Line1")==liveBefore,"Zero game time advanced stage lines");
                    liveChecks.Add("Zero Time.timeScale freezes global stage presentation across real editor frames");
                    Time.timeScale=1;liveNext=now+.1;livePhase++;
                }
                else if(!Visible(liveGame))
                {
                    Require(liveGame.LastError==null,"Runtime error");
                    liveChecks.Add("The real background Update completes slide, rise and hold and hides the one-shot naturally");
                    FinishLive(null);
                }
            }
            catch(Exception e){FinishLive(e.ToString());}
        }
        private static void FinishLive(string error)
        {
            EditorApplication.update-=LiveUpdate;Time.timeScale=liveScale;
            Report("stage-info-live",liveChecks,error);liveGame=null;
        }
        public static void PrepareVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();Time.timeScale=0;Stage(g);Set(g,"toastUntil",0f);
        }
        public static void CaptureVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();var checks=new List<string>();string error=null;
            try
            {
                g.Machine.Command(0x4C,new object[0]);
                Save("stage-info-entry");Tick(g,9);Save("stage-info-slide");
                int guard=0;while(I(g,"OffsetY")<24&&Visible(g)){Tick(g,1);Require(++guard<100,"Visual rise timeout");}
                Save("stage-info-hold");Tick(g,25);Require(!Visible(g),"Visual title did not expire");Save("stage-info-expired");
                checks.Add("Captured source line entry, partial slide, full title rise/hold and expired presentation from the existing GameView");
            }
            catch(Exception e){error=e.ToString();}
            Report("stage-info-rendered",checks,error);
        }
        private static void Save(string name)
        {
            var image=(Texture2D)typeof(HudFeedbackValidation).GetMethod("Render",S).Invoke(null,null);
            try{File.WriteAllBytes("../Reference/Validation/"+name+".png",image.EncodeToPNG());}
            finally{UnityEngine.Object.DestroyImmediate(image);}
        }
    }
}
