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
    public static class ProjectileValidation
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly List<string> checks = new List<string>();
        private static UnityEngine.Random.State random;
        private static float scale;
        private static double started;
        private static int emissions, initialHp;
        private static object Get(object o, string n) => o.GetType().GetField(n, Private | BindingFlags.Public).GetValue(o);
        private static void Set(object o, string n, object v) => o.GetType().GetField(n, Private | BindingFlags.Public).SetValue(o, v);
        private static object Call(GuJianGame g, string n, params object[] args) => typeof(GuJianGame).GetMethod(n, Private).Invoke(g, args);
        private static object Fighter(GuJianGame g, int id) => ((IDictionary)Get(g, "fighters"))[id];
        private static IList Projectiles(GuJianGame g) => (IList)Get(g, "projectiles");
        private static OriginalActor Visual(object p) => (OriginalActor)Get(p, "actor");
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        private static void Tick(GuJianGame g, int count = 1)
        {
            for (int i = 0; i < count; i++) Call(g, "TickProjectiles", .08001f);
        }
        private static void Clear(GuJianGame g) => Call(g, "ClearProjectiles", new object[] { null });
        private static OriginalActor Spawn(GuJianGame g, int id)
        {
            Clear(g);
            g.Machine.Command(0x31, new object[] { 900 });
            g.Machine.Command(0x30, new object[] { 900, id, "r", 100, 250, 2 });
            var a = g.Actors[900]; a.locked = true;
            var f = Fighter(g, 900);
            Set(f, "attackMode", 2); Set(f, "hit", 100000); Set(f, "attack", 3000); Set(f, "critical", 1000);
            var role = g.Actors[-1]; role.ResetAction(); role.mapPosition = new Vector2(300, 250);
            role.hp = role.maxHp = 1000000; role.hidden = false; role.frozen = false;
            Set(Fighter(g, -1), "dead", false); Set(Fighter(g, -1), "hold", 1000000);
            return a;
        }
        private static object Emit(GuJianGame g, OriginalActor owner, int action = 21, int direction = 1, int speed = 30,
            bool persistent = false, bool end = false, bool dodge = true)
        {
            Call(g, "AddProjectile", Fighter(g, owner.actorId), action, owner.mapPosition, direction, speed, persistent, end, dodge);
            return Projectiles(g)[Projectiles(g).Count - 1];
        }
        private static void NextDodge(bool dodge)
        {
            for (int seed = 0; seed < 1000; seed++)
            {
                UnityEngine.Random.InitState(seed);
                if ((UnityEngine.Random.Range(0, 100) < 10) != dodge) continue;
                UnityEngine.Random.InitState(seed); return;
            }
            throw new Exception("Dodge fixture seed missing");
        }

        [MenuItem("古剑/验证远程招式与飞行物")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            scale = Time.timeScale; random = UnityEngine.Random.state; Time.timeScale = 0;
            checks.Clear(); started = EditorApplication.timeSinceStartup;
            try
            {
                g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999;
                g.Machine.Enqueue("200"); g.Machine.Tick();
                Require(g.Map != null && !g.Machine.Running && !g.DialogueVisible, "Test map load failed");
                foreach (var a in g.Actors.Values) a.locked = true;
                Set(g, "cameraPoint", new Vector2(266.5f, 250));
                foreach (int id in new[] { 210, 211, 212, 219, 221 })
                foreach (bool right in new[] { false, true })
                {
                    var a = Spawn(g, id); a.Face(right ? "r" : "l"); a.PlayOnce(19);
                    for (int i = 0; i < 4; i++) a.Tick(.08001f);
                    Require(g.ActiveProjectileCount == 0, "Early emission " + id);
                    a.Tick(.08001f);
                    Require(g.ActiveProjectileCount == 1 && Visual(Projectiles(g)[0]).actionIndex == 21, "Missing frame-4 emission " + id);
                    var p = Projectiles(g)[0]; var visual = Visual(p);
                    Require(!visual.mirror && (Vector2)Get(p, "velocity") == new Vector2(right ? 30 : -30, 0), "Direction/mirror " + id);
                    var saved = Array.Find(OriginalAssets.Catalog.effects, e => e.animation == a.Definition.animation).prefab;
                    Require(EditorUtility.IsPersistent(saved) && visual.GetComponentsInChildren<SpriteRenderer>().Length == saved.GetComponentsInChildren<SpriteRenderer>().Length &&
                        visual.transform.parent != a.transform, "Projectile must preserve saved prefab node capacity under the actor root");
                    for (int i = 0; i < 5; i++) a.Tick(.08001f);
                    Require(g.ActiveProjectileCount == 1, "Early second emission " + id);
                    a.Tick(.08001f); Require(g.ActiveProjectileCount == 2, "Missing frame-10 emission " + id);
                }
                checks.Add("All five ranged enemy definitions emit original action 21 at frames 4 and 10 in both directions, speed 30, unmirrored saved Prefab instances");

                foreach (int id in new[] { 240, 241, 242 })
                foreach (int action in new[] { 18, 19 })
                {
                    var a = Spawn(g, id); Set(Fighter(g, 900), "attackMode", action - 17); a.PlayOnce(action);
                    for (int i = 0; i < 9; i++) a.Tick(.08001f);
                    Require(g.ActiveProjectileCount == 0, "Early stationary spell");
                    a.Tick(.08001f);
                    Require(g.ActiveProjectileCount == 1, "Stationary spell missing");
                    var p = Projectiles(g)[0];
                    Require(Visual(p).actionIndex == action + 9 && Visual(p).mapPosition == g.Actors[-1].mapPosition && (Vector2)Get(p, "velocity") == Vector2.zero, "Wrong stationary action/placement");
                }
                checks.Add("Enemies 240/241/242 emit stationary original actions 27/28 at the role position on frame 9 of actions 18/19");

                foreach (int id in new[] { 238, 239 })
                {
                    var a = Spawn(g, id); a.PlayOnce(19);
                    for (int i = 0; i < 9; i++) a.Tick(.08001f);
                    Require(a.mapPosition.x < 200, "Teleport occurred early");
                    a.Tick(.08001f);
                    var role = g.Actors[-1];
                    var expected = role.mapPosition + new Vector2((role.Animation.footWidth - a.Animation.footWidth) >> 1,
                        (role.Animation.footHeight - a.Animation.footHeight) >> 1);
                    Require(a.mapPosition == expected && g.ActiveProjectileCount == 0, "Enemy 238/239 frame-9 teleport alignment");
                }
                checks.Add("Enemies 238/239 action 19 frame 9 aligns their foot rectangles with the role without emitting a projectile");

                var owner = Spawn(g, 210); var roleActor = g.Actors[-1]; roleActor.mapPosition = new Vector2(180, 250);
                var bullet = Emit(g, owner, speed: 120); int hp = g.CurrentHp, damage = (int)Get(bullet, "damage");
                Require(!OriginalCombatMath.Hit(Visual(bullet), roleActor), "Sweep starts overlapping");
                Visual(bullet).mapPosition += new Vector2(120, 0);
                Require(!OriginalCombatMath.Hit(Visual(bullet), roleActor), "Sweep endpoint overlaps");
                Visual(bullet).mapPosition -= new Vector2(120, 0);
                NextDodge(false); Tick(g);
                Require(g.ActiveProjectileCount == 0 && g.CurrentHp == hp - damage, "High-speed projectile tunneled or inherited owner critical");
                checks.Add("Actual original hitboxes: 120-pixel travel hits a target between non-overlapping endpoints; one-shot is consumed and does not inherit owner 100% critical");

                owner = Spawn(g, 210); roleActor.mapPosition = owner.mapPosition;
                bullet = Emit(g, owner); hp = g.CurrentHp; NextDodge(true); Tick(g);
                Require(g.CurrentHp == hp && g.ActiveProjectileCount == 0, "Dodged projectile was not consumed");
                bullet = Emit(g, owner, speed: 0, persistent: true, dodge: false);
                Set(Fighter(g, -1), "hold", 1000000); Tick(g);
                Require(g.CurrentHp < hp && roleActor.actionIndex == 2, "Undodgeable projectile did not interrupt armor");
                checks.Add("Dodged one-shot disappears without HP loss; an undodgeable projectile forces the original role hurt action through armor");

                owner = Spawn(g, 240); owner.mapPosition = roleActor.mapPosition;
                bullet = Emit(g, owner, 27, speed: 0, persistent: true, end: true, dodge: false);
                // Keep hold above zero so forced interruptions do not change the hurtbox fixture.
                Set(bullet, "canDodge", true); Set(Fighter(g, -1), "hold", 1000000);
                hp = g.CurrentHp; damage = (int)Get(bullet, "damage");
                for (int i = 0; i < 18; i++) { roleActor.Tick(.08001f); NextDodge(false); Tick(g); }
                Require(g.CurrentHp == hp - damage * 4 && g.ActiveProjectileCount == 0, "Stationary spell hit count or end cleanup mismatch");
                checks.Add("Action 27 applies exactly four equal hits on attack frames 6/9/12/15 with refreshed target hurt windows, then disappears on frame 18");

                owner = Spawn(g, 240); owner.mapPosition = roleActor.mapPosition;
                bullet = Emit(g, owner, 28, speed: 0, persistent: true, end: true, dodge: true);
                Set(Fighter(g, -1), "hold", 1000000); hp = g.CurrentHp; damage = (int)Get(bullet, "damage");
                for (int i = 0; i < 13; i++) { roleActor.Tick(.08001f); NextDodge(false); Tick(g); }
                Require(g.CurrentHp == hp - damage * 7 && g.ActiveProjectileCount == 0, "Action 28 repeated hit count or cleanup mismatch");
                checks.Add("Action 28 applies seven hits on consecutive attack frames 6 through 12 as target hurt windows advance, then disappears on frame 13");

                owner = Spawn(g, 210); roleActor.hidden = true; roleActor.mapPosition = owner.mapPosition;
                bullet = Emit(g, owner, speed: 0); hp = g.CurrentHp; Tick(g, 4);
                Require(g.CurrentHp == hp && g.ActiveProjectileCount == 1, "Hidden target hit or looping bullet expired");
                roleActor.hidden = false; roleActor.mapPosition = new Vector2(400, 250);
                Visual(bullet).mapPosition = new Vector2(300, 250); Set(bullet, "velocity", new Vector2(30, 0));
                Set(g, "cameraPoint", new Vector2(266.5f, 250));
                foreach (string flag in new[] { "menu", "reinforceOpen" })
                {
                    var pos = Visual(bullet).mapPosition; int frame = Visual(bullet).FrameIndex;
                    Set(g, flag, true); Tick(g, 20); Set(g, flag, false);
                    Require(Visual(bullet).mapPosition == pos && Visual(bullet).FrameIndex == frame, "UI failed to freeze projectile: " + flag);
                }
                Call(g, "Talk", "", "validation", null, false, null, false); Tick(g, 10); g.ContinueDialogue();
                Require(Visual(bullet).mapPosition.x == 300, "Dialogue failed to freeze projectile");
                Set(g, "criticalStage", 24); Tick(g, 10); Set(g, "criticalStage", 0);
                Require(Visual(bullet).mapPosition.x == 300, "Role critical failed to freeze projectile");
                Visual(bullet).mapPosition = new Vector2(520, 250); Tick(g);
                Require(g.ActiveProjectileCount == 0, "Offscreen projectile not removed");
                bullet = Emit(g, owner); Visual(bullet).mapPosition = new Vector2(g.Map.Width - 20, 250); Tick(g);
                Require(g.ActiveProjectileCount == 0, "Map boundary projectile not removed");
                checks.Add("Hidden role receives no hits; looping bullets have no artificial TTL; menu, reinforcement, dialogue and role critical freeze movement/animation; screen and map boundaries recycle bullets");

                Emit(g, owner); g.Machine.Command(0x31, new object[] { 900 });
                Require(g.ActiveProjectileCount == 0, "Removed owner left a projectile");
                owner = Spawn(g, 210); Emit(g, owner); g.Machine.Enqueue("200"); g.Machine.Tick();
                Require(g.ActiveProjectileCount == 0, "Stage change left projectiles");
                owner = Spawn(g, 210); Emit(g, owner); g.Begin();
                Require(g.ActiveProjectileCount == 0, "New game left projectiles");
                checks.Add("Explicit owner removal, stage replacement and a new game clear live projectile records and instances");

                g.Machine.Clear(); g.Machine.Values["event"] = -999; g.Machine.Enqueue("200"); g.Machine.Tick();
                foreach (var a in g.Actors.Values) a.locked = true;
                owner = Spawn(g, 210); emissions = 0; initialHp = g.CurrentHp;
                owner.Stepped += a => { if (a.actionIndex == 19 && (a.FrameIndex == 4 || a.FrameIndex == 10)) emissions++; };
                owner.PlayOnce(19); Set(g, "fixedCamera", true); Set(g, "cameraPoint", new Vector2(266.5f, 250));
                Time.timeScale = 2;
                EditorApplication.update -= Advance; EditorApplication.update += Advance;
            }
            catch (Exception e) { Finish(g, false, e.ToString()); }
        }

        private static void Advance()
        {
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            try
            {
                Require(EditorApplication.isPlaying && g && g.LastError == null, "Runtime failed");
                Require(EditorApplication.timeSinceStartup - started < 35, "Timed out waiting for real Update projectiles");
                if (emissions < 2 || g.ActiveProjectileCount > 0) return;
                Require(g.CurrentHp < initialHp, "Real Update bullets missed target");
                checks.Add("Real Unity Update plays enemy action 19, emits both original bullets, moves them into actual role hurtboxes, applies damage and removes both");
                Finish(g, true, null);
            }
            catch (Exception e) { Finish(g, false, e.ToString()); }
        }

        private static void Finish(GuJianGame g, bool passed, string error)
        {
            EditorApplication.update -= Advance; Time.timeScale = scale; UnityEngine.Random.state = random;
            if (g) { Set(g, "criticalStage", 0); Set(g, "menu", false); Set(g, "reinforceOpen", false); Set(g, "fixedCamera", false); Clear(g); }
            var report = new ReconstructionValidation.Report { passed = passed, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/projectiles.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_PROJECTILE_VALIDATION " + passed + " " + error);
        }
    }
}
