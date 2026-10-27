using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private sealed class DropReward
        {
            public OriginalGroundDrop.DropKind kind;
            public int value;
            public EquipmentInstance equipment;
        }
        private sealed class DropNotice { public string text; public float x, width; public int ticks = 35; }
        private readonly List<OriginalGroundDrop> groundDrops = new List<OriginalGroundDrop>();
        private readonly List<DropNotice> dropNotices = new List<DropNotice>();
        private float dropElapsed;
        public IReadOnlyList<OriginalGroundDrop> GroundDrops => groundDrops;

        private void ClearDrops()
        {
            foreach (var drop in groundDrops) if (drop) Destroy(drop.gameObject);
            groundDrops.Clear(); dropElapsed = 0;
        }
        private void AwardDrops(OriginalActor source, EnemyDefinition enemy, int enemyLevel)
        {
            var rewards = new List<DropReward>();
            if (UnityEngine.Random.Range(0,100) < (experienceRoom ? 80 : 40))
                rewards.Add(new DropReward { kind = OriginalGroundDrop.DropKind.Money, value = EnemyDropMoney(enemy, enemyLevel) });
            if (enemyLevel > level - 10)
            {
                foreach (var row in enemy.equipmentDrops.Split(';'))
                {
                    var pair = row.Split(',');
                    if (pair.Length < 2 || UnityEngine.Random.Range(0,100) >= int.Parse(pair[0]) * (experienceRoom ? 2 : 1)) continue;
                    var equip = EquipmentInstance.Create(int.Parse(pair[1]));
                    rewards.Add(new DropReward { kind = OriginalGroundDrop.DropKind.Weapon, equipment = equip }); break;
                }
                foreach (var row in enemy.itemDrops.Split(';'))
                {
                    var pair = row.Split(',');
                    if (pair.Length < 2 || UnityEngine.Random.Range(0,100) >= int.Parse(pair[0]) * (experienceRoom ? 2 : 1)) continue;
                    rewards.Add(new DropReward { kind = OriginalGroundDrop.DropKind.Item, value = int.Parse(pair[1]) });
                }
            }
            SpawnDrops(source, rewards);
        }
        private void SpawnDrops(OriginalActor source, List<DropReward> rewards)
        {
            if (rewards.Count == 0) return;
            bool multiple = rewards.Count > 1;
            var cells = Enumerable.Range(0,9).ToList();
            int index = multiple ? rewards.Count - 1 : 0;
            while (index >= 0 && cells.Count > 0)
            {
                Vector2 position = source.mapPosition + new Vector2((source.Animation.footWidth - 22) >> 1, (source.Animation.footHeight - 22) >> 1);
                if (multiple)
                {
                    int cellIndex = UnityEngine.Random.Range(0,cells.Count), cell = cells[cellIndex]; cells.RemoveAt(cellIndex);
                    position += new Vector2((cell % 3 - 1) * 30, (cell / 3 - 1) * 30);
                    position = new Vector2(Mathf.Clamp(position.x,15,Map.Width-15),Mathf.Clamp(position.y,15,Map.Height-15));
                }
                var reward = rewards[index]; string path;
                if (reward.kind == OriginalGroundDrop.DropKind.Weapon)
                {
                    // Original multi-drop visuals use the role's weapon image, while the payload remains the dropped equipment.
                    string image = (multiple && Equipped != null ? Equipped.Data : reward.equipment.Data).image.Split(',')[0];
                    path = "Weapons/" + image; position = source.mapPosition;
                }
                else path = reward.kind == OriginalGroundDrop.DropKind.Money ? "Coin" : "Items/" + reward.value;
                var prefab = Resources.Load<OriginalGroundDrop>("Baked/Drops/" + path);
                if (!prefab) throw new System.InvalidOperationException("Missing saved drop prefab " + path);
                var drop = Instantiate(prefab,actorRoot.transform,false);
                int direction = reward.kind == OriginalGroundDrop.DropKind.Weapon ? 2 : UnityEngine.Random.Range(0,2);
                drop.Initialize(reward.kind,reward.value,reward.equipment,position,direction);
                groundDrops.Add(drop); index--;
            }
        }
        private void CollectDrop(OriginalGroundDrop drop)
        {
            if (!groundDrops.Remove(drop)) return;
            string message;
            if (drop.Kind == OriginalGroundDrop.DropKind.Money) { money += drop.Value; message = "银两+" + drop.Value; }
            else if (drop.Kind == OriginalGroundDrop.DropKind.Item)
            {
                Inventory.Add(drop.Value, 1);
                message = "获得 " + OriginalAssets.Table<ItemDefinition>("item").First(i=>i.id==drop.Value).name;
            }
            else { equipment.Add(drop.Equipment); message = "获得 " + drop.Equipment.Data.name; }
            dropNotices.Insert(0,new DropNotice { text=message, x=ViewWidth, width=message.Length * 15 });
            if (dropNotices.Count > 3) dropNotices.RemoveAt(3);
            Destroy(drop.gameObject);
        }
        private void PickUpTouchingWeapons()
        {
            if (!Role || Role.hidden) return;
            foreach (var drop in groundDrops.ToArray()) if (drop && drop.Touches(Role)) CollectDrop(drop);
        }
        private void TickDrops(float delta)
        {
            if (menu || reinforceOpen || DialogueVisible || CriticalActive || LastError != null || !Role || Map == null) { dropElapsed=0; return; }
            dropElapsed += delta;
            while (dropElapsed >= .08f)
            {
                dropElapsed -= .08f;
                foreach (var notice in dropNotices)
                {
                    if (notice.x >= ViewWidth - notice.width + 7) notice.x -= 10;
                    else { notice.x = ViewWidth - notice.width - 3; if (notice.ticks > 0) notice.ticks--; }
                }
                foreach (var drop in groundDrops.ToArray())
                {
                    if (!drop) { groundDrops.Remove(drop); continue; }
                    if (drop.Step(Role,Map)) CollectDrop(drop);
                    else if (drop.Expired) { groundDrops.Remove(drop); Destroy(drop.gameObject); }
                }
            }
        }
        private void DrawDropNotices()
        {
            for (int i=0;i<dropNotices.Count;i++)
            {
                var notice=dropNotices[i]; if (notice.ticks<=0) break;
                notice.width=bodyStyle.CalcSize(new GUIContent(notice.text)).x;
                GUI.Label(new Rect(notice.x,ViewHeight-(i+1)*18-20-14,notice.width+5,24),notice.text,bodyStyle);
            }
        }
    }
}
