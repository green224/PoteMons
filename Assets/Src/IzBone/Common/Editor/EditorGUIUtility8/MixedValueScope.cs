using System;
using UnityEditor;


namespace IzBone.Common {
internal static partial class EditorGUIUtility8 {

	/** EditorGUI.showMixedValue を、指定値にするスコープ */
	public sealed class MixedValueScope : IDisposable
	{
		public MixedValueScope(SerializedProperty prop)
			: this(prop.hasMultipleDifferentValues) {}

		public MixedValueScope(bool isMixed) {
			_lastMixed = EditorGUI.showMixedValue;
			EditorGUI.showMixedValue = isMixed;
		}

		public void Dispose() {
			EditorGUI.showMixedValue = _lastMixed;
		}

		bool _lastMixed;
	}


}}
