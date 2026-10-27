using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class EnemyBackEffectValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags S = BindingFlags.Static | BindingFlags.NonPublic;
        private static object Get(object o, string n) => o.GetType().GetField(n, F).GetValue(o);
        private static void Set(object o, string n, object v) => o.GetType().GetField(n, F).SetValue(o, v);
        private static object Call(GuJianGame g, string n, params object[] a) => typeof(GuJianGame).GetMethod(n, F).Invoke(g, a);
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        private static void Refresh(GuJianGame g) => Call(g, "RefreshScenePresentation");
        private static OriginalActor Stage(GuJianGame g)
        {
            typeof(ComboValidation).GetMethod("Stage", S).Invoke(null, new object[] { g, false });
            Time.timeScale = 0;
            foreach (var a in g.Actors.Values) { a.hidden = true; a.locked = true; }
            g.Actors[-1].mapPosition = new Vector2(210, 250);
            g.Machine.Command(0x30, new object[] { 910, 203, "l", 210, 250, 2 });
            var target = g.Actors[910]; target.locked = true; target.ResetAction();
            Set(g, "lockInput", true); Set(g, "toastUntil", 0f); Call(g, "ResetStageInfo"); Call(g, "ResetCombatFeedback");
            Call(g, "UpdateCamera"); Refresh(g); return target;
        }
        [MenuItem("古剑/验证特殊敌人背后光环")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Play first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            float scale = Time.timeScale; var random = UnityEngine.Random.state;
            var checks = new List<string>(); string error = null;
            try
            {
                var definitions = OriginalAssets.Table<SpriteDefinition>("sprite");
                var special = definitions.Where(d => d.type == 11).ToArray();
                Require(special.Length == 10, "Ten original type11 definitions");
                foreach (var e in OriginalAssets.Catalog.actors)
                {
                    bool wanted = definitions.First(d => d.id == e.id).type == 11;
                    Require((bool)e.prefab.EnemyBackEffect == wanted, "Saved aura ownership " + e.id);
                    if (wanted) Require(AssetDatabase.Contains(e.prefab.EnemyBackEffect) && e.prefab.EnemyBackEffect.PresentationGroup, "Persistent aura child " + e.id);
                }
                var data = OriginalAssets.Json<OriginalAnimation>("Animations/action/s_36");
                Require(data.actions[0].steps.Length == 1 && data.frames.Length == 1 && data.frames[0].pieces.Length == 4, "Source single-step four-piece aura");
                foreach (var rect in data.sheets[0].rects) Require(OriginalAssets.Catalog.FindSprite("action/s_36", rect), "Persistent source crop");
                checks.Add("Exactly ten type11 actor prefabs contain a saved aura child; original s_36 is one static four-piece frame with persistent atlas crops");
                Stage(g);
                for (int i = 0; i < special.Length; i++)
                {
                    g.Machine.Command(0x30, new object[] { 920 + i, special[i].id, "l", 210 + i, 250, 2 });
                    var a = g.Actors[920 + i]; a.locked = true; Refresh(g); var effect = a.EnemyBackEffect;
                    Require(effect.Animation.frames.Length == 1 && effect.FrameIndex == 0 && !effect.hidden, "Type11 aura initialization " + a.definitionId);
                    var expected = a.mapPosition + new Vector2((a.Animation.footWidth - data.footWidth) >> 1, (a.Animation.footHeight - data.footHeight) >> 1);
                    Require(effect.mapPosition == expected && effect.transform.position == new Vector3(expected.x, -expected.y, 0), "Original integer foot centering " + a.definitionId);
                    var pieces = (List<SpriteRenderer>)Get(effect, "renderers");
                    Require(pieces.Count(r => r.enabled) == 4 && pieces.Where(r => r.enabled).All(r => r.sprite && AssetDatabase.Contains(r.sprite)), "Real saved aura pieces");
                }
                checks.Add("All ten actual production x30 spawns initialize the saved effect, draw its four persistent pieces and use the source signed-shift foot centering");
                var target = g.Actors[910]; var aura = target.EnemyBackEffect;
                var point = aura.mapPosition; target.mirror = true; target.frozen = true; Set(g, "menu", true); Set(g, "hideUi", true); Refresh(g);
                Require(!aura.hidden && !aura.mirror && aura.mapPosition == point && aura.FrameIndex == 0, "Mirror/menu/frozen aura");
                Set(g, "menu", false); Set(g, "hideUi", false); target.frozen = false;
                checks.Add("Mirroring does not flip the aura; opening the menu, hide-UI and freezing its owner preserve the original static aura");
                foreach (int stage in new[] { 0, 22, 24, 25 })
                {
                    Set(g, "criticalStage", stage); Refresh(g);
                    Require(aura.hidden == (stage == 24 || stage == 25), "Role isCriticalKill phase " + stage);
                }
                Set(g, "criticalStage", 0);
                g.Machine.Command(0x30, new object[] { 980, 232, "l", 210, 250, 2 });
                var boss = g.Actors[980]; boss.hidden = true; Set(g, "bossCritical", boss); Refresh(g);
                Require(!aura.hidden, "Boss critical is not role.isCriticalKill"); Set(g, "bossCritical", null);
                checks.Add("Only player critical hit phases24/25 hide the aura; preparation22 and a real Boss232 critical owner retain it");
                for (int layer = 0; layer < 4; layer++)
                {
                    target.layer = layer; Refresh(g);
                    Require(aura.hidden == (layer != 2), "Only source spriteOnLayer1 emits aura " + layer);
                    if (layer != 2) Require(aura.GetComponentsInChildren<SpriteRenderer>().All(r => !r.enabled), "Moved-layer stale aura");
                }
                target.layer = 2; target.hidden = true; Refresh(g); Require(aura.hidden, "Invisible owner aura");
                target.hidden = false; target.presentationHidden = true; Refresh(g); Require(aura.hidden, "Presentation-hidden owner aura");
                target.presentationHidden = false; Set(g, "miniMapOpen", true); Refresh(g); Require(aura.hidden, "Minimap aura"); Set(g, "miniMapOpen", false);
                Set(g, "worldMapOpen", true); Refresh(g); Require(aura.hidden, "Worldmap aura"); Set(g, "worldMapOpen", false); Refresh(g);
                checks.Add("Invisible owners, hidden presentation, map screens and moving to source layers0/1/3 suppress the aura without leaving stale renderers");
                var lower = g.Actors[900]; lower.hidden = false; lower.layer = 1; Refresh(g);
                Require(aura.PresentationGroup.sortAtRoot && aura.PresentationGroup.sortingOrder > -25000 && aura.PresentationGroup.sortingOrder < lower.PresentationGroup.sortingOrder, "Aura must precede all lower-layer bodies after map0");
                Require(g.Actors.Values.Where(a => a.EnemyBackEffect && a.layer == 2 && !a.hidden).All(a => a.EnemyBackEffect.PresentationGroup.sortingOrder < lower.PresentationGroup.sortingOrder), "All elite auras precede lower layer");
                checks.Add("Aura SortingGroups draw at root between map0 and every source layer1 actor, independently of their owning layer2 actor groups");
                int objects = UnityEngine.Object.FindObjectsOfType<GameObject>().Length, components = target.GetComponentsInChildren<Component>(true).Length, textures = Resources.FindObjectsOfTypeAll<Texture2D>().Length;
                for (int i = 0; i < 20; i++) { target.mapPosition += Vector2.one; Refresh(g); }
                Require(objects == UnityEngine.Object.FindObjectsOfType<GameObject>().Length && components == target.GetComponentsInChildren<Component>(true).Length && textures == Resources.FindObjectsOfTypeAll<Texture2D>().Length, "Runtime aura resource creation");
                checks.Add("Twenty production presentation refreshes and owner moves reuse saved nodes/components/textures");
                g.Begin(); g.Machine.Clear(); Refresh(g);
                Require(!g.Actors.Values.Any(a => a.EnemyBackEffect), "New-game registry retained elite aura");
                checks.Add("New game clears all elite actors and aura ownership from the active registry");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                Set(g, "criticalStage", 0); Set(g, "bossCritical", null); Set(g, "menu", false); Set(g, "hideUi", false); Set(g, "miniMapOpen", false); Set(g, "worldMapOpen", false);
                Time.timeScale = scale; UnityEngine.Random.state = random;
                var r = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
                File.WriteAllText("../Reference/Validation/enemy-back-effect.json", JsonUtility.ToJson(r, true)); Debug.Log("GUJIAN_ENEMY_BACK_EFFECT " + r.passed + " " + error);
            }
        }
        public static void PrepareVisuals() { Stage(UnityEngine.Object.FindObjectOfType<GuJianGame>()); }
        private static void Save(string name)
        {
            var image = (Texture2D)typeof(HudFeedbackValidation).GetMethod("Render", S).Invoke(null, null);
            try { File.WriteAllBytes("../Reference/Validation/" + name + ".png", image.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(image); }
        }
        public static void CaptureVisuals()
        {
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>(); var target = g.Actors[910]; var aura = target.EnemyBackEffect;
            aura.gameObject.SetActive(false); Save("enemy-aura-blank"); aura.gameObject.SetActive(true); Save("enemy-aura-visible");
            var lower = g.Actors[900]; lower.hidden = false; lower.layer = 1; lower.mapPosition = new Vector2(210, 285); lower.ResetAction();
            aura.gameObject.SetActive(false); Save("enemy-aura-covered-blank"); aura.gameObject.SetActive(true); Save("enemy-aura-covered");
            lower.hidden = true; Set(g, "criticalStage", 24); Save("enemy-aura-critical"); Set(g, "criticalStage", 0);
            Set(g, "hideUi", true); Save("enemy-aura-hideui"); Set(g, "hideUi", false);
        }
    }
}
