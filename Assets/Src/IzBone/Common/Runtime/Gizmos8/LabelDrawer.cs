using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IzBone.Common {

	/** プロパティのラベル部分を指定の文字列に変更する属性 */
	internal sealed class LabelAttribute : PropertyAttribute {
		readonly public string label;
		public LabelAttribute(string label) {
			this.label = label;
		}
	}

#if UNITY_EDITOR
	/**
	 * プロパティのラベル部分を指定の文字列に変更する
	 */
	[ CustomPropertyDrawer( typeof( LabelAttribute ) ) ]
	sealed class LabelDrawer : PropertyDrawer {

		public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent lbl) {
			var attr = (LabelAttribute)attribute;
			EditorGUI.PropertyField( rect, prop, new GUIContent(attr.label), true );
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, true);
		}
	}
#endif

}