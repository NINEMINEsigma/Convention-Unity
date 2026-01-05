using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorVec3 : InspectorBaseItem
    {
        [Header("Vec3")]
        [Resources, SerializeField, OnlyNotNullMode] private GameObject MyPlane;
        [Resources, SerializeField, OnlyNotNullMode] private ModernUIInputField MyXField;
        [Resources, SerializeField, OnlyNotNullMode] private ModernUIInputField MyYField;
        [Resources, SerializeField, OnlyNotNullMode] private ModernUIInputField MyZField;
        [Setting, SerializeField] private int UnitSize = 100;

        public override void SetFolder(bool status)
        {
            MyPlane.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : UnitSize) + LabelSize);
        }

        public override void UpdateValue()
        {
            throw new System.NotImplementedException();
        }

        protected override void InitBindingEvent()
        {
            MyXField.AddListener((string value) =>
            {
                if (float.TryParse(value, out var x))
                {
                    var vec = GetValue<Vector3>();
                    vec.x = x;
                    SetValue(vec);
                }
            });
            MyYField.AddListener((string value) =>
            {
                if (float.TryParse(value, out var y))
                {
                    var vec = GetValue<Vector3>();
                    vec.y = y;
                    SetValue(vec);
                }
            });
            MyZField.AddListener((string value) =>
            {
                if (float.TryParse(value, out var z))
                {
                    var vec = GetValue<Vector3>();
                    vec.z = z;
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
            MyZField.interactable = isInteractable;
        }
    }
}
