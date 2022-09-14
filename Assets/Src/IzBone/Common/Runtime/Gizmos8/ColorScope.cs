
// OnDrawGizmosで使用できるように、いつでもインクルード可能にしているが、
// Editorでのみ有効である必要があるので、ここで有効無効を切り替える
#if UNITY_EDITOR


using System;
using UnityEngine;

namespace IzBone.Common {
static internal partial class Gizmos8 {

	/** 描画する色を変更するスコープ */
	public sealed class ColorScope : IDisposable
	{
		public ColorScope(Color color) {
			_lastCol = Gizmos8.color;
			Gizmos8.color = color;
		}

		public void Dispose() {
			if (_isDisposed) return;
			color = _lastCol;
			_isDisposed = true;
		}

		Color _lastCol;
		bool _isDisposed = false;

		~ColorScope() {
			if (!_isDisposed) Debug.LogError("ColorScope is not disposed");
		}
	}

} }
#endif
