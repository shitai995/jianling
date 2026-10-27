using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class WeaponVisibilityValidation
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> checks = new List<string>();
        private static object Get(object o, string name) => o.GetType().GetField(name, Flags).GetValue(o);
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        private static void Tick(GuJianGame g) { for (int i = 0; i < 30 && g.Machine.Running; i++) g.Machine.Tick(4096); Require(!g.Machine.Running && !g.DialogueVisible, "Fixture did not settle"); }
        private static void Stage(GuJianGame g)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = 9999;
            g.Machine.Enqueue("200"); Tick(g);
            foreach (var enemy in g.Actors.Values.Where(a => a.IsEnemy)) enemy.locked = true;
        }
        [MenuItem("古剑/验证收剑显示与攻击限制")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            string previousSave = g.saveFileName, error = null; float scale = Time.timeScale;
            checks.Clear(); Time.timeScale = 0; g.saveFileName = "validation-weapon-visibility.json";
            try
            {
                Stage(g); g.Machine.Command(0x53, new object[] { 9, 0, false }); g.Machine.Command(0x55, new object[] { 9 });
                var role = g.Actors[-1];
                int action = Array.FindIndex(role.Animation.actions, a => a.steps.Any(s => role.Animation.frames[s.frame].pieces.Any(p => p.image == 4)));
                int frame = Array.FindIndex(role.Animation.actions[action].steps, s => role.Animation.frames[s.frame].pieces.Any(p => p.image == 4));
                role.Sample(action, frame); var pieces = role.CurrentFrame.pieces;
                var renderers = (List<SpriteRenderer>)Get(role, "renderers");
                int nodes = role.GetComponentsInChildren<Transform>(true).Length, bag = g.Equipment.Count, skill = (int)Get(g, "equippedSkill");
                var bodyIndices = Enumerable.Range(0, pieces.Length).Where(i => pieces[i].image != 4 && renderers[i].enabled).ToArray();
                var sprites = bodyIndices.Select(i => renderers[i].sprite).ToArray();
                string image = ((string[])Get(role, "images"))[4];
                Require(Enumerable.Range(0, pieces.Length).Any(i => pieces[i].image == 4 && renderers[i].enabled), "No visible weapon sample");
                g.Machine.Command(0x56, new object[0]);
                Require(!role.WeaponVisible && Enumerable.Range(0, pieces.Length).Where(i => pieces[i].image == 4).All(i => !renderers[i].enabled), "Weapon sheet not hidden");
                Require(bodyIndices.Select((i, j) => renderers[i].enabled && renderers[i].sprite == sprites[j]).All(x => x) && nodes == role.GetComponentsInChildren<Transform>(true).Length, "Body or baked nodes changed");
                Require(((string[])Get(role, "images"))[4] == image && skill == (int)Get(g, "equippedSkill") && bag == g.Equipment.Count, "Stow replaced texture/skill/equipment");
                checks.Add("x56 disables only role sheet 4 renderers; body sprites and baked node count are unchanged; equipped texture, skill and equipment instance remain intact");

                role.ResetAction(); g.Machine.Command(0x98, new object[0]); int version = role.ActionVersion, durability = g.durability;
                g.Attack();
                Require(role.ActionVersion == version && !g.UseSkill(4) && !g.UseSkill(5) && !g.TryCriticalKill() && g.Energy == 2000 && g.durability == durability, "Stowed player attacks or consumed resources");
                Require(g.UseSkill(5, 1, true), "Stow blocked scripted choreography"); role.ResetAction();
                checks.Add("Stowed weapon rejects player normal attacks, break, skill and critical without energy/durability loss; script-forced skill remains available for choreography");

                g.Machine.Command(0x55, new object[] { 0 });
                Require(!role.WeaponVisible && g.equipmentId == 0, "Changing equipment unexpectedly drew weapon");
                string replacement = ((string[])Get(role, "images"))[4];
                g.Machine.Command(0x55, new object[] { -1 });
                Require(role.WeaponVisible && ((string[])Get(role, "images"))[4] == replacement, "x55(-1) did not draw current texture");
                Require(g.UseSkill(0), "Drawing weapon did not restore attack"); role.ResetAction();
                checks.Add("Changing equipment while stowed keeps it hidden; x55(-1) draws the current weapon and restores player attacks");

                g.Machine.Command(0x64, new object[] { 20 });
                typeof(GuJianGame).GetField("spiritDurability", Flags).SetValue(g, (int)typeof(GuJianGame).GetMethod("SpiritAttribute", Flags).Invoke(g, new object[] { 1 }));
                g.Machine.Command(0x7F, new object[0]); role.ResetAction();
                image = ((string[])Get(role, "images"))[4];
                g.Machine.Command(0x56, new object[0]); g.Machine.Command(0x55, new object[] { -1 });
                Require(role.WeaponVisible && ((string[])Get(role, "images"))[4] == image && role.actionBank == 27, "Draw replaced integrated spirit weapon");
                checks.Add("Integrated stow/draw retains the active spirit weapon texture and action bank 27");

                g.Machine.Command(0x56, new object[0]); g.Save(); g.Load(); Tick(g); role = g.Actors[-1];
                Require(!role.WeaponVisible && role.actionBank == 27 && !g.UseSkill(5), "Stowed integrated save/load");
                string path = Path.Combine(Application.persistentDataPath, g.saveFileName);
                string legacy = System.Text.RegularExpressions.Regex.Replace(File.ReadAllText(path), "\"weaponStowed\"\\s*:\\s*(true|false)\\s*,?", "");
                File.WriteAllText(path, legacy); g.Load(); Tick(g); role = g.Actors[-1];
                Require(role.WeaponVisible, "Old v3 save did not default to drawn weapon");
                checks.Add("Save/Load retains integrated stow state; older v3 saves without weaponStowed default to drawn weapon");

                g.Machine.Command(0x56, new object[0]); g.Machine.Enqueue("200"); Tick(g);
                Require(g.Actors[-1].WeaponVisible, "Map reload did not reset attackability");
                g.Machine.Command(0x56, new object[0]); g.Begin();
                Require(g.Actors[-1].WeaponVisible, "New game retained stow state"); g.Machine.Clear();
                checks.Add("Map initialization and a new game restore visible/attackable state, matching GameCanvas.initMapElement");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                Time.timeScale = scale; g.saveFileName = previousSave;
                File.WriteAllText("../Reference/Validation/weapon-visibility.json", JsonUtility.ToJson(new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
                Debug.Log("GUJIAN_WEAPON_VISIBILITY " + (error == null) + " " + error);
            }
        }
    }
}
