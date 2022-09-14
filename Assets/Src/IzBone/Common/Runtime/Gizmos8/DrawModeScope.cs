
// OnDrawGizmosで使用できるように、いつでもインクルード可能にしているが、
// Editorでのみ有効である必要があるので、ここで有効無効を切り替える
#if UNITY_EDITOR


using System;
using UnityEngine;

namespace IzBone.Common {
static internal partial class Gizmos8 {

	/** DrawModeを変更するスコープ */
	public sealed class DrawModeScope : IDisposable
	{
		public DrawModeScope(DrawMode drawMode) {
			_lastMode = Gizmos8.drawMode;
			Gizmos8.drawMode = drawMode;
		}

		public void Dispose() {
			if (_isDisposed) return;
			drawMode = _lastMode;
			_isDisposed = true;
		}

		DrawMode _lastMode;
		bool _isDisposed = false;

		~DrawModeScope() {
			if (!_isDisposed) Debug.LogError("DrawModeScope is not disposed");
		}
	}

} }
#endif
