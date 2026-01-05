using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorTransform : InspectorBaseItem
    {
        [Header("Transform")]
        [Resources, SerializeField, OnlyNotNullMode] private GameObject MyPlane;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField PosXField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField PosYField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField PosZField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField RotXField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField RotYField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField RotZField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField ScaXField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField ScaYField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField ScaZField;
        [Setting, SerializeField] private int UnitSize = 130;

        public override void SetFolder(bool status)
        {
            MyPlane.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : UnitSize) + LabelSize);
        }

        public override void UpdateValue()
        {
            var transformValue = GetValue<Transform>();
            PosXField.text = transformValue.localPosition.x.ToString();
            PosYField.text = transformValue.localPosition.y.ToString();
            PosZField.text = transformValue.localPosition.z.ToString();
            RotXField.text = transformValue.localEulerAngles.x.ToString();
            RotYField.text = transformValue.localEulerAngles.y.ToString();
            RotZField.text = transformValue.localEulerAngles.z.ToString();
            ScaXField.text = transformValue.localScale.x.ToString();
            ScaYField.text = transformValue.localScale.y.ToString();
            ScaZField.text = transformValue.localScale.z.ToString();
        }

        protected override void InitBindingEvent()
        {
            var transformValue = GetValue<Transform>();
            PosXField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var x))
                {
                    var vec = transformValue.localPosition;
                    vec.x = x;
                    transformValue.localPosition = vec;
                }
            });
            PosYField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var y))
                {
                    var vec = transformValue.localPosition;
                    vec.y = y;
                    transformValue.localPosition = vec;
                }
            });
            PosZField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var z))
                {
                    var vec = transformValue.localPosition;
                    vec.z = z;
                    transformValue.localPosition = vec;
                }
            });
            RotXField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var x))
                {
                    var vec = transformValue.localEulerAngles;
                    vec.x = x;
                    transformValue.localEulerAngles = vec;
                }
            });
            RotYField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var y))
                {
                    var vec = transformValue.localEulerAngles;
                    vec.y = y;
                    transformValue.localEulerAngles = vec;
                }
            });
            RotZField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var z))
                {
                    var vec = transformValue.localEulerAngles;
                    vec.z = z;
                    transformValue.localEulerAngles = vec;
                }
            });
            ScaXField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var x))
                {
                    var vec = transformValue.localScale;
                    vec.x = x;
                    transformValue.localScale = vec;
                }
            });
            ScaYField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var y))
                {
                    var vec = transformValue.localScale;
                    vec.y = y;
                    transformValue.localScale = vec;
                }
            });
            ScaZField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var z))
                {
                    var vec = transformValue.localScale;
                    vec.z = z;
                    transformValue.localScale = vec;
                }
            });
        }

        protected override void SetContainerSize(int size)
        {
            // fixed size
        }

        protected override void SetInteractable(bool isInteractable)
        {
            PosXField.interactable = isInteractable;
            PosYField.interactable = isInteractable;
            PosZField.interactable = isInteractable;
            RotXField.interactable = isInteractable;
            RotYField.interactable = isInteractable;
            RotZField.interactable = isInteractable;
            ScaXField.interactable = isInteractable;
            ScaYField.interactable = isInteractable;
            ScaZField.interactable = isInteractable;
        }
    }
}
