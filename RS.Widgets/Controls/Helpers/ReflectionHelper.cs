using System;
using System.Reflection;

namespace RS.Widgets.Controls
{

    public static class ReflectionHelper
    {

        public static object? ReflectionCallWithRefOut(this object objOrType,
            string methodName,
            Type[] parameterTypes,
            object[] parameters,
            bool isStatic = false)
        {
            Type type = objOrType as Type ?? objOrType.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic |
                        (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            var method = type.GetMethod(methodName, flags, null, parameterTypes, null);
            if (method == null)
            {
                return default;
            }
            return method.Invoke(isStatic ? null : objOrType, parameters);
        }


        public static object? ReflectionGetFieldAll(this object obj, string fieldName)
        {
            var type = obj.GetType();
            var fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                return default;
            }
            return fieldInfo.GetValue(obj);
        }


        public static object? ReflectionGetPropertyAll(this object obj, string propertyName)
        {
            var type = obj.GetType();
            var propInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (propInfo == null)
            {
                return default;
            }
            return propInfo.GetValue(obj);
        }

        public static object? ReflectionNewAll(this Type type, params object[] args)
        {
            var argTypes = Array.ConvertAll(args, a => a?.GetType() ?? typeof(object));
            var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, argTypes, null);
            if (ctor == null)
            {
                return default;
            }
            return ctor.Invoke(args);
        }


        public static TResult? ReflectionStaticCall<TResult>(this Type type, string methodName)
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method == null)
            {
                return default; 
            }
            return (TResult?)method.Invoke(null, null);
        }
        public static TResult? ReflectionStaticCall<TResult, TArg1>(this Type type, string methodName, TArg1 arg1)
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1) }, null);
            if (method == null)
            {
                return default;
            }
            return (TResult?)method.Invoke(null, new object[] { arg1 });
        }
        public static TResult? ReflectionStaticCall<TResult, TArg1, TArg2>(this Type type, string methodName, TArg1 arg1, TArg2 arg2)
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2) }, null);
            if (method == null)
            {
                return default;
            }
            return (TResult?)method.Invoke(null, new object[] { arg1, arg2 });
        }
        public static TResult? ReflectionStaticCall<TResult, TArg1, TArg2, TArg3>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, null);
            if (method == null)
            {
                return default; 
            }
            return (TResult?)method.Invoke(null, new object[] { arg1, arg2, arg3 });
        }
        public static TResult? ReflectionStaticCall<TResult, TArg1, TArg2, TArg3, TArg4>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) }, null);
            if (method == null)
            {
                return default;
            }
            return (TResult?)method.Invoke(null, new object[] { arg1, arg2, arg3, arg4 });
        }
        public static TResult? ReflectionStaticCall<TResult, TArg1, TArg2, TArg3, TArg4, TArg5>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5) }, null);
            if (method == null)
            {
                return default;
            }
            return (TResult?)method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5 });
        }
        public static TResult? ReflectionStaticCall<TResult, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6) }, null);
            if (method == null)
            {
                return default;
            }
            return (TResult?)method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
        }


        public static TResult? ReflectionCall<TResult>(this object obj, string methodName)
        {
            return (TResult?)obj.ReflectionCall(methodName);
        }
        public static object? ReflectionCall(this object obj, string methodName)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method == null)
            {
                return default;
            }
            return method.Invoke(obj, null);
        }
        public static object? ReflectionCall<TArg1>(this object obj, string methodName, TArg1 arg1)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1) }, null);
            if (method == null)
            {
                return default;
            }
            return method.Invoke(obj, new object[] { arg1 });
        }
        public static TResult? ReflectionCall<TResult, TArg1>(this object obj, string methodName, TArg1 arg1)
        {
            return (TResult?)obj.ReflectionCall<TArg1>(methodName, arg1);
        }
        public static object? ReflectionCall<TArg1, TArg2>(this object obj, string methodName, TArg1 arg1, TArg2 arg2)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2) }, null);
            if (method == null)
            {
                return default;
            }
            return method.Invoke(obj, new object[] { arg1, arg2 });
        }
        public static TResult? ReflectionCall<TResult, TArg1, TArg2>(this object obj, string methodName, TArg1 arg1, TArg2 arg2)
        {
            return (TResult?)obj.ReflectionCall<TArg1, TArg2>(methodName, arg1, arg2);
        }
        public static object? ReflectionCall<TArg1, TArg2, TArg3>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, null);
            if (method == null)
            {
                return default;
            }
            return method.Invoke(obj, new object[] { arg1, arg2, arg3 });
        }
        public static TResult? ReflectionCall<TResult, TArg1, TArg2, TArg3>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return (TResult?)obj.ReflectionCall<TArg1, TArg2, TArg3>(methodName, arg1, arg2, arg3);
        }
        public static object? ReflectionCall<TArg1, TArg2, TArg3, TArg4>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) }, null);
            if (method == null)
            {
                return default;
            }
            return method.Invoke(obj, new object[] { arg1, arg2, arg3, arg4 });
        }
        public static TResult? ReflectionCall<TResult, TArg1, TArg2, TArg3, TArg4>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return (TResult?)obj.ReflectionCall<TArg1, TArg2, TArg3, TArg4>(methodName, arg1, arg2, arg3, arg4);
        }
        public static object? ReflectionCall<TArg1, TArg2, TArg3, TArg4, TArg5>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5) }, null);
            if (method == null)
            {
                return default;
            }
            return method.Invoke(obj, new object[] { arg1, arg2, arg3, arg4, arg5 });
        }
        public static TResult? ReflectionCall<TResult, TArg1, TArg2, TArg3, TArg4, TArg5>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            return (TResult?)obj.ReflectionCall<TArg1, TArg2, TArg3, TArg4, TArg5>(methodName, arg1, arg2, arg3, arg4, arg5);
        }
        public static object? ReflectionCall<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6) }, null);
            if (method == null)
            {
                return default;
            }
            return method.Invoke(obj, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
        }
        public static TResult? ReflectionCall<TResult, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            return (TResult?)obj.ReflectionCall<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(methodName, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static TResult? ReflectionGetField<TResult>(this object obj, string fieldName)
        {
            var fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (fieldInfo == null)
            {
                return default;
            }
            return (TResult?)fieldInfo.GetValue(obj);
        }

        public static object? ReflectionNew(this Type type)
        {
            var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (constructor == null)
            {
                return default;
            }
            return constructor.Invoke(null);
        }

        public static object? ReflectionNew<TArg1>(this Type type, TArg1 arg1)
        {
            var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1) }, null);
            if (constructor == null)
            {
                return null;
            }
            return constructor.Invoke(new object[] { arg1 });
        }

        public static object? ReflectionNew<TArg1, TArg2>(this Type type, TArg1 arg1, TArg2 arg2)
        {
            var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(TArg1), typeof(TArg2) }, null);
            if (constructor == null)
            {
               return null;
            }
            return constructor.Invoke(new object[] { arg1, arg2 });
        }

        public static TResult? ReflectionGetProperty<TResult>(this object obj, string propertyName)
        {
            var type = obj.GetType();
            var p = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (p == null)
            {
                return default;
            }
            return (TResult?)p.GetValue(obj);
        }

        public static object? ReflectionGetProperty(this object obj, string propertyName)
        {
            return obj.ReflectionGetProperty<object>(propertyName);
        }

        public static TResult? ReflectionStaticGetProperty<TResult>(this Type type, string propertyName)
        {
            var p = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (p == null)
            {
                return default;
            }
            return (TResult?)p.GetValue(null);
        }



        public static bool ReflectionSetProperty(this object obj, string propertyName, object value)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
            {
                return false;
            }
           

            var prop = obj.GetType().GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(obj, value);
                return true;
            }
            return false;
        }
    }
}