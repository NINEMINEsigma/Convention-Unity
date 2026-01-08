using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

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
        // Texture
        Texture = 1 << 2,
        // Object
        Reference = 1 << 3,
        // Method
        Method = 1 << 4,
        // Vec3
        Vector3 = 1 << 5,
        // Vec2
        Vector2 = 1 << 6,
        // Color
        Color = 1 << 7,
        // Transform
        Transform = 1 << 8,
        // Number
        Number = 1 << 9,
        // Structure
        Structure = 1 << 10,
        // Array
        Array = 1 << 11,
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
        public Func<object> overrideGetter = null;
        public Action<object> overrideSetter = null;
        private string memberName = null;
        public string SafeMemberName => memberName ?? "null";
        private Type type;
        public Type SafeType => type ?? typeof(object);
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        public T GetValue<T>()
        {
            if (overrideGetter != null)
            {
                var result = overrideGetter();
                if (result.GetType().IsAssignableFrom(typeof(T)))
                    return (T)result;
                return (T)Convert.ChangeType(overrideGetter(), typeof(T));
            }
            if (typeof(T) == typeof(object))
            {
                if (string.IsNullOrEmpty(memberName))
                    return (T)target;
                else
                    return (T)ConventionUtility.SeekValue(target, memberName, type, DefaultBindingFlags);
            }
            if (target == null)
                return (T)ConventionUtility.GetDefault(typeof(T));
            if (string.IsNullOrEmpty(memberName))
                return (T)Convert.ChangeType(target, typeof(T));
            return (T)Convert.ChangeType(ConventionUtility.SeekValue(target, memberName, type, DefaultBindingFlags), typeof(T));
        }
        public void SetValue(object value)
        {
            if (overrideSetter != null)
            {
                if (value == null)
                    overrideSetter(ConventionUtility.GetDefault(type));
                else
                    overrideSetter(type == typeof(object) ? value : Convert.ChangeType(value, type));
                return;
            }
            if (memberName == null)
                throw new InvalidOperationException("Cannot set value to null memberName");
            if (value == null)
                ConventionUtility.PushValue(target, ConventionUtility.GetDefault(type), memberName, DefaultBindingFlags);
            else
                ConventionUtility.PushValue(target, type == typeof(object) ? value : Convert.ChangeType(value, type), memberName, DefaultBindingFlags);
        }
        public void InvokeMember()
        {
            if (memberName == null)
                throw new InvalidOperationException("Cannot invoke null memberName");
            if (ConventionUtility.TryInvokeMember(target.GetType().GetMethod(memberName, DefaultBindingFlags), target, out var _) == false)
                Debug.LogWarning($"Invoke member {memberName} failed");
        }
        public const int LabelSize = 20;

        [Setting, SerializeField, OnlyPlayMode] private bool isFolder = false;
        public bool IsFolder => isFolder;
        [Header("Inspector Item")]
        [Resources, SerializeField, OnlyNotNullMode] private Text ItemTitle;
        [Resources, SerializeField, OnlyNotNullMode] private UnityEngine.UI.Button FoldButton;

        private void Start()
        {
            FoldButton.onClick.AddListener(SwitchFolder);
            InitBindingEvent();
        }
        protected abstract void InitBindingEvent();

        public string title { get => ((ITitle)ItemTitle).title; set => ((ITitle)ItemTitle).title = value; }

        public void SetTarget([In] object target, [Opt] string memberName, [In] Type type, [In] InspectorDrawConfig config)
        {
            if (this.target != target)
            {
                this.target = target;
                this.title = string.IsNullOrEmpty(memberName) ? type.GetFriendlyName() : memberName;
                this.memberName = memberName;
                this.type = type;
                SetContainerSize(config.size);
                SetInteractable(config.IsInteractable);
                UpdateValue();
            }
        }
        public abstract void SetFolder(bool status);
        protected abstract void SetContainerSize(int size);
        public abstract void SetInteractable(bool isInteractable);
        [Setting, OnlyPlayMode]
        public void SwitchFolder()
        {
            isFolder = !isFolder;
            SetFolder(isFolder);
        }
        [Setting, OnlyPlayMode]
        public abstract void UpdateValue();
    }
}
