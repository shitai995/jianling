using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public sealed class MapCollisionEditor : EditorWindow
    {
        private OriginalAssetCatalog catalog;
        private int selected;
        private Vector2 scroll;
        private float zoom = .65f;
        private bool paintBlocked = true, grid = true, painting;
        private int stroke = -1;
        [MenuItem("古剑/地图碰撞编辑器")]
        public static void Open() { GetWindow<MapCollisionEditor>("地图碰撞").Show(); }
        private void OnEnable() { catalog = AssetDatabase.LoadAssetAtPath<OriginalAssetCatalog>("Assets/Resources/OriginalAssetCatalog.asset"); Undo.undoRedoPerformed += Repaint; }
        private void OnDisable() { Undo.undoRedoPerformed -= Repaint; }
        public static void SetCell(OriginalAssetCatalog target, int mapIndex, int col, int row, bool blocked)
        {
            var map = target.maps[mapIndex].data;
            if (col < 0 || row < 0 || col >= map.columns || row >= map.rows) throw new ArgumentOutOfRangeException("cell");
            Undo.RecordObject(target, "绘制地图碰撞");
            map.cells[col * map.rows + row].blocked = blocked;
            EditorUtility.SetDirty(target);
        }
        public static void ImportCollision(OriginalAssetCatalog target, int mapIndex, string json)
        {
            var source = JsonUtility.FromJson<OriginalMap>(json); var map = target.maps[mapIndex].data;
            if (source == null || source.id != map.id || source.columns != map.columns || source.rows != map.rows || source.cellWidth != map.cellWidth || source.cellHeight != map.cellHeight || source.cells == null || source.cells.Length != map.cells.Length || source.cells.Any(c=>c==null)) throw new InvalidDataException("地图标识、尺寸或格子数量不匹配");
            Undo.RecordObject(target, "导入地图碰撞");
            for (int i=0;i<map.cells.Length;i++) map.cells[i].blocked=source.cells[i].blocked;
            EditorUtility.SetDirty(target);
        }
        private void OnGUI()
        {
            if (!catalog || catalog.maps == null) { EditorGUILayout.HelpBox("未找到原版资源目录", MessageType.Error); return; }
            if (EditorApplication.isPlaying) { EditorGUILayout.HelpBox("退出播放后编辑碰撞，修改会保存到工程。",MessageType.Info); return; }
            selected = EditorGUILayout.Popup("地图",selected,catalog.maps.Select(m=>m.id).ToArray());
            var entry=catalog.maps[selected]; var map=entry.data;
            EditorGUILayout.BeginHorizontal();
            paintBlocked=GUILayout.Toggle(paintBlocked,"阻挡","Button"); paintBlocked=!GUILayout.Toggle(!paintBlocked,"通行","Button");
            grid=GUILayout.Toggle(grid,"网格"); zoom=EditorGUILayout.Slider(zoom,.2f,2f); EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(map.columns+" × "+map.rows+"    格子 "+map.cellWidth+" × "+map.cellHeight+"    红色=阻挡；拖动绘制，⌘Z 撤销");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("打开地图 Prefab")) AssetDatabase.OpenAsset(entry.prefab);
            if (GUILayout.Button("保存碰撞")) AssetDatabase.SaveAssetIfDirty(catalog);
            if (GUILayout.Button("导出碰撞 JSON"))
            {
                string path=EditorUtility.SaveFilePanel("导出碰撞","",map.id+"-collision.json","json");
                if (!string.IsNullOrEmpty(path)) File.WriteAllText(path,JsonUtility.ToJson(map,true));
            }
            if (GUILayout.Button("导入碰撞 JSON"))
            {
                string path=EditorUtility.OpenFilePanel("导入碰撞","","json");
                if (!string.IsNullOrEmpty(path))
                    try { ImportCollision(catalog,selected,File.ReadAllText(path)); }
                    catch(Exception e) { EditorUtility.DisplayDialog("导入失败",e.Message,"确定"); }
            }
            EditorGUILayout.EndHorizontal();
            scroll=EditorGUILayout.BeginScrollView(scroll);
            Rect bounds=GUILayoutUtility.GetRect(map.Width*zoom,map.Height*zoom,GUILayout.ExpandWidth(false));
            var color=GUI.color; GUI.color=new Color(.18f,.19f,.2f); GUI.DrawTexture(bounds,Texture2D.whiteTexture); GUI.color=color;
            for(int layer=0;layer<3;layer++)
            {
                var image=OriginalAssets.Texture("Maps/"+entry.id+"_"+layer);
                if(image) GUI.DrawTexture(bounds,image,ScaleMode.StretchToFill,true);
            }
            float w=map.cellWidth*zoom,h=map.cellHeight*zoom;
            for(int x=0;x<map.columns;x++) for(int y=0;y<map.rows;y++)
            {
                var r=new Rect(bounds.x+x*w,bounds.y+y*h,w,h);
                if(map.cells[x*map.rows+y].blocked) EditorGUI.DrawRect(r,new Color(1,.1f,.1f,.36f));
                if(grid) { EditorGUI.DrawRect(new Rect(r.x,r.y,w,1),new Color(1,1,1,.18f)); EditorGUI.DrawRect(new Rect(r.x,r.y,1,h),new Color(1,1,1,.18f)); }
            }
            var evt=Event.current;
            if(evt.type==EventType.MouseDown && evt.button==0 && bounds.Contains(evt.mousePosition)) { painting=true; Undo.IncrementCurrentGroup(); stroke=Undo.GetCurrentGroup(); }
            if(painting && (evt.type==EventType.MouseDown || evt.type==EventType.MouseDrag) && bounds.Contains(evt.mousePosition))
            {
                int x=Mathf.FloorToInt((evt.mousePosition.x-bounds.x)/w), y=Mathf.FloorToInt((evt.mousePosition.y-bounds.y)/h);
                SetCell(catalog,selected,x,y,paintBlocked); evt.Use(); Repaint();
            }
            if(evt.rawType==EventType.MouseUp && painting) { painting=false; if(stroke>=0) Undo.CollapseUndoOperations(stroke); }
            EditorGUILayout.EndScrollView();
        }
    }
}
