using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorVec3 : InspectorBaseItem
    {
        [Header("Vec3")]
        [Resources, SerializeField, OnlyNotNullMode] private GameObject MyPlane;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField MyXField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField MyYField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField MyZField;
        [Setting, SerializeField] private int UnitSize = 30;

        public override void SetFolder(bool status)
        {
            MyPlane.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : UnitSize) + LabelSize);
        }

        public override void UpdateValue()
        {
            var value = GetValue<Vector3>();
            MyXField.text = value.x.ToString();
            MyYField.text = value.y.ToString();
            MyZField.text = value.z.ToString();
        }

        protected override void InitBindingEvent()
        {
            MyXField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var x))
                {
                    var vec = GetValue<Vector3>();
                    vec.x = x;
                    SetValue(vec);
                }
            });
            MyYField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var y))
                {
                    var vec = GetValue<Vector3>();
                    vec.y = y;
                    SetValue(vec);
                }
            });
            MyZField.onEndEdit.AddListener((string value) =>
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

        public override void SetInteractable(bool isInteractable)
        {
            MyXField.interactable = isInteractable;
            MyYField.interactable = isInteractable;
            MyZField.interactable = isInteractable;
        }
    }
}
