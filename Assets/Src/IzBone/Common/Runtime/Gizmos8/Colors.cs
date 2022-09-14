
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

	/** 固定色一覧 */
	static internal class Colors {

		readonly public static Color JointMovable = new Color(0, 1, 0, 0.3f);
		readonly public static Color JointFixed = new Color(1, 1, 1, 0.5f);
		readonly public static Color BoneMovable = new Color(1, 0.1f, 1);
		readonly public static Color BoneFixed = new Color(1, 1, 1, 0.75f);

		readonly public static Color AngleLimit = new Color(1, 0, 0);
		readonly public static Color AngleMargin = new Color(0.97f, 0.7f, 0);

		readonly public static Color ShiftLimit = new Color(0, 0, 1);
		readonly public static Color ShiftMargin = new Color(0, 0.7f, 0.97f);

		readonly public static Color Collider = new Color(0.3f, 1f, 0.2f, 0.6f);
	}

} }
#endif
