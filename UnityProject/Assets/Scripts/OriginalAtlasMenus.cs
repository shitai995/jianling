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
            return clicked && !originalMessage;
        }
        private void AtlasLabel(float x,float y,float w,float h,string text) { GUI.Label(new Rect(x,y,w,h),text ?? "",atlasText); }
        private void AtlasColoredLabel(float x,float y,float w,float h,string text, Color color)
        {
            var old = atlasText.normal.textColor; atlasText.normal.textColor = color;
            AtlasLabel(x,y,w,h,text); atlasText.normal.textColor = old;
        }
        private void AtlasFrame(int action) { OriginalGui.Animation(originalUi.menu,originalUi.menuSheets,action,originalUiTicks,new Vector2(160,120)); }
        private void DrawAtlasMenu()
        {
            AtlasStyles();
            if (atlasPage != menuPage) { atlasSelection = 0; atlasScroll = Vector2.zero; atlasPage = menuPage; }
            var previous = GUI.matrix;
            var bounds = originalUi.menuBounds;
            GUI.matrix = previous * Matrix4x4.TRS(new Vector3(bounds.x,bounds.y,0),Quaternion.identity,new Vector3(bounds.width/320,bounds.height/240,1));
            try
            {
                if (systemMenuPage != SystemMenuPage.None) { DrawSystemSubpage(); return; }
                if (menuPage == 4) { DrawAtlasMap(); return; }
                int native = menuPage == 7 ? 0 : menuPage == 2 ? 1 : menuPage == 1 ? 2 : menuPage == 6 ? 3 : menuPage == 0 ? 4 : 5;
                if (menuPage == 6) { AtlasFrame(23); AtlasFrame(24 + SpiritFeedFace); }
                if(native==0)AtlasFrame(1);
                AtlasFrame(menuPage == 5 ? (shopSelling ? 14 : 13) : menuPage == 0 && atlasBranch ? 7 : menuPage == 6 && spiritFoodChoosing ? 6 : menuPage == 3 && atlasSelection > 0 ? atlasSelection + 7 : native);
                bool modal = TradeConfirming || SpiritFeedAnimating || itemConfirming || spiritMenuExitTicks > 0;
                GUI.enabled = !modal;
                if (menuPage != 5)
                {
                    int[] pages = {7,2,1,6,0,3};
                    for (int i=0;i<6;i++)
                        if (AtlasClick(new Rect(54+i*26+(i>native?50:0),2,i==native?76:24,24))) SelectMenuPage(pages[i]);
                }
                if (AtlasClick(new Rect(263,211,52,23))) HandleOriginalMenuKey(10);
                if (menuPage == 2) DrawAtlasEquipment();
                else if (menuPage == 1 && (!inventoryView || !inventoryView.ReplacesInventory || !NativeInventoryVisible)) DrawAtlasItems();
                else if (menuPage == 0) DrawAtlasMission();
                else if (menuPage == 6) DrawAtlasSpirit();
                else if (menuPage == 5) DrawAtlasShop();
                else if (menuPage == 7) DrawAtlasRole();
                else DrawAtlasSystem();
                GUI.enabled = true;
                if (TradeConfirming) DrawAtlasTrade();
                if (SpiritFeedConfirming) DrawAtlasFeed();
                if (itemConfirming) DrawAtlasItemConfirmation();
                if (menuPage == 6 && SpiritFeedAnimating && AtlasClick(new Rect(263,211,52,23))) HandleOriginalMenuKey(10);
                if (menuPage != 6) AtlasLabel(196,220,64,18,money+" 两");
            }
            finally { GUI.enabled = true; GUI.matrix = previous; }
        }
        private void AtlasRows(int count,Rect bounds,int rowHeight,Action<int,float> draw)
        {
            if (count > 0)
            {
                float top = atlasSelection * rowHeight;
                if (top < atlasScroll.y) atlasScroll.y = top;
                else if (top + rowHeight > atlasScroll.y + bounds.height) atlasScroll.y = top + rowHeight - bounds.height;
            }
            atlasScroll=GUI.BeginScrollView(bounds,atlasScroll,new Rect(0,0,bounds.width-18,Mathf.Max(bounds.height,count*rowHeight)));
            for(int i=0;i<count;i++) draw(i,i*rowHeight);
            GUI.EndScrollView();
        }
        private void DrawAtlasEquipment()
        {
            if(Equipment.Count==0) return;
            atlasSelection=Mathf.Clamp(atlasSelection,0,Equipment.Count-1);
            AtlasRows(Equipment.Count,new Rect(82,39,113,124),31,(i,y)=>{
                if(i==atlasSelection) { var c=GUI.color; GUI.color=new Color(.78f,.71f,.56f);GUI.DrawTexture(new Rect(0,y,98,26),Texture2D.whiteTexture);GUI.color=c; }
                DrawEquipmentIcon(Equipment[i].Data,new Rect(2,y+2,22,22));
                if(AtlasClick(new Rect(25,y,72,29))) atlasSelection=i;
                GUI.Label(new Rect(25,y,72,29),Equipment[i].Data.name+(Equipment[i].reinforcement>0?" +"+Equipment[i].reinforcement:""),i==atlasSelection?atlasNumber:atlasText);
            });
            var item=Equipment[atlasSelection];
            int[] values={item.Hp,item.Attack,item.MaximumDurability,item.Defense,item.Data.hit,item.Data.dodge,item.Data.critical};
            if (spiritFoodChoosing) DrawAtlasSpiritCandidates();
            else for(int i=0;i<7;i++) GUI.Label(new Rect(258,38+i*24,46,20),values[i].ToString(),atlasNumber);
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
            var stacks=Inventory.StackRows();
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
                if(AtlasClick(new Rect(5,211,52,23)))OpenItemConfirmation(id,stacks[atlasSelection][1]);
            }
            if(AtlasClick(new Rect(229,194,74,16),"用药 "+autoRecoverPercent+"%"))SetAutoRecoverPercent(autoRecoverPercent==90?10:autoRecoverPercent+10);
        }
        private void DrawAtlasItemConfirmation()
        {
            AtlasFrame(15 + itemConfirmChoice);
            if (AtlasClick(new Rect(108, 99, 52, 42)))
            {
                itemAction.Select(0);
                ConfirmItemConfirmation();
            }
            else if (AtlasClick(new Rect(160, 99, 52, 42)))
            {
                itemAction.Select(1);
                ConfirmItemConfirmation();
            }
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
            if (spiritFoodChoosing) DrawAtlasSpiritCandidates();
            else for(int i=0;i<7;i++)
            {
                float y=52+i*21;
                OriginalGui.Number(originalUi.menuNumbers,GetSpiritAttribute(i),new Vector2(SpiritFeedCompleted?234:257,y),10,2,SpiritFeedCompleted?-2:0);
                int gain=SpiritFeedAttributeGain(i);
                if(SpiritFeedCompleted && gain>0)
                {
                    var arrow=originalUi.attributeGainArrow;
                    if(arrow)GUI.DrawTexture(new Rect(254,y-Mathf.FloorToInt(Time.unscaledTime/.08f)%2,arrow.width,arrow.height),arrow);
                    OriginalGui.Number(originalUi.attributeGainNumbers,gain,new Vector2(285,y),10,2,-2);
                }
            }
            OriginalGui.Number(originalUi.menuNumbers,SpiritLevel,new Vector2(68,135),10,2,-2);
            var expBounds=new Rect(19,150,originalUi.spiritExperience.width,originalUi.spiritExperience.height);
            OriginalGui.Bar(originalUi.spiritExperience,expBounds,SpiritFeedProgress);
            DrawSpiritMenuInfo();
            if (!SpiritFeedAnimating && spiritMenuExitTicks == 0 && AtlasClick(new Rect(5,211,52,23))) HandleOriginalMenuKey(4);
            if (SpiritFeedAnimating)
            {
                OriginalGui.Animation(originalUi.menu,originalUi.menuSheets,21,Mathf.FloorToInt(Time.unscaledTime/.08f),new Vector2(19+expBounds.width*SpiritFeedProgress,160));
            }
            if (SpiritFeedFlashFrame >= 0) OriginalGui.Animation(originalUi.menu,originalUi.menuSheets,30,SpiritFeedFlashFrame,new Vector2(160,120));
            var skill=OriginalAssets.Table<SkillDefinition>("skill").FirstOrDefault(s=>s.id==spiritSkill);
            if(skill!=null)
            {
                int icon=skill.BehaviorId-5;if(icon==6)icon=7;else if(icon==7)icon=6;
                OriginalGui.Region(originalUi.skillIcons,new Rect(icon*22,0,22,22),new Rect(111,211,22,22));
            }
            GUI.Label(new Rect(135,211,64,24),SpiritSkillName(),atlasTitle);
        }
        private void DrawAtlasRole()
        {
            DrawPreviewRoleEquipment(Equipped,new Rect(5,45,84,137));
            OriginalGui.Frame(originalUi.menu, originalUi.menuSheets, 2, new Vector2(90, 60));
            if (AtlasClick(new Rect(5, 45, 84, 137))) HandleOriginalMenuKey(18);
            AtlasLabel(120,39,75,20,CurrentHp+"/"+MaximumHp);AtlasLabel(120,61,75,20,durability+"/"+Equipped.MaximumDurability);
            AtlasLabel(120,84,75,20,experience+"/"+(level*level*level*25));
            var f=fighters[-1];int[] values={f.attack,f.hit,f.dodge,f.defense,f.critical};
            for(int i=0;i<5;i++)GUI.Label(new Rect(263,38+i*24,42,20),values[i].ToString(),atlasNumber);
            AtlasLabel(100,170,195,28,"等级 "+level+"    "+(integrated?"合体":"普通"));
            AtlasLabel(210,183,95,29,equippedSkill>=0?OriginalAssets.Table<SkillDefinition>("skill").First(s=>s.id==equippedSkill).name:"无技能");
        }
        private void DrawAtlasSystem()
        {
            for (int i = 0; i < 6; i++)
            {
                if (AtlasClick(new Rect(77, 39 + i * 28, 164, 22)))
                {
                    atlasSelection = systemSelection = i;
                    ActivateSystemSelection();
                }
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
            int[][] itemRows=shopType<2?(shopSelling?ShopItemRows():OriginalAssets.Table<ItemDefinition>("item").Where(i=>ItemForSale(i.id)).Select(i=>new[]{i.id,-1}).ToArray()):null;
            int[] equipmentRows=shopType>=2?(shopSelling?Enumerable.Range(0,Equipment.Count).Where(i=>i!=EquippedIndex).ToArray():OriginalAssets.Table<EquipmentDefinition>("equip").Where(e=>e.grade<3&&e.level<=level).Select(e=>e.id).ToArray()):null;
            int rowCount=shopType<2?itemRows.Length:equipmentRows.Length;
            atlasSelection=Mathf.Clamp(atlasSelection,0,Mathf.Max(0,rowCount-1));
            if(scriptedMenu){atlasSelection=shopType<2?Array.FindIndex(itemRows,r=>r[0]==ShopSelectedItemId):Array.IndexOf(equipmentRows,ShopSelectedItemId);atlasSelection=Mathf.Max(0,atlasSelection);atlasScroll.y=Mathf.Max(0,atlasSelection*31-120);}
            AtlasRows(rowCount,new Rect(18,48,205,155),31,(i,y)=>{
                string name; int price, gradeOrType; Rect icon = new Rect(23,y+2,22,22);
                int itemId=-1, stack=-1;
                if(shopType<2){itemId=itemRows[i][0];stack=itemRows[i][1];var item=OriginalAssets.Table<ItemDefinition>("item").First(a=>a.id==itemId);name=item.name;price=shopSelling?item.sell:ItemPurchasePrice(item.id);gradeOrType=item.type;DrawShopIcon(item.id, gradeOrType, icon);}
                else {var item=shopSelling?Equipment[equipmentRows[i]].Data:OriginalAssets.Table<EquipmentDefinition>("equip").First(a=>a.id==equipmentRows[i]);name=item.name;price=shopSelling?item.sell:item.buy;gradeOrType=item.grade;DrawShopIcon(item, gradeOrType, icon);}
                if(i==atlasSelection){var c=GUI.color;GUI.color=new Color32(198,181,142,255);GUI.DrawTexture(new Rect(22,y,187,26),Texture2D.whiteTexture);GUI.color=c;}
                if(AtlasClick(new Rect(0,y,187,29))&&!scriptedMenu)atlasSelection=i;
                AtlasColoredLabel(52,y+4,150,22,name,ShopGradeColor(gradeOrType));
                if(shopSelling && shopType<2) OriginalGui.Number(originalUi.stackNumbers ? originalUi.stackNumbers : originalUi.menuNumbers, ItemStackQuantity(itemId,stack), new Vector2(42,y+17), 10, 2, -1);
                OriginalGui.Price(originalUi, price, new Vector2(208,y+8), 1);
            });
            if(rowCount>0 && originalUi.menuScrollSign)
            {
                int offset = rowCount > 1 ? (139-originalUi.menuScrollSign.height)*atlasSelection/(rowCount-1) : 0;
                GUI.DrawTexture(new Rect(216,54+offset,originalUi.menuScrollSign.width,originalUi.menuScrollSign.height),originalUi.menuScrollSign);
            }
            if(rowCount==0 || scriptedMenu)return;
            int selected=shopType<2?itemRows[atlasSelection][0]:equipmentRows[atlasSelection];
            string desc=shopType<2?OriginalAssets.Table<ItemDefinition>("item").First(i=>i.id==selected).description:(shopSelling?Equipment[selected].Data:OriginalAssets.Table<EquipmentDefinition>("equip").First(e=>e.id==selected)).description;
            AtlasLabel(232,47,70,133,Clean(desc));
            if(shopType>=2&&!shopSelling&&AtlasClick(new Rect(231,180,73,25),"试穿"))PreviewShopWeapon(selected);
            if(AtlasClick(new Rect(5,211,52,23))){if(shopType<2)SelectItemTrade(selected,shopSelling,itemRows[atlasSelection][1]);else SelectWeaponTrade(selected,shopSelling);}
        }
        private void DrawAtlasTrade()
        {
            var t=trade; AtlasFrame((t.selling ? 19 : 17) + t.choice);
            bool limited=t.weapon || (!t.selling && t.id>=16);
            const int center=160;
            const int canvasOffsetY=(320-240)/2;
            int arrowHeight=originalUi.transactionArrow?originalUi.transactionArrow.height:0;
            int fontHeight=Mathf.CeilToInt(atlasText.CalcSize(new GUIContent("测")).y);
            if(!limited)
            {
                float labelWidth=atlasText.CalcSize(new GUIContent("数量：")).x;
                int quantityY=arrowHeight+75+20-canvasOffsetY;
                AtlasLabel(center-labelWidth,quantityY,labelWidth,Mathf.Max(18,fontHeight),"数量：");
                string quantityText=t.quantity.ToString();
                float quantityWidth=atlasText.CalcSize(new GUIContent(quantityText)).x;
                AtlasLabel(center+5,quantityY,quantityWidth,Mathf.Max(18,fontHeight),quantityText);
                if(originalUi.transactionArrow)
                {
                    int wobble=originalUiTicks%2;
                    float arrowX=center+5+(quantityWidth-originalUi.transactionArrow.width)*.5f;
                    float upperY=75-wobble+20-canvasOffsetY;
                    var upper=new Rect(arrowX,upperY,originalUi.transactionArrow.width,originalUi.transactionArrow.height);
                    if(AtlasClick(upper))MoveTradeQuantity(-1);
                    GUI.DrawTexture(upper,originalUi.transactionArrow);
                    float lowerY=fontHeight+75+originalUi.transactionArrow.height+wobble+20-canvasOffsetY;
                    var lower=new Rect(arrowX,lowerY,originalUi.transactionArrow.width,originalUi.transactionArrow.height);
                    if(AtlasClick(lower))MoveTradeQuantity(1);
                    var matrix=GUI.matrix;
                    GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(0,2*lowerY+originalUi.transactionArrow.height,0),Quaternion.identity,new Vector3(1,-1,1));
                    GUI.DrawTexture(new Rect(arrowX,lowerY,originalUi.transactionArrow.width,originalUi.transactionArrow.height),originalUi.transactionArrow);
                    GUI.matrix=matrix;
                }
            }
            else
            {
                string prompt=t.weapon?"是否购买武器？":"每次限购1个，是否购买？";
                float promptWidth=atlasText.CalcSize(new GUIContent(prompt)).x;
                AtlasLabel((320-promptWidth)*.5f,(arrowHeight<<1)+75+20-canvasOffsetY,promptWidth,Mathf.Max(18,fontHeight),prompt);
            }
            int rowY=(arrowHeight<<1)+75+fontHeight+3+20-canvasOffsetY;
            float totalWidth=atlasText.CalcSize(new GUIContent("总额：")).x;
            AtlasLabel(center-totalWidth,rowY,totalWidth,Mathf.Max(18,fontHeight),"总额：");
            int priceY=rowY+Mathf.Max(0,(fontHeight-(originalUi.menuGoldNumbers?originalUi.menuGoldNumbers.height:originalUi.menuNumbers.height))>>1);
            OriginalGui.Price(originalUi,TradePrice,new Vector2(center+5,priceY),0);
            if(AtlasClick(new Rect(110,134,50,20)))ConfirmTrade();
            if(AtlasClick(new Rect(174,134,50,20)))CancelTrade();
        }
        private Color ShopGradeColor(int gradeOrType)
        {
            switch (Mathf.Clamp(gradeOrType,0,4))
            {
                case 1: return new Color32(100,220,110,255);
                case 2: return new Color32(150,190,255,255);
                case 3: return new Color32(220,120,245,255);
                case 4: return new Color32(255,180,60,255);
                default: return new Color32(245,235,205,255);
            }
        }
        private void DrawShopIcon(int id,int gradeOrType,Rect bounds)
        {
            if(originalUi && originalUi.iconBackgrounds!=null && gradeOrType>=0 && gradeOrType<originalUi.iconBackgrounds.Length && originalUi.iconBackgrounds[gradeOrType]) GUI.DrawTexture(bounds,originalUi.iconBackgrounds[gradeOrType]);
            DrawItemIcon(id,new Rect(bounds.x+2,bounds.y+2,bounds.width-4,bounds.height-4));
        }
        private void DrawShopIcon(EquipmentDefinition data,int gradeOrType,Rect bounds)
        {
            if(originalUi && originalUi.iconBackgrounds!=null && gradeOrType>=0 && gradeOrType<originalUi.iconBackgrounds.Length && originalUi.iconBackgrounds[gradeOrType]) GUI.DrawTexture(bounds,originalUi.iconBackgrounds[gradeOrType]);
            DrawEquipmentIcon(data,new Rect(bounds.x+2,bounds.y+2,bounds.width-4,bounds.height-4));
        }
        private string spiritInfoSource;
        private OriginalDialogueText.Line[] spiritInfoLines;
        private GUIStyle spiritInfoStyle;
        private int spiritInfoLineHeight, spiritInfoOffset;
        private float spiritInfoElapsed;

        private void DrawSpiritMenuInfo()
        {
            string source = SpiritFeedAnimationMessage ?? "";
            if (spiritInfoLines == null || spiritInfoSource != source)
            {
                int size = originalUi ? originalUi.dialogueFontSize : 18;
                spiritInfoStyle = new GUIStyle { font = font, fontSize = size, richText = true, wordWrap = false,
                    padding = new RectOffset(0, 0, 0, 0), alignment = TextAnchor.UpperLeft };
                spiritInfoStyle.normal.textColor = Color.white;
                spiritInfoLineHeight = Mathf.CeilToInt(font.lineHeight * (size / (float)Mathf.Max(1, font.fontSize))) + 2;
                Func<string, int> measure = text => (int)spiritInfoStyle.CalcSize(new GUIContent(text)).x;
                spiritInfoLines = OriginalDialogueText.Format(source, 140, measure("你"), measure);
                spiritInfoOffset = spiritInfoLines.Length * spiritInfoLineHeight > 38 ? spiritInfoLineHeight : 0;
                spiritInfoElapsed = 0; spiritInfoSource = source;
            }
            GUI.BeginGroup(new Rect(18, 164, 141, 39));
            for (int i = 0; i < spiritInfoLines.Length; i++)
            {
                float x = 0, y = spiritInfoOffset + i * spiritInfoLineHeight;
                foreach (var span in spiritInfoLines[i].spans)
                {
                    OriginalGui.StyledLabel(new Rect(x, y, span.width + 4, spiritInfoLineHeight + 4), span.drawRuns, spiritInfoStyle);
                    x += span.width;
                }
            }
            GUI.EndGroup();
        }

        private void TickSpiritMenuInfo(float delta)
        {
            if (!menu || menuPage != 6 || spiritInfoLines == null || spiritInfoSource != (SpiritFeedAnimationMessage ?? "") ||
                spiritInfoLines.Length * spiritInfoLineHeight <= 38) return;
            spiritInfoElapsed += delta;
            while (spiritInfoElapsed >= .08f)
            {
                spiritInfoElapsed -= .08f;
                spiritInfoOffset -= spiritInfoLineHeight / 10;
                if (spiritInfoOffset <= -spiritInfoLines.Length * spiritInfoLineHeight) spiritInfoOffset = 38;
            }
        }

        private void DrawAtlasSpiritCandidates()
        {
            var foods = SpiritFoodCandidates();
            if (foods.Length == 0) return;
            atlasSelection = Mathf.Clamp(atlasSelection, 0, foods.Length - 1);
            int first = Mathf.Max(0, atlasSelection - 4);
            int row = atlasSelection - first;
            var old = GUI.color;
            GUI.color = new Color32(198, 181, 142, 255);
            GUI.DrawTexture(new Rect(184, 45 + row * 31, 107, 26), Texture2D.whiteTexture);
            GUI.color = old;
            for (int i = first; i < Mathf.Min(foods.Length, first + 5); i++)
            {
                var item = Equipment[foods[i]];
                float y = 45 + (i - first) * 31;
                int grade = Mathf.Clamp(item.Data.grade, 0, 4);
                if (originalUi && originalUi.iconBackgrounds != null && grade < originalUi.iconBackgrounds.Length && originalUi.iconBackgrounds[grade])
                    GUI.DrawTexture(new Rect(185, y, 26, 26), originalUi.iconBackgrounds[grade]);
                DrawEquipmentIcon(item.Data, new Rect(187, y + 2, 22, 22));
                var oldColor = atlasNumber.normal.textColor;
                atlasNumber.normal.textColor = AtlasEquipmentGradeColor(grade);
                GUI.Label(new Rect(212, y, 79, 26), item.Data.name + (item.reinforcement > 0 ? " + " + item.reinforcement : ""), atlasNumber);
                atlasNumber.normal.textColor = oldColor;
                if (AtlasClick(new Rect(184, y, 107, 26)))
                {
                    atlasSelection = i;
                    if (SpiritFeedConfirming) spiritFood = item;
                }
            }
            var sign = originalUi.menuScrollSign;
            if (sign)
            {
                int offset = foods.Length > 1 ? (139 - sign.height) * atlasSelection / (foods.Length - 1) : 0;
                GUI.DrawTexture(new Rect(296, 51 + offset, sign.width, sign.height), sign);
            }
        }

        private static Color AtlasEquipmentGradeColor(int grade)
        {
            switch (Mathf.Clamp(grade, 0, 4))
            {
                case 1: return new Color32(0, 255, 0, 255);
                case 2: return new Color32(0, 204, 255, 255);
                case 3: return new Color32(255, 0, 255, 255);
                case 4: return new Color32(249, 161, 47, 255);
                default: return Color.white;
            }
        }

        private void DrawAtlasFeed()
        {
            OriginalGui.TiledPanel(originalUi.messagePanel, new Rect(10, 120, 300, 120));
            var previous = atlasText.normal.textColor;
            atlasText.normal.textColor = Color.black;
            AtlasLabel(14, 128, 292, 76, spiritFeedTip);
            atlasText.normal.textColor = Color.red;
            AtlasLabel(12, 216, 24, 22, "否");
            AtlasLabel(290, 216, 24, 22, "是");
            atlasText.normal.textColor = previous;
            if (AtlasClick(new Rect(10, 208, 52, 32))) HandleOriginalMenuKey(9);
            if (AtlasClick(new Rect(258, 208, 52, 32))) HandleOriginalMenuKey(10);
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
                if(GUI.Button(r,AreaNames[i],buttonStyle) && !originalMessage)TravelToArea(i);
            }
            GUI.enabled=true;
            if(!worldMapOpen && AtlasClick(new Rect(263,211,52,23),"返回"))SelectMenuPage(0);
        }
    }
}
