using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorReference : InspectorBaseItem
    {
        [Header("Button")]
        [Resources, SerializeField, OnlyNotNullMode] private Button MyButton;
        [Setting, SerializeField] private int UnitSize = 30;

        public override void SetFolder(bool status)
        {
            MyButton.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : UnitSize) + LabelSize);
        }

        public override void UpdateValue()
        {

        }

        protected override void InitBindingEvent()
        {
            MyButton.onClick.AddListener(() =>
            {
                var obj = GetValue<object>();
                if (obj == null)
                    InspectorWindow.instance.ClearWindow();
                else
                    InspectorWindow.instance.SetTarget(obj);
            });
        }

        protected override void SetContainerSize(int size)
        {
            // button only fixed size
        }

        protected override void SetInteractable(bool isInteractable)
        {
            MyButton.interactable = isInteractable;
        }
    }
}
