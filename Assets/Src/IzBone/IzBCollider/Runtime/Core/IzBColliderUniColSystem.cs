using UnityEngine.Jobs;
using Unity.Jobs;

using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;



namespace IzBone.IzBCollider.Core {
using Common;

[UpdateInGroup(typeof(IzBoneSystemGroup))]
[AlwaysUpdateSystem]
[UpdateBefore(typeof(IzBColliderSystem))]
public sealed partial class IzBColliderUniColSystem: SystemBase {



	// Authoringを登録・登録解除する処理
	internal void register(UniColCollectorAuthoring auth, UniColEntityRegisterer.RegLink regLink)
		=> _entityReg.register(auth, regLink);
	internal void unregister(UniColCollectorAuthoring auth, UniColEntityRegisterer.RegLink regLink)
		=> _entityReg.unregister(auth, regLink);


//	EntityCommandBufferSystem _ecbSystem;
	UniColEntityRegisterer _entityReg;


	protected override void OnCreate() {
		_entityReg = new UniColEntityRegisterer(128);

//		_ecbSystem = World.GetExistingSystem<BeginPresentationEntityCommandBufferSystem>();
	}

	protected override void OnDestroy() {
		_entityReg.Dispose();
	}

	override protected void OnUpdate() {

//		var ecb = _ecbSystem.CreateCommandBuffer();

		// 追加・削除されたAuthの情報をECSへ反映させる
		_entityReg.apply(EntityManager);


	}

}
}
