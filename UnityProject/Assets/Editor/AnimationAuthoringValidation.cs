using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class AnimationAuthoringValidation
    {
        private static void Require(bool value, string reason) { if (!value) throw new Exception(reason); }
        [MenuItem("古剑/验证动作编辑与生产加载")]
        public static void Run()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Edit mode required");
            var checks = new List<string>(); string error = null;
            EditableAnimation asset = null; string backup = null; GameObject root = null;
            var source = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Original/Animations/action/h_00.json");
            string sourceText = source.text;
            try
            {
                asset = AnimationAuthoring.GetOrCreate("h_00"); backup = EditorJsonUtility.ToJson(asset);
                Require(AnimationAuthoring.GetOrCreate("h_00") == asset, "Repeated import replaces edited asset");
                var originalPublished = asset.ReadPublished(); int before = originalPublished.actions[7].steps[0].vx;
                asset.draft.actions[7].steps[0].vx = before + 3;
                OriginalAssets.ClearCache();
                Require(OriginalAssets.Json<OriginalAnimation>("Animations/action/h_00").actions[7].steps[0].vx == before, "Unpublished draft leaked into gameplay");
                checks.Add("Native animation import preserves the original JSON and existing edits; gameplay reads the published snapshot, independent of draft edits");
                int oldCount = asset.draft.frames.Length, oldFrame = asset.draft.actions[7].steps[0].frame;
                int duplicated = AnimationAuthoring.DuplicateFrame(asset, 7, 0);
                Require(duplicated == oldCount && asset.draft.actions[7].steps[0].frame == duplicated && !ReferenceEquals(asset.draft.frames[oldFrame],asset.draft.frames[duplicated]), "Frame duplicate aliases source");
                int oldX = asset.draft.frames[oldFrame].attackRects[0].x;
                asset.draft.frames[duplicated].attackRects[0].x = oldX + 7;
                Require(asset.draft.frames[oldFrame].attackRects[0].x == oldX, "Shared source frame changed");
                AnimationAuthoring.SaveAndPublish(asset);
                var actorPrefab = OriginalAssets.Catalog.actors.Single(e=>e.id==200).prefab;
                root = (GameObject)PrefabUtility.InstantiatePrefab(actorPrefab.gameObject);
                var actor = root.GetComponent<OriginalActor>();
                actor.Initialize(OriginalAssets.Table<SpriteDefinition>("sprite").Single(d=>d.id==200), -1, new Vector2(100,100), 2);
                actor.returnToIdle = false; actor.Act(7); actor.Sample(7, 0);
                Require(actor.CurrentStep.vx == before + 3 && actor.CurrentStep.frame == duplicated && actor.CurrentFrame.attackRects[0].x == oldX+7, "Production actor ignored published animation");
                var r = actor.CurrentFrame.attackRects[0]; actor.mirror = false;
                var left = OriginalCombatMath.Rect(actor,r); actor.mirror = true;
                var right = OriginalCombatMath.Rect(actor,r);
                Require(left.x == 100+r.x && right.x == 100-r.x-r.w+actor.Animation.footWidth && right.width==left.width, "Published attack rectangle mirror");
                actor.Tick(.08001f);
                Require(actor.FrameIndex == 1, "Published animation lost 80ms timing");
                Require(actor.FramePieceCapacity >= asset.draft.frames.Max(f=>f.pieces.Length) && root.GetComponentsInChildren<SpriteRenderer>(true).All(s=>!s.sprite || AssetDatabase.Contains(s.sprite)), "Missing saved frame capacity or transient sprite");
                checks.Add("Independent frame copy publishes through the production loader and real actor; 80ms step timing and mirrored attack geometry remain intact, using saved renderer nodes and sprites");
                foreach (var sheet in AnimationAuthoring.ImageSources("h_00"))
                    foreach (string image in sheet.Value)
                        foreach (var rect in asset.draft.sheets[sheet.Key].rects.Where(q=>q.w>0&&q.h>0))
                            Require(OriginalAssets.Sprite(image,rect) && AssetDatabase.Contains(OriginalAssets.Sprite(image,rect)), "Outfit crop not baked: "+image);
                checks.Add("All configured normal weapons and four sword-spirit appearances resolve to persistent sprites for the edited role animation");
                var published = JsonUtility.ToJson(asset.ReadPublished());
                int validFrame = asset.draft.actions[7].steps[0].frame;
                asset.draft.actions[7].steps[0].frame = int.MaxValue;
                bool rejected = false;
                try { AnimationAuthoring.SaveAndPublish(asset); } catch (InvalidDataException) { rejected = true; }
                Require(rejected && JsonUtility.ToJson(asset.ReadPublished()) == published, "Invalid frame was published");
                asset.draft.actions[7].steps[0].frame = validFrame;
                int validWidth = asset.draft.sheets[0].rects[0].w; asset.draft.sheets[0].rects[0].w = int.MaxValue;
                rejected = false; try { AnimationAuthoring.SaveAndPublish(asset); } catch (InvalidDataException) { rejected = true; }
                Require(rejected && JsonUtility.ToJson(asset.ReadPublished()) == published, "Invalid crop was published");
                asset.draft.sheets[0].rects[0].w = validWidth;
                var isolated = asset.ReadPublished(); isolated.actions[7].steps[0].vx = -999;
                Require(asset.ReadPublished().actions[7].steps[0].vx == before+3, "Runtime read mutates asset");
                checks.Add("Invalid frame references and crop overflow reject before publication; runtime snapshots cannot mutate the persisted animation");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                if (root) UnityEngine.Object.DestroyImmediate(root);
                if (asset && backup != null) { EditorJsonUtility.FromJsonOverwrite(backup,asset); EditorUtility.SetDirty(asset); AssetDatabase.SaveAssetIfDirty(asset); }
                OriginalAssets.ClearCache();
                if (source.text != sourceText) error = (error ?? "") + "Original JSON changed";
                File.WriteAllText("../Reference/Validation/animation-authoring.json", JsonUtility.ToJson(new ReconstructionValidation.Report {
                    passed=error==null, utc=DateTime.UtcNow.ToString("O"), checks=checks.ToArray(), error=error },true));
                Debug.Log("GUJIAN_ANIMATION_AUTHORING "+(error==null)+" "+error);
            }
        }
    }
}
