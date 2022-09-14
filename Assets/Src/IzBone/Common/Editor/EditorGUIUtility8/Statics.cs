using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common {

/**
 * EditorGUIに対する便利処理
 */
internal static partial class EditorGUIUtility8
{
	// ------------------------------------- public メンバ --------------------------------------------

	/**
	 * 指定の範囲にカーブ(x:0~1,y:0~1)のプレビューを表示する。
	 * 内部でRepaint時にのみ処理するので、OnGUIで呼べばOK
	 */
	public static void drawGraphPreview(
		Rect rect,
		Func<float, float> getValue
	) {
		if (Event.current.type != EventType.Repaint) return;
		checkInitialized();

		GUI.BeginClip(rect);
		GL.PushMatrix();
		GL.Clear(true, false, Color.black);
		s_guiMtl.SetPass(0);

		// 背景を表示
		GL.Begin(GL.TRIANGLES); {
			var x0 = 0;
			var x1 = rect.width;
			var y0 = 0;
			var y1 = rect.height;
			GL.Color( new Color(0.15f,0.15f,0.15f) );
			GL.Vertex3(x0,y0,0); GL.Vertex3(x1,y0,0); GL.Vertex3(x0,y1,0);
			GL.Vertex3(x1,y0,0); GL.Vertex3(x1,y1,0); GL.Vertex3(x0,y1,0);
		} GL.End();

		// グラフを表示
		GL.Begin(GL.LINE_STRIP); {
			GL.Color( new Color(0,1,0) );
			for (int x=0; x<=rect.width-4; ++x) {
				var xRate = x / (rect.width-4);
				var yRate = 1 - getValue(xRate);
				GL.Vertex3(2+x, 3 + (rect.height-6)*yRate, 0);
			}
			GL.Vertex3(rect.width-2, 3 + (rect.height-6)*(1-getValue(1)), 0);
		} GL.End();

		// 枠を表示
		GL.Begin(GL.LINE_STRIP); {
			float2 size = float2( (int)rect.width, (int)rect.height ) - 0.5f;
			GL.Color( Color.black );
			GL.Vertex3(   0.5f,   0.5f, 0 );
			GL.Vertex3( size.x,   0.5f, 0 );
			GL.Vertex3( size.x, size.y, 0 );
			GL.Vertex3(   0.5f, size.y, 0 );
			GL.Vertex3(   0.5f,   0.5f, 0 );
		} GL.End();

		GL.PopMatrix();
		GUI.EndClip();
	}


	// ------------------------------------- private メンバ --------------------------------------------

	static Material s_guiMtl = null;

	static void checkInitialized() {
		// マテリアルを生成
		if (s_guiMtl == null) {
			var shader = Shader.Find("Hidden/Internal-Colored");
			s_guiMtl = new Material(shader);
		}
	}


	// --------------------------------------------------------------------------------------------------
}

}
