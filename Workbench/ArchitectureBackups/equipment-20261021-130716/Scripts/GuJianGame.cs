using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame : MonoBehaviour
    {
        public string initialScript = "start";
        public bool startAtTitle = true;
        public const float ViewWidth = 533, ViewHeight = 320;
        public OriginalScriptMachine Machine { get; private set; }
        public OriginalMap Map { get; private set; }
        public string StageName { get; private set; }
        public string MissionTitle { get; private set; }
        public string MissionInfo { get; private set; }
        public string LastError { get; private set; }
        public IReadOnlyDictionary<int, OriginalActor> Actors => actors;
        public bool DialogueVisible => dialogue != null;
        public string[] DialogueChoices => options == null ? null : (string[])options.Clone();
        public int TutorialKey => tutorialKey;
        public bool AtTitle { get; private set; }
        public int LastChoice { get; private set; }
        public int money, level = 1, experience, equipmentId, durability = 50;
        public bool allStagesUnlocked = false;
        public string saveFileName = "gujian-save.json";
        private string currentStageScript;
        private SaveData pendingRestore;
        private readonly Queue<string> dialoguePages = new Queue<string>();
        private int tutorialKey = -1;
        private bool wrongTutorialKey;
        private Camera gameCamera;
        [SerializeField] private GameObject mapRoot, actorRoot;
        private GameObject backdrop;
        private readonly Dictionary<int, OriginalActor> pendingFixedActors = new Dictionary<int, OriginalActor>();
        private SpriteDefinition[] definitions;
        private readonly Dictionary<int, OriginalActor> actors = new Dictionary<int, OriginalActor>();
        private readonly Dictionary<Vector2Int, string> transitions = new Dictionary<Vector2Int, string>();
        public ItemInventory Inventory { get; } = new ItemInventory();
        public ItemTrading ItemTrading { get; private set; }
        private readonly List<EquipmentInstance> equipment = new List<EquipmentInstance>();
        private readonly List<Move> movements = new List<Move>();
        private readonly List<PendingTask> tasks = new List<PendingTask>();
        private Vector2Int lastCell = new Vector2Int(-1, -1);
        private string dialogue, speaker, portrait, toast;
        private string[] options;
        private float waitUntil, toastUntil;
        private bool storyScreen, lockInput, fixedCamera, hideUi, menu;
        private bool loadingVisible, loadingFinished, loadingOtherState;
        private int loadingFrame, loadingFinishCycles;
        private float loadingClock;
        private OriginalAnimation loadingAnimation;
        private Texture2D[] loadingImages;
        private GUIStyle loadingStyle;
        private static readonly string[] LoadingTips = {
            "普通攻击中按‘后’可使用后跳回避躲开攻击", "强化武器可以使游戏更轻松！",
            "在展云飞那里可以领取各种支线任务，可能获得珍贵道具哦", "当敌人防御时，对他使用崩击可以破除防御",
            "去藏剑山庄找古长老可以进入练功场练级，获得双倍经验", "去藏剑山庄找古长老可以进入剑冢打宝，获得强力武器",
            "铁匠出售的武器数量与主角等级有关，等级越高出售武器越多", "某些武器的技能是随机的，每次获得都不一样",
            "武器每强化一次，喂给剑灵的经验加1倍", "武器强化到+9喂给剑灵可获12倍经验",
            "命中与回避比攻击和防御更重要", "等级比敌人低时，命中和防御降低",
            "练功场最高可享受8倍经验哦！"
        };
        private string loadingTip;
        private Vector2 cameraPoint = new Vector2(ViewWidth / 2, ViewHeight / 2);
        private Font font;
        private GUIStyle bodyStyle, titleStyle, buttonStyle, dialogueStyle;
        private Vector2 menuScroll;

        private sealed class Move { public OriginalActor actor; public Vector2 target; public float speed; }
        [Serializable] private sealed class PendingTask { public int[] stages, enemyIds, counts; public string script; }
        [Serializable] private sealed class SaveData
        {
            public int version = 3;
            public int hp, energy, spiritLevel, spiritExperience, weaponLevel;
            public bool integrated;
            public bool weaponStowed;
            public string missionTitle, missionInfo, branchTitle, branchInfo;
            public int[] mainRewards, branchRewards;
            public PendingTask[] tasks;
            public string direction;
            public string[] keys;
            public int[] values, itemIds, itemCounts, permanentBonuses, spiritBonuses, specialItemPurchases;
            public EquipmentInstance[] equipment;
            public int equippedIndex, spiritDurability, spiritSkill, spiritSkillLevel;
            public int skillCooldownTicks, superArmorTicks, superArmorElapsed;
            public SpiritFeedState spiritFeed;
            public string timerScript;
            public float timerSeconds, spiritElapsed;
            public bool timerPaused;
            public int spiritDurabilityCounter;
            public bool[] openAreas;
            public int missionStageIndex, areaIndex;
            public int autoRecoverPercent = 70;
            public int money, level, experience, equipped, durability;
            public int smsGiftTier;
            public float x, y;
            public string stage, recordName;
        }

        private void Awake()
        {
            Application.runInBackground = true;
            gameCamera = Camera.main;
            if (!gameCamera) throw new InvalidOperationException("Startup scene requires its saved Main Camera");
            gameCamera.tag = "MainCamera";
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = ViewHeight / 2;
            gameCamera.backgroundColor = Color.black;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            ApplySoundVolume();
            definitions = OriginalAssets.Table<SpriteDefinition>("sprite");
            ItemTrading = new ItemTrading(Inventory, OriginalAssets.Table<ItemDefinition>("item"));
            if (!actorRoot || !mapRoot) throw new InvalidOperationException("Startup scene requires saved Actors and Map roots");
            Machine = new OriginalScriptMachine
            {
                LoadScript = OriginalAssets.Script,
                Command = HandleCommand,
                IsWaiting = () => loadingVisible && loadingFinished || originalBlackScriptPaused || (dialogue != null && (!originalMessage || originalMessageBlocksScript)) || Time.time < waitUntil || movements.Count > 0 || (menu && !scriptedMenu) || tutorialKey >= 0 || criticalWait || cameraWait,
            };
            Machine.ScriptStarted = OnScriptStarted;
            AtTitle = startAtTitle;
            if (!AtTitle) Begin();
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
            BeginPlayerValidation();
#endif
        }

        private void OnEnable()
        {
            // A play-mode script reload clears the nonserialized VM and actor registries.
            // Reinitialize at the configured entry screen, clearing the orphaned instances first.
            if (!Application.isPlaying || Machine != null) return;
            if (actorRoot) foreach (Transform child in actorRoot.transform) Destroy(child.gameObject);
            if (mapRoot) foreach (Transform child in mapRoot.transform) Destroy(child.gameObject);
            LastError = null;
            Awake();
        }

        public void Begin()
        {
            OriginalAssets.ClearCache();
            ResetCriticalSequences();
            reinforceOpen = reinforceConfirm = false;
            AtTitle = false;
            pendingRestore = null;
            ClearStage();
            foreach (var a in actors.Values.ToArray()) Destroy(a.gameObject);
            actors.Clear(); fighters.Clear();
            ResetComboStatistics(); breakCooldownTicks = 0; combatPauseRemaining = 0;
            ResetHudFeedback();
            ResetStageInfo();
            ResetCombatFeedback();
            ResetSwordBreak(); repairCost = 0; characterDialogue = false; characterLines = null; characterStartLine = 0;
            Machine.Clear();
            Machine.Values.Clear();
            money = experience = equipmentId = 0;
            level = 1; durability = 50;
            Inventory.Clear(); equipment.Clear(); equipment.Add(EquipmentInstance.Create(0)); equippedIndex = 0;
            permanentBonuses = new int[6]; spiritBonuses = new int[7]; spiritDurability = 0; spiritSkill = 13; spiritSkillLevel = 1; Array.Clear(openAreas, 0, openAreas.Length); tasks.Clear();
            ResetOriginalMessage();
            dialogue = null; dialoguePages.Clear(); options = null; LastError = null; menu = worldMapOpen = scriptedMenu = false;
            lockInput = fixedCamera = hideUi = integrated = experienceRoom = infiniteDurability = maximumPower = bloodProtection = disableCooldown = false;
            dropItems = true; waitUntil = 0; ResetSupportSkills(); timerScript = null; tutorialKey = -1;
            energy = spiritExperience = 0; weaponLevel = 1; equippedSkill = -2;
            dropNotices.Clear();
            numbers.Clear(); aiElapsed = 0; faces.Clear(); guide = skillFront = skillBack = null; loadedSkill = -1; guideTarget = -3;
            CancelTrade(); CancelItemConfirmation(); CancelAutoMedicine(); autoRecoverPercent = 70; ItemTrading.RestorePurchaseCounts(null); shopSelling = false;
            ResetOriginalSystemMenu(); activeRecordIndex = 0;
            LoadSmsGlobalState();
            RestoreSmsGiftTier(LoadSmsGiftTier(activeRecordIndex));
            ResetRoleSkillPreview();
            ResetSpiritMenu(); spiritFeedResult = null; spiritFeed = null; spiritElapsed = 0; spiritDurabilityCounter = 0;
            MissionTitle = MissionInfo = branchMissionTitle = branchMissionInfo = null;
            mainMissionRewards = EmptyMissionRewards(); branchMissionRewards = EmptyMissionRewards();
            cameraPoint = new Vector2(ViewWidth / 2, ViewHeight / 2);
            touchHeld.Clear(); previousTouchHeld.Clear(); touchMovement = Vector2.zero;
            introductory = false; tutorialKeys.Clear(); ResetCameraEffects();
            AddActor(-1, 200, Vector2.zero, 2);
            Machine.Enqueue(initialScript);
        }

        private void Update()
        {
            TickOriginalUiClock(Time.deltaTime);
            if (!swordBreakVisible || !SwordBreakPausesScene) TickCriticalHitEffect(Time.deltaTime);
            if (AtTitle) return;
            if (loadingVisible)
            {
                TickLoading(Time.unscaledDeltaTime);
                TickScriptMachine();
                return;
            }
            if (originalBlackScreen)
            {
                PollOriginalBlackScreenKeys();
                TickOriginalBlackScreen(Time.deltaTime);
                TickScriptMachine();
                return;
            }
            if (combatPauseRemaining > 0) { combatPauseRemaining = Mathf.Max(0, combatPauseRemaining - Time.deltaTime); return; }
            bool feedingInput = SpiritFeedAnimating;
            TickSpiritFeedAnimation(Time.unscaledDeltaTime);
            TickOriginalSystemMenu(Time.unscaledDeltaTime);
            PollTouchControls();
            bool reinforcementInput = reinforceOpen;
            TickOriginalMenuInput();
            PollSceneKeys(feedingInput, WeaponPreviewVisible, reinforcementInput);
            TickOriginalMessage(Time.deltaTime);
            TickOriginalChoice(Time.deltaTime);
            // GameCanvas.run continues/starts scripts outside GameCanvas.cycle.
            // A break may suspend scene12, but must not add a VM wait condition.
            bool scriptTicked = swordBreakVisible;
            if (scriptTicked) { TickScriptMachine(); if (AtTitle) return; }
            bool breakingScene = swordBreakVisible && SwordBreakPausesScene;
            float delta = TickSwordBreak(Time.deltaTime);
            if (breakingScene && delta <= 0)
            {
                if (modalInputFrame != Time.frameCount && !WeaponPreviewVisible && !Machine.Running && !DialogueVisible && movements.Count == 0 && !menu && !reinforceOpen && !lockInput && !CriticalActive)
                    HandleSceneInput(0);
                return;
            }
            TickHealthDecrement(delta);
            bool previewInput = WeaponPreviewVisible;
            if (previewInput) TickWeaponPreview(Time.unscaledDeltaTime);
            TickDrops(delta);
            foreach (var actor in actors.Values.ToArray())
                if (!menu && !reinforceOpen && (!DialogueVisible || CriticalActive) && ShouldTickDuringCritical(actor)) actor.Tick(delta);
            TickCriticalProjectile(delta);
            TickProjectiles(delta);
            TickCombat(delta);
            TickSpirit(delta);
            TickAutoMedicine();
            if (!menu) { scriptedMenu = false; CancelSpiritFeed(); CancelTrade(); }
            if (reinforceOpen) TickReinforceInput(); else if (!previewInput) TickTutorial();
            if (!WeaponPreviewVisible) TickMovements(delta);
            if (!scriptTicked) TickScriptMachine();
            if (modalInputFrame != Time.frameCount && !previewInput && !Machine.Running && !DialogueVisible && movements.Count == 0 && !menu && !reinforceOpen && !lockInput && !CriticalActive) HandleSceneInput(delta);
            UpdateCameraForDelta(delta);
            TickVisuals(delta);
            TickCriticalVisuals();
            TickStageInfo(delta);
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
            TickPlayerValidation();
#endif
        }

        private void TickScriptMachine()
        {
            try { Machine.Tick(); }
            catch (Exception error) { LastError = error.Message; lockInput = true; Debug.LogException(error); }
        }

        private void OnScriptStarted(string name, bool isLoading)
        {
            if (isLoading) StartLoadingOverlay();
        }

        private void StartLoadingOverlay()
        {
            loadingAnimation = OriginalAssets.Json<OriginalAnimation>("Animations/action/z_loading");
            loadingImages = new[] {
                OriginalAssets.Texture("action/l_00"), OriginalAssets.Texture("action/l_01"),
                OriginalAssets.Texture("action/l_02"), OriginalAssets.Texture("action/l_03"),
                OriginalAssets.Texture("action/o_0" + UnityEngine.Random.Range(2, 6))
            };
            loadingVisible = true;
            loadingFinished = false;
            loadingOtherState = false;
            loadingFrame = 0;
            loadingFinishCycles = 2;
            loadingClock = 0;
            loadingTip = LoadingTips[Mathf.Abs((int)(Time.unscaledTime * 10f)) % LoadingTips.Length];
            lockInput = true;
        }

        private void TickLoading(float delta)
        {
            loadingClock += Mathf.Max(0, delta);
            while (loadingClock >= .08f)
            {
                loadingClock -= .08f;
                loadingFrame++;
                if (!loadingFinished) continue;
                loadingFinishCycles--;
                if (loadingFinishCycles <= 0)
                {
                    loadingVisible = false;
                    loadingFinished = false;
                    if (!loadingOtherState)
                    {
                        menu = false;
                        worldMapOpen = false;
                        miniMapOpen = false;
                        scriptedMenu = false;
                        reinforceOpen = false;
                        reinforceConfirm = false;
                        systemMenuPage = SystemMenuPage.None;
                        lockInput = false;
                    }
                }
            }
        }

        private void PollSceneKeys(bool feedingInput, bool previewInput, bool reinforcementInput)
        {
            if (PollOriginalMessageKeys() || PollOriginalChoiceKeys() || PollOriginalCharacterKeys()) return;
            if(Input.GetKeyDown(KeyCode.M)) { if(miniMapOpen) CloseMiniMap(); else OpenMiniMap(); }
            if (!feedingInput && !previewInput && Input.GetKeyDown(KeyCode.Escape) && !reinforcementInput && !worldMapOpen && dialogue == null && !Machine.Running && !CriticalActive)
            {
                if(miniMapOpen) CloseMiniMap();
                else if (TradeConfirming) CancelTrade();
                else if (SpiritFeedConfirming) CancelSpiritFeed();
                else if (menuInputFrame != Time.frameCount) { menu = !menu; menuPage = 0; }
            }
            if (!previewInput && dialogue != null && options == null && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))) ContinueDialogue();
        }

        private void ClearStage()
        {
            // GameCanvas.script_loadStart resets the training modifiers for every stage.
            experienceRoom = false;
            maximumPower = false;
            CloseMiniMap();
            ClearDrops();
            numbers.Clear();
            foreach(var fighter in fighters.Values) fighter.missTicks=0;
            if (Role) Role.SetWeaponVisible(true);
            CloseWeaponPreview(); CancelSupportCast(); supportRecoveryTicks = 0; ResetAttackChain();
            breakHit = false; ResetCameraEffects();
            CancelSpiritFeed(); CancelTrade(); CancelAutoMedicine();
            ClearProjectiles();
            ResetCriticalSequences();
            foreach (var actor in actors.Values.ToArray())
                if (actor.actorId >= 0) { actors.Remove(actor.actorId); fighters.Remove(actor.actorId); Destroy(actor.gameObject); }
            foreach (Transform child in mapRoot.transform) Destroy(child.gameObject);
            if (backdrop) Destroy(backdrop);
            transitions.Clear(); movements.Clear(); Map = null;
            pendingFixedActors.Clear();
            lastCell = new Vector2Int(-1, -1);
        }

        private OriginalActor AddActor(int id, int definitionId, Vector2 position, int layer)
        {
            var definition = definitions.FirstOrDefault(d => d.id == definitionId);
            if (definition == null) throw new InvalidOperationException("Unknown sprite " + definitionId);
            if (actors.TryGetValue(id, out var previous)) { ClearProjectiles(previous); Destroy(previous.gameObject); }
            OriginalActor actor;
            if (pendingFixedActors.TryGetValue(id, out actor) && actor.definitionId == definitionId)
            {
                pendingFixedActors.Remove(id);
                position = new Vector2(actor.transform.position.x, -actor.transform.position.y);
                layer = actor.layer;
                actor.gameObject.SetActive(true);
            }
            else
            {
                var entry = OriginalAssets.Catalog.actors.FirstOrDefault(e => e.id == definitionId);
                if (entry == null || !entry.prefab) throw new InvalidOperationException("Missing actor prefab " + definitionId);
                actor = Instantiate(entry.prefab, actorRoot.transform);
            }
            actor.Initialize(definition, id, position, layer);
            actors[id] = actor;
            RegisterFighter(actor);
            if (id == -1 || id == -2) actor.ActionFinished += OnItemActionFinished;
            return actor;
        }

        private OriginalActor Actor(int id) => actors.TryGetValue(id, out var actor) ? actor : null;
        private OriginalActor Role => Actor(-1);

        private void LoadMap(string name)
        {
            string id = Path.GetFileNameWithoutExtension(name);
            var entry = OriginalAssets.Catalog.maps.FirstOrDefault(e => e.id == id);
            if (entry == null) throw new InvalidOperationException("Missing map prefab " + id);
            Map = entry.data;
            var stage = OriginalAssets.Catalog.stages.FirstOrDefault(e => e.script == currentStageScript && e.prefab.mapId == id);
            if (stage != null)
            {
                var layout = Instantiate(stage.prefab, mapRoot.transform);
                foreach (var actor in layout.fixedActors)
                {
                    pendingFixedActors[actor.actorId] = actor;
                    actor.gameObject.SetActive(false);
                }
            }
            else Instantiate(entry.prefab, mapRoot.transform);
        }

        private GameObject ImageObject(string name, string path, Vector2 point, int order, Transform parent)
        {
            var prefab = Resources.Load<GameObject>("Baked/Images/" + path);
            if (!prefab) throw new InvalidOperationException("Missing image prefab " + path);
            var go = Instantiate(prefab, parent, false);
            go.name = name;
            go.transform.position = new Vector3(point.x, -point.y, 0);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = order;
            return go;
        }

        private void SetBackdrop(string path)
        {
            if (backdrop) Destroy(backdrop);
            var texture = OriginalAssets.Texture("pic/" + path);
            backdrop = ImageObject("Backdrop", "pic/" + path, new Vector2(texture.width / 2f, texture.height / 2f), -30000, mapRoot.transform);
        }

        private void UpdateCamera() { UpdateCameraForDelta(Time.deltaTime); }
        private void UpdateCameraForDelta(float delta)
        {
            TickCameraEffects(delta);
            float aspect = ViewWidth / ViewHeight, screenAspect = (float)Screen.width / Screen.height;
            gameCamera.rect = screenAspect > aspect ? new Rect((1 - aspect / screenAspect) / 2, 0, aspect / screenAspect, 1) :
                new Rect(0, (1 - screenAspect / aspect) / 2, 1, screenAspect / aspect);
            if (!fixedCamera && Role && Map != null)
                cameraPoint = new Vector2(Mathf.Clamp(Role.mapPosition.x, ViewWidth / 2, Mathf.Max(ViewWidth / 2, Map.Width - ViewWidth / 2)),
                    Mathf.Clamp(Role.mapPosition.y, ViewHeight / 2, Mathf.Max(ViewHeight / 2, Map.Height - ViewHeight / 2)));
            gameCamera.transform.position = new Vector3(cameraPoint.x + shakeOffset.x, -cameraPoint.y - shakeOffset.y, -10);
            if (backdrop)
            {
                var image = backdrop.GetComponent<SpriteRenderer>().sprite;
                backdrop.transform.position = new Vector3(cameraPoint.x - ViewWidth / 2 + image.rect.width / 2,
                    -cameraPoint.y + ViewHeight / 2 - image.rect.height / 2, 0);
            }
        }

        private static Vector2 Direction(string dir)
        {
            switch (dir) { case "r": return Vector2.right; case "u": return Vector2.down; case "d": return Vector2.up; default: return Vector2.left; }
        }
        private void MoveActor(OriginalActor actor, string dir, float speed, float distance)
        {
            if (!actor) return;
            actor.Face(dir); actor.Act(1 + actor.actionBank);
            movements.Add(new Move { actor = actor, target = actor.mapPosition + Direction(dir) * distance, speed = speed / .08f });
        }
        private void TickMovements(float delta)
        {
            for (int i = movements.Count - 1; i >= 0; i--)
            {
                var move = movements[i];
                if (!move.actor) { movements.RemoveAt(i); continue; }
                move.actor.mapPosition = Vector2.MoveTowards(move.actor.mapPosition, move.target, move.speed * delta);
                if (Vector2.Distance(move.actor.mapPosition, move.target) < .01f) { move.actor.Act(move.actor.actionBank); movements.RemoveAt(i); }
            }
        }

        private void HandleInput() { HandleSceneInput(Time.deltaTime); }
        private void HandleSceneInput(float delta)
        {
            if (!Role || Role.locked || Map == null || Role.hp <= 0) return;
            float horizontal = touchMovement.x + (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            if (TryBackstep(horizontal)) return;
            if (Input.GetKey(KeyCode.J) || touchHeld.Contains(4)) PrimaryControl();
            if (Input.GetKeyDown(KeyCode.K) && equippedSkill >= 5) UseSkill(equippedSkill, integrated ? spiritSkillLevel : Equipped.skillLevel);
            if (Input.GetKeyDown(KeyCode.L)) UseSkill(4);
            if (Input.GetKeyDown(KeyCode.I)) ToggleIntegrate();
            if (Input.GetKeyDown(KeyCode.O)) TryCriticalKill();
            if (Role.LogicalAction > 1) return;
            Vector2 movement = new Vector2((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) -
                (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
                (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0) -
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0));
            movement += touchMovement;
            if (movement.sqrMagnitude > 0)
            {
                Role.Face(movement.x != 0 ? movement.x > 0 ? "r" : "l" : movement.y > 0 ? "d" : "u");
                Role.Act(1 + Role.actionBank);
                Vector2 next = Role.mapPosition + movement.normalized * (6 / .08f) * delta;
                if (Map.CanPass(next.x + Role.Animation.footWidth / 2f, Role.mapPosition.y)) Role.mapPosition = new Vector2(next.x, Role.mapPosition.y);
                if (Map.CanPass(Role.mapPosition.x + Role.Animation.footWidth / 2f, next.y)) Role.mapPosition = new Vector2(Role.mapPosition.x, next.y);
            }
            else if (Role.LogicalAction == 1) Role.Act(Role.actionBank);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)) Interact();
            var cell = new Vector2Int((int)Role.mapPosition.x / Map.cellWidth, (int)Role.mapPosition.y / Map.cellHeight);
            if (cell != lastCell)
            {
                lastCell = cell;
                if (transitions.TryGetValue(cell, out var script)) Machine.Enqueue(script);
            }
        }

        private void Talk(string who, string text, string image = null, bool story = false, string[] choices = null, bool onLeft = false)
        {
            ResetOriginalMessage();
            speaker = who ?? ""; portrait = image; storyScreen = story; options = choices;
            portraitOnLeft = onLeft;
            characterDialogue = false;
            characterLines = null; characterStartLine = 0;
            dialoguePages.Clear();
            string cleaned = Clean(text);
            int perLine = story ? 27 : 30, lines = story ? 6 : 3;
            var page = new System.Text.StringBuilder();
            int column = 0, line = 0;
            foreach (char ch in cleaned)
            {
                if (ch == '\n' || ++column >= perLine) { column = 0; line++; }
                if (line >= lines && page.Length > 0) { dialoguePages.Enqueue(page.ToString()); page.Clear(); line = 0; }
                page.Append(ch);
            }
            if (page.Length > 0 || dialoguePages.Count == 0) dialoguePages.Enqueue(page.ToString());
            dialogue = dialoguePages.Dequeue();
        }
        public void ContinueDialogue(int choice = 0)
        {
            if (HandleOriginalBlackScreenControl(4) || HandleOriginalMessageControl(4)) return;
            if (characterDialogue) modalInputFrame = Time.frameCount;
            if (dialoguePages.Count > 0)
            {
                if (characterDialogue) characterStartLine += CharacterLinesPerPage;
                dialogue = dialoguePages.Dequeue(); return;
            }
            if (options != null) LastChoice = choice;
            dialogue = null; options = null; characterDialogue = false; characterLines = null;
        }
        private static string Clean(string text) => Regex.Replace(text ?? "", @"</?c(?:=[^>]*)?>", "");
        private void Toast(string text) { toast = Clean(text); toastUntil = Time.time + 3; }

        private object HandleCommand(int id, object[] args)
        {
            object A(int i) => i < args.Length ? args[i] : null;
            int N(int i) => OriginalScriptMachine.Int(A(i));
            bool B(int i) => OriginalScriptMachine.Bool(A(i));
            string S(int i) => Convert.ToString(A(i)) ?? "";
            OriginalActor ActorAt(int i) => Actor(N(i));
            switch (id)
            {
                case 0x20: Machine.Enqueue(S(0), B(1)); break;
                case 0x21: ClearStage(); StageName = S(0); currentStageScript = Machine.CurrentScript; break;
                case 0x22:
                    if (loadingVisible)
                    {
                        loadingFinished = true;
                        loadingOtherState = B(0);
                        ApplyPendingRestore();
                    }
                    else { lockInput = false; ApplyPendingRestore(); }
                    break;
                case 0x23: LoadMap(S(0)); break;
                case 0x24: transitions[new Vector2Int(N(0), N(1))] = S(2); break;
                case 0x25:
                    foreach (var key in transitions.Where(t => t.Value == S(0)).Select(t => t.Key).ToArray()) transitions.Remove(key);
                    break;
                case 0x26:
                    Role.mapPosition = new Vector2(N(0), N(1)); Role.Face(S(2)); Role.Act(Role.actionBank); break;
                case 0x27:
                    OpenOriginalCharacterDialogue(S(0), S(1), S(2), B(3)); break;
                case 0x28: ShowOriginalMessage(S(0), N(1), false); break;
                case 0x29: ShowOriginalMessage(S(0), 0, true); break;
                case 0x2A: PlayMusic(S(0)); break;
                case 0x2B: if (music) music.Stop(); break;
                case 0x2C:
                    var cell = Map.cells.First(c => c.x == N(0) && c.y == N(1)); if (N(2) == 0) cell.blocked = B(3); break;
                case 0x2D: OpenOriginalChoice(S(0), ((object[])A(1)).Select(CleanObject).ToArray(), B(2)); break;
                case 0x2E: return LastChoice;
                case 0x2F:
                    var famulus = Actor(-2) ?? AddActor(-2, 199, new Vector2(N(0), N(1)), 2);
                    famulus.mapPosition = new Vector2(N(0), N(1)); famulus.Face(S(2)); break;
                case 0x30:
                    var added = AddActor(N(0), N(1), new Vector2(N(3), N(4)), N(5)); if (!added.useLayoutTransform) added.Face(S(2)); break;
                case 0x31: if (ActorAt(0)) { ClearProjectiles(ActorAt(0)); Destroy(ActorAt(0).gameObject); actors.Remove(N(0)); fighters.Remove(N(0)); } break;
                case 0x32: ActorAt(0)?.Face(S(1)); break;
                case 0x33: MoveActor(Role, S(0), N(1), N(2)); break;
                case 0x34: MoveActor(ActorAt(0), S(1), N(2), N(3)); break;
                case 0x35:
                    foreach (var value in (object[])A(0)) MoveActor(Actor(OriginalScriptMachine.Int(value)), S(1), N(2), N(3)); break;
                case 0x36:
                    var ids = (object[])A(0); var dirs = (object[])A(1); var speeds = (object[])A(2); var distances = (object[])A(3);
                    for (int i = 0; i < ids.Length; i++) MoveActor(Actor(OriginalScriptMachine.Int(ids[i])), Convert.ToString(dirs[i]), OriginalScriptMachine.Int(speeds[i]), OriginalScriptMachine.Int(distances[i]));
                    break;
                case 0x37:
                    if (ActorAt(0)) { if (N(2) == 1) ActorAt(0).deathScript = S(1); else if (N(2) == 2) ActorAt(0).specialScript = S(1); else ActorAt(0).interactionScript = S(1); } break;
                case 0x38: if (ActorAt(0)) { if (N(1) == 1) ActorAt(0).deathScript = null; else if (N(1) == 2) ActorAt(0).specialScript = null; else ActorAt(0).interactionScript = null; } break;
                case 0x39: if (ActorAt(0)) ActorAt(0).hidden = B(1); break;
                case 0x3A:
                    if (N(0) == -1 && criticalStage != 0) EndRoleCritical();
                    if (ActorAt(0)) ActorAt(0).ScriptAct(N(1), N(2), B(3), B(4)); break;
                case 0x3B:
                    if (N(0) == -1 && criticalStage != 0) EndRoleCritical();
                    ActorAt(0)?.ResetAction(); break;
                case 0x3D: ShowFace(N(0), N(1), N(2)); break;
                // GameCanvas.script_setEnemyLevel only caps the requested level at 150.
                // It does not raise low values to 10; early-game scripts legitimately
                // pass the party level while the player is still below that threshold.
                case 0x3E: foreach (var a in actors.Values.Where(a => a.definitionId == N(0))) { a.level = Mathf.Min(N(1), 150); RefreshAttributes(a, true); } break;
                case 0x3F: if (ActorAt(0)) { ActorAt(0).locked = B(1); if (B(1)) ActorAt(0).ResetAction(); } break;
                case 0x40: MoveCameraRelative(N(0), N(1), N(2), B(3)); break;
                case 0x41: MoveCameraTo(cameraBack, N(0), B(1)); break;
                case 0x42: MoveCameraTo(new Vector2(N(0), N(1) - (ViewWidth - ViewHeight) / 2), N(2), B(3)); break;
                case 0x43: SetCameraPosition(new Vector2(N(0), N(1))); break;
                case 0x44: fixedCamera = B(0); break;
                case 0x45: SetShake(N(0), N(1)); break;
                case 0x46: SetShake(0, 0); break;
                case 0x49: Toast(S(0)); break;
                case 0x4A: return Inventory.Count(N(0));
                case 0x4B: Inventory.RemoveUpTo(N(0), N(1)); break;
                case 0x4C: ShowStageInfo(); break;
                case 0x4D: level = N(0); RefreshAttributes(Role, true); break;
                case 0x4E: experienceRoom = true; break;
                case 0x4F:
                    UseSkill(N(0), N(1), true); break;
                case 0x50: ScriptAddMoney(N(0), B(1)); break;
                case 0x51: ScriptAddItem(N(0), N(1), B(2)); break;
                case 0x52: dropItems = B(0); break;
                case 0x53: ScriptAddEquipment(N(0), N(1), B(2)); break;
                case 0x54: RemoveEquipment(N(0)); break;
                case 0x55:
                    Equip(N(0), true); break;
                case 0x56: Role.SetWeaponVisible(false); break;
                case 0x59:
                    tasks.Add(new PendingTask { stages = ToInts(A(0)), enemyIds = ToInts(A(1)), counts = ToInts(A(2)), script = S(3) }); break;
                case 0x5A: SetMission(S(0), S(1), ToInts(A(2)), B(3)); break;
                case 0x5B: case 0x5C: Toast(S(0)); break;
                case 0x5D: OpenOriginalBlackScreen(S(0), B(1), N(2), N(3), S(4), N(5)); break;
                case 0x5E: waitUntil = Time.time + N(0) * .08f; break;
                case 0x5F: waitUntil = Time.time + N(0) / 1000f; break;
                case 0x60: menu = false; break;
                case 0x61: hideUi = B(0); break;
                case 0x64:
                    // GameCanvas.script_addFamulus always constructs a fresh Famulus.  Do not
                    // silently retain the previous spirit's level, feed bonuses, skill or durability
                    // when a script re-adds the spirit (notably 204e1).
                    if (Actor(-2))
                    {
                        ClearProjectiles(Actor(-2));
                        Destroy(Actor(-2).gameObject);
                        actors.Remove(-2);
                        fighters.Remove(-2);
                    }
                    var addedSpirit = AddActor(-2, 199, Role.mapPosition, 2);
                    addedSpirit.level = Mathf.Clamp(N(0), 1, 40);
                    spiritExperience = 0;
                    spiritBonuses = new int[7];
                    spiritSkill = 13;
                    spiritSkillLevel = 1;
                    spiritDurability = 0;
                    UpdateSpiritEquipment();
                    break;
                case 0x65: lockInput = B(0); break;
                case 0x68: Save(true); break;
                case 0x69: AtTitle = true; break;
                case 0x6D: SetBackdrop(S(0)); break;
                case 0x6E: guideTarget = N(0); break;
                case 0x72:
                    // GameCanvas.script_removeFamulus frees the spirit sprite and clears the
                    // role's famulus reference.  Keep every Unity-side registration in sync:
                    // projectiles owned by the spirit, combat callbacks, and scripted movement
                    // must not survive the actor removal.
                    var removedSpirit = Actor(-2);
                    if (removedSpirit)
                    {
                        ClearProjectiles(removedSpirit);
                        movements.RemoveAll(move => move.actor == removedSpirit);
                        fighters.Remove(-2);
                        actors.Remove(-2);
                        Destroy(removedSpirit.gameObject);
                    }
                    break;
                case 0x6F: return EnemiesAlive == 0;
                case 0x77: return level;
                case 0x78:
                    if (N(0) == 1) ShowOriginalMessage("升级过慢，可到商城购买多倍经验。", 0, false);
                    else if (N(0) == 3 && !allStagesUnlocked) ShowOriginalMessage("该章节尚未开放!", 0, false);
                    break;
                case 0x79: return allStagesUnlocked;
                case 0x7A: LoadSkillVisual(N(0)); break;
                case 0x7C: SetDurabilityNonLoss(B(0)); break;
                case 0x87: RepairOrdinarySwords(); break;
                case 0x88:
                    Role.hp = Role.maxHp;
                    if (fighters.TryGetValue(-1, out var healedFighter) && healedFighter.dead)
                    { healedFighter.dead = false; Role.ResetAction(); }
                    break;
                case 0x90: return Role.hp < Role.maxHp;
                case 0x91: return integrated;
                case 0x92: ScriptAddExperience(N(0), B(1)); break;
                case 0x93: SetScriptedSpiritLevel(N(0)); break;
                case 0x95: missionStageIndex = N(0); break;
                case 0x96: SetAreaIndex(N(0)); break;
                case 0x99: return Actor(-2)?.level ?? 1;
                case 0x9A: introductory = B(0); break;
                case 0x9B: return money;
                case 0x9C: bloodProtection = B(0); break;
                case 0x9D: disableCooldown = B(0); break;
                default: return HandleAdditionalCommand(id, args);
            }
            return 0;
        }

        private static int[] ToInts(object value) => ((object[])value).Select(OriginalScriptMachine.Int).ToArray();
        private static string CleanObject(object value) => Clean(Convert.ToString(value));

        // GameCanvas.script_setDurabilityNonLoss repairs the active weapon on
        // every call, including false and the parameterless reset.
        private void SetDurabilityNonLoss(bool enabled)
        {
            infiniteDurability = enabled;
            if (integrated)
            {
                spiritDurability = SpiritAttribute(1);
                durability = spiritDurability;
            }
            else if (Equipped != null)
            {
                durability = Equipped.MaximumDurability;
                Equipped.durability = durability;
            }
        }

        private string SavePath => RecordPath(activeRecordIndex);
        public void Save(bool scriptCheckpoint = false)
        {
            if (!Role || (!scriptCheckpoint && (Machine.Running || DialogueVisible))) { Toast("请在剧情结束后保存"); return; }
            if (Equipped != null && !integrated) Equipped.durability = durability;
            if (integrated) spiritDurability = durability;
            var items = Inventory.Capture();
            var data = new SaveData { hp = Role.hp, energy = energy, spiritLevel = Actor(-2)?.level ?? 0, spiritExperience = spiritExperience, weaponLevel = weaponLevel, integrated = integrated, direction = Role.direction, tasks = tasks.ToArray(), missionTitle = MissionTitle, missionInfo = MissionInfo, branchTitle = branchMissionTitle, branchInfo = branchMissionInfo, keys = Machine.Values.Keys.ToArray(), values = Machine.Values.Values.ToArray(),
                spiritFeed = spiritFeed, timerScript = timerScript, timerSeconds = timerSeconds, timerPaused = timerPaused, spiritElapsed = spiritElapsed, spiritDurabilityCounter = spiritDurabilityCounter,
                mainRewards = mainMissionRewards, branchRewards = branchMissionRewards, weaponStowed = !Role.WeaponVisible,
                money = money, level = level, experience = experience, equipped = equipmentId, durability = durability,
                smsGiftTier = smsGiftTier,
                itemIds = items.Select(p => p.Key).ToArray(), itemCounts = items.Select(p => p.Value).ToArray(), equipment = equipment.ToArray(), equippedIndex = equippedIndex, permanentBonuses = permanentBonuses, spiritBonuses = spiritBonuses, specialItemPurchases = ItemTrading.CapturePurchaseCounts(), autoRecoverPercent = autoRecoverPercent, skillCooldownTicks = skillCooldownTicks, superArmorTicks = superArmorTicks, superArmorElapsed = superArmorElapsed, openAreas = openAreas, spiritDurability = spiritDurability, spiritSkill = spiritSkill, spiritSkillLevel = spiritSkillLevel, missionStageIndex = missionStageIndex, areaIndex = areaIndex,
                x = Role.mapPosition.x, y = Role.mapPosition.y, recordName = StageName, stage = currentStageScript ?? "100" };
            string temporary = SavePath + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
            if (File.Exists(SavePath)) File.Replace(temporary, SavePath, SavePath + ".bak");
            else File.Move(temporary, SavePath);
            Toast("进度已保存");
        }

        public void Load()
        {
            if (!File.Exists(SavePath)) return;
            int slot = activeRecordIndex;
            var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            if (data == null || data.version != 3 || data.keys == null || data.values == null || data.keys.Length != data.values.Length || data.itemIds == null || data.itemCounts == null || data.itemIds.Length != data.itemCounts.Length || data.itemCounts.Any(n => n < 0) || string.IsNullOrEmpty(data.stage)) throw new InvalidDataException("存档格式不完整");
            ImportLegacySmsState(File.ReadAllText(SavePath));
            Begin(); activeRecordIndex = slot; Machine.Clear();
            for (int i = 0; i < data.keys.Length; i++) Machine.Values[data.keys[i]] = data.values[i];
            money = data.money; level = data.level; experience = data.experience; equipmentId = data.equipped; durability = data.durability;
            RestoreSmsGiftTier(data.smsGiftTier);
            PersistSmsGiftTier(slot, smsGiftTier);
            Inventory.Restore(data.itemIds, data.itemCounts);
            equipment.Clear(); equipment.AddRange(data.equipment);
            pendingRestore = data;
            Machine.Enqueue(data.stage);
        }
        private void ApplyPendingRestore()
        {
            if (pendingRestore == null) return;
            var data = pendingRestore; pendingRestore = null;
            Role.mapPosition = new Vector2(data.x, data.y);
            Role.Face(data.direction ?? "l");
            int restoredIndex = Mathf.Clamp(data.equippedIndex, 0, equipment.Count - 1);
            equippedIndex = -1;
            SelectEquipment(restoredIndex);
            permanentBonuses = data.permanentBonuses ?? new int[6]; spiritBonuses = data.spiritBonuses ?? new int[7];
            autoRecoverPercent = data.autoRecoverPercent >= 10 ? Mathf.Clamp(data.autoRecoverPercent, 10, 90) : 70;
            ItemTrading.RestorePurchaseCounts(data.specialItemPurchases);
            spiritDurability = data.spiritDurability; spiritSkill = data.spiritSkill; spiritSkillLevel = data.spiritSkillLevel;
            missionStageIndex = data.missionStageIndex; areaIndex = data.areaIndex;
            if (data.openAreas != null) Array.Copy(data.openAreas, openAreas, Mathf.Min(13, data.openAreas.Length));
            SetAreaIndex(data.areaIndex);
            weaponLevel = data.weaponLevel;
            durability = data.durability;
            if (smsUnlimitedDurability) infiniteDurability = true;
            RefreshAttributes(Role, false);
            Role.hp = Mathf.Clamp(data.hp, 0, Role.maxHp);
            energy = data.energy; spiritExperience = data.spiritExperience;
            tasks.Clear(); if (data.tasks != null) tasks.AddRange(data.tasks);
            MissionTitle = data.missionTitle; MissionInfo = data.missionInfo;
            branchMissionTitle = data.branchTitle; branchMissionInfo = data.branchInfo;
            mainMissionRewards = NormalizeMissionRewards(data.mainRewards); branchMissionRewards = NormalizeMissionRewards(data.branchRewards);
            if (data.spiritLevel > 0) { var spirit = Actor(-2) ?? AddActor(-2, 199, Role.mapPosition, 2); spirit.level = data.spiritLevel; }
            integrated = data.integrated;
            Role.actionBank = integrated ? 27 : 0; Role.ResetAction();
            if (Actor(-2)) Actor(-2).hidden = integrated;
            UpdateSpiritEquipment();
            Role.SetWeaponVisible(!data.weaponStowed);
            spiritFeed = data.spiritFeed;
            if (spiritFeed != null && (spiritFeed.before == null || spiritFeed.before.Length != 7 || spiritFeed.remaining < 0 || spiritFeed.remaining > 1000000 || float.IsNaN(spiritFeed.elapsed) || float.IsInfinity(spiritFeed.elapsed))) spiritFeed = null;
            if (SpiritFeedAnimating) { menu = true; menuPage = 6; }
            timerScript = string.IsNullOrEmpty(data.timerScript) ? null : data.timerScript;
            timerSeconds = float.IsNaN(data.timerSeconds) || float.IsInfinity(data.timerSeconds) ? 0 : Mathf.Max(0, data.timerSeconds);
            timerPaused = data.timerPaused;
            spiritElapsed = Mathf.Clamp(data.spiritElapsed, 0, .08f);
            spiritDurabilityCounter = Mathf.Clamp(data.spiritDurabilityCounter, 0, 10);
            breakCooldownTicks = 3;
            skillCooldownTicks = Mathf.Max(0, data.skillCooldownTicks); superArmorTicks = Mathf.Max(0, data.superArmorTicks); superArmorElapsed = Mathf.Max(0, data.superArmorElapsed);
            RefreshAttributes(Role, false);
            Role.hp = Mathf.Clamp(data.hp, 0, Role.maxHp);
        }

        private void EnsureStyles()
        {
            if (bodyStyle != null) return;
            font = Font.CreateDynamicFontFromOSFont(new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 16);
            bodyStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 15, wordWrap = true, richText = false };
            dialogueStyle = new GUIStyle(bodyStyle);
            dialogueStyle.normal.textColor = new Color(.19f,.08f,.035f);
            titleStyle = new GUIStyle(bodyStyle) { fontSize = 19, alignment = TextAnchor.MiddleCenter };
            buttonStyle = new GUIStyle(GUI.skin.button) { font = font, fontSize = 15 };
            loadingStyle = new GUIStyle(bodyStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 13, normal = { textColor = Color.white } };
        }

        private void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Min(Screen.width / ViewWidth, Screen.height / ViewHeight);
            var savedMatrix=GUI.matrix; var savedColor=GUI.color;
            float offsetX=(Screen.width-ViewWidth*scale)/2, offsetY=(Screen.height-ViewHeight*scale)/2;
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - ViewWidth * scale) / 2, (Screen.height - ViewHeight * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
            try
            {
            if (AtTitle)
            {
                GUI.DrawTexture(new Rect(0, 0, ViewWidth, ViewHeight), OriginalAssets.Texture("pic/t_00"), ScaleMode.ScaleAndCrop);
                if (GUI.Button(new Rect(190, 185, 153, 32), "开始游戏", buttonStyle)) Begin();
                GUI.enabled = File.Exists(SavePath);
                if (GUI.Button(new Rect(190, 228, 153, 32), "继续游戏", buttonStyle)) Load();
                GUI.enabled = true;
                return;
            }
            if (loadingVisible) { DrawLoading(); return; }
            if (originalBlackScreen) { DrawOriginalBlackScreen(); return; }
            if(miniMapOpen) { DrawMiniMap(); DrawTouchControls(); return; }
            if (storyScreen && dialogue != null) { var old = GUI.color; GUI.color = Color.black; GUI.DrawTexture(new Rect(0, 0, ViewWidth, ViewHeight), Texture2D.whiteTexture); GUI.color = old; }
            if (!menu && !reinforceOpen) DrawDropNotices();
            if (!hideUi && dialogue == null && !reinforceOpen && !menu)
            {
                DrawOriginalHud();
                if (RemainingTimeText != null) GUI.Label(new Rect(9,55,180,22), RemainingTimeText, bodyStyle);
            }
            if (!(storyScreen && dialogue != null) && !worldMapOpen)
            {
                DrawCriticalHitEffect();
                DrawStageInfo();
            }
            if (Time.time < toastUntil && dialogue == null)
            {
                GUI.Box(new Rect(65, 145, 403, 42), "");
                GUI.Label(new Rect(75, 153, 383, 32), toast, bodyStyle);
            }
            // A menu is a modal atlas layer.  Dialogue state can remain queued while
            // a validation fixture or a script transition opens the menu, but the
            // original GameCanvas does not paint the dialogue panel underneath it.
            if (!menu && characterDialogue && dialogue != null) DrawOriginalCharacterDialogue();
            else if (!menu && !originalMessage && dialogue != null && options == null)
            {
                var bounds = storyScreen ? new Rect(35, 85, 463, 160) : DrawOriginalDialogue();
                if (storyScreen) GUI.Box(bounds, "");
                if (!string.IsNullOrEmpty(speaker)) GUI.Label(new Rect(!storyScreen && portraitOnLeft ? bounds.xMax-145 : bounds.x+42, bounds.y+6,130,25),speaker,bodyStyle);
                GUI.Label(new Rect(bounds.x + 14, bounds.y + (speaker.Length > 0 ? 32 : 10), bounds.width - 28, bounds.height - 35), dialogue, storyScreen ? bodyStyle : dialogueStyle);
                if (options == null)
                {
                    if (GUI.Button(new Rect(445, 286, 66, 27), "继续", buttonStyle)) ContinueDialogue();
                }
            }
            if (menu && options == null) DrawMenu();
            if (reinforceOpen && options == null) DrawReinforce();
            if (!menu && !originalMessage && dialogue != null && options != null) DrawOriginalChoiceOptions();
            if (!menu && originalMessage && dialogue != null) DrawOriginalMessage();
            if (LastError != null)
            {
                GUI.Box(new Rect(30, 70, 473, 180), "");
                GUI.Label(new Rect(45, 80, 443, 115), "运行中断：" + LastError, bodyStyle);
                if (GUI.Button(new Rect(180, 205, 173, 30), "返回标题", buttonStyle)) { AtTitle = true; LastError = null; }
            }
            DrawTouchControls();
            DrawTutorial();
            }
            finally
            {
                // Frame drawing changes GUI.matrix per piece; cover the bars in screen space last.
                GUI.matrix=Matrix4x4.identity;GUI.color=Color.black;
                if(offsetX>0)
                {
                    GUI.DrawTexture(new Rect(0,0,offsetX,Screen.height),Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(Screen.width-offsetX,0,offsetX,Screen.height),Texture2D.whiteTexture);
                }
                if(offsetY>0)
                {
                    GUI.DrawTexture(new Rect(0,0,Screen.width,offsetY),Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(0,Screen.height-offsetY,Screen.width,offsetY),Texture2D.whiteTexture);
                }
                GUI.matrix=savedMatrix;GUI.color=savedColor;
            }
        }

        private void DrawLoading()
        {
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, ViewWidth, ViewHeight), Texture2D.whiteTexture);
            GUI.color = Color.white;
            if (loadingAnimation != null && loadingImages != null)
                OriginalGui.Animation(loadingAnimation, loadingImages, 0, loadingFrame, new Vector2(ViewWidth / 2, ViewHeight / 2 + 40));
            if (!string.IsNullOrEmpty(loadingTip))
                GUI.Label(new Rect(15, 18, ViewWidth - 30, 70), loadingTip, loadingStyle);
        }
    }
}
