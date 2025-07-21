using System;
using UnityEngine;
using UnityEngine.UI;

namespace Convention.WindowsUI
{
    public class BaseWindowPlane : WindowsComponent
    {
        [Resources, SerializeField, OnlyNotNullMode] private RectTransform m_Plane;
        [Resources, SerializeField, HopeNotNull, Tooltip("This animational plane should has the same parent transform")]
        private RectTransform m_AnimationPlane;
        [Setting, OnlyNotNullMode(nameof(m_AnimationPlane)), SerializeField, Header("Animation Setting")]
        private bool IsEnableAnimation = true;
        [Setting, OnlyNotNullMode(nameof(m_AnimationPlane)), Percentage(0, 1), Range(0, 1), WhenAttribute.Is(nameof(IsEnableAnimation), true)]
        public float AnimationSpeed = 0.5f;

        public RectTransform Plane => m_Plane;

        [Content, OnlyPlayMode, Ignore] public RectTransformInfo BeforeMaximizeWindow = null;
        [Content, OnlyPlayMode, Ignore] public float BeforeMaximizeWindowBackgroundColorA = 1f;
        private bool IsMaximizeWindowMode = false;
        [Content, OnlyPlayMode]
        public void MaximizeWindow()
        {
            if (IsMaximizeWindowMode)
                return;
            BeforeMaximizeWindow = new(m_Plane);
            var prect = m_Plane.transform.parent.GetComponent<RectTransform>();
            m_Plane.SetPositionAndRotation(prect.position, prect.rotation);
            m_Plane.anchoredPosition = Vector3.zero;
            m_Plane.anchorMax = Vector2.one;
            m_Plane.anchorMin = Vector2.zero;
            m_Plane.sizeDelta = Vector2.zero;
            var backgroundPlane = m_AnimationPlane == null ? m_Plane : m_AnimationPlane;
            if (backgroundPlane.TryGetComponent<Image>(out var image))
            {
                BeforeMaximizeWindowBackgroundColorA = image.color.a;
                var color = image.color;
                color.a = 1;
                image.color = color;
            }
            IsMaximizeWindowMode = true;
        }
        [Content, OnlyPlayMode]
        public void ExitMaximizeWindowMode()
        {
            if (!IsMaximizeWindowMode)
                return;
            BeforeMaximizeWindow.Setup(m_Plane);
            var backgroundPlane = m_AnimationPlane == null ? m_Plane : m_AnimationPlane;
            if (backgroundPlane.TryGetComponent<Image>(out var image))
            {
                var color = image.color;
                color.a = BeforeMaximizeWindowBackgroundColorA;
                image.color = color;
            }
            IsMaximizeWindowMode = false;
        }

        private void OnEnable()
        {
            if (m_AnimationPlane != null)
            {
                new RectTransformInfo(m_Plane).Setup(m_AnimationPlane);
            }
        }
        [Content]
        public void SynchronizedAnimationPlane()
        {
            new RectTransformInfo(m_Plane).Setup(m_AnimationPlane);
        }

        private void LateUpdate()
        {
            if (IsEnableAnimation && m_Plane && m_AnimationPlane)
            {
                RectTransformInfo.UpdateAnimationPlane(m_Plane, m_AnimationPlane, AnimationSpeed, IsMaximizeWindowMode ? 1 : -1, false);
            }
        }

        public virtual void AddChild(RectTransform target, Rect rect, bool isAdjustSizeToContainsChilds = false)
        {
            RectTransformExtension.SetParentAndResizeWithoutNotifyBaseWindowPlane(m_Plane, target, rect, isAdjustSizeToContainsChilds);
        }
        public virtual void AddChild(RectTransform target, bool isAdjustSizeToContainsChilds = false)
        {
            RectTransformExtension.SetParentAndResizeWithoutNotifyBaseWindowPlane(m_Plane, target, isAdjustSizeToContainsChilds);
        }

        [Content]

        public void ForceRebuildLayoutImmediate()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        [Resources, Tooltip("before AdjustSizeToContainsChilds, will compute it first")] public RectTransform AdjustSizeToContainsRect;

        [Content]
        public void AdjustSizeToContainsChilds()
        {
            if (AdjustSizeToContainsRect == null)
                RectTransformExtension.AdjustSizeToContainsChilds(rectTransform);
            else
            {
                var corners = new Vector3[4];
                Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 max = new Vector2(float.MinValue, float.MinValue);
                AdjustSizeToContainsRect.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    Vector2 localCorner = rectTransform.InverseTransformPoint(corner);
                    if (float.IsNaN(localCorner.x) || float.IsNaN(localCorner.y))
                        break;
                    min.x = Mathf.Min(min.x, localCorner.x);
                    min.y = Mathf.Min(min.y, localCorner.y);
                    max.x = Mathf.Max(max.x, localCorner.x);
                    max.y = Mathf.Max(max.y, localCorner.y);
                }
                RectTransformExtension.AdjustSizeToContainsChilds(rectTransform, min, max, null);
            }
        }
    }
}
