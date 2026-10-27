using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ReconstructionValidation
    {
        [Serializable] public sealed class Report { public bool passed; public string utc; public string[] checks; public string error; }
        [MenuItem("古剑/验证资源与脚本")]
        public static void ValidateData()
        {
            var checks = new List<string>();
            var report = new Report { utc = DateTime.UtcNow.ToString("O") };
            try
            {
                var definitions = OriginalAssets.Table<SpriteDefinition>("sprite");
                foreach (var d in definitions)
                {
                    var a = OriginalAssets.Json<OriginalAnimation>("Animations/action/" + d.animation);
                    if (a.sheets.Length > d.images.Length) throw new Exception("Sheet count " + d.id);
                    for (int i = 0; i < a.sheets.Length; i++)
                    {
                        var image = OriginalAssets.Texture(d.images[i]);
                        foreach (var r in a.sheets[i].rects)
                            if (r.x < 0 || r.y < 0 || r.w < 0 || r.h < 0 || r.x + r.w > image.width || r.y + r.h > image.height) throw new Exception("Sprite rectangle " + d.id);
                    }
                    foreach (var action in a.actions)
                        foreach (var step in action.steps)
                            if (step.frame < 0 || step.frame >= a.frames.Length) throw new Exception("Frame reference " + d.id);
                }
                checks.Add("137 sprite definitions: images, atlas rectangles, animation frame references passed");
                foreach (var stage in OriginalAssets.Table<StageDefinition>("stages"))
                {
                    var map = OriginalAssets.Json<OriginalMap>("Maps/" + stage.map);
                    if (map.cells.Length != map.columns * map.rows) throw new Exception("Cell count " + stage.map);
                    for (int i = 0; i < 3; i++) OriginalAssets.Texture("Maps/" + stage.map + "_" + i);
                }
                checks.Add("All stage maps, layer textures and cell counts passed");
                var vm = new OriginalScriptMachine();
                vm.LoadScript = OriginalAssets.Script;
                vm.Command = (id, args) => { if (id == 32) vm.Enqueue((string)args[0]); return 0; };
                vm.Enqueue("start");
                for (int i = 0; i < 100 && vm.Running; i++) vm.Tick();
                if (vm.Running || vm.Error != null || vm.Get("event") != 200 || vm.Get("stageID") != 200) throw new Exception("Opening script language flow");
                checks.Add("Original opening scripts: queued calls / arrays / conditions -> event 200, stage 200 (host command stubs)");
                vm = new OriginalScriptMachine();
                vm.LoadScript = _ => new[] { "a[2][2]:", "n=0", "while(n<4)", "if(n<2)", "a[0][n]=n+1", "else", "a[1][n-2]=n+1", "endif", "n=n+1", "loop", "x09(\"sum\",a[0][0]+a[0][1]+a[1][0]+a[1][1])" };
                vm.Enqueue("language-check"); vm.Tick();
                if (vm.Get("sum") != 10 || vm.Running) throw new Exception("Nested VM language checks");
                checks.Add("VM nested conditions, while loops, multidimensional arrays: sum = 10");
                var skill = OriginalAssets.Table<SkillDefinition>("skill").First(s => s.id == 0);
                if (OriginalCombatMath.RoleDamage(230, 100, 1, 1, 120, skill, 1) != 73) throw new Exception("Damage integer arithmetic");
                if (OriginalCombatMath.SkillValue("50,5,9,5", 9, true) != 100 || OriginalCombatMath.SkillValue("0,500,9,500", 9, false) != 5000) throw new Exception("Skill formula");
                checks.Add("Original integer damage and level-9 skill formula checks passed");
                report.passed = true;
            }
            catch (Exception e) { report.error = e.ToString(); Debug.LogException(e); }
            report.checks = checks.ToArray();
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/data-checks.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_DATA_VALIDATION " + report.passed);
            if (!report.passed) throw new Exception(report.error);
        }

        [MenuItem("古剑/验证首战与存档")]
        public static void ValidateBattleAndSave()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (!g || g.Machine.Get("event") != 200 || g.Machine.Running) throw new Exception("Run opening playback validation first");
            originalTimeScale = Time.timeScale; Time.timeScale = 8;
            started = EditorApplication.timeSinceStartup; saveTestPhase = 0;
            originalSaveFileName = g.saveFileName; g.saveFileName = "validation-battle-save.json";
            g.Machine.Enqueue("200n0");
            EditorApplication.update -= AdvanceBattle;
            EditorApplication.update += AdvanceBattle;
        }

        [MenuItem("古剑/验证原版 x78 指令")]
        public static void ValidateOpcode78()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var isolatedRoot = new GameObject("x78 validation (temporary)");
            isolatedRoot.SetActive(false);
            var g = isolatedRoot.AddComponent<GuJianGame>();
            var blocks = typeof(GuJianGame).GetField("originalMessageBlocksScript", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var checks = new List<string>();
            try
            {
                var vm = new OriginalScriptMachine
                {
                    Command = (id, args) => typeof(GuJianGame).GetMethod("HandleCommand", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(g, new object[] { id, args }),
                    IsWaiting = () => g.DialogueVisible && (bool)blocks.GetValue(g)
                };
                vm.LoadScript = name => name == "x78-experience" ? new[] { "x78(1)", "x09(\"x78Done\",1)" }
                    : name == "x78-locked" ? new[] { "x78(3)", "x09(\"x78Done\",1)" }
                    : new[] { "x78(3)", "x09(\"x78Done\",1)" };

                g.allStagesUnlocked = false;
                vm.Enqueue("x78-experience"); vm.Tick();
                if (!g.DialogueVisible || !vm.Running || vm.Get("x78Done") != 0 || !vm.IsWaiting())
                    throw new Exception("x78(1) failed to show the original offline hint and block the isolated VM");
                if (!CurrentOriginalMessageIs(g, "升级过慢，可到商城购买多倍经验。"))
                    throw new Exception("x78(1) message text mismatch");
                checks.Add("x78(1): original offline experience hint and VM wait");
                g.ContinueDialogue(); vm.Tick();
                if (vm.Running || vm.Get("x78Done") != 1) throw new Exception("x78(1) VM did not resume after confirmation");

                vm.Clear(); vm.Values.Clear(); vm.Enqueue("x78-locked"); vm.Tick();
                if (!g.DialogueVisible || !vm.Running || vm.Get("x78Done") != 0 || !vm.IsWaiting())
                    throw new Exception("Locked x78(3) failed to show and block on the original message");
                if (!CurrentOriginalMessageIs(g, "该章节尚未开放!")) throw new Exception("Locked x78(3) message text mismatch");
                checks.Add("locked x78(3): original message and VM wait");
                g.ContinueDialogue(); vm.Tick();
                if (vm.Running || vm.Get("x78Done") != 1) throw new Exception("Locked x78(3) VM did not resume after confirmation");

                g.allStagesUnlocked = true;
                vm.Clear(); vm.Values.Clear(); vm.Enqueue("x78-unlocked"); vm.Tick();
                if (g.DialogueVisible || vm.Running || vm.Get("x78Done") != 1)
                    throw new Exception("Unlocked x78(3) unexpectedly displayed a message or blocked the VM");
                checks.Add("unlocked x78(3): no message and no VM wait");
                Debug.Log("GUJIAN_OPCODE78_VALIDATION passed: " + string.Join("; ", checks));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(isolatedRoot);
            }
        }

        [MenuItem("古剑/验证原版 x7C 耐久切换")]
        public static void ValidateOpcode7C()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var command = typeof(GuJianGame).GetMethod("HandleCommand", flags);
            var integrated = typeof(GuJianGame).GetField("integrated", flags);
            var noLoss = typeof(GuJianGame).GetField("infiniteDurability", flags);
            var spiritDurability = typeof(GuJianGame).GetField("spiritDurability", flags);
            var bonuses = typeof(GuJianGame).GetField("spiritBonuses", flags);
            var report = new Report { utc = DateTime.UtcNow.ToString("O") };
            var checks = new List<string>();
            var root = new GameObject("x7C validation (temporary)");
            root.SetActive(false);
            try
            {
                // Inactive objects avoid Awake, scene setup, Update and production save writes.
                var game = root.AddComponent<GuJianGame>();
                var spiritRoot = new GameObject("isolated spirit");
                spiritRoot.transform.SetParent(root.transform);
                var spirit = spiritRoot.AddComponent<OriginalActor>();
                spirit.level = 12;
                ((IDictionary<int, OriginalActor>)game.Actors).Add(-2, spirit);
                ((int[])bonuses.GetValue(game))[1] = 37;
                var bag = (IList<EquipmentInstance>)game.Equipment;
                bag.Add(EquipmentInstance.Create(0, 5));
                bag.Add(EquipmentInstance.Create(1));
                // Original iron sword: 50 + 50*5*10/100 = 75.
                // Original level-12 spirit: 700 + 35*(12-10) + 37 = 807.
                string[] commands = { "x7C(true)", "x7C(false)", "x7C()" };
                foreach (bool isIntegrated in new[] { false, true })
                {
                    integrated.SetValue(game, isIntegrated);
                    foreach (string instruction in commands)
                    {
                        bool enabled = instruction == "x7C(true)";
                        game.durability = 9;
                        bag[0].durability = 9;
                        bag[1].durability = 7;
                        spiritDurability.SetValue(game, 11);
                        noLoss.SetValue(game, !enabled);
                        var vm = new OriginalScriptMachine
                        {
                            LoadScript = _ => new[] { instruction, "x09(\"afterRepair\",1)" },
                            Command = (id, args) => command.Invoke(game, new object[] { id, args })
                        };
                        vm.Enqueue("isolated-x7C");
                        vm.Tick();
                        int expected = isIntegrated ? 807 : 75;
                        if (vm.Error != null || vm.Running || vm.Get("afterRepair") != 1 ||
                            game.durability != expected || (bool)noLoss.GetValue(game) != enabled)
                            throw new Exception(instruction + " flag, immediate repair or VM continuation mismatch (integrated=" + isIntegrated + ")");
                        if (bag[0].durability != (isIntegrated ? 9 : 75) ||
                            (int)spiritDurability.GetValue(game) != (isIntegrated ? 807 : 11) ||
                            bag[1].durability != 7)
                            throw new Exception(instruction + " repaired an inactive weapon or failed to synchronize the active durability");
                        vm.Enqueue("isolated-x7C");
                        vm.Tick();
                        if (game.durability != expected || vm.Error != null || vm.Running)
                            throw new Exception(instruction + " repeated call mismatch");
                        checks.Add((isIntegrated ? "integrated spirit" : "ordinary +5 iron sword") + ": " +
                            instruction + " repairs immediately to " + expected + ", sets flag, leaves inactive weapons unchanged and continues VM; repeat stable");
                    }
                }
                report.passed = true;
            }
            catch (Exception error) { report.error = error.ToString(); }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            report.checks = checks.ToArray();
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/opcode-7c.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_OPCODE7C_VALIDATION " + report.passed + " " + report.error);
            if (!report.passed) throw new Exception(report.error);
        }

        private static bool CurrentOriginalMessageIs(GuJianGame game, string expected)
        {
            var dialogue = typeof(GuJianGame).GetField("dialogue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var active = typeof(GuJianGame).GetField("originalMessage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return (bool)active.GetValue(game) && (string)dialogue.GetValue(game) == expected;
        }

        [MenuItem("古剑/验证原版 x57 无可强化武器")]
        public static void ValidateOpcode57()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var root = new GameObject("x57 validation (temporary)");
            root.SetActive(false);
            var checks = new List<string>();
            var report = new Report { utc = DateTime.UtcNow.ToString("O") };
            try
            {
                var game = root.AddComponent<GuJianGame>();
                var command = typeof(GuJianGame).GetMethod("HandleCommand", Flags);
                var blocks = typeof(GuJianGame).GetField("originalMessageBlocksScript", Flags);
                if (command == null || blocks == null) throw new MissingMemberException("x57 validation hooks");
                var vm = new OriginalScriptMachine
                {
                    Command = (id, args) => command.Invoke(game, new object[] { id, args }),
                    IsWaiting = () => game.DialogueVisible && (bool)blocks.GetValue(game)
                };
                vm.LoadScript = _ => new[] { "x57()", "x09(\"x57Done\",1)" };
                vm.Enqueue("x57-no-candidate"); vm.Tick();
                if (!game.DialogueVisible || !vm.Running || vm.Get("x57Done") != 0 || !vm.IsWaiting())
                    throw new Exception("x57 did not block the isolated VM while showing its message");
                if (!CurrentOriginalMessageIs(game, "无可强化的武器!"))
                    throw new Exception("x57 original message text mismatch");
                checks.Add("x57 no-candidate message and blocking wait passed");
                game.ContinueDialogue(); vm.Tick();
                if (game.DialogueVisible || vm.Running || vm.Get("x57Done") != 1)
                    throw new Exception("x57 VM did not resume after confirmation");
                checks.Add("x57 confirmation resumes the following script instruction");
                report.passed = true;
            }
            catch (Exception error) { report.error = error.ToString(); Debug.LogException(error); }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            report.checks = checks.ToArray();
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/opcode-57.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_OPCODE57_VALIDATION " + report.passed + " " + report.error);
            if (!report.passed) throw new Exception(report.error);
        }

        [MenuItem("古剑/验证原版 x20 x22 加载状态")]
        public static void ValidateLoadingState()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var root = new GameObject("x20 x22 validation (temporary)");
            root.SetActive(false);
            var checks = new List<string>();
            var report = new Report { utc = DateTime.UtcNow.ToString("O") };
            try
            {
                var game = root.AddComponent<GuJianGame>();
                Func<string, FieldInfo> field = name => typeof(GuJianGame).GetField(name, Flags);
                Func<string, MethodInfo> method = name => typeof(GuJianGame).GetMethod(name, Flags);
                var command = method("HandleCommand");
                var menu = field("menu");
                var worldMap = field("worldMapOpen");
                var menuPage = field("menuPage");
                var atlasPage = field("atlasPage");
                var selection = field("atlasSelection");
                var systemPage = field("systemMenuPage");
                var loadingVisible = field("loadingVisible");
                if (command == null || menu == null || worldMap == null || menuPage == null || atlasPage == null ||
                    selection == null || systemPage == null || loadingVisible == null)
                    throw new MissingMemberException("Loading state validation hooks");

                foreach (bool keepOtherState in new[] { false, true })
                {
                    menu.SetValue(game, true);
                    worldMap.SetValue(game, true);
                    menuPage.SetValue(game, 4);
                    atlasPage.SetValue(game, 4);
                    selection.SetValue(game, 2);
                    var enumType = systemPage.FieldType;
                    systemPage.SetValue(game, Enum.Parse(enumType, "Settings"));
                    method("StartLoadingOverlay").Invoke(game, null);
                    if (!(bool)loadingVisible.GetValue(game)) throw new Exception("x20 did not display the loading overlay");

                    if (keepOtherState)
                    {
                        menu.SetValue(game, false);
                        worldMap.SetValue(game, false);
                        menuPage.SetValue(game, 2);
                        atlasPage.SetValue(game, 1);
                        selection.SetValue(game, 3);
                        systemPage.SetValue(game, Enum.Parse(enumType, "None"));
                    }

                    var vm = new OriginalScriptMachine
                    {
                        LoadScript = _ => new[] { "x22(" + (keepOtherState ? "true" : "false") + ")", "x09(\"afterLoad\",1)" },
                        Command = (id, args) => command.Invoke(game, new object[] { id, args }),
                        IsWaiting = () => (bool)field("loadingVisible").GetValue(game) && (bool)field("loadingFinished").GetValue(game)
                    };
                    vm.Enqueue("x20-x22-state");
                    vm.Tick();
                    if (!vm.Running || !vm.IsWaiting()) throw new Exception("x22 did not wait for loading cycles (other=" + keepOtherState + ")");
                    method("TickLoading").Invoke(game, new object[] { .16f });
                    vm.Tick();
                    if (vm.Running || vm.Get("afterLoad") != 1 || (bool)loadingVisible.GetValue(game))
                        throw new Exception("Loading overlay did not close and resume the VM (other=" + keepOtherState + ")");

                    bool expectedMenu = false;
                    bool expectedWorldMap = false;
                    int expectedPage = keepOtherState ? 2 : 4;
                    int expectedAtlasPage = keepOtherState ? 1 : 4;
                    int expectedSelection = keepOtherState ? 3 : 2;
                    string expectedSystemPage = "None";
                    if ((bool)menu.GetValue(game) != expectedMenu || (bool)worldMap.GetValue(game) != expectedWorldMap ||
                        (int)menuPage.GetValue(game) != expectedPage || (int)atlasPage.GetValue(game) != expectedAtlasPage ||
                        (int)selection.GetValue(game) != expectedSelection || systemPage.GetValue(game).ToString() != expectedSystemPage)
                        throw new Exception("x22 state restoration mismatch (other=" + keepOtherState + ") actual=" +
                            menu.GetValue(game) + "," + worldMap.GetValue(game) + "," + menuPage.GetValue(game) + "," +
                            atlasPage.GetValue(game) + "," + selection.GetValue(game) + "," + systemPage.GetValue(game));
                    checks.Add("x20 → x22(" + keepOtherState.ToString().ToLowerInvariant() + "): loading overlay, two 80ms cycles, scene-state handling and VM continuation passed");
                }
                report.passed = true;
            }
            catch (Exception error) { report.error = error.ToString(); Debug.LogException(error); }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            report.checks = checks.ToArray();
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/loading-state.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_LOADING_STATE_VALIDATION " + report.passed + " " + report.error);
            if (!report.passed) throw new Exception(report.error);
        }

        private static int saveTestPhase;
        private static void AdvanceBattle()
        {
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            try
            {
                if (!g || !EditorApplication.isPlaying) throw new Exception("Play mode ended");
                if (g.LastError != null) throw new Exception(g.LastError);
                if (EditorApplication.timeSinceStartup - started > 90) throw new Exception("Battle/save validation timed out");
                if (g.WeaponPreviewVisible) { g.CloseWeaponPreview(); return; }
                if (g.DialogueVisible) { g.ContinueDialogue(); return; }
                if (g.Machine.Running) return;
                if (saveTestPhase == 0 && g.Machine.Get("event") == 200)
                {
                    var living = g.Actors.Values.Where(a => a.IsEnemy && !a.hidden && a.hp > 0).ToArray();
                    var role = g.Actors[-1];
                    foreach (var a in living) a.frozen = false;
                    if (living.Length > 0 && role.actionIndex <= 1)
                    {
                        var a = living[0]; a.hp = 1;
                        role.mapPosition = a.mapPosition + new Vector2(-40, a.Animation.footHeight - role.Animation.footHeight);
                        role.Face("r"); g.UseSkill(0, 1, true);
                    }
                    return;
                }
                if (saveTestPhase == 0 && g.Machine.Get("event") == 201)
                {
                    foreach (var a in g.Actors.Values.Where(a => a.IsEnemy)) a.frozen = false;
                    g.money = 12345; g.durability = 17;
                    g.Machine.Command(0x51, new object[] { 0, 3 });
                    g.Actors[-1].hp = 1234;
                    g.Save();
                    g.money = 1; g.durability = 1; g.Actors[-1].hp = 1;
                    saveTestPhase = 1; g.Load(); return;
                }
                if (saveTestPhase == 1)
                {
                    if (g.money != 12345 || g.durability != 17 || g.CurrentHp != 1234 || g.Machine.Get("event") != 201) throw new Exception("Save/load state mismatch");
                    if ((int)g.Machine.Command(0x4A, new object[] { 0 }) != 3) throw new Exception("Inventory restore mismatch");
                    CompleteBattle(true, null);
                }
            }
            catch (Exception error) { CompleteBattle(false, error.ToString()); }
        }
        private static void CompleteBattle(bool passed, string error)
        {
            EditorApplication.update -= AdvanceBattle;
            Time.timeScale = originalTimeScale;
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (g) g.saveFileName = originalSaveFileName;
            var report = new Report { passed = passed, utc = DateTime.UtcNow.ToString("O"), error = error,
                checks = new[] { "Real 200n0 script: acquire/equip weapon, create four-kill task", "Actual original attack/body rectangles and animation steps, two enemies respawn once, task callback -> event 201", "For deterministic integration testing enemy HP is set to 1; combat balance is not verified", "JSON save/load: event, money 12345, HP 1234, durability 17, three items" } };
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/battle-save.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_BATTLE_SAVE_VALIDATION " + passed + " " + error);
        }

        private static double started;
        private static int dialogueCount;
        private static float originalTimeScale;
        private static string originalSaveFileName;
        [MenuItem("古剑/验证真实开场播放")]
        public static void ValidateOpening()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (!g) throw new Exception("Open Assets/Scenes/GuJian.unity first");
            originalSaveFileName = g.saveFileName; g.saveFileName = "validation-gujian-save.json";
            g.Begin();
            started = EditorApplication.timeSinceStartup; dialogueCount = 0;
            originalTimeScale = Time.timeScale; Time.timeScale = 30;
            EditorApplication.update -= AdvanceOpening;
            EditorApplication.update += AdvanceOpening;
        }
        private static void AdvanceOpening()
        {
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (!EditorApplication.isPlaying || !g) { Finish(false, "Play mode ended"); return; }
            if (g.LastError != null) { Finish(false, g.LastError); return; }
            if (EditorApplication.timeSinceStartup - started > 90) { Finish(false, "Opening playback timed out"); return; }
            if (g.DialogueVisible) { g.ContinueDialogue(); dialogueCount++; }
            if (g.Machine.Get("event") == 200 && g.Machine.Get("stageID") == 200 && !g.Machine.Running)
                Finish(true, null);
        }
        private static void Finish(bool passed, string error)
        {
            EditorApplication.update -= AdvanceOpening; Time.timeScale = originalTimeScale;
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (g) g.saveFileName = originalSaveFileName;
            var report = new Report { passed = passed, utc = DateTime.UtcNow.ToString("O"), error = error,
                checks = new[] { "Real Unity Update loop, original start script, actual scene and command host", "Dialogue pages advanced: " + dialogueCount, "Stage: " + (g ? g.StageName : "unavailable") } };
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/opening-playback.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_OPENING_VALIDATION " + passed + " " + error);
        }
    }
}
