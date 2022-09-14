using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common.Field {


[CustomPropertyDrawer(typeof(PowRangeAttribute))]
sealed class PowRangeEditor : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		var lastIdtLv = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// attribute本体
		var attr = (PowRangeAttribute)attribute;

		// ラベルを表示
		var pos = position;
		EditorGUI.indentLevel = lastIdtLv;
		pos = EditorGUI.PrefixLabel(pos, label);
		EditorGUI.indentLevel = 0;

		// スライダーを表示
		pos.width -= 55;
		using (var cc = new EditorGUI.ChangeCheckScope()) {
			var a = attr.srcValue2showValue( property.floatValue );
			a = GUI.HorizontalSlider(pos, a, 0, 1);
			if (cc.changed) property.floatValue = attr.showValue2srcValue(a);
		}
		
		// 数値入力フィールドを表示
		pos.x = pos.xMax + 5;
		pos.width = 50;
		using (var cc = new EditorGUI.ChangeCheckScope()) {
			var a = EditorGUI.FloatField( pos, GUIContent.none, property.floatValue );
			if (cc.changed) property.floatValue = a;
		}


		EditorGUI.indentLevel = lastIdtLv;
		EditorGUI.EndProperty();
	}

	const int BUTTON_WIDTH = 18;
}

}