using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CityGen.Core
{
    public class CityFlyCameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        private float moveSpeed = 20f;

        [SerializeField]
        private float sprintMultiplier = 2.5f;

        [Header("Look")]
        [SerializeField]
        private float lookSensitivity = 2f;

        [SerializeField]
        private float minPitch = -89f;

        [SerializeField]
        private float maxPitch = 89f;

        [SerializeField]
        private bool requireRightMouseButtonToLook;

        [SerializeField]
        private bool lockCursorWhileLooking = true;

        private float yaw;
        private float pitch;
        private bool isLookActive = true;

        private void OnEnable()
        {
            Vector3 eulerAngles = transform.eulerAngles;
            yaw = eulerAngles.y;
            pitch = NormalizeAngle(eulerAngles.x);
            isLookActive = !lockCursorWhileLooking || !requireRightMouseButtonToLook;
        }

        private void OnDisable()
        {
            ReleaseCursor();
        }

        private void Update()
        {
            UpdateLookState();
            UpdateCursorState();
            UpdateRotation(isLookActive);
            UpdatePosition();
        }

        private void UpdateRotation(bool isLooking)
        {
            if (!isLooking)
            {
                return;
            }

            Vector2 lookInput = GetLookInput();
            if (lookInput.sqrMagnitude <= 0f)
            {
                return;
            }

            yaw += lookInput.x * lookSensitivity;
            pitch = Mathf.Clamp(pitch - (lookInput.y * lookSensitivity), minPitch, maxPitch);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void UpdatePosition()
        {
            Vector3 moveInput = GetMoveInput();
            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }

            float speed = moveSpeed;
            if (IsSprintPressed())
            {
                speed *= sprintMultiplier;
            }

            float deltaTime = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            Vector3 movement = (right * moveInput.x) + (Vector3.up * moveInput.y) + (forward * moveInput.z);

            transform.position += movement * speed * deltaTime;
        }

        private void UpdateCursorState()
        {
            if (!lockCursorWhileLooking)
            {
                return;
            }

            Cursor.lockState = isLookActive ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLookActive;
        }

        private void UpdateLookState()
        {
            if (!lockCursorWhileLooking)
            {
                isLookActive = !requireRightMouseButtonToLook || IsLookButtonPressed();
                return;
            }

            if (WasReleaseLookPressed())
            {
                isLookActive = false;
                ReleaseCursor();
                return;
            }

            if (!isLookActive)
            {
                if (ShouldActivateLook())
                {
                    isLookActive = true;
                }

                return;
            }

            if (requireRightMouseButtonToLook && !IsLookButtonPressed())
            {
                isLookActive = false;
                ReleaseCursor();
            }
        }

        private static float NormalizeAngle(float angle)
        {
            if (angle > 180f)
            {
                angle -= 360f;
            }

            return angle;
        }

        private static void ReleaseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private static bool IsSprintPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null &&
                (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed))
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                return true;
            }
#endif
            return false;
        }

        private static bool IsLookButtonPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButton(1))
            {
                return true;
            }
#endif
            return false;
        }

        private static Vector2 GetLookInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Pointer.current != null)
            {
                return Pointer.current.delta.ReadValue() * 0.1f;
            }

            if (Mouse.current != null)
            {
                return Mouse.current.delta.ReadValue() * 0.1f;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
#else
            return Vector2.zero;
#endif
        }

        private static Vector3 GetMoveInput()
        {
            float horizontal = 0f;
            float vertical = 0f;
            float elevation = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed)
                {
                    horizontal -= 1f;
                }

                if (Keyboard.current.dKey.isPressed)
                {
                    horizontal += 1f;
                }

                if (Keyboard.current.sKey.isPressed)
                {
                    vertical -= 1f;
                }

                if (Keyboard.current.wKey.isPressed)
                {
                    vertical += 1f;
                }

                if (Keyboard.current.qKey.isPressed)
                {
                    elevation -= 1f;
                }

                if (Keyboard.current.eKey.isPressed)
                {
                    elevation += 1f;
                }

                return new Vector3(horizontal, elevation, vertical);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            if (Input.GetKey(KeyCode.Q))
            {
                elevation -= 1f;
            }

            if (Input.GetKey(KeyCode.E))
            {
                elevation += 1f;
            }
#endif

            return new Vector3(horizontal, elevation, vertical);
        }

        private static bool WasReleaseLookPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                return true;
            }
#endif
            return false;
        }

        private bool ShouldActivateLook()
        {
            if (!Application.isFocused)
            {
                return false;
            }

            if (requireRightMouseButtonToLook)
            {
                return IsLookButtonPressed();
            }

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null &&
                (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
            {
                return true;
            }

            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.anyKeyDown)
            {
                return true;
            }
#endif

            return false;
        }
    }
}
