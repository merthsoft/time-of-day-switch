using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TimerSwitches {
    public static class Helpers {
        static BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                                    BindingFlags.Static | BindingFlags.GetField | BindingFlags.GetProperty;

        public static T GetBaseInstanceField<T>(this object instance, string fieldName) where T : class {
            var type = instance.GetType().BaseType;
            var field = type.GetField(fieldName, bindingFlags);
            return field.GetValue(instance) as T;
        }

        public static T GetInstanceField<T>(this object instance, string fieldName) where T : class {
            var type = instance.GetType();
            var field = type.GetField(fieldName, bindingFlags);
            return field.GetValue(instance) as T;
        }

        public static void SetInstanceField<T>(this object instance, string fieldName, T value) {
            var type = instance.GetType();
            var field = type.GetField(fieldName, bindingFlags);
            field.SetValue(instance, value);
        }

        public static void SetBaseInstanceField<T>(this object instance, string fieldName, T value) {
            var type = instance.GetType().BaseType;
            var field = type.GetField(fieldName, bindingFlags);
            field.SetValue(instance, value);
        }

        public static void InvokeMethod(this object obj, string methodName, params object[] methodParams) {
            MethodInfo dynMethod = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(obj, methodParams);
        }

        public static T InvokeMethod<T>(this object obj, string methodName, params object[] methodParams) where T : class {
            MethodInfo dynMethod = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return dynMethod.Invoke(obj, methodParams) as T;
        }

        public static void InvokeBaseMethod(this object obj, string methodName, params object[] methodParams) {
            MethodInfo dynMethod = obj.GetType().BaseType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(obj, methodParams);
        }

        public static T InvokeBaseMethod<T>(this object obj, string methodName, params object[] methodParams) where T : class {
            MethodInfo dynMethod = obj.GetType().BaseType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return dynMethod.Invoke(obj, methodParams) as T;
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action) {
            foreach (T t in list) {
                action(t);
            }
        }
    }
}
