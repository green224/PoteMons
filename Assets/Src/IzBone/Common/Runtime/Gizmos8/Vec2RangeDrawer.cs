using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IzBone.Common {

	/** Vector2使用して範囲スライダー表示を行う属性 */
	internal sealed class Vec2RangeAttribute : PropertyAttribute {
		readonly public float min, max;
		public Vec2RangeAttribute(float min, float max) {
			this.min = min; this.max = max;
		}
	}

#if UNITY_EDITOR
	/**
	 * Vector2使用して範囲スライダー表示を行う
	 */
	[ CustomPropertyDrawer( typeof( Vec2RangeAttribute ) ) ]
	sealed class Vec2RangeDrawer : PropertyDrawer {

		public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent lbl) {
			if ( prop.propertyType == SerializedPropertyType.Vector2 ) {
				using (var cc=new EditorGUI.ChangeCheckScope()) {
					var a = drawCore(prop.vector2Value, rect, prop, lbl);
					if (cc.changed) prop.vector2Value = a;
				}
				return;
			}

			var xProp = prop.FindPropertyRelative( "x" );
			var yProp = prop.FindPropertyRelative( "y" );
			if (xProp==null || yProp==null) {
				xProp = prop.FindPropertyRelative( "value.x" );
				yProp = prop.FindPropertyRelative( "value.y" );
			}

			if (xProp==null || yProp==null) {
				EditorGUI.HelpBox(rect, "invalid type", MessageType.Error);
				return;
			}

			var x = xProp.floatValue;
			var y = yProp.floatValue;
			using (var cc=new EditorGUI.ChangeCheckScope()) {
				var vec = drawCore(new Vector2(x,y), rect, prop, lbl);
				if (cc.changed) {
					xProp.floatValue = vec.x;
					yProp.floatValue = vec.y;
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return Screen.width < 333 ? (16f + 18f) : 16f;
		}

		Vector2 drawCore(Vector2 src, Rect rect, SerializedProperty property, GUIContent label) {
			var a = src.x;
			var b = src.y;
			var attr = ( Vec2RangeAttribute )attribute;

			if (Screen.width < 333) {
				EditorGUI.PrefixLabel(rect, label);
				rect.yMin += 18;
			} else {
				rect = EditorGUI.PrefixLabel(rect, label);
			}

			float ffsize = 50;
			var rct0 = new Rect(rect.x, rect.y, ffsize, rect.height);
			var rct1 = new Rect(rect.x+ffsize+5, rect.y, rect.width-ffsize*2-10, rect.height);
			var rct2 = new Rect(rect.x+rect.width-ffsize, rect.y, ffsize, rect.height);

			a = EditorGUI.FloatField(rct0, a);
			EditorGUI.MinMaxSlider(rct1, ref a, ref b, attr.min, attr.max);
			b = EditorGUI.FloatField(rct2, b);

			return new Vector2(a, b);
		}
	}
#endif

}