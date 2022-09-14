#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace TexGenerator8.Core {

/**
 * EditorGUIに対する便利処理
 */
static class EditorGuiUtil {
	// ------------------------------------- public メンバ --------------------------------------------

	/**
	 * 指定の範囲にカスタムグラデーションのプレビューを表示する。
	 * 内部でRepaint時にのみ処理するので、OnGUIで呼べばOK
	 */
	public static void drawGradationPreview(
		Rect rect,
		Func<float, Color> getColor
	) {
		if (Event.current.type != EventType.Repaint) return;
		checkInitialized();

		GUI.BeginClip(rect);
		GL.PushMatrix();
		GL.Clear(true, false, Color.black);
		s_guiMtl.SetPass(0);

		// 背景を表示
		GL.Begin(GL.TRIANGLES); {
			var w = (int)( rect.height / 2 );
			var col0 = new Color(1,1,1);
			var col1 = new Color(0.5f,0.5f,0.5f);
			for (int x=0,i=0; x<=rect.width; x+=w,++i) {
				var x0 = x;
				var x1 = min(x+w, rect.width);
				var y0 = 0;
				var y1 = w;
				var y2 = rect.height;

				GL.Color( i%2==0 ? col0 : col1 );
				GL.Vertex3(x0,y0,0); GL.Vertex3(x1,y0,0); GL.Vertex3(x0,y1,0);
				GL.Vertex3(x1,y0,0); GL.Vertex3(x1,y1,0); GL.Vertex3(x0,y1,0);
				GL.Color( i%2==0 ? col1 : col0 );
				GL.Vertex3(x0,y1,0); GL.Vertex3(x1,y1,0); GL.Vertex3(x0,y2,0);
				GL.Vertex3(x1,y1,0); GL.Vertex3(x1,y2,0); GL.Vertex3(x0,y2,0);
			}
		} GL.End();
						
		// グラデーションを表示
		GL.Begin(GL.TRIANGLE_STRIP);
		for (int x=0; x<=rect.width; ++x) {
			var xRate = x / rect.width;
			GL.Color( getColor( xRate ) );
			GL.Vertex3(x, 0, 0);
			GL.Vertex3(x, rect.height, 0);
		}
		GL.End();

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

#endif