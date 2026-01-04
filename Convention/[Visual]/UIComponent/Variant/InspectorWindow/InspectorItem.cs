using System;
using System.Reflection;
using UnityEngine;

namespace Convention.WindowsUI.Variant
{
    public enum InspectorDrawType
    {
        // Auto
        Auto = -1,
        // String
        Text = 0,
        // Bool
        Toggle = 1 << 1,
        // Sripte
        Image = 1 << 2,
        // Transform
        Transform = 1 << 3,
        // Container
        List = 1 << 4 + 1, Dictionary = 1 << 5 + 1, Array = 1 << 6 + 1,
        // Object
        Reference = 1 << 7, Structure = 1 << 8,
        // Method
        Button = 1 << 9,
        // Enum
        Enum = 1 << 10
    }

    public struct InspectorDrawConfig
    {
        public bool IsInteractable;
        public int size;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class InspectorDrawAttribute : Attribute
    {
        public InspectorDrawType drawType;
        public readonly InspectorDrawConfig config;

        public InspectorDrawAttribute(
            InspectorDrawType drawType = InspectorDrawType.Auto,
            bool isInteractable = true,
            int size = 1
            )
        {
            this.drawType = drawType;
            this.config = new()
            {
                IsInteractable = isInteractable,
                size = size
            };
        }
    }

    public interface IOnlyFocusThisOnInspector
    {

    }

    public abstract class InspectorBaseItem : WindowsComponent, ITitle
    {
        private object target = null;
        private string memberName = null;
        private Type type;
        public object GetValue()
        {
            return ConventionUtility.SeekValue(target, memberName, type, BindingFlags.Default);
        }
        public void SetValue(object value)
        {
            ConventionUtility.PushValue(target, value, memberName, BindingFlags.Default);
        }
        public const int LabelSize = 20;

        [Setting, SerializeField, OnlyPlayMode] private bool isFolder = false;
        [Header("Inspector Item")]
        [Resources, SerializeField, OnlyNotNullMode] private Text ItemTitle;
        [Resources, SerializeField, OnlyNotNullMode] private UnityEngine.UI.Button FoldButton;

        private void Start()
        {
            FoldButton.onClick.AddListener(() =>
            {
                SetFolder(!isFolder); 
            });
            InitBindingEvent();
        }
        protected abstract void InitBindingEvent();

        public string title { get => ((ITitle)ItemTitle).title; set => ((ITitle)ItemTitle).title = value; }

        public void SetTarget([In] object target, [In]string name, [In]Type type, [In] InspectorDrawConfig config)
        {
            if (this.target != target)
            {
                this.target = target;
                this.title = name;
                this.memberName = name;
                this.type = type;
                SetContainerSize(config.size);
                SetInteractable(config.IsInteractable);
                UpdateValue();
            }
        }
        public abstract void SetFolder(bool status);
        protected abstract void SetContainerSize(int size);
        protected abstract void SetInteractable(bool isInteractable);
        [Setting]
        public void SwitchFolder()
        {
            isFolder = !isFolder;
            SetFolder(isFolder);
        }
        [Setting]
        public abstract void UpdateValue();
    }
}
