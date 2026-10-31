using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace GuJian
{
    // Device reads live here; the existing modal handlers keep their original key semantics.
    // This adapter is shared by gameplay, menus, dialogue, tutorials and the old touch artwork.
    public static class GameInput
    {
        public static void Initialize()
        {
            if (!EnhancedTouchSupport.enabled) EnhancedTouchSupport.Enable();
        }

        private static KeyControl KeyControl(KeyCode key)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return null;
            switch (key)
            {
                case KeyCode.W: return keyboard.wKey;
                case KeyCode.A: return keyboard.aKey;
                case KeyCode.S: return keyboard.sKey;
                case KeyCode.D: return keyboard.dKey;
                case KeyCode.J: return keyboard.jKey;
                case KeyCode.K: return keyboard.kKey;
                case KeyCode.L: return keyboard.lKey;
                case KeyCode.I: return keyboard.iKey;
                case KeyCode.O: return keyboard.oKey;
                case KeyCode.P: return keyboard.pKey;
                case KeyCode.E: return keyboard.eKey;
                case KeyCode.M: return keyboard.mKey;
                case KeyCode.UpArrow: return keyboard.upArrowKey;
                case KeyCode.DownArrow: return keyboard.downArrowKey;
                case KeyCode.LeftArrow: return keyboard.leftArrowKey;
                case KeyCode.RightArrow: return keyboard.rightArrowKey;
                case KeyCode.Space: return keyboard.spaceKey;
                case KeyCode.Return: return keyboard.enterKey;
                case KeyCode.KeypadEnter: return keyboard.numpadEnterKey;
                case KeyCode.Escape: return keyboard.escapeKey;
                default: return null;
            }
        }

        public static bool Held(KeyCode key) => KeyControl(key)?.isPressed ?? false;
        public static bool Pressed(KeyCode key) => KeyControl(key)?.wasPressedThisFrame ?? false;
        public static bool MouseHeld => Mouse.current?.leftButton.isPressed ?? false;
        public static Vector2 MousePosition => Mouse.current?.position.ReadValue() ?? Vector2.zero;
        public static int TouchCount => Touch.activeTouches.Count;

        public static bool ActiveTouch(int index, out Vector2 position)
        {
            var touches = Touch.activeTouches;
            if (index < 0 || index >= touches.Count) { position = Vector2.zero; return false; }
            var touch = touches[index];
            position = touch.screenPosition;
            return touch.phase != UnityEngine.InputSystem.TouchPhase.Ended &&
                touch.phase != UnityEngine.InputSystem.TouchPhase.Canceled;
        }
    }
}
