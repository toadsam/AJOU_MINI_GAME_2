using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AjouBuntu.Core
{
    public sealed class InputManager : MonoBehaviour
    {
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }

        private void Update()
        {
            JumpPressed = false;
            JumpHeld = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                JumpPressed |= Keyboard.current.spaceKey.wasPressedThisFrame;
                JumpHeld |= Keyboard.current.spaceKey.isPressed;
            }

            if (Mouse.current != null)
            {
                JumpPressed |= Mouse.current.leftButton.wasPressedThisFrame;
                JumpHeld |= Mouse.current.leftButton.isPressed;
            }

            if (Touchscreen.current != null)
            {
                JumpPressed |= Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
                JumpHeld |= Touchscreen.current.primaryTouch.press.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            JumpPressed |= Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
            JumpHeld |= Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                JumpPressed |= touch.phase == UnityEngine.TouchPhase.Began;
                JumpHeld = true;
            }
#endif
        }
    }
}
