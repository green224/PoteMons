using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IzBone.Common {

	/** クオータニオンの表示をEuler表示にする属性 */
	internal sealed class UseEulerAttribute : PropertyAttribute {}

#if UNITY_EDITOR
	/**
	* クオータニオンのEditor表示をEuler角にする
	* 参考: http://wordpress.notargs.com/blog/blog/2015/11/07/unity5quaternion%E3%82%92%E3%82%AA%E3%82%A4%E3%83%A9%E3%83%BC%E8%A7%92%E3%81%A7%E6%89%B1%E3%81%86%E3%81%9F%E3%82%81%E3%81%AE%E3%82%A8%E3%83%87%E3%82%A3%E3%82%BF%E6%8B%A1%E5%BC%B5%E3%82%92%E4%BD%9C/
	*/
	[ CustomPropertyDrawer( typeof( UseEulerAttribute ) ) ]
	sealed class QuaternionEulerDrawer : PropertyDrawer {

		public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent lbl) {
			if ( prop.propertyType == SerializedPropertyType.Quaternion ) {
				using (var cc=new EditorGUI.ChangeCheckScope()) {
					var a = drawCore(prop.quaternionValue, rect, prop, lbl);
					if (cc.changed) prop.quaternionValue = a;
				}
				return;
			}

			var xProp = prop.FindPropertyRelative( "x" );
			var yProp = prop.FindPropertyRelative( "y" );
			var zProp = prop.FindPropertyRelative( "z" );
			var wProp = prop.FindPropertyRelative( "w" );
			if (xProp==null || yProp==null || zProp==null || wProp==null) {
				xProp = prop.FindPropertyRelative( "value.x" );
				yProp = prop.FindPropertyRelative( "value.y" );
				zProp = prop.FindPropertyRelative( "value.z" );
				wProp = prop.FindPropertyRelative( "value.w" );
			}

			if (xProp==null || yProp==null || zProp==null || wProp==null) {
				EditorGUI.HelpBox(rect, "invalid type", MessageType.Error);
				return;
			}

			var x = xProp.floatValue;
			var y = yProp.floatValue;
			var z = zProp.floatValue;
			var w = wProp.floatValue;
			using (var cc=new EditorGUI.ChangeCheckScope()) {
				var quo = drawCore(new Quaternion(x,y,z,w), rect, prop, lbl);
				if (cc.changed) {
					xProp.floatValue = quo.x;
					yProp.floatValue = quo.y;
					zProp.floatValue = quo.z;
					wProp.floatValue = quo.w;
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return Screen.width < 345 ? (18f + 20f) : 18f;
		}

		Quaternion drawCore(Quaternion src, Rect position, SerializedProperty property, GUIContent label) {
			var euler = src.eulerAngles;
			euler = EditorGUI.Vector3Field(position, label, euler);
			return Quaternion.Euler(euler);
		}
	}
#endif

}