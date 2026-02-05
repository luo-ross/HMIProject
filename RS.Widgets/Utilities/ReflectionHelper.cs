using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RS.Widgets.Utilities
{
    public static class ReflectionHelper
    {
        // 统一的 BindingFlags 常量
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags AllFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        /// <summary>
        /// 在整个类型层次结构中查找方法
        /// </summary>
        private static MethodInfo? FindMethodInHierarchy(Type type, string methodName, BindingFlags flags, Type[]? parameterTypes = null)
        {
            Type? currentType = type;
            while (currentType != null)
            {
                MethodInfo? method = null;
                
                if (parameterTypes != null && parameterTypes.Length > 0)
                {
                    // 有参数类型，使用 GetMethod 精确匹配
                    method = currentType.GetMethod(methodName, flags, null, parameterTypes, null);
                }
                else
                {
                    // 无参数或参数类型未指定，先尝试使用 GetMethods 获取所有方法
                    var methods = currentType.GetMethods(flags);
                    method = Array.Find(methods, m => 
                        m.Name == methodName && 
                        m.GetParameters().Length == 0);
                    
                    // 如果 GetMethods 找不到，再使用 GetMethod
                    if (method == null)
                    {
                        method = currentType.GetMethod(methodName, flags, null, Type.EmptyTypes, null);
                    }
                }
                
                if (method != null)
                {
                    return method;
                }
                
                currentType = currentType.BaseType;
            }
            
            return null;
        }

        /// <summary>
        /// 调用方法（支持 ref/out 参数）
        /// </summary>
        public static object? ReflectionCallWithRefOut(this object objOrType,
            string methodName,
            Type[] parameterTypes,
            object[] parameters,
            bool isStatic = false)
        {
            if (objOrType == null || string.IsNullOrEmpty(methodName))
            {
                return default;
            }

            Type type = objOrType as Type ?? objOrType.GetType();
            var flags = isStatic ? StaticFlags : InstanceFlags;
            
            var method = FindMethodInHierarchy(type, methodName, flags, parameterTypes);
            if (method == null)
            {
                return default;
            }
            
            try
            {
                return method.Invoke(isStatic ? null : objOrType, parameters);
            }
            catch
            {
                return default;
            }
        }


        /// <summary>
        /// 在整个类型层次结构中查找字段
        /// </summary>
        private static FieldInfo? FindFieldInHierarchy(Type type, string fieldName, BindingFlags flags)
        {
            Type? currentType = type;
            while (currentType != null)
            {
                var fieldInfo = currentType.GetField(fieldName, flags);
                if (fieldInfo != null)
                {
                    return fieldInfo;
                }
                currentType = currentType.BaseType;
            }
            return null;
        }

        /// <summary>
        /// 在整个类型层次结构中查找属性
        /// </summary>
        private static PropertyInfo? FindPropertyInHierarchy(Type type, string propertyName, BindingFlags flags)
        {
            Type? currentType = type;
            while (currentType != null)
            {
                var propInfo = currentType.GetProperty(propertyName, flags);
                if (propInfo != null)
                {
                    return propInfo;
                }
                currentType = currentType.BaseType;
            }
            return null;
        }

        /// <summary>
        /// 获取字段值（在整个类型层次结构中查找）
        /// </summary>
        public static object? ReflectionGetFieldAll(this object obj, string fieldName)
        {
            if (obj == null || string.IsNullOrEmpty(fieldName))
            {
                return default;
            }

            var fieldInfo = FindFieldInHierarchy(obj.GetType(), fieldName, AllFlags);
            if (fieldInfo == null)
            {
                return default;
            }
            
            try
            {
                return fieldInfo.GetValue(obj);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 获取属性值（在整个类型层次结构中查找）
        /// </summary>
        public static object? ReflectionGetPropertyAll(this object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
            {
                return default;
            }

            var propInfo = FindPropertyInHierarchy(obj.GetType(), propertyName, AllFlags);
            if (propInfo == null)
            {
                return default;
            }
            
            try
            {
                return propInfo.GetValue(obj);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 创建实例（支持所有访问级别的构造函数）
        /// </summary>
        public static object? ReflectionNewAll(this Type type, params object[] args)
        {
            if (type == null)
            {
                return default;
            }

            var argTypes = Array.ConvertAll(args, a => a?.GetType() ?? typeof(object));
            var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, argTypes, null);
            if (ctor == null)
            {
                return default;
            }
            
            try
            {
                return ctor.Invoke(args);
            }
            catch
            {
                return default;
            }
        }


        // 静态方法调用的通用实现
        private static TResult? ReflectionStaticCallInternal<TResult>(Type type, string methodName, Type[] argTypes, object[] args)
        {
            if (type == null || string.IsNullOrEmpty(methodName))
            {
                return default;
            }

            var method = FindMethodInHierarchy(type, methodName, StaticFlags, argTypes.Length > 0 ? argTypes : null);
            if (method == null)
            {
                return default;
            }
            
            try
            {
                return (TResult?)method.Invoke(null, args);
            }
            catch
            {
                return default;
            }
        }

        public static TResult? ReflectionStaticCall<TResult>(this Type type, string methodName)
        {
            return ReflectionStaticCallInternal<TResult>(type, methodName, Type.EmptyTypes, Array.Empty<object>());
        }

        public static TResult? ReflectionStaticCall<TResult, TArg1>(this Type type, string methodName, TArg1 arg1)
        {
            return ReflectionStaticCallInternal<TResult>(type, methodName, new[] { typeof(TArg1) }, new object[] { arg1! });
        }

        public static TResult? ReflectionStaticCall<TResult, TArg1, TArg2>(this Type type, string methodName, TArg1 arg1, TArg2 arg2)
        {
            return ReflectionStaticCallInternal<TResult>(type, methodName, new[] { typeof(TArg1), typeof(TArg2) }, new object[] { arg1!, arg2! });
        }

        public static TResult? ReflectionStaticCall<TResult, TArg1, TArg2, TArg3>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return ReflectionStaticCallInternal<TResult>(type, methodName, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, new object[] { arg1!, arg2!, arg3! });
        }

        public static TResult? ReflectionStaticCall<TResult, TArg1, TArg2, TArg3, TArg4>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return ReflectionStaticCallInternal<TResult>(type, methodName, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) }, new object[] { arg1!, arg2!, arg3!, arg4! });
        }

        public static TResult? ReflectionStaticCall<TResult, TArg1, TArg2, TArg3, TArg4, TArg5>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            return ReflectionStaticCallInternal<TResult>(type, methodName, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5) }, new object[] { arg1!, arg2!, arg3!, arg4!, arg5! });
        }

        public static TResult? ReflectionStaticCall<TResult, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            return ReflectionStaticCallInternal<TResult>(type, methodName, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6) }, new object[] { arg1!, arg2!, arg3!, arg4!, arg5!, arg6! });
        }


        // 实例方法调用的通用实现
        private static object? ReflectionCallInternal(object obj, string methodName, Type[] argTypes, object[] args)
        {
            if (obj == null || string.IsNullOrEmpty(methodName))
            {
                return default;
            }

            var method = FindMethodInHierarchy(obj.GetType(), methodName, InstanceFlags, argTypes.Length > 0 ? argTypes : null);
            if (method == null)
            {
                return default;
            }
            
            try
            {
                return method.Invoke(obj, args);
            }
            catch
            {
                return default;
            }
        }

        public static TResult? ReflectionCall<TResult>(this object obj, string methodName)
        {
            return (TResult?)obj.ReflectionCall(methodName);
        }

        public static object? ReflectionCall(this object obj, string methodName)
        {
            return ReflectionCallInternal(obj, methodName, Type.EmptyTypes, Array.Empty<object>());
        }

        public static object? ReflectionCall<TArg1>(this object obj, string methodName, TArg1 arg1)
        {
            return ReflectionCallInternal(obj, methodName, new[] { typeof(TArg1) }, new object[] { arg1! });
        }

        public static TResult? ReflectionCall<TResult, TArg1>(this object obj, string methodName, TArg1 arg1)
        {
            return (TResult?)obj.ReflectionCall<TArg1>(methodName, arg1);
        }

        public static object? ReflectionCall<TArg1, TArg2>(this object obj, string methodName, TArg1 arg1, TArg2 arg2)
        {
            return ReflectionCallInternal(obj, methodName, new[] { typeof(TArg1), typeof(TArg2) }, new object[] { arg1!, arg2! });
        }

        public static TResult? ReflectionCall<TResult, TArg1, TArg2>(this object obj, string methodName, TArg1 arg1, TArg2 arg2)
        {
            return (TResult?)obj.ReflectionCall<TArg1, TArg2>(methodName, arg1, arg2);
        }

        public static object? ReflectionCall<TArg1, TArg2, TArg3>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return ReflectionCallInternal(obj, methodName, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, new object[] { arg1!, arg2!, arg3! });
        }

        public static TResult? ReflectionCall<TResult, TArg1, TArg2, TArg3>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return (TResult?)obj.ReflectionCall<TArg1, TArg2, TArg3>(methodName, arg1, arg2, arg3);
        }

        public static object? ReflectionCall<TArg1, TArg2, TArg3, TArg4>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return ReflectionCallInternal(obj, methodName, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) }, new object[] { arg1!, arg2!, arg3!, arg4! });
        }

        public static TResult? ReflectionCall<TResult, TArg1, TArg2, TArg3, TArg4>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return (TResult?)obj.ReflectionCall<TArg1, TArg2, TArg3, TArg4>(methodName, arg1, arg2, arg3, arg4);
        }

        public static object? ReflectionCall<TArg1, TArg2, TArg3, TArg4, TArg5>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            return ReflectionCallInternal(obj, methodName, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5) }, new object[] { arg1!, arg2!, arg3!, arg4!, arg5! });
        }

        public static TResult? ReflectionCall<TResult, TArg1, TArg2, TArg3, TArg4, TArg5>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            return (TResult?)obj.ReflectionCall<TArg1, TArg2, TArg3, TArg4, TArg5>(methodName, arg1, arg2, arg3, arg4, arg5);
        }

        public static object? ReflectionCall<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            return ReflectionCallInternal(obj, methodName, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6) }, new object[] { arg1!, arg2!, arg3!, arg4!, arg5!, arg6! });
        }

        public static TResult? ReflectionCall<TResult, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            return (TResult?)obj.ReflectionCall<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(methodName, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 获取字段值（在整个类型层次结构中查找）
        /// </summary>
        public static TResult? ReflectionGetField<TResult>(this object obj, string fieldName)
        {
            if (obj == null || string.IsNullOrEmpty(fieldName))
            {
                return default;
            }

            var fieldInfo = FindFieldInHierarchy(obj.GetType(), fieldName, AllFlags);
            if (fieldInfo == null)
            {
                return default;
            }
            
            try
            {
                return (TResult?)fieldInfo.GetValue(obj);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 创建实例（无参数构造函数）
        /// </summary>
        public static object? ReflectionNew(this Type type)
        {
            if (type == null)
            {
                return default;
            }

            var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (constructor == null)
            {
                return default;
            }
            
            try
            {
                return constructor.Invoke(null);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 创建实例（单参数构造函数）
        /// </summary>
        public static object? ReflectionNew<TArg1>(this Type type, TArg1 arg1)
        {
            if (type == null)
            {
                return null;
            }

            var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1) }, null);
            if (constructor == null)
            {
                return null;
            }
            
            try
            {
                return constructor.Invoke(new object[] { arg1! });
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 创建实例（双参数构造函数）
        /// </summary>
        public static object? ReflectionNew<TArg1, TArg2>(this Type type, TArg1 arg1, TArg2 arg2)
        {
            if (type == null)
            {
                return null;
            }

            var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2) }, null);
            if (constructor == null)
            {
                return null;
            }
            
            try
            {
                return constructor.Invoke(new object[] { arg1!, arg2! });
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取属性值（在整个类型层次结构中查找）
        /// </summary>
        public static TResult? ReflectionGetProperty<TResult>(this object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
            {
                return default;
            }

            var propInfo = FindPropertyInHierarchy(obj.GetType(), propertyName, AllFlags);
            if (propInfo == null)
            {
                return default;
            }
            
            try
            {
                return (TResult?)propInfo.GetValue(obj);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 获取属性值（返回 object）
        /// </summary>
        public static object? ReflectionGetProperty(this object obj, string propertyName)
        {
            return obj.ReflectionGetProperty<object>(propertyName);
        }

        /// <summary>
        /// 获取静态属性值
        /// </summary>
        public static TResult? ReflectionStaticGetProperty<TResult>(this Type type, string propertyName)
        {
            if (type == null || string.IsNullOrEmpty(propertyName))
            {
                return default;
            }

            var propInfo = type.GetProperty(propertyName, StaticFlags);
            if (propInfo == null)
            {
                return default;
            }
            
            try
            {
                return (TResult?)propInfo.GetValue(null);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 设置属性值（在整个类型层次结构中查找）
        /// </summary>
        public static bool ReflectionSetProperty(this object obj, string propertyName, object? value)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            var propInfo = FindPropertyInHierarchy(obj.GetType(), propertyName, InstanceFlags);
            if (propInfo == null || !propInfo.CanWrite)
            {
                return false;
            }

            try
            {
                propInfo.SetValue(obj, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 根据属性路径获取属性值（支持嵌套属性，如"User.Name"）
        /// </summary>
        /// <param name="targetObject">目标对象</param>
        /// <param name="propertyPath">属性路径，支持嵌套属性（如"User.Name"）</param>
        /// <returns>属性值，如果属性不存在或路径无效则返回null</returns>
        public static object? GetPropertyValue(object? targetObject, string? propertyPath)
        {
            if (targetObject == null || string.IsNullOrEmpty(propertyPath))
            {
                return null;
            }

            var propertyNames = propertyPath.Split('.');
            object? currentObject = targetObject;

            foreach (var propertyName in propertyNames)
            {
                if (currentObject == null)
                {
                    return null;
                }

                Type type = currentObject.GetType();
                PropertyInfo? property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null)
                {
                    return null;
                }

                currentObject = property.GetValue(currentObject);
            }

            return currentObject;
        }

        /// <summary>
        /// 获取显示成员的值（用于标签显示等场景）
        /// </summary>
        /// <param name="item">数据项</param>
        /// <param name="displayMemberPath">显示成员路径，如果为空则返回null</param>
        /// <returns>显示值，如果路径无效或为空则返回null</returns>
        public static object? GetDisplayMemberValue(object? item, string? displayMemberPath)
        {
            if (item == null || string.IsNullOrEmpty(displayMemberPath))
            {
                return null;
            }

            try
            {
                return GetPropertyValue(item, displayMemberPath);
            }
            catch
            {
                return null;
            }
        }
    }
}