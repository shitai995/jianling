using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        [SerializeField] private OriginalMiniMapLayout miniMapLayout;
        private bool miniMapOpen;
        private Vector2 miniMapOrigin;
        public bool MiniMapVisible => miniMapOpen;
        public bool OpenMiniMap()
        {
            if(!miniMapLayout || miniMapLayout.areas==null || areaIndex<0 || areaIndex>=miniMapLayout.areas.Length ||
                AtTitle || menu || DialogueVisible || Machine.Running || lockInput || CriticalActive || reinforceOpen || !Role || Role.hp<=0) return false;
            var area=miniMapLayout.areas[areaIndex];
            int stage; int.TryParse(currentStageScript,out stage);
            var room=area.rooms.FirstOrDefault(r=>r.stage==stage); int column=room==null?0:room.x;
            float x=((533-area.width)>>1)-12;
            if(x+column*53<0) x=-column*53;
            if(x+12+column*53>533-45) x=533-45-12-column*53;
            miniMapOrigin=new Vector2(x+12,((320-area.height)>>1));
            miniMapOpen=menu=true; worldMapOpen=false; scriptedMenu=false; return true;
        }
        public void CloseMiniMap() { if(!miniMapOpen) return; miniMapOpen=false; menu=false; }
        private static void MapFill(Rect rect,Color color)
        {
            var previous=GUI.color; GUI.color=color; GUI.DrawTexture(rect,Texture2D.whiteTexture); GUI.color=previous;
        }
        private static void MapIcon(Texture2D icon,float x,float y)
        { if(icon) GUI.DrawTexture(new Rect(x,y,icon.width,icon.height),icon); }
        private void DrawMiniMap()
        {
            MapFill(new Rect(0,0,ViewWidth,ViewHeight),Color.black);
            var area=miniMapLayout.areas[areaIndex]; int stage; int.TryParse(currentStageScript,out stage);
            foreach(var room in area.rooms)
            {
                float x=miniMapOrigin.x+room.x*53+(room.exits[0]>1||room.exits[1]>1?26:0), y=miniMapOrigin.y+room.y*38;
                // Original Graphics.drawRect draws four one-pixel lines with inclusive endpoints.
                MapFill(new Rect(x,y,46,1),Color.white); MapFill(new Rect(x,y+30,46,1),Color.white);
                MapFill(new Rect(x,y,1,31),Color.white); MapFill(new Rect(x+45,y,1,31),Color.white);
                if(room.stage==stage)
                {
                    if(room.stage==missionStageIndex || room.save)
                    {
                        MapIcon(miniMapLayout.current,x+5,y+7);
                        MapIcon(room.stage==missionStageIndex?miniMapLayout.mission:miniMapLayout.save,x+25,y+7);
                    }
                    else MapIcon(miniMapLayout.current,x+14,y+7);
                }
                else if(room.stage==missionStageIndex) MapIcon(miniMapLayout.mission,x+14,y+7);
                else if(room.save) MapIcon(miniMapLayout.save,x+14,y+7);
                for(int direction=0;direction<4;direction++)
                {
                    int exit=room.exits[direction]; if(exit==0) continue;
                    if(direction<2)
                    {
                        float ey=direction==0?y-4:y+30;
                        if(exit<0) MapFill(new Rect(x+19,ey+(direction==0?2:-2),6,4),Color.black);
                        else if(exit>1)
                        { MapFill(new Rect(x+5,ey,4,4),Color.yellow); MapFill(new Rect(x+35,ey,4,4),Color.yellow); }
                        else
                        {
                            int offset=20;
                            foreach(var other in area.rooms)
                            {
                                if(other.exits[1-direction]<=1 || other.y!=room.y+(direction==0?-1:1)) continue;
                                if(other.x==room.x) offset=31;
                                else if(other.x+1==room.x) offset=8;
                            }
                            MapFill(new Rect(x+offset,ey,4,4),Color.yellow);
                        }
                    }
                    else if(exit<0) MapFill(new Rect(direction==2?x-2:x+43,y+12,4,6),Color.black);
                    else
                    {
                        bool adjacent=area.rooms.Any(r=>r.y==room.y && r.x==room.x+(direction==2?-1:1));
                        MapFill(new Rect(direction==2?x-(adjacent?4:30):x+45,y+13,adjacent?4:direction==2?30:31,4),Color.yellow);
                    }
                }
            }
        }
    }
}
