using UnityEngine;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorNumberField : InspectorBaseItem
    {
        [Header("Text")]
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField MyTextField;
        [Setting, SerializeField] private int UnitSize = 30;

        public override void SetFolder(bool status)
        {
            MyTextField.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : UnitSize) + LabelSize);
        }

        public override void UpdateValue()
        {
            MyTextField.text = GetValue<string>() ?? "null";
        }

        protected override void InitBindingEvent()
        {
            MyTextField.onEndEdit.AddListener((string value) =>
            {
                SetValue(value);
            });
        }

        protected override void SetContainerSize(int size)
        {
            // fixed size
        }

        public override void SetInteractable(bool isInteractable)
        {
            MyTextField.interactable = isInteractable;
        }
    }
}
