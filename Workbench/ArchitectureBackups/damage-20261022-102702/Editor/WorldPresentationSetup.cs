using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GuJian.Editor
{
    public static class WorldPresentationSetup
    {
        [MenuItem("古剑/补充角色整体排序与战斗数字节点（保留已有编辑）")]
        public static void Run()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Exit Play first");
            PrepareEnemyBackEffects();
            var ui=AssetDatabase.LoadAssetAtPath<OriginalUiLayout>("Assets/Resources/OriginalUiLayout.asset");
            if(!ui||ui.combatNumbers==null||ui.combatNumbers.Length!=4||!ui.combatMiss)throw new InvalidOperationException("Saved combat atlases required");
            if(ui.combatDigits==null||ui.combatDigits.Length!=40)ui.combatDigits=new Sprite[40];
            for(int type=0;type<4;type++)for(int digit=0;digit<10;digit++)
                if(!ui.combatDigits[type*10+digit])
                {
                    var texture=ui.combatNumbers[type];int width=texture.width/10;
                    ui.combatDigits[type*10+digit]=Slice(ui,texture,new Rect(digit*width,0,width,texture.height),"Combat "+type+" digit "+digit);
                }
            if(!ui.combatMissSprite)ui.combatMissSprite=Slice(ui,ui.combatMiss,new Rect(0,0,ui.combatMiss.width,ui.combatMiss.height),"Combat miss");
            PrepareEnemyHealth(ui);
            EditorUtility.SetDirty(ui);AssetDatabase.SaveAssets();
            int actors=0,feedback=0,drops=0;
            var definitions=OriginalAssets.Table<SpriteDefinition>("sprite");
            foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Resources/Baked/Actors","Assets/Resources/Baked/Effects","Assets/Resources/Baked/Drops"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var actor=root.GetComponent<OriginalActor>();
                    if(actor)
                    {
                        OriginalWorldFeedback component=null;
                        int id;var definition=path.Contains("/Actors/")&&int.TryParse(System.IO.Path.GetFileNameWithoutExtension(path),out id)?definitions.First(d=>d.id==id):null;
                        if(definition!=null&&(definition.type==0||definition.type==9||definition.type==10||definition.type==11))
                        {
                            component=root.GetComponent<OriginalWorldFeedback>();
                            if(!component)component=root.AddComponent<OriginalWorldFeedback>();
                            component.PrepareAsset();feedback++;
                        }
                        actor.PreparePresentation(component);actors++;
                    }
                    else if(root.GetComponent<OriginalGroundDrop>())
                    {
                        if(!root.GetComponent<SortingGroup>())root.AddComponent<SortingGroup>();drops++;
                    }
                    else continue;
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            AssetDatabase.SaveAssets();Debug.Log("GUJIAN_WORLD_PRESENTATION_SETUP actors="+actors+" feedback="+feedback+" iconDrops="+drops);
        }
        [MenuItem("古剑/补充特殊敌人背后特效（保留已有编辑）")]
        public static void PrepareEnemyBackEffects()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Exit Play first");
            OriginalAssetBaker.BakeEnemyBackEffect();
            var effect = AssetDatabase.LoadAssetAtPath<OriginalActor>("Assets/Resources/Baked/Effects/s_36.prefab");
            foreach (var definition in OriginalAssets.Table<SpriteDefinition>("sprite").Where(d => d.type == 11))
            {
                string path = "Assets/Resources/Baked/Actors/" + definition.id + ".prefab";
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var actor = root.GetComponent<OriginalActor>();
                    if (actor.EnemyBackEffect) continue;
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(effect.gameObject, root.transform);
                    instance.name = "Enemy back effect";
                    var child = instance.GetComponent<OriginalActor>(); child.hidden = true;
                    actor.PrepareEnemyBackEffect(child);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
        }
        private static Sprite Slice(OriginalUiLayout ui,Texture2D texture,Rect rect,string name)
        {
            var sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f),1,0,SpriteMeshType.FullRect);
            sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,ui);return sprite;
        }
        private static Color32 Rgb(int rgb)=>new Color32((byte)(rgb>>16),(byte)(rgb>>8),(byte)rgb,255);
        private static void PrepareEnemyHealth(OriginalUiLayout ui)
        {
            if(ui.enemyHealthBars==null||ui.enemyHealthBars.Length!=20)ui.enemyHealthBars=new Sprite[20];
            if(ui.enemyHealthBars.All(s=>s))return;
            var texture=AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(ui)).OfType<Texture2D>().FirstOrDefault(t=>t.name=="Enemy health bar raster");
            if(!texture)
            {
                texture=new Texture2D(420,5,TextureFormat.RGBA32,false){name="Enemy health bar raster",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
                var pixels=new Color32[420*5];
                for(int width=0;width<20;width++)
                {
                    int offset=width*21;
                    for(int y=1;y<=3;y++)for(int x=1;x<=19;x++)pixels[(4-y)*420+offset+x]=Rgb(x<=width?16776960:4013368);
                    for(int x=Mathf.Min(1,width);x<=Mathf.Max(1,width);x++)
                    {pixels[3*420+offset+x]=Rgb(15976556);pixels[420+offset+x]=Rgb(15976556);}
                    for(int x=1;x<=19;x++){pixels[offset+x]=Rgb(15960172);pixels[4*420+offset+x]=Rgb(15960172);}
                    for(int y=1;y<=3;y++){pixels[y*420+offset]=Rgb(15960172);pixels[y*420+offset+20]=Rgb(15960172);}
                }
                texture.SetPixels32(pixels);texture.Apply();AssetDatabase.AddObjectToAsset(texture,ui);
            }
            for(int width=0;width<20;width++)if(!ui.enemyHealthBars[width])
                ui.enemyHealthBars[width]=Slice(ui,texture,new Rect(width*21,0,21,5),"Enemy health bar "+width);
        }
    }
}
