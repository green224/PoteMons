using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common.Field {


[CustomPropertyDrawer(typeof(LeftToggleAttribute))]
sealed class LeftToggleEditor : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);

		using (new EditorGUIUtility8.MixedValueScope(property))
		using (var cc = new EditorGUI.ChangeCheckScope()) {
			var a = EditorGUI.ToggleLeft(position, label, property.boolValue);
			if (cc.changed) property.boolValue = a;
		}
		
		EditorGUI.EndProperty();
	}
}

}