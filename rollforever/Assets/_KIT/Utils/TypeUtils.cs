using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace _KIT.Utils
{
#if UNITY_EDITOR
    public static class TypeUtils
    {
        private static Dictionary<string, Type> cacheType = new Dictionary<string, Type>();
		private static Dictionary<Type, Type[]> cacheImplement = new Dictionary<Type, Type[]>();

		public static object GetPropertyProtected(object parent, string fieldName)
		{
			if (parent != null)
			{
				Type type = parent.GetType();
				PropertyInfo field = type.GetProperty(fieldName,
					BindingFlags.Instance | BindingFlags.Default | BindingFlags.NonPublic
				);
				if (field != null)
				{
					return field.GetValue(parent);
				}

				Debug.LogError($"TypeExtension.GetPropertyProtected: Error get field ({fieldName}) of {parent.GetType()}");
			}
			else
			{
				Debug.LogError($"TypeExtension.GetPropertyProtected: Error get field (parent is null)");
			}
			return null;
		}
		public static object GetFieldNonPublic(object parent, string fieldName)
		{
			if (parent != null)
			{
				Type type = parent.GetType();
				FieldInfo field = type.GetField(fieldName,
					BindingFlags.Instance | BindingFlags.Default | BindingFlags.NonPublic
				);
				if (field != null)
				{
					return field.GetValue(parent);
				}
			}

			Debug.LogError($"TypeExtension.GetFieldProtected: Error get field ({fieldName}) of {parent.GetType()}");
			return null;
		}

		public static void SetFieldProtected(object parent, string fieldName, object value)
		{
			if (parent != null)
			{
				Type type = parent.GetType();
				FieldInfo field = type.GetField(fieldName,
					BindingFlags.Instance | BindingFlags.Default | BindingFlags.NonPublic
				);
				if (field != null)
				{
					field.SetValue(parent, value);
				}
			}
		}public static object GetFieldPublic(object parent, string fieldName)
		{
			if (parent != null)
			{
				Type type = parent.GetType();
				FieldInfo field = type.GetField(fieldName,
					BindingFlags.Instance | BindingFlags.Default | BindingFlags.Public
				);
				if (field != null)
				{
					return field.GetValue(parent);
				}
			}
			
			Debug.LogError($"TypeExtension.GetFieldPublic: Error get field ({fieldName}) of {parent.GetType()}");
			return null;
		}

		public static void SetFieldPublic(object parent, string fieldName, object value)
		{
			if (parent != null)
			{
				Type type = parent.GetType();
				FieldInfo field = type.GetField(fieldName,
					BindingFlags.Instance | BindingFlags.Default | BindingFlags.Public
				);
				if (field != null)
				{
					field.SetValue(parent, value);
				}
			}
		}
		
		public static Type GetDepthType(string typeName)
		{
			if (cacheType.TryGetValue(typeName, out Type type))
				return type;
			Type depthType = Type.GetType(typeName);
			if (depthType != null)
				return depthType;
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type3 = assembly.GetType(typeName);
				if (type3 != null)
				{
					cacheType[typeName] = type3;
					return type3;
				}
			}

			return null;
		}

		public static Type GetDeepestBaseType(Type type)
		{
			if (type.BaseType == null || type.BaseType == typeof(object))
				return type;
			return GetDeepestBaseType(type.BaseType);
		}

		public static void GetBaseOfType(Type type, Func<Type, bool> func)
		{
			if ((type.BaseType == null || type.BaseType == typeof(object)) && func(type))
				return;
			GetBaseOfType(type.BaseType, func);
		}

		public static bool GetBaseOfType(Type type, Type baseTypeCompare)
		{
			if (type.BaseType == null || type.BaseType == typeof(object))
				return false;
			if (type == baseTypeCompare)
				return true;
			return GetBaseOfType(type.BaseType, baseTypeCompare);
		}

		public static Type[] GetAllTypeThatImplement<T>()
		{
			if (cacheImplement.TryGetValue(typeof(T), out Type[] typeArray))
				return typeArray;
			Type[] array = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => (IEnumerable<Type>) x.GetTypes()).Where(x =>
				{
					if (typeof(T).IsAssignableFrom(x) && !x.IsInterface)
						return !x.IsAbstract;
					return false;
				}).Select(x => x).ToArray();
			cacheImplement[typeof(T)] = array;
			return array;
		}

		public static string[] GetAllNamesThatImplement<T>()
		{
			if (!cacheImplement.TryGetValue(typeof(T), out Type[] array))
			{
				array = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(x => (IEnumerable<Type>) x.GetTypes()).Where(x =>
					{
						if (typeof(T).IsAssignableFrom(x) && !x.IsInterface)
							return !x.IsAbstract;
						return false;
					}).Select(x => x).ToArray();
				cacheImplement[typeof(T)] = array;
			}

			string[] strArray = new string[array.Length];
			for (int index = 0; index < strArray.Length; ++index)
				strArray[index] = array[index].Name;
			return strArray;
		}
    }
#endif
}