#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

using System.Reflection;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace TexGenerator8.Core {

	/**
	 * SerializedPropertyへの便利機能
	 */
	static class SerializedPropertyUtil {
		// ------------------------------------- public メンバ --------------------------------------------

		// SerializedPropertyから、指定の型のオブジェクトを取得する
		// 参考:https://forum.unity.com/threads/get-a-general-object-value-from-serializedproperty.327098/
		public static T getValue<T>(SerializedProperty property) {
			object obj = property.serializedObject.targetObject;
			var propertyNames = property.propertyPath.Split('.');

			// プロパティパスごとにたどって中身を得る
			for (int i=0; i<propertyNames.Length; ++i)
			{
				if (propertyNames[i] == "Array") {
					// Arrayの場合は特別な処理
					var idxStr = propertyNames[i+1].Substring(5, propertyNames[i+1].Length-6);
					var idx = int.Parse( idxStr );
					obj = ((Array)obj).GetValue(idx);
					++i;
				} else {
					// Arrayじゃない場合は、フィールドから取得
					var bf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
					obj = obj.GetType()
						.GetField(propertyNames[i], bf)
						.GetValue(obj);
				}
			}

			return (T)obj;
		}


		// ------------------------------------- private メンバ --------------------------------------------
		// --------------------------------------------------------------------------------------------------
	}

}

#endif
