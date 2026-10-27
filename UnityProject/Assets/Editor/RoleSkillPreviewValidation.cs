using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class RoleSkillPreviewValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static GuJianGame game;
        private static object Get(string n) => ValidationState.Get(game, n);
        private static void Set(string n, object v) => ValidationState.Set(game, n, v);
        private static object Call(string n, params object[] a) => typeof(GuJianGame).GetMethod(n, F).Invoke(game, a);
        private static object Prop(string n) => typeof(GuJianGame).GetProperty(n, F).GetValue(game);
        private static int Index => (int)Get("roleSkillPreviewIndex");
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        private static string Hash(string p)
        {
            if (!File.Exists(p)) return null;
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(p)));
        }
        private static void Stage(bool integrated = false)
        {
            game.Begin(); game.Machine.Clear(); game.Machine.Values["event"] = -999;
            game.Machine.Enqueue("200"); game.Machine.Tick();
            var equipment = game.EquipmentBag;
            equipment.Add(EquipmentInstance.Create(25, 9)); Call("SelectEquipment", 1);
            if (integrated)
            {
                game.Machine.Command(0x64, new object[] { 21 });
                Call("SetSpiritDurability", Call("GetSpiritAttribute", 1));
                game.Machine.Command(0x7F, new object[0]);
                Require((bool)Get("integrated") && game.Actors[-1].actionBank == 27, "Integrated fixture did not enter integrated state");
            }
            foreach (var actor in game.Actors.Values) actor.locked = true;
            Set("menu", true); game.SelectMenuPage(7); Set("modalInputFrame", -1);
            Set("originalUiTicks", 0); Set("originalUiCycle", 0d);
        }
        private static void Capture(string name)
        {
            var render = typeof(HudFeedbackValidation).GetMethod("Render", BindingFlags.Static | BindingFlags.NonPublic);
            var image = (Texture2D)render.Invoke(null, null);
            try { File.WriteAllBytes("../Reference/Validation/role-preview-" + name + ".png", image.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(image); }
        }
        [MenuItem("古剑/验证角色技能预览")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Play first");
            game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var checks = new List<string>(); string error = null;
            float scale = Time.timeScale; var random = UnityEngine.Random.state;
            string formal0 = (string)Call("RecordPath", 0), formal1 = (string)Call("RecordPath", 1);
            string hash0 = Hash(formal0), hash1 = Hash(formal1);
            Time.timeScale = 0;
            try
            {
                var ui = AssetDatabase.LoadAssetAtPath<OriginalUiLayout>("Assets/Resources/OriginalUiLayout.asset");
                Require(ui.skillPreviewEffects.Length == 9 && ui.skillPreviewEffects.Select((e, i) => e != null && e.animation.actions.Length == new[] { 2, 1, 2, 1, 2, 1, 1, 1, 2 }[i] && e.sheets.All(t => t && EditorUtility.IsPersistent(t))).All(ok => ok) &&
                    ui.skillPreviewSupport.animation.actions.Length == 4 && ui.skillPreviewSupport.sheets.All(t => t && EditorUtility.IsPersistent(t)), "Saved effect assets");
                string path = "Assets/Resources/OriginalUiLayout.asset", before = Hash(path);
                UiAssetSetup.PrepareSystemMenus(); Require(Hash(path) == before, "Repeated asset preparation changed saved data");
                checks.Add("Nine original skill effect sets and four-action support set reference saved textures; repeated preparation is byte-stable");

                for (int mode = 0; mode < 2; mode++)
                {
                    Stage(mode == 1); var role = game.Actors[-1];
                    for (int skill = 5; skill <= 21; skill++)
                    {
                        Set("equippedSkill", skill); Call("ResetRoleSkillPreview");
                        int hp = role.hp, money = game.money, energy = game.Energy, durability = (int)Get("durability");
                        int cooldown = game.SkillCooldownTicks, action = role.actionIndex, version = role.ActionVersion;
                        var position = role.mapPosition; int objects = UnityEngine.Object.FindObjectsOfType<GameObject>().Length;
                        game.PressControl(18);
                        Require(game.RoleSkillPreviewVisible && Index == 0, "Public18 did not start preview " + skill);
                        int expectedAction = (skill < 14 ? skill + 7 : 21) + mode * 27;
                        int sourceAction = skill < 14 ? skill + 7 : 21;
                        int expectedLength = role.Animation.actions[sourceAction].steps.Length;
                        int support = skill < 14 ? -1 : skill < 17 ? 1 : skill < 20 ? 2 : 0;
                        if (support >= 0) expectedLength += ui.skillPreviewSupport.animation.actions[support].steps.Length;
                        Require((int)Prop("RoleSkillPreviewAction") == expectedAction && (int)Prop("RoleSkillPreviewLength") == expectedLength, "Original action/length mapping " + skill);
                        Call("CycleRoleSkillPreview"); int current = Index; game.PressControl(18);
                        Require(Index == current, "Repeated18 restarted active preview");
                        if (skill == 5 || skill == 13 || skill == 14 || skill == 17 || skill == 20)
                        {
                            Set("roleSkillPreviewIndex", skill < 14 ? 2 : role.Animation.actions[21].steps.Length + 2);
                            Capture((mode == 0 ? "normal-" : "integrated-") + skill);
                            Set("roleSkillPreviewIndex", current);
                        }
                        for (int i = current; i < expectedLength - 1; i++) Call("CycleRoleSkillPreview");
                        Require(game.RoleSkillPreviewVisible && Index == expectedLength - 1, "Last frame missing " + skill);
                        Call("CycleRoleSkillPreview");
                        Require(!game.RoleSkillPreviewVisible && Index == 0, "One-shot did not finish " + skill);
                        Require(role.hp == hp && game.money == money && game.Energy == energy && (int)Get("durability") == durability && game.SkillCooldownTicks == cooldown &&
                            role.actionIndex == action && role.ActionVersion == version && role.mapPosition == position &&
                            UnityEngine.Object.FindObjectsOfType<GameObject>().Length == objects, "Preview mutated combat or created actors " + skill);
                    }
                    checks.Add((mode == 0 ? "Ordinary" : "Integrated") + "17 skills use source actions/lengths, retain last frame, finish once, ignore repeated18 and change no HP/money/EP/durability/cooldown/action/position or GameObjects");
                }

                Stage(); Set("equippedSkill", -2); game.PressControl(18);
                Require(game.RoleSkillPreviewVisible && (int)Prop("RoleSkillPreviewAction") == 0 &&
                    (int)Prop("RoleSkillPreviewLength") == game.Actors[-1].Animation.actions[11].steps.Length, "No-skill source idle/length rule");
                Call("ResetRoleSkillPreview"); Set("equippedSkill", 5); game.PressControl(18);
                Call("TickOriginalUiClock", .079f); Require(Index == 0, "Advanced before80ms");
                Call("TickOriginalUiClock", .00101f); Require(Index == 1, "Did not advance on80ms");
                checks.Add("No-skill displays idle for source action11 duration; shared production UI clock advances at80ms boundary");

                Require(game.SelectMenuPage(2), "Original page switch blocked during preview");
                Require(!game.RoleSkillPreviewVisible, "Preview covered equipment page");
                Call("CycleRoleSkillPreview"); Require(Index == 2, "Menu page change stopped original preview counter");
                game.SelectMenuPage(3); Set("atlasSelection", 3); Call("HandleOriginalMenuKey", 4);
                Call("CycleRoleSkillPreview"); Require(Index == 2, "Independent settings advanced preview");
                Call("HandleOriginalMenuKey", 10); game.SelectMenuPage(7);
                Require(game.RoleSkillPreviewVisible && Index == 2, "Role reentry lost preview position");
                Call("HandleOriginalMenuKey", 10); Call("CycleRoleSkillPreview");
                Require(!(bool)Get("menu") && Index == 2, "Closed menu advanced preview");
                game.Begin(); game.Machine.Clear(); Require(!(bool)Get("roleSkillPreview") && Index == 0, "New game retained preview");
                checks.Add("Original menu switching retains running preview, independent settings and closed menu pause it, and Begin clears transient state");

                Stage(); game.SelectMenuPage(2); game.PressControl(18); Require(!(bool)Get("roleSkillPreview"), "18 started on wrong page");
                game.SelectMenuPage(7); Set("equippedSkill", 5); game.PressControl(18);
                Set("roleSkillPreviewIndex", 2); Capture("attack");
                Call("ResetRoleSkillPreview"); Capture("restored");
                checks.Add("18 is limited to role page; actual GameView captures cover attack, projectile and all three support types in ordinary/integrated modes");
                Require(Hash(formal0) == hash0 && Hash(formal1) == hash1, "Formal save changed");
                checks.Add("Both formal save hashes remain unchanged");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                game.Begin(); game.Machine.Clear(); typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                Time.timeScale = scale; UnityEngine.Random.state = random;
            }
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/role-skill-preview.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_ROLE_SKILL_PREVIEW " + report.passed + " " + error); game = null;
        }
    }
}
