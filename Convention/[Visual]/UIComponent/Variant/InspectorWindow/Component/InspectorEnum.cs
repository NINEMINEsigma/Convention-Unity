using System;
using UnityEngine;


namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorEnum : InspectorBaseItem
    {
        [Header("Enum")]
        [Resources, SerializeField, OnlyNotNullMode] private ModernUIDropdown MyDropdown;
        [Setting, SerializeField] private int UnitSize = 80;
        [Setting, SerializeField] private int DropdownListSize = 300;

        public override void SetFolder(bool status)
        {
            MyDropdown.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : UnitSize) + LabelSize);
        }

        public override void SetInteractable(bool isInteractable)
        {
            MyDropdown.interactable = isInteractable;
        }

        public override void UpdateValue()
        {
            var value = GetValue<object>();
            if (MyDropdown.Select(value.ToString()) == false)
                MyDropdown.title = value.ToString();
        }

        protected override void InitBindingEvent()
        {
            MyDropdown.DropdownOnEvent.AddListener(x =>
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (x ? DropdownListSize : 0) + UnitSize + LabelSize);
            });
            var enumType = GetValue<object>().GetType();
            foreach (var item in Enum.GetNames(enumType))
            {
                MyDropdown.CreateOption(item, x =>
                {
                    if (x)
                        SetValue(Enum.Parse(enumType, item));
                });
            }
            MyDropdown.RefreshImmediate();
        }

        protected override void SetContainerSize(int size)
        {
            // fixed
        }
    }
}
