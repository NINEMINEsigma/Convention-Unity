using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorReference : InspectorBaseItem
    {
        [Resources, SerializeField, OnlyNotNullMode] private RectTransform MyPlane;
        [Header("Button")]
        [Resources, SerializeField, OnlyNotNullMode] private Button MyButton;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_Text MyButtonText;
        [Resources, SerializeField, OnlyNotNullMode] private ModernUIDropdown MyDropdownSelector;
        [Setting, SerializeField] private int UnitSize = 90;

        public override void SetFolder(bool status)
        {
            MyPlane.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : UnitSize) + LabelSize);
        }

        public override void UpdateValue()
        {
            var obj = GetValue<object>();
            MyDropdownSelector.title = MyButtonText.text = obj == null ? "null" : obj.ToString();
        }

        protected override void InitBindingEvent()
        {
            MyButton.onClick.AddListener(() =>
            {
                var obj = GetValue<object>();
                if (obj != null)
                    InspectorWindow.instance.SetTarget(obj);
            });
            var temp = SafeType.GetMember(SafeMemberName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (temp.Length == 1 && ConventionUtility.GetMemberValueType(temp[0], out var fieldType))
            {
                MyDropdownSelector.DropdownOnEvent.AddListener(x =>
                {
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (x ? 300 : 0) + UnitSize + LabelSize);
                });
                var assignableReferences = from obj in HierarchyWindow.instance.GetAllReferenceLinker()
                                           where obj != null
                                           where fieldType.IsAssignableFrom(obj.GetType())
                                           select obj;
                MyDropdownSelector.CreateOption("null", x =>
                {
                    if (x)
                        SetValue(null);
                });
                foreach (var item in assignableReferences)
                {
                    MyDropdownSelector.CreateOption(item.ToString(), x =>
                    {
                        if (x)
                            SetValue(item);
                    });
                }
                MyDropdownSelector.RefreshImmediate();
            }
            else
            {
                MyDropdownSelector.interactable = false;
            }
        }

        protected override void SetContainerSize(int size)
        {
            // button only fixed size
        }

        public override void SetInteractable(bool isInteractable)
        {
            //MyButton.interactable = isInteractable;
            MyDropdownSelector.interactable = isInteractable;
        }
    }
}
