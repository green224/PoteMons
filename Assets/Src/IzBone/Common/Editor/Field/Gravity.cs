using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common.Field {


	[CustomPropertyDrawer(typeof(Gravity))]
	sealed class GravityEditor : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 2 + 2;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			// プロパティを得る
			var prop_scale = property.FindPropertyRelative("scale");
			var prop_useCustom = property.FindPropertyRelative("useCustom");
			var prop_customA = property.FindPropertyRelative("customA");

			// ラベル
			var line = position;
			line.height = EditorGUIUtility.singleLineHeight;
			var pos = EditorGUI.PrefixLabel(line, label);

			{// カスタム重力を使用するか否か
				var lastIdtLv = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				var p0 = pos;
				var p1 = pos;
				p0.xMax -= 18;
				var style = new GUIStyle( GUI.skin.label );
				style.alignment = TextAnchor.MiddleRight;
				EditorGUI.LabelField( p0, "UseCustom", style );
				p1.xMin = p1.xMax - 14;
				EditorGUI.PropertyField( p1, prop_useCustom, GUIContent.none );

				EditorGUI.indentLevel = lastIdtLv;
			}

			// 中身
			line.y += line.height + 2;
			EditorGUI.indentLevel++;
			if ( prop_useCustom.boolValue ) {
				EditorGUI.PropertyField( line, prop_customA, GUIContent.none );
			} else {
				EditorGUI.PropertyField( line, prop_scale, new GUIContent( "scale" ) );
			}
			EditorGUI.indentLevel--;


			EditorGUI.EndProperty();
		}
	}

}