using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public sealed class StoryDebugger : EditorWindow
    {
        [SerializeField] private int stageIndex, storyEvent=9999, roleLevel=1, spiritLevel;
        [SerializeField] private string variableName="event", script="";
        [SerializeField] private int variableValue;
        private Vector2 scroll;
        private GuJianGame previewGame;
        private string originalSave;
        [MenuItem("古剑/剧情调试器")]
        public static void Open()=>GetWindow<StoryDebugger>("古剑剧情调试");
        private void OnGUI()
        {
            var stages=OriginalAssets.Table<StageDefinition>("stages");
            stageIndex=Mathf.Clamp(stageIndex,0,stages.Length-1);
            stageIndex=EditorGUILayout.Popup("场景入口",stageIndex,stages.Select(s=>s.script+" "+s.name).ToArray());
            storyEvent=EditorGUILayout.IntField("初始 event",storyEvent);
            roleLevel=EditorGUILayout.IntSlider("主角等级",roleLevel,1,999);
            spiritLevel=EditorGUILayout.IntSlider("剑灵等级（0 不加入）",spiritLevel,0,40);
            EditorGUILayout.HelpBox("播放模式下加载原脚本；临时存档使用 gujian-story-preview.json。默认 event 9999 用于浏览场景，可改成剧情入口事件。",MessageType.Info);
            var stage=stages[stageIndex];
            if(GUILayout.Button("打开场景布局 Prefab"))AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Baked/Stages/"+stage.script+".prefab"));
            if(GUILayout.Button("编辑运行时剧情资产"))StoryAssetSetup.OpenStory(stage.script);
            var game=EditorApplication.isPlaying?UnityEngine.Object.FindObjectOfType<GuJianGame>():null;
            using(new EditorGUI.DisabledScope(!game))
            {
                if(GUILayout.Button("加载入口"))
                {
                    if(!previewGame){previewGame=game;originalSave=game.saveFileName;}
                    game.saveFileName="gujian-story-preview.json";
                    game.Begin();game.Machine.Clear();game.Machine.Values["event"]=storyEvent;
                    game.Machine.Command(0x4D,new object[]{roleLevel});
                    if(spiritLevel>0)game.Machine.Command(0x64,new object[]{spiritLevel});
                    game.Machine.Enqueue(stage.script);
                }
                if(previewGame && GUILayout.Button("结束调试，返回标题"))
                {
                    previewGame.Begin();previewGame.Machine.Clear();previewGame.Machine.Command(0x69,new object[0]);
                    previewGame.saveFileName=originalSave;previewGame=null;
                }
                if(!game)return;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("当前场景",game.StageName??"标题");
                EditorGUILayout.LabelField("VM",game.Machine.CurrentScript+":"+game.Machine.Instruction+" / "+(game.Machine.Running?"运行":"空闲"));
                EditorGUILayout.LabelField("等待",game.DialogueVisible?"对话":game.TutorialKey>=0?"教程键 "+game.TutorialKey:game.CriticalActive?"绝杀":game.WeaponPreviewVisible?"武器预览":"无上述等待");
                if(game.LastError!=null)EditorGUILayout.HelpBox(game.LastError,MessageType.Error);
                if(game.DialogueVisible && GUILayout.Button("推进对话"))game.ContinueDialogue();
                variableName=EditorGUILayout.TextField("变量",variableName);variableValue=EditorGUILayout.IntField("值",variableValue);
                if(GUILayout.Button("设置变量")&&!string.IsNullOrWhiteSpace(variableName))game.Machine.Values[variableName]=variableValue;
                script=EditorGUILayout.TextField("后续脚本",script);
                if(GUILayout.Button("执行脚本")&&!string.IsNullOrWhiteSpace(script))
                {
                    string name=Path.GetFileNameWithoutExtension(script);
                    if(OriginalAssets.HasScript(name))game.Machine.Enqueue(name);
                    else Debug.LogWarning("找不到剧情 "+name);
                }
                scroll=EditorGUILayout.BeginScrollView(scroll);
                foreach(var pair in game.Machine.Values.OrderBy(p=>p.Key))EditorGUILayout.LabelField(pair.Key,pair.Value.ToString());
                EditorGUILayout.EndScrollView();
            }
        }
        private void OnInspectorUpdate(){if(EditorApplication.isPlaying)Repaint();}
        private void OnDisable(){if(previewGame)previewGame.saveFileName=originalSave;previewGame=null;}
    }
}
