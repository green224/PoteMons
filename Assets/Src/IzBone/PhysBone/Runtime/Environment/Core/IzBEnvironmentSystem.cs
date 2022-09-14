using UnityEngine.Jobs;
using Unity.Jobs;

using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;



namespace IzBone.PhysBone.Environment.Core {
using Common;

[UpdateInGroup(typeof(IzBoneSystemGroup))]
[AlwaysUpdateSystem]
public sealed partial class IzBEnvironmentSystem : SystemBase {



	// Authoringを登録・登録解除する処理
	internal void register(LocalSettingAuthoring auth, EntityRegisterer.RegLink regLink)
		=> _entityReg.register(auth, regLink);
	internal void unregister(LocalSettingAuthoring auth, EntityRegisterer.RegLink regLink)
		=> _entityReg.unregister(auth, regLink);
	internal void resetParameters(EntityRegisterer.RegLink regLink)
		=> _entityReg.resetParameters(regLink);
	EntityRegisterer _entityReg;



	protected override void OnCreate() {
		_entityReg = new EntityRegisterer();
	}

	protected override void OnDestroy() {
		_entityReg.Dispose();
	}

	override protected void OnUpdate() {

		// 追加・削除されたAuthの情報をECSへ反映させる
		_entityReg.apply(EntityManager);

	}

}
}
