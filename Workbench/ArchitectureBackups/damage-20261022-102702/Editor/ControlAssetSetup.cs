using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ControlAssetSetup
    {
        [MenuItem("古剑/补充操作布局与音效组件（保留已有编辑）")]
        public static void Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play first");
            const string path = "Assets/Resources/OriginalTouchLayout.asset";
            var layout = AssetDatabase.LoadAssetAtPath<OriginalTouchLayout>(path);
            if (!layout)
            {
                layout = ScriptableObject.CreateInstance<OriginalTouchLayout>();
                layout.controls = DefaultControls();
                AssetDatabase.CreateAsset(layout, path);
            }
            PrepareLayout(layout);
            EditorUtility.SetDirty(layout);
            var miniMap=PrepareMiniMap();
            var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/GuJianGame.prefab");
            Bind(root.GetComponent<GuJianGame>(), layout, miniMap);
            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/GuJianGame.prefab"); PrefabUtility.UnloadPrefabContents(root);
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path == "Assets/Scenes/GuJian.unity")
            {
                Bind(UnityEngine.Object.FindObjectOfType<GuJianGame>(), layout, miniMap);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
        public static OriginalTouchLayout.Control[] DefaultControls() => new[] {
            Button("↑",0,26,199,70,40),Button("↓",1,26,279,70,40),Button("←",2,1,239,50,40),Button("→",3,71,239,50,40),
            Button("攻",4,462,254,70,60),Button("技",7,422,279,40,40),Button("崩",8,430,213,40,40),
            Button("合",9,140,280,120,40),Button("葬",10,492,212,40,40),Button("菜单",5,431,1,50,50),Button("地图",12,482,1,50,50)
        };
        public static void PrepareLayout(OriginalTouchLayout layout)
        {
            if (layout.controls == null || layout.controls.Length == 0) layout.controls=DefaultControls();
            if (layout.presentationVersion == 0)
            {
                var old=new[] {
                    Button("↑",0,52,202,34,32),Button("↓",1,52,274,34,32),Button("←",2,15,238,34,32),Button("→",3,89,238,34,32),
                    Button("攻",4,471,264,48,42),Button("技",7,418,235,42,36),Button("崩",8,471,211,42,36),
                    Button("合",9,365,266,38,36),Button("葬",10,418,280,38,30),Button("谈",11,129,273,38,32),Button("菜单",5,477,110,48,28)
                };
                var defaults=DefaultControls(); var controls=new List<OriginalTouchLayout.Control>();
                foreach(var control in layout.controls)
                {
                    var previous=old.FirstOrDefault(c=>c.code==control.code);
                    if(previous!=null && control.bounds==previous.bounds)
                    {
                        if(control.code==11)
                        {
                            var original=OriginalAssets.Texture("key/c_02");
                            if(control.label=="谈" && (!control.normal || control.normal==original) &&
                                (!control.pressed || control.pressed==OriginalAssets.Texture("key/c_12"))) continue;
                            controls.Add(control); continue;
                        }
                        var target=defaults.First(c=>c.code==control.code); control.bounds=target.bounds;
                    }
                    controls.Add(control);
                }
                if(!controls.Any(c=>c.code==12)) controls.Add(defaults.First(c=>c.code==12));
                layout.controls=controls.ToArray();
                if(layout.directionBounds==new Rect(10,200,114,114)) layout.directionBounds=new Rect(-1,197,124,124);
                if(Mathf.Approximately(layout.opacity,.65f)) layout.opacity=1;
                layout.presentationVersion=1;
            }
            if(layout.direction==null || layout.direction.frames==null || layout.direction.frames.Length==0)
                layout.direction=OriginalAssets.Json<OriginalAnimation>("Animations/key/c_00");
            layout.directionSheets=FillSheets(layout.directionSheets,new[]{"c_00","c_01"});
            if(layout.criticalReady==null || layout.criticalReady.frames==null || layout.criticalReady.frames.Length==0)
                layout.criticalReady=OriginalAssets.Json<OriginalAnimation>("Animations/key/c_15");
            layout.criticalReadySheets=FillSheets(layout.criticalReadySheets,new[]{"c_15"});
            if(!layout.back) layout.back=OriginalAssets.Texture("key/c_08");
            if(!layout.backPressed) layout.backPressed=OriginalAssets.Texture("key/c_18");
            foreach(var control in layout.controls)
            {
                int key=control.code==4 || control.code==11 ? 2 : control.code==7 ? 3 : control.code==8 ? 4 : control.code==10 ? 5 : control.code==5 ? 6 : control.code==12 ? 7 : -1;
                if(key<0) continue;
                if(!control.normal) control.normal=OriginalAssets.Texture("key/c_"+key.ToString("00"));
                if(!control.pressed) control.pressed=key==5 ? control.normal : OriginalAssets.Texture("key/c_"+(key+10).ToString("00"));
            }
        }
        private static Texture2D[] FillSheets(Texture2D[] sheets,string[] names)
        {
            if(sheets==null) sheets=new Texture2D[names.Length];
            if(sheets.Length<names.Length) Array.Resize(ref sheets,names.Length);
            for(int i=0;i<names.Length;i++) if(!sheets[i]) sheets[i]=OriginalAssets.Texture("key/"+names[i]);
            return sheets;
        }
        private static OriginalTouchLayout.Control Button(string label, int code, int x, int y, int w, int h) =>
            new OriginalTouchLayout.Control { label = label, code = code, bounds = new Rect(x,y,w,h) };
        private static OriginalMiniMapLayout PrepareMiniMap()
        {
            const string path="Assets/Resources/OriginalMiniMapLayout.asset";
            var asset=AssetDatabase.LoadAssetAtPath<OriginalMiniMapLayout>(path);
            if(!asset) { asset=ScriptableObject.CreateInstance<OriginalMiniMapLayout>(); AssetDatabase.CreateAsset(asset,path); }
            if(asset.areas==null || asset.areas.Length==0)
                asset.areas=JsonUtility.FromJson<MiniMapData>(File.ReadAllText("../Reference/minimap.json")).areas;
            if(!asset.current) asset.current=OriginalAssets.Texture("pic/mini_0");
            if(!asset.mission) asset.mission=OriginalAssets.Texture("pic/mini_1");
            if(!asset.save) asset.save=OriginalAssets.Texture("pic/mini_2");
            EditorUtility.SetDirty(asset); return asset;
        }
        private static void Bind(GuJianGame game, OriginalTouchLayout layout, OriginalMiniMapLayout miniMap)
        {
            var serialized = new SerializedObject(game);
            var touch=serialized.FindProperty("touchLayout"); if(!touch.objectReferenceValue) touch.objectReferenceValue=layout;
            var map=serialized.FindProperty("miniMapLayout"); if(!map.objectReferenceValue) map.objectReferenceValue=miniMap;
            string[] fields = { "swingAudio", "criticalAudio", "hitAudio" }, files = { "miss", "critical", "hit" };
            for (int i = 0; i < fields.Length; i++)
            {
                var property = serialized.FindProperty(fields[i]); if (property.objectReferenceValue) continue;
                string name = "Sound_" + files[i]; var child = game.transform.Find(name);
                if (!child) { var go = new GameObject(name); go.transform.SetParent(game.transform, false); child = go.transform; }
                var audio = child.GetComponent<AudioSource>(); if (!audio) audio = child.gameObject.AddComponent<AudioSource>();
                audio.clip = Resources.Load<AudioClip>("Original/Audio/" + files[i]); audio.playOnAwake = false; audio.loop = false; audio.volume = .65f; audio.spatialBlend = 0;
                property.objectReferenceValue = audio;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
