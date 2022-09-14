using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common.Field {


[CustomPropertyDrawer(typeof(HalfLife))]
sealed class HalfLifeEditor : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);

		// プロパティを得る
		var prop_value = property.FindPropertyRelative("value");

		// 減衰力としてのインスペクタ表示が必要かどうかチェック
		var drgAttr = Gizmos8.AttributeUtil.GetFieldAttribute<HalfLifeDragAttribute>(property);
		if (drgAttr == null) {

			// 普通にインスペクタ表示
			using (var cc = new EditorGUI.ChangeCheckScope()) {
				var a = prop_value.floatValue;
				a = EditorGUI.Slider(
					position, label, a,
					min(HalfLifeDragAttribute.LEFT_VAL, HalfLifeDragAttribute.RIGHT_VAL),
					max(HalfLifeDragAttribute.LEFT_VAL, HalfLifeDragAttribute.RIGHT_VAL)
				);
				if (cc.changed) {
					prop_value.floatValue = a;
				}
			}

		} else {

			// 減衰力としてインスペクタ表示
			using (var cc = new EditorGUI.ChangeCheckScope()) {
				var a = HalfLifeDragAttribute.halfLife2ShowValue( prop_value.floatValue );
				a = EditorGUI.Slider(position, label, a, 0, 1);
				a = HalfLifeDragAttribute.showValue2HalfLife( a );

				if (cc.changed) {
					prop_value.floatValue = a;
				}
			}
		}

		EditorGUI.EndProperty();
	}
}

}