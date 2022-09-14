
// OnDrawGizmosで使用できるように、いつでもインクルード可能にしているが、
// Editorでのみ有効である必要があるので、ここで有効無効を切り替える
#if UNITY_EDITOR

using Unity.Mathematics;
using static Unity.Mathematics.math;

using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace IzBone.Common {
static internal partial class Gizmos8 {
static public class AttributeUtil {

	/** SerializedPropertyから指定の属性値があれば取り出す */
	public static T GetFieldAttribute<T>(SerializedProperty prop) where T : PropertyAttribute {
		var attrs = GetFieldAttributes(prop);
		if (attrs == null) return null;
		foreach (var i in attrs) {
			if (i is T) return (T)i;
		}
		return null;
	}

	/** SerializedPropertyから属性値を取り出す */
	public static List<PropertyAttribute> GetFieldAttributes(SerializedProperty prop) {
		// 参考 : https://github.com/Unity-Technologies/UnityCsReference/blob/fbd4f2bd409f7adb9b077acfaed620bf992f7e55/Editor/Mono/ScriptAttributeGUI/ScriptAttributeUtility.cs
		return  GetFieldAttributes( GetFieldInfo(prop) );
	}

	/**
	 * フィールドInfoから属性値を取り出す
	 * taken from : https://github.com/Unity-Technologies/UnityCsReference/blob/fbd4f2bd409f7adb9b077acfaed620bf992f7e55/Editor/Mono/ScriptAttributeGUI/ScriptAttributeUtility.cs
	 */
	public static List<PropertyAttribute> GetFieldAttributes(FieldInfo field) {
		if (field == null)
			return null;

		object[] attrs = field.GetCustomAttributes(typeof(PropertyAttribute), true);
		if (attrs != null && attrs.Length > 0)
			return new List<PropertyAttribute>(attrs.Select(e => e as PropertyAttribute));

		return null;
	}

	/**
	 * SerializedProperty から FieldInfo を取得する
	 * taken from : https://hacchi-man.hatenablog.com/entry/2020/03/08/220000
	 */
	public static FieldInfo GetFieldInfo(SerializedProperty property)
	{
		FieldInfo GetField(Type type, string path)
		{
			// 基底クラスのPrivateフィールドがSerializeField指定されている場合、
			// FlattenHierarchyでも取得することができないので、Forで回して取得する
			for (; type!=null; type=type.BaseType) {
				var ret = type.GetField(
					path,
					BindingFlags.NonPublic |
					BindingFlags.Public |
					BindingFlags.Instance |
					BindingFlags.Static
				);
				if (ret != null) return ret;
			}
			return null;
		}

		var parentType = property.serializedObject.targetObject.GetType();
		var splits = property.propertyPath.Split('.');
		var fieldInfo = GetField(parentType, splits[0]);
		for (var i = 1; i < splits.Length; i++)
		{
			if (splits[i] == "Array")
			{
				i += 2;
				if (i >= splits.Length)
					continue;

				var type = fieldInfo.FieldType.IsArray
					? fieldInfo.FieldType.GetElementType()
					: fieldInfo.FieldType.GetGenericArguments()[0];
					
				fieldInfo = GetField(type, splits[i]);
			}
			else
			{
				fieldInfo = i + 1 < splits.Length && splits[i + 1] == "Array"
					? GetField(parentType, splits[i])
					: GetField(fieldInfo.FieldType, splits[i]);
			}

			if (fieldInfo == null)
				throw new Exception("Invalid FieldInfo. " + property.propertyPath);

			parentType = fieldInfo.FieldType;
		}
		
		return fieldInfo;
	}

}
} }
#endif
