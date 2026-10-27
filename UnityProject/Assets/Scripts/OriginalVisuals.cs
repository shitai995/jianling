using System.Collections.Generic;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private sealed class Visual
        {
            public OriginalActor renderer, parent;
            public float until;
        }
        private readonly Dictionary<int, Visual> faces = new Dictionary<int, Visual>();
        private OriginalActor guide, skillFront, skillBack;
        private int guideTarget = -3, loadedSkill = -1;
        private OriginalActor CreateVisual(string name, string animation, string[] images, OriginalActor parent)
        {
            var entry = System.Array.Find(OriginalAssets.Catalog.effects, e => e.animation == animation);
            if (entry == null) throw new System.InvalidOperationException("Missing effect prefab " + animation);
            var actor = Instantiate(entry.prefab, parent.transform, false);
            actor.Initialize(new SpriteDefinition { id = -1000, type = 7, name = name, animation = animation, images = images }, -1000, parent.mapPosition, 3);
            return actor;
        }
        private void ShowFace(int id, int face, int cycles)
        {
            var parent = Actor(id); if (!parent) return;
            if (!faces.TryGetValue(id, out var v) || !v.renderer)
            {
                v = new Visual { parent = parent, renderer = CreateVisual("表情", "e_99", new[] { "action/s_99" }, parent) };
                faces[id] = v;
            }
            if (face < 0 || face >= v.renderer.Animation.actions.Length) return;
            v.renderer.Act(face);
            v.until = Time.time + v.renderer.Animation.actions[face].steps.Length * Mathf.Max(1, cycles) * .08f;
        }
        private int SkillBehavior(int id) => System.Array.Find(OriginalAssets.Table<SkillDefinition>("skill"), s => s.id == id)?.BehaviorId ?? id;
        private void LoadSkillVisual(int id)
        {
            id = SkillBehavior(id);
            if (loadedSkill == id && skillFront) return;
            if (skillFront) Destroy(skillFront.gameObject);
            if (skillBack) Destroy(skillBack.gameObject);
            skillFront = skillBack = null; loadedSkill = id;
            string animation = null;
            string[] images = null;
            switch (id)
            {
                case 5: case 9: animation = "skill_" + id.ToString("00"); images = new[] { "action/s_20" }; break;
                case 6: animation = "skill_06"; images = new[] { "action/s_19" }; break;
                case 7: case 11: animation = "skill_" + id.ToString("00"); images = new[] { "action/s_21" }; break;
                case 8: case 12: animation = "skill_" + id.ToString("00"); images = new[] { "action/s_22" }; break;
                case 10: animation = "skill_10"; images = new[] { "action/s_11", "action/s_19", "action/s_08" }; break;
                case 14: case 15: case 16: case 17: case 18: case 19: case 20: case 21:
                    animation = "skill_14"; images = new[] { "action/s_24" }; break;
            }
            if (animation == null || !Role) return;
            skillFront = CreateVisual("技能前景", animation, images, Role);
            if (skillFront.Animation.actions.Length > 1) skillBack = CreateVisual("技能背景", animation, images, Role);
        }
        private static void SortVisual(OriginalActor actor, int order)
        {
            actor.SetPresentationOrder(order);
        }
        private void TickVisuals(float delta)
        {
            DrawSupportSkillVisuals();
            foreach (var entry in faces.Values)
            {
                if (!entry.parent || !entry.renderer) continue;
                var parent = entry.parent; var a = entry.renderer;
                a.hidden = AtTitle || parent.hidden || parent.presentationHidden || Time.time >= entry.until;
                float dy = parent.CurrentFrame.edgeRect.y - a.Animation.footHeight * 2;
                if (parent.actorId == -1) dy += 8;
                else if (parent.actorId == -2) { }
                else if (parent.IsEnemy) dy -= 16;
                a.mapPosition = parent.mapPosition + new Vector2((parent.Animation.footWidth - a.Animation.footWidth) / 2f, dy);
                a.Tick(delta); SortVisual(a, 2000);
            }
            var target = Actor(guideTarget);
            if (target && Role)
            {
                if (!guide) guide = CreateVisual("任务指引", "z_arrow", new[] { "action/s_35" }, Role);
                var d = target.mapPosition - Role.mapPosition;
                guide.hidden = AtTitle || hideUi || CriticalActive || (Mathf.Abs(d.x) <= 80 && Mathf.Abs(d.y) <= 60);
                float slope = Mathf.Abs(d.x) < .01f ? 1000000 : Mathf.Abs(d.y / d.x);
                int index = slope > 2.41421f ? (d.y < 0 ? 0 : 4) : slope < .41421f ? (d.x < 0 ? 2 : 6) :
                    d.x < 0 ? (d.y < 0 ? 1 : 3) : (d.y < 0 ? 7 : 5);
                guide.mapPosition = Role.mapPosition;
                guide.Sample(index, Mathf.FloorToInt(Time.time / .08f)); SortVisual(guide, -250);
            }
            else if (guide) { guide.hidden = true; guide.RefreshFrame(); }
            if (skillFront && Role)
            {
                int action = Role.LogicalAction;
                bool visible = action >= 12 && action <= 21 && action != 20 && !Role.hidden && !Role.presentationHidden && !AtTitle;
                skillFront.hidden = !visible; skillFront.mirror = Role.mirror; skillFront.mapPosition = Role.mapPosition;
                skillFront.Sample(0, Role.FrameIndex);
                SortVisual(skillFront, 1000);
                if (skillBack)
                {
                    skillBack.hidden = !visible || (action != 12 && action != 14 && action != 16 && action != 21);
                    skillBack.mirror = Role.mirror; skillBack.mapPosition = Role.mapPosition;
                    skillBack.Sample(1, Role.FrameIndex); SortVisual(skillBack, -500);
                }
            }
        }
    }
}
