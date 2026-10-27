using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private GUIStyle atlasText, atlasNumber, atlasTitle;
        private int atlasSelection, atlasPage = -1;
        private bool atlasBranch;
        private Vector2 atlasScroll;
        private void AtlasStyles()
        {
            if (atlasText != null) return;
            atlasText = new GUIStyle(bodyStyle) { fontSize = 11, clipping = TextClipping.Clip };
            atlasText.normal.textColor = new Color(.94f,.87f,.68f);
            atlasNumber = new GUIStyle(atlasText) { alignment = TextAnchor.MiddleRight };
            atlasNumber.normal.textColor = new Color(.12f,.07f,.025f);
            atlasTitle = new GUIStyle(atlasText) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
        }
        private bool AtlasClick(Rect r, string text = null)
        {
            bool clicked = GUI.Button(r,GUIContent.none,GUIStyle.none);
            if (text != null) GUI.Label(r,text,atlasTitle);
            return clicked;
        }
        private void AtlasLabel(float x,float y,float w,float h,string text) { GUI.Label(new Rect(x,y,w,h),text ?? "",atlasText); }
        private void AtlasFrame(int action) { OriginalGui.Animation(originalUi.menu,originalUi.menuSheets,action,Mathf.FloorToInt(Time.unscaledTime/.08f),new Vector2(160,120)); }
        private void DrawAtlasMenu()
        {
            AtlasStyles();
            if (atlasPage != menuPage) { atlasSelection = 0; atlasScroll = Vector2.zero; atlasPage = menuPage; }
            var previous = GUI.matrix;
            var bounds = originalUi.menuBounds;
            GUI.matrix = previous * Matrix4x4.TRS(new Vector3(bounds.x,bounds.y,0),Quaternion.identity,new Vector3(bounds.width/320,bounds.height/240,1));
            try
            {
                if (menuPage == 4) { DrawAtlasMap(); return; }
                int native = menuPage == 7 ? 0 : menuPage == 2 ? 1 : menuPage == 1 ? 2 : menuPage == 6 ? 3 : menuPage == 0 ? 4 : 5;
                if (menuPage == 6) { AtlasFrame(23); AtlasFrame(SpiritFeedAnimating ? 26 : 24); }
                AtlasFrame(menuPage == 5 ? (shopSelling ? 14 : 13) : menuPage == 0 && atlasBranch ? 7 : native);
                bool modal = TradeConfirming || SpiritFeedConfirming || SpiritFeedAnimating;
                GUI.enabled = !modal;
                if (menuPage != 5)
                {
                    int[] pages = {7,2,1,6,0,3};
                    for (int i=0;i<6;i++)
                        if (AtlasClick(new Rect(54+i*26+(i>native?50:0),2,i==native?76:24,24))) SelectMenuPage(pages[i]);
                }
                if (AtlasClick(new Rect(263,211,52,23))) CloseMenu();
                if (menuPage == 2) DrawAtlasEquipment();
                else if (menuPage == 1) DrawAtlasItems();
                else if (menuPage == 0) DrawAtlasMission();
                else if (menuPage == 6) DrawAtlasSpirit();
                else if (menuPage == 5) DrawAtlasShop();
                else if (menuPage == 7) DrawAtlasRole();
                else DrawAtlasSystem();
                GUI.enabled = true;
                if (TradeConfirming) DrawAtlasTrade();
                if (SpiritFeedConfirming) DrawAtlasFeed();
                if (menuPage != 6) AtlasLabel(196,220,64,18,money+" 两");
            }
            finally { GUI.enabled = true; GUI.matrix = previous; }
        }
        private void AtlasRows(int count,Rect bounds,int rowHeight,Action<int,float> draw)
        {
            atlasScroll=GUI.BeginScrollView(bounds,atlasScroll,new Rect(0,0,bounds.width-15,Mathf.Max(bounds.height,count*rowHeight)));
            for(int i=0;i<count;i++) draw(i,i*rowHeight);
            GUI.EndScrollView();
        }
        private void DrawAtlasEquipment()
        {
            if(equipment.Count==0) return;
            atlasSelection=Mathf.Clamp(atlasSelection,0,equipment.Count-1);
            AtlasRows(equipment.Count,new Rect(82,39,113,124),31,(i,y)=>{
                if(i==atlasSelection) { var c=GUI.color; GUI.color=new Color(.78f,.71f,.56f);GUI.DrawTexture(new Rect(0,y,98,26),Texture2D.whiteTexture);GUI.color=c; }
                DrawEquipmentIcon(equipment[i].Data,new Rect(2,y+2,22,22));
                if(AtlasClick(new Rect(25,y,72,29))) atlasSelection=i;
                GUI.Label(new Rect(25,y,72,29),equipment[i].Data.name+(equipment[i].reinforcement>0?" +"+equipment[i].reinforcement:""),i==atlasSelection?atlasNumber:atlasText);
            });
            var item=equipment[atlasSelection];
            int[] values={item.Hp,item.Attack,item.MaximumDurability,item.Defense,item.Data.hit,item.Data.dodge,item.Data.critical};
            for(int i=0;i<7;i++) GUI.Label(new Rect(258,38+i*24,46,20),values[i].ToString(),atlasNumber);
            DrawPreviewRoleEquipment(item,new Rect(8,48,68,145));
            AtlasLabel(15,193,285,17,Clean(item.Data.description));
            if(AtlasClick(new Rect(5,211,51,23))) { if(item.Data.level<=level) SelectEquipment(atlasSelection);else Toast("等级不足"); }
            AtlasLabel(92,171,105,20,item.skillId>=0?OriginalAssets.Table<SkillDefinition>("skill").First(s=>s.id==item.skillId).name+" "+item.skillLevel+"级":"无技能");
        }
        private void DrawPreviewRoleEquipment(EquipmentInstance item,Rect bounds)
        {
            // Reuse the preview renderer without allocating an actor, Sprite or texture.
            var old=weaponPreview; var oldBounds=weaponPreviewBounds;
            weaponPreview=item; weaponPreviewBounds=PreviewIdleBounds(Role.Animation);
            DrawPreviewActor(bounds); weaponPreview=old; weaponPreviewBounds=oldBounds;
        }
        private void DrawAtlasItems()
        {
            var stacks=inventory.Where(p=>p.Value>0).OrderBy(p=>p.Key).SelectMany(p=>Enumerable.Range(0,ItemStackCount(p.Key)).Select(stack=>new[]{p.Key,stack})).ToArray();
            atlasSelection=Mathf.Clamp(atlasSelection,0,Mathf.Max(0,stacks.Length-1));
            AtlasRows(stacks.Length,new Rect(16,50,205,145),31,(i,y)=>{
                var item=OriginalAssets.Table<ItemDefinition>("item").First(a=>a.id==stacks[i][0]);
                if(AtlasClick(new Rect(0,y,188,30)))atlasSelection=i;
                DrawItemIcon(item.id,new Rect(2,y+2,22,22));
                AtlasLabel(30,y+4,155,24,(i==atlasSelection?"› ":"")+item.name+" ×"+ItemStackQuantity(item.id,stacks[i][1]));
            });
            if(stacks.Length>0)
            {
                int id=stacks[atlasSelection][0]; AtlasLabel(232,47,70,147,Clean(OriginalAssets.Table<ItemDefinition>("item").First(a=>a.id==id).description));
                if(AtlasClick(new Rect(5,211,52,23)))UseItemStack(id,stacks[atlasSelection][1]);
            }
            if(AtlasClick(new Rect(229,194,74,16),"用药 "+autoRecoverPercent+"%"))SetAutoRecoverPercent(autoRecoverPercent==90?10:autoRecoverPercent+10);
        }
        private void DrawAtlasMission()
        {
            if(AtlasClick(new Rect(17,37,138,48)))atlasBranch=false;
            if(AtlasClick(new Rect(17,93,138,49)))atlasBranch=true;
            AtlasLabel(30,47,122,38,MissionTitle ?? "暂无主线"); AtlasLabel(30,105,122,35,branchMissionTitle ?? "暂无支线");
            string text=(atlasBranch?branchMissionInfo:MissionInfo)??"";
            var objectives=MissionObjectiveLabels();if(objectives.Length>0)text+="\n"+string.Join("\n",objectives);
            float h=Mathf.Max(103,atlasText.CalcHeight(new GUIContent(text),126));
            atlasScroll=GUI.BeginScrollView(new Rect(166,37,140,105),atlasScroll,new Rect(0,0,124,h));
            GUI.Label(new Rect(0,0,124,h),text,atlasText);GUI.EndScrollView();
            AtlasLabel(92,169,215,21,"经验 "+Mathf.Max(0,MissionReward(atlasBranch,0))+"   银两 "+Mathf.Max(0,MissionReward(atlasBranch,1)));
            var equip=OriginalAssets.Table<EquipmentDefinition>("equip").FirstOrDefault(e=>e.id==MissionReward(atlasBranch,2));
            if(equip!=null) { DrawEquipmentIcon(equip,new Rect(92,191,22,22));AtlasLabel(120,191,180,22,equip.name); }
        }
        private void DrawAtlasSpirit()
        {
            if(!Actor(-2)){AtlasLabel(20,60,280,60,"剑灵尚未加入。");return;}
            for(int i=0;i<7;i++)GUI.Label(new Rect(260,50+i*21,45,20),GetSpiritAttribute(i).ToString(),atlasNumber);
            AtlasLabel(53,107,40,18,SpiritLevel.ToString());
            var expBounds=new Rect(20,153,174,8);
            OriginalGui.Bar(originalUi.spiritExperience,expBounds,SpiritFeedAnimating?SpiritFeedProgress:spiritExperience/(float)Mathf.Max(1,SpiritExperienceRequired));
            AtlasLabel(19,164,176,33,SpiritFeedAnimating?"正在吸收……":SpiritFeedAnimationMessage ?? SpiritSkillName());
            var foods=Enumerable.Range(0,equipment.Count).Where(i=>i!=equippedIndex).ToArray();
            if(foods.Length>0 && !SpiritFeedAnimating)
            {
                atlasSelection=Mathf.Clamp(atlasSelection,0,foods.Length-1);
                if(AtlasClick(new Rect(22,181,173,24),equipment[foods[atlasSelection]].Data.name+" +"+equipment[foods[atlasSelection]].reinforcement))atlasSelection=(atlasSelection+1)%foods.Length;
                if(AtlasClick(new Rect(5,211,52,23)))SelectSpiritFood(foods[atlasSelection]);
            }
            AtlasLabel(92,211,165,23,"耐久 "+SpiritDurability+"/"+SpiritAttribute(1));
        }
        private void DrawAtlasRole()
        {
            DrawPreviewRoleEquipment(Equipped,new Rect(5,45,84,137));
            AtlasLabel(120,39,75,20,CurrentHp+"/"+MaximumHp);AtlasLabel(120,61,75,20,durability+"/"+Equipped.MaximumDurability);
            AtlasLabel(120,84,75,20,experience+"/"+(level*level*level*25));
            var f=fighters[-1];int[] values={f.attack,f.hit,f.dodge,f.defense,f.critical};
            for(int i=0;i<5;i++)GUI.Label(new Rect(263,38+i*24,42,20),values[i].ToString(),atlasNumber);
            AtlasLabel(100,170,195,28,"等级 "+level+"    "+(integrated?"合体":"普通"));
            AtlasLabel(210,183,95,29,equippedSkill>=0?OriginalAssets.Table<SkillDefinition>("skill").First(s=>s.id==equippedSkill).name:"无技能");
        }
        private void DrawAtlasSystem()
        {
            string[] names={"保存进度","读取进度","继续游戏","触屏："+(touchLayout&&touchLayout.visible?"开":"关"),"世界地图","回主菜单"};
            for(int i=0;i<names.Length;i++)
            {
                var rect=new Rect(82,37+i*28,148,20); var c=GUI.color;GUI.color=new Color(.30f,.24f,.18f);GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=c;
                if(!AtlasClick(new Rect(78,35+i*28,156,24),names[i]))continue;
                if(i==0)Save();else if(i==1)Load();else if(i==2)CloseMenu();else if(i==3&&touchLayout)touchLayout.visible=!touchLayout.visible;else if(i==4)SelectMenuPage(4);else if(i==5){menu=false;AtTitle=true;}
            }
        }
        private void DrawAtlasShop()
        {
            if(AtlasClick(new Rect(92,4,66,23)) && !scriptedMenu){shopSelling=false;atlasSelection=0;}
            if(AtlasClick(new Rect(161,4,66,23)) && !scriptedMenu){shopSelling=true;atlasSelection=0;}
            if(scriptedMenu)
            {
                if(AtlasClick(new Rect(230,180,73,25),"↑ 选择") && tutorialKey==0)RespondTutorial(0);
                AtlasLabel(232,48,70,130,OriginalAssets.Table<ItemDefinition>("item").First(i=>i.id==ShopSelectedItemId).name+"\n"+Clean(OriginalAssets.Table<ItemDefinition>("item").First(i=>i.id==ShopSelectedItemId).description));
            }
            int[] ids=shopType<2?OriginalAssets.Table<ItemDefinition>("item").Where(i=>shopSelling?ItemCount(i.id)>0:ItemForSale(i.id)).Select(i=>i.id).ToArray():shopSelling?Enumerable.Range(0,equipment.Count).Where(i=>i!=equippedIndex).ToArray():OriginalAssets.Table<EquipmentDefinition>("equip").Where(e=>e.grade<3&&e.level<=level).Select(e=>e.id).ToArray();
            atlasSelection=Mathf.Clamp(atlasSelection,0,Mathf.Max(0,ids.Length-1));
            if(scriptedMenu){atlasSelection=Array.IndexOf(ids,ShopSelectedItemId);atlasScroll.y=Mathf.Max(0,atlasSelection*31-120);}
            AtlasRows(ids.Length,new Rect(18,48,200,155),31,(i,y)=>{
                string name;int price;
                if(shopType<2){var item=OriginalAssets.Table<ItemDefinition>("item").First(a=>a.id==ids[i]);name=item.name;price=shopSelling?item.sell:ItemPurchasePrice(item.id);DrawItemIcon(item.id,new Rect(2,y+2,22,22));}
                else {var item=shopSelling?equipment[ids[i]].Data:OriginalAssets.Table<EquipmentDefinition>("equip").First(a=>a.id==ids[i]);name=item.name;price=shopSelling?item.sell:item.buy;DrawEquipmentIcon(item,new Rect(2,y+2,22,22));}
                if(AtlasClick(new Rect(0,y,183,29))&&!scriptedMenu)atlasSelection=i;
                AtlasLabel(28,y+2,155,27,(i==atlasSelection?"› ":"")+name+"  "+price);
            });
            if(ids.Length==0 || scriptedMenu)return;
            int selected=ids[atlasSelection];
            string desc=shopType<2?OriginalAssets.Table<ItemDefinition>("item").First(i=>i.id==selected).description:(shopSelling?equipment[selected].Data:OriginalAssets.Table<EquipmentDefinition>("equip").First(e=>e.id==selected)).description;
            AtlasLabel(232,47,70,133,Clean(desc));
            if(shopType>=2&&!shopSelling&&AtlasClick(new Rect(231,180,73,25),"试穿"))PreviewShopWeapon(selected);
            if(AtlasClick(new Rect(5,211,52,23))){if(shopType<2)SelectItemTrade(selected,shopSelling);else SelectWeaponTrade(selected,shopSelling);}
        }
        private void DrawAtlasTrade()
        {
            var t=trade; AtlasFrame(17);
            string name=t.weapon?OriginalAssets.Table<EquipmentDefinition>("equip").First(e=>e.id==t.id).name:OriginalAssets.Table<ItemDefinition>("item").First(i=>i.id==t.id).name;
            AtlasLabel(60,61,203,22,(t.selling?"出售 ":"购买 ")+name);
            if(AtlasClick(new Rect(65,88,28,24),"−"))SetTradeQuantity(t.quantity-1);
            AtlasLabel(100,91,82,22,"数量 "+t.quantity);
            if(AtlasClick(new Rect(180,88,28,24),"+"))SetTradeQuantity(t.quantity+1);
            AtlasLabel(62,115,194,22,"合计 "+TradePrice+" 两");
            if(AtlasClick(new Rect(65,139,80,22),"确认"))ConfirmTrade();
            if(AtlasClick(new Rect(174,139,80,22),"取消"))CancelTrade();
        }
        private void DrawAtlasFeed()
        {
            var food=spiritFood;AtlasFrame(17);
            AtlasLabel(60,61,204,22,"吞噬 "+food.Data.name+" +"+food.reinforcement);
            AtlasLabel(60,85,204,41,"经验 +"+SpiritFoodExperience(food)+"\n确认后消耗该武器");
            if(AtlasClick(new Rect(59,136,67,25),"继承吞噬"))ConfirmSpiritFeed(true);
            if(AtlasClick(new Rect(130,136,67,25),"保留技能"))ConfirmSpiritFeed(false);
            if(AtlasClick(new Rect(201,136,59,25),"取消"))CancelSpiritFeed();
        }
        private void DrawAtlasMap()
        {
            if(originalUi.worldMap)GUI.DrawTexture(new Rect(0,0,320,240),originalUi.worldMap,ScaleMode.StretchToFill);
            else AtlasFrame(4);
            AtlasLabel(10,8,220,23,"世界地图");
            for(int i=0;i<AreaNames.Length;i++)
            {
                GUI.enabled=openAreas[i];
                var r=new Rect(15+i%2*153,37+i/2*25,140,23);
                if(GUI.Button(r,AreaNames[i],buttonStyle))TravelToArea(i);
            }
            GUI.enabled=true;
            if(!worldMapOpen && AtlasClick(new Rect(263,211,52,23),"返回"))SelectMenuPage(0);
        }
    }
}
