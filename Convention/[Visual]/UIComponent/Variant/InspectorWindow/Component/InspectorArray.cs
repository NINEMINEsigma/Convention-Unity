using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorArray : InspectorBaseItem
    {
        [Content, SerializeField] private List<InspectorBaseItem> InspectorItemList = new();
        [Resources] public RectTransform ContentPlane;
        [Header("Array")]
        [Resources, SerializeField] private RectTransform MyPlane;
        [Setting, SerializeField] private int UnitSize = 50;
        private int CacheSize = 50;
        private bool isInteractable = true;

        protected override void InitBindingEvent()
        {
            
        }

        public override void SetFolder(bool status)
        {
            MyPlane.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : CacheSize) + LabelSize);
        }

        protected override void SetContainerSize(int size)
        {
            CacheSize = Mathf.Max(100, size * UnitSize);
        }

        public override void SetInteractable(bool isInteractable)
        {
            this.isInteractable = isInteractable;
        }

        public void ClearDraw()
        {
            foreach (var item in InspectorItemList)
            {
                Destroy(item.gameObject);
            }
            InspectorItemList.Clear();
        }

        public override void UpdateValue()
        {
            ClearDraw();
            InspectorUtility.DrawArray(GetValue<object>(), ContentPlane, InspectorItemList);
            foreach (var item in InspectorItemList)
            {
                item.SetInteractable(isInteractable);
                item.UpdateValue();
            }
        }
    }
}
