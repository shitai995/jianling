using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class SpiritMenuValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static GuJianGame game;
        private static object Get(string n) => typeof(GuJianGame).GetField(n, F).GetValue(game);
        private static void Set(string n, object v) => typeof(GuJianGame).GetField(n, F).SetValue(game, v);
        private static object Call(string n, params object[] args) => typeof(GuJianGame).GetMethod(n, F).Invoke(game, args);
        private static void Key(int code) => Call("HandleOriginalMenuKey", code);
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static List<EquipmentInstance> Bag => (List<EquipmentInstance>)Get("equipment");
        private static void Stage()
        {
            game.Begin(); game.Machine.Clear(); game.Machine.Values["event"] = -999;
            game.Machine.Enqueue("200"); game.Machine.Tick();
            foreach (var actor in game.Actors.Values) actor.locked = true;
            game.Machine.Command(0x64, new object[] { 1 });
            Set("menu", true); Require(game.SelectMenuPage(6), "Spirit page rejected"); Set("modalInputFrame", -1);
        }
        private static EquipmentInstance Food(int skill = -2, int level = 1)
        {
            var food = EquipmentInstance.Create(3);
            food.skillId = skill; food.skillLevel = level; Bag.Add(food); return food;
        }
        private static void OpenTip(int skill = 5, int level = 7)
        {
            Food(skill, level); Key(4);
            Require(game.SpiritFoodChoosing && !game.SpiritFeedConfirming, "First OK must only open candidate list");
            Key(4); Require(game.SpiritFeedConfirming && !game.SpiritFeedAnimating, "Second OK must open inheritance tip");
        }
        private static void Capture(string name)
        {
            var render = typeof(HudFeedbackValidation).GetMethod("Render", BindingFlags.NonPublic | BindingFlags.Static);
            var image = (Texture2D)render.Invoke(null, null);
            try { File.WriteAllBytes("../Reference/Validation/" + name + ".png", image.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(image); }
        }
        [MenuItem("古剑/验证剑灵菜单原版状态")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play first");
            game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            float scale = Time.timeScale; string save = game.saveFileName;
            var random = UnityEngine.Random.state;
            var checks = new List<string>(); string error = null;
            Time.timeScale = 0; game.saveFileName = "validation-spirit-menu.json";
            try
            {
                Stage(); Key(4);
                Require(!game.SpiritFoodChoosing && !game.SpiritFeedConfirming && !game.SpiritFeedAnimating && game.Equipment.Count == 1, "Empty candidate list state");
                Require(game.SpiritFeedFace == 2 && game.SpiritFeedAnimationMessage == "你身上根本没有多余的武器嘛！大骗子！", "Original empty-list face/text");
                Capture("spirit-menu-empty");
                checks.Add("No spare weapon retains status page, original angry face and original text; consumes nothing");

                Stage(); Food(); Capture("spirit-menu-status"); Key(4);
                Require(game.SpiritFoodChoosing && !game.SpiritFeedConfirming && game.Equipment.Count == 2 && game.SpiritExperience == 0, "First OK consumed weapon");
                Capture("spirit-menu-candidates");
                Key(4);
                Require(!game.SpiritFoodChoosing && !game.SpiritFeedConfirming && game.SpiritFeedAnimating && game.Equipment.Count == 1 && game.SpiritSkill == 13, "Skillless candidate did not feed directly");
                checks.Add("First OK opens candidates only; second OK directly feeds skillless weapon and preserves existing skill");

                Stage(); OpenTip(); string tip = (string)Get("spiritFeedTip");
                Require(tip.Contains("是否继承技能") && tip.Contains("7级?\n继承技能将替换灵剑现有的技能"), "Original inheritance question text");
                Key(4); Require(game.SpiritFeedConfirming && game.Equipment.Count == 2 && game.SpiritSkill == 13, "FIRE must not accept inheritance question");
                Capture("spirit-menu-tip");
                Key(9);
                Require(!game.SpiritFeedConfirming && !game.SpiritFoodChoosing && game.SpiritFeedAnimating && game.SpiritSkill == 13 && game.Equipment.Count == 1, "Soft9 must keep old skill and feed");
                checks.Add("Different skill opens original inheritance tip; FIRE is inert, soft9 retains skill and consumes candidate once");

                Stage(); OpenTip(); Key(10);
                Require(game.SpiritSkill == 5 && game.SpiritSkillLevel == 7 && game.SpiritFeedAnimating && game.Equipment.Count == 1, "Soft10 must inherit and feed");
                int remaining = game.SpiritFeedRemainingExperience;
                Key(4); Key(9); Key(0); Key(3);
                Require(game.SpiritFeedRemainingExperience == remaining && (int)Get("menuPage") == 6, "Absorption leaked unrelated input");
                Key(10);
                Require(game.SpiritFeedCompleted && !game.SpiritFeedAnimating && game.SpiritLevel > 1 && (bool)Get("menu"), "Soft10 must finish absorption without closing menu");
                Capture("spirit-menu-result");
                checks.Add("Soft10 inherits selected skill and feeds; absorption ignores FIRE/soft9/directions, soft10 fast-forwards while preserving menu");

                Stage(); Set("spiritSkill", -2); Food(8, 2); Key(9); Key(9);
                Require(!game.SpiritFeedConfirming && game.SpiritFeedAnimating && game.SpiritSkill == 8 && game.SpiritSkillLevel == 2, "Empty skill did not auto-inherit");
                checks.Add("Empty spirit skill auto-inherits without asking, including both steps via soft9");

                Stage(); Set("spiritSkill", 5); Set("spiritSkillLevel", 7); Food(5, 3); Key(4); Key(4);
                Require(!game.SpiritFeedConfirming && game.SpiritFeedAnimating && game.SpiritSkillLevel == 7, "Same-skill downgrade asked or replaced");
                checks.Add("Same-skill lower level feeds immediately without downgrade or inheritance prompt");

                Stage(); Set("spiritSkill", 5); Set("spiritSkillLevel", 7); Food(5, 8); Key(4); Key(4);
                Require(game.SpiritFeedConfirming && !game.SpiritFeedAnimating, "Same-skill upgrade must ask");
                Key(10); Require(game.SpiritSkill == 5 && game.SpiritSkillLevel == 8, "Upgrade answer ignored");
                checks.Add("Same-skill higher level asks first and upgrades only after soft10");

                Stage(); for (int i = 0; i < 7; i++) Food(5, i + 1);
                Key(4); Key(0); Require((int)Get("atlasSelection") == 6, "Candidate Up did not wrap to last");
                Capture("spirit-menu-candidates-last");
                Key(1); Require((int)Get("atlasSelection") == 0, "Candidate Down did not wrap to first");
                Key(10);
                Require(!game.SpiritFoodChoosing && (bool)Get("menu") && game.Equipment.Count == 8 && game.SpiritFeedFace == 1, "Back from candidates must return to status");
                checks.Add("Candidate list excludes equipped weapon, wraps over seven entries, draws five-row viewport; soft10 returns to status without consumption");

                Stage(); OpenTip(); Food(-2); string originalTip = (string)Get("spiritFeedTip");
                Key(1);
                Require((int)Get("atlasSelection") == 1 && (string)Get("spiritFeedTip") == originalTip, "Original tip candidate navigation changed");
                Key(10);
                Require(game.SpiritSkill == -2 && game.Equipment.Count == 2 && game.Equipment[1].skillId == 5, "Tip answer did not use current candidate");
                checks.Add("Original tip permits Up/Down while retaining question text; soft10 uses current candidate, including its absent skill");

                Stage(); OpenTip(); Key(3);
                Require((int)Get("menuPage") == 0 && !game.SpiritFeedConfirming && !game.SpiritFoodChoosing && game.Equipment.Count == 2, "Right from tip did not leave spirit page");
                Key(2);
                Require((int)Get("menuPage") == 6 && !game.SpiritFeedConfirming && !game.SpiritFoodChoosing, "Reentry retained stale candidate/tip");
                checks.Add("Left/Right can leave inheritance state; returning to spirit resets candidates and prompt without consuming equipment");

                Stage(); Key(10);
                Require((bool)Get("menu") && (int)Get("spiritMenuExitTicks") == 21 && game.SpiritFeedAnimationMessage == "这样就走了？再陪人家聊一下嘛~", "Farewell not started");
                Key(4); Key(3); Require((int)Get("menuPage") == 6 && !game.SpiritFoodChoosing, "Farewell accepts menu input");
                Capture("spirit-menu-farewell");
                for (int i = 0; i < 20; i++) Call("TickSpiritFeedAnimation", .080001f);
                Require((bool)Get("menu"), "Farewell closed before original twentieth decrement");
                Call("TickSpiritFeedAnimation", .080001f);
                Require(!(bool)Get("menu"), "Farewell did not close on twenty-first cycle");
                checks.Add("Status soft10 starts original farewell and blocks input; twenty decrement cycles retain menu, next 80ms cycle closes");

                Stage(); Food(5, 7); game.PressControl(4); game.PressControl(4); game.PressControl(5);
                Require(game.SpiritSkill == 5 && game.SpiritFeedAnimating, "Public Back5 did not map to original inheritance soft10");
                game.PressControl(5);
                Require(game.SpiritFeedCompleted && (bool)Get("menu"), "Public Back5 did not fast-forward absorption");
                checks.Add("Public control4 and Back5 use production two-stage inheritance and fast-forward rules with explicit native soft10 mapping");

                Stage(); OpenTip(); game.Begin(); game.Machine.Clear();
                Require(!game.SpiritFoodChoosing && !game.SpiritFeedConfirming && !game.SpiritFeedAnimating && (int)Get("spiritMenuExitTicks") == 0 && Get("spiritFeedTip") == null, "Begin retained spirit menu state");
                checks.Add("New game clears candidate, prompt, animation, farewell timer and text");

                Stage(); Food(); Key(4); Capture("spirit-menu-scroll-start");
                int lineHeight = (int)Get("spiritInfoLineHeight"), initialOffset = (int)Get("spiritInfoOffset");
                var lines = (OriginalDialogueText.Line[])Get("spiritInfoLines");
                Require(lines.Length * lineHeight > 38 && initialOffset == lineHeight, "Long StyledText must start one line below viewport top");
                Call("TickSpiritMenuInfo", .079f);
                Require((int)Get("spiritInfoOffset") == initialOffset, "Scroll moved before 80ms cycle");
                Call("TickSpiritMenuInfo", .001001f);
                Require((int)Get("spiritInfoOffset") == initialOffset - lineHeight / 10, "Scroll step must use integer font-height/10");
                for (int i = 0; i < 11; i++) Call("TickSpiritMenuInfo", .080001f);
                Capture("spirit-menu-scroll-visible");
                int cycles = 0;
                while ((int)Get("spiritInfoOffset") != 38 && cycles++ < 500) Call("TickSpiritMenuInfo", .080001f);
                Require((int)Get("spiritInfoOffset") == 38, "Long StyledText did not loop to viewport height after leaving top");
                checks.Add("Original StyledText long description: measured fixed140px wrapping, initial font-height offset, integer font-height/10 movement every80ms, loop to38px after full exit");
                Stage(); Food(); Key(4); Key(0); Capture("spirit-menu-single-highlight");
                checks.Add("Actual GameView captures preserve native five-row candidate focus and clip long description to141x39 including original one-pixel margin");

                var ui = AssetDatabase.LoadAssetAtPath<OriginalUiLayout>("Assets/Resources/OriginalUiLayout.asset");
                var sign = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Original/pic/u_27.png");
                Require(ui.menuScrollSign == sign && EditorUtility.IsPersistent(ui.menuScrollSign), "Candidate scroll sign is not saved original asset");
                checks.Add("Candidate scroll marker references saved original u_27 texture; UI renders original action6 and saved message-panel asset");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                game.Begin(); game.Machine.Clear(); typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                game.saveFileName = save; Time.timeScale = scale; UnityEngine.Random.state = random;
            }
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/spirit-menu.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_SPIRIT_MENU " + report.passed + " " + error);
            game = null;
        }
    }
}
