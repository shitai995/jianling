using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class WeaponPreviewValidation
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly List<string> checks = new List<string>();
        private static object Get(object o, string n) => ValidationState.Get(o, n);
        private static void Set(object o, string n, object v) => ValidationState.Set(o, n, v);
        private static object Call(GuJianGame g, string n, params object[] args) => typeof(GuJianGame).GetMethod(n, Private).Invoke(g, args);
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        private static void Stage(GuJianGame g)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999;
            g.Machine.Enqueue("200"); g.Machine.Tick();
            Require(g.Map != null && !g.Machine.Running && !g.DialogueVisible, "Stage fixture");
            foreach (var a in g.Actors.Values) a.locked = true;
        }
        private static void Shop(GuJianGame g) { g.Machine.Command(0x63, new object[] { 2 }); }
        private static string Appearance(GuJianGame g) => string.Join("|", (string[])Get(g.Actors[-1], "images"));
        [MenuItem("古剑/验证武器试穿与剧情预览")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            string save = g.saveFileName, error = null; var random = UnityEngine.Random.state; float scale = Time.timeScale;
            var loader = g.Machine.LoadScript;
            Time.timeScale = 0; checks.Clear();
            try
            {
                Stage(g); Require(!g.PreviewShopWeapon(0), "Preview outside shop accepted"); Shop(g);
                Require(!g.PreviewShopWeapon(-1) && !g.PreviewShopWeapon(25) && !g.PreviewShopWeapon(3), "Invalid/high-level/high-grade shop preview");
                Require(g.SelectWeaponTrade(0, false) && !g.PreviewShopWeapon(0), "Preview over trade confirmation"); g.CancelTrade();
                g.level = 40; int money = g.money, hp = g.CurrentHp, durability = g.durability;
                string look = Appearance(g); var equipped = g.Equipment[g.EquippedIndex]; int count = g.Equipment.Count;
                var state = UnityEngine.Random.state; int expected = UnityEngine.Random.Range(0, int.MaxValue); UnityEngine.Random.state = state;
                int sprites = Resources.FindObjectsOfTypeAll<Sprite>().Length, objects = UnityEngine.Object.FindObjectsOfType<Transform>().Length;
                int candidate = OriginalAssets.Table<EquipmentDefinition>("equip").First(e => e.skill == -1 && e.grade < 3 && e.level <= g.level).id;
                Require(g.PreviewShopWeapon(candidate) && g.WeaponPreviewVisible && g.WeaponPreviewSkillLabel == "随机技能", "Random skill preview");
                Require(!g.SelectWeaponTrade(0, false) && !g.SelectItemTrade(0, false) && !g.PreviewShopWeapon(0), "Underlying controls active");
                Call(g, "TickWeaponPreview", .080001f); Require(g.WeaponPreviewFrame == 1 % g.Actors[-1].Animation.actions[0].steps.Length, "Preview cadence");
                Require(g.CurrentHp == hp && g.money == money && g.durability == durability && ReferenceEquals(g.Equipment[g.EquippedIndex], equipped) && g.Equipment.Count == count && Appearance(g) == look,
                    "Preview changed actual equipment/HP/appearance");
                g.CloseWeaponPreview();
                Require((bool)Get(g, "menu") && (int)Get(g, "menuPage") == 5 && UnityEngine.Random.Range(0, int.MaxValue) == expected, "Shop return/random stream");
                Require(Resources.FindObjectsOfTypeAll<Sprite>().Length == sprites && UnityEngine.Object.FindObjectsOfType<Transform>().Length == objects, "Preview generated sprites/nodes");
                checks.Add("Shop-only eligibility, trade-modal exclusion, 80-ms idle preview, random skill label, unchanged actual equipment/HP/durability/money/appearance and random stream, return to shop, and no new Sprite or Transform");

                Stage(g);
                foreach (var data in OriginalAssets.Table<EquipmentDefinition>("equip"))
                foreach (int plus in new[] { 0, 9 })
                {
                    g.Machine.Command(0x94, new object[] { data.id, plus, true });
                    int multiplier = plus == 9 ? 2 : 1;
                    Require(g.WeaponPreviewId == data.id && g.WeaponPreviewReinforcement == plus && g.GetWeaponPreviewAttribute(0) == data.hp * multiplier &&
                        g.GetWeaponPreviewAttribute(1) == data.durability * multiplier && g.GetWeaponPreviewAttribute(2) == data.attack * multiplier &&
                        g.GetWeaponPreviewAttribute(3) == data.defense * multiplier && g.GetWeaponPreviewAttribute(4) == data.hit &&
                        g.GetWeaponPreviewAttribute(5) == data.dodge && g.GetWeaponPreviewAttribute(6) == data.critical, "Preview attributes " + data.id);
                    Require(data.skill == -2 ? g.WeaponPreviewSkillLabel == "无技能" : data.skill == -1 ? g.WeaponPreviewSkillLabel == "随机技能" : g.WeaponPreviewSkillLabel.Contains("级"), "Skill type label");
                    g.CloseWeaponPreview(); Require(!(bool)Get(g, "menu"), "Script preview left menu open");
                }
                checks.Add("All 30 weapon definitions at +0 and +9 display exact seven equipment attributes and fixed/random/absent skill types; script previews close back to the original world state");

                Stage(g); g.Machine.LoadScript = name => name == "preview-wait-check" ? new[] { "x94(3,9,true)", "x09(\"previewPassed\",1)" } : loader(name);
                g.Machine.Enqueue("preview-wait-check"); g.Machine.Tick();
                Require(g.WeaponPreviewVisible && g.WeaponPreviewReinforcement == 9 && g.Machine.Running && g.Machine.Get("previewPassed") == 0, "x94 did not pause VM");
                for (int i = 0; i < 4; i++) g.Machine.Tick(); Require(g.Machine.Get("previewPassed") == 0, "VM ran through open preview");
                g.CloseWeaponPreview(); g.Machine.Tick(); Require(!g.Machine.Running && g.Machine.Get("previewPassed") == 1 && g.Equipment.Count == 1, "VM resume or preview awarded weapon");
                g.Machine.LoadScript = loader;
                checks.Add("Actual x94(id, reinforcement, true) pauses the script VM until close, then executes the next instruction; viewing alone grants no weapon");

                Stage(g); g.Machine.Command(0x64, new object[] { 15 }); Set(g, "spiritDurability", g.GetSpiritAttribute(1));
                g.Machine.Command(0x7F, new object[0]); look = Appearance(g); hp = g.CurrentHp; durability = g.durability;
                int skill = (int)Get(g, "equippedSkill"); Shop(g); g.level = 40;
                Require(g.PreviewShopWeapon(3), "Integrated shop preview"); g.CloseWeaponPreview();
                Require((bool)g.Machine.Command(0x91, new object[0]) && Appearance(g) == look && g.CurrentHp == hp && g.durability == durability && (int)Get(g, "equippedSkill") == skill && g.Actors[-2].hidden,
                    "Integrated preview corrupted spirit appearance/attributes/skill");
                checks.Add("Integrated preview preserves spirit sword appearance, integration flag, hidden companion, live durability, HP and active skill");

                Stage(g); Shop(g); g.level = 40; g.money = 100000; g.durability = 17; hp = g.CurrentHp; look = Appearance(g);
                Require(g.PreviewShopWeapon(3), "Save fixture preview"); g.saveFileName = "validation-preview.json"; g.Save();
                g.Load(); g.Machine.Tick();
                Require(!g.WeaponPreviewVisible && !(bool)Get(g, "menu") && g.Equipment[g.EquippedIndex].id == 0 && g.Equipment.Count == 1 && g.money == 100000 && g.durability == 17 && g.CurrentHp == hp && Appearance(g) == look,
                    "Preview leaked into save/load");
                Shop(g); Require(g.PreviewShopWeapon(3), "Restart preview fixture"); g.Begin();
                Require(!g.WeaponPreviewVisible && !(bool)Get(g, "menu"), "New game retained preview");
                checks.Add("Saving during a shop preview stores the actual weapon and durability; Load and Begin clear transient preview state without leaking the tried-on weapon");

                Stage(g); g.level = 40; Shop(g); g.money = 100000; money = g.money;
                Require(g.PreviewShopWeapon(3), "Buy preview"); g.CloseWeaponPreview();
                Require(g.SelectWeaponTrade(3, false) && g.ConfirmTrade() && g.Equipment.Count == 2 && g.Equipment[1].id == 3 && g.Equipment[0].id == 0 && g.money == money - g.Equipment[1].Data.buy,
                    "Purchase after preview");
                checks.Add("Returning from preview permits a normal confirmed purchase, creates a separate inventory instance, deducts the original price and keeps the current weapon equipped");

                g.Machine.Command(0x94, new object[] { 3, 9, true });
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                Time.timeScale = scale; UnityEngine.Random.state = random; g.saveFileName = save; g.Machine.LoadScript = loader;
                var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
                Directory.CreateDirectory("../Reference/Validation"); File.WriteAllText("../Reference/Validation/weapon-preview.json", JsonUtility.ToJson(report, true));
                Debug.Log("GUJIAN_WEAPON_PREVIEW_VALIDATION " + (error == null) + " " + error);
            }
        }
    }
}
