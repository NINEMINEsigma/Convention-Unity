using Convention.WindowsUI.Variant.InspectorComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UnityEngine;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorStructure : InspectorBaseItem
    {
        [Content, SerializeField] private List<InspectorBaseItem> InspectorItemList = new();
        [Resources] public RectTransform ContentPlane;
        [Header("Structure")]
        [Resources, SerializeField] private RectTransform MyPlane;
        [Setting, SerializeField] private int UnitSize = 50;
        private int CacheSize = 50;
        private bool isInteractable = true;

        public override void SetFolder(bool status)
        {
            MyPlane.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : CacheSize) + LabelSize);
        }

        private void DrawInspector(object target, Type type)
        {
            InspectorUtility.DrawInspector(target, type, ContentPlane, InspectorItemList);
        }

        protected override void InitBindingEvent()
        {
            
        }

        protected override void SetContainerSize(int size)
        {
            CacheSize = Mathf.Max(100, size * UnitSize);
        }

        public override void SetInteractable(bool isInteractable)
        {
            this.isInteractable = isInteractable;
        }

        public override void UpdateValue()
        {
            DrawInspector(GetValue<object>(), SafeType);
            foreach (var item in InspectorItemList)
            {
                item.SetInteractable(isInteractable);
            }
        }
    }
}
