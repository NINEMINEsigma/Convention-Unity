using UnityEngine;
using UnityEngine.UI;

namespace Convention.WindowsUI.Variant
{
    public class InspectorButton : InspectorDrawer
    {
        [Resources] public Button RawButton;
        [Resources] public ModernUIButton ModernButton;

        private void OnCallback()
        {
            targetItem.InvokeAction();
        }

        private void Start()
        {
            if (RawButton)
            {
                RawButton.onClick.AddListener(OnCallback);
                if (ModernButton)
                {
                    ModernButton.gameObject.SetActive(false);
                }
            }
            else if (ModernButton)
            {
                ModernButton.AddListener(OnCallback);
                if (targetItem.targetMemberInfo != null)
                    ModernButton.title = targetItem.targetMemberInfo.Name;
                else
                    ModernButton.title = "Invoke";
            }
        }

        private void OnEnable()
        {
            if (RawButton)
                RawButton.interactable = targetItem.AbleChangeType;
            if (ModernButton)
                ModernButton.interactable = targetItem.AbleChangeType;
        }

        private void Reset()
        {
            RawButton = GetComponent<Button>();
            ModernButton = GetComponent<ModernUIButton>();
        }
    }
}
