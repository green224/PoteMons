using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common.Field {


[CustomPropertyDrawer(typeof(JointCountAttribute))]
sealed class JointCountEditor : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		var lastIdtLv = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// attribute本体
		var attr = (JointCountAttribute)attribute;

		// intフィールドをそのまま表示
		var pos = position;
		pos.width -= BUTTON_WIDTH*2 + 5;
		EditorGUI.indentLevel = lastIdtLv;
		using (var cc = new EditorGUI.ChangeCheckScope()) {
			var a = EditorGUI.IntField(pos, label, property.intValue);
			if (cc.changed) property.intValue = max(attr.minVal, a);
		}
		EditorGUI.indentLevel = 0;
		
		// ±ボタンを表示
		pos = position;
		pos.xMin = pos.xMax - (BUTTON_WIDTH*2 + 2);
		pos.width = BUTTON_WIDTH;
		if ( GUI.Button(pos, "-", "minibuttonleft") && property.intValue!=attr.minVal )
			property.intValue--;
		pos.x += BUTTON_WIDTH + 1;
		if ( GUI.Button(pos, "+", "minibuttonright") )
			property.intValue++;


		EditorGUI.indentLevel = lastIdtLv;
		EditorGUI.EndProperty();
	}

	const int BUTTON_WIDTH = 18;
}

}