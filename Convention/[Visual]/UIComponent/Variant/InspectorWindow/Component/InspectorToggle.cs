using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorToggle : InspectorBaseItem
    {
        [Header("Toggle")]
        [Resources, SerializeField, OnlyNotNullMode] private ModernUIToggle MyToggle;
        [Setting, SerializeField] private int UnitSize = 30;

        public override void SetFolder(bool status)
        {
            MyToggle.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : UnitSize) + LabelSize);
        }

        public override void UpdateValue()
        {
            MyToggle.ref_value = GetValue<bool>();
        }

        protected override void InitBindingEvent()
        {
            MyToggle.AddListener((bool value) =>
            {
                SetValue(value);
            });
        }

        protected override void SetContainerSize(int size)
        {
            // toggle only fixed size
        }

        protected override void SetInteractable(bool isInteractable)
        {
            MyToggle.interactable = isInteractable;
        }
    }
}
