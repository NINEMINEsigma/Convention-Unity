using Convention.WindowsUI.Variant.InspectorComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;

namespace Convention.WindowsUI.Variant
{
    public static class InspectorUtility
    {
        private static InspectorTextField TextFieldPrefab => InspectorWindow.instance.TextFieldPrefab;
        private static InspectorToggle ToggleFieldPrefab => InspectorWindow.instance.ToggleFieldPrefab;
        private static InspectorImage TextureFieldPrefab => InspectorWindow.instance.TextureFieldPrefab;
        private static InspectorButton MethodFieldPrefab => InspectorWindow.instance.MethodFieldPrefab;
        private static InspectorReference ReferFieldPrefab => InspectorWindow.instance.ReferFieldPrefab;
        private static InspectorVec2 Vec2FieldPrefab => InspectorWindow.instance.Vec2FieldPrefab;
        private static InspectorVec3 Vec3FieldPrefab => InspectorWindow.instance.Vec3FieldPrefab;
        private static InspectorTransform TransformFieldPrefab => InspectorWindow.instance.TransformFieldPrefab;
        private static InspectorNumberField NumberFieldPrefab => InspectorWindow.instance.NumberFieldPrefab;
        private static InspectorColor ColorFieldPrefab => InspectorWindow.instance.ColorFieldPrefab;
        private static InspectorStructure StructFieldPrefab => InspectorWindow.instance.StructFieldPrefab;
        private static InspectorArray ArrayFieldPrefab => InspectorWindow.instance.ArrayFieldPrefab;
        private static InspectorEnum EnumFieldPrefab => InspectorWindow.instance.EnumFieldPrefab;
        private static Dictionary<Type, List<MemberInfo>> DrawPlaneFieldCache => InspectorWindow.instance.DrawPlaneFieldCache;
        public static T CreateItem<T>(T prefab, RectTransform ContentPlane, List<InspectorBaseItem> InspectorItemList) where T : InspectorBaseItem
        {
            var item = GameObject.Instantiate<T>(prefab, ContentPlane);
            item.gameObject.SetActive(true);
            InspectorItemList.Add(item);
            return item;
        }

        public static void CreateItem(RectTransform ContentPlane, List<InspectorBaseItem> InspectorItemList, InspectorDrawType type, object target, string title, Type targetType = null)
        {
            if (targetType == null)
                targetType = target.GetType();
            var item = type switch
            {
                InspectorDrawType.Auto => throw new InvalidOperationException("cannot create by auto"),
                InspectorDrawType.Text => (InspectorBaseItem)CreateItem(TextFieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Toggle => CreateItem(ToggleFieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Texture => CreateItem(TextureFieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Reference => CreateItem(ReferFieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Method => CreateItem(MethodFieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Vector3 => CreateItem(Vec3FieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Vector2 => CreateItem(Vec2FieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Color => CreateItem(ColorFieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Transform => CreateItem(TransformFieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Number => CreateItem(NumberFieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Structure => CreateItem(StructFieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Array => CreateItem(ArrayFieldPrefab, ContentPlane, InspectorItemList),
                InspectorDrawType.Enum => CreateItem(EnumFieldPrefab, ContentPlane, InspectorItemList),
                _ => throw new NotImplementedException(),
            };
            item.SetTarget(target, null, targetType, new() { IsInteractable = false, size = 1 });
            item.title = title;
        }

        public static void DrawInspector(object target, Type type, RectTransform ContentPlane, List<InspectorBaseItem> InspectorItemList)
        {
            if (DrawPlaneFieldCache.TryGetValue(type, out var fields) == false)
            {
                fields = (from field
                          in ConventionUtility.SeekMemberInfoFromType(type, new Type[] { typeof(InspectorDrawAttribute) }, null, null)
                          where field.HasAttribute<InspectorDrawAttribute>()
                          where field is FieldInfo || field is PropertyInfo || field is MethodInfo
                          select field
                         ).ToList();
                    //      (from field
                    //      in type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    //      where field.HasAttribute<InspectorDrawAttribute>()
                    //      select field).ToList();
                fields.Sort((x, y) =>
                {
                    int a = 0, b = 0;
                    if (x is MethodInfo)
                        a = 0;
                    else if (ConventionUtility.GetMemberValueType(x).IsAssignableFrom(typeof(Transform)))
                        a = 1;
                    if (y is MethodInfo)
                        b = 0;
                    else if (ConventionUtility.GetMemberValueType(y).IsAssignableFrom(typeof(Transform)))
                        b = 1;
                    return a - b;
                });
                DrawPlaneFieldCache.Add(type, fields);
            }
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<InspectorDrawAttribute>();
                var drawType = InspectorDrawType.Auto;
                var fieldName = field.Name;
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
                    CreateItem(MethodFieldPrefab, ContentPlane, InspectorItemList).SetTarget(target, fieldName, typeof(MethodInfo), config);
                }
                else
                {
                    var fieldType = ConventionUtility.GetMemberValueType(field);
                    if (drawType == InspectorDrawType.Auto)
                    {
                        CreateItemByAuto(target, ContentPlane, InspectorItemList, fieldName, config, fieldType);
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
                            InspectorDrawType.Transform => (InspectorBaseItem)TransformFieldPrefab,
                            InspectorDrawType.Number => (InspectorBaseItem)NumberFieldPrefab,
                            InspectorDrawType.Color => (InspectorBaseItem)ColorFieldPrefab,
                            InspectorDrawType.Structure => (InspectorBaseItem)StructFieldPrefab,
                            InspectorDrawType.Array => (InspectorBaseItem)ArrayFieldPrefab,
                            InspectorDrawType.Enum => (InspectorBaseItem)EnumFieldPrefab,
                            _ => null
                        };
                        if (prefab)
                            CreateItem(prefab, ContentPlane, InspectorItemList).SetTarget(target, fieldName, fieldType, config);
                        else
                        {
                            var textField = CreateItem(TextFieldPrefab, ContentPlane, InspectorItemList);
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

        private static InspectorBaseItem CreateItemByAuto(object target, RectTransform ContentPlane, List<InspectorBaseItem> InspectorItemList, string fieldName, InspectorDrawConfig config, Type fieldType)
        {
            if (fieldType == typeof(string))
            {
                var textField = CreateItem(TextFieldPrefab, ContentPlane, InspectorItemList);
                textField.SetTarget(target, fieldName, fieldType, config);
                return textField;
            }
            else if (Utility.IsNumber(fieldType))
            {
                var numberField = CreateItem(NumberFieldPrefab, ContentPlane, InspectorItemList);
                numberField.SetTarget(target, fieldName, fieldType, config);
                return numberField;
            }
            else if(fieldType.IsEnum)
            {
                var enumField = CreateItem(EnumFieldPrefab, ContentPlane, InspectorItemList);
                enumField.SetTarget(target, fieldName, fieldType, config);
                return enumField;
            }
            else if (fieldType == typeof(bool))
            {
                var toggleField = CreateItem(ToggleFieldPrefab, ContentPlane, InspectorItemList);
                toggleField.SetTarget(target, fieldName, fieldType, config);
                return toggleField;
            }
            else if (fieldType.IsAssignableFrom(typeof(Texture)))
            {
                var textureField = CreateItem(TextureFieldPrefab, ContentPlane, InspectorItemList);
                textureField.SetTarget(target, fieldName, fieldType, config);
                return textureField;
            }
            else if (fieldType == typeof(Vector2))
            {
                var vec2Field = CreateItem(Vec2FieldPrefab, ContentPlane, InspectorItemList);
                vec2Field.SetTarget(target, fieldName, fieldType, config);
                return vec2Field;
            }
            else if (fieldType == typeof(Vector3))
            {
                var vec3Field = CreateItem(Vec3FieldPrefab, ContentPlane, InspectorItemList);
                vec3Field.SetTarget(target, fieldName, fieldType, config);
                return vec3Field;
            }
            else if (fieldType.IsAssignableFrom(typeof(Transform)))
            {
                var transField = CreateItem(TransformFieldPrefab, ContentPlane, InspectorItemList);
                transField.SetTarget(target, fieldName, fieldType, config);
                return transField;
            }
            else if (fieldType == typeof(Color))
            {
                var colorField = CreateItem(ColorFieldPrefab, ContentPlane, InspectorItemList);
                colorField.SetTarget(target, fieldName, fieldType, config);
                return colorField;
            }
            else if (fieldType.IsArray || fieldType.IsSZArray || fieldType.Name.StartsWith("NativeArray"))
            {
                var arrayField = CreateItem(ArrayFieldPrefab, ContentPlane, InspectorItemList);
                arrayField.SetTarget(target, fieldName, fieldType, config);
                return arrayField;
            }
            else if (!fieldType.IsPrimitive)
            {
                if (fieldType.IsValueType)
                {
                    var structField = CreateItem(StructFieldPrefab, ContentPlane, InspectorItemList);
                    structField.SetTarget(target, fieldName, fieldType, config);
                    return structField;
                }
                else
                {
                    var refField = CreateItem(ReferFieldPrefab, ContentPlane, InspectorItemList);
                    refField.SetTarget(target, fieldName, fieldType, config);
                    return refField;
                }
            }
            else
            {
                var textField = CreateItem(TextFieldPrefab, ContentPlane, InspectorItemList);
                textField.SetTarget($"Unsupport {fieldType.GetFriendlyName()}", null, typeof(string), new InspectorDrawConfig()
                {
                    IsInteractable = false,
                    size = 1
                });
                return textField;
            }
        }

        public static void DrawArray(object target, RectTransform ContentPlane, List<InspectorBaseItem> InspectorItemList)
        {
            if (target == null)
            {

            }
            if (target is IEnumerable enumer)
            {
                var targetType = target.GetType();
                var itemOperator = targetType.GetProperty("Item");
                if (itemOperator == null)
                {
                    InspectorDrawConfig config = new()
                    {
                        IsInteractable = false,
                        size = 1
                    };
                    int i = 0;
                    foreach (var item in enumer)
                    {
                        CreateItemByAuto(item, ContentPlane, InspectorItemList, null, config, item == null ? typeof(string) : item.GetType())
                        .title = i++.ToString();
                    }
                }
                else
                {
                    InspectorDrawConfig config = new()
                    {
                        IsInteractable = true,
                        size = 1
                    };
                    int i = 0;
                    foreach (var item in enumer)
                    {
                        var arrayItem = CreateItemByAuto(item, ContentPlane, InspectorItemList, null, config, item == null ? typeof(string) : item.GetType());
                        int index = i;
                        arrayItem.title = index.ToString();
                        arrayItem.overrideGetter = () =>
                        {
                            return itemOperator.GetValue(target, new object[] { index });
                        };
                        arrayItem.overrideSetter = (value) =>
                        {
                            itemOperator.SetValue(target, value, new object[] { index });
                        };
                        i++;
                    }
                }
            }
            else
            {
                Debug.LogError("DrawArray target is not IEnumerable");
            }
        }
    }

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
            DrawPlaneFieldCache.Clear();
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
            InspectorItemList.Clear();
            target = null;
        }

        private T CreateItem<T>(T prefab) where T : InspectorBaseItem
        {
            return InspectorUtility.CreateItem(prefab, ContentPlane, InspectorItemList);
        }

        public void CreateItem(InspectorDrawType type, object target, string title, Type targetType = null)
        {
            InspectorUtility.CreateItem(ContentPlane, InspectorItemList, type, target, title, targetType);
        }

        internal readonly Dictionary<Type, List<MemberInfo>> DrawPlaneFieldCache = new();

        private void DrawInspector(object target, Type type)
        {
            ClassTypeField.text = type.GetFriendlyName();
            InspectorUtility.DrawInspector(target, type, ContentPlane, InspectorItemList);
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
            if (type == typeof(string))
            {
                var textField = CreateItem(TextFieldPrefab);
                textField.SetTarget(target.ToString(), null, type, defaultConfig);
            }
            else if (Utility.IsNumber(type))
            {
                var numberField = CreateItem(NumberFieldPrefab);
                numberField.SetTarget(target, null, type, defaultConfig);
            }
            else if (type == typeof(bool))
            {
                var toggleField = CreateItem(ToggleFieldPrefab);
                toggleField.SetTarget(target, null, type, defaultConfig);
            }
            else if (type.IsAssignableFrom(typeof(Texture)))
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
            else if (type.IsAssignableFrom(typeof(Transform)))
            {
                defaultConfig.IsInteractable = true;
                var transField = CreateItem(TransformFieldPrefab);
                transField.SetTarget(target, null, type, defaultConfig);
            }
            else if (type == typeof(Color))
            {
                var colorField = CreateItem(ColorFieldPrefab);
                colorField.SetTarget(target, null, type, defaultConfig);
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
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorTextField TextFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorToggle ToggleFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorImage TextureFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorButton MethodFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorReference ReferFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorVec2 Vec2FieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorVec3 Vec3FieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorTransform TransformFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorNumberField NumberFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorColor ColorFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorStructure StructFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorArray ArrayFieldPrefab;
        [Resources, SerializeField, OnlyNotNullMode] internal InspectorEnum EnumFieldPrefab;

#if UNITY_EDITOR
        [Content, OnlyPlayMode]
        public void TestTextField()
        {
            CreateItem(TextFieldPrefab);
        }
        [Content, OnlyPlayMode]
        public void TestToggleField()
        {
            CreateItem(ToggleFieldPrefab);
        }
        [Content, OnlyPlayMode]
        public void TestTextureField()
        {
            CreateItem(TextureFieldPrefab);
        }
        [Content, OnlyPlayMode]
        public void TestMethodField()
        {
            CreateItem(MethodFieldPrefab);
        }
        [Content, OnlyPlayMode]
        public void TestVec3Field()
        {
            CreateItem(Vec3FieldPrefab);
        }
        [Content, OnlyPlayMode]
        public void TestNumberField()
        {
            CreateItem(NumberFieldPrefab);
        }
        [Content, OnlyPlayMode]
        public void TestColorField()
        {
            CreateItem(ColorFieldPrefab);
        }
        public class TestInspectorClass
        {
            public class MyTestClass
            {
                [InspectorDraw] public int intValue = 10;
                [InspectorDraw] public float floatValue = 3.14f;
                [InspectorDraw] public string stringValue = "Hello World";
                [InspectorDraw] public bool boolValue = true;
                [InspectorDraw] public Vector3 vec3Test = new(1, 2, 3);
                [InspectorDraw] public Color colorTest = Color.red;
            }
            public struct MyTestStruct
            {
                [InspectorDraw] public int intValue;
                [InspectorDraw] public float floatValue;
                [InspectorDraw] public string stringValue;
                [InspectorDraw] public bool boolValue;
                [InspectorDraw] public Vector3 vec3Test;
                [InspectorDraw] public Color colorTest;
            }
            [InspectorDraw] public int intValue = 10;
            [InspectorDraw] public float floatValue = 3.14f;
            [InspectorDraw] public string stringValue = "Hello World";
            [InspectorDraw] public bool boolValue = true;
            [InspectorDraw(InspectorDrawType.Reference)] public MyTestClass refTest = new();
            [InspectorDraw(InspectorDrawType.Structure, size: 6)] public MyTestStruct structTest = new();
            [InspectorDraw] public Vector3 vec3Test = new(1, 2, 3);
            [InspectorDraw] public Color colorTest = Color.red;
            [InspectorDraw] public int[] intArray = new int[6] {1,2,3,4,5,6 };
            [InspectorDraw] public NativeArray<float> floatArray = new NativeArray<float>(new float[4] { 0.1f, 0.2f, 0.3f, 0.4f }, Allocator.Persistent);
            [InspectorDraw] public MyTestClass[] classArray = new MyTestClass[3];
            [InspectorDraw] public MyTestStruct[] structArray = new MyTestStruct[3];
        }
        [Content, OnlyPlayMode]
        public void TestClass()
        {
            SetTarget(new TestInspectorClass());
        }
#endif
    }
}
