using System;
using System.Collections.Generic;
using Convention.WindowsUI;
using Convention.WindowsUI.Variant;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Convention
{
    namespace WindowsUI
    {
        [ArgPackage, Serializable]
        public class RectTransformInfo
        {
            [Setting] public Vector2 position;
            [Setting] public Vector2 anchoredPosition;
            [Setting] public Quaternion rotation;
            [Setting] public Vector2 sizeDelta;
            [Setting] public Vector2 anchorMax;
            [Setting] public Vector2 anchorMin;
            [Setting] public Vector2 pivot;

            public RectTransformInfo(RectTransform rect)
            {
                //position = rect.position;
                position = rect.localPosition;
                rotation = rect.rotation;
                anchoredPosition = rect.anchoredPosition;
                sizeDelta = rect.sizeDelta;
                anchorMax = rect.anchorMax;
                anchorMin = rect.anchorMin;
                pivot = rect.pivot;
            }

            public RectTransformInfo(Vector2 localPosition, Vector2 anchoredPosition, Quaternion rotation, Vector2 sizeDelta, Vector2 anchorMax, Vector2 anchorMin, Vector2 pivot)
            {
                //this.position = position;
                this.position = localPosition;
                this.anchoredPosition = anchoredPosition;
                this.rotation = rotation;
                this.sizeDelta = sizeDelta;
                this.anchorMax = anchorMax;
                this.anchorMin = anchorMin;
                this.pivot = pivot;
            }

            public void Setup(RectTransform rect)
            {
                rect.position = position;
                rect.rotation = rotation;
                rect.anchoredPosition = anchoredPosition;
                rect.sizeDelta = sizeDelta;
                rect.anchorMax = anchorMax;
                rect.anchorMin = anchorMin;
                rect.pivot = pivot;
            }
            protected static float AnimationalAdd(float value, float additional, float from, float to)
            {
                float min = Mathf.Min(from, to);
                float max = Mathf.Max(from, to);
                return Mathf.Clamp(value + additional, min, max);
            }
            protected static float AnimationalPercentage([Percentage(0, 1)] float percentage, float from, float to, float minimumThresholdValue)
            {
                float min = Mathf.Min(from, to);
                float max = Mathf.Max(from, to);
                return Mathf.Clamp(Mathf.Lerp(from, to, percentage) + minimumThresholdValue, min, max);
            }
            protected static float AnimationalPercentageAngle([Percentage(0, 1)] float percentage, float from, float to, float minimumThresholdValue)
            {
                float min = Mathf.Min(from, to);
                float max = Mathf.Max(from, to);
                return Mathf.Clamp(Mathf.LerpAngle(from, to, percentage) + minimumThresholdValue, min, max);
            }
            /// <summary>
            /// update until they as the same
            /// </summary>
            /// <param name="rectTransform"></param>
            /// <param name="animationTran"></param>
            /// <param name="speed"></param>
            /// <returns>is same</returns>
            public static void UpdateAnimationPlane(
                [In] RectTransform rectTransform,
                [In] RectTransform animationTran,
                float speed,
                float minimumThresholdValue,
                bool isMakeParentSame)
            {
                if (isMakeParentSame && rectTransform.parent != animationTran.parent)
                    animationTran.SetParent(rectTransform.parent, true);
                animationTran.position = new(
                    Lerp(rectTransform.position.x, animationTran.position.x),
                    Lerp(rectTransform.position.y, animationTran.position.y),
                    Lerp(rectTransform.position.z, animationTran.position.z));
                animationTran.eulerAngles = (new(
                    LerpAngle(rectTransform.rotation.eulerAngles.x, animationTran.rotation.eulerAngles.x),
                    LerpAngle(rectTransform.rotation.eulerAngles.y, animationTran.rotation.eulerAngles.y),
                    LerpAngle(rectTransform.rotation.eulerAngles.z, animationTran.rotation.eulerAngles.z)));

                minimumThresholdValue = 0;
                animationTran.pivot = new(
                    Lerp(rectTransform.pivot.x, animationTran.pivot.x),
                    Lerp(rectTransform.pivot.y, animationTran.pivot.y));
                animationTran.anchoredPosition = new(
                    Lerp(rectTransform.anchoredPosition.x, animationTran.anchoredPosition.x),
                    Lerp(rectTransform.anchoredPosition.y, animationTran.anchoredPosition.y));
                animationTran.sizeDelta = new(
                    Lerp(rectTransform.sizeDelta.x, animationTran.sizeDelta.x),
                    Lerp(rectTransform.sizeDelta.y, animationTran.sizeDelta.y));
                animationTran.anchorMax = new(
                    Lerp(rectTransform.anchorMax.x, animationTran.anchorMax.x),
                    Lerp(rectTransform.anchorMax.y, animationTran.anchorMax.y));
                animationTran.anchorMin = new(
                    Lerp(rectTransform.anchorMin.x, animationTran.anchorMin.x),
                    Lerp(rectTransform.anchorMin.y, animationTran.anchorMin.y));

                float Lerp(float to, float from)
                {
                    return AnimationalPercentage(speed, from, to, minimumThresholdValue);
                }
                float LerpAngle(float to, float from)
                {
                    return AnimationalPercentageAngle(speed, from, to, minimumThresholdValue);
                }
            }
        }
        public class WindowsComponent : MonoBehaviour
        {
            [Resources, Ignore] private RectTransform m_rectTransform;
            public RectTransform rectTransform
            {
                get
                {
                    if (m_rectTransform == null)
                    {
                        m_rectTransform = GetComponent<RectTransform>();
                    }
                    return m_rectTransform;
                }
                protected set
                {
                    m_rectTransform = value;
                }
            }
        }
    }

    public class WindowManager : MonoBehaviour
    {
        private void Reset()
        {
            string str = "";
            str = WindowPlane?.ToString();
        }

        public static WindowManager GenerateWindow()
        {
            return Instantiate(SO.Windows.GlobalInstance.GetWindowsComponent(nameof(WindowManager)).gameObject).GetComponent<WindowManager>();
        }
        [Content, OnlyPlayMode]
        public void CloseWindow()
        {
            throw new NotImplementedException();
        }

        [Content, Ignore, SerializeField] private int m_CurrentContextKey;

        [ArgPackage, Serializable]
        private class ContentEntry
        {
            [OnlyNotNullMode] public RectTransform plane;
            [HopeNotNull] public RectTransform root;
            public GameObject GetRootObject()
            {
                if (root == null)
                    return plane.gameObject;
                return root.gameObject;
            }
        }
        [Resources, SerializeField] private List<ContentEntry> m_AllContextPlane = new();
        [Resources, SerializeField, OnlyNotNullMode] public BaseWindowPlane WindowPlane;
        [Resources, SerializeField] public BaseWindowBar WindowBar;

        public RectTransform this[int index]
        {
            get
            {
                if (m_AllContextPlane.Count > index && index >= 0)
                    return m_AllContextPlane[index].plane;
                if (WindowPlane)
                    return WindowPlane.rectTransform;
                return this.transform as RectTransform;
            }
        }
        public RectTransform CurrentContextRectTransform
        {
            [return: ReturnNotNull]
            get
            {
                if (m_AllContextPlane.Count > 0)
                    return m_AllContextPlane[m_CurrentContextKey].plane;
                if (WindowPlane)
                    return WindowPlane.rectTransform;
                return this.transform as RectTransform;
            }
        }

        public void SelectContextPlane(int key)
        {
            if (CurrentContextRectTransform != this.transform)
            {
                m_AllContextPlane[m_CurrentContextKey].GetRootObject().SetActive(false);
                m_CurrentContextKey = key;
                m_AllContextPlane[m_CurrentContextKey].GetRootObject().SetActive(true);
            }
        }

        [Setting, OnlyPlayMode]
        [return: ReturnNotNull]
        public void SelectNextContextPlane()
        {
            if (m_AllContextPlane.Count > 0)
            {
                m_CurrentContextKey = (m_CurrentContextKey + 1) % m_AllContextPlane.Count;
            }
        }

        public int AddContextPlane([In] RectTransform plane)
        {
            int result = m_AllContextPlane.Count;
            m_AllContextPlane.Add(new() { plane = plane, root = null });
            plane.gameObject.SetActive(false);
            return result;
        }
        public int AddContextPlane([In] RectTransform plane, [In] RectTransform root)
        {
            int result = m_AllContextPlane.Count;
            m_AllContextPlane.Add(new() { plane = plane, root = root });
            root.gameObject.SetActive(false);
            return result;
        }
        [return: ReturnNotNull, When("return current plane's root")]
        public GameObject RemoveContextPlane(int index)
        {
            var result = m_AllContextPlane[index].GetRootObject();
            result.SetActive(false);
            m_AllContextPlane.RemoveAt(index);
            if (m_CurrentContextKey >= index)
                m_CurrentContextKey--;
            m_AllContextPlane[m_CurrentContextKey].GetRootObject().SetActive(true);
            return result;
        }

        public void AddContextChild([In][IsInstantiated(true)]RectTransform child, Rect rect, bool isAdjustSizeToContainsChilds)
        {
            if (CurrentContextRectTransform.GetComponents<BaseWindowPlane>().Length == 0)
                RectTransformExtension.SetParentAndResize(CurrentContextRectTransform, child, rect, isAdjustSizeToContainsChilds);
            else
                CurrentContextRectTransform.GetComponents<BaseWindowPlane>()[0].AddChild(child, rect, isAdjustSizeToContainsChilds);
        }
        public void AddContextChild([In][IsInstantiated(true)]RectTransform child, bool isAdjustSizeToContainsChilds)
        {
            if (CurrentContextRectTransform.GetComponents<BaseWindowPlane>().Length == 0)
                RectTransformExtension.SetParentAndResize(CurrentContextRectTransform, child, isAdjustSizeToContainsChilds);
            else
                CurrentContextRectTransform.GetComponents<BaseWindowPlane>()[0].AddChild(child, isAdjustSizeToContainsChilds);
        }
        public void AddContextChild(
            [In,When("from this[]")]RectTransform context,
            [In][IsInstantiated(true)] RectTransform child, 
            Rect rect,
            bool isAdjustSizeToContainsChilds)
        {
            if (context.GetComponents<BaseWindowPlane>().Length == 0)
                RectTransformExtension.SetParentAndResize(context, child, rect, isAdjustSizeToContainsChilds);
            else
                context.GetComponents<BaseWindowPlane>()[0].AddChild(child, rect, isAdjustSizeToContainsChilds);
        }
        public void AddContextChild(
            [In,When("from this")]RectTransform context,
            [In][IsInstantiated(true)] RectTransform child,
            bool isAdjustSizeToContainsChilds)
        {
            if (context.GetComponents<BaseWindowPlane>().Length == 0)
                RectTransformExtension.SetParentAndResize(context, child, isAdjustSizeToContainsChilds);
            else
                context.GetComponents<BaseWindowPlane>()[0].AddChild(child, isAdjustSizeToContainsChilds);
        }

        [Content, OnlyPlayMode]
        public BaseWindowBar.RegisteredButtonWrapper CreateWindowBarButton(params UnityAction[] actions)
        {
            var wrapper = WindowBar.RegisterButton();
            wrapper.button.AddListener(actions);
            wrapper.buttonModule.gameObject.SetActive(true);
            return wrapper;
        }
        public BaseWindowBar.RegisteredPageWrapper CreateSubWindowWithBarButton([In][IsInstantiated(true)] RectTransform plane)
        {
            WindowPlane.AddChild(plane, false);
            var result = WindowBar.RegisterPage(plane);
            result.buttonModule.gameObject.SetActive(true);
            return result;
        }
        public BaseWindowBar.RegisteredPageWrapper CreateSubWindowWithBarButton(
            [In][IsInstantiated(true)] RectTransform plane,
            [In][IsInstantiated(true)] RectTransform root
            )
        {
            WindowPlane.AddChild(plane, false);
            var result = WindowBar.RegisterPage(plane, root);
            result.buttonModule.gameObject.SetActive(true);
            return result;
        }
    }
}