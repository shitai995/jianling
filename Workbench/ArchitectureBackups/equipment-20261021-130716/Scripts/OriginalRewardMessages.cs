using System.Linq;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private static readonly string[] originalRewardColors = { "ffffff", "00ff00", "00ccff", "ff00ff", "f9a12f", "d70369" };

        private void ScriptAddMoney(int amount, bool showMessage)
        {
            string message;
            if (amount > 0)
            {
                money += amount;
                message = "获得  <c=ff00ff,0,1>" + amount + "银两";
            }
            else if (unchecked(-amount) > money) message = "你没有足够的银两！";
            else
            {
                money += amount;
                message = "失去  <c=ff00ff,0,1>" + unchecked(-amount) + "银两";
            }
            if (showMessage) ShowOriginalMessage(message, 0, false);
        }

        private void ScriptAddItem(int id, int quantity, bool showMessage)
        {
            Inventory.Add(id, quantity);
            if (!showMessage) return;
            var item = OriginalAssets.Table<ItemDefinition>("item").First(i => i.id == id);
            ShowOriginalMessage("得到 <c=ff00ff>" + quantity + "</c>个 <c=" + originalRewardColors[item.type] + ">" + item.name + "</c>!", 0, false);
        }

        private void ScriptAddEquipment(int id, int reinforcement, bool showMessage)
        {
            var item = EquipmentInstance.Create(id, reinforcement);
            equipment.Add(item);
            if (showMessage) ShowOriginalMessage("得到 <c=" + originalRewardColors[item.Data.grade] + ">" + item.Data.name + "</c>", 0, false);
        }

        private void ScriptAddExperience(int amount, bool showMessage)
        {
            AddExperience(amount);
            // Original script_addExp displays the message without script.pause().
            if (showMessage) ShowOriginalMessage("得到 <c=00ff00>" + amount + "</c>点经验!", 0, false, false);
        }
    }
}
