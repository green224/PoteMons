using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using System.Collections.Generic;
using System.Linq;


namespace IzBone.PhysCloth.Authoring {

/**
 * IzBoneを使用するオブジェクトにつけるコンポーネント。
 * 平面的な布のようなものを再現する際に使用する
 */
[AddComponentMenu("IzBone/IzBone_PhysCloth_Plane")]
public unsafe sealed class PlaneAuthoring : SimpleAuthoring {
	// ------------------------------- inspectorに公開しているフィールド ------------------------

	[Space]
	[SerializeField] internal Transform[] _topOfBones = null;
	[SerializeField] internal bool _isLoopConnect = false;		// スカートなどの筒状のつながりにする

	[Space]
	[Compliance][SerializeField] float _cmpl_vert = 0.000000001f;		//!< Compliance値 上下方向接続
	[Compliance][SerializeField] float _cmpl_hori = 0.000000001f;		//!< Compliance値 左右方向接続
	[Compliance][SerializeField] float _cmpl_diag = 0.0000001f;			//!< Compliance値 対角方向接続


	// --------------------------------------- publicメンバ -------------------------------------

	// 対象のTopOfBonesを配列で得る
	public Transform[] TopOfBones => _topOfBones?.ToArray();


	// ----------------------------------- private/protected メンバ -------------------------------

	// 一番上のパーティクル
	ParticleMng _rootPtcl;

	// ボーンの最大深度
	int _jointDepth = -1;
	override internal int JointDepth {get{
		// 実行時にはキャッシュする
		if (Application.isPlaying && _jointDepth!=-1) return _jointDepth;

		_jointDepth = 0;
		foreach (var i in _topOfBones) {
			if (i==null) continue;
			int depth = 0;
			for (var t=i;; t=t.GetChild(0), ++depth) {
				if (t.childCount == 0) break;
			}
			_jointDepth = max(_jointDepth, depth);
		}
		return _jointDepth + 1;
	}}

	/** Particlesのバッファをビルドする処理 */
	override protected void buildParticles() {

		{// 質点リストを構築
			static ParticleMng genPtcl(
				int ptclIdx, Transform transHead, Transform transTail,
				ParticleMng parent, ParticleMng left
			) {
				ParticleMng ret = new ParticleMng(ptclIdx, transHead, new[]{transTail});
				if (parent != null) {
					ret.parent = parent;
					if (parent.child==null) parent.child = ret;
				}
				if (left != null) { ret.left = left; left.right = ret; }
				return ret;
			}

			var particles = new List<ParticleMng>();
			_rootPtcl = genPtcl(0, null, _topOfBones[0].parent, null, null);
			particles.Add( _rootPtcl );

			ParticleMng topPL = null;
			foreach ( var i in _topOfBones ) {

				var pL = topPL;
				var p = _rootPtcl;
				var trans = i;
				p = topPL = genPtcl( particles.Count, null, trans, p, pL );
				particles.Add( p );
				while (trans.childCount != 0) {
					trans = trans.GetChild( 0 );
					pL = pL?.child;
					p = genPtcl( particles.Count, trans.parent, trans, p, pL );
					particles.Add( p );
				}
			}

			_particles = particles.ToArray();
		}

		// ループ接続の場合は、末端の質点を接続する
		if (_isLoopConnect) {
			var pL = _rootPtcl.child;
			var pR = pL; while (pR.right != null) pR = pR.right;

			while (pL!=null && pR!=null) {
				pR.right = pL; pL.left = pR;
				pL = pL.child; pR = pR.child;
			}
		}
	}

	/** 全ボーンに対して、ConstraintMng羅列する処理 */
	override protected void processInAllConstraint(Action<float, ParticleMng, ParticleMng> proc) {
		for (int i=1; i<_particles.Length; ++i) {	//i=0はルートなので除外
			var p = _particles[i];

			// 下と右
			if (p.child != null) proc(_cmpl_vert, p, p.child);
			if (p.right != null) proc(_cmpl_hori, p, p.right);

			// 右斜め下
			if (p.right?.child != null)
				proc(_cmpl_diag, p, p.right?.child);
			else if (p.child?.right != null)
				proc(_cmpl_diag, p, p.child?.right);

			// 右斜め上
			if (p.right?.parent != null && p.right?.parent != _rootPtcl)
				proc(_cmpl_diag, p, p.right?.parent);
			else if (p.parent?.right != null && p.parent?.right != _rootPtcl)
				proc(_cmpl_diag, p, p.parent?.right);
		}
	}


	// --------------------------------------------------------------------------------------------
}

}

