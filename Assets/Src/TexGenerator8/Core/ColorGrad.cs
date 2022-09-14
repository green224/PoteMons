using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using Unity.Mathematics;
using static Unity.Mathematics.math;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TexGenerator8.Core {

	/**
	 * カラーの指定方法をいくつか選ぶことができる
	 * インスペクタ公開用グラデーションモジュール。
	 */
	[Serializable] public sealed class ColorGrad {
		// ------------------------------------- public メンバ --------------------------------------------

		public enum Mode { Gradient, RgbaCurve, HsvaCurve, }
		public Mode mode = Mode.Gradient;

		public Gradient gradient =
			new Gradient() {
				alphaKeys = new []{ new GradientAlphaKey(1,0) },
				colorKeys = new []{ new GradientColorKey(Color.white,0) },
			};
		public AnimationCurve xCurve =
			new AnimationCurve(new []{
				new Keyframe(0, 1),
				new Keyframe(1, 1),
			});
		public AnimationCurve yCurve =
			new AnimationCurve(new []{
				new Keyframe(0, 1),
				new Keyframe(1, 1),
			});
		public AnimationCurve zCurve =
			new AnimationCurve(new []{
				new Keyframe(0, 1),
				new Keyframe(1, 1),
			});
		public AnimationCurve wCurve =
			new AnimationCurve(new []{
				new Keyframe(0, 1),
				new Keyframe(1, 1),
			});

		/** 入力値を指定して、モードを考慮したカラー値を計算する */
		public Color evaluate(float t) {
			static Color hsvCol(float h, float s, float v, float a) {
				var ret = Color.HSVToRGB(frac(h),s,v,true);
				ret.a = a;
				return ret;
			}
			return mode switch {
				Mode.Gradient => gradient.Evaluate(t),
				Mode.RgbaCurve => new Color(
					saturate( xCurve.Evaluate(t) ),
					saturate( yCurve.Evaluate(t) ),
					saturate( zCurve.Evaluate(t) ),
					saturate( wCurve.Evaluate(t) )
				),
				Mode.HsvaCurve => hsvCol(
					xCurve.Evaluate(t),
					saturate( yCurve.Evaluate(t) ),
					saturate( zCurve.Evaluate(t) ),
					saturate( wCurve.Evaluate(t) )
				),
				_ => default,
			};
		}


		// ------------------------------------- private メンバ --------------------------------------------
		// --------------------------------------------------------------------------------------------------
	}

#if UNITY_EDITOR
	[ CustomPropertyDrawer( typeof( ColorGrad ) ) ]
	sealed class ColorGradDrawer : PropertyDrawer {

		public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent lbl) {

			var prop_mode = prop.FindPropertyRelative( "mode" );
			var prop_grad = prop.FindPropertyRelative( "gradient" );
			var prop_xCurve = prop.FindPropertyRelative( "xCurve" );
			var prop_yCurve = prop.FindPropertyRelative( "yCurve" );
			var prop_zCurve = prop.FindPropertyRelative( "zCurve" );
			var prop_wCurve = prop.FindPropertyRelative( "wCurve" );

			// インデントがついているのと、Horizontal時に表示がバグるので無効化する
			var lastIndentLv = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			drawCore(
				rect,
				lbl,
				SerializedPropertyUtil.getValue<ColorGrad>(prop),
				prop_mode,
				prop_grad,
				prop_xCurve,
				prop_yCurve,
				prop_zCurve,
				prop_wCurve
			);

			EditorGUI.indentLevel = lastIndentLv;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return 16f + 18f;
		}

		Material _mtl;


		void drawCore(
			Rect rect,
			GUIContent label,
			ColorGrad target,
			SerializedProperty prop_mode,
			SerializedProperty prop_grad,
			SerializedProperty prop_xCurve,
			SerializedProperty prop_yCurve,
			SerializedProperty prop_zCurve,
			SerializedProperty prop_wCurve
		) {
			var line0 = new Rect( rect.x,    rect.y+0,  rect.width,    16 );
			var line1 = new Rect( rect.x+40, rect.y+18, rect.width-40, 16 );

			EditorGUI.PropertyField(line0, prop_mode, label);


			var mode = (ColorGrad.Mode)prop_mode.enumValueIndex;
			switch ( mode ) {
			case ColorGrad.Mode.Gradient:
				EditorGUI.PropertyField(line1, prop_grad, GUIContent.none);
				break;

			case ColorGrad.Mode.RgbaCurve:
			case ColorGrad.Mode.HsvaCurve:
				{
					var rects = new Rect[6];
					for (int i=0; i<rects.Length; ++i) {
						var r = line1;
						if (i == 0) {
							r.width = (int)(line1.width / rects.Length);
						} else if (i == rects.Length-1) {
							r.x = rects[i-1].xMax + 1;
							r.xMax = line1.xMax;
						} else {
							r.x = rects[i-1].xMax + 1;
							r.width = (int)( (line1.xMax - r.x) / (rects.Length - i) );
						}
						rects[i] = r;
					}

					var names = mode==ColorGrad.Mode.RgbaCurve ? "RGBA" : "HSVA";

					// カーブプロパティを描画
					static void drawOneProp(Rect rect, string lbl, SerializedProperty prop) {
						EditorGUI.LabelField(rect, lbl);
						EditorGUI.indentLevel=1;
						EditorGUI.PropertyField(rect, prop, GUIContent.none);
						EditorGUI.indentLevel=0;
					}
					drawOneProp(rects[0], " "+names[0], prop_xCurve);
					drawOneProp(rects[1], " "+names[1], prop_yCurve);
					drawOneProp(rects[2], " "+names[2], prop_zCurve);
					drawOneProp(rects[3], " "+names[3], prop_wCurve);

					// グラデーションプレビューを表示
					var r45 = rects[4];
					r45.y += 1;
					r45.height -= 1;
					r45.x += 10;
					r45.xMax = rects[5].xMax;
					EditorGuiUtil.drawGradationPreview(
						r45,
						t => target.evaluate(t)
					);

				} break;

			default: Debug.LogError("invalid mode"); return;
			}
		}
	}
#endif

}