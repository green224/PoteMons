using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using System.Collections.Generic;


namespace IzBone.PhysCloth.Authoring {
using Common;
using Common.Field;

using RangeSC = Common.Field.SimpleCurveRangeAttribute;
using SC = Common.Field.SimpleCurve;

/** IzBoneを使用するオブジェクトにつけるコンポーネントのシンプルな実装となる基底クラス */
public unsafe abstract class SimpleAuthoring : BaseAuthoring {
	// ------------------------------- inspectorに公開しているフィールド ------------------------

	[Space]
	[UnityEngine.Serialization.FormerlySerializedAs("_r")]
	[SerializeField][RangeSC(0,1)] SC _radius = 1;				// パーティクルの半径
	[UnityEngine.Serialization.FormerlySerializedAs("_m")]
	[SerializeField][RangeSC(0,10)] SC _mass = 1;				// パーティクルの重さ
	[SerializeField][RangeSC(0,180)] SC _maxAngle = 60;			// 最大曲げ角度
	[SerializeField][RangeSC(0,1)] SC _aglRestorePow = 0;		// 曲げ角度の復元力
	[SerializeField][RangeSC(0,1)] SC _restorePow = 0;			// 初期位置への強制戻し力
	[SerializeField][RangeSC(0,10)] SC _maxMovableRange = 0;	// 移動可能距離。0で制限なし


	// --------------------------------------- publicメンバ -------------------------------------
	// ----------------------------------- private/protected メンバ -------------------------------

	// ジョイントの最大深度と固定深度。
	// getM系の物理パラメータ取得時に使用される。
	// パラメータ取得のみに使用されるので。厳密である必要はない。
	abstract internal int JointDepth {get;}
	virtual internal int JointDepthFixCnt => 1;


	/** ParticlesとConstraintsのバッファをビルドする処理 */
	override protected void buildBuffers() {
		buildParticles();
		buildConstraints();
	}

	/** Particlesのバッファをビルドする処理。派生先で実装すること */
	abstract protected void buildParticles();

	/** Constraintsをビルドする処理。必要であれば派生先で変更可能 */
	virtual protected void buildConstraints() {
		// 制約リストを構築
		var constraints = new List<ConstraintMng>();
		processInAllConstraint(
			(compliance, p0, p1) => {
				if (1 <= compliance) return;
				constraints.Add( new ConstraintMng() {
					mode = ConstraintMng.Mode.Distance,
					srcPtclIdx = p0.idx,
					dstPtclIdx = p1.idx,
					param = length(p0.getTailWPos() - p1.getTailWPos()),
				} );
			}
		);
		_constraints = constraints.ToArray();
	}

	/** ParticlesとConstraintsのパラメータを再構築する処理。必要であれば派生先で変更可能 */
	override protected void rebuildParameters() {
		// 質点パラメータを構築
		foreach (var i in _particles) {
			// 一番上に二つのFixedJointがある想定なので、Depthは-1から始める
			int depth = -1;
			for (var p=i.parent; p!=null; p=p.parent) ++depth;

			if (depth == -1) i.setParams(0,0,0,0,0,0);
			else i.setParams(
				getMass( depth ),
				getRadius( depth ),
				getMaxAgl( depth ),
				getAglCompliance( depth ),
				getRestoreHL( depth ),
				getMaxMovableRange( depth )
			);
		}

		{// 制約パラメータを構築
			int i = -1;
			processInAllConstraint(
				(compliance, p0, p1) => {
					if (1 <= compliance) return;
					var c = _constraints[++i];
					c.compliance = compliance;
				}
			);
		}

		base.rebuildParameters();
	}

	/** 全ボーンに対して、ConstraintMng羅列する処理。派生先で実装すること */
	abstract protected void processInAllConstraint(Action<float, ParticleMng, ParticleMng> proc);


	// ジョイント位置の各種物理パラメータを得る処理
	internal float getMass(int idx) => idx<JointDepthFixCnt ? 0 : _mass.evaluate( idx2rate(idx) );
	internal float getRadius(int idx) => _radius.evaluate( idx2rate(idx) );
	internal float getMaxAgl(int idx) => _maxAngle.evaluate( idx2rate(idx) );
	internal float getAglCompliance(int idx) =>
		AngleComplianceAttribute.showValue2Compliance( _aglRestorePow.evaluate( idx2rate(idx) ) );
	internal float getRestoreHL(int idx) =>
		HalfLifeDragAttribute.showValue2HalfLife( _restorePow.evaluate( idx2rate(idx) ) );
	internal float getMaxMovableRange(int idx) => _maxMovableRange.evaluate( idx2rate(idx) );

	float idx2rate(int idx) =>
		max(0, (idx - JointDepthFixCnt) / (JointDepth - JointDepthFixCnt - 1f) );


	// --------------------------------------------------------------------------------------------
}

}

