using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorVec2 : InspectorBaseItem
    {
        [Header("Vec2")]
        [Resources, SerializeField, OnlyNotNullMode] private GameObject MyPlane;
        [Resources, SerializeField, OnlyNotNullMode] private ModernUIInputField MyXField;
        [Resources, SerializeField, OnlyNotNullMode] private ModernUIInputField MyYField;
        [Setting, SerializeField] private int UnitSize = 70;

        public override void SetFolder(bool status)
        {
            MyPlane.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : UnitSize) + LabelSize);
        }

        public override void UpdateValue()
        {
            var value = GetValue<Vector2>();
            MyXField.text = value.x.ToString();
            MyYField.text = value.y.ToString();
        }

        protected override void InitBindingEvent()
        {
            MyXField.AddListener((string value) =>
            {
                if (float.TryParse(value, out var x))
                {
                    var vec = GetValue<Vector2>();
                    vec.x = x;
                    SetValue(vec);
                }
            });
            MyYField.AddListener((string value) =>
            {
                if (float.TryParse(value, out var y))
                {
                    var vec = GetValue<Vector2>();
                    vec.y = y;
                    SetValue(vec);
                }
            });
        }

        protected override void SetContainerSize(int size)
        {
            // fixed size
        }

        protected override void SetInteractable(bool isInteractable)
        {
            MyXField.interactable = isInteractable;
            MyYField.interactable = isInteractable;
        }
    }
}
