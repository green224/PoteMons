using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysCloth.Authoring {
	using Common;
	using Common.Field;
	
	public sealed class ParticleMng {
		public int idx;

		// パーティクルの元となるボーンの根本（Head）と先端（Tail）のTransform。
		// 根本は一つだが、先端は複数ある場合がある。
		// Particleは先端平均位置に配置され、根本のTranformの回転・位置にフィードバックされる。
		public readonly Transform transHead;
		public readonly Transform[] transTail;

		public readonly float3 defaultTailLPos;	// 初期先端ローカル位置。これは変更しない
		public readonly float headToTailWDist;	// HeadからTailまでのワールド座標距離。これは変化するが、一旦登録したら変更しない

		public ParticleMng parent, child, left, right;	// 上下左右のパーティクル。childは一番最初の子供
		public float m;
		public float radius;
		public float maxAngle;
		public float angleCompliance;
		public HalfLife restoreHL;		// 初期位置への復元半減期
		public float maxMovableRange;	// デフォルト位置からの移動可能距離

	#if UNITY_EDITOR
		// デバッグ表示用のバッファ
		internal float3 DEBUG_curPos;
		internal float3 DEBUG_curV;
	#endif


		public ParticleMng(int idx, Transform transHead, Transform[] transTail) {
			this.idx = idx;
			this.transHead = transHead;
			this.transTail = transTail;
			defaultTailLPos = default;
			foreach (var i in transTail) defaultTailLPos += (float3)i.localPosition;
			defaultTailLPos /= transTail.Length;
			headToTailWDist = transTail[0].parent.localToWorldMatrix
				.MultiplyVector(defaultTailLPos).magnitude;
		}

		// Headのみを指定して生成する
		static public ParticleMng generateByTransHead(int idx, Transform transHead) {
			if (transHead.childCount == 0) throw new ArgumentException("The transHead must have at least one child.");
			var transTail = new Transform[ transHead.childCount ];
			for (int i=0; i<transHead.childCount; ++i) transTail[i] = transHead.GetChild(i);

			return new ParticleMng( idx, transHead, transTail );
		}

		// Tailのみを指定して生成する
		static public ParticleMng generateByTransTail(int idx, Transform transTail) {
			return new ParticleMng( idx, null, new[]{transTail} );
		}

		public void setParams(
			float m,
			float radius,
			float maxAngle,
			float angleCompliance,
			HalfLife restoreHL,
			float maxMovableRange
		) {
			this.m = m;
			this.radius = radius;
			this.angleCompliance = angleCompliance;
			this.maxAngle = maxAngle;
			this.restoreHL = restoreHL;
			this.maxMovableRange = maxMovableRange;
		}

		public float3 getTailWPos() {
			if (transHead == null) {
				Unity.Assertions.Assert.IsTrue(transTail.Length == 1);
				return transTail[0].position;
			}
			return transHead.localToWorldMatrix.MultiplyPoint( defaultTailLPos );
		}
	}


}
