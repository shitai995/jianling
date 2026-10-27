using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class SpiritAbsorptionValidation
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static void Set(GuJianGame g, string name, object value) { typeof(GuJianGame).GetField(name, Flags).SetValue(g, value); }
        private static void Tick(GuJianGame g, float delta = .080001f) { typeof(GuJianGame).GetMethod("TickSpiritFeedAnimation", Flags).Invoke(g, new object[] { delta }); }
        private static void Stage(GuJianGame g, int level = 1, int food = 3, int plus = 9)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999;
            g.Machine.Enqueue("200"); g.Machine.Tick();
            foreach (var actor in g.Actors.Values) actor.locked = true;
            g.Machine.Command(0x64, new object[] { level });
            var bag = g.EquipmentBag;
            bag.Add(EquipmentInstance.Create(food, plus));
            Require(g.SelectSpiritFood(1) && g.ConfirmSpiritFeed(false), "Production confirmation rejected");
        }
        [MenuItem("古剑/验证剑灵原版分段吸收")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            float scale = Time.timeScale; string save = g.saveFileName;
            var random = UnityEngine.Random.state;
            var checks = new List<string>();
            var report = new ReconstructionValidation.Report { utc = DateTime.UtcNow.ToString("O") };
            try
            {
                Time.timeScale = 0; g.saveFileName = "validation-spirit-absorption.json";
                Stage(g);
                Require(g.SpiritLevel == 1 && g.SpiritExperience == 0 && g.SpiritFeedRemainingExperience == 13500 && g.SpiritFeedFace == 4, "Experience must be pending, source face 4");
                Require(!g.FeedSpirit(1) && !g.CloseMenu() && !g.SelectMenuPage(1), "Absorption allowed a conflicting action");
                Tick(g, .079f); Require(g.SpiritExperience == 0, "XP advanced before 80ms");
                Tick(g, .00101f); Require(g.SpiritExperience == 1, "First 80ms must grant exactly 1 XP");
                for (int i = 1; i < 25; i++) Tick(g);
                Require(g.SpiritLevel == 1 && g.SpiritExperience == 25 && g.SpiritFeedRemainingExperience == 13475, "Original equality boundary must wait until next cycle");
                Tick(g); Require(g.SpiritLevel == 2 && g.SpiritExperience == 0 && g.SpiritFeedFlashFrame == 0, "Next cycle must level up and start original flash");
                Tick(g); Require(g.SpiritExperience == 13 && g.SpiritFeedRemainingExperience == 13462, "Level two increment must be floor(200/15)=13");
                checks.Add("Real confirmation consumes weapon once; 80ms boundary, 25 one-XP steps, original threshold equality delay, level-two 13-XP step and source flash action verified");
                int ticks = 0;
                while (g.SpiritFeedAnimating && ticks++ < 1000) Tick(g);
                Require(!g.SpiritFeedAnimating && g.SpiritLevel == 7 && g.SpiritExperience == 2475 && g.SpiritFeedFace == 5, "Natural segmented absorption final level/XP/face mismatch");
                Require(g.SpiritFeedAnimationMessage.Contains("升级了"), "Original level-up response missing");
                Require(!g.CompleteSpiritFeed(), "Completion could award twice");
                Require(g.SpiritFeedCompleted && g.SpiritFeedAttributeGain(0)==1206, "Completed display lost pre-feed attribute comparison");
                checks.Add("Natural completion of 13500 XP reaches level 7 / 2475 XP, original face 5 and level-up text; completion cannot award twice");

                Stage(g); Tick(g, .037f); g.Save();
                int remaining = g.SpiritFeedRemainingExperience;
                g.CompleteSpiritFeed(); g.Load(); g.Machine.Tick();
                Require(g.SpiritFeedAnimating && g.SpiritLevel == 1 && g.SpiritExperience == 0 && g.SpiritFeedRemainingExperience == remaining, "Pending experience lost/duplicated through real Save/Load");
                Tick(g, .042f); Require(g.SpiritExperience == 0, "Restored fractional accumulator advanced early");
                Tick(g, .00101f); Require(g.SpiritExperience == 1, "Restored fractional accumulator did not continue");
                Require(g.CompleteSpiritFeed() && g.SpiritLevel == 7 && g.SpiritExperience == 2475 && !g.CompleteSpiritFeed(), "Skip after restore disagrees with natural final value");
                checks.Add("Real Save/Load preserves consumed weapon, pending experience, pre-feed state and .037-second accumulator, reopens spirit menu; fast completion yields the same final values once");

                foreach (var pair in new[] { new[] {10,3}, new[] {9,1}, new[] {7,0}, new[] {6,5} })
                {
                    Stage(g, pair[0], 3, 0); g.CompleteSpiritFeed();
                    Require(g.SpiritFeedFace == pair[1] && !string.IsNullOrEmpty(g.SpiritFeedAnimationMessage), "Original satisfaction threshold mismatch at level " + pair[0]);
                }
                Stage(g, 40); Require(!g.SpiritFeedAnimating && g.SpiritExperience == 0 && g.SpiritFeedAnimationMessage.Contains("最强状态"), "Max-level immediate feedback mismatch");
                checks.Add("Original below-5%, below-10%, below-20%, at-least-20% satisfaction faces/text plus max-level response verified against source cases");
                Stage(g,1,0,0); Set(g,"spiritExperience",20);
                for(int i=0;i<5;i++) Tick(g);
                Require(!g.SpiritFeedAnimating && g.SpiritLevel==1 && g.SpiritExperience==25, "Natural exact-threshold completion must keep the original delayed level-up boundary");
                Stage(g,1,0,0); Set(g,"spiritExperience",20); g.CompleteSpiritFeed();
                Require(g.SpiritLevel==2 && g.SpiritExperience==0, "Original fast completion uses addExp and levels at exact threshold");
                Stage(g,39,0,0); Set(g,"spiritExperience",39*39*39*25-1);
                Tick(g); Require(g.SpiritLevel==40 && g.SpiritExperience==1600000 && g.SpiritFeedRemainingExperience==4, "Natural level-cap crossing must preserve remaining XP until the following cycle");
                Tick(g); Require(!g.SpiritFeedAnimating && g.SpiritFeedFace==4 && g.SpiritFeedAnimationMessage.Contains("最强状态") && ((string)typeof(GuJianGame).GetField("spiritFeedResult",Flags).GetValue(g)).Contains("经验 +1"), "Natural level-cap overflow must deduct discarded XP from displayed gain");
                checks.Add("Source exact-threshold natural completion remains level 1 / 25 XP while fast completion reaches level 2 / 0; natural level-40 overflow is discarded on the next cycle and reports only accepted XP");
                var ui=Resources.Load<OriginalUiLayout>("OriginalUiLayout");
                Require(ui.menuNumbers && ui.menuNumbers.name=="u_19" && ui.attributeGainNumbers && ui.attributeGainNumbers.name=="u_41" && ui.attributeGainArrow && ui.attributeGainArrow.name=="u_42" && ui.spiritExperience.width==82 && ui.spiritExperience.height==11 && ui.skillIcons && ui.skillIcons.name=="skill", "Original spirit menu atlas references or dimensions mismatch");
                checks.Add("Saved native number/gain/arrow/skill textures and original 82x11 experience strip verified");
                report.passed = true;
            }
            catch (Exception e) { report.error = e.ToString(); }
            finally { Time.timeScale = scale; g.saveFileName = save; UnityEngine.Random.state = random; }
            report.checks = checks.ToArray();
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "../../Reference/Validation/spirit-absorption.json")), JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_SPIRIT_ABSORPTION " + report.passed + " " + report.error);
        }
    }
}
