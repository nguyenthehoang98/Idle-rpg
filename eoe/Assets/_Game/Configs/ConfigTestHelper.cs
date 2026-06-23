#if UNITY_INCLUDE_TESTS && UNITY_EDITOR
using System;
using System.Reflection;

namespace _Game.Configs
{
    public static class ConfigTestHelper
    {
        private static readonly BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;

        public static void SetField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, InstanceNonPublic);
            if (field == null) throw new ArgumentException($"Field '{fieldName}' not found on {target.GetType()}");
            field.SetValue(target, value);
        }

        public static T GetField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, InstanceNonPublic);
            if (field == null) throw new ArgumentException($"Field '{fieldName}' not found on {target.GetType()}");
            return (T)field.GetValue(target);
        }
    }
}
#endif