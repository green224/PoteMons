#if false

using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using System.Collections.Generic;


namespace IzBone.PhysCloth.Controller {

	/**
	 * IzBoneを使用するオブジェクトにつけるコンポーネント。
	 * 平面的な布のようなものを再現する際に使用する
	 */
	[AddComponentMenu("IzBone/IzBone_PhysCloth_Rope")]
	public unsafe sealed class RopeAuthoring : Base {
		// ------------------------------- inspectorに公開しているフィールド ------------------------

		/** 骨の情報 */
		[Serializable] sealed class BoneInfo {
			public Transform boneTop = null;
			public int depth = 1;
			[SerializeField] AnimationCurve _m = null;
			[SerializeField] AnimationCurve _r = null;

			[NonSerialized] public Point point = null;

			public float getM(int idx) => max(0, _m.Evaluate( idx2rate(idx) ) );
			public float getR(int idx) => max(0, _r.Evaluate( idx2rate(idx) ) );

			float idx2rate(int idx) => depth==1 ? 0 : ((float)idx/(depth-1));
		}
		[SerializeField] BoneInfo _boneInfo = null;

		[Space]
		[Compliance][SerializeField] float _cmpl_direct = 0.000000001f;		//!< Compliance値 直接接続
		[Compliance][SerializeField] float _cmpl_bend = 0.00002f;			//!< Compliance値 曲げ用の１つ飛ばし接続
		[Compliance][SerializeField] float _cmpl_roll = 0.0000001f;			//!< Compliance値 捻じれ用の対角線接続


		// --------------------------------------- publicメンバ -------------------------------------

		// ----------------------------------- private/protected メンバ -------------------------------

		/** PointsとConstraintsをビルドする処理 */
		override protected void rebuildPointsAndConstrains() {

			// 質点リストを構築
			var points = new List<Point>();
			{
				Point p = _boneInfo.point = new Point(
					points.Count, _boneInfo.boneTop,
					_boneInfo.getM(0),
					_boneInfo.getR(0),
					60,
					100
				);
				points.Add(p);

				int k = 1;
				for (var j=p.trans; k<_boneInfo.depth; ++k) {
					j=j.GetChild(0);
					var newP = new Point(
						points.Count, j,
						_boneInfo.getM(k),
						_boneInfo.getR(k),
						60,
						100
					) {
						parent = p,
					};
					p.child = newP;
					p = newP;
					points.Add(p);
				}
			}
			_points = points.ToArray();

			// 制約リストを構築
			_constraints.Clear();
			{
				var c = _boneInfo.point;
				var d0 = c.child;
				var d1 = d0?.child;

				int depth=0;
				while (c!=null) {
					if (d0!=null) addCstr(_cmpl_direct, c, d0);
					if (d1!=null) addCstr(_cmpl_bend,   c, d1);

					c=c.child;
					d0=d0?.child;
					d1=d1?.child;
					++depth;
				}
			}
		}

//	#if UNITY_EDITOR
//		void OnValidate() {
//
//			if (_boneInfo == null) return;
//			{
//				// Depthを有効範囲に丸める
//				if ( _boneInfo.boneTop == null ) {
//					_boneInfo.depth = 0;
//				} else {
//					int depthMax = 1;
//					for (var j=_boneInfo.boneTop; j.childCount!=0; j=j.GetChild(0)) ++depthMax;
//
//					_boneInfo.depth = Mathf.Clamp(_boneInfo.depth, 1, depthMax);
//				}
//			}
//		}
//	#endif


		// --------------------------------------------------------------------------------------------
	}

}

#endif
