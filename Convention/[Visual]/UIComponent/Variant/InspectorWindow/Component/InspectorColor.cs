using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Convention.WindowsUI.Variant.InspectorComponent
{
    public class InspectorColor : InspectorBaseItem
    {
        [Header("Color")]
        [Resources, SerializeField, OnlyNotNullMode] private GameObject MyPlane;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField MyRField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField MyGField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField MyBField;
        [Resources, SerializeField, OnlyNotNullMode] private TMPro.TMP_InputField MyAField;
        [Resources, SerializeField, OnlyNotNullMode] private Image MyImage;
        [Setting, SerializeField] private int UnitSize = 50;

        public override void SetFolder(bool status)
        {
            MyPlane.gameObject.SetActive(status == false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (status ? 0 : UnitSize) + LabelSize);
        }

        public override void UpdateValue()
        {
            var value = GetValue<Color>();
            MyRField.text = value.r.ToString();
            MyGField.text = value.g.ToString();
            MyBField.text = value.b.ToString();
            MyAField.text = value.a.ToString();
            MyImage.color = value;
        }

        protected override void InitBindingEvent()
        {
            MyRField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var x))
                {
                    var color = GetValue<Color>();
                    color.r = x;
                    SetValue(color);
                    MyImage.color = color;
                }
            });
            MyGField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var y))
                {
                    var color = GetValue<Color>();
                    color.g = y;
                    SetValue(color);
                    MyImage.color = color;
                }
            });
            MyBField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var z))
                {
                    var color = GetValue<Color>();
                    color.b = z;
                    SetValue(color);
                    MyImage.color = color;
                }
            });
            MyAField.onEndEdit.AddListener((string value) =>
            {
                if (float.TryParse(value, out var a))
                {
                    var color = GetValue<Color>();
                    color.a = a;
                    SetValue(color);
                    MyImage.color = color;
                }
            });
        }

        protected override void SetContainerSize(int size)
        {
            // fixed size
        }

        public override void SetInteractable(bool isInteractable)
        {
            MyRField.interactable = isInteractable;
            MyGField.interactable = isInteractable;
            MyBField.interactable = isInteractable;
            MyAField.interactable = isInteractable;
        }
    }
}
