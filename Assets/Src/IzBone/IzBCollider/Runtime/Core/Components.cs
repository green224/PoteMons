
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace IzBone.IzBCollider.Core {



	/** 複数のコライダーにアクセスするためのルート要素 */
	public struct BodiesPack : IComponentData {
		// 各種コライダーの最初の要素
		public Entity firstSphere;
		public Entity firstCapsule;
		public Entity firstBox;
		public Entity firstPlane;

		public Entity next;
	}



	// 以降、１コライダーごとのEntityに対して付けるコンポーネント群
	public struct Body:IComponentData {}
	public struct Body_Next:IComponentData {public Entity value;}	// 次のコライダーへの参照
	public struct Body_Center:IComponentData {public float3 value;}
	public struct Body_R:IComponentData {public float3 value;}
	public struct Body_Rot:IComponentData {public quaternion value;}
	public struct Body_CurL2W:IComponentData {public float4x4 value;}
	public struct Body_UniCol:IComponentData {}		// UnityCollider用のコライダーEntityに対して付けるコンポーネント

	public struct Body_Raw_Sphere:IComponentData {public RawCollider.Sphere value;}
	public struct Body_Raw_Capsule:IComponentData {public RawCollider.Capsule value;}
	public struct Body_Raw_Box:IComponentData {public RawCollider.Box value;}
	public struct Body_Raw_Plane:IComponentData {public RawCollider.Plane value;}



	// BodyとBodyAuthoringとの橋渡し役を行うためのマネージドコンポーネント
	public sealed class Body_M2D:IComponentData {
		public BodyAuthoring bodyAuth;				//!< 生成元
	}
	public sealed class BodiesPack_UniCol_M2D:IComponentData {
		public UniColCollectorAuthoring auth;		//!< 生成元
	}

}
