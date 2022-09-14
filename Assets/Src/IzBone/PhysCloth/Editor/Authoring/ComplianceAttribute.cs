using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;




namespace IzBone.PhysCloth.Authoring {

using Common;
using Common.Field;


abstract class ComplianceAttributeDrawerBase : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);

		using (new EditorGUIUtility8.MixedValueScope(property))
		using (var cc = new EditorGUI.ChangeCheckScope()) {

			var a = compliance2ShowValue( property.floatValue );
			a = EditorGUI.Slider(position, label, a, 0, 1);
			a = showValue2Compliance( a );

			if (cc.changed) {
				property.floatValue = a;
			}
		}

		EditorGUI.EndProperty();
	}

	abstract protected float compliance2ShowValue(float cmp);
	abstract protected float showValue2Compliance(float val);
}


[CustomPropertyDrawer(typeof(ComplianceAttribute))]
sealed class ComplianceAttributeDrawer : ComplianceAttributeDrawerBase {
	override protected float compliance2ShowValue(float cmp) =>
		ComplianceAttribute.compliance2ShowValue( cmp );
	override protected float showValue2Compliance(float val) =>
		ComplianceAttribute.showValue2Compliance( val );
}

[CustomPropertyDrawer(typeof(AngleComplianceAttribute))]
sealed class AngleComplianceAttributeDrawer : ComplianceAttributeDrawerBase {
	override protected float compliance2ShowValue(float cmp) =>
		AngleComplianceAttribute.compliance2ShowValue( cmp );
	override protected float showValue2Compliance(float val) =>
		AngleComplianceAttribute.showValue2Compliance( val );
}


}

