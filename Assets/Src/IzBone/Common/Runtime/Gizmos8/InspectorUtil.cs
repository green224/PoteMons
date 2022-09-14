
// OnDrawGizmosで使用できるように、いつでもインクルード可能にしているが、
// Editorでのみ有効である必要があるので、ここで有効無効を切り替える
#if UNITY_EDITOR

using Unity.Mathematics;
using static Unity.Mathematics.math;

using System;
using UnityEditor;
using UnityEngine;

namespace IzBone.Common {
static internal partial class Gizmos8 {
static public class InspectorUtil {

	/** EditorのIntSliderはラベル欄をNoneにすると当たり判定がバグるので、そこを修正した表示処理 */
	public static void drawIntSliderBugFixed(Rect position, SerializedProperty prop) {
		// この関数を呼ぶということはRange属性が必ず指定されているはず
		var attr = AttributeUtil.GetFieldAttribute<RangeAttribute>(prop);
		drawIntSliderBugFixed(position, prop, (int)attr.min, (int)attr.max);
	}

	/** EditorのIntSliderはラベル欄をNoneにすると当たり判定がバグるので、そこを修正した表示処理 */
	public static void drawIntSliderBugFixed(Rect position, SerializedProperty prop, int leftValue, int rightValue) {
		using (var cc = new EditorGUI.ChangeCheckScope()) {
			var a = EditorGUI.Slider(position, prop.intValue, leftValue, rightValue);
			if ( cc.changed ) prop.intValue = Mathf.RoundToInt(a);
		}
	}

}
} }
#endif
