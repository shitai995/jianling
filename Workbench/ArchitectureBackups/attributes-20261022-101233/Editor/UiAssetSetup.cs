using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GuJian.Editor
{
    public static class UiAssetSetup
    {
        [MenuItem("古剑/补充原版界面资源（保留已有编辑）")]
        public static void Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play first");
            const string path="Assets/Resources/OriginalUiLayout.asset";
            var ui=AssetDatabase.LoadAssetAtPath<OriginalUiLayout>(path);
            if (!ui)
            {
                ui=ScriptableObject.CreateInstance<OriginalUiLayout>();
                ui.scene=OriginalAssets.Json<OriginalAnimation>("Animations/action/u_00");
                ui.sceneSheets=Array.ConvertAll(new[]{"u_01","u_03","u_05","u_00","jiantou","ui","help"},s=>OriginalAssets.Texture("action/"+s));
                ui.health=Pic("u_23"); ui.durability=Pic("u_16"); ui.experience=Pic("u_08"); ui.energy=Pic("u_26");
                ui.bossHealth=Pic("u_25"); ui.spiritDurability=Pic("u_07"); ui.roleHead=Pic("u_37"); ui.spiritHead=Pic("u_38"); ui.dialogue=Pic("u_28");
                ui.itemIcons=Pic("item"); ui.weaponIcons=Pic("weapon"); ui.skillIcons=Pic("skill"); ui.combo=Pic("u_12"); ui.comboNumbers=Pic("u_30"); ui.comboBonus=Pic("u_29");
                string[] names={"古小天","灵儿","秦天虎","叶秋心","楚山南","天草"};
                ui.portraits=new OriginalUiLayout.Portrait[names.Length];
                for (int i=0;i<names.Length;i++) ui.portraits[i]=new OriginalUiLayout.Portrait{speaker=names[i],texture=Pic("f_"+i.ToString("00"))};
                AssetDatabase.CreateAsset(ui,path);
            }
            if (ui.menu == null || ui.menu.actions == null || ui.menu.actions.Length == 0)
            {
                ui.menu=OriginalAssets.Json<OriginalAnimation>("Animations/action/u_01");
                ui.menuSheets=Array.ConvertAll(new[]{"u_04","u_02","u_06","u_11","s_34","f_07","u_00","u_46"},s=>OriginalAssets.Texture("action/"+s));
                ui.worldMap=Pic("m_87"); ui.spiritExperience=Pic("u_09");
                EditorUtility.SetDirty(ui);
            }
            if (!ui.messagePanel) ui.messagePanel=Pic("u_40");
            if (!ui.menuScrollSign) ui.menuScrollSign=Pic("u_27");
            FillMessageHint(ui);
            FillSystemMenus(ui);
            FillPreview(ref ui.criticalHitOverlay, "s_32", new[] { "s_34" });
            if (!ui.menuNumbers) ui.menuNumbers=Pic("u_19");
            if (!ui.menuGoldNumbers) ui.menuGoldNumbers=Pic("u_17");
            if (!ui.menuSilverNumbers) ui.menuSilverNumbers=Pic("u_18");
            if (!ui.stackNumbers) ui.stackNumbers=Pic("u_31");
            if (!ui.money) ui.money=Pic("money");
            if (!ui.transactionArrow) ui.transactionArrow=Pic("u_42");
            if (ui.iconBackgrounds == null || ui.iconBackgrounds.Length < 5)
            {
                ui.iconBackgrounds = FillTextures(ui.iconBackgrounds, new[]{"i_01","i_02","i_03","i_04","i_05"});
            }
            if (!ui.attributeGainNumbers) ui.attributeGainNumbers=Pic("u_41");
            if (!ui.attributeGainArrow) ui.attributeGainArrow=Pic("u_42");
            if (!ui.skillIcons || ui.skillIcons.name=="skillandrod") ui.skillIcons=Pic("skill");
            if(!ui.shortcutIcons) ui.shortcutIcons=Pic("skillandrod");
            ui.comboFrames=FillTextures(ui.comboFrames,new[]{"u_12","u_13","u_14"},ui.combo);
            ui.comboCountNumbers=FillTextures(ui.comboCountNumbers,new[]{"u_20","u_21","u_22"});
            ui.combatNumbers=FillTextures(ui.combatNumbers,new[]{"u_33","u_34","u_35","u_36"});
            if(!ui.combatMiss) ui.combatMiss=Pic("miss");
            EditorUtility.SetDirty(ui);
            var root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/GuJianGame.prefab");
            Bind(root.GetComponent<GuJianGame>(),ui);
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/GuJianGame.prefab"); PrefabUtility.UnloadPrefabContents(root);
            var scene=EditorSceneManager.GetActiveScene();
            if(scene.path=="Assets/Scenes/GuJian.unity") { Bind(UnityEngine.Object.FindObjectOfType<GuJianGame>(),ui); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); }
            AssetDatabase.SaveAssets();
        }
        public static void PrepareMessagePanel()
        {
            var ui=AssetDatabase.LoadAssetAtPath<OriginalUiLayout>("Assets/Resources/OriginalUiLayout.asset");
            if (!ui) throw new InvalidOperationException("UI asset missing");
            bool changed=false;
            if (!ui.messagePanel) { ui.messagePanel=Pic("u_40"); changed=true; }
            changed=FillMessageHint(ui)||changed;
            if (changed) { EditorUtility.SetDirty(ui); AssetDatabase.SaveAssets(); }
        }
        public static bool MigrateOriginalDialogueBounds()
        {
            var ui=AssetDatabase.LoadAssetAtPath<OriginalUiLayout>("Assets/Resources/OriginalUiLayout.asset");
            if (!ui) throw new InvalidOperationException("UI asset missing");
            // Only replace the previous reconstruction default; preserve user-authored layouts.
            if (ui.dialogueBounds != new Rect(10,218,513,102) && ui.dialogueBounds != new Rect(133,230,267,90)) return false;
            int width=ui.dialogue ? ui.dialogue.width : 268, height=ui.dialogue ? ui.dialogue.height : 90;
            ui.dialogueBounds=new Rect((533-width)>>1,320-height,width,height);
            EditorUtility.SetDirty(ui); AssetDatabase.SaveAssets(); return true;
        }
        private static bool FillSkillPreviews(OriginalUiLayout ui)
        {
            bool changed = false;
            if (ui.skillPreviewEffects == null) { ui.skillPreviewEffects = new OriginalUiLayout.PreviewAnimation[9]; changed = true; }
            if (ui.skillPreviewEffects.Length < 9) { Array.Resize(ref ui.skillPreviewEffects, 9); changed = true; }
            string[][] sheets = { new[] { "s_20" }, new[] { "s_19" }, new[] { "s_21" }, new[] { "s_22" },
                new[] { "s_20" }, new[] { "s_11", "s_19", "s_08" }, new[] { "s_21" }, new[] { "s_22" }, new[] { "s_24" } };
            for (int i = 0; i < 9; i++)
                changed = FillPreview(ref ui.skillPreviewEffects[i], "skill_" + (i < 8 ? i + 5 : 14).ToString("00"), sheets[i]) || changed;
            changed = FillPreview(ref ui.skillPreviewSupport, "s_41", new[] { "s_41", "h_04" }) || changed;
            return changed;
        }
        private static bool FillPreview(ref OriginalUiLayout.PreviewAnimation preview, string animation, string[] names)
        {
            bool changed = false;
            if (preview == null) { preview = new OriginalUiLayout.PreviewAnimation(); changed = true; }
            if (preview.animation == null || preview.animation.actions == null || preview.animation.actions.Length == 0)
            { preview.animation = OriginalAssets.Json<OriginalAnimation>("Animations/action/" + animation); changed = true; }
            if (preview.sheets == null) { preview.sheets = new Texture2D[names.Length]; changed = true; }
            if (preview.sheets.Length < names.Length) { Array.Resize(ref preview.sheets, names.Length); changed = true; }
            for (int i = 0; i < names.Length; i++)
                if (!preview.sheets[i]) { preview.sheets[i] = OriginalAssets.Texture("action/" + names[i]); changed = true; }
            return changed;
        }
        public static void PrepareSystemMenus()
        {
            var ui = AssetDatabase.LoadAssetAtPath<OriginalUiLayout>("Assets/Resources/OriginalUiLayout.asset");
            if (!ui) throw new InvalidOperationException("UI asset missing");
            if (FillSystemMenus(ui)) { EditorUtility.SetDirty(ui); AssetDatabase.SaveAssets(); }
        }
        public static void PrepareCriticalHitOverlay()
        {
            var ui = AssetDatabase.LoadAssetAtPath<OriginalUiLayout>("Assets/Resources/OriginalUiLayout.asset");
            if (!ui) throw new InvalidOperationException("UI asset missing");
            if (FillPreview(ref ui.criticalHitOverlay, "s_32", new[] { "s_34" }))
            { EditorUtility.SetDirty(ui); AssetDatabase.SaveAssets(); }
        }
        private static bool FillSystemMenus(OriginalUiLayout ui)
        {
            bool changed = FillSkillPreviews(ui);
            if (ui.mainMenu == null || ui.mainMenu.frames == null || ui.mainMenu.frames.Length == 0)
            { ui.mainMenu = OriginalAssets.Json<OriginalAnimation>("Animations/action/u_02"); changed = true; }
            if (ui.mainMenuSheets == null || ui.mainMenuSheets.Length == 0)
            { ui.mainMenuSheets = Array.ConvertAll(new[] { "u_02", "u_00", "u_43", "u_45" }, s => OriginalAssets.Texture("action/" + s)); changed = true; }
            if (!ui.settingsArrow) { ui.settingsArrow = Pic("u_44"); changed = true; }
            if (ui.marketplaceOptions == null || ui.marketplaceOptions.Length == 0)
            {
                ui.marketplaceOptions = Array.ConvertAll(new[] { "sm0", "sm1", "sm2", "sm3", "sm4_1", "sm4_2", "sm0_1", "sm1_1", "sm2_1", "sm3_1", "sm4_1_1", "sm4_2_1" }, Pic);
                changed = true;
            }
            if (ui.marketplaceInfo == null || ui.marketplaceInfo.Length == 0)
            { ui.marketplaceInfo = Array.ConvertAll(new[] { "sminfo_0", "sminfo_1", "sminfo_2", "sminfo_3", "sminfo_4", "sminfo_0_1" }, Pic); changed = true; }
            return changed;
        }
        private static bool FillMessageHint(OriginalUiLayout ui)
        {
            bool changed=false;
            if (ui.keyPressTip == null || ui.keyPressTip.actions == null || ui.keyPressTip.actions.Length == 0)
            { ui.keyPressTip=OriginalAssets.Json<OriginalAnimation>("Animations/action/jiantou"); changed=true; }
            if (ui.keyPressTipSheets == null || ui.keyPressTipSheets.Length == 0)
            { ui.keyPressTipSheets=new[]{OriginalAssets.Texture("action/jiantou")}; changed=true; }
            else if (!ui.keyPressTipSheets[0])
            { ui.keyPressTipSheets[0]=OriginalAssets.Texture("action/jiantou"); changed=true; }
            return changed;
        }
        private static Texture2D Pic(string name)=>OriginalAssets.Texture("pic/"+name);
        private static Texture2D[] FillTextures(Texture2D[] textures,string[] names,Texture2D first=null)
        {
            if(textures==null) textures=new Texture2D[names.Length];
            if(textures.Length<names.Length) Array.Resize(ref textures,names.Length);
            for(int i=0;i<names.Length;i++) if(!textures[i]) textures[i]=i==0&&first?first:Pic(names[i]);
            return textures;
        }
        private static void Bind(GuJianGame game,OriginalUiLayout ui)
        {
            var so=new SerializedObject(game); if(so.FindProperty("originalUi").objectReferenceValue) return;
            so.FindProperty("originalUi").objectReferenceValue=ui;so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
