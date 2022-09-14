using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace IzBone.Common.Field {


	/**
	 * int値をジョイント数として表示するための属性。
	 * インスペクタ表示する際に、±のボタンを表示したりする
	 */
	internal sealed class JointCountAttribute : PropertyAttribute {
		readonly public int minVal;

		public JointCountAttribute(int minVal = 0) {
			this.minVal = minVal;
		}
	}


}
