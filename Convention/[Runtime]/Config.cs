using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Convention.WindowsUI;
using Unity.Collections;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Build;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
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

        public static string CurrentWorkPath => Environment.CurrentDirectory;

        public static string Combine(string a, string b) => Path.Combine(a, b);
        public static string CombineAsDir(string a, string b)
        {
            string name = b.Replace("\\", "/");
            if (name.EndsWith("/") == false)
            {
                name += "/";
            }
            return Path.Combine(a, name);
        }
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


        /// <summary>
        /// Marshal 相关的实用函数。
        /// </summary>
        public static class Marshal
        {

            [Serializable]
            public class MarshalException : Exception
            {
                public MarshalException() { }
                public MarshalException(string message) : base(message) { }
                public MarshalException(string message, Exception inner) : base(message, inner) { }
                protected MarshalException(
                  System.Runtime.Serialization.SerializationInfo info,
                  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
            }

            private const int BlockSize = 1024 * 4;
            private static IntPtr s_CachedHGlobalPtr = IntPtr.Zero;
            private static int s_CachedHGlobalSize = 0;

            /// <summary>
            /// 获取缓存的从进程的非托管内存中分配的内存的大小。
            /// </summary>
            public static int CachedHGlobalSize
            {
                get
                {
                    return s_CachedHGlobalSize;
                }
            }

            /// <summary>
            /// 确保从进程的非托管内存中分配足够大小的内存并缓存。
            /// </summary>
            /// <param name="ensureSize">要确保从进程的非托管内存中分配内存的大小。</param>
            public static void EnsureCachedHGlobalSize(int ensureSize)
            {
                if (ensureSize < 0)
                {
                    throw new MarshalException("Ensure size is invalid.");
                }

                if (s_CachedHGlobalPtr == IntPtr.Zero || s_CachedHGlobalSize < ensureSize)
                {
                    FreeCachedHGlobal();
                    int size = (ensureSize - 1 + BlockSize) / BlockSize * BlockSize;
                    s_CachedHGlobalPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
                    s_CachedHGlobalSize = size;
                }
            }

            /// <summary>
            /// 释放缓存的从进程的非托管内存中分配的内存。
            /// </summary>
            public static void FreeCachedHGlobal()
            {
                if (s_CachedHGlobalPtr != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(s_CachedHGlobalPtr);
                    s_CachedHGlobalPtr = IntPtr.Zero;
                    s_CachedHGlobalSize = 0;
                }
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <returns>存储转换结果的二进制流。</returns>
            public static byte[] StructureToBytes<T>(T structure)
            {
                return StructureToBytes(structure, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <param name="structureSize">要转换的对象的大小。</param>
            /// <returns>存储转换结果的二进制流。</returns>
            internal static byte[] StructureToBytes<T>(T structure, int structureSize)
            {
                if (structureSize < 0)
                {
                    throw new MarshalException("Structure size is invalid.");
                }

                EnsureCachedHGlobalSize(structureSize);
                System.Runtime.InteropServices.Marshal.StructureToPtr(structure, s_CachedHGlobalPtr, true);
                byte[] result = new byte[structureSize];
                System.Runtime.InteropServices.Marshal.Copy(s_CachedHGlobalPtr, result, 0, structureSize);
                return result;
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <param name="result">存储转换结果的二进制流。</param>
            public static void StructureToBytes<T>(T structure, byte[] result)
            {
                StructureToBytes(structure, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), result, 0);
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <param name="structureSize">要转换的对象的大小。</param>
            /// <param name="result">存储转换结果的二进制流。</param>
            internal static void StructureToBytes<T>(T structure, int structureSize, byte[] result)
            {
                StructureToBytes(structure, structureSize, result, 0);
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <param name="result">存储转换结果的二进制流。</param>
            /// <param name="startIndex">写入存储转换结果的二进制流的起始位置。</param>
            public static void StructureToBytes<T>(T structure, byte[] result, int startIndex)
            {
                StructureToBytes(structure, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), result, startIndex);
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <param name="structureSize">要转换的对象的大小。</param>
            /// <param name="result">存储转换结果的二进制流。</param>
            /// <param name="startIndex">写入存储转换结果的二进制流的起始位置。</param>
            internal static void StructureToBytes<T>(T structure, int structureSize, byte[] result, int startIndex)
            {
                if (structureSize < 0)
                {
                    throw new MarshalException("Structure size is invalid.");
                }

                if (result == null)
                {
                    throw new MarshalException("Result is invalid.");
                }

                if (startIndex < 0)
                {
                    throw new MarshalException("Start index is invalid.");
                }

                if (startIndex + structureSize > result.Length)
                {
                    throw new MarshalException("Result length is not enough.");
                }

                EnsureCachedHGlobalSize(structureSize);
                System.Runtime.InteropServices.Marshal.StructureToPtr(structure, s_CachedHGlobalPtr, true);
                System.Runtime.InteropServices.Marshal.Copy(s_CachedHGlobalPtr, result, startIndex, structureSize);
            }

            /// <summary>
            /// 将数据从二进制流转换为对象。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="buffer">要转换的二进制流。</param>
            /// <returns>存储转换结果的对象。</returns>
            public static T BytesToStructure<T>(byte[] buffer)
            {
                return BytesToStructure<T>(System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer, 0);
            }

            /// <summary>
            /// 将数据从二进制流转换为对象。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="buffer">要转换的二进制流。</param>
            /// <param name="startIndex">读取要转换的二进制流的起始位置。</param>
            /// <returns>存储转换结果的对象。</returns>
            public static T BytesToStructure<T>(byte[] buffer, int startIndex)
            {
                return BytesToStructure<T>(System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer, startIndex);
            }

            /// <summary>
            /// 将数据从二进制流转换为对象。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structureSize">要转换的对象的大小。</param>
            /// <param name="buffer">要转换的二进制流。</param>
            /// <returns>存储转换结果的对象。</returns>
            internal static T BytesToStructure<T>(int structureSize, byte[] buffer)
            {
                return BytesToStructure<T>(structureSize, buffer, 0);
            }

            /// <summary>
            /// 将数据从二进制流转换为对象。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structureSize">要转换的对象的大小。</param>
            /// <param name="buffer">要转换的二进制流。</param>
            /// <param name="startIndex">读取要转换的二进制流的起始位置。</param>
            /// <returns>存储转换结果的对象。</returns>
            internal static T BytesToStructure<T>(int structureSize, byte[] buffer, int startIndex)
            {
                if (structureSize < 0)
                {
                    throw new MarshalException("Structure size is invalid.");
                }

                if (buffer == null)
                {
                    throw new MarshalException("Buffer is invalid.");
                }

                if (startIndex < 0)
                {
                    throw new MarshalException("Start index is invalid.");
                }

                if (startIndex + structureSize > buffer.Length)
                {
                    throw new MarshalException("Buffer length is not enough.");
                }

                EnsureCachedHGlobalSize(structureSize);
                System.Runtime.InteropServices.Marshal.Copy(buffer, startIndex, s_CachedHGlobalPtr, structureSize);
                return (T)System.Runtime.InteropServices.Marshal.PtrToStructure(s_CachedHGlobalPtr, typeof(T));
            }
        }

        public static bool HasAttribute(this MemberInfo self, Type attrType)
        {
            return (from item in self.GetCustomAttributes(true) where item.GetType().IsSubclassOf(attrType) select item).Count() != 0;
        }

        public static bool HasAttribute<T>(this MemberInfo self) where T : Attribute
        {
            return (from item in self.GetCustomAttributes(true) where item.GetType().IsSubclassOf(typeof(T)) select item).Count() != 0;
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
            var parse_method = type.GetMethod(nameof(int.Parse), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string) }, null);
            if (parse_method != null &&
                (parse_method.ReturnType.IsSubclassOf(type) || parse_method.ReturnType == type) &&
                parse_method.GetParameters().Length == 1 &&
                parse_method.GetParameters()[0].ParameterType == typeof(string))
            {
                return (_T)parse_method.Invoke(null, new object[] { str });
            }

            throw new InvalidCastException($"\"{str}\" is cannt convert to type<{type}>");
        }

        public static object ConventValue([In] Type type, [In] string str)
        {
            var parse_method = type.GetMethod("Parse");
            if (parse_method != null &&
                (parse_method.ReturnType.IsSubclassOf(type) || parse_method.ReturnType == type) &&
                parse_method.GetParameters().Length == 1 &&
                parse_method.GetParameters()[0].ParameterType == typeof(string))
            {
                return parse_method.Invoke(null, new object[] { str });
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
            UnityEngine.Application.quitting += () =>
            {
                GameObject.Destroy(s_CoroutineStarter.gameObject);
                s_CoroutineStarter = null;
            };

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

        private static CoroutineMonoStarterUtil s_CoroutineStarter;
        private static CoroutineMonoStarterUtil CoroutineStarter
        {
            get
            {
                if (s_CoroutineStarter == null)
                {
                    s_CoroutineStarter = new GameObject($"{nameof(ConventionUtility)}-{nameof(CoroutineStarter)}").AddComponent<CoroutineMonoStarterUtil>();
                }
                return s_CoroutineStarter;
            }
        }

        public static GameObject Singleton => CoroutineStarter.gameObject;

        private class CoroutineMonoStarterUtil : MonoBehaviour
        {
            internal float waitClock;
            private void Update()
            {
                waitClock = Time.realtimeSinceStartup;
                MainThreadID = Thread.CurrentThread.ManagedThreadId;
            }

            private void OnDestroy()
            {
                s_CoroutineStarter = null;
            }
        }
        /// <summary>
        /// 包装即将用于协程的迭代器成为一个防止假死机的迭代器
        /// </summary>
        /// <param name="ir"></param>
        /// <returns></returns>
        public static IEnumerator AvoidFakeStop(IEnumerator ir, float fps = 5)
        {
            Stack<IEnumerator> loadingTask = new();
            loadingTask.Push(ir);
            float maxWaitClock = 1 / fps;
            while (loadingTask.Count > 0)
            {
                // 防止大量无延迟函数的使用导致假死机
                if (Time.realtimeSinceStartup - CoroutineStarter.waitClock > maxWaitClock)
                {
                    yield return null;
                }
                if (loadingTask.Peek().MoveNext())
                {
                    var current = loadingTask.Peek().Current;
                    if (current is IEnumerator next)
                        loadingTask.Push(next);
                    else if (current is YieldInstruction yieldInstruction)
                        yield return yieldInstruction;
                    else if (current is CustomYieldInstruction customYieldInstruction)
                        yield return customYieldInstruction;
                }
                else
                {
                    loadingTask.Pop();
                }
            }
        }
        /// <summary>
        /// 包装即将用于协程的迭代器成为一个防止假死机的迭代器
        /// </summary>
        /// <param name="ir"></param>
        /// <returns></returns>
        public static IEnumerator AvoidFakeStopWithoutDeepStack(IEnumerator ir, float fps = 5)
        {
            float maxWaitClock = 1 / fps;
            while (ir.MoveNext())
            {
                // 防止大量无延迟函数的使用导致假死机
                if (Time.realtimeSinceStartup - CoroutineStarter.waitClock > maxWaitClock)
                {
                    yield return ir.Current;
                }
            }
        }
        public static Coroutine StartCoroutine(IEnumerator coroutine)
        {
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
            private struct YieldInstructionWrapper
            {
                public YieldInstruction UnityYieldInstruction;
                public CustomYieldInstruction CustomYieldInstruction;

                public YieldInstructionWrapper(YieldInstruction unityYieldInstruction, CustomYieldInstruction customYieldInstruction)
                {
                    this.UnityYieldInstruction = unityYieldInstruction;
                    this.CustomYieldInstruction = customYieldInstruction;
                }

                public static YieldInstructionWrapper Make(YieldInstruction unityYieldInstruction)
                {
                    return new(unityYieldInstruction, null);
                }

                public static YieldInstructionWrapper Make(CustomYieldInstruction customYieldInstruction)
                {
                    return new(null, customYieldInstruction);
                }
            }

            private List<KeyValuePair<YieldInstructionWrapper, Action>> steps = new();

            public ActionStepCoroutineWrapper Update(Action action)
            {
                steps.Add(new(new(), action));
                return this;
            }
            public ActionStepCoroutineWrapper Wait(float time, Action action)
            {
                steps.Add(new(YieldInstructionWrapper.Make(new WaitForSeconds(time)), action));
                return this;
            }
            public ActionStepCoroutineWrapper FixedUpdate(Action action)
            {
                steps.Add(new(YieldInstructionWrapper.Make(new WaitForFixedUpdate()), action));
                return this;
            }
            public ActionStepCoroutineWrapper Next(Action action)
            {
                steps.Add(new(YieldInstructionWrapper.Make(new WaitForEndOfFrame()), action));
                return this;
            }
            public ActionStepCoroutineWrapper Until(Func<bool> pr, Action action)
            {
                steps.Add(new(YieldInstructionWrapper.Make(new WaitUntil(pr)), action));
                return this;
            }
            private static IEnumerator Execute(List<KeyValuePair<YieldInstructionWrapper, Action>> steps)
            {
                foreach (var (waiting, action) in steps)
                {
                    if (waiting.UnityYieldInstruction != null)
                        yield return waiting.UnityYieldInstruction;
                    else
                        yield return waiting.CustomYieldInstruction;
                    action();
                }
            }
            ~ActionStepCoroutineWrapper()
            {
                this.Invoke();
            }
            public void Invoke()
            {
                StartCoroutine(Execute(new List<KeyValuePair<YieldInstructionWrapper, Action>>(steps)));
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

        public static List<MemberInfo> SeekMemberInfoFromType(
            [In] Type targetType,
            [In, Opt] IEnumerable<Type> attrs, [In, Opt] IEnumerable<Type> types,
            [In, Opt] Type untilBase = null
            )
        {
            List<MemberInfo> result = new();
            result.AddRange(targetType.GetMembers(BindingFlags.Public | BindingFlags.Instance));
            while (targetType != null && targetType != typeof(object) && targetType != untilBase)
            {
                result.AddRange(
                    from info in targetType.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                    where attrs == null || HasCustomAttribute(info, attrs)
                    where types == null || (GetMemberValueType(info, out var type) && types.Contains(type))
                    select info
                    );
                targetType = targetType.BaseType;
            }
            return result;
        }
        public static List<MemberInfo> SeekMemberInfo(
            [In] object target,
            [In, Opt] IEnumerable<Type> attrs, [In, Opt] IEnumerable<Type> types,
            [In, Opt] Type untilBase = null
            )
        {
            return SeekMemberInfoFromType(target.GetType(), attrs, types, untilBase);
        }
        public static List<MemberInfo> SeekMemberInfoFromType([In] Type target, IEnumerable<string> names,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            List<MemberInfo> result = target.GetMembers(flags).ToList();
            HashSet<string> nameSet = names.ToHashSet();
            result.RemoveAll(x => nameSet.Contains(x.Name) == false);
            return result;
        }
        public static List<MemberInfo> SeekMemberInfo([In] object target, IEnumerable<string> names, 
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            return SeekMemberInfoFromType(target.GetType(), names, flags);
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

#if UNITY_6000_0_OR_NEWER
        public static void Add(string define, NamedBuildTarget target, bool log = false)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbols(target);
            if (definesString.Contains(define)) return;
            string[] allDefines = definesString.Split(';');
            ArrayUtility.Add(ref allDefines, define);
            definesString = string.Join(";", allDefines);
            PlayerSettings.SetScriptingDefineSymbols(target, definesString);
            Debug.Log("Added \"" + define + "\" from " + EditorUserBuildSettings.selectedBuildTargetGroup + " Scripting define in Player Settings");
        }

        public static void Remove(string define, NamedBuildTarget target, bool log = false)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbols(target);
            if (!definesString.Contains(define)) return;
            string[] allDefines = definesString.Split(';');
            ArrayUtility.Remove(ref allDefines, define);
            definesString = string.Join(";", allDefines);
            PlayerSettings.SetScriptingDefineSymbols(target, definesString);
            Debug.Log("Removed \"" + define + "\" from " + EditorUserBuildSettings.selectedBuildTargetGroup + " Scripting define in Player Settings");
        }
        public static void Add(string define, BuildTargetGroup target, bool log = false)
        {
            Add(define, NamedBuildTarget.FromBuildTargetGroup(target), log);
        }

        public static void Remove(string define, BuildTargetGroup target, bool log = false)
        {
            Remove(define, NamedBuildTarget.FromBuildTargetGroup(target), log);
        }
#else
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

    public static partial class ConventionUtility
    {
        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }
    }
}

namespace Convention
{
    public static partial class Utility
    {
        /// <summary>
        /// 生成期望类型的栈实例
        /// </summary>
        /// <param name="type">期望类型</param>
        /// <returns></returns>
        public static object CreateStack([In] Type type)
        {
            var stackType = typeof(Stack<>).MakeGenericType(type);
            var stackInstance = Activator.CreateInstance(stackType);
            return stackInstance;
        }

        /// <summary>
        /// 生成期望类型的队列实例
        /// </summary>
        /// <param name="type">期望类型</param>
        /// <returns></returns>
        public static object CreateQueue([In] Type type)
        {
            var queueType = typeof(Queue<>).MakeGenericType(type);
            var queueInstance = Activator.CreateInstance(queueType);
            return queueInstance;
        }

        /// <summary>
        /// 生成期望类型的列表实例
        /// </summary>
        /// <param name="type">期望类型</param>
        /// <returns></returns>
        public static object CreateList([In] Type type)
        {
            var listType = typeof(List<>).MakeGenericType(type);
            var listInstance = Activator.CreateInstance(listType);
            return listInstance;
        }

        /// <summary>
        /// 生成期望键值对类型的字典实例
        /// </summary>
        /// <param name="keyType">期望键类型</param>
        /// <param name="valueType">期望值类型</param>
        /// <returns></returns>
        public static object CreateDictionary([In] Type keyType, [In] Type valueType)
        {
            var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var dictInstance = Activator.CreateInstance(dictType);
            return dictInstance;
        }
    }
}

namespace Convention
{
    public static class BinarySerializeUtility
    {
        #region Base

        public static void WriteBool(BinaryWriter writer, bool data)
        {
            writer.Write(data);
        }
        public static bool ReadBool(BinaryReader reader)
        {
            return reader.ReadBoolean();
        }
        public static void WriteInt(BinaryWriter writer, in int data)
        {
            writer.Write(data);
        }
        public static int ReadInt(BinaryReader reader)
        {
            return reader.ReadInt32();
        }
        public static void WriteFloat(BinaryWriter writer, in float data)
        {
            writer.Write(data);
        }
        public static float ReadFloat(BinaryReader reader)
        {
            return reader.ReadSingle();
        }
        public static void WriteChar(BinaryWriter writer, in char data)
        {
            writer.Write(data);
        }
        public static char ReadChar(BinaryReader reader)
        {
            return reader.ReadChar();
        }
        public static void WriteString(BinaryWriter writer, in string data)
        {
            writer.Write(data ?? "");
        }
        public static string ReadString(BinaryReader reader)
        {
            return reader.ReadString();
        }
        public static void WriteVec3(BinaryWriter writer, in Vector3 data)
        {
            writer.Write(data.x);
            writer.Write(data.y);
            writer.Write(data.z);
        }
        public static Vector3 ReadVec3(BinaryReader reader)
        {
            float x, y, z;
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            return new Vector3(x, y, z);
        }
        public static void WriteVec2(BinaryWriter writer, in Vector2 data)
        {
            writer.Write(data.x);
            writer.Write(data.y);
        }
        public static Vector2 ReadVec2(BinaryReader reader)
        {
            float x, y;
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            return new Vector2(x, y);
        }
        public static void WriteColor(BinaryWriter writer, in Color data)
        {
            writer.Write(data.r);
            writer.Write(data.g);
            writer.Write(data.b);
            writer.Write(data.a);
        }
        public static Color ReadColor(BinaryReader reader)
        {
            float r, g, b, a;
            r = reader.ReadSingle();
            g = reader.ReadSingle();
            b = reader.ReadSingle();
            a = reader.ReadSingle();
            return new(r, g, b, a);
        }

        #endregion

        #region Serialize

        #region Int

        public static void SerializeNativeArray(BinaryWriter writer, in NativeArray<int> array, int start = 0, int end = int.MaxValue)
        {
            int e = Mathf.Min(array == null ? 0 : array.Length, end);
            writer.Write(e - start);
            while (start < e)
            {
                writer.Write(array[start++]);
            }
        }
        public static int DeserializeNativeArray(BinaryReader reader, ref NativeArray<int> array)
        {
            int count = reader.ReadInt32();
            if (array.Length < count)
                array.ResizeArray(count);
            for (int i = 0; i < count; i++)
            {
                array[i] = reader.ReadInt32();
            }
            return count;
        }
        public static void SerializeArray(BinaryWriter writer, in int[] array, int start = 0, int end = int.MaxValue)
        {
            int e = Mathf.Min(array == null ? 0 : array.Length, end);
            writer.Write(e - start);
            while (start < e)
            {
                writer.Write(array[start++]);
            }
        }
        public static int[] DeserializeIntArray(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            int[] array = new int[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = reader.ReadInt32();
            }
            return array;
        }

        #endregion

        #region Float

        public static void SerializeNativeArray(BinaryWriter writer, in NativeArray<float> array, int start = 0, int end = int.MaxValue)
        {
            int e = Mathf.Min(array == null ? 0 : array.Length, end);
            writer.Write(e - start);
            while (start < e)
            {
                writer.Write(array[start++]);
            }
        }
        public static int DeserializeNativeArray(BinaryReader reader, ref NativeArray<float> array)
        {
            int count = reader.ReadInt32();
            if (array.Length < count)
                array.ResizeArray(count);
            for (int i = 0; i < count; i++)
            {
                array[i] = reader.ReadSingle();
            }
            return count;
        }
        public static void SerializeArray(BinaryWriter writer, in float[] array, int start = 0, int end = int.MaxValue)
        {
            int e = Mathf.Min(array == null ? 0 : array.Length, end);
            writer.Write(e - start);
            while (start < e)
            {
                writer.Write(array[start++]);
            }
        }
        public static float[] DeserializeFloatArray(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            float[] array = new float[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = reader.ReadSingle();
            }
            return array;
        }

        #endregion

        #region String

        public static void SerializeArray(BinaryWriter writer, in string[] array, int start = 0, int end = int.MaxValue)
        {
            int e = Mathf.Min(array == null ? 0 : array.Length, end);
            writer.Write(e - start);
            while (start < e)
            {
                writer.Write(array[start++] ?? "");
            }
        }
        public static string[] DeserializeStringArray(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            string[] array = new string[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = reader.ReadString();
            }
            return array;
        }

        #endregion

        #region Vector3

        public static void SerializeNativeArray(BinaryWriter writer, in NativeArray<Vector3> array, int start = 0, int end = int.MaxValue)
        {
            int e = Mathf.Min(array == null ? 0 : array.Length, end);
            writer.Write(e - start);
            while (start < e)
            {
                writer.Write(array[start].x);
                writer.Write(array[start].y);
                writer.Write(array[start].z);
                start++;
            }
        }
        public static int DeserializeNativeArray(BinaryReader reader, ref NativeArray<Vector3> array)
        {
            int count = reader.ReadInt32();
            if (array.Length < count)
                array.ResizeArray(count);
            float x, y, z;
            for (int i = 0; i < count; i++)
            {
                x = reader.ReadSingle();
                y = reader.ReadSingle();
                z = reader.ReadSingle();
                array[i] = new(x, y, z);
            }
            return count;
        }
        public static void SerializeArray(BinaryWriter writer, in Vector3[] array, int start = 0, int end = int.MaxValue)
        {
            int e = Mathf.Min(array == null ? 0 : array.Length, end);
            writer.Write(e - start);
            while (start < e)
            {
                writer.Write(array[start].x);
                writer.Write(array[start].y);
                writer.Write(array[start].z);
                start++;
            }
        }
        public static Vector3[] DeserializeVec3Array(BinaryReader reader, int start = 0)
        {
            int count = reader.ReadInt32();
            Vector3[] array = new Vector3[count];
            float x, y, z;
            for (int i = 0; i < count; i++)
            {
                x = reader.ReadSingle();
                y = reader.ReadSingle();
                z = reader.ReadSingle();
                array[start + i] = new(x, y, z);
            }
            return array;
        }

        #endregion

        #region Vector2

        public static void SerializeNativeArray(BinaryWriter writer, in NativeArray<Vector2> array, int start = 0, int end = int.MaxValue)
        {
            int e = Mathf.Min(array == null ? 0 : array.Length, end);
            writer.Write(e - start);
            while (start < e)
            {
                writer.Write(array[start].x);
                writer.Write(array[start].y);
                start++;
            }
        }
        public static int DeserializeNativeArray(BinaryReader reader, ref NativeArray<Vector2> array)
        {
            int count = reader.ReadInt32();
            if (array.Length < count)
                array.ResizeArray(count);
            float x, y;
            for (int i = 0; i < count; i++)
            {
                x = reader.ReadSingle();
                y = reader.ReadSingle();
                array[i] = new(x, y);
            }
            return count;
        }
        public static void SerializeArray(BinaryWriter writer, in Vector2[] array, int start = 0, int end = int.MaxValue)
        {
            int e = Mathf.Min(array == null ? 0 : array.Length, end);
            writer.Write(e - start);
            while (start < e)
            {
                writer.Write(array[start].x);
                writer.Write(array[start].y);
                start++;
            }
        }
        public static Vector2[] DeserializeVec2Array(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            Vector2[] array = new Vector2[count];
            float x, y;
            for (int i = 0; i < count; i++)
            {
                x = reader.ReadSingle();
                y = reader.ReadSingle();
                array[i] = new(x, y);
            }
            return array;
        }

        #endregion

        #region Color

        public static void SerializeNativeArray(BinaryWriter writer, in NativeArray<Color> array, int start = 0, int end = int.MaxValue)
        {
            int e = Mathf.Min(array.Length, end);
            writer.Write(e - start);
            while (start < e)
            {
                WriteColor(writer, array[start]);
                start++;
            }
        }
        public static int DeserializeNativeArray(BinaryReader reader, ref NativeArray<Color> array)
        {
            int count = reader.ReadInt32();
            if (array.Length < count)
                array.ResizeArray(count);
            float x, y, z;
            for (int i = 0; i < count; i++)
            {
                array[i] = ReadColor(reader);
            }
            return count;
        }
        public static void SerializeArray(BinaryWriter writer, in Color[] array, int start = 0, int end = int.MaxValue)
        {
            int e = Mathf.Min(array.Length, end);
            writer.Write(e - start);
            while (start < e)
            {
                WriteColor(writer, array[start]);
                start++;
            }
        }
        public static Color[] DeserializeVec3Array(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            Color[] array = new Color[count];
            float x, y, z;
            for (int i = 0; i < count; i++)
            {
                array[i] = ReadColor(reader);
            }
            return array;
        }

        #endregion

        #endregion
    }

    public static partial class Utility
    {
        /// <summary>
        /// 获取类型的友好显示名称，支持泛型、数组等复杂类型
        /// </summary>
        public static string GetFriendlyName(this Type type)
        {
            if (type == null) return null;

            // 处理泛型类型
            if (type.IsGenericType)
            {
                // 特殊处理可空类型（Nullable<T>）
                if (type.IsNullable())
                {
                    return $"{GetFriendlyName(type.GetGenericArguments()[0])}?";
                }

                var sb = new StringBuilder();
                // 获取类型基础名称（移除 `1, `2 等后缀）
                string baseName = type.Name.Contains('`') ? type.Name[..type.Name.IndexOf('`')] : type.Name;
                sb.Append(baseName);
                sb.Append('<');

                // 递归处理泛型参数
                Type[] args = type.GetGenericArguments();
                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(GetFriendlyName(args[i]));
                }

                sb.Append('>');
                return sb.ToString();
            }

            // 处理数组类型
            if (type.IsArray)
            {
                string elementName = GetFriendlyName(type.GetElementType());
                int rank = type.GetArrayRank();
                return rank == 1 ? $"{elementName}[]" : $"{elementName}[{new string(',', rank - 1)}]";
            }

            // 处理指针类型
            if (type.IsPointer)
            {
                return $"{GetFriendlyName(type.GetElementType())}*";
            }

            // 处理引用类型（ref/out 参数）
            if (type.IsByRef)
            {
                return $"{GetFriendlyName(type.GetElementType())}&";
            }

            // 普通类型直接返回名称
            return type.Name;
        }

        /// <summary>
        /// 判断是否为可空类型
        /// </summary>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}