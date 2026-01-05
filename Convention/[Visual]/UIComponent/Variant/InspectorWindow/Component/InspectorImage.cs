using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorImage : InspectorBaseItem
    {
        [Header("Toggle")]
        [Resources, SerializeField, OnlyNotNullMode] private ModernUIImage MyImage;
        [Setting, SerializeField] private int UnitSize = 80;

        public override void SetFolder(bool status)
        {
            MyImage.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : UnitSize) + LabelSize);
        }

        public override void UpdateValue()
        {
            MyImage.texture = GetValue<Texture>();
        }

        protected override void InitBindingEvent()
        {
            // no event
        }

        protected override void SetContainerSize(int size)
        {
            // image only fixed size
        }

        protected override void SetInteractable(bool isInteractable)
        {
            // image cannot interact
        }
    }
}
