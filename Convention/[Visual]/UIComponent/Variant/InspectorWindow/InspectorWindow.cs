using Convention.WindowsUI.Variant.InspectorComponent;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Convention.WindowsUI.Variant
{
    public class InspectorWindow : WindowsComponent
    {
        public static InspectorWindow instance { get; private set; }
        private void Awake()
        {
            if (instance != null)
            {
                throw new InvalidProgramException("mutil-InspectorWindow is awake");
            }
            instance = this;
        }

        private object target;
        [Content, SerializeField] private List<InspectorBaseItem> InspectorItemList = new();
        [Resources] public RectTransform ContentPlane;
        [Resources, SerializeField] private Text ClassTypeField;

        public object GetTarget()
        {
            return target;
        }

        [Content]
        public void ClearWindow()
        {
            if (target == null)
                return;
            foreach (var item in InspectorItemList)
            {
                GameObject.Destroy(item.gameObject);
            }
            target = null;
        }

        private T CreateItem<T>(T prefab) where T : InspectorBaseItem
        {
            var item = GameObject.Instantiate<T>(prefab, ContentPlane);
            item.gameObject.SetActive(true);
            InspectorItemList.Add(item);
            return item;
        }

        private void DrawInspector(object target, Type type)
        {
            ClassTypeField.text = type.GetFriendlyName();
            var fields = from field
                         in type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         where field.HasAttribute<InspectorDrawAttribute>()
                         select field;
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<InspectorDrawAttribute>();
                var drawType = InspectorDrawType.Auto;
                var config = new InspectorDrawConfig()
                {
                    IsInteractable = false,
                    size = 1
                };
                if (attr != null)
                {
                    drawType = attr.drawType;
                    config = attr.config;
                }
                if (field is MethodInfo methodInfo)
                {
                }
                else
                {
                    var fieldType = ConventionUtility.GetMemberValueType(field);
                    var fieldName = field.Name;
                    if (drawType == InspectorDrawType.Auto)
                    {
                        if (fieldType == typeof(string) || Utility.IsNumber(fieldType))
                        {
                            var textField = CreateItem(TextFieldPrefab);
                            textField.SetTarget(target, fieldName, fieldType, config);
                        }
                        else if (fieldType == typeof(bool))
                        {
                            var toggleField = CreateItem(ToggleFieldPrefab);
                            toggleField.SetTarget(target, fieldName, fieldType, config);
                        }
                        else if (fieldType.IsSubclassOf(typeof(Texture)))
                        {
                            var textureField = CreateItem(TextureFieldPrefab);
                            textureField.SetTarget(target, fieldName, fieldType, config);
                        }
                        else if (fieldType == typeof(Vector2))
                        {
                            var vec2Field = CreateItem(Vec2FieldPrefab);
                            vec2Field.SetTarget(target, fieldName, fieldType, config);
                        }
                        else if (fieldType == typeof(Vector3))
                        {
                            var vec3Field = CreateItem(Vec3FieldPrefab);
                            vec3Field.SetTarget(target, fieldName, fieldType, config);
                        }
                        else
                        {
                            var textField = CreateItem(TextFieldPrefab);
                            textField.SetTarget($"Unsupport {fieldType.GetFriendlyName()}", null, typeof(string), new InspectorDrawConfig()
                            {
                                IsInteractable = false,
                                size = 1
                            });
                        }
                    }
                    else
                    {
                        var prefab = drawType switch
                        {
                            InspectorDrawType.Text => (InspectorBaseItem)TextFieldPrefab,
                            InspectorDrawType.Toggle => (InspectorBaseItem)ToggleFieldPrefab,
                            InspectorDrawType.Texture => (InspectorBaseItem)TextFieldPrefab,
                            InspectorDrawType.Reference => (InspectorBaseItem)ReferFieldPrefab,
                            InspectorDrawType.Method => (InspectorBaseItem)MethodFieldPrefab,
                            InspectorDrawType.Vector2 => (InspectorBaseItem)Vec2FieldPrefab,
                            InspectorDrawType.Vector3 => (InspectorBaseItem)Vec3FieldPrefab,
                            _ => null
                        };
                        if (prefab)
                            CreateItem(prefab).SetTarget(target, fieldName, fieldType, config);
                        else
                        {
                            var textField = CreateItem(TextFieldPrefab);
                            textField.SetTarget($"Unsupport {fieldType.GetFriendlyName()}", null, type, new InspectorDrawConfig()
                            {
                                IsInteractable = false,
                                size = 1
                            });
                        }
                    }
                }
            }
        }

        public void SetTarget(object target)
        {
            if (this.target == target)
            {
                foreach (var item in InspectorItemList)
                {
                    item.UpdateValue();
                }
                return;
            }
            ClearWindow();
            if (target == null)
                return;
            this.target = target;
            var type = target.GetType();
            var defaultConfig = new InspectorDrawConfig()
            {
                IsInteractable = false,
                size = 1
            };
            if (type == typeof(string) || Utility.IsNumber(type))
            {
                var textField = CreateItem(TextFieldPrefab);
                textField.SetTarget(target.ToString(), null, type, defaultConfig);
            }
            else if (type == typeof(bool))
            {
                var toggleField = CreateItem(ToggleFieldPrefab);
                toggleField.SetTarget(target, null, type, defaultConfig);
            }
            else if (type.IsSubclassOf(typeof(Texture)))
            {
                var textureField = CreateItem(TextureFieldPrefab);
                textureField.SetTarget(target, null, type, defaultConfig);
            }
            else if (type == typeof(Vector2))
            {
                var vec2Field = CreateItem(Vec2FieldPrefab);
                vec2Field.SetTarget(target, null, type, defaultConfig);

            }
            else if (type == typeof(Vector3))
            {
                var vec3Field = CreateItem(Vec3FieldPrefab);
                vec3Field.SetTarget(target, null, type, defaultConfig);
            }
            else if (type.IsValueType)
            {
                var textField = CreateItem(TextFieldPrefab);
                textField.SetTarget($"Unsupport {type.GetFriendlyName()}", null, type, defaultConfig);
            }
            else
            {
                DrawInspector(target, type);
            }
        }

        [Header("Inspector Items")]
        [Resources, SerializeField, OnlyNotNullMode] private InspectorTextField TextFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] private InspectorToggle ToggleFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] private InspectorImage TextureFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] private InspectorButton MethodFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] private InspectorReference ReferFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] private InspectorVec2 Vec2FieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] private InspectorVec3 Vec3FieldPrefab;

    }
}
