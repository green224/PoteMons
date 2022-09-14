using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace App.Editor.ShaderInspector.Core {

	/**
	 * MaterialPropertyのhasMixedValueの状態によって、
	 * EditorGUIのMixedValue状態を自動変更するモジュール
	 */
	sealed class MixedValueScope : IDisposable {

		public MixedValueScope(params MaterialProperty[] props) {
			_lastSMV = EditorGUI.showMixedValue;

			var hasMixedValue = false;
			foreach (var i in props)
				hasMixedValue |= i.hasMixedValue;

			EditorGUI.showMixedValue = hasMixedValue;
		}

		public void Dispose() {
			EditorGUI.showMixedValue = _lastSMV;
		}
		
		bool _lastSMV;
	}


}
