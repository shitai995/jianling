using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GuJian
{
    public sealed class OriginalActor : MonoBehaviour
    {
        public int actorId, definitionId, layer, actionIndex;
        public int BehaviorId => Definition == null ? definitionId : Definition.BehaviorId;
        public Vector2 mapPosition;
        public string direction = "l", interactionScript, deathScript, specialScript;
        public bool hidden, frozen, locked, presentationHidden, keepLastFrame, mirror, screenCentered;
        public int level = 1, hp, maxHp;
        private CombatHealth health;
        public CombatHealth Health => health ?? (health = new CombatHealth(() => hp, value => hp = value, () => maxHp));
        public SpriteDefinition Definition { get; private set; }
        public OriginalAnimation Animation { get; private set; }
        public int FrameIndex { get; private set; }
        public bool WeaponVisible { get; private set; } = true;
        public AnimationStep CurrentStep => Animation.actions[actionIndex].steps[FrameIndex];
        public OriginalFrame CurrentFrame => Animation.frames[CurrentStep.frame];
        public Texture2D ImageTexture(int sheet) => images != null && sheet >= 0 && sheet < images.Length && !string.IsNullOrEmpty(images[sheet]) ? OriginalAssets.Texture(images[sheet]) : null;
        public bool IsEnemy => Definition.type == 9 || Definition.type == 10 || Definition.type == 11;
        public bool IsBusy => LogicalAction > 1 && FrameIndex < Animation.actions[actionIndex].steps.Length - 1;
        [SerializeField] private List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        [SerializeField] private SortingGroup presentationGroup;
        [SerializeField] private OriginalWorldFeedback combatFeedback;
        [SerializeField] private OriginalActor enemyBackEffect;
        public OriginalActor EnemyBackEffect => enemyBackEffect;
        public SortingGroup PresentationGroup => presentationGroup;
        public int FramePieceCapacity => renderers.Count;
        public OriginalWorldFeedback CombatFeedback => combatFeedback;
        public void SetPresentationOrder(int order, bool atRoot = false)
        {
            if (!presentationGroup) throw new System.InvalidOperationException("Missing saved SortingGroup: " + name);
            presentationGroup.sortingOrder = order; presentationGroup.sortAtRoot = atRoot;
        }
        public bool useLayoutTransform;
        private string[] images;
        private float elapsed;
        private static Sprite solidSprite;
        private int scriptedTicks = -1;
        public bool Completed { get; private set; }
        public bool ScriptControlled => scriptedTicks >= 0;
        public int ActionVersion { get; private set; }
        [System.NonSerialized] public int actionBank;
        public int LogicalAction => actionIndex - actionBank;
        public event System.Action<OriginalActor> Stepped;
        public event System.Action<OriginalActor> Cycled;
        public event System.Action<OriginalActor> ActionFinished;
        public bool returnToIdle;
        public bool suppressMirror;
        public Vector2 renderOffset;
#if UNITY_EDITOR
        public void PrepareEnemyBackEffect(OriginalActor effect)
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Prepare prefab in edit mode");
            if (!enemyBackEffect) enemyBackEffect = effect;
        }
        public void PrepareFramePieces(int count)
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Prepare prefab in edit mode");
            PreparePresentation();
            while (renderers.Count < count)
            {
                var piece = new GameObject("Piece " + renderers.Count);
                piece.transform.SetParent(transform, false);
                renderers.Add(piece.AddComponent<SpriteRenderer>());
            }
        }
        public void PreparePresentation(OriginalWorldFeedback feedback = null)
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Prepare prefab in edit mode");
            if (!presentationGroup) presentationGroup = GetComponent<SortingGroup>();
            if (!presentationGroup) presentationGroup = gameObject.AddComponent<SortingGroup>();
            if (feedback) combatFeedback = feedback;
        }
#endif
        private bool hitThisStep;
        public bool CanReceiveHit => CurrentStep.vulnerable && !hitThisStep;
        public void MarkHit() { hitThisStep = true; }
        public void HoldFrame() { keepLastFrame = true; Completed = true; }
        // Defense and horizontal knockback repeat their last step, with a fresh hurt window each tick.
        public void ContinueLastStep() { keepLastFrame = true; Completed = false; hitThisStep = false; }

        public void Initialize(SpriteDefinition definition, int id, Vector2 position, int sortingLayer)
        {
            Definition = definition;
            definitionId = definition.id;
            actorId = id;
            layer = sortingLayer;
            mapPosition = position;
            images = (string[])definition.images.Clone();
            WeaponVisible = true;
            // The original role replaces the fifth image sheet with its equipped weapon.
            if (definition.type == 0 && images.Length > 4) images[4] = "action/w_01";
            Animation = OriginalAssets.Json<OriginalAnimation>("Animations/action/" + definition.animation);
            name = id + " " + definition.name;
            if (enemyBackEffect)
            {
                enemyBackEffect.hidden = true;
                enemyBackEffect.Initialize(new SpriteDefinition { id = -1001, type = 7, name = "Enemy back effect", animation = "s_36", images = new[] { "action/s_36" } }, -1001, position, sortingLayer);
            }
            RefreshFrame();
        }

        public void EquipWeapon(string image)
        {
            if (Definition.type == 0 && images.Length > 4) { images[4] = "action/" + image; RefreshFrame(); }
        }

        public void EquipSpiritWeapon(string image)
        {
            if (Definition.type == 0 && images.Length > 1) { images[1] = "action/" + image; RefreshFrame(); }
        }

        public void SetWeaponVisible(bool visible) { WeaponVisible = visible; RefreshFrame(); }

        public void Face(string value)
        {
            direction = value;
            if (value == "r") mirror = true;
            if (value == "l") mirror = false;
            if (Definition.type >= 1 && Definition.type <= 5)
            {
                mirror = value == "r" || value == "d";
                if (actionIndex <= 1) actionIndex = value == "u" || value == "d" ? 1 : 0;
            }
            RefreshFrame();
        }

        public void Act(int index, bool hold = false)
        {
            if (index < 0 || index >= Animation.actions.Length) return;
            if (actionIndex != index || hold)
            {
                actionIndex = index;
                FrameIndex = 0;
                elapsed = 0;
                Completed = false;
                hitThisStep = false;
                ActionVersion++;
            }
            keepLastFrame = hold;
            RefreshFrame();
        }

        public void ScriptAct(int index, int duration, bool hold, bool centered)
        {
            Act(index, hold);
            FrameIndex = 0;
            elapsed = 0;
            screenCentered = centered;
            scriptedTicks = duration > 0 ? duration : Animation.actions[actionIndex].steps.Length - 1;
        }

        public void PlayOnce(int index)
        {
            scriptedTicks = -1;
            screenCentered = false;
            Act(index, true);
        }

        public void ResetAction()
        {
            scriptedTicks = -1;
            screenCentered = false;
            // Reset the clock and hurt window even when already in the idle action.
            Act(actionBank, true);
            keepLastFrame = false;
            FrameIndex = 0;
            RefreshFrame();
        }

        public void Tick(float delta)
        {
            if (frozen) { RefreshFrame(); return; }
            elapsed += delta;
            while (elapsed >= .08f)
            {
                elapsed -= .08f;
                Cycled?.Invoke(this);
                if (Completed && keepLastFrame) continue;
                int stepVersion = ActionVersion;
                Stepped?.Invoke(this);
                if (stepVersion != ActionVersion) continue;
                if (scriptedTicks == 0)
                {
                    scriptedTicks = -1;
                    if (!keepLastFrame) { ResetAction(); continue; }
                    Completed = true;
                }
                if (Completed && keepLastFrame) continue;
                if (scriptedTicks > 0) scriptedTicks--;
                var steps = Animation.actions[actionIndex].steps;
                if (FrameIndex + 1 < steps.Length) { FrameIndex++; hitThisStep = false; }
                else
                {
                    Completed = true;
                    int version = ActionVersion;
                    ActionFinished?.Invoke(this);
                    if (version != ActionVersion) continue;
                    if (keepLastFrame) continue;
                    if ((Definition.type == 0 || returnToIdle) && LogicalAction > 1)
                    {
                        actionIndex = actionBank;
                        ActionVersion++;
                    }
                    FrameIndex = 0;
                    hitThisStep = false;
                }
            }
            RefreshFrame();
        }

        public void Sample(int action, int frame)
        {
            actionIndex = action;
            FrameIndex = frame % Animation.actions[action].steps.Length;
            RefreshFrame();
        }

        public void RefreshFrame()
        {
            if (Animation == null) return;
            transform.position = new Vector3(mapPosition.x + renderOffset.x, -mapPosition.y - renderOffset.y, 0);
            if (screenCentered && Camera.main)
            {
                var camera = Camera.main.transform.position;
                transform.position = new Vector3(camera.x - Animation.footWidth / 2f, camera.y + Animation.footHeight / 2f, 0);
            }
            var pieces = CurrentFrame.pieces;
            if (renderers.Count < pieces.Length) throw new System.InvalidOperationException("Prefab has too few frame pieces: " + name);
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                renderer.enabled = !hidden && !presentationHidden && i < pieces.Length;
                if (!renderer.enabled) continue;
                var piece = pieces[i];
                if (Definition.type == 0 && piece.image == 4 && !WeaponVisible) { renderer.enabled = false; continue; }
                if (piece.image < 0)
                {
                    if (!solidSprite)
                    {
                        solidSprite = OriginalAssets.Catalog ? OriginalAssets.Catalog.solid : null;
                        if (!solidSprite) throw new System.InvalidOperationException("Missing baked solid sprite");
                    }
                    renderer.sprite = solidSprite;
                    renderer.flipX = renderer.flipY = false;
                    renderer.transform.localRotation = Quaternion.identity;
                    renderer.color = new Color(((piece.color >> 16) & 255) / 255f, ((piece.color >> 8) & 255) / 255f, (piece.color & 255) / 255f);
                    renderer.transform.localScale = new Vector3(piece.w, piece.h, 1);
                    renderer.transform.localPosition = new Vector3(piece.x + piece.w / 2f, -piece.y - piece.h / 2f, 0);
                    renderer.sortingOrder = i;
                    continue;
                }
                if (piece.image >= images.Length || piece.rect < 0 ||
                    piece.rect >= Animation.sheets[piece.image].rects.Length)
                {
                    renderer.enabled = false;
                    continue;
                }
                var rect = Animation.sheets[piece.image].rects[piece.rect];
                if (rect.w <= 0 || rect.h <= 0) { renderer.enabled = false; continue; }
                renderer.color = Color.white;
                renderer.transform.localScale = Vector3.one;
                renderer.sprite = OriginalAssets.Sprite(images[piece.image], rect);
                int mode = piece.transform;
                float x = piece.x;
                if (mirror && !suppressMirror) { x = -x - rect.w + Animation.footWidth; mode = MirrorTransform(mode); }
                bool rotated = mode >= 4;
                float w = rotated ? rect.h : rect.w, h = rotated ? rect.w : rect.h;
                renderer.transform.localPosition = new Vector3(x + w * .5f, -piece.y - h * .5f, 0);
                ApplyTransform(renderer, mode);
                renderer.sortingOrder = i;
            }
        }

        public static int MirrorTransform(int mode)
        {
            int[] mapping = { 2, 3, 0, 1, 6, 7, 4, 5 };
            return mapping[mode & 7];
        }

        public static void ApplyTransform(SpriteRenderer renderer, int mode)
        {
            renderer.flipX = mode == 2 || mode == 4 || mode == 7;
            renderer.flipY = mode == 1;
            float angle = mode == 3 ? 180 : mode == 4 || mode == 6 ? 90 : mode == 5 || mode == 7 ? -90 : 0;
            renderer.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
