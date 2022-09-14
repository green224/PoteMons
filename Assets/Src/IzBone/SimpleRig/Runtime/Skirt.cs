using System;
using UnityEngine;
using Unity.Entities;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Collections;

using System.Collections.Generic;
using System.Linq;


namespace IzBone.SimpleRig {
using Common;
using Common.Field;

using RangeSC = Common.Field.SimpleCurveRangeAttribute;
using SC = Common.Field.SimpleCurve;

/**
 * スカート用のシンプルなRIG
 */
[AddComponentMenu("IzBone/SimpleRig/Skirt")]
[ExecuteAlways]
[DefaultExecutionOrder(-100)]
//[UnityEngine.Animations.NotKeyable]
//[DisallowMultipleComponent]
public sealed class Skirt : MonoBehaviour {
	// --------------------------- インスペクタに公開しているフィールド -----------------------------

	// 対象とするスカートのボーン
	[SerializeField] Transform[] _skirtTopOfBones = null;

	// ふともものボーン
	[SerializeField] Transform[] _legBones = null;

	// 角度による影響の範囲
	[Vec2Range(0,1)]
	[SerializeField] float2 _effectAngleRange = float2(0.1f, 0.39f);

	// 距離による影響の範囲
	[Vec2Range(0,1)]
	[SerializeField] internal float2 _effectPosRange = float2(0.31f, 0.965f);

	// 振り上げによる影響の範囲
	[Vec2Range(0,1)]
	[SerializeField] internal float2 _effectRaiseRange = float2(0.19f, 0.41f);


	// ------------------------------------- public メンバ ----------------------------------------

	/**
	 * 初期化処理。現在の姿勢を初期姿勢としてリセットする。
	 * Startから自動的に呼ばれるが、手動で呼ぶこともできる。
	 */
	public void setup()
	{
		// ボーンの親がちゃんとみな一致しているかどうかのチェック
		var tgtTOBs = _skirtTopOfBones?.Where(i => i!=null)?.ToArray();
		if (tgtTOBs!=null && tgtTOBs.Length!=0) {
			var a = tgtTOBs[0].parent;
			foreach ( var i in tgtTOBs ) {
				if (i.parent == a) continue;
				Debug.LogError("TopOfBones requires that the parent transforms be the same.");
				return;
			}
		}

		// スカートのボーン部分のデータを構築
		if (tgtTOBs!=null && tgtTOBs.Length!=0) {

			// まず対象のTransformツリーを末端まで取得
			static Transform[] getTransTree(Transform self) {
				var ret = new List<Transform>();
				for (var i=self;; i=i.GetChild(0)) {
					ret.Add(i);
					if (i.childCount == 0) break;
				}
				return ret.ToArray();
			}
			var fullTransTrees = _skirtTopOfBones.Select( i => getTransTree(i) ).ToArray();

			// ボーンデータを生成
			_boneDatas = fullTransTrees.Select( i=>new BoneData(i) ).ToArray();
		}

		// ふともものボーン部分のデータを構築
		if (_legBones!=null) {
			_legDatas = _legBones
				.Select(i => new LegData(i))
				.ToArray();
		}

		// ルートのボーンデータを初期化
		if (_boneDatas != null && _boneDatas.Length != 0) {
			_rootData = new RootData(_boneDatas[0].transTree[0].trans.parent);
		}

		// 初期姿勢でのボーンの根本から先端までの方向、を記録しておく
		if (_boneDatas != null && _boneDatas.Length != 0) {
			float3 p0 = default;
			float3 p1 = default;
			foreach (var i in _boneDatas) {
				p0 += (float3)i.transTree[0].trans.position;
				p1 += (float3)i.transTree.Last().trans.position;
			}
			_initSkirtWDir = normalize(p1 - p0);
		}

		// LegからSkirtBoneへの距離の最大値のキャッシュ、を記録しておく
		if (_boneDatas != null && _boneDatas.Length != 0) {
			_maxPosDistCache = 0;
			var rootTrans = _boneDatas[0].transTree[0].trans.parent;
			var rootW2L = rootTrans==null ? Matrix4x4.identity : rootTrans.worldToLocalMatrix;
			foreach (var i in _boneDatas) {
				for (int j=0; j<_legDatas.Length; ++j) {
					var d = _legDatas[j].trans.position - i.transTree[0].trans.position;
					d = rootW2L.MultiplyVector(d);
				
					_maxPosDistCache = max(_maxPosDistCache, length(d));
				}
			}
		}
	}


	// --------------------------------- private / protected メンバ -------------------------------

	/** スカートのボーンデータ */
	internal sealed class BoneData {
		public struct InitParam {
			readonly public Transform trans;

			public float4x4 l2p;
			public float3 posL;
			public quaternion rotL;
			public float3 sclL;

			public InitParam(Transform trans) {
				this.trans = trans;
				posL = trans.localPosition;
				rotL = trans.localRotation;
				sclL = trans.localScale;
				l2p = Unity.Mathematics.float4x4.TRS(posL, rotL, sclL);
			}
		}
		public InitParam[] transTree;
//		public float[] weight;			// 各ふともものウェイト値

		public BoneData(Transform[] transTree) {
			this.transTree = transTree.Select(i => new InitParam(i)).ToArray();

			var rootTrans = transTree[0].parent;
			var rootW2L = rootTrans==null ? Matrix4x4.identity : rootTrans.worldToLocalMatrix;
		}
	}

	/** ふともものボーンデータ */
	internal sealed class LegData {
		readonly public Transform trans;

		public float4x4 initW2L;
		public float4x4 curL2W;
		public float4x4 init2curW;

		public float3 curSkirtWDir;

		public LegData(Transform trans) {
			this.trans = trans;
			initW2L = trans.worldToLocalMatrix;
			init2curW = mul(curL2W, initW2L);
		}
		public void update() {
			curL2W = trans.localToWorldMatrix;
			init2curW = mul(curL2W, initW2L);
		}
	}

	/** ルートのボーンデータ */
	internal sealed class RootData {
		readonly public Transform trans;

		readonly public float4x4 initW2L;
		public float4x4 curL2W;
		public float4x4 curW2L;
		public float4x4 init2curW;

		public RootData(Transform trans) {
			this.trans = trans;
			initW2L = trans?.worldToLocalMatrix ?? Matrix4x4.identity;
		}
		public void update() {
			curL2W = trans?.localToWorldMatrix ?? Matrix4x4.identity;
			curW2L = trans?.worldToLocalMatrix ?? Matrix4x4.identity;
			init2curW = mul(curL2W, initW2L);
		}
	}

	internal BoneData[] _boneDatas;
	internal LegData[] _legDatas;
	internal RootData _rootData;
	internal float3 _initSkirtWDir;		// 初期姿勢でのボーンの根本から先端までの方向

	internal float _maxPosDistCache;	// LegからSkirtBoneへの距離の最大値のキャッシュ


	/** ボーンのL2Wを更新する */
	void refreshL2W() {
		if (_rootData != null)
			_rootData.update();
		if (_legDatas != null)
			foreach (var i in _legDatas) i.update();
	}

	/** 影響を反映する処理 */
	void update() {
		if (_boneDatas == null || _legDatas == null) return;

		// 何もふとももを動かしていないときの、ボーン方向
		var initBoneDir = Math8.transVector(
			_rootData.init2curW,
			_initSkirtWDir
		);
		initBoneDir = normalize(initBoneDir);
		var initBoneDirL = Math8.transVector( _rootData.curW2L, initBoneDir );

		// ふとももに完全に同期したときの、ボーン方向
		foreach (var i in _legDatas) {
			var a = Math8.transVector(
				i.init2curW,
				_initSkirtWDir
			);
			i.curSkirtWDir = normalize(a);
		}


		foreach (var i in _boneDatas) {
			var transTree = i.transTree;

			var pB0 = Math8.trans(
				_rootData.curL2W,
				transTree[0].l2p.c3.xyz
			);

			// スカートの方向にどのくらい傾いているかの影響度一覧を計算する
			var powRates = new NativeArray<float>(_legDatas.Length, Allocator.Temp);
			float ttlPowRates = 0.0000001f;
			for (int j=0; j<_legDatas.Length; ++j) {
				var ld = _legDatas[j];

				// 何もふとももを動かしていないときのボーン方向をY軸として、各方向の軸を計算
				var pLeg = ld.curL2W.c3.xyz;
				var l2b = pB0 - pLeg;
				var y = initBoneDir;
				var z = normalizesafe( cross(l2b, y) );
				var x = cross(y, z);

				// スカートの方向にどのくらい傾いているかで、影響度を計算する
				var xR = dot(x, ld.curSkirtWDir);
				var zR = dot(z, ld.curSkirtWDir);
				var powRate = saturate(remap(
					(1 - _effectAngleRange.x) * PI,
					(1 - _effectAngleRange.y) * PI,
					0, 1,
					abs( atan2( zR, xR ) )
				));
				
				// スカートとふとももの距離によって影響度を計算する
				var l2b_l = Math8.transVector( _rootData.curW2L, l2b );
				var dist = length(l2b_l);
				dist = lerp(
					dist,
					dist * abs(zR) / ( sqrt(xR*xR + zR*zR) + 0.00001f ),
					smoothstep( _effectRaiseRange.x, _effectRaiseRange.y, xR )
				);
				powRate *= smoothstep(
					_maxPosDistCache * _effectPosRange.y,
					_maxPosDistCache * _effectPosRange.x,
					dist
				);

				// 影響度を記録
				powRates[j] = powRate;
				ttlPowRates += powRate;
			}

			// スカートの傾きを確定
			float3 dDir = default;
			for (int j=0; j<_legDatas.Length; ++j) {
				var ld = _legDatas[j];

				var powRate = powRates[j] / max(1,ttlPowRates);

//				var fixedWeight = i.weight[j] *
//					( _legDatas.Length * powRate );
//				powRate *= fixedWeight;

				// 傾きを記録
				dDir += (ld.curSkirtWDir -initBoneDir) * powRate;
			}
			powRates.Dispose();

			// 回転量を確定
			var rot = Math8.fromToRotation(
				initBoneDirL,
				Math8.transVector( _rootData.curW2L, initBoneDir + dDir )
			);
			transTree[0].trans.localRotation = mul(rot, transTree[0].rotL);
			transTree[0].trans.localPosition = transTree[0].posL;
			transTree[0].trans.localScale = transTree[0].sclL;

			// 根本以外のボーンも初期姿勢にリセットする
			for (int j=1; j<transTree.Length; ++j) {
				transTree[j].trans.localRotation = transTree[j].rotL;
				transTree[j].trans.localPosition = transTree[j].posL;
				transTree[j].trans.localScale = transTree[j].sclL;
			}
		}
	}

	void LateUpdate() {
#if UNITY_EDITOR
		if (__need2Setup) {__need2Setup=false; setup();}
#endif
		refreshL2W();

		if (Application.isPlaying) update();
	}

	void Start() {
		setup();
	}


	// --------------------------------------------------------------------------------------------
#if UNITY_EDITOR
	bool __need2Setup = false;
	void OnValidate() {
		if (!Application.isPlaying) __need2Setup = true;
	}

	// 同一GameObjectについているPlaneから、TopOfBonesを自動設定する
	[ContextMenu("Auto-Setup from PhysCloth")]
	void autoSetupFromPhysCloth() {
		var cloth = GetComponent<PhysCloth.Authoring.PlaneAuthoring>();
		if (cloth != null) {
			_skirtTopOfBones = cloth.TopOfBones;
			__need2Setup = true;
		}
	}
#endif
}
}
