using System;
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysCloth.Authoring {
using Common;
using Common.Field;

/** IzBoneを使用するオブジェクトにつけるコンポーネントの基底クラス */
public unsafe abstract class BaseAuthoring
: PhysBone.Body.BaseAuthoringT<BaseAuthoring, Core.IzBPhysClothSystem>
{
	// ------------------------------- inspectorに公開しているフィールド ------------------------

	// --------------------------------------- publicメンバ -------------------------------------

	[Space]
	public bool useSimulation = true;				// 物理演算を行うか否か
	[Range(1,50)] public int iterationNum = 15;		// 1frame当たりの計算イテレーション回数

	[Space]
	public Gravity g = new Gravity(1);				// 重力加速度
	public float3 windSpeed = default;				// 風速
	[HalfLifeDrag] public HalfLife airDrag = 0.1f;	// 空気抵抗による半減期
	[Min(0)] public float maxSpeed = 100;			// 最大速度

	[Space]
	// アニメーション付きのボーンに対して使用するフラグ。毎フレームデフォルト位置を再キャッシュする。
	// リグを付けたスカートなどの場合もこれをONにして使用する。
	public bool withAnimation = false;


	// ----------------------------------- private/protected メンバ -------------------------------

	internal ConstraintMng[] _constraints;
	internal ParticleMng[] _particles;
	internal Entity _rootEntity = Entity.Null;


	override protected void OnEnable() {
		buildBuffers();
		rebuildParameters();

		base.OnEnable();
	}


	/** ParticlesとConstraintsのバッファをビルドする処理。派生先で実装すること */
	abstract protected void buildBuffers();

	/**
	 * パラメータを再構築する処理。
	 * この中でバウンダリー級の半径も計算する。
	 */
	override protected void rebuildParameters() {

		// バウンダリー球の半径を計算
		var w2l = transform.worldToLocalMatrix;
		var maxDist = 0f;
		foreach (var i in _particles) {
			foreach (var j in i.transTail)
				maxDist = max( maxDist, w2l.MultiplyPoint(j.position).magnitude );
		}
		BoundaryR = maxDist * 1.5f;		// 適当に1.5倍にする
	}


	// --------------------------------------------------------------------------------------------
}

}

