using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private Matrix4x4 BeginAtlasPanel(int action)
        {
            AtlasStyles();var previous=GUI.matrix;var r=originalUi.menuBounds;
            GUI.matrix=previous*Matrix4x4.TRS(new Vector3(r.x,r.y,0),Quaternion.identity,new Vector3(r.width/320,r.height/240,1));
            AtlasFrame(action);return previous;
        }
        private void DrawAtlasWeaponPreview()
        {
            var previous=BeginAtlasPanel(34);
            try
            {
                DrawPreviewActor(new Rect(8,53,72,145));
                DrawEquipmentIcon(weaponPreview.Data,new Rect(87,44,22,22));
                AtlasLabel(115,44,80,23,weaponPreview.Data.name+" +"+weaponPreview.reinforcement);
                int[] order={0,2,1,3,4,5,6};
                for(int i=0;i<7;i++)GUI.Label(new Rect(258,38+i*24,46,20),GetWeaponPreviewAttribute(order[i]).ToString(),atlasNumber);
                AtlasLabel(90,170,104,33,WeaponPreviewSkillLabel);
                AtlasLabel(14,192,285,18,Clean(weaponPreview.Data.description));
                AtlasLabel(80,215,174,19,"等级 "+weaponPreview.Data.level+"    售价 "+weaponPreview.Data.buy+" 两");
                if(AtlasClick(new Rect(5,211,52,23)) || AtlasClick(new Rect(263,211,52,23)))CloseWeaponPreview();
            }
            finally {GUI.matrix=previous;}
        }
        private void DrawAtlasReinforce()
        {
            var previous=BeginAtlasPanel(31);
            try
            {
                if(reinforceCandidates.Length==0){CloseReinforce();return;}
                GUI.enabled=!reinforceConfirm;
                reinforceScroll=GUI.BeginScrollView(new Rect(82,42,113,124),reinforceScroll,new Rect(0,0,98,Mathf.Max(124,reinforceCandidates.Length*31)));
                for(int i=0;i<reinforceCandidates.Length;i++)
                {
                    var data=Equipment[reinforceCandidates[i]];DrawEquipmentIcon(data.Data,new Rect(2,i*31+2,22,22));
                    if(AtlasClick(new Rect(25,i*31,72,29)))reinforceIndex=i;
                    AtlasLabel(25,i*31,72,29,(reinforceIndex==i?"› ":"")+data.Data.name+" +"+data.reinforcement);
                }
                GUI.EndScrollView();GUI.enabled=true;
                var preview=EquipmentBag.PreviewReinforce(Equipment[reinforceCandidates[reinforceIndex]],reinforceCandidates[reinforceIndex]==EquippedIndex&&!integrated?durability:(int?)null);
                if(preview==null)return;
                var item=preview.Current; var next=preview.Next;
                DrawPreviewRoleEquipment(item,new Rect(7,48,70,144));
                int[] values={item.Hp,item.Attack,item.MaximumDurability,item.Defense,item.Data.hit,item.Data.dodge,item.Data.critical};
                for(int i=0;i<7;i++)GUI.Label(new Rect(259,38+i*24,46,20),values[i].ToString(),atlasNumber);
                AtlasLabel(84,174,110,23,"费用 "+preview.Cost+" 两");
                if(AtlasClick(new Rect(5,211,52,23)))ReinforceConfirmOrApply();
                if(AtlasClick(new Rect(263,211,52,23))) { if(reinforceConfirm)reinforceConfirm=false;else CloseReinforce(); }
                if(reinforceConfirm)
                {
                    AtlasFrame(32);
                    AtlasLabel(60,61,204,24,item.Data.name+" +"+item.reinforcement+" → +"+next.reinforcement);
                    AtlasLabel(60,84,204,47,"攻击 "+item.Attack+" → "+next.Attack+"\n防御 "+item.Defense+" → "+next.Defense+"\n费用 "+item.ReinforceCost+" 两");
                    if(AtlasClick(new Rect(62,137,82,24),"确认强化"))ReinforceConfirmOrApply();
                    if(AtlasClick(new Rect(174,137,82,24),"取消"))reinforceConfirm=false;
                }
            }
            finally {GUI.enabled=true;GUI.matrix=previous;}
        }
    }
}
