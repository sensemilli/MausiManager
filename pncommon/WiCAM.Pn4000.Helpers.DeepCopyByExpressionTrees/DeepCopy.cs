using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WiCAM.Pn4000.Helpers.DeepCopyByExpressionTrees;

public static class DeepCopy
{
	public class ReferenceEqualityComparer : EqualityComparer<object>
	{
		public override bool Equals(object x, object y)
		{
			return x == y;
		}

		public override int GetHashCode(object obj)
		{
			return obj?.GetHashCode() ?? 0;
		}
	}

	private static readonly object IsStructTypeToDeepCopyDictionaryLocker = new object();

	private static Dictionary<Type, bool> _isStructTypeToDeepCopyDictionary = new Dictionary<Type, bool>();

	private static readonly object CompiledCopyFunctionsDictionaryLocker = new object();

	private static Dictionary<Type, Func<object, Dictionary<object, object>, object>> _compiledCopyFunctionsDictionary = new Dictionary<Type, Func<object, Dictionary<object, object>, object>>();

	private static readonly Type ObjectType = typeof(object);

	private static readonly Type ObjectDictionaryType = typeof(Dictionary<object, object>);

	private static readonly Type FieldInfoType = typeof(FieldInfo);

	private static readonly MethodInfo SetValueMethod = DeepCopy.FieldInfoType.GetMethod("SetValue", new Type[2]
	{
		DeepCopy.ObjectType,
		DeepCopy.ObjectType
	});

	private static readonly Type ThisType = typeof(DeepCopy);

	private static readonly MethodInfo DeepCopyByExpressionTreeObjMethod = DeepCopy.ThisType.GetMethod("DeepCopyByExpressionTreeObj", BindingFlags.Static | BindingFlags.NonPublic);

	public static T Copy<T>(this T original, Dictionary<object, object> copiedReferencesDict = null)
	{
		return (T)DeepCopy.DeepCopyByExpressionTreeObj(original, forceDeepCopy: false, copiedReferencesDict ?? new Dictionary<object, object>(new ReferenceEqualityComparer()));
	}

	private static object DeepCopyByExpressionTreeObj(object original, bool forceDeepCopy, Dictionary<object, object> copiedReferencesDict)
	{
		if (original == null)
		{
			return null;
		}
		Type type = original.GetType();
		if (DeepCopy.IsDelegate(type))
		{
			return null;
		}
		if (!forceDeepCopy && !DeepCopy.IsTypeToDeepCopy(type))
		{
			return original;
		}
		if (copiedReferencesDict.TryGetValue(original, out var value))
		{
			return value;
		}
		if (type == DeepCopy.ObjectType)
		{
			return new object();
		}
		return DeepCopy.GetOrCreateCompiledLambdaCopyFunction(type)(original, copiedReferencesDict);
	}

	private static Func<object, Dictionary<object, object>, object> GetOrCreateCompiledLambdaCopyFunction(Type type)
	{
		if (DeepCopy._compiledCopyFunctionsDictionary.TryGetValue(type, out var value))
		{
			return value;
		}
		lock (DeepCopy.CompiledCopyFunctionsDictionaryLocker)
		{
			if (!DeepCopy._compiledCopyFunctionsDictionary.TryGetValue(type, out value))
			{
				value = DeepCopy.CreateCompiledLambdaCopyFunctionForType(type).Compile();
				Dictionary<Type, Func<object, Dictionary<object, object>, object>> dictionary = DeepCopy._compiledCopyFunctionsDictionary.ToDictionary((KeyValuePair<Type, Func<object, Dictionary<object, object>, object>> pair) => pair.Key, (KeyValuePair<Type, Func<object, Dictionary<object, object>, object>> pair) => pair.Value);
				dictionary.Add(type, value);
				DeepCopy._compiledCopyFunctionsDictionary = dictionary;
			}
		}
		return value;
	}

	private static Expression<Func<object, Dictionary<object, object>, object>> CreateCompiledLambdaCopyFunctionForType(Type type)
	{
		DeepCopy.InitializeExpressions(type, out var inputParameter, out var inputDictionary, out var outputVariable, out var boxingVariable, out var endLabel, out var variables, out var expressions);
		DeepCopy.IfNullThenReturnNullExpression(inputParameter, endLabel, expressions);
		DeepCopy.MemberwiseCloneInputToOutputExpression(type, inputParameter, outputVariable, expressions);
		if (DeepCopy.IsClassOtherThanString(type))
		{
			DeepCopy.StoreReferencesIntoDictionaryExpression(inputParameter, inputDictionary, outputVariable, expressions);
		}
		DeepCopy.FieldsCopyExpressions(type, inputParameter, inputDictionary, outputVariable, boxingVariable, expressions);
		if (DeepCopy.IsArray(type) && DeepCopy.IsTypeToDeepCopy(type.GetElementType()))
		{
			DeepCopy.CreateArrayCopyLoopExpression(type, inputParameter, inputDictionary, outputVariable, variables, expressions);
		}
		return DeepCopy.CombineAllIntoLambdaFunctionExpression(inputParameter, inputDictionary, outputVariable, endLabel, variables, expressions);
	}

	private static void InitializeExpressions(Type type, out ParameterExpression inputParameter, out ParameterExpression inputDictionary, out ParameterExpression outputVariable, out ParameterExpression boxingVariable, out LabelTarget endLabel, out List<ParameterExpression> variables, out List<Expression> expressions)
	{
		inputParameter = Expression.Parameter(DeepCopy.ObjectType);
		inputDictionary = Expression.Parameter(DeepCopy.ObjectDictionaryType);
		outputVariable = Expression.Variable(type);
		boxingVariable = Expression.Variable(DeepCopy.ObjectType);
		endLabel = Expression.Label();
		variables = new List<ParameterExpression>();
		expressions = new List<Expression>();
		variables.Add(outputVariable);
		variables.Add(boxingVariable);
	}

	private static void IfNullThenReturnNullExpression(ParameterExpression inputParameter, LabelTarget endLabel, List<Expression> expressions)
	{
		expressions.Add(Expression.IfThen(Expression.Equal(inputParameter, Expression.Constant(null, DeepCopy.ObjectType)), Expression.Return(endLabel)));
	}

	private static void MemberwiseCloneInputToOutputExpression(Type type, ParameterExpression inputParameter, ParameterExpression outputVariable, List<Expression> expressions)
	{
		expressions.Add(Expression.Assign(outputVariable, Expression.Convert(Expression.Call(inputParameter, DeepCopy.ObjectType.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)), type)));
	}

	private static void StoreReferencesIntoDictionaryExpression(ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, List<Expression> expressions)
	{
		expressions.Add(Expression.Assign(Expression.Property(inputDictionary, DeepCopy.ObjectDictionaryType.GetProperty("Item"), inputParameter), Expression.Convert(outputVariable, DeepCopy.ObjectType)));
	}

	private static Expression<Func<object, Dictionary<object, object>, object>> CombineAllIntoLambdaFunctionExpression(ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, LabelTarget endLabel, List<ParameterExpression> variables, List<Expression> expressions)
	{
		expressions.Add(Expression.Label(endLabel));
		expressions.Add(Expression.Convert(outputVariable, DeepCopy.ObjectType));
		return Expression.Lambda<Func<object, Dictionary<object, object>, object>>(Expression.Block(variables, expressions), new ParameterExpression[2] { inputParameter, inputDictionary });
	}

	private static void CreateArrayCopyLoopExpression(Type type, ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, List<ParameterExpression> variables, List<Expression> expressions)
	{
		int arrayRank = type.GetArrayRank();
		List<ParameterExpression> list = DeepCopy.GenerateIndices(arrayRank);
		variables.AddRange(list);
		Expression expression = DeepCopy.ArrayFieldToArrayFieldAssignExpression(inputParameter, inputDictionary, outputVariable, type.GetElementType(), type, list);
		for (int i = 0; i < arrayRank; i++)
		{
			expression = DeepCopy.LoopIntoLoopExpression(inputParameter, list[i], expression, i);
		}
		expressions.Add(expression);
	}

	private static List<ParameterExpression> GenerateIndices(int arrayRank)
	{
		List<ParameterExpression> list = new List<ParameterExpression>();
		for (int i = 0; i < arrayRank; i++)
		{
			list.Add(Expression.Variable(typeof(int)));
		}
		return list;
	}

	private static BinaryExpression ArrayFieldToArrayFieldAssignExpression(ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, Type elementType, Type arrayType, List<ParameterExpression> indices)
	{
		IndexExpression left = Expression.ArrayAccess(outputVariable, indices);
		MethodCallExpression expression = Expression.ArrayIndex(Expression.Convert(inputParameter, arrayType), indices);
		return Expression.Assign(left, Expression.Convert(Expression.Call(arg1: Expression.Constant(elementType != DeepCopy.ObjectType, typeof(bool)), method: DeepCopy.DeepCopyByExpressionTreeObjMethod, arg0: Expression.Convert(expression, DeepCopy.ObjectType), arg2: inputDictionary), elementType));
	}

	private static BlockExpression LoopIntoLoopExpression(ParameterExpression inputParameter, ParameterExpression indexVariable, Expression loopToEncapsulate, int dimension)
	{
		ParameterExpression parameterExpression = Expression.Variable(typeof(int));
		LabelTarget labelTarget = Expression.Label();
		LoopExpression loopExpression = Expression.Loop(Expression.Block(new ParameterExpression[0], Expression.IfThen(Expression.GreaterThanOrEqual(indexVariable, parameterExpression), Expression.Break(labelTarget)), loopToEncapsulate, Expression.PostIncrementAssign(indexVariable)), labelTarget);
		BinaryExpression lengthForDimensionExpression = DeepCopy.GetLengthForDimensionExpression(parameterExpression, inputParameter, dimension);
		BinaryExpression binaryExpression = Expression.Assign(indexVariable, Expression.Constant(0));
		return Expression.Block(new ParameterExpression[1] { parameterExpression }, lengthForDimensionExpression, binaryExpression, loopExpression);
	}

	private static BinaryExpression GetLengthForDimensionExpression(ParameterExpression lengthVariable, ParameterExpression inputParameter, int i)
	{
		MethodInfo method = typeof(Array).GetMethod("GetLength", BindingFlags.Instance | BindingFlags.Public);
		ConstantExpression constantExpression = Expression.Constant(i);
		return Expression.Assign(lengthVariable, Expression.Call(Expression.Convert(inputParameter, typeof(Array)), method, constantExpression));
	}

	private static void FieldsCopyExpressions(Type type, ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, ParameterExpression boxingVariable, List<Expression> expressions)
	{
		FieldInfo[] allRelevantFields = DeepCopy.GetAllRelevantFields(type);
		List<FieldInfo> list = allRelevantFields.Where((FieldInfo f) => f.IsInitOnly).ToList();
		List<FieldInfo> list2 = allRelevantFields.Where((FieldInfo f) => !f.IsInitOnly).ToList();
		bool flag = list.Any();
		if (flag)
		{
			expressions.Add(Expression.Assign(boxingVariable, Expression.Convert(outputVariable, DeepCopy.ObjectType)));
		}
		foreach (FieldInfo item in list)
		{
			if (DeepCopy.IsDelegate(item.FieldType))
			{
				DeepCopy.ReadonlyFieldToNullExpression(item, boxingVariable, expressions);
			}
			else
			{
				DeepCopy.ReadonlyFieldCopyExpression(type, item, inputParameter, inputDictionary, boxingVariable, expressions);
			}
		}
		if (flag)
		{
			expressions.Add(Expression.Assign(outputVariable, Expression.Convert(boxingVariable, type)));
		}
		foreach (FieldInfo item2 in list2)
		{
			if (DeepCopy.IsDelegate(item2.FieldType))
			{
				DeepCopy.WritableFieldToNullExpression(item2, outputVariable, expressions);
			}
			else
			{
				DeepCopy.WritableFieldCopyExpression(type, item2, inputParameter, inputDictionary, outputVariable, expressions);
			}
		}
	}

	private static FieldInfo[] GetAllRelevantFields(Type type, bool forceAllFields = false)
	{
		List<FieldInfo> list = new List<FieldInfo>();
		Type type2 = type;
		while (type2 != null)
		{
			list.AddRange(from field in type2.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
				where forceAllFields || DeepCopy.IsTypeToDeepCopy(field.FieldType)
				select field);
			type2 = type2.BaseType;
		}
		return list.ToArray();
	}

	private static FieldInfo[] GetAllFields(Type type)
	{
		return DeepCopy.GetAllRelevantFields(type, forceAllFields: true);
	}

	private static void ReadonlyFieldToNullExpression(FieldInfo field, ParameterExpression boxingVariable, List<Expression> expressions)
	{
		expressions.Add(Expression.Call(Expression.Constant(field), DeepCopy.SetValueMethod, boxingVariable, Expression.Constant(null, field.FieldType)));
	}

	private static void ReadonlyFieldCopyExpression(Type type, FieldInfo field, ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression boxingVariable, List<Expression> expressions)
	{
		MemberExpression expression = Expression.Field(Expression.Convert(inputParameter, type), field);
		bool flag = field.FieldType != DeepCopy.ObjectType;
		expressions.Add(Expression.Call(Expression.Constant(field, DeepCopy.FieldInfoType), DeepCopy.SetValueMethod, boxingVariable, Expression.Call(DeepCopy.DeepCopyByExpressionTreeObjMethod, Expression.Convert(expression, DeepCopy.ObjectType), Expression.Constant(flag, typeof(bool)), inputDictionary)));
	}

	private static void WritableFieldToNullExpression(FieldInfo field, ParameterExpression outputVariable, List<Expression> expressions)
	{
		expressions.Add(Expression.Assign(Expression.Field(outputVariable, field), Expression.Constant(null, field.FieldType)));
	}

	private static void WritableFieldCopyExpression(Type type, FieldInfo field, ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, List<Expression> expressions)
	{
		MemberExpression expression = Expression.Field(Expression.Convert(inputParameter, type), field);
		Type fieldType = field.FieldType;
		expressions.Add(Expression.Assign(Expression.Field(outputVariable, field), Expression.Convert(Expression.Call(arg1: Expression.Constant(field.FieldType != DeepCopy.ObjectType, typeof(bool)), method: DeepCopy.DeepCopyByExpressionTreeObjMethod, arg0: Expression.Convert(expression, DeepCopy.ObjectType), arg2: inputDictionary), fieldType)));
	}

	private static bool IsArray(Type type)
	{
		return type.IsArray;
	}

	private static bool IsDelegate(Type type)
	{
		return typeof(Delegate).IsAssignableFrom(type);
	}

	private static bool IsTypeToDeepCopy(Type type)
	{
		if (!DeepCopy.IsClassOtherThanString(type))
		{
			return DeepCopy.IsStructWhichNeedsDeepCopy(type);
		}
		return true;
	}

	private static bool IsClassOtherThanString(Type type)
	{
		if (!type.IsValueType)
		{
			return type != typeof(string);
		}
		return false;
	}

	private static bool IsStructWhichNeedsDeepCopy(Type type)
	{
		if (DeepCopy._isStructTypeToDeepCopyDictionary.TryGetValue(type, out var value))
		{
			return value;
		}
		lock (DeepCopy.IsStructTypeToDeepCopyDictionaryLocker)
		{
			if (!DeepCopy._isStructTypeToDeepCopyDictionary.TryGetValue(type, out value))
			{
				value = DeepCopy.IsStructWhichNeedsDeepCopy_NoDictionaryUsed(type);
				Dictionary<Type, bool> dictionary = DeepCopy._isStructTypeToDeepCopyDictionary.ToDictionary((KeyValuePair<Type, bool> pair) => pair.Key, (KeyValuePair<Type, bool> pair) => pair.Value);
				dictionary[type] = value;
				DeepCopy._isStructTypeToDeepCopyDictionary = dictionary;
			}
		}
		return value;
	}

	private static bool IsStructWhichNeedsDeepCopy_NoDictionaryUsed(Type type)
	{
		if (DeepCopy.IsStructOtherThanBasicValueTypes(type))
		{
			return DeepCopy.HasInItsHierarchyFieldsWithClasses(type);
		}
		return false;
	}

	private static bool IsStructOtherThanBasicValueTypes(Type type)
	{
		if (type.IsValueType && !type.IsPrimitive && !type.IsEnum)
		{
			return type != typeof(decimal);
		}
		return false;
	}

	private static bool HasInItsHierarchyFieldsWithClasses(Type type, HashSet<Type> alreadyCheckedTypes = null)
	{
		alreadyCheckedTypes = alreadyCheckedTypes ?? new HashSet<Type>();
		alreadyCheckedTypes.Add(type);
		List<Type> source = (from f in DeepCopy.GetAllFields(type)
			select f.FieldType).Distinct().ToList();
		if (source.Any(IsClassOtherThanString))
		{
			return true;
		}
		foreach (Type item in (from t in source.Where(IsStructOtherThanBasicValueTypes).ToList()
			where !alreadyCheckedTypes.Contains(t)
			select t).ToList())
		{
			if (DeepCopy.HasInItsHierarchyFieldsWithClasses(item, alreadyCheckedTypes))
			{
				return true;
			}
		}
		return false;
	}
}
