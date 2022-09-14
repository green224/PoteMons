
using UnityEngine;
using Unity.Entities;
using UnityEngine.Jobs;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysCloth.Core {
	using Common;
	using Common.Field;
	using ICD = IComponentData;

	// 以降、PhysCloth１セットごとのEntityに対して付けるコンポーネント群

	public struct Root:ICD {}
	public struct Root_UseSimulation:ICD {public bool value;}	// 物理演算を行うか否か
	public struct Root_G:ICD {public Gravity src; public float3 value;}	// 重力加速度
	public struct Root_Air:ICD {			// 空気関連のパラメータ
		public float3 winSpd;					// 風速
		public HalfLife airDrag;				// 空気抵抗

		public float3 winSpdIntegral;			// 空気抵抗を考慮して積分した結果
		public float airResRateIntegral;		// 空気抵抗の積分結果
	}
	public struct Root_MaxSpd:ICD {public float value;}			// 最大速度
	public struct Root_WithAnimation:ICD {public bool value;}	// 毎フレームデフォルト位置を再キャッシュするか否か
	public struct Root_ColliderPack:ICD {public Entity value;}	// 衝突検出を行う対象のコライダー



	// 以降、１ParticleごとのEntityに対して付けるコンポーネント群

	public struct Ptcl:ICD {}

	// PhysCloth１セット内の、次のPaticle・親のParticle・RootのParticleへの参照。
	public struct Ptcl_Next:ICD {public Entity value;}
	public struct Ptcl_Parent:ICD {public Entity value;}
	public struct Ptcl_Root:ICD {public Entity value;}

	// シミュレーションが行われなかった際のL2W,L2P。
	// L2Wはシミュレーション対象外のボーンのアニメーションなどを反映して毎フレーム更新する
	public struct Ptcl_DefaultHeadL2W:ICD {public float4x4 value;}
	public struct Ptcl_DefaultHeadL2P:ICD {
		public float4x4 l2p;
		public quaternion rot;
		public Ptcl_DefaultHeadL2P(Transform trans) {
			if (trans == null) {
				rot = Unity.Mathematics.quaternion.identity;
				l2p = Unity.Mathematics.float4x4.identity;
			} else {
				rot = trans.localRotation;
				l2p = Unity.Mathematics.float4x4.TRS(
					trans.localPosition,
					trans.localRotation,
					trans.localScale
				);
			}
		}
		public Ptcl_DefaultHeadL2P(TransformAccess trans) {
			rot = trans.localRotation;
			l2p = Unity.Mathematics.float4x4.TRS(
				trans.localPosition,
				trans.localRotation,
				trans.localScale
			);
		}
	}
	public struct Ptcl_DefaultTailLPos:ICD {public float3 value;}
	public struct Ptcl_DefaultTailWPos:ICD {public float3 value;}

	// 子の位置までのワールド座標での距離。これをもとにいくつかのパラメータが自動スケールされる
	public struct Ptcl_ToChildWDist:ICD {public float value;}

	// 位置・半径・速度・質量の逆数
	public struct Ptcl_WPos:ICD {public float3 value;}
	public struct Ptcl_R:ICD {public float value;}
	public struct Ptcl_Velo:ICD {public float3 value;}
	public struct Ptcl_InvM:ICD {
		readonly public float value;
		public const float MinimumM = 0.00000001f;
		public Ptcl_InvM(float m) { value = m < MinimumM ? 0 : (1f/m); }
	}

	// 現在の姿勢値。デフォルト姿勢からの差分値。ワールド座標で計算する
	public struct Ptcl_DWRot:ICD {public quaternion value;}

	// 最大差分角度(ラジアン)
	public struct Ptcl_MaxAngle:ICD {public float value;}
	// 角度変位の拘束条件へのコンプライアンス値
	public struct Ptcl_AngleCompliance:ICD {public float value;}

	// Default位置への復元半減期
	public struct Ptcl_RestoreHL:ICD {public HalfLife value;}
	// デフォルト位置からの移動可能距離
	public struct Ptcl_MaxMovableRange:ICD {public float value;}

	// λ
	public struct Ptcl_CldCstLmd:ICD {public float value;}
	public struct Ptcl_AglLmtLmd:ICD {public float value;}
	public struct Ptcl_MvblRngLmd:ICD {public float value;}

	// シミュレーション結果をフィードバックする用のTransform情報
	public struct Ptcl_CurHeadTrans:ICD {
		public float4x4 l2w;
		public float4x4 w2l;
		public float3 lPos;
		public quaternion lRot;
	}
	



	// 以降、１DistanceConstraintごとのEntityに対して付けるコンポーネント群

	public struct DistCstr:ICD {}
	public struct Cstr_Target:ICD {public Entity src, dst;}		// 処理対象のParticle
	public struct Cstr_Compliance:ICD {public float value;}		// コンプライアンス値
	public struct Cstr_DefaultLen:ICD {public float value;}		// 初期長さ
	public struct Cstr_Lmd:ICD {public float value;}			// λ



	// ECSとAuthoringとの橋渡し役を行うためのマネージドコンポーネント
	public sealed class Root_M2D:ICD {
		public Authoring.BaseAuthoring auth;			//!< 生成元
	}
	public sealed class Ptcl_M2D:ICD {
		public Authoring.ParticleMng auth;				//!< 生成元
	}
	public sealed class Cstr_M2D:ICD {
		public Authoring.ConstraintMng auth;			//!< 生成元
	}

}
