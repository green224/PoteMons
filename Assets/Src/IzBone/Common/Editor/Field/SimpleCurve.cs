using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common.Field {


[CustomPropertyDrawer(typeof(SimpleCurve))]
sealed class SimpleCurveEditor : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		var prop_useCurve = property.FindPropertyRelative("useCurve");
		if (prop_useCurve.boolValue) {
			return EditorGUIUtility.singleLineHeight * 2
				+ EditorGUIUtility.standardVerticalSpacing;
		} else {
			return EditorGUIUtility.singleLineHeight;
		}
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		var lastIdtLv = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// 範囲指定があるかどうかチェック
		var range = Gizmos8.AttributeUtil.GetFieldAttribute<SimpleCurveRangeAttribute>(property);

		// プロパティを得る
		var prop_start = property.FindPropertyRelative("start");
		var prop_end = property.FindPropertyRelative("end");
		var prop_curve = property.FindPropertyRelative("curve");
		var prop_useCurve = property.FindPropertyRelative("useCurve");

		// 行のRectを得る
		var line = position;
		line.yMax = line.yMin + EditorGUIUtility.singleLineHeight;

		// カーブを使用するか否かのトグル
		var pos = line;
		pos.xMin = pos.xMax - 15;
//		EditorGUI.PropertyField(pos, prop_useCurve, GUIContent.none);
		using (var cc = new EditorGUI.ChangeCheckScope()) {
			var a = EditorGUI.Toggle(pos, GUIContent.none, prop_useCurve.boolValue, "PaneOptions");
			if (cc.changed) prop_useCurve.boolValue = a;
		}

		// 値本体部分
		if (prop_useCurve.boolValue) {

			// ラベル
			EditorGUI.indentLevel = lastIdtLv;
			line = EditorGUI.PrefixLabel(line, label);
			EditorGUI.indentLevel = 0;
			pos = line;
			pos.width -= 16;

			// カーブView
			var tgt = new SimpleCurve() {
				start = prop_start.floatValue,
				end = prop_end.floatValue,
				curve = prop_curve.floatValue,
				useCurve = true,
			};
			var minY = min(tgt.start, tgt.end);
			var height = abs(tgt.start - tgt.end) + 0.00001f;
			EditorGUIUtility8.drawGraphPreview(
				pos,
				x => ( tgt.evaluate(x) - minY ) / height
			);

			// パラメータ部分
			line.y += EditorGUIUtility.singleLineHeight
				+ EditorGUIUtility.standardVerticalSpacing;
			pos = line;
			pos.width = 40;
			SimpleCurveRangeAttribute.drawGUI(range, pos, prop_start, GUIContent.none, true);
			pos = line;
			pos.xMin = pos.xMax - 40;
			SimpleCurveRangeAttribute.drawGUI(range, pos, prop_end, GUIContent.none, true);

			pos = line;
			pos.xMin += 46;
			pos.xMax -= 46;
			using (var cc = new EditorGUI.ChangeCheckScope()) {
				var a = GUI.HorizontalSlider(pos, prop_curve.floatValue, 0, 1);
				if (cc.changed) prop_curve.floatValue = a;
			}

		} else {
			// スカラー値ひとつ
			pos = line;
			pos.width -= 16;
			EditorGUI.indentLevel = lastIdtLv;
			SimpleCurveRangeAttribute.drawGUI(
				range, pos,
				prop_start, label,
				dstProps: new[]{prop_start, prop_end}
			);
			EditorGUI.indentLevel = 0;
		}

		EditorGUI.indentLevel = lastIdtLv;
		EditorGUI.EndProperty();
	}
}

}