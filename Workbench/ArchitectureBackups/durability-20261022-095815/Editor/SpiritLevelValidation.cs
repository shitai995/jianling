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
    public static class SpiritLevelValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static readonly List<string> checks = new List<string>();
        private static readonly int[] bonuses = { 113, 37, 59, 23, 3, 2, 1 };
        private static GuJianGame game;
        private static string previousSave, formalHash;
        private static float previousScale;
        private static UnityEngine.Random.State previousRandom;
        private static double started;
        private static int phase, updates, confirms, lastFrame, resetCalls;
        private static Action<string> previousTrace;
        private static object Get(object obj, string name) => obj.GetType().GetField(name, F).GetValue(obj);
        private static void Set(object obj, string name, object value) => obj.GetType().GetField(name, F).SetValue(obj, value);
        private static object Call(string name, params object[] args) => typeof(GuJianGame).GetMethod(name, F).Invoke(game, args);
        private static void Require(bool yes, string message) { if (!yes) throw new Exception(message); }
        private static string Hash(string name)
        {
            string path = Path.Combine(Application.persistentDataPath, name);
            if (!File.Exists(path)) return "absent";
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }
        private static void Stage(int level)
        {
            game.Begin(); game.Machine.Clear(); game.Machine.Values["event"] = -999;
            game.Machine.Enqueue("200"); game.Machine.Tick();
            Require(game.Map != null && !game.Machine.Running && !game.DialogueVisible, "Stage setup");
            foreach (var actor in game.Actors.Values) actor.locked = true;
            if (level > 0) game.Machine.Command(0x64, new object[] { level });
        }
        private static string Sword(int level) => level < 11 ? "w_97" : level < 21 ? "w_98" : level < 31 ? "w_99" : "w_00";
        private static int VerifyRenderers(string expectedSword)
        {
            var role = game.Actors[-1];
            var images = (string[])Get(role, "images");
            string ordinary = "action/" + game.Equipment[game.EquippedIndex].Data.image;
            Require(images[1] == "action/" + expectedSword && images[4] == ordinary, "Sword sheets not independent");
            var renderers = (List<SpriteRenderer>)Get(role, "renderers");
            int capacity = renderers.Count, children = role.transform.childCount, piecesChecked = 0;
            int action = role.actionIndex, frame = role.FrameIndex;
            var frameField = typeof(OriginalActor).GetField("<FrameIndex>k__BackingField", F);
            try
            {
                for (int a = 0; a < role.Animation.actions.Length; a++)
                for (int f = 0; f < role.Animation.actions[a].steps.Length; f++)
                {
                    role.actionIndex = a; frameField.SetValue(role, f); role.RefreshFrame();
                    var pieces = role.CurrentFrame.pieces;
                    for (int i = 0; i < pieces.Length; i++)
                    {
                        var piece = pieces[i];
                        if (piece.image != 1 && piece.image != 4) continue;
                        var rect = role.Animation.sheets[piece.image].rects[piece.rect];
                        if (rect.w <= 0 || rect.h <= 0) continue;
                        var expected = OriginalAssets.Catalog.FindSprite(piece.image == 1 ? "action/" + expectedSword : ordinary, rect);
                        Require(expected && AssetDatabase.Contains(expected), "Expected sprite was not baked");
                        bool visible = !role.hidden && !role.presentationHidden && (piece.image != 4 || role.WeaponVisible);
                        Require(renderers[i].enabled == visible, "Sword piece visibility differs from role/stowed state");
                        if (visible)
                        {
                            Require(renderers[i].sprite == expected, "Actual renderer does not use the saved sword sprite: sheet " + piece.image + " action " + a + " frame " + f);
                            piecesChecked++;
                        }
                    }
                }
                Require(capacity == renderers.Count && children == role.transform.childCount && piecesChecked > 0, "Sword refresh changed saved piece capacity");
            }
            finally { role.actionIndex = action; frameField.SetValue(role, frame); role.RefreshFrame(); }
            return piecesChecked;
        }
        private static void Capture(string name)
        {
            var render = typeof(HudFeedbackValidation).GetMethod("Render", BindingFlags.Static | BindingFlags.NonPublic);
            var image = (Texture2D)render.Invoke(null, null);
            try { File.WriteAllBytes("../Reference/Validation/" + name + ".png", image.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(image); }
        }
        [MenuItem("古剑/验证原版剑灵重置与图层")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying && !game, "Enter Play and finish previous run first");
            game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            previousSave = game.saveFileName; formalHash = Hash(previousSave);
            previousScale = Time.timeScale; previousRandom = UnityEngine.Random.state;
            previousTrace = game.Machine.Trace;
            checks.Clear(); updates = confirms = resetCalls = 0;
            Directory.CreateDirectory("../Reference/Validation");
            Time.timeScale = 0; game.saveFileName = "validation-spirit-level.json";
            try
            {
                Stage(12);
                Set(game, "spiritBonuses", (int[])bonuses.Clone());
                Set(game, "spiritExperience", 1234567);
                Set(game, "spiritSkill", 5); Set(game, "spiritSkillLevel", 7);
                Set(game, "spiritDurability", 600);
                game.Machine.Command(0x64, new object[] { 31 });
                Require(game.SpiritLevel == 31 && game.SpiritExperience == 0 &&
                    ((int[])Get(game, "spiritBonuses")).All(value => value == 0) &&
                    game.SpiritSkill == 13 && game.SpiritSkillLevel == 1 && game.SpiritDurability == 0,
                    "x64 reused existing spirit experience, bonuses, skill or durability");
                Require(game.SpiritDurability == 0, "New spirit must start empty");
                int rendered = VerifyRenderers("w_00");
                checks.Add("x64 replaces an existing level-12 spirit after seeding XP1234567, seven bonuses, skill5/7 and durability600; new level31 spirit resets XP/bonuses/skill/durability and uses saved w_00 sheet1 while ordinary weapon sheet4 stays independent; actual renderers checked");

                foreach (bool integrated in new[] { false, true })
                {
                    Stage(12);
                    Set(game, "spiritBonuses", (int[])bonuses.Clone());
                    Set(game, "spiritExperience", 1234567);
                    Set(game, "spiritSkill", 5); Set(game, "spiritSkillLevel", 7);
                    Set(game, "spiritDurability", 600);
                    int ordinaryDurability = game.durability;
                    if (integrated) { game.Machine.Command(0x7F, new object[0]); game.durability = 600; }
                    Require(game.GetSpiritAttribute(1) == 807, "Original level12+37 maximum");
                    int ownerMaximum = game.Actors[-1].maxHp, ownerHp = game.Actors[-1].hp;
                    game.Machine.Command(0x93, new object[] { 1 });
                    Require(game.SpiritLevel == 1 && game.GetSpiritAttribute(1) == 537 && game.SpiritDurability == 399, "x93 integer durability ratio 537*600/807");
                    Require(game.SpiritExperience == 1234567 && game.SpiritExperienceRequired == 25 &&
                        game.SpiritSkill == 5 && game.SpiritSkillLevel == 7 && ((int[])Get(game, "spiritBonuses")).SequenceEqual(bonuses), "x93 erased XP, skill or permanent bonuses");
                    Require(game.Actors[-1].maxHp == ownerMaximum && game.Actors[-1].hp == ownerHp, "x93 unexpectedly re-integrated owner attributes");
                    Require(integrated ? game.durability == 399 && game.Equipment[game.EquippedIndex].durability == ordinaryDurability : game.durability == ordinaryDurability,
                        "x93 changed ordinary weapon durability");
                    rendered += VerifyRenderers("w_97");
                    checks.Add("x93 level12->1 " + (integrated ? "integrated" : "separate") + ": 807->537 maximum, 600->399 current; XP1234567, seven bonuses, skill5/7 and ordinary equipment retained; owner attributes not recomputed");
                }

                Stage(1);
                int previousMax = game.GetSpiritAttribute(1);
                foreach (int target in new[] { 10, 11, 20, 21, 30, 31, 40, 1 })
                {
                    Set(game, "spiritDurability", previousMax);
                    game.Machine.Command(0x93, new object[] { target });
                    Require(game.SpiritDurability == game.GetSpiritAttribute(1), "Full durability ratio at " + target);
                    previousMax = game.GetSpiritAttribute(1);
                    rendered += VerifyRenderers(Sword(target));
                    Set(game, "spiritDurability", 0);
                    game.Machine.Command(0x93, new object[] { target });
                    Require(game.SpiritDurability == 0, "Empty durability ratio at " + target);
                }
                checks.Add("Upgrade/downgrade targets10/11/20/21/30/31/40/1 preserve full/empty durability and select all four original spirit swords; total actual saved renderer pieces checked: " + rendered);

                foreach (int lv in new[] { 1, 11, 21, 31 })
                {
                    game.Machine.Command(0x93, new object[] { lv });
                    Set(game, "spiritDurability", game.GetSpiritAttribute(1));
                    game.Machine.Command(0x7F, new object[0]);
                    game.Actors[-1].Act(27, true); Capture("spirit-sword-" + lv);
                    game.Machine.Command(0x7F, new object[0]);
                }
                Stage(10); Set(game, "spiritExperience", game.SpiritExperienceRequired - 1);
                Call("AddSpiritExperience", 2);
                Require(game.SpiritLevel == 11 && game.SpiritExperience == 1, "Feed level crossing");
                rendered = VerifyRenderers("w_98");
                game.Save(); game.Machine.Command(0x93, new object[] { 31 });
                game.Load(); game.Machine.Tick();
                Require(game.SpiritLevel == 11 && game.SpiritExperience == 1, "Real save/load level and XP");
                rendered += VerifyRenderers("w_98");
                checks.Add("Production AddSpiritExperience crosses10->11 and real Save/Load restores w_98 on sheet1; actual saved renderer pieces checked: " + rendered);

                Stage(31);
                var bag = game.EquipmentBag;
                bag.Add(EquipmentInstance.Create(17)); Call("SelectEquipment", 1);
                Set(game, "spiritBonuses", (int[])bonuses.Clone());
                Set(game, "spiritExperience", 1234567); Set(game, "spiritSkill", 5); Set(game, "spiritSkillLevel", 7);
                Set(game, "spiritDurability", game.GetSpiritAttribute(1));
                game.Machine.Command(0x7F, new object[0]);
                game.Machine.Values["event"] = 404; game.Machine.Values["stageID"] = 334;
                game.Machine.Trace = line => { if (line.Contains("x93(")) resetCalls++; if (previousTrace != null) previousTrace(line); };
                game.Machine.Enqueue("334");
                phase = 0; lastFrame = -1; started = EditorApplication.timeSinceStartup;
                Time.timeScale = 8;
                EditorApplication.update -= Advance;
                EditorApplication.update += Advance;
            }
            catch (Exception error) { Finish(error.ToString()); }
        }
        private static void Advance()
        {
            try
            {
                Require(EditorApplication.isPlaying && game && game.LastError == null && game.Machine.Error == null, "Play or production VM failed");
                if (EditorApplication.timeSinceStartup - started > 180) throw new TimeoutException("Original334e1 timed out at " + game.Machine.CurrentScript + ":" + game.Machine.Instruction);
                updates++;
                if (game.DialogueVisible && lastFrame != Time.frameCount)
                {
                    if (confirms == 0) Capture("spirit-reset-story");
                    lastFrame = Time.frameCount; confirms++; game.PressControl(4);
                }
                if (game.Machine.Running || game.DialogueVisible || game.Machine.Get("event") != 405) return;
                Require(game.SpiritLevel == 1 && game.SpiritExperience == 1234567 && game.SpiritSkill == 5 && game.SpiritSkillLevel == 7 &&
                    ((int[])Get(game, "spiritBonuses")).SequenceEqual(bonuses), "Original story reset lost persistent spirit values");
                Require(!(bool)game.Machine.Command(0x91, new object[0]) && game.Equipment[game.EquippedIndex].id == 0 &&
                    !game.Equipment.Any(e => e.id == 17), "Original story did not split and replace 七星 with 铁剑");
                VerifyRenderers("w_97");
                if (phase == 0)
                {
                    Require(resetCalls == 1 && updates > 1 && confirms > 10, "Original x93 was not reached exactly once through real Update");
                    Require(!(bool)Get(game, "infiniteDurability") && game.SpiritDurability == 537, "Story x7C completion did not refill level1 spirit");
                    Capture("spirit-reset-result");
                    checks.Add("Unmodified334->334e1 runs through real Unity Update with production waits and confirmation; event404->405, one x93, split, 七星 removal/铁剑 equip, XP/bonuses/skill retained; " + updates + " callbacks, " + confirms + " confirmations");
                    Require(File.Exists(Path.Combine(Application.persistentDataPath, game.saveFileName)), "Original x68 did not save");
                    phase = 1; game.Load(); return;
                }
                Require(resetCalls == 1, "Reading event405 replayed reset story");
                checks.Add("Original x68 checkpoint loads back into334 at event405 without replaying404 reset; level1, XP, seven bonuses, skill and sheet1 sprite restored");
                Finish(null);
            }
            catch (Exception error) { Finish(error.ToString()); }
        }
        private static void Finish(string error)
        {
            EditorApplication.update -= Advance;
            if (game)
            {
                if (Hash(previousSave) != formalHash) error = (error ?? "") + " Formal save hash changed";
                else checks.Add("Formal player save SHA256 unchanged (" + formalHash + "); only independent validation save used");
                game.Machine.Trace = previousTrace;
                game.Begin(); game.Machine.Clear(); typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                game.saveFileName = previousSave;
            }
            Time.timeScale = previousScale; UnityEngine.Random.state = previousRandom;
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/spirit-level.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_SPIRIT_LEVEL " + report.passed + " " + error);
            game = null;
        }
    }
}
