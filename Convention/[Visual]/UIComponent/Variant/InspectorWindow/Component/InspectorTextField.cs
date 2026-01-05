using UnityEngine;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorTextField : InspectorBaseItem
    {
        [Header("Text")]
        [Resources, SerializeField, OnlyNotNullMode] private InputField MyTextField;
        [Setting, SerializeField] private int UnitSize = 50;
        private int CacheSize;

        public override void SetFolder(bool status)
        {
            MyTextField.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : CacheSize) + LabelSize);
        }

        public override void UpdateValue()
        {
            MyTextField.text = GetValue<string>() ?? "null";
        }

        protected override void InitBindingEvent()
        {
            MyTextField.AddListener((string value) =>
            {
                SetValue(value);
            });
        }

        protected override void SetContainerSize(int size)
        {
            CacheSize = size * UnitSize;
            if (IsFolder == false)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CacheSize + LabelSize);
        }

        protected override void SetInteractable(bool isInteractable)
        {
            MyTextField.interactable = isInteractable;
        }
    }
}
