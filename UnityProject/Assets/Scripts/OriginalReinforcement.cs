using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private bool reinforceOpen, reinforceConfirm;
        private int reinforceIndex;
        private int[] reinforceCandidates = new int[0];
        private Vector2 reinforceScroll;
        public bool ReinforcementVisible => reinforceOpen;
        public bool ReinforcementConfirming => reinforceConfirm;

        private void OpenReinforce()
        {
            reinforceCandidates = Enumerable.Range(0, Equipment.Count).Where(i => Equipment[i].CanReinforce).ToArray();
            // GameCanvas.initReinforce uses script_showMessage("无可强化的武器!", 0, 1)
            // when no candidate exists.  It is a blocking message: the script VM resumes
            // only after the original confirmation key, and the ASCII ! is intentional.
            if (reinforceCandidates.Length == 0) { ShowOriginalMessage("无可强化的武器!", 0, false); return; }
            menu = false; reinforceOpen = true; reinforceConfirm = false;
            reinforceIndex = 0; reinforceScroll = Vector2.zero;
        }
        // Shared by keyboard, GUI and the original guide1 tutorial; a confirmation never identifies an item by ID.
        public void ReinforceConfirmOrApply()
        {
            if (!reinforceOpen) return;
            int index = reinforceCandidates[reinforceIndex];
            if (reinforceConfirm) { ReinforceEquipment(index); reinforceConfirm = false; }
            else if (!Equipment[index].CanReinforce) Toast("强化等级已达上限！");
            else if (money < Equipment[index].ReinforceCost) Toast("银两不足");
            else reinforceConfirm = true;
            RespondTutorial(4);
        }
        public void CloseReinforce()
        {
            reinforceOpen = reinforceConfirm = false;
            RespondTutorial(6);
        }
        private void TickReinforceInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || (tutorialKey == 6 && Input.GetKeyDown(KeyCode.Return)))
            {
                if (reinforceConfirm) reinforceConfirm = false; else CloseReinforce();
            }
            else if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) ReinforceConfirmOrApply();
            else if (!reinforceConfirm && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
            {
                reinforceIndex = (reinforceIndex + reinforceCandidates.Length - 1) % reinforceCandidates.Length; RespondTutorial(0);
            }
            else if (!reinforceConfirm && (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)))
            {
                reinforceIndex = (reinforceIndex + 1) % reinforceCandidates.Length; RespondTutorial(1);
            }
        }
        private void DrawReinforce()
        {
            if (originalUi && originalUi.menu != null && originalUi.menu.actions != null) { DrawAtlasReinforce(); return; }
            var oldColor = GUI.color;
            GUI.color = new Color(.06f, .07f, .09f, .98f);
            GUI.DrawTexture(new Rect(25, 18, 483, 287), Texture2D.whiteTexture);
            GUI.color = oldColor;
            GUI.Box(new Rect(25, 18, 483, 287), "");
            GUI.Label(new Rect(40, 28, 280, 28), "强化武器    银两 " + money, titleStyle);
            if (GUI.Button(new Rect(438, 27, 55, 28), "返回", buttonStyle)) CloseReinforce();
            reinforceScroll = GUI.BeginScrollView(new Rect(38, 65, 205, 213), reinforceScroll,
                new Rect(0, 0, 182, Mathf.Max(213, reinforceCandidates.Length * 35)));
            for (int row = 0; row < reinforceCandidates.Length; row++)
            {
                var e = Equipment[reinforceCandidates[row]];
                GUI.enabled = !reinforceConfirm;
                if (GUI.Button(new Rect(0, row * 35, 181, 32), (row == reinforceIndex ? "▶ " : "") + e.Data.name + " +" + e.reinforcement, buttonStyle)) reinforceIndex = row;
            }
            GUI.enabled = true; GUI.EndScrollView();
            var preview = EquipmentBag.PreviewReinforce(Equipment[reinforceCandidates[reinforceIndex]], reinforceCandidates[reinforceIndex] == EquippedIndex && !integrated ? durability : (int?)null);
            if (preview == null) return;
            var item = preview.Current; var next = preview.Next; int current = preview.CurrentDurability;
            GUI.Label(new Rect(258, 64, 232, 145),
                "气血  " + item.Hp + " → " + next.Hp + "\n攻击  " + item.Attack + " → " + next.Attack +
                "\n防御  " + item.Defense + " → " + next.Defense + "\n耐久  " + current + "/" + item.MaximumDurability + " → " + next.durability + "/" + next.MaximumDurability +
                (item.skillId >= 0 ? "\n技能等级  " + item.skillLevel + " → " + next.skillLevel : ""), bodyStyle);
            GUI.Label(new Rect(258, 212, 230, 30), "费用 " + preview.Cost + " 两", bodyStyle);
            if (GUI.Button(new Rect(258, 250, 145, 33), reinforceConfirm ? "确认强化" : "强化（J）", buttonStyle)) ReinforceConfirmOrApply();
            if (reinforceConfirm && GUI.Button(new Rect(408, 250, 80, 33), "取消", buttonStyle)) reinforceConfirm = false;
            if (Time.time < toastUntil) GUI.Label(new Rect(40, 283, 450, 20), toast, bodyStyle);
        }
    }
}
