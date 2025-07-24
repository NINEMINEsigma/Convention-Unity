using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Convention.WindowsUI;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityEditor
{

}

namespace Convention
{
    public static class PlatformIndicator
    {
#if DEBUG
        public static bool IsRelease => false;
#else
        public static bool IsRelease => true;
#endif
        public static bool IsPlatformWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsPlatformLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsPlatformOsx => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        public static bool IsPlatformX64 => System.Environment.Is64BitOperatingSystem;

        static PlatformIndicator()
        {
            MainThreadID = Thread.CurrentThread.ManagedThreadId;
        }

        public static int MainThreadID { get; private set; }
        public static bool CurrentThreadIsMainThread()
        {
            return MainThreadID == Thread.CurrentThread.ManagedThreadId;
        }

        public static string CompanyName = Application.companyName;

        public static string ProductName = Application.productName;

        public static string ApplicationPath => throw new NotSupportedException("Not support to get ApplicationPath");

        public static string StreamingAssetsPath => Application.streamingAssetsPath;

        public static string PersistentDataPath => Application.persistentDataPath;

        public static string DataPath => Application.dataPath;
    }

    public static partial class Utility
    {
        public static string ConvertString(object obj)
        {
            return Convert.ToString(obj);
        }
        public static T ConvertValue<T>(string str)
        {
            Type type = typeof(T);
            var parse_method = type.GetMethod("Parse");
            if (parse_method != null &&
                (parse_method.ReturnType.IsSubclassOf(type) || parse_method.ReturnType == type) &&
                parse_method.GetParameters().Length == 1 &&
                parse_method.GetParameters()[0].ParameterType == typeof(string))
            {
                return (T)parse_method.Invoke(null, new object[] { str });
            }

            throw new InvalidCastException($"\"{str}\" is cannt convert to type<{type}>");
        }


        public static object SeekValue(object obj, string name, BindingFlags flags, out bool isSucceed)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            isSucceed = true;
            if (field != null)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null)
            {
                return property.GetValue(obj);
            }
            isSucceed = false;
            return null;
        }
        public static object SeekValue(object obj, string name, BindingFlags flags)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            if (field != null)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null)
            {
                return property.GetValue(obj);
            }
            return null;
        }
        public static object SeekValue(object obj, string name, Type valueType, BindingFlags flags, out bool isSucceed)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            isSucceed = true;
            if (field != null && field.FieldType == valueType)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null && property.PropertyType == valueType)
            {
                return property.GetValue(obj);
            }
            isSucceed = false;
            return null;
        }
        public static object SeekValue(object obj, string name, Type valueType, BindingFlags flags)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            if (field != null && field.FieldType == valueType)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null && property.PropertyType == valueType)
            {
                return property.GetValue(obj);
            }
            return null;
        }
        public static bool PushValue(object obj, object value, string name, BindingFlags flags)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            if (field != null)
            {
                field.SetValue(obj, value);
                return true;
            }
            var property = type.GetProperty(name, flags);
            if (property != null)
            {
                property.SetValue(obj, value);
                return true;
            }
            return false;
        }

        public static List<Type> SeekType(Predicate<Type> pr, IEnumerable<Assembly> assemblys = null, int findCount = -1)
        {
            List<Type> types = new List<Type>();
            if (assemblys == null)
                assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (pr(type))
                        types.Add(type);
                    if (types.Count == findCount)
                        return types;
                }
            }
            return types;
        }

        public static List<MemberInfo> GetMemberInfos(Type type, IEnumerable<Type> cutOffType = null, bool isGetNotPublic = false, bool isGetStatic = false)
        {
            Type current = type;
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            List<MemberInfo> result = new();
            if (isGetNotPublic)
                flags |= BindingFlags.NonPublic;
            if (isGetStatic)
                flags |= BindingFlags.Static;
            while ((cutOffType != null && !cutOffType.Contains(current)) && current != null)
            {
                result.AddRange(current.GetFields(flags));
                result.AddRange(current.GetProperties(flags));
                result.AddRange(current.GetMethods(flags));
                current = current.BaseType;
            }
            return result;
        }

        public static bool IsNumber([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsNumber(type);
        }
        public static bool IsString([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsString(type);
        }
        public static bool IsBinary([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsBinary(type);
        }
        public static bool IsArray([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsArray(type);
        }
        public static bool IsBool([In] object data)
        {
            if (data == null) return false;
            return IsBool(data.GetType());
        }

        public static bool IsNumber([In] Type type)
        {
            return
                type == typeof(double) ||
                type == typeof(float) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(uint) ||
                type == typeof(ulong) ||
                type == typeof(char);
        }
        public static bool IsString([In] Type type)
        {
            return type == typeof(string) || type == typeof(char[]);
        }
        public static bool IsBinary([In] Type type)
        {
            return
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(byte[]) ||
                type == typeof(sbyte[]);
        }
        public static bool IsArray([In] Type type)
        {
            return type.IsArray;
        }
        public static bool IsBool([In] Type type)
        {
            return type == typeof(bool);
        }
        public static bool IsEnum([In] Type type)
        {
            return type.IsEnum;
        }

        public static bool HasCustomAttribute(MemberInfo member, IEnumerable<Type> attrs)
        {
            foreach (var attr in attrs)
            {
                if (member.GetCustomAttribute(attr, true) != null)
                    return true;
            }
            return false;
        }
        public static Type GetMemberValueType(MemberInfo member)
        {
            if (member is FieldInfo field)
            {
                return field.FieldType;
            }
            else if (member is PropertyInfo property)
            {
                return property.PropertyType;
            }
            return null;
        }
        public static bool GetMemberValueType(MemberInfo member, out Type type)
        {
            if (member is FieldInfo field)
            {
                type = field.FieldType;
                return true;
            }
            else if (member is PropertyInfo property)
            {
                type = property.PropertyType;
                return true;
            }
            type = null;
            return false;
        }
        public static void PushValue(object target, object value, MemberInfo info)
        {
            if (info is FieldInfo field)
            {
                if (value.GetType().IsSubclassOf(field.FieldType))
                    field.SetValue(target, value);
                else
                {
                    field.SetValue(target, field.FieldType.GetMethod(nameof(float.Parse)).Invoke(target, new object[] { value }));
                }
            }
            else if (info is PropertyInfo property)
            {
                property.SetValue(target, value);
            }
            else
            {
                throw new InvalidOperationException("info is unsupport");
            }
        }
        public static object SeekValue(object target, MemberInfo info)
        {
            if (info is FieldInfo field)
            {
                return field.GetValue(target);
            }
            else if (info is PropertyInfo property)
            {
                return property.GetValue(target);
            }
            else
            {
                throw new InvalidOperationException("info is unsupport");
            }
        }
        public static bool TrySeekValue(object target, MemberInfo info, out object value)
        {
            if (info is FieldInfo field)
            {
                value = field.GetValue(target);
                return true;
            }
            else if (info is PropertyInfo property)
            {
                value = property.GetValue(target);
                return true;
            }
            value = null;
            return false;
        }

        public static List<MemberInfo> SeekMemberInfo(object target, IEnumerable<Type> attrs, IEnumerable<Type> types, Type untilBase = null)
        {
            Type _CurType = target.GetType();
            List<MemberInfo> result = new();
            result.AddRange(_CurType.GetMembers(BindingFlags.Public | BindingFlags.Instance));
            while (_CurType != null && _CurType != typeof(object) && _CurType != untilBase)
            {
                result.AddRange(
                    from info in _CurType.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                    where attrs == null || HasCustomAttribute(info, attrs)
                    where types == null || (GetMemberValueType(info, out var type) && types.Contains(type))
                    select info
                    );
                _CurType = _CurType.BaseType;
            }
            return result;
        }
        public static List<MemberInfo> SeekMemberInfo(object target, IEnumerable<string> names, BindingFlags flags = BindingFlags.Default)
        {
            Type _CurType = target.GetType();
            List<MemberInfo> result = _CurType.GetMembers(flags).ToList();
            HashSet<string> nameSet = names.ToHashSet();
            result.RemoveAll(x => nameSet.Contains(x.Name) == false);
            return result;
        }
        public static object InvokeMember(MemberInfo member, object target, params object[] parameters)
        {
            if (member is MethodInfo method)
            {
                return method.Invoke(target, parameters);
            }
            return null;
        }
        public static bool TryInvokeMember(MemberInfo member, object target, out object returnValue, params object[] parameters)
        {
            returnValue = null;
            if (member is MethodInfo method)
            {
                returnValue = method.Invoke(target, parameters);
                return true;
            }
            else return false;
        }

        public static T Shared<T>(T target, out T value)
        {
            value = target;
            return value;
        }


        public static string NowFormat(string format = "yyyy-MM-dd_HH-mm-ss")
        {
            return DateTime.Now.ToString(format);
        }
    }
}

namespace Convention
{
    public static partial class ConventionUtility
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string convert_xstring([In] object obj)
        {
            return Convert.ToString(obj);
        }
        public static _T convert_xvalue<_T>([In] string str)
        {
            Type type = typeof(_T);
            var parse_method = type.GetMethod("Parse");
            if (parse_method != null &&
                (parse_method.ReturnType.IsSubclassOf(type) || parse_method.ReturnType == type) &&
                parse_method.GetParameters().Length == 1 &&
                parse_method.GetParameters()[0].ParameterType == typeof(string))
            {
                return (_T)parse_method.Invoke(null, new object[] { str });
            }

            throw new InvalidCastException($"\"{str}\" is cannt convert to type<{type}>");
        }
        public static string Combine([In] params object[] args)
        {
            if (args.Length == 0)
                return "";
            if (args.Length == 1)
                return args[0].ToString();
            return Combine(args[0]) + Combine(args[1..]);
        }
        public static string Trim([In] string str, int left_right_flag = 3)
        {
            string result = new string(str);
            if ((left_right_flag & (1 << 0)) == 1)
                result = result.TrimStart();
            if ((left_right_flag & (1 << 1)) == 1)
                result = result.TrimEnd();
            return result;
        }

        public static object SeekValue([In] object obj, [In] string name, BindingFlags flags, [Out][Opt] out bool isSucceed)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            isSucceed = true;
            if (field != null)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null)
            {
                return property.GetValue(obj);
            }
            isSucceed = false;
            return null;
        }
        public static object SeekValue([In] object obj, [In] string name, BindingFlags flags)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            if (field != null)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null)
            {
                return property.GetValue(obj);
            }
            return null;
        }
        public static object SeekValue([In] object obj, [In] string name, [In] Type valueType, BindingFlags flags, [Out][Opt] out bool isSucceed)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            isSucceed = true;
            if (field != null && field.FieldType == valueType)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null && property.PropertyType == valueType)
            {
                return property.GetValue(obj);
            }
            isSucceed = false;
            return null;
        }
        public static object SeekValue([In] object obj, [In] string name, [In] Type valueType, BindingFlags flags)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            if (field != null && field.FieldType == valueType)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null && property.PropertyType == valueType)
            {
                return property.GetValue(obj);
            }
            return null;
        }
        public static bool PushValue([In] object obj, [In] object value, [In] string name, BindingFlags flags)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            if (field != null)
            {
                field.SetValue(obj, value);
                return true;
            }
            var property = type.GetProperty(name, flags);
            if (property != null)
            {
                property.SetValue(obj, value);
                return true;
            }
            return false;
        }

        public static T GetOrAddComponent<T>(this MonoBehaviour self) where T : Component
        {
            if (self.GetComponents<T>().Length == 0)
            {
                return self.gameObject.AddComponent<T>();
            }
            else
            {
                return self.gameObject.GetComponents<T>()[0];
            }
        }
        public static T SeekComponent<T>(this MonoBehaviour self) where T : class
        {
            var results = self.gameObject.GetComponents<T>();
            if (results.Length == 0)
                return null;
            return results[0];
        }
        public static T GetOrAddComponent<T>(this GameObject self) where T : Component
        {
            if (self.GetComponents<T>().Length == 0)
            {
                return self.AddComponent<T>();
            }
            else
            {
                return self.GetComponents<T>()[0];
            }
        }
        public static T SeekComponent<T>(this GameObject self) where T : class
        {
            var results = self.GetComponents<T>();
            if (results.Length == 0)
                return null;
            return results[0];
        }

        public static List<Type> SeekType(Predicate<Type> pr, IEnumerable<Assembly> assemblys = null, int findCount = -1)
        {
            List<Type> types = new List<Type>();
            if (assemblys == null)
                assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (pr(type))
                        types.Add(type);
                    if (types.Count == findCount)
                        return types;
                }
            }
            return types;
        }

        public static List<MemberInfo> GetMemberInfos(Type type, IEnumerable<Type> cutOffType = null, bool isGetNotPublic = false, bool isGetStatic = false)
        {
            Type current = type;
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            List<MemberInfo> result = new();
            if (isGetNotPublic)
                flags |= BindingFlags.NonPublic;
            if (isGetStatic)
                flags |= BindingFlags.Static;
            while ((cutOffType != null && !cutOffType.Contains(current)) && current != null)
            {
                result.AddRange(current.GetFields(flags));
                result.AddRange(current.GetProperties(flags));
                result.AddRange(current.GetMethods(flags));
                current = current.BaseType;
            }
            return result;
        }
    }

    [Serializable]
    public class SALCheckException : Exception
    {
        public delegate bool Predicate(object val);
        public Attribute attribute;
        public SALCheckException(Attribute attribute) { this.attribute = attribute; }
        public SALCheckException(Attribute attribute, string message) : base(message) { this.attribute = attribute; }
        public SALCheckException(Attribute attribute, string message, Exception inner) : base(message, inner) { this.attribute = attribute; }
    }
    [System.AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public class InAttribute : Attribute { }
    [System.AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public class OutAttribute : Attribute { }
    [System.AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = true)]
    public class OptAttribute : Attribute { }
    [System.AttributeUsage(AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
    public class ReturnVirtualAttribute : Attribute { }
    [System.AttributeUsage(AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
    public class ReturnMayNullAttribute : Attribute { }
    [System.AttributeUsage(AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
    public class ReturnNotNullAttribute : Attribute { }
    [System.AttributeUsage(AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
    public class ReturnSelfAttribute : Attribute { }
    [System.AttributeUsage(AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
    public class ReturnNotSelfAttribute : Attribute { }
#if UNITY_2017_1_OR_NEWER
    [System.AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue |
        AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class IsInstantiatedAttribute : Attribute
    {
        public bool isInstantiated;
        public IsInstantiatedAttribute(bool isInstantiated)
        {
            this.isInstantiated = isInstantiated;
        }
    }
#endif
    [System.AttributeUsage(AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
    public class SucceedAttribute : Attribute
    {
        private SALCheckException.Predicate pr;
        public SucceedAttribute([In] object succeed_when_return_value_is_equal_this_value_or_pr_is_return_true)
        {
            var prm = succeed_when_return_value_is_equal_this_value_or_pr_is_return_true.GetType().GetMethod("Invoke");
            if (prm != null &&
                prm.GetParameters().Length == 1 &&
                prm.ReturnType == typeof(bool))
                this.pr = (SALCheckException.Predicate)succeed_when_return_value_is_equal_this_value_or_pr_is_return_true;
            else
                this.pr = (object obj) => obj == succeed_when_return_value_is_equal_this_value_or_pr_is_return_true;
        }

        public bool Check([In][Opt] object value)
        {
            if (this.pr(value))
                return true;
            throw new SALCheckException(this, $"return value<{value.ToString()[..25]}...> is not expect");
        }
    }
    [System.AttributeUsage(AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
    public class NotSucceedAttribute : Attribute
    {
        private SALCheckException.Predicate pr;
        public NotSucceedAttribute([In] object failed_when_return_value_is_equal_this_value_or_pr_is_return_true)
        {
            var prm = failed_when_return_value_is_equal_this_value_or_pr_is_return_true.GetType().GetMethod("Invoke");
            if (prm != null &&
                prm.GetParameters().Length == 1 &&
                prm.ReturnType == typeof(bool))
                this.pr = (SALCheckException.Predicate)failed_when_return_value_is_equal_this_value_or_pr_is_return_true;
            else
                this.pr = (object obj) => obj == failed_when_return_value_is_equal_this_value_or_pr_is_return_true;
        }

        public bool Check([In][Opt] object value)
        {
            if (this.pr(value))
                throw new SALCheckException(this, $"return value<{value.ToString()[..25]}...> is not expect");
            return true;
        }
    }
    [System.AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class MethodReturnSelfAttribute : Attribute { }
    [System.AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class MethodReturnNotSelfAttribute : Attribute { }
    [System.AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class WhenAttribute : Attribute
    {
        private SALCheckException.Predicate pr = null;
        public readonly Type TypenAttribute = null;
        /// <summary>
        /// <list type="bullet">bool predicate(object)+typenAttribute</list>
        /// <list type="bullet">value+typenAttribute</list>
        /// </summary>
        /// <param name="control_value_or_predicate">The value will been checked or predicate</param>
        /// <param name="typenAttribute">Target Checker</param>
        public WhenAttribute([In] object control_value_or_predicate, [In] Type typenAttribute)
        {
            this.TypenAttribute = typenAttribute;
            if (typenAttribute == typeof(OnlyNotNullModeAttribute))
            {
                if (ConventionUtility.IsString(control_value_or_predicate))
                {
                    this.pr = (object obj) =>
                    {
                        return new OnlyNotNullModeAttribute((string)control_value_or_predicate).Check(obj);
                    };
                    return;
                }
            }
            else if (
                typenAttribute.Name.EndsWith("SucceedAttribute") ||
                typenAttribute.Name.StartsWith("Return")
                )
            {
                return;
            }
            do
            {
                var prm = control_value_or_predicate.GetType().GetMethod("Invoke");
                if (prm != null &&
                    prm.GetParameters().Length == 1 &&
                    prm.ReturnType == typeof(bool))
                    this.pr = (SALCheckException.Predicate)control_value_or_predicate;
                else
                    this.pr = (object obj) => obj == control_value_or_predicate;
            } while (false);
        }
        /// <summary>
        /// do nothing
        /// </summary>
        /// <param name="description"></param>
        public WhenAttribute([In] string description) { }
        protected WhenAttribute() { }

#if UNITY_EDITOR
        /// <summary>
        /// The value is <see cref="UnityEditor.Editor.target"/>
        /// <list type="bullet"><b><see cref="TypenAttribute"/> is <see cref="OnlyNotNullModeAttribute"/>:</b>member value is not null</list>
        /// <list type="bullet"><b>Default:</b>predicate(target)</list>
        /// </summary>
        /// <param name="value"><see cref="UnityEditor.Editor.target"/></param>
        /// <returns></returns>
#endif
        public virtual bool Check([In][Opt] object value)
        {
            if (TypenAttribute == typeof(OnlyNotNullModeAttribute))
            {
                return pr(value);
            }
            else
            {
                if (pr == null)
                    //throw new SALCheckException(this, "you should not check at this");
                    return true;
                if (this.pr(value))
                    return true;
                return false;
            }
        }

        [System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
        public abstract class WhenMemberValueAttribute : WhenAttribute
        {
            public readonly string Name;
            public readonly object Value;
            protected object InjectGetValue(object target)
            {
                return ConventionUtility.SeekValue(target, Name, BindingFlags.NonPublic | BindingFlags.Public |
                   BindingFlags.Instance | BindingFlags.Static);
            }
            public override bool Check(object target)
            {
                throw new NotImplementedException();
            }
            public WhenMemberValueAttribute(string Name, object value)
            {
                this.Name = Name;
                this.Value = value;
            }
        }
        [System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
        public class IsAttribute : WhenMemberValueAttribute
        {
            public override bool Check(object target)
            {
                if (this.Value is Type)
                {
                    var targetValue = this.InjectGetValue(target);
                    if (targetValue == null)
                        return false;
                    else
                        return targetValue.GetType().IsSubclassOf(this.Value as Type);
                }
                if (this.Value != null)
                    return this.Value.Equals(this.InjectGetValue(target));
                var injectValue = this.InjectGetValue(target);
                if (injectValue != null)
                    return injectValue.Equals(this.Value);
                return injectValue == null && this.Value == null;
            }
            public IsAttribute(string Name, object value) : base(Name, value) { }
        }
        [System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
        public class NotAttribute : IsAttribute
        {
            public override bool Check(object target)
            {
                return !base.Check(target);
            }
            public NotAttribute(string Name, object value) : base(Name, value) { }
        }
    }
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class IgnoreAttribute : Attribute { }
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class ProjectContextLabelAttribute : Attribute
    {
        public enum ContextLabelType
        {
            Content, Resources, Setting
        }
        public static void DebugError(string mainName, string message, ContextLabelType type, UnityEngine.Object obj)
        {
            string labelname = type switch
            {
                ContextLabelType.Setting => "setting",
                ContextLabelType.Resources => "resources",
                ContextLabelType.Content => "content",
                _ => "context-label"
            };
            Debug.LogError($"{mainName} - {message} due to missing {labelname}.", obj);
        }
    }
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class SettingAttribute : ProjectContextLabelAttribute { }
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class ResourcesAttribute : ProjectContextLabelAttribute { }
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class ContentAttribute : ProjectContextLabelAttribute { }
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class OnlyPlayModeAttribute : Attribute { }
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class OnlyNotNullModeAttribute : Attribute
    {
        public string Name;
        public bool Check(object target)
        {
            if (IsSelf())
            {
#if UNITY_2017_1_OR_NEWER
                if (target is UnityEngine.Object && (target as UnityEngine.Object) == null)
                    return false;
#endif
                return target != null;
            }
            var field = target.GetType().GetField(Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                object value = field.GetValue(target);
                if (value == null)
                    return false;
#if UNITY_2017_1_OR_NEWER
                if (value is UnityEngine.Object && (value as UnityEngine.Object) == null)
                    return false;
#endif
                return true;
            }
            var property = target.GetType().GetProperty(Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
            {
                object value = property.GetValue(target);
                if (value == null)
                    return false;
#if UNITY_2017_1_OR_NEWER
                if (value is UnityEngine.Object && (value as UnityEngine.Object) == null)
                    return false;
#endif
                return true;
            }
            return false;
        }
        public bool IsSelf() => Name == null || Name.Length == 0;
        /// <summary>
        /// binding to target field
        /// </summary>
        /// <param name="fieldName"></param>
        public OnlyNotNullModeAttribute(string fieldName) { this.Name = fieldName; }
        /// <summary>
        /// binding to self
        /// </summary>
        public OnlyNotNullModeAttribute() { this.Name = null; }
    }
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class HopeNotNullAttribute : Attribute
    {
        public bool Check(object target)
        {
            return target != null;
        }
        public HopeNotNullAttribute() { }
    }
    [System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
        AttributeTargets.Parameter | AttributeTargets.ReturnValue,
        Inherited = false, AllowMultiple = false)]
    public class PercentageAttribute : Attribute
    {
        public float min = 0, max = 100;
        public PercentageAttribute([In] float min, [In] float max)
        {
            this.min = min;
            this.max = max;
        }
    }
    [System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
        AttributeTargets.Class | AttributeTargets.Class |
        AttributeTargets.Parameter | AttributeTargets.ReturnValue |
        AttributeTargets.Interface | AttributeTargets.GenericParameter, Inherited = false, AllowMultiple = false)]
    public class ArgPackageAttribute : Attribute
    {
        public Type[] UsedFor;
        public ArgPackageAttribute([In][Opt] params Type[] usedFor)
        {
            UsedFor = usedFor;
        }
    }
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class TODOAttribute : Attribute
    {
        public bool Check(object any)
        {
            throw new InvalidOperationException("TODO");
            throw new NotImplementedException();
        }
    }
    [System.AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public class ImportantAttribute : Attribute
    {
        public string description;

        public ImportantAttribute(string description)
        {
            this.description = description;
        }
        public ImportantAttribute() { }
    }
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class DescriptionAttribute : Attribute
    {
        public string description;
        public DescriptionAttribute(string description)
        {
            this.description = description;
        }
    }
    [System.AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class TypeCheckAttribute : Attribute
    {
        public readonly Type[] typens;
        public readonly string description;

        public TypeCheckAttribute(string description, params Type[] typens)
        {
            this.typens = typens;
            this.description = description;
        }

        public TypeCheckAttribute(params Type[] typens) : this("Type Check Failed: value is not sub class of " +
            $"{string.Join(",", typens.ToList().ConvertAll(x => x.Name))}" +
            ", current is ${type}", typens)
        {

        }

        public bool Check(object target)
        {
            return target != null &&
                typens.Any(x => target.GetType().IsSubclassOf(x) || x.IsInterface && target.GetType().GetInterface(x.Name) != null);
        }
    }


    public static partial class ConventionUtility
    {
        public static event Action InitExtensionEnvCalls;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Convention/InitExtensionEnv", priority = 100000)]
#endif
        public static void InitExtensionEnv()
        {
            UnityEngine.Application.quitting += () => CoroutineStarter = null;

            InitExtensionEnvCalls();
            GlobalConfig.InitExtensionEnv();
            UnityExtension.InitExtensionEnv();
            ES3Plugin.InitExtensionEnv();
        }

        public static int MainThreadID { get; private set; }
        public static bool CurrentThreadIsMainThread()
        {
            return MainThreadID == Thread.CurrentThread.ManagedThreadId;
        }

        private static CoroutineMonoStarterUtil CoroutineStarter;

        private class CoroutineMonoStarterUtil : MonoBehaviour
        {
            private void Update()
            {
                MainThreadID = Thread.CurrentThread.ManagedThreadId;
            }

            private void OnDestroy()
            {
                CoroutineStarter = null;
            }
        }
        public static Coroutine StartCoroutine(IEnumerator coroutine)
        {
            if (CoroutineStarter == null)
            {
                CoroutineStarter = new GameObject($"{nameof(ConventionUtility)}-{nameof(CoroutineStarter)}").AddComponent<CoroutineMonoStarterUtil>();
            }
            return CoroutineStarter.StartCoroutine(coroutine);
        }
        public static void CloseCoroutine(Coroutine coroutine)
        {
            CoroutineStarter.StopCoroutine(coroutine);
        }
        public static void StopAllCoroutine()
        {
            CoroutineStarter.StopAllCoroutines();
        }

        public class ActionStepCoroutineWrapper
        {
            private List<KeyValuePair<YieldInstruction, Action>> steps = new();
            public ActionStepCoroutineWrapper Update(Action action)
            {
                steps.Add(new(null, action));
                return this;
            }
            public ActionStepCoroutineWrapper Wait(float time, Action action)
            {
                steps.Add(new(new WaitForSeconds(time), action));
                return this;
            }
            public ActionStepCoroutineWrapper FixedUpdate(Action action)
            {
                steps.Add(new(new WaitForFixedUpdate(), action));
                return this;
            }
            public ActionStepCoroutineWrapper Next(Action action)
            {
                steps.Add(new(new WaitForEndOfFrame(), action));
                return this;
            }
            private static IEnumerator Execute(List<KeyValuePair<YieldInstruction, Action>> steps)
            {
                foreach (var (waiting, action) in steps)
                {
                    action();
                    yield return waiting;
                }
            }
            ~ActionStepCoroutineWrapper()
            {
                this.Invoke();
            }
            public void Invoke()
            {
                StartCoroutine(Execute(new List<KeyValuePair<YieldInstruction, Action>>(steps)));
                steps.Clear();
            }
        }

        [return: ReturnNotSelf]
        public static ActionStepCoroutineWrapper CreateSteps() => new();

        public static bool IsNumber([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsNumber(type);
        }
        public static bool IsString([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsString(type);
        }
        public static bool IsBinary([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsBinary(type);
        }
        public static bool IsArray([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsArray(type);
        }
        public static bool IsBool([In] object data)
        {
            if (data == null) return false;
            return IsBool(data.GetType());
        }

        public static bool IsNumber([In] Type type)
        {
            return
                type == typeof(double) ||
                type == typeof(float) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(uint) ||
                type == typeof(ulong) ||
                type == typeof(char);
        }
        public static bool IsString([In] Type type)
        {
            return type == typeof(string) || type == typeof(char[]);
        }
        public static bool IsBinary([In] Type type)
        {
            return
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(byte[]) ||
                type == typeof(sbyte[]);
        }
        public static bool IsArray([In] Type type)
        {
            return type.IsArray;
        }
        public static bool IsBool([In] Type type)
        {
            return type == typeof(bool);
        }
        public static bool IsEnum([In] Type type)
        {
            return type.IsEnum;
        }
        public static bool IsImage([In] Type type)
        {
            return type.IsSubclassOf(typeof(Texture)) || type == typeof(Sprite);
        }

        public static bool HasCustomAttribute([In] MemberInfo member, [In] IEnumerable<Type> attrs)
        {
            foreach (var attr in attrs)
            {
                if (member.GetCustomAttribute(attr, true) != null)
                    return true;
            }
            return false;
        }
        [return: ReturnMayNull]
        public static Type GetMemberValueType([In] MemberInfo member)
        {
            if (member is FieldInfo field)
            {
                return field.FieldType;
            }
            else if (member is PropertyInfo property)
            {
                return property.PropertyType;
            }
            return null;
        }
        public static bool GetMemberValueType([In] MemberInfo member, [Out] out Type type)
        {
            if (member is FieldInfo field)
            {
                type = field.FieldType;
                return true;
            }
            else if (member is PropertyInfo property)
            {
                type = property.PropertyType;
                return true;
            }
            type = null;
            return false;
        }
        public static void PushValue([In] object target, [In][Opt, When("If you sure")] object value, [In] MemberInfo info)
        {
            if (info is FieldInfo field)
            {
                if (value.GetType().IsSubclassOf(field.FieldType))
                    field.SetValue(target, value);
                else
                {
                    field.SetValue(target, field.FieldType.GetMethod(nameof(float.Parse)).Invoke(target, new object[] { value }));
                }
            }
            else if (info is PropertyInfo property)
            {
                property.SetValue(target, value);
            }
            else
            {
                throw new InvalidOperationException("info is unsupport");
            }
        }
        public static object SeekValue([In] object target, [In] MemberInfo info)
        {
            if (info is FieldInfo field)
            {
                return field.GetValue(target);
            }
            else if (info is PropertyInfo property)
            {
                return property.GetValue(target);
            }
            else
            {
                throw new InvalidOperationException("info is unsupport");
            }
        }
        public static bool TrySeekValue([In] object target, [In] MemberInfo info, [Out] out object value)
        {
            if (info is FieldInfo field)
            {
                value = field.GetValue(target);
                return true;
            }
            else if (info is PropertyInfo property)
            {
                value = property.GetValue(target);
                return true;
            }
            value = null;
            return false;
        }

        public static List<MemberInfo> SeekMemberInfo(
            [In] object target,
            [In, Opt] IEnumerable<Type> attrs, [In, Opt] IEnumerable<Type> types,
            [In, Opt] Type untilBase = null
            )
        {
            Type _CurType = target.GetType();
            List<MemberInfo> result = new();
            result.AddRange(_CurType.GetMembers(BindingFlags.Public | BindingFlags.Instance));
            while (_CurType != null && _CurType != typeof(object) && _CurType != untilBase)
            {
                result.AddRange(
                    from info in _CurType.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                    where attrs == null || HasCustomAttribute(info, attrs)
                    where types == null || (GetMemberValueType(info, out var type) && types.Contains(type))
                    select info
                    );
                _CurType = _CurType.BaseType;
            }
            return result;
        }
        public static List<MemberInfo> SeekMemberInfo([In] object target, IEnumerable<string> names, BindingFlags flags = BindingFlags.Default)
        {
            Type _CurType = target.GetType();
            List<MemberInfo> result = _CurType.GetMembers(flags).ToList();
            HashSet<string> nameSet = names.ToHashSet();
            result.RemoveAll(x => nameSet.Contains(x.Name) == false);
            return result;
        }
        public static object InvokeMember([In] MemberInfo member, [In] object target, params object[] parameters)
        {
            if (member is MethodInfo method)
            {
                return method.Invoke(target, parameters);
            }
            return null;
        }
        public static bool TryInvokeMember([In] MemberInfo member, object target, out object returnValue, params object[] parameters)
        {
            returnValue = null;
            if (member is MethodInfo method)
            {
                returnValue = method.Invoke(target, parameters);
                return true;
            }
            else return false;
        }
    }

#if UNITY_2017_1_OR_NEWER
    namespace Internal
    {
        public interface IRectTransform
        {
            UnityEngine.RectTransform rectTransform { get; }
        }
    }
#endif

    public static partial class ConventionUtility
    {
        public static UnityEvent WrapperAction2Event(params UnityAction[] actions)
        {
            var result = new UnityEvent();
            foreach (var action in actions)
            {
                result.AddListener(action);
            }
            return result;
        }
        public static UnityEvent<T> WrapperAction2Event<T>(params UnityAction<T>[] actions)
        {
            var result = new UnityEvent<T>();
            foreach (var action in actions)
            {
                result.AddListener(action);
            }
            return result;
        }
        public static UnityEvent<T, Y> WrapperAction2Event<T, Y>(params UnityAction<T, Y>[] actions)
        {
            var result = new UnityEvent<T, Y>();
            foreach (var action in actions)
            {
                result.AddListener(action);
            }
            return result;
        }
        public static UnityEvent<T, Y, U> WrapperAction2Event<T, Y, U>(params UnityAction<T, Y, U>[] actions)
        {
            var result = new UnityEvent<T, Y, U>();
            foreach (var action in actions)
            {
                result.AddListener(action);
            }
            return result;
        }
        public static UnityEvent<T, Y, U, I> WrapperAction2Event<T, Y, U, I>(params UnityAction<T, Y, U, I>[] actions)
        {
            var result = new UnityEvent<T, Y, U, I>();
            foreach (var action in actions)
            {
                result.AddListener(action);
            }
            return result;
        }
    }

    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {

        [Setting, Ignore] private static T m_instance;
        public static T instance { get => m_instance; protected set => m_instance = value; }
        public virtual bool IsDontDestroyOnLoad { get => false; }

        protected virtual void Awake()
        {
            if (instance != null)
            {
                this.gameObject.SetActive(false);
                return;
            }
            if (IsDontDestroyOnLoad && this.transform.parent == null)
                DontDestroyOnLoad(this);
            instance = (T)this;
        }

        public static bool IsAvailable()
        {
            return instance != null;
        }
    }
}

namespace Convention
{
    public static class UnityExtension
    {
        public static void InitExtensionEnv()
        {
            AsyncOperationExtension.InitExtensionEnv();
            RectTransformExtension.InitExtensionEnv();
        }
    }

    public static class AsyncOperationExtension
    {
        public static void InitExtensionEnv()
        {
            CompletedHelper.InitExtensionEnv();
        }
        public static void MarkCompleted(this AsyncOperation operation, [In] Action action)
        {
            operation.completed += new CompletedHelper(action).InternalCompleted;
        }

        private class CompletedHelper
        {
            static CompletedHelper() => helpers = new();
            public static void InitExtensionEnv() => helpers.Clear();
            private static readonly List<CompletedHelper> helpers = new();

            readonly Action action;

            public CompletedHelper([In] Action action)
            {
                helpers.Add(this);
                this.action = action;
            }
            ~CompletedHelper()
            {
                helpers.Remove(this);
            }

            public void InternalCompleted(AsyncOperation obj)
            {
                if (obj.progress < 0.99f) return;
                action.Invoke();
                helpers.Remove(this);
            }

        }
    }

    public static partial class RectTransformExtension
    {
        private static bool IsDisableAdjustSizeToContainsChilds2DeferUpdates = false;


        public static void InitExtensionEnv()
        {
            IsDisableAdjustSizeToContainsChilds2DeferUpdates = false;
        }

        public class AdjustSizeIgnore : MonoBehaviour { }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Convention/DisableAdjustSize", priority = 100000)]
#endif
        public static void DisableAdjustSizeToContainsChilds2DeferUpdates()
        {
            IsDisableAdjustSizeToContainsChilds2DeferUpdates = true;
        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Convention/EnableAdjustSize", priority = 100000)]
#endif
        public static void AppleAndEnableAdjustSizeToContainsChilds()
        {
            IsDisableAdjustSizeToContainsChilds2DeferUpdates = false;
        }
        public static void AdjustSizeToContainsChilds([In] RectTransform rectTransform, Vector2 min, Vector2 max, RectTransform.Axis? axis)
        {
            if (IsDisableAdjustSizeToContainsChilds2DeferUpdates)
                return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            bool stats = false;

            List<RectTransform> currentList = new(), nextList = new();
            var corners = new Vector3[4];
            foreach (RectTransform item in rectTransform)
            {
                currentList.Add(item);
            }
            do
            {
                currentList.AddRange(nextList);
                nextList.Clear();
                foreach (RectTransform childTransform in currentList)
                {
                    if (childTransform.gameObject.activeInHierarchy == false)
                        continue;
                    if (childTransform.name.ToLower().Contains("<ignore rect>"))
                        continue;
                    if (childTransform.name.ToLower().Contains($"<ignore {nameof(AdjustSizeToContainsChilds)}>"))
                        continue;
                    if (childTransform.GetComponents<AdjustSizeIgnore>().Length != 0)
                        continue;
                    stats = true;
                    foreach (RectTransform item in childTransform)
                    {
                        nextList.Add(item);
                    }
                    childTransform.GetWorldCorners(corners);
                    foreach (var corner in corners)
                    {
                        Vector2 localCorner = rectTransform.InverseTransformPoint(corner);
                        if (float.IsNaN(localCorner.x) || float.IsNaN(localCorner.y))
                            break;
                        min.x = Mathf.Min(min.x, localCorner.x);
                        min.y = Mathf.Min(min.y, localCorner.y);
                        max.x = Mathf.Max(max.x, localCorner.x);
                        max.y = Mathf.Max(max.y, localCorner.y);
                    }
                }
                currentList.Clear();
            } while (nextList.Count > 0);
            if (stats)
            {
                if ((axis.HasValue && axis.Value == RectTransform.Axis.Vertical) ||
                    (rectTransform.anchorMin.x == 0 && rectTransform.anchorMax.x == 1 && rectTransform.anchorMin.y == rectTransform.anchorMax.y))
                {
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, max.y - min.y);
                }
                else if ((axis.HasValue && axis.Value == RectTransform.Axis.Horizontal) ||
                    (rectTransform.anchorMin.y == 0 && rectTransform.anchorMax.y == 1 && rectTransform.anchorMin.x == rectTransform.anchorMax.x))
                {
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, max.x - min.x);
                }
                else
                {
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, max.x - min.x);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, max.y - min.y);
                }
            }
        }
        public static void AdjustSizeToContainsChilds([In] RectTransform rectTransform, RectTransform.Axis axis)
        {
            if (IsDisableAdjustSizeToContainsChilds2DeferUpdates)
                return;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            AdjustSizeToContainsChilds(rectTransform, min, max, axis);
        }
        public static void AdjustSizeToContainsChilds([In] RectTransform rectTransform)
        {
            if (IsDisableAdjustSizeToContainsChilds2DeferUpdates)
                return;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            AdjustSizeToContainsChilds(rectTransform, min, max, null);
        }

        internal static void SetParentAndResizeWithoutNotifyBaseWindowPlane([In] RectTransform parent, [In] RectTransform child, Rect rect, bool isAdjustSizeToContainsChilds)
        {
            child.SetParent(parent, false);
            child.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rect.x, rect.width);
            child.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, rect.y, rect.height);
            if (isAdjustSizeToContainsChilds)
                AdjustSizeToContainsChilds(parent);
        }
        internal static void SetParentAndResizeWithoutNotifyBaseWindowPlane(RectTransform parent, RectTransform child, bool isAdjustSizeToContainsChilds)
        {
            child.SetParent(parent, false);
            if (isAdjustSizeToContainsChilds)
                AdjustSizeToContainsChilds(parent);
        }
        public static void SetParentAndResize(RectTransform parent, RectTransform child, Rect rect, bool isAdjustSizeToContainsChilds)
        {
            if (parent.GetComponents<BaseWindowPlane>().Length != 0)
            {
                parent.GetComponents<BaseWindowPlane>()[0].AddChild(child, rect, isAdjustSizeToContainsChilds);
            }
            else
            {
                SetParentAndResizeWithoutNotifyBaseWindowPlane(parent, child, rect, isAdjustSizeToContainsChilds);
            }
        }
        public static void SetParentAndResize(RectTransform parent, RectTransform child, bool isAdjustSizeToContainsChilds)
        {
            if (parent.GetComponents<BaseWindowPlane>().Length != 0)
            {
                parent.GetComponents<BaseWindowPlane>()[0].AddChild(child, isAdjustSizeToContainsChilds);
            }
            else
            {
                SetParentAndResizeWithoutNotifyBaseWindowPlane(parent, child, isAdjustSizeToContainsChilds);
            }
        }

        public static bool IsVisible([In] RectTransform rectTransform, [In, Opt] Camera camera = null)
        {
            if (camera == null)
                camera = Camera.main;
            Transform camTransform = camera.transform;
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            foreach (var worldPos in corners)
            {
                Vector2 viewPos = camera.WorldToViewportPoint(worldPos);
                Vector3 dir = (worldPos - camTransform.position).normalized;
                float dot = Vector3.Dot(camTransform.forward, dir);

                if (dot <= 0 || viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
                    return false;
            }
            return true;
        }
    }

    public static partial class SkyExtension
    {
        public static Material GetSky()
        {
            return RenderSettings.skybox;
        }

        public static void Load([In][Opt, When("If you sure")] Material skybox)
        {
            RenderSettings.skybox = skybox;
        }

        public static void Rotation(float angle)
        {
            RenderSettings.skybox.SetFloat("_Rotation", angle);
        }
    }

    public static partial class SceneExtension
    {
        public static void Load(string name)
        {
            SceneManager.LoadScene(name, LoadSceneMode.Additive);
        }
        public static void Load(string name, out AsyncOperation async)
        {
            async = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        }
        public static void Unload(string name)
        {
            SceneManager.UnloadSceneAsync(name);
        }
        public static Scene GetScene(string name)
        {
            return SceneManager.GetSceneByName(name);
        }
    }

    public static class GameObjectExtension
    {
        /// <summary>
        /// 递归设置GameObject及其所有子物体的Layer
        /// </summary>
        public static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform t in gameObject.transform)
            {
                SetLayerRecursively(t.gameObject, layer);
            }
        }

        /// <summary>
        /// 递归设置GameObject及其所有子物体的Tag
        /// </summary>
        public static void SetTagRecursively(GameObject gameObject, string tag)
        {
            gameObject.tag = tag;
            foreach (Transform t in gameObject.transform)
            {
                SetTagRecursively(t.gameObject, tag);
            }
        }

        /// <summary>
        /// 递归启用/禁用所有Collider组件
        /// </summary>
        public static void SetCollisionRecursively(GameObject gameObject, bool enabled)
        {
            var colliders = gameObject.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                collider.enabled = enabled;
            }
        }

        /// <summary>
        /// 递归启用/禁用所有Renderer组件
        /// </summary>
        public static void SetVisualRecursively(GameObject gameObject, bool enabled)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = enabled;
            }
        }

        /// <summary>
        /// 获取指定Tag的所有子组件
        /// </summary>
        public static T[] GetComponentsInChildrenWithTag<T>(GameObject gameObject, string tag) where T : Component
        {
            List<T> results = new List<T>();

            if (gameObject.CompareTag(tag))
            {
                var component = gameObject.GetComponent<T>();
                if (component != null)
                    results.Add(component);
            }

            foreach (Transform t in gameObject.transform)
            {
                results.AddRange(GetComponentsInChildrenWithTag<T>(t.gameObject, tag));
            }

            return results.ToArray();
        }

        /// <summary>
        /// 在父物体中查找组件
        /// </summary>
        public static T GetComponentInParents<T>(GameObject gameObject) where T : Component
        {
            for (Transform t = gameObject.transform; t != null; t = t.parent)
            {
                T result = t.GetComponent<T>();
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// 获取或添加组件
        /// </summary>
        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component ?? gameObject.AddComponent<T>();
        }
    }

    public static class TransformExtension
    {
        /// <summary>
        /// 获取所有子物体
        /// </summary>
        public static List<Transform> GetAllChildren(this Transform transform)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform)
            {
                children.Add(child);
                children.AddRange(child.GetAllChildren());
            }
            return children;
        }

        /// <summary>
        /// 销毁所有子物体
        /// </summary>
        public static void DestroyAllChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 设置父物体并保持世界坐标
        /// </summary>
        public static void SetParentKeepWorldPosition(this Transform transform, Transform parent)
        {
            Vector3 worldPos = transform.position;
            Quaternion worldRot = transform.rotation;
            transform.SetParent(parent);
            transform.position = worldPos;
            transform.rotation = worldRot;
        }
    }

    public static class CoroutineExtension
    {
        /// <summary>
        /// 延迟执行
        /// </summary>
        public static IEnumerator Delay(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        /// <summary>
        /// 延迟执行并返回结果
        /// </summary>
        public static IEnumerator Delay<T>(float delay, Func<T> action, Action<T> callback)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke(action());
        }

        /// <summary>
        /// 等待直到条件满足
        /// </summary>
        public static IEnumerator WaitUntil(Func<bool> condition, Action onComplete = null)
        {
            yield return new WaitUntil(condition);
            onComplete?.Invoke();
        }

        /// <summary>
        /// 等待直到条件满足，带超时
        /// </summary>
        public static IEnumerator WaitUntil(Func<bool> condition, float timeout, Action onComplete = null, Action onTimeout = null)
        {
            float elapsedTime = 0;
            while (!condition() && elapsedTime < timeout)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (elapsedTime >= timeout)
            {
                onTimeout?.Invoke();
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 执行动画曲线
        /// </summary>
        public static IEnumerator Animate(float duration, AnimationCurve curve, Action<float> onUpdate)
        {
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / duration;
                float evaluatedValue = curve.Evaluate(normalizedTime);
                onUpdate?.Invoke(evaluatedValue);
                yield return null;
            }
            onUpdate?.Invoke(curve.Evaluate(1));
        }

        /// <summary>
        /// 执行线性插值
        /// </summary>
        public static IEnumerator Lerp<T>(T start, T end, float duration, Action<T> onUpdate, Func<T, T, float, T> lerpFunction)
        {
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / duration;
                T current = lerpFunction(start, end, normalizedTime);
                onUpdate?.Invoke(current);
                yield return null;
            }
            onUpdate?.Invoke(end);
        }

        /// <summary>
        /// 执行Vector3插值
        /// </summary>
        public static IEnumerator LerpVector3(Vector3 start, Vector3 end, float duration, Action<Vector3> onUpdate)
        {
            yield return Lerp(start, end, duration, onUpdate, Vector3.Lerp);
        }

        /// <summary>
        /// 执行Quaternion插值
        /// </summary>
        public static IEnumerator LerpQuaternion(Quaternion start, Quaternion end, float duration, Action<Quaternion> onUpdate)
        {
            yield return Lerp(start, end, duration, onUpdate, Quaternion.Lerp);
        }

        /// <summary>
        /// 执行float插值
        /// </summary>
        public static IEnumerator LerpFloat(float start, float end, float duration, Action<float> onUpdate)
        {
            yield return Lerp(start, end, duration, onUpdate, Mathf.Lerp);
        }
    }

    public static class ScriptingDefineUtility
    {
#if UNITY_EDITOR
        public static void Add(string define, BuildTargetGroup target, bool log = false)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            if (definesString.Contains(define)) return;
            string[] allDefines = definesString.Split(';');
            ArrayUtility.Add(ref allDefines, define);
            definesString = string.Join(";", allDefines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, definesString);
            Debug.Log("Added \"" + define + "\" from " + EditorUserBuildSettings.selectedBuildTargetGroup + " Scripting define in Player Settings");
        }

        public static void Remove(string define, BuildTargetGroup target, bool log = false)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            if (!definesString.Contains(define)) return;
            string[] allDefines = definesString.Split(';');
            ArrayUtility.Remove(ref allDefines, define);
            definesString = string.Join(";", allDefines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, definesString);
            Debug.Log("Removed \"" + define + "\" from " + EditorUserBuildSettings.selectedBuildTargetGroup + " Scripting define in Player Settings");
        }
#endif
    }
}

namespace Convention
{
    public static class StringExtension
    {
        public static void InitExtensionEnv()
        {
            CurrentStringTransformer = null;
            MyLazyTransformer.Clear();
        }

        public static string LimitString([In] object data, int maxLength = 50)
        {
            return LimitString(data.ToString(), maxLength);
        }
        public static string LimitString([In] in string data, int maxLength = 50)
        {
            if (data.Length <= maxLength)
                return data;
            var insideStr = "\n...\n...\n";
            int headLength = maxLength / 2;
            int tailLength = maxLength - headLength - insideStr.Length;
            return data[..headLength] + insideStr + data[^tailLength..];
        }

        public enum Side
        {
            Left,
            Right,
            Center
        }
        public static string FillString([In] object data, int maxLength = 50, char fillChar = ' ', Side side = Side.Right)
        {
            return FillString(data.ToString(), maxLength, fillChar, side);
        }
        public static string FillString([In] in string data, int maxLength = 50, char fillChar = ' ', Side side = Side.Right)
        {
            if (data.Length >= maxLength)
                return data;
            var fillStr = new string(fillChar, maxLength - data.Length);
            switch (side)
            {
                case Side.Left:
                    return fillStr + data;
                case Side.Right:
                    return data + fillStr;
                case Side.Center:
                    int leftLength = (maxLength - data.Length) / 2;
                    int rightLength = maxLength - leftLength - data.Length;
                    return new string(fillChar, leftLength) + data + new string(fillChar, rightLength);
                default:
                    return data;
            }
        }

        public static List<string> BytesToStrings([In] IEnumerable<byte[]> bytes)
        {
            return BytesToStrings(bytes, Encoding.UTF8);
        }
        public static List<string> BytesToStrings([In] IEnumerable<byte[]> bytes, Encoding encoding)
        {
            return bytes.ToList().ConvertAll(x => encoding.GetString(x));
        }

        private static Dictionary<string, string> MyLazyTransformer = new();

        public class StringTransformer
        {
            [Serializable, ArgPackage]
            public class StringContentTree
            {
                public string leaf = null;
                public Dictionary<string, StringContentTree> branch = null;
            }

            private StringContentTree contents;

            public StringTransformer([In] string transformerFile)
            {
                var file = new ToolFile(transformerFile);
                contents = file.LoadAsRawJson<StringContentTree>();
            }

            public string Transform([In] string stringName)
            {
                if (contents == null || contents.branch == null)
                    return stringName;
                var keys = stringName.Split('.');
                StringContentTree current = contents;
                foreach (var k in keys)
                {
                    if (current.branch != null && current.branch.TryGetValue(k, out var next))
                    {
                        current = next;
                    }
                    else
                    {
                        return stringName; // If any key is not found, return the original key
                    }
                }
                return current.leaf ?? stringName; // Return leaf or original key if leaf is null
            }
        }

        private static StringTransformer MyCurrentStringTransformer = null;
        public static StringTransformer CurrentStringTransformer
        {
            get => MyCurrentStringTransformer;
            set
            {
                if (MyCurrentStringTransformer != value)
                {
                    MyLazyTransformer.Clear();
                    MyCurrentStringTransformer = value;
                }
            }
        }

        public static string Transform([In] string stringName)
        {
            if (MyLazyTransformer.TryGetValue(stringName, out var result))
                return result;
            return MyLazyTransformer[stringName] = CurrentStringTransformer != null
                ? CurrentStringTransformer.Transform(stringName)
                : stringName;
        }
    }
}

namespace Convention
{
    [ArgPackage]
    public class ValueWrapper
    {
        private Func<object> getter;
        private Action<object> setter;
        public readonly Type type;

        public ValueWrapper([In, Opt] Func<object> getter, [In, Opt] Action<object> setter, [In] Type type)
        {
            this.getter = getter;
            this.setter = setter;
            this.type = type;
        }

        public bool IsChangeAble => setter != null;
        public bool IsObtainAble => getter != null;

        public void SetValue(object value)
        {
            setter(value);
        }
        public object GetValue()
        {
            return getter();
        }
    }
}

namespace Convention
{
    public static class TextureExtenion
    {
        public static Texture2D CropTexture(this Texture texture, Rect source)
        {
            RenderTexture active = RenderTexture.active;
            RenderTexture renderTexture = (RenderTexture.active = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 8));
            bool sRGBWrite = GL.sRGBWrite;
            GL.sRGBWrite = false;
            GL.Clear(clearDepth: false, clearColor: true, new Color(1f, 1f, 1f, 0f));
            Graphics.Blit(texture, renderTexture);
            Texture2D texture2D = new Texture2D((int)source.width, (int)source.height, TextureFormat.ARGB32, mipChain: true, linear: false);
            texture2D.filterMode = FilterMode.Point;
            texture2D.ReadPixels(source, 0, 0);
            texture2D.Apply();
            GL.sRGBWrite = sRGBWrite;
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(renderTexture);
            return texture2D;
        }
        public static Texture2D CopyTexture(this Texture texture)
        {
            return CropTexture(texture, new(0, 0, texture.width, texture.height));
        }
        public static Sprite ToSprite(this Texture2D texture)
        {
            return Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f));
        }
    }
}

namespace Convention
{
    public static partial class ConventionUtility
    {
        public static object GetDefault([In] Type type)
        {
            if (type.IsClass)
                return null;
            else
                return Activator.CreateInstance(type);
        }
    }
}

