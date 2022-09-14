
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace IzBone.PhysSpring.Core {
	using Common;
	using Common.Field;
	using ICD = IComponentData;

	// 以降、1繋がりのSpringリスト１セットごとのEntityに対して付けるコンポーネント群

	// 1繋がりのSpringリストを管理するコンポーネント。
	// 再親のEntityについているわけではない事に注意
	public struct Root:ICD {
		public int depth;			// Springが何個連なっているか
		public int iterationNum;	// 繰り返し計算回数

		/**
		 * 回転と移動の影響割合。
		 * 0だと回転のみ。1だと移動のみ。その間はブレンドされる。
		 */
		public float rsRate;

		public float4x4 l2w;		// ボーン親の更に親のL2W
	}
	public struct Root_G:ICD {public Gravity src; public float3 value;}	// 重力加速度
	public struct Root_Air:ICD {			// 空気関連のパラメータ
		public float3 winSpd;					// 風速
		public HalfLife airDrag;				// 空気抵抗

		public float3 winSpdIntegral;			// 空気抵抗を考慮して積分した結果
		public float airResRateIntegral;		// 空気抵抗の積分結果
	}
	public struct Root_WithAnimation:ICD {public bool value;}	// 毎フレームデフォルト位置を再キャッシュするか否か
	public struct Root_FirstPtcl:ICD {public Entity value;}		// Springの開始位置のEntity。Ptclがついている
	public struct Root_ColliderPack:ICD {public Entity value;}	// 衝突検出を行う対象のコライダー



	// 以降、１ParticleごとのEntityに対して付けるコンポーネント群

	public struct Ptcl:ICD {}
	public struct Ptcl_Spring:ICD {
		public float aglMax; public float aglMargin;	// 範囲 - 回転
		public float posMax; public float posMargin;	// 範囲 - 移動
		public float springPow;							// バネ係数のスケール
		public float maxV;								// 最高速度のスケール

		public Math8.SmoothRange_Float getAglRange() {
			Math8.SmoothRange_Float ret = default;
			ret.reset(-aglMax, aglMax, aglMargin);
			return ret;
		}
		public Math8.SmoothRange_Float3 getPosRange(float toChildDist) {
			Math8.SmoothRange_Float3 ret = default;
			ret.reset(-posMax*toChildDist, posMax*toChildDist, posMargin*toChildDist);
			return ret;
		}
	}
	public struct Ptcl_Velo:ICD {public float3 v; public float3 omg;}	// 速度・角速度

	public struct Ptcl_DefState:ICD {	// 関節ごとのデフォルト位置姿勢情報
		public quaternion defRot;			// 親の初期姿勢
		public float3 defPos;				// 親の初期ローカル座標
		public float3 childDefPos;			// 子の初期ローカル座標
		public float3 childDefPosMPR;		// 子の初期ローカル座標に親の回転とスケールを掛けたもの。これはキャッシュすべきか悩みどころ…
	}

	// 子の位置までのワールド座標での距離。これをもとにいくつかのパラメータが自動スケールされる
	public struct Ptcl_ToChildWDist:ICD {public float value;}

	public struct Ptcl_LastWPos:ICD {public float3 value;}		// 前フレームでのワールド位置のキャッシュ
	public struct Ptcl_Root:ICD {public Entity value;}			// RootのEntity
	public struct Ptcl_Child:ICD {public Entity value;}			// 子供側のEntity
	public struct Ptcl_R:ICD {public float value;}				// 衝突判定用の半径
	public struct Ptcl_InvM:ICD {public float value;}			// 質量の逆数
	public struct Ptcl_RestoreHL:ICD {public HalfLife value;}	// Default位置への復元半減期




	// シミュレーション毎のTransformの現在値の取得と、
	// シミュレーション結果をフィードバックする際に使用されるTransform情報。
	// これは一番末端PtclやRootにも付くが、そいつは参照用のみに使用される。（フィードバックもされてしまうが）
	public struct CurTrans:ICD {
		public float3 lPos, wPos;
		public quaternion lRot;
		public float3 lScl;
	}




	// ECSとAuthoringとの橋渡し役を行うためのマネージドコンポーネント
	public sealed class Ptcl_M2D:ICD {
		public RootAuthoring.Bone boneAuth;			// 生成元
		public Transform parentTrans, childTrans;	// Springでつながる親と子のTransform
		public float depthRate;						// 0~Depthを0～1にリマップしたもの
	}
	public sealed class Root_M2D:ICD {
		public RootAuthoring.Bone auth;			// 生成元
	}
}
