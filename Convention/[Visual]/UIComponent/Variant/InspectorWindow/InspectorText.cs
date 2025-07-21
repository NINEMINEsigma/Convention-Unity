using System;
using UnityEngine;

namespace Convention.WindowsUI.Variant
{
    public class InspectorText : InspectorDrawer
    {
        [Resources] public ModernUIInputField TextArea;
        [Content] public bool isEditing = false;

        private void OnCallback(string str)
        {
            Type[] paramaters = new Type[] { typeof(string), targetItem.GetValueType().MakeByRefType() };
            var parser = targetItem.GetValueType().GetMethod(nameof(float.Parse), paramaters);
            if (parser != null)
            {
                object out_value = ConventionUtility.GetDefault(targetItem.GetValueType());
                if ((bool)parser.Invoke(null, new object[] { str, out_value }))
                {
                    targetItem.SetValue(out_value);
                }
            }
            else
            {
                targetItem.SetValue(str);
            }
            if (targetItem.target is IInspectorUpdater updater)
            {
                updater.OnInspectorUpdate();
            }
        }

        private void Start()
        {
            TextArea.AddListener(OnCallback);
            TextArea.InputFieldSource.Source.onEndEdit.AddListener(x => isEditing = false);
            TextArea.InputFieldSource.Source.onSelect.AddListener(x => isEditing = true);
        }

        private void OnEnable()
        {
            TextArea.InputFieldSource.Source.readOnly = !targetItem.AbleChangeType;
            if (targetItem.AbleChangeType)
            {
                try
                {
                    TextArea.interactable = targetItem.GetValueType().GetMethod(nameof(float.Parse)) != null || ConventionUtility.IsString(targetItem.GetValueType());
                }
                catch (Exception) { }
            }
            var value = targetItem.GetValue();
            TextArea.text = value == null ? "" : value.ToString();
        }

        private void FixedUpdate()
        {
            if (targetItem.UpdateType && !isEditing)
            {
                TextArea.text = targetItem.GetValue().ToString();
            }
        }

        private void Reset()
        {
            TextArea = GetComponent<ModernUIInputField>();
        }
    }
}
