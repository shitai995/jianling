using System.Collections.Generic;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        [SerializeField] private OriginalTouchLayout touchLayout;
        private readonly HashSet<int> touchHeld = new HashSet<int>(), previousTouchHeld = new HashSet<int>();
        private Vector2 touchMovement;
        public bool TouchControlsVisible => touchLayout && touchLayout.visible && !AtTitle && !hideUi && !DialogueVisible && (!menu || miniMapOpen) && !reinforceOpen && !WeaponPreviewVisible && !CriticalActive;
        public void SetTouchControl(int code, bool pressed)
        {
            if (pressed) touchHeld.Add(code); else touchHeld.Remove(code);
        }
        private Vector2 ScreenToGame(Vector2 position)
        {
            float scale = Mathf.Min(Screen.width / ViewWidth, Screen.height / ViewHeight);
            return new Vector2((position.x - (Screen.width - ViewWidth * scale) / 2) / scale,
                (Screen.height - position.y - (Screen.height - ViewHeight * scale) / 2) / scale);
        }
        private void PollTouchControls()
        {
            previousTouchHeld.Clear(); foreach (int code in touchHeld) previousTouchHeld.Add(code);
            touchHeld.Clear(); touchMovement = Vector2.zero;
            if (!TouchControlsVisible || !touchLayout || touchLayout.controls == null) return;
            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled) ReadTouchPoint(ScreenToGame(touch.position));
            }
            if (Input.touchCount == 0 && Input.GetMouseButton(0)) ReadTouchPoint(ScreenToGame(Input.mousePosition));
            touchMovement = new Vector2((touchHeld.Contains(3) ? 1 : 0) - (touchHeld.Contains(2) ? 1 : 0),
                (touchHeld.Contains(1) ? 1 : 0) - (touchHeld.Contains(0) ? 1 : 0));
            foreach (int code in touchHeld)
                if (!previousTouchHeld.Contains(code)) PressControl(code);
        }
        private void ReadTouchPoint(Vector2 point)
        {
            foreach (var control in touchLayout.controls) if (control.bounds.Contains(point)) touchHeld.Add(control.code);
        }
        public void PressControl(int code)
        {
            if (AtTitle) return;
            if (HandleOriginalBlackScreenControl(code) || HandleOriginalMessageControl(code) || HandleOriginalChoiceControl(code) || HandleOriginalCharacterControl(code)) return;
            if (modalInputFrame == Time.frameCount) return;
            if (miniMapOpen) { if(code==12) CloseMiniMap(); return; }
            if (tutorialKey >= 0) { RespondTutorial(code); return; }
            if (menu)
            {
                // Abstract control5 is Back; original menu soft-back is key10.
                if ((code >= 0 && code <= 4) || code == 5 || code == 9 || code == 18) HandleOriginalMenuKey(code == 5 ? 10 : code);
                return;
            }
            if (code == 5 && !DialogueVisible && !CriticalActive && !reinforceOpen && !Machine.Running) { menu = !menu; menuPage = 0; return; }
            if(code==12) { OpenMiniMap(); return; }
            if (!Role || Role.locked || Role.frozen || Role.hp <= 0 || menu || reinforceOpen || DialogueVisible || Machine.Running || lockInput || CriticalActive) return;
            switch (code)
            {
                case 0: case 1: case 2: case 3: TryBackstep(code == 2 ? -1 : code == 3 ? 1 : 0); break;
                case 4: PrimaryControl(); break;
                case 7: if (equippedSkill >= 5) UseSkill(equippedSkill, integrated ? spiritSkillLevel : Equipped.skillLevel); break;
                case 8: UseSkill(4); break;
                case 9: if (Role.LogicalAction <= 1) ToggleIntegrate(); break;
                case 10: TryCriticalKill(); break;
                case 11: Interact(); break;
            }
        }
        private void Interact()
        { TryInteract(); }
        private bool TryInteract()
        {
            OriginalActor nearest = null; float distance = 60;
            foreach (var target in actors.Values)
            {
                if (target.actorId < 0 || target.hidden || string.IsNullOrEmpty(target.interactionScript)) continue;
                float d = Vector2.Distance(target.mapPosition, Role.mapPosition);
                if (d < distance) { nearest = target; distance = d; }
            }
            if (!nearest) return false;
            Machine.Enqueue(nearest.interactionScript); return true;
        }
        private void PrimaryControl()
        { if(Role && Role.LogicalAction<=1 && TryInteract()) return; Attack(); }
        public static Rect TouchImageBounds(OriginalTouchLayout.Control control, Texture2D image)
        {
            var center=control.bounds.center;
            return new Rect(center.x-(image.width>>1),center.y-(image.height>>1),image.width,image.height);
        }
        private bool ControlHeld(int code)
        {
            if(touchHeld.Contains(code)) return true;
            switch(code)
            {
                case 0:return Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow);
                case 1:return Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow);
                case 2:return Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow);
                case 3:return Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow);
                case 4:return Input.GetKey(KeyCode.J)||Input.GetKey(KeyCode.Space)||Input.GetKey(KeyCode.Return);
                case 5:return Input.GetKey(KeyCode.Escape);
                case 7:return Input.GetKey(KeyCode.K);
                case 8:return Input.GetKey(KeyCode.L);
                case 10:return Input.GetKey(KeyCode.O);
                case 12:return Input.GetKey(KeyCode.M);
                default:return false;
            }
        }
        private void DrawTouchControls()
        {
            if (!TouchControlsVisible || touchLayout.controls == null) return;
            var color = GUI.color; GUI.color = new Color(1, 1, 1, touchLayout.opacity);
            bool hasDirection=touchLayout.direction!=null && touchLayout.direction.frames!=null && touchLayout.direction.frames.Length>=5 && touchLayout.directionSheets!=null && touchLayout.directionSheets.Length>=2;
            if (hasDirection)
            {
                var r=touchLayout.directionBounds;var matrix=GUI.matrix;
                GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(r.center.x,r.center.y,0),Quaternion.identity,new Vector3(r.width/124,r.height/124,1));
                int dir=ControlHeld(0)?1:ControlHeld(1)?2:ControlHeld(2)?3:ControlHeld(3)?4:0;
                OriginalGui.Frame(touchLayout.direction,touchLayout.directionSheets,dir,Vector2.zero);GUI.matrix=matrix;
            }
            foreach (var control in touchLayout.controls)
            {
                if(miniMapOpen && control.code>=4 && control.code!=4 && control.code!=12) continue;
                if (control.code < 4 && hasDirection) continue;
                // The integration hit zone is drawn by the original spirit HUD.
                if(control.code==9) continue;
                var image=ControlHeld(control.code) && control.pressed ? control.pressed : control.normal;
                if(control.code==12 && miniMapOpen) image=ControlHeld(12)?touchLayout.backPressed:touchLayout.back;
                if(image)
                {
                    GUI.DrawTexture(TouchImageBounds(control,image),image);
                    if(control.code==10 && energy==2000)
                        OriginalGui.Animation(touchLayout.criticalReady,touchLayout.criticalReadySheets,0,Mathf.FloorToInt(Time.time/.08f),control.bounds.center);
                }
                else GUI.Box(control.bounds,control.label,buttonStyle);
            }
            GUI.color = color;
        }
    }
}
