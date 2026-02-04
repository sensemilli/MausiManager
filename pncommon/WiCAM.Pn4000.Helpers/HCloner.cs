using System;
using System.Reflection;
using System.Windows.Shapes;

namespace WiCAM.Pn4000.Helpers;

public class HCloner
{
	private static Type _polygonType = typeof(Polygon);

	private static Type _stringType = typeof(string);

	public static T DeepCopy<T>(T obj)
	{
		if (obj == null)
		{
			return default(T);
		}
		return (T)HCloner.Process(obj);
	}

	private static Type GetTypeFromString(string typeName)
	{
		Type type = Type.GetType(typeName);
		if (type != null)
		{
			return type;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			type = assemblies[i].GetType(typeName);
			if (type != null)
			{
				return type;
			}
		}
		return null;
	}

	private static object Process(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		Type type = obj.GetType();
		if (type == typeof(Polygon))
		{
			return null;
		}
		if (type.IsValueType || type == HCloner._stringType)
		{
			return obj;
		}
		if (type.IsArray)
		{
			Type typeFromString = HCloner.GetTypeFromString(type.FullName.Replace("[]", string.Empty));
			Array array = obj as Array;
			Array array2 = Array.CreateInstance(typeFromString, array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				array2.SetValue(HCloner.Process(array.GetValue(i)), i);
			}
			return Convert.ChangeType(array2, obj.GetType());
		}
		if (type.IsClass)
		{
			object obj2 = Activator.CreateInstance(obj.GetType());
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				object value = fieldInfo.GetValue(obj);
				if (value != null)
				{
					fieldInfo.SetValue(obj2, HCloner.Process(value));
				}
			}
			return obj2;
		}
		throw new ArgumentException("Unknown type");
	}
}
