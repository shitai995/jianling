using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ChapterValidation
    {
        private sealed class Sample
        {
            public string stage; public int storyEvent, expectedEvent, sub1;
            public int[] deaths = new int[0]; public bool ending;
        }
        private static readonly Sample[] samples = {
            new Sample{stage="300",storyEvent=1050,expectedEvent=1051},
            new Sample{stage="400",storyEvent=400,expectedEvent=401},
            new Sample{stage="500",storyEvent=500,expectedEvent=501},
            new Sample{stage="600",storyEvent=600,expectedEvent=601},
            new Sample{stage="700",storyEvent=701,expectedEvent=800,deaths=new[]{79}},
            new Sample{stage="817",storyEvent=835,expectedEvent=836,deaths=new[]{86}},
            new Sample{stage="1000",storyEvent=1055,expectedEvent=1061,deaths=new[]{41,42,43,44,45,46,47,48,41,42,43,44,45,46,47,48}},
            new Sample{stage="1100",storyEvent=1100,expectedEvent=1101},
            new Sample{stage="1124",storyEvent=1105,sub1=4,expectedEvent=1180,deaths=new[]{100,101}},
            new Sample{stage="919",storyEvent=1184,expectedEvent=1282,deaths=new[]{69}},
            new Sample{stage="335",storyEvent=1289,expectedEvent=1300,deaths=new[]{91},ending=true},
            new Sample{stage="100",storyEvent=1300,expectedEvent=1300,ending=true},
        };
        private static readonly List<string> checks = new List<string>();
        private static readonly HashSet<string> scripts = new HashSet<string>();
        private static int index, deathIndex, pages;
        private static double started, idleSince;
        private static float previousScale;
        private static string previousSave;
        private static Action<string> previousTrace;
        private static readonly BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
        [MenuItem("古剑/验证章节剧情与结局")]
        public static void Run()
        {
            if(!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            previousScale=Time.timeScale; previousSave=g.saveFileName; previousTrace=g.Machine.Trace;
            g.saveFileName="validation-chapters.json";Time.timeScale=12;
            checks.Clear();index=0; Load(g);
            EditorApplication.update-=Advance; EditorApplication.update+=Advance;
        }
        private static void Load(GuJianGame g)
        {
            var sample=samples[index];
            g.Begin();g.Machine.Clear();g.Machine.Values["event"]=sample.storyEvent;g.Machine.Values["sub1"]=sample.sub1;
            g.Machine.Command(0x4D,new object[]{70});g.Machine.Command(0x64,new object[]{40});
            g.Machine.Command(0x7C,new object[]{true});g.Machine.Command(0x9C,new object[]{true});
            g.Machine.Trace=line=>{int separator=line.IndexOf(':');if(separator>0)scripts.Add(line.Substring(0,separator));};
            scripts.Clear();pages=deathIndex=0;idleSince=0;started=EditorApplication.timeSinceStartup;
            g.Machine.Enqueue(sample.stage);
        }
        private static void Advance()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            try
            {
                if(!EditorApplication.isPlaying || !g)throw new Exception("Play mode ended");
                if(g.LastError!=null)throw new Exception(g.LastError);
                if(EditorApplication.timeSinceStartup-started>90)throw new Exception("Timeout "+g.Machine.CurrentScript+":"+g.Machine.Instruction+" event="+g.Machine.Get("event")+" deathIndex="+deathIndex);
                var sample=samples[index];
                if(g.WeaponPreviewVisible){g.CloseWeaponPreview();return;}
                if(g.DialogueVisible){g.ContinueDialogue();pages++;return;}
                if(g.TutorialKey>=0){g.RespondTutorial(g.TutorialKey);return;}
                if(g.AtTitle && sample.ending){Complete(g);return;}
                if(g.Machine.Running || g.CriticalActive){idleSince=0;return;}
                if(idleSince==0){idleSince=EditorApplication.timeSinceStartup;return;}
                if(EditorApplication.timeSinceStartup-idleSince<.2)return;
                if(deathIndex<sample.deaths.Length)
                {
                    int id=sample.deaths[deathIndex];
                    OriginalActor actor;
                    if(!g.Actors.TryGetValue(id,out actor) || actor.hp<=0)return;
                    // Fixture damage skips balance, but executes the production death handling,
                    // kill counters and original callbacks. Do not enqueue follow-up scripts here.
                    var fighters=(IDictionary)typeof(GuJianGame).GetField("fighters",flags).GetValue(g);
                    typeof(GuJianGame).GetMethod("Damage",flags).Invoke(g,new object[]{fighters[id],actor.hp,false});
                    deathIndex++;idleSince=0;return;
                }
                if(g.Machine.Get("event")==sample.expectedEvent && !sample.ending){Complete(g);return;}
            }
            catch(Exception ex){Finish(false,"Sample "+samples[index].stage+": "+ex);}
        }
        private static void Complete(GuJianGame g)
        {
            var s=samples[index];
            if(g.Machine.Get("event")!=s.expectedEvent)throw new Exception("Wrong final event");
            if(s.ending && !scripts.Contains("endding"))throw new Exception("Original ending not executed");
            checks.Add(s.stage+" event "+s.storyEvent+" -> "+s.expectedEvent+"; "+pages+" dialogue pages; "+deathIndex+" fixture deaths; scripts: "+string.Join(",",scripts.OrderBy(x=>x))+"; "+(g.AtTitle?"ending returned to title":"real Update completed"));
            if(++index==samples.Length)Finish(true,null);else Load(g);
        }
        private static void Finish(bool passed,string error)
        {
            EditorApplication.update-=Advance;Time.timeScale=previousScale;
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();if(g){g.saveFileName=previousSave;g.Machine.Trace=previousTrace;}
            File.WriteAllText("../Reference/Validation/chapters.json",JsonUtility.ToJson(new ReconstructionValidation.Report{passed=passed,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error},true));
            Debug.Log("GUJIAN_CHAPTER_VALIDATION "+passed+" "+error);
        }
    }
}
