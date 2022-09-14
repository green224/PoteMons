using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace IzBone.Common.Field {


	/**
	 * 簡単なカーブと定数のどちらかを指定できるインスペクタ公開用フィールド型。
	 * t=0~1のカーブを定義する。
	 * 
	 * Burst領域にもそのまま持っていける。
	 */
	[Serializable]
	public struct SimpleCurve
	{
		public float start, end, curve;
		public bool useCurve;

		static public implicit operator SimpleCurve(float val) => new SimpleCurve(val);

		public SimpleCurve(float value) {
			start = end = value;
			curve = 0.5f;
			useCurve = false;
		}

		/** 指定時刻(0~1)の値を得る */
		public float evaluate(float t) {
			if (!useCurve) return start;

			float rate;
			if (curve < 0.5f) {
				var p = 1 / max(curve*2, 0.00001f);
				rate = 1 - pow(clamp(1-t,0,1), p);
			} else {
				var p = 1 / max(2 - curve*2, 0.00001f);
				rate = pow(clamp(t,0,1), p);
			}

			return lerp(start, end, rate);
		}
	}

	/** SimpleCurve用の範囲Attribute */
	internal sealed class SimpleCurveRangeAttribute : PropertyAttribute {
		public float? Min => _useMin ? _minValue : (float?)null;
		public float? Max => _useMax ? _maxValue : (float?)null;

		public SimpleCurveRangeAttribute(float min, float max) {
			_useMin = true; _minValue = min;
			_useMax = true; _maxValue = max;
		}
		public SimpleCurveRangeAttribute(float min) {
			_useMin = true; _minValue = min;
		}
		public SimpleCurveRangeAttribute(bool useMin, bool useMax, float min, float max) {
			_useMin = useMin; _minValue = min;
			_useMax = useMax; _maxValue = max;
		}

#if UNITY_EDITOR
		// 指定の値を属性の設定値でClampする
		static public float clamp(SimpleCurveRangeAttribute attr, float value) {
			if (attr == null) return value;
			if (attr.Min.HasValue) value = max(attr.Min.Value, value);
			if (attr.Max.HasValue) value = min(attr.Max.Value, value);
			return value;
		}

		// 指定のプロパティを属性の設定値にしたがってプロパティ描画する
		static public void drawGUI(
			SimpleCurveRangeAttribute attr,
			Rect pos,
			SerializedProperty prop,
			GUIContent label,
			bool forceFloatView = false,			// 属性によらず、つねにFloatFieldで描画するか否か
			SerializedProperty[] dstProps = null	// 結果を書き込む先のPropertyリスト。nullの場合はそのままpropに書き込む
		) {
			if (attr == null) {
				EditorGUI.PropertyField(pos, prop, label);
			} else {
				using (var cc = new EditorGUI.ChangeCheckScope()) {

					float val;
					if (attr.Min.HasValue && attr.Max.HasValue && !forceFloatView) {
						val = EditorGUI.Slider(
							pos, label,
							prop.floatValue,
							attr.Min.Value, attr.Max.Value
						);
					} else {
						val = EditorGUI.FloatField(
							pos, label,
							prop.floatValue
						);
					}

					if (cc.changed) {
						val = clamp(attr, val);
						if (dstProps != null) {
							foreach (var i in dstProps) i.floatValue = val;
						} else {
							prop.floatValue = val;
						}
					}
				}
			}
		}
#endif

		readonly float _minValue, _maxValue;
		readonly bool _useMin, _useMax;
	}


}
