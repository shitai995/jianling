using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GuJian.Editor
{
    // Prepare/Cleanup run outside Play. The runtime check uses an independent save slot.
    public static class ContentExtensionValidation
    {
        private const string Backup = "Library/ContentExtensionValidation";
        private const string Database = "Assets/Resources/OriginalDatabase.asset";
        private const string Catalog = "Assets/Resources/OriginalAssetCatalog.asset";
        private const string MapId = "map_extension_validation";
        private const string StoryId = "9005";
        private const string TileAsset = "Assets/Editor/__validation_tile.asset";
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static string[] TemporaryPaths => new[] {
            "Assets/Resources/Baked/Stages/9005.prefab", TilemapAuthoring.Root + "/" + MapId + ".prefab",
            "Assets/Resources/Baked/Drops/Items/9001.prefab", "Assets/Resources/Baked/Actors/9004.prefab",
            TileAsset, StoryAssetSetup.AssetPath(StoryId) };
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        private static object Call(GuJianGame game, string name, params object[] args) => typeof(GuJianGame).GetMethod(name, Flags).Invoke(game, args);
        private static void Report(string file, List<string> checks, string error)
        {
            File.WriteAllText("../Reference/Validation/" + file + ".json", JsonUtility.ToJson(new ReconstructionValidation.Report {
                passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            Debug.Log("GUJIAN_" + file + " passed=" + (error == null) + " " + error);
        }
        public static void Prepare()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Directory.Exists(Backup)) throw new InvalidOperationException("测试需在编辑模式且没有未清理的备份");
            foreach (string path in TemporaryPaths) if (File.Exists(path)) throw new InvalidOperationException("测试路径被占用：" + path);
            Directory.CreateDirectory(Backup); File.Copy(Database, Backup + "/database.bin"); File.Copy(Catalog, Backup + "/catalog.bin");
            var checks = new List<string>(); string error = null;
            try
            {
                var db = Resources.Load<OriginalDatabase>("OriginalDatabase");
                ContentConfiguration.Duplicate(db, "items", 0, 9001);
                ContentConfiguration.Duplicate(db, "skills", 5, 9002);
                ContentConfiguration.Duplicate(db, "equipment", 0, 9003);
                ContentConfiguration.Duplicate(db, "enemies", 201, 9004);
                var item = db.addedItems.First(i => i.id == 9001); item.hp = 333; item.buy = 17; item.shopMask = 2;
                var skill = db.addedSkills.First(s => s.id == 9002); skill.cooldown = 17;
                var weapon = db.addedEquipment.First(e => e.id == 9003); weapon.attack = 333; weapon.skill = 9002;
                var enemy = db.addedEnemies.First(e => e.id == 9004); enemy.money = 321;
                ContentConfiguration.SyncConfiguredDrops(db); EditorUtility.SetDirty(db); AssetDatabase.SaveAssetIfDirty(db);
                Require(item.customRules && item.EffectKind == ItemEffectKind.HealHealth && item.iconSprite &&
                    weapon.ReinforceAllowed && skill.BehaviorId == 5 && db.addedSprites.First(a => a.id == 9004).BehaviorId == 201,
                    "Template duplication lost rules/assets");
                checks.Add("Independent IDs 9001-9004 created through production authoring; item icon/drop, weapon, skill behavior and enemy actor prefab saved");
                Require(ContentConfiguration.Validate(db).Length == 0, "Valid extension configuration rejected");
                int oldSkill = weapon.skill; weapon.skill = 123456;
                Require(ContentConfiguration.Validate(db).Any(s => s.Contains("技能不存在")), "Broken reference not detected"); weapon.skill = oldSkill;
                string oldDrops = enemy.itemDrops; enemy.itemDrops = "100,123456";
                Require(ContentConfiguration.Validate(db).Any(s => s.Contains("掉落：引用不存在")), "Missing enemy drop reference not detected"); enemy.itemDrops = oldDrops;
                checks.Add("Cross-table validator accepts valid extensions and rejects missing weapon skill and enemy drop references");
                var trading = new ItemTrading(new ItemInventory(), db.Get<ItemDefinition>());
                Require(!trading.ForSale(9001, 1, 0) && trading.ForSale(9001, 1, 1), "Configured shop mask ignored");
                var quote = trading.Quote(9001, false, -1, 1, 1); trading.SetQuantity(quote, 3); int money = 100;
                Require(trading.Confirm(quote, 1, 1, ref money, null) == ItemTradeResult.Success && money == 49, "Extended item commerce failed");
                checks.Add("New item IDs honor explicit shop masks and ordinary quantity/pricing without ID-range restrictions");
                item.maxPurchaseQuantity = 4;
                quote = trading.Quote(9001, false, -1, 1, 1); trading.SetQuantity(quote, 99);
                Require(quote.Quantity == 4 && trading.MaximumQuantity(quote) == 4, "Configured purchase limit ignored");
                trading.MoveQuantity(quote, 1); Require(quote.Quantity == 1, "Configured purchase quantity did not wrap");
                item.maxPurchaseQuantity = 99;
                var view = db.Get<ItemDefinition>();
                Require(ReferenceEquals(view, db.Get<ItemDefinition>()) && view.First(i => i.id == 9001) == item, "Merged table does not cache references");
                db.ClearTableCache(); Require(!ReferenceEquals(view, db.Get<ItemDefinition>()), "Merged table cache was not invalidated");
                checks.Add("Configured purchase cap clamps and wraps; merged extension tables reuse references and invalidate explicitly");
                var prefab = TilemapAuthoring.Create(MapId, 4, 3, 32, 16);
                var tile = ScriptableObject.CreateInstance<Tile>(); tile.sprite = OriginalAssets.Catalog.solid;
                AssetDatabase.CreateAsset(tile, TileAsset);
                var root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
                try
                {
                    var map = root.GetComponent<EditableTilemap>(); map.background.SetTile(new Vector3Int(0,-1,0), tile);
                    map.collision.SetTile(new Vector3Int(1,-2,0), tile);
                    PrefabUtility.SaveAsPrefabAsset(root, AssetDatabase.GetAssetPath(prefab));
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
                TilemapAuthoring.Publish(prefab);
                var entry = OriginalAssets.Catalog.maps.First(m => m.id == MapId);
                Require(entry.data.Width == 128 && entry.data.Height == 48 && !entry.data.CanPass(48,24) && entry.data.CanPass(16,8), "Tilemap collision bake mismatch");
                Require(prefab.GetComponent<EditableTilemap>().background.HasTile(new Vector3Int(0,-1,0)), "Tilemap visual tile was not saved");
                checks.Add("Saved four-layer Tilemap prefab retains painted tile; editor-baked collision maps (1,-2) to original (1,1) grid coordinates");
                TilemapAuthoring.CreateStage(prefab, 9005, "扩展地图验证", 16, 8);
                var story = AssetDatabase.LoadAssetAtPath<StorySequence>(StoryAssetSetup.AssetPath(StoryId));
                story.Import(StoryId, "扩展地图验证", story.ReadInstructions().Concat(new[] { "x24(3,1,\"200.s\")" }).ToArray());
                EditorUtility.SetDirty(story); AssetDatabase.SaveAssetIfDirty(story);
                var before = JsonUtility.ToJson(db); ReconstructionTools.ImportDatabase();
                Require(JsonUtility.ToJson(db) == before, "Repeated original import overwrote baseline edits/extensions");
                OriginalAssets.ClearCache();
                Require(OriginalAssets.Table<StageDefinition>("stages").Any(s => s.script == StoryId) && OriginalAssets.Table<ItemDefinition>("item").Any(i => i.id == 9001), "Runtime table lookup ignored additions");
                checks.Add("Missing-only original import preserves all edited baseline/extension fields; shared lookup discovers custom stage and new IDs");
            }
            catch (Exception e) { error = e.ToString(); }
            Report("content-extension-editor", checks, error);
            if (error != null) { Cleanup(); throw new Exception(error); }
        }
        public static void RunInPlay()
        {
            var checks = new List<string>(); string error = null;
            var game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            string save = game.saveFileName; var random = UnityEngine.Random.state;
            try
            {
                game.saveFileName = "validation-content-extension.json";
                game.Begin(); game.Machine.Clear(); game.Machine.Values["event"] = -999; game.Machine.Enqueue("200"); game.Machine.Tick();
                foreach (var a in game.Actors.Values.Where(a => a.IsEnemy)) { a.locked = true; a.hidden = true; }
                game.Machine.Command(0x51, new object[] { 9001, 2 }); var role = game.Actors[-1]; role.hp = role.maxHp - 500;
                int hp = role.hp; Require(game.UseItem(9001) && role.hp == hp + 333 && game.ItemCount(9001) == 1, "Configured healing failed");
                checks.Add("New potion ID heals by configured amount and consumes inventory via actual gameplay use");
                var item = OriginalAssets.Table<ItemDefinition>("item").First(i => i.id == 9001);
                item.effectKind = ItemEffectKind.Money; item.effectValue = 45; item.consumeWholeStack = true;
                game.Machine.Command(0x51, new object[] { 9001, 2 }); int money = game.money;
                Require(game.UseItem(9001) && game.money == money + 135 && game.ItemCount(9001) == 0, "Configured whole-stack effect failed");
                item.effectKind = ItemEffectKind.HealHealth; item.consumeWholeStack = false;
                checks.Add("Configured Money effect and whole-stack consumption work on a new ID, independent of legacy item ranges");
                var weapon = EquipmentInstance.Create(9003); game.EquipmentBag.Add(weapon); Call(game, "SelectEquipment", game.Equipment.Count - 1);
                Require(weapon.Attack == 333 && weapon.skillId == 9002 && weapon.CanReinforce, "New weapon data not used");
                role.ResetAction(); Call(game, "ResetSkillCooldown", true);
                Require(game.UseSkill(9002) && role.LogicalAction == 12 && game.SkillCooldownTicks == 17, "New attack skill behavior/cooldown mapping failed");
                role.Tick(.08f);
                checks.Add("New weapon equips saved art and configured stats; new skill ID runs attack-template 5 animation with its own cooldown");
                var skill = OriginalAssets.Table<SkillDefinition>("skill").First(s => s.id == 9002);
                skill.behaviorId = 14; skill.damage = "10"; skill.amend = "0";
                role.ResetAction(); role.hp = role.maxHp / 2; Call(game, "ResetSkillCooldown", true);
                Require(game.UseSkill(9002) && game.SupportSkillCasting && role.LogicalAction == 21, "New support skill behavior failed");
                hp = role.hp; Call(game, "CompleteSupportSkill", role); Require(role.hp > hp, "New support skill effect failed");
                skill.behaviorId = 5;
                checks.Add("Same new skill ID can select support-template 14 and heal through the production support casting path");
                game.Machine.Command(0x30, new object[] { 9900, 9004, "l", 200, 250, 2 });
                var enemy = game.Actors[9900]; Require(enemy.IsEnemy && enemy.definitionId == 9004 && enemy.BehaviorId == 201 && enemy.maxHp > 0, "New enemy did not spawn with independent ID");
                checks.Add("New enemy ID spawns saved actor prefab, retains its own definition and references existing AI behavior 201");
                game.Save(); game.Load(); game.Machine.Tick();
                Require(game.Equipment.Any(e => e.id == 9003 && e.skillId == 9002), "New equipment IDs failed save/load");
                checks.Add("Independent test save/load preserves new equipment and skill IDs");
                game.Machine.Clear(); game.Machine.Enqueue(StoryId); game.Machine.Tick();
                Require(game.Machine.Error == null && game.Map.id == MapId && game.Map.Width == 128 && !game.Map.CanPass(48,24), "New Tilemap stage did not load");
                var mapRoot = (GameObject)typeof(GuJianGame).GetField("mapRoot", Flags).GetValue(game);
                var instance = mapRoot.GetComponentsInChildren<EditableTilemap>(true).First(m => m.mapId == MapId);
                Require(instance.background.HasTile(new Vector3Int(0,-1,0)) && instance.GetComponentsInChildren<Tilemap>(true).Length == 4, "Saved Tilemaps missing at runtime");
                var exits = (IDictionary)typeof(GuJianGame).GetField("transitions", Flags).GetValue(game);
                Require(Convert.ToString(exits[new Vector2Int(3,1)]) == "200.s", "Tilemap transition not registered by original VM");
                checks.Add("Custom stage uses production map loader, instantiates four saved Tilemaps, preserves baked collision and registers its original x24 exit");
            }
            catch (Exception e) { error = e.ToString(); }
            finally { UnityEngine.Random.state = random; game.saveFileName = save; game.Machine.Clear(); game.Machine.Command(0x69, new object[0]); }
            Report("content-extension-play", checks, error);
        }
        public static void Cleanup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("退出播放模式后清理测试资产");
            if (!Directory.Exists(Backup)) return;
            File.Copy(Backup + "/catalog.bin", Catalog, true);
            AssetDatabase.ImportAsset(Catalog, ImportAssetOptions.ForceUpdate);
            // Remove dependants before their map, tile and database subassets.
            foreach (string path in TemporaryPaths) if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            File.Copy(Backup + "/database.bin", Database, true);
            AssetDatabase.ImportAsset(Database, ImportAssetOptions.ForceUpdate);
            Require(File.ReadAllBytes(Database).SequenceEqual(File.ReadAllBytes(Backup + "/database.bin")) &&
                File.ReadAllBytes(Catalog).SequenceEqual(File.ReadAllBytes(Backup + "/catalog.bin")), "Fixture restoration changed production asset bytes");
            File.Delete(Backup + "/database.bin"); File.Delete(Backup + "/catalog.bin"); Directory.Delete(Backup);
            OriginalAssets.ClearCache(); Debug.Log("GUJIAN_CONTENT_EXTENSION_CLEANUP production database/catalog restored byte-for-byte");
        }
    }
}
