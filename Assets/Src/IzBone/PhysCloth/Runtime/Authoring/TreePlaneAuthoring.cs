using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using System.Collections.Generic;
using System.Linq;
using Unity.Collections;


namespace IzBone.PhysCloth.Authoring {

/**
 * IzBoneを使用するオブジェクトにつけるコンポーネント。
 * ツリー構造のように、根本から接続が複数に広がっていく形状に対して使用する。
 */
[AddComponentMenu("IzBone/IzBone_PhysCloth_TreePlane")]
public unsafe sealed class TreePlaneAuthoring : SimpleAuthoring {
	// ------------------------------- inspectorに公開しているフィールド ------------------------

	[Space]
	[SerializeField] internal Transform[] _topOfBones = null;

	[Space]
	[Compliance][SerializeField] float _cmpl_vert = 0.000000001f;	//!< Compliance値 上下方向接続
	[Compliance][SerializeField] float _cmpl_hori = 0.000000001f;	//!< Compliance値 左右方向接続


	// --------------------------------------- publicメンバ -------------------------------------


	// ----------------------------------- private/protected メンバ -------------------------------

	// 一番上のパーティクル
//	ParticleMng[] _rootPtcls;

	// ボーンの最大深度
	int _jointDepth = -1;
	override internal int JointDepth {get{
		// 実行時にはキャッシュする
		if (Application.isPlaying && _jointDepth!=-1) return _jointDepth;

		_jointDepth = 0;
		var cIdxs = new NativeList<int>(64, Allocator.Temp);
		foreach (var i in _topOfBones) {
			if (i==null) continue;
			int depth = 0;
			int cIdx = 0;
			var t=i;
			while (true) {
				if (t.childCount <= cIdx) {
					if (cIdxs.Length == 0) break;
					cIdx = cIdxs[cIdxs.Length-1] + 1;
					cIdxs.RemoveAt(cIdxs.Length - 1);
					t = t.parent;
					--depth;
				} else {
					cIdxs.Add(cIdx);
					t = t.GetChild(cIdx);
					++depth;
					_jointDepth = max(_jointDepth, depth);
				}
			}
		}
		cIdxs.Dispose();

		return _jointDepth;
	}}

	/** Particlesのバッファをビルドする処理 */
	override protected void buildParticles() {

		// Particleを一つ生成する処理
		static ParticleMng genPtcl(
			int ptclIdx, Transform transHead, Transform[] transTail,
			ParticleMng parent
		) {
			ParticleMng ret = new ParticleMng(ptclIdx, transHead, transTail);
			if (parent != null) {
				ret.parent = parent;
				if (parent.child==null) parent.child = ret;
			}
			return ret;
		}

		// 全TopOfBoneについて、パーティクルを生成する。
		// TopOfBoneごとのツリーは互いに干渉しない前提なので、それぞれごとにまとめて生成する
		var particles = new List<ParticleMng>();
//		_rootPtcls = new ParticleMng[_topOfBones.Length];
		var topPtcls = new ParticleMng[_topOfBones.Length];
		for (int topIdx=0; topIdx<_topOfBones.Length; ++topIdx) {

			// TopOfBoneで指定されたTransformの一つ上のTransformをルートParticleとして生成する
			var p = genPtcl(particles.Count, null, new[]{_topOfBones[topIdx].parent}, null);
//			_rootPtcls[topIdx] = p;
			particles.Add( p );

			// TopOfBoneで指定されたTransformに対応するParticleを生成する
			var pLst0 = new List<ParticleMng>();
			var pLst1 = new List<ParticleMng>();
			p = genPtcl(particles.Count, null, new[]{_topOfBones[topIdx]}, p);
			topPtcls[topIdx] = p;
			pLst0.Add( p );
			particles.Add( p );

			// それより下のTransformに対応するParticleを、ツリー状にたどりながら生成していく。
			// 生成処理はTreeの同一深度ごとに連続して生成されるように処理を進める。
			// これにより、Particles内に入る並び順が子から親に絶対に逆流しないようになる。
			while (true) {

				// パーティクル生成対象の深度を一段階進める。
				foreach (var k in pLst0)
				foreach (var i in k.transTail) {
					if (i.childCount == 0) continue;
					var tails = new Transform[i.childCount];
					for (int j=0; j<tails.Length; ++j) tails[j] = i.GetChild(j);
					var a = genPtcl(-1, i, tails.ToArray(), k);
					pLst1.Add(a);
				}
				if (pLst1.Count == 0) break;

				// ここでいい感じにLRがつながるように並び替える
				{
					// 指定の点群から、特徴方向を得る。
					// 本当は最小二乗法を用いるべきだが、難しいので適当な方法で近似する
					static float3 getSpDir(List<ParticleMng> pLst) {
						float3 ctr = 0;
						foreach (var i in pLst) ctr = i.transHead.position;
						ctr /= pLst.Count;

						float maxDist = 0;
						ParticleMng retPtcl = pLst[0];
						foreach (var i in pLst) {
							var dist = lengthsq((float3)i.transHead.position - ctr);
							if ( maxDist < dist ) {retPtcl = i; maxDist = dist;}
						}

						return (float3)retPtcl.transHead.position - ctr;
					}

					// パーティクルの方向を得る。ここで、TopOfBoneが複数ある場合は、
					// TopOfBoneの進む方向に向かってLeft→Rightになるようにもしておく
					var spDir = getSpDir(pLst1);
					{
						Transform p0, p1;
						p0 = 0==topIdx ? _topOfBones[topIdx] : _topOfBones[topIdx-1];
						p1 = topIdx==_topOfBones.Length-1 ? _topOfBones[topIdx] : _topOfBones[topIdx+1];
						float3 dir = p1.position - p0.position;

						if ( dot(spDir, dir) < 0 ) spDir = -spDir;
					}

					pLst1 = pLst1.OrderBy( a => dot(a.transHead.position, spDir) ).ToList();
				}

				// 左右を接続する
				pLst1[0].parent.child = pLst1[0];
				for (int i=1; i<pLst1.Count; ++i) {
					pLst1[i].left = pLst1[i-1];
					pLst1[i-1].right = pLst1[i];
				}

				// パーティクル一覧に登録
				foreach (var i in pLst1) {
					i.idx = particles.Count;
					particles.Add(i);
				}

				// 作業用バッファをバックバッファとスワップ
				(pLst0,pLst1) = (pLst1,pLst0);
				pLst1.Clear();
			}
		}

		// 作成されたTopOfBoneごとのツリーについて、それらを接続する
		for (int i=1; i<topPtcls.Length; ++i) {

			var pL = topPtcls[i-1];
			var pR = topPtcls[i];

			for (; pL.child!=null && pR.child!=null; ) {
				pL = pL.child;
				pR = pR.child;
				for (; pL.right!=null;) pL=pL.right;
				pL.right = pR;
				pR.left = pL;
			}
		}


		_particles = particles.ToArray();
	}

	/** 全ボーンに対して、ConstraintMng羅列する処理 */
	override protected void processInAllConstraint(Action<float, ParticleMng, ParticleMng> proc) {
		for (int i=1; i<_particles.Length; ++i) {	//i=0はルートなので除外
			var p = _particles[i];

			// 上と右
			if (p.parent != null) proc(_cmpl_vert, p, p.parent);
			if (p.right != null) proc(_cmpl_hori, p, p.right);
		}
	}


	// --------------------------------------------------------------------------------------------
}

}

