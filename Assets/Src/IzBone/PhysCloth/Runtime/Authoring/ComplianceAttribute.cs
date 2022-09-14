using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;



namespace IzBone.PhysCloth.Authoring {

using Common;
using Common.Field;


/**
 * Compliance値として使用するfloatにつける属性。
 * インスペクタ表示時に、いい感じのスライダーで設定可能にする。
 */
public sealed class ComplianceAttribute : PropertyAttribute {
	public ComplianceAttribute() {}

	public const float LEFT_VAL = 0.1f;
	public const float RIGHT_VAL = 1e-12f;

	// 強度として表示する値と、実際のcomplianceの値との相互変換
	static public float compliance2ShowValue(float cmp) =>
		(float)PowRangeAttribute.srcValue2showValue(
			cmp, 1000, LEFT_VAL, RIGHT_VAL
		);
	static public float showValue2Compliance(float val) =>
		(float)PowRangeAttribute.showValue2srcValue(
			val, 1000, LEFT_VAL, RIGHT_VAL
		);
}

/**
 * AngleCompliance値として使用するfloatにつける属性。
 * インスペクタ表示時に、いい感じのスライダーで設定可能にする。
 */
public sealed class AngleComplianceAttribute : PropertyAttribute {
	public AngleComplianceAttribute() {}

	public const float LEFT_VAL = 10;
	public const float RIGHT_VAL = 0.0001f;

	// 強度として表示する値と、実際のcomplianceの値との相互変換
	static public float compliance2ShowValue(float cmp) =>
		(float)PowRangeAttribute.srcValue2showValue(
			cmp, 1000, LEFT_VAL, RIGHT_VAL
		);
	static public float showValue2Compliance(float val) =>
		(float)PowRangeAttribute.showValue2srcValue(
			val, 1000, LEFT_VAL, RIGHT_VAL
		);
}

}

