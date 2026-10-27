using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ContentConfiguration
    {
        public static T Clone<T>(T template) => JsonUtility.FromJson<T>(JsonUtility.ToJson(template));
        public static void Configure(EquipmentDefinition row)
        { row.canReinforce = row.ReinforceAllowed; row.maxReinforcement = row.ReinforcementLimit; row.customRules = true; }
        public static void Configure(ItemDefinition row)
        {
            if (row.customRules) return;
            row.effectKind = row.EffectKind; row.consumeWholeStack = row.ConsumeWholeStack;
            row.immediateUseInShop = row.ImmediateUseInShop; row.specialPurchaseGroup = row.PurchaseGroup;
            row.maxPurchaseQuantity = row.id >= 16 ? 1 : 99;
            row.shopMask = row.id != 14 && row.id != 15 ? 3 : 0;
            var fields = (row.effect ?? "").Split(',');
            if (fields.Length > 1) int.TryParse(fields[1], out row.effectValue);
            if (row.effectKind == ItemEffectKind.AttributeBonus) row.attributeIndex = row.id - 17;
            row.customRules = true;
        }
        private static void RequireEditMode()
        { if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("退出播放模式后编辑配置"); }
        public static int Duplicate(OriginalDatabase db, string category, int templateId, int newId)
        {
            RequireEditMode(); if (newId < 0) throw new ArgumentException("编号不能小于0");
            if (category == "items" && File.Exists("Assets/Resources/Baked/Drops/Items/" + newId + ".prefab") ||
                category == "enemies" && File.Exists("Assets/Resources/Baked/Actors/" + newId + ".prefab"))
                throw new InvalidOperationException("目标编号已有Prefab，请选择其他编号");
            Undo.RecordObject(db, "复制内容配置");
            switch (category)
            {
                case "equipment":
                    if (db.Get<EquipmentDefinition>().Any(r => r.id == newId)) throw new ArgumentException("武器编号重复");
                    var weapon = Clone(db.Get<EquipmentDefinition>().First(r => r.id == templateId)); Configure(weapon);
                    weapon.id = newId; weapon.name += "（副本）"; db.addedEquipment = (db.addedEquipment ?? new EquipmentDefinition[0]).Concat(new[] { weapon }).ToArray(); break;
                case "items":
                    if (db.Get<ItemDefinition>().Any(r => r.id == newId)) throw new ArgumentException("道具编号重复");
                    var item = Clone(db.Get<ItemDefinition>().First(r => r.id == templateId)); Configure(item);
                    var sourceDrop = "Assets/Resources/Baked/Drops/Items/" + templateId + ".prefab";
                    if (!File.Exists(sourceDrop)) throw new InvalidOperationException("模板缺少已保存道具掉落Prefab");
                    if (!item.iconSprite)
                    {
                        var ui = Resources.Load<OriginalUiLayout>("OriginalUiLayout");
                        int icon = templateId == 23 ? 8 : templateId;
                        item.iconSprite = Sprite.Create(ui.itemIcons, new Rect(icon * 22, 0, 22, 22), new Vector2(.5f,.5f), 1);
                        item.iconSprite.name = "ItemIcon_" + newId; AssetDatabase.AddObjectToAsset(item.iconSprite, db);
                    }
                    item.id = newId; item.name += "（副本）"; item.specialPurchaseGroup = -1;
                    db.addedItems = (db.addedItems ?? new ItemDefinition[0]).Concat(new[] { item }).ToArray();
                    CopyAssetIfMissing(sourceDrop, "Assets/Resources/Baked/Drops/Items/" + newId + ".prefab"); break;
                case "skills":
                    if (db.Get<SkillDefinition>().Any(r => r.id == newId)) throw new ArgumentException("技能编号重复");
                    var skill = Clone(db.Get<SkillDefinition>().First(r => r.id == templateId)); skill.behaviorId = skill.BehaviorId; skill.customBehavior = true;
                    skill.id = newId; skill.name += "（副本）"; db.addedSkills = (db.addedSkills ?? new SkillDefinition[0]).Concat(new[] { skill }).ToArray(); break;
                case "enemies":
                    if (db.Get<EnemyDefinition>().Any(r => r.id == newId) || db.Get<SpriteDefinition>().Any(r => r.id == newId)) throw new ArgumentException("敌人／精灵编号重复");
                    var enemy = Clone(db.Get<EnemyDefinition>().First(r => r.id == templateId));
                    var actor = Clone(db.Get<SpriteDefinition>().First(r => r.id == templateId));
                    actor.behaviorId = actor.BehaviorId; actor.customBehavior = true; actor.id = enemy.id = newId; actor.name += "（副本）";
                    string source = "Assets/Resources/Baked/Actors/" + templateId + ".prefab", target = "Assets/Resources/Baked/Actors/" + newId + ".prefab";
                    CopyAssetIfMissing(source, target);
                    var prefab = AssetDatabase.LoadAssetAtPath<OriginalActor>(target); if (!prefab) throw new InvalidOperationException("敌人模板缺少已保存角色Prefab");
                    db.addedEnemies = (db.addedEnemies ?? new EnemyDefinition[0]).Concat(new[] { enemy }).ToArray();
                    db.addedSprites = (db.addedSprites ?? new SpriteDefinition[0]).Concat(new[] { actor }).ToArray();
                    var catalog = OriginalAssets.Catalog; Undo.RecordObject(catalog, "登记扩展角色");
                    catalog.actors = catalog.actors.Concat(new[] { new OriginalAssetCatalog.ActorEntry { id = newId, prefab = prefab } }).ToArray();
                    EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssetIfDirty(catalog); break;
                default: throw new ArgumentException("不支持的内容分类");
            }
            db.ClearTableCache(); EditorUtility.SetDirty(db); AssetDatabase.SaveAssetIfDirty(db); OriginalAssets.ClearCache(); return newId;
        }
        private static void CopyAssetIfMissing(string source, string target)
        {
            if (File.Exists(target)) throw new InvalidOperationException("目标Prefab已存在：" + target);
            if (!AssetDatabase.CopyAsset(source, target)) throw new InvalidOperationException("无法复制模板Prefab：" + source);
        }
        public static string[] Validate(OriginalDatabase db)
        {
            var issues = new List<string>();
            CheckIds(db.Get<EquipmentDefinition>(), v => v.id, "武器", issues);
            CheckIds(db.Get<ItemDefinition>(), v => v.id, "道具", issues);
            CheckIds(db.Get<SkillDefinition>(), v => v.id, "技能", issues);
            CheckIds(db.Get<EnemyDefinition>(), v => v.id, "敌人", issues);
            CheckIds(db.Get<SpriteDefinition>(), v => v.id, "精灵", issues);
            var skills = new HashSet<int>(db.Get<SkillDefinition>().Select(v => v.id));
            var weapons = new HashSet<int>(db.Get<EquipmentDefinition>().Select(v => v.id));
            var items = new HashSet<int>(db.Get<ItemDefinition>().Select(v => v.id));
            foreach (var e in db.Get<EquipmentDefinition>())
            {
                if (e.skill >= 0 && !skills.Contains(e.skill)) issues.Add("武器" + e.id + "：技能不存在 " + e.skill);
                if (e.customRules && (e.maxReinforcement < 0 || e.maxReinforcement > 9)) issues.Add("武器" + e.id + "：强化上限必须为0～9");
                if (string.IsNullOrEmpty(e.image) || string.IsNullOrEmpty(e.icon) && !e.iconSprite) issues.Add("武器" + e.id + "：缺少外观或图标");
                foreach (var image in (e.image ?? "").Split(',').Where(s => !string.IsNullOrWhiteSpace(s)))
                    if (!Resources.Load<Texture2D>("Original/action/" + image)) issues.Add("武器" + e.id + "：外观纹理不存在 " + image);
                CheckPool(e.randomSkillIds, skills, "武器" + e.id, issues);
            }
            foreach (var i in db.Get<ItemDefinition>())
            {
                if (!i.customRules) continue;
                if (i.effectKind == ItemEffectKind.None) issues.Add("道具" + i.id + "：未选择效果");
                if (i.hp < 0 || i.durability < 0 || i.effectValue < 0 || i.buy < 0 || i.sell < 0) issues.Add("道具" + i.id + "：数值不能为负");
                if (i.specialPurchaseGroup < -1 || i.specialPurchaseGroup > 6) issues.Add("道具" + i.id + "：递增价格组必须为-1或0～6");
                if (i.shopMask < 0 || i.shopMask > 3) issues.Add("道具" + i.id + "：商店掩码为0～3");
                if (i.specialPurchaseGroup >= 0 && !i.immediateUseInShop) issues.Add("道具" + i.id + "：递增价格组要求购买后立即生效");
                if (i.maxPurchaseQuantity < 1 || i.maxPurchaseQuantity > 99) issues.Add("道具" + i.id + "：单次购买上限为1～99");
                if (i.priceIncreaseStep < 0 || i.specialPurchaseGroup >= 0 && i.priceCap < i.buy) issues.Add("道具" + i.id + "：递增价格参数无效");
                if (i.attributeIndex < 0 || i.attributeIndex > 5) issues.Add("道具" + i.id + "：永久属性槽为0～5");
                CheckPool(i.randomSkillIds, skills, "道具" + i.id, issues);
            }
            foreach (var i in db.addedItems ?? new ItemDefinition[0])
            {
                if (!i.iconSprite) issues.Add("扩展道具" + i.id + "：缺少持久图标");
                if (!Resources.Load<OriginalGroundDrop>("Baked/Drops/Items/" + i.id)) issues.Add("扩展道具" + i.id + "：缺少已保存掉落Prefab");
            }
            foreach (var s in db.Get<SkillDefinition>())
            {
                if (s.customBehavior && (s.behaviorId < 5 || s.behaviorId > 21)) issues.Add("技能" + s.id + "：当前行为模板支持5～21");
                if (s.cooldown < 0) issues.Add("技能" + s.id + "：冷却不能为负");
                try { OriginalCombatMath.SkillValue(s.damage, 1, true); OriginalCombatMath.SkillValue(s.amend, 1, false); OriginalCombatMath.Growth(s.cost, 1); }
                catch { issues.Add("技能" + s.id + "：伤害／修正／消耗公式无效"); }
            }
            foreach (var e in db.Get<EnemyDefinition>())
            {
                CheckDrops(e.equipmentDrops, weapons, "敌人" + e.id + "武器掉落", issues);
                CheckDrops(e.itemDrops, items, "敌人" + e.id + "道具掉落", issues);
                var actor = db.Get<SpriteDefinition>().FirstOrDefault(a => a.id == e.id);
                if (actor == null) { issues.Add("敌人" + e.id + "：缺少对应精灵配置"); continue; }
                if (actor.customBehavior)
                {
                    var template = db.sprites.FirstOrDefault(a => a.id == actor.behaviorId);
                    if (template == null || template.type != actor.type || template.animation != actor.animation)
                        issues.Add("敌人" + e.id + "：行为模板必须使用匹配的角色类型和动作布局");
                }
                if (!OriginalAssets.Catalog.actors.Any(a => a.id == e.id && a.prefab)) issues.Add("敌人" + e.id + "：缺少已保存的角色Prefab");
                int count = (e.attackIntervals ?? "").Split(',').Length;
                if ((e.damageCoefficients ?? "").Split(',').Length != count || (e.holdDamage ?? "").Split(',').Length != count || (e.attackRanges ?? "").Split(';').Length != count)
                    issues.Add("敌人" + e.id + "：攻击范围、伤害、霸体伤害和间隔条目数量不一致");
            }
            return issues.ToArray();
        }
        private static void CheckDrops(string value, HashSet<int> ids, string label, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim() == "0") return;
            foreach (string row in value.Split(';'))
            {
                var pair = row.Split(','); int chance, id;
                if (pair.Length != 2 || !int.TryParse(pair[0], out chance) || !int.TryParse(pair[1], out id) || chance < 0 || chance > 100)
                    issues.Add(label + "：格式为概率0～100,编号，多个条目用分号分隔");
                else if (!ids.Contains(id)) issues.Add(label + "：引用不存在 " + id);
            }
        }
        public static void SyncConfiguredDrops(OriginalDatabase db)
        {
            RequireEditMode();
            foreach (var item in db.addedItems ?? new ItemDefinition[0])
            {
                if (!item.iconSprite) continue;
                string path = "Assets/Resources/Baked/Drops/Items/" + item.id + ".prefab";
                if (!File.Exists(path)) continue;
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var icon = root.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(r => r.name == "道具");
                    if (!icon)
                    {
                        var child = new GameObject("道具"); child.transform.SetParent(root.transform, false);
                        child.transform.localPosition = new Vector3(11,-11,0); icon = child.AddComponent<SpriteRenderer>();
                    }
                    icon.sprite = item.iconSprite;
                    root.GetComponent<OriginalGroundDrop>().PrepareAsset(root.GetComponentsInChildren<SpriteRenderer>(true));
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }
        private static void CheckIds<T>(T[] rows, Func<T,int> id, string label, List<string> issues)
        { foreach (var g in rows.GroupBy(id).Where(g => g.Count() > 1)) issues.Add(label + "编号重复：" + g.Key); }
        private static void CheckPool(string text, HashSet<int> ids, string label, List<string> issues)
        {
            foreach (string value in (text ?? "").Split(',').Where(v => !string.IsNullOrWhiteSpace(v)))
                if (!int.TryParse(value, out int id) || !ids.Contains(id)) issues.Add(label + "：无效技能池条目 " + value);
        }
    }

    public sealed class ContentConfigurationWindow : EditorWindow
    {
        private readonly string[] names = { "武器", "道具", "技能", "敌人", "精灵", "阶段" };
        private readonly string[] fields = { "equipment", "items", "skills", "enemies", "sprites", "stages" };
        private readonly string[] added = { "addedEquipment", "addedItems", "addedSkills", "addedEnemies", "addedSprites", "addedStages" };
        private int tab, newId = 1000; private string search = "", message; private Vector2 listScroll, editScroll;
        private OriginalDatabase db; private SerializedObject serialized; private string selectedPath;
        [MenuItem("古剑/内容/武器道具技能敌人配置")]
        public static void Open() => GetWindow<ContentConfigurationWindow>("内容配置");
        private void OnEnable() { db = Resources.Load<OriginalDatabase>("OriginalDatabase"); if (db) serialized = new SerializedObject(db); }
        private void OnGUI()
        {
            if (!db) { EditorGUILayout.HelpBox("缺少 OriginalDatabase.asset", MessageType.Error); return; }
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                int next = GUILayout.Toolbar(tab, names); if (tab != next) { tab = next; selectedPath = null; }
                search = EditorGUILayout.TextField("搜索名称／编号", search);
                EditorGUILayout.HelpBox("原版数据与扩展数据共用运行入口。复制模板会分配新编号，复用已有规则／动作；新效果或AI家族需要新增规则实现。保存时校验跨表引用。", MessageType.Info);
                serialized.Update(); EditorGUILayout.BeginHorizontal(); listScroll = EditorGUILayout.BeginScrollView(listScroll, GUILayout.Width(230));
                foreach (var field in new[] { fields[tab], added[tab] })
                {
                    var array = serialized.FindProperty(field); if (array == null) continue;
                    EditorGUILayout.LabelField(field == fields[tab] ? "原版基线" : "扩展内容", EditorStyles.boldLabel);
                    for (int i = 0; i < array.arraySize; i++)
                    {
                        var row = array.GetArrayElementAtIndex(i); var id = row.FindPropertyRelative(tab == 5 ? "script" : "id");
                        var name = row.FindPropertyRelative("name"); string label = (tab == 5 ? id.stringValue : id.intValue.ToString()) + " " + (name == null ? "" : name.stringValue);
                        if (!string.IsNullOrEmpty(search) && label.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                        if (GUILayout.Toggle(selectedPath == row.propertyPath, label, "Button")) selectedPath = row.propertyPath;
                    }
                }
                EditorGUILayout.EndScrollView(); editScroll = EditorGUILayout.BeginScrollView(editScroll);
                var selected = string.IsNullOrEmpty(selectedPath) ? null : serialized.FindProperty(selectedPath);
                if (selected != null)
                {
                    EditorGUILayout.PropertyField(selected, new GUIContent(names[tab] + "配置"), true);
                    serialized.ApplyModifiedProperties();
                    if (tab < 4)
                    {
                        newId = EditorGUILayout.IntField("新编号", newId);
                        if (GUILayout.Button("复制选中模板到扩展内容")) Run(() => ContentConfiguration.Duplicate(db, fields[tab], selected.FindPropertyRelative("id").intValue, newId));
                    }
                    if (tab == 0 || tab == 1)
                        if (GUILayout.Button("将选中原版规则显式配置化")) Run(() =>
                        {
                            Undo.RecordObject(db, "配置原版规则"); int id = selected.FindPropertyRelative("id").intValue;
                            if (tab == 0) ContentConfiguration.Configure(db.Get<EquipmentDefinition>().First(e => e.id == id));
                            else ContentConfiguration.Configure(db.Get<ItemDefinition>().First(e => e.id == id));
                            EditorUtility.SetDirty(db); AssetDatabase.SaveAssetIfDirty(db);
                        });
                }
                EditorGUILayout.EndScrollView(); EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("校验并保存配置")) Run(() =>
                {
                    var errors = ContentConfiguration.Validate(db);
                    if (errors.Length > 0) throw new InvalidOperationException(string.Join("\n", errors));
                    ContentConfiguration.SyncConfiguredDrops(db); AssetDatabase.SaveAssetIfDirty(db); OriginalAssets.ClearCache(); message = "配置校验通过，已保存";
                });
                if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Info);
            }
        }
        private void Run(Action action) { try { action(); serialized.Update(); } catch (Exception e) { message = e.Message; } }
    }
}
