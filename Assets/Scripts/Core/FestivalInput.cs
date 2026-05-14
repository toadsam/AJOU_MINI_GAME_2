using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AjouFestival.Core
{
    public static class FestivalInput
    {
        public static bool GetKey(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKey(key);
#else
                return false;
#endif
            }

            return key switch
            {
                KeyCode.Space => Keyboard.current.spaceKey.isPressed,
                KeyCode.A => Keyboard.current.aKey.isPressed,
                KeyCode.D => Keyboard.current.dKey.isPressed,
                KeyCode.W => Keyboard.current.wKey.isPressed,
                KeyCode.S => Keyboard.current.sKey.isPressed,
                KeyCode.LeftArrow => Keyboard.current.leftArrowKey.isPressed,
                KeyCode.RightArrow => Keyboard.current.rightArrowKey.isPressed,
                KeyCode.UpArrow => Keyboard.current.upArrowKey.isPressed,
                KeyCode.DownArrow => Keyboard.current.downArrowKey.isPressed,
                KeyCode.Return => Keyboard.current.enterKey.isPressed,
                KeyCode.KeypadEnter => Keyboard.current.numpadEnterKey.isPressed,
                KeyCode.RightControl => Keyboard.current.rightCtrlKey.isPressed,
                KeyCode.Escape => Keyboard.current.escapeKey.isPressed,
                KeyCode.R => Keyboard.current.rKey.isPressed,
                _ => false
            };
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(key);
#else
            return false;
#endif
        }

        public static bool GetKeyDown(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(key);
#else
                return false;
#endif
            }

            return key switch
            {
                KeyCode.Space => Keyboard.current.spaceKey.wasPressedThisFrame,
                KeyCode.Return => Keyboard.current.enterKey.wasPressedThisFrame,
                KeyCode.KeypadEnter => Keyboard.current.numpadEnterKey.wasPressedThisFrame,
                KeyCode.RightControl => Keyboard.current.rightCtrlKey.wasPressedThisFrame,
                KeyCode.Escape => Keyboard.current.escapeKey.wasPressedThisFrame,
                KeyCode.R => Keyboard.current.rKey.wasPressedThisFrame,
                _ => false
            };
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key);
#else
            return false;
#endif
        }

        public static bool MouseOrTouchDown()
        {
#if ENABLE_INPUT_SYSTEM
            bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool touch = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
            return mouse || touch;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#else
            return false;
#endif
        }

        public static bool MouseOrTouchHeld()
        {
#if ENABLE_INPUT_SYSTEM
            bool mouse = Mouse.current != null && Mouse.current.leftButton.isPressed;
            bool touch = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
            return mouse || touch;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(0) || Input.touchCount > 0;
#else
            return false;
#endif
        }

        public static Vector2 MoveWasd()
        {
            Vector2 input = Vector2.zero;
            if (GetKey(KeyCode.A)) input.x -= 1f;
            if (GetKey(KeyCode.D)) input.x += 1f;
            if (GetKey(KeyCode.S)) input.y -= 1f;
            if (GetKey(KeyCode.W)) input.y += 1f;
            return Vector2.ClampMagnitude(input, 1f);
        }

        public static Vector2 MoveArrows()
        {
            Vector2 input = Vector2.zero;
            if (GetKey(KeyCode.LeftArrow)) input.x -= 1f;
            if (GetKey(KeyCode.RightArrow)) input.x += 1f;
            if (GetKey(KeyCode.DownArrow)) input.y -= 1f;
            if (GetKey(KeyCode.UpArrow)) input.y += 1f;
            return Vector2.ClampMagnitude(input, 1f);
        }
    }
}
