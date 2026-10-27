using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private int[] mainMissionRewards = EmptyMissionRewards(), branchMissionRewards = EmptyMissionRewards();
        private static int[] EmptyMissionRewards() => new[] { -1, -1, -1 };
        private static int[] NormalizeMissionRewards(int[] values)
        {
            var result = EmptyMissionRewards();
            if (values != null) System.Array.Copy(values, result, Mathf.Min(3, values.Length));
            return result;
        }
        public int MissionReward(bool branch, int index) => index >= 0 && index < 3 ? (branch ? branchMissionRewards : mainMissionRewards)[index] : -1;
        public string BranchMissionTitle => branchMissionTitle;
        public string BranchMissionInfo => branchMissionInfo;
        private void SetMission(string title, string info, int[] rewards, bool branch)
        {
            if (branch)
            {
                branchMissionTitle = title; branchMissionInfo = Clean(info);
                branchMissionRewards = NormalizeMissionRewards(rewards);
                if (string.IsNullOrWhiteSpace(title)) tasks.Clear();
            }
            else
            {
                MissionTitle = title; MissionInfo = Clean(info);
                mainMissionRewards = NormalizeMissionRewards(rewards);
            }
            Toast("当前任务已更新，请在菜单查看");
        }
        public string[] MissionObjectiveLabels()
        {
            var labels = new List<string>();
            var definitions = OriginalAssets.Table<SpriteDefinition>("sprite");
            foreach (var task in tasks)
                for (int i = 0; i < Mathf.Min(task.enemyIds.Length, task.counts.Length); i++)
                {
                    var enemy = definitions.FirstOrDefault(d => d.id == task.enemyIds[i]);
                    labels.Add((enemy == null ? task.enemyIds[i].ToString() : enemy.name) + "：剩余 " + Mathf.Max(0, task.counts[i]));
                }
            return labels.ToArray();
        }
        private float MissionCardHeight(string title, string info, bool branch)
        {
            if (string.IsNullOrWhiteSpace(title)) return 48;
            float height = 42 + Mathf.Max(24, bodyStyle.CalcHeight(new GUIContent(info ?? ""), 420));
            if (MissionReward(branch, 0) > 0 || MissionReward(branch, 1) > 0) height += 26;
            if (MissionReward(branch, 2) >= 0) height += 30;
            return height + 12;
        }
        private float MissionMenuHeight() => MissionCardHeight(MissionTitle, MissionInfo, false) + MissionCardHeight(branchMissionTitle, branchMissionInfo, true) + 90 + MissionObjectiveLabels().Length * 25;
        private float DrawMissionCard(float y, bool branch)
        {
            string title = branch ? branchMissionTitle : MissionTitle, info = branch ? branchMissionInfo : MissionInfo;
            float height = MissionCardHeight(title, info, branch);
            GUI.Label(new Rect(6, y, 420, 28), (branch ? "支线 · " : "主线 · ") + (string.IsNullOrWhiteSpace(title) ? "暂无任务" : title), titleStyle);
            if (string.IsNullOrWhiteSpace(title)) return y + height;
            y += 36;
            float textHeight = Mathf.Max(24, bodyStyle.CalcHeight(new GUIContent(info ?? ""), 420));
            GUI.Label(new Rect(6, y, 420, textHeight), info, bodyStyle); y += textHeight + 6;
            var rewards = new List<string>();
            if (MissionReward(branch, 0) > 0) rewards.Add("经验 " + MissionReward(branch, 0));
            if (MissionReward(branch, 1) > 0) rewards.Add("银两 " + MissionReward(branch, 1));
            if (rewards.Count > 0) { GUI.Label(new Rect(6, y, 420, 24), "奖励：" + string.Join("　", rewards), bodyStyle); y += 26; }
            int id = MissionReward(branch, 2);
            var weapon = OriginalAssets.Table<EquipmentDefinition>("equip").FirstOrDefault(e => e.id == id);
            if (weapon != null)
            {
                DrawEquipmentIcon(weapon, new Rect(6, y, 22, 22));
                GUI.Label(new Rect(34, y, 392, 24), "装备奖励：" + weapon.name, bodyStyle); y += 30;
            }
            return y + 12;
        }
        private void DrawMissionMenu()
        {
            float y = DrawMissionCard(0, false); y = DrawMissionCard(y, true);
            var objectives = MissionObjectiveLabels();
            if (objectives.Length > 0)
            {
                GUI.Label(new Rect(6, y, 420, 26), "击杀目标", bodyStyle); y += 28;
                foreach (string objective in objectives) { GUI.Label(new Rect(16, y, 410, 24), objective, bodyStyle); y += 25; }
            }
            GUI.Label(new Rect(6, y + 12, 420, 45), "WASD / 方向键移动，E 交谈，J 攻击，K 技能，L 崩击，I 合体。", bodyStyle);
        }
    }
}
