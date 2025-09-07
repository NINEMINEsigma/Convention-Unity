using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Convention.WindowsUI.Variant;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Convention
{
    public class FreeSceneCamera : MonoSingleton<FreeSceneCamera>, ILoadedInHierarchy
    {
        [Resources, InspectorDraw(InspectorDrawType.Reference)] public Transform TargetFollow;
        //[Resources, InspectorDraw(InspectorDrawType.Reference)] public CinemachineVirtualCamera VirtualCamera;
        [Setting, InspectorDraw(InspectorDrawType.Text)] public float moveSpeed = 1;
        [Setting, InspectorDraw(InspectorDrawType.Text)] public float rotationSpeed = 1;
        private bool m_IsFocus = false;
        [Setting, InspectorDraw(InspectorDrawType.Toggle)]
        public bool isFocus
        {
            get => m_IsFocus;
            set
            {
                if (m_IsFocus != value)
                {
                    m_IsFocus = value;
                    Cursor.lockState = m_IsFocus ? CursorLockMode.Locked : CursorLockMode.None;
                    Cursor.visible = !m_IsFocus;
                }
            }
        }

        private void Start()
        {
            m_IsFocus = false;
        }

        private void Update()
        {
            Vector3 dxyz = Vector3.zero;
            //Vector3 rxyz = Vector3.zero;
            if (Keyboard.current[Key.W].isPressed || Keyboard.current[Key.UpArrow].isPressed)
            {
                var temp = TargetFollow.forward;
                //temp.y = 0;
                dxyz += temp.normalized;
            }
            if (Keyboard.current[Key.A].isPressed || Keyboard.current[Key.LeftArrow].isPressed)
            {
                var temp = TargetFollow.right;
                temp.y = 0;
                dxyz -= temp.normalized;
            }
            if (Keyboard.current[Key.D].isPressed || Keyboard.current[Key.RightArrow].isPressed)
            {
                var temp = TargetFollow.right;
                temp.y = 0;
                dxyz += temp.normalized;
            }
            if (Keyboard.current[Key.S].isPressed || Keyboard.current[Key.DownArrow].isPressed)
            {
                var temp = TargetFollow.forward;
                //temp.y = 0;
                dxyz -= temp.normalized;
            }
            if (Keyboard.current[Key.Space].isPressed)
                dxyz += Vector3.up;
#if !UNITY_EDITOR
            if (Keyboard.current[Key.LeftShift].isPressed)
#else
            if (Keyboard.current[Key.Q].isPressed)
#endif
                dxyz -= Vector3.up;

            var drotation = Vector3.zero;
            if (isFocus)
            {
                var temp = Mouse.current.delta.ReadValue();
                drotation = new(-temp.y, temp.x, 0);
            }

            //

            TargetFollow.Translate(dxyz * moveSpeed, Space.World);
            TargetFollow.Rotate(drotation * rotationSpeed, Space.Self);

            //

            if (Keyboard.current[Key.Escape].isPressed)
                isFocus = false;
            if (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.LeftShift].isPressed)
                TargetFollow.localEulerAngles = new(0, TargetFollow.eulerAngles.y, 0);
        }
    }
}
