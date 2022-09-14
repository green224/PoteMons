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
public sealed partial class IzBColliderSystem : SystemBase {



	// BodyAuthoringを登録・登録解除する処理
	internal void register(BodiesPackAuthoring auth, EntityRegisterer.RegLink regLink)
		=> _entityReg.register(auth, regLink);
	internal void unregister(BodiesPackAuthoring auth, EntityRegisterer.RegLink regLink)
		=> _entityReg.unregister(auth, regLink);
	internal void resetParameters(EntityRegisterer.RegLink regLink)
		=> _entityReg.resetParameters(regLink);
	EntityRegisterer _entityReg;



	// マネージドTransformからECSへデータを反映させる処理
	[BurstCompile] struct MngTrans2ECSJob : IJobParallelForTransform
	{
		[ReadOnly] public NativeArray<Entity> entities;
		[ReadOnly] public ComponentDataFromEntity<Body_Center> centers;
		[ReadOnly] public ComponentDataFromEntity<Body_Rot> rots;

		[NativeDisableParallelForRestriction]
		[WriteOnly] public ComponentDataFromEntity<Body_CurL2W> l2ws;

		public void Execute(int index, TransformAccess transform)
		{
			var entity = entities[index];

			var center = centers[entity].value;
			var rot = rots[entity].value;

			// L2Wを計算
			float4x4 l2w;
			if ( rot.value.Equals(float4(0,0,0,0)) ) {
				l2w = Unity.Mathematics.float4x4.identity;
				l2w.c3.xyz = center;
			} else {
				l2w = float4x4(rot, center);
			}
			l2w = mul(transform.localToWorldMatrix, l2w);

			// コンポーネントへデータを格納
			l2ws[entity] = new Body_CurL2W{value = l2w};
		}
	}




	protected override void OnCreate() {
		_entityReg = new EntityRegisterer();
	}

	protected override void OnDestroy() {
		_entityReg.Dispose();
	}

	override protected void OnUpdate() {

		// 追加・削除されたAuthの情報をECSへ反映させる
		_entityReg.apply(EntityManager);

		// マネージドTransformから、ECSへL2Wを同期
		var etp = _entityReg.etPacks;
		if (etp.Length != 0) {
			Dependency = new MngTrans2ECSJob{
				entities = etp.Entities,
				centers = GetComponentDataFromEntity<Body_Center>(true),
				rots = GetComponentDataFromEntity<Body_Rot>(true),
				l2ws = GetComponentDataFromEntity<Body_CurL2W>(false),
			}.Schedule( etp.Transforms, Dependency );
		}


		{// 各種コライダー形状ごとに、現在位置を更新
			var dLst = new NativeArray<JobHandle>((int)ShapeType.MAX_COUNT, Allocator.Temp);

			// Sphere
			dLst[0] = Entities.ForEach((
				ref Body_Raw_Sphere rc,
				in Body_CurL2W l2w,
				in Body_R r
			)=>{
				rc.value = new RawCollider.Sphere() {
					pos = l2w.value.c3.xyz,
					r = length( l2w.value.c0.xyz ) * r.value.x,
				};
			}).ScheduleParallel(Dependency);

			// Capsule
			dLst[1] = Entities.ForEach((
				ref Body_Raw_Capsule rc,
				in Body_CurL2W l2w,
				in Body_R r
			)=>{
				var sclX = length( l2w.value.c0.xyz );
				var sclY = length( l2w.value.c1.xyz );
				rc.value = new RawCollider.Capsule() {
					pos = l2w.value.c3.xyz,
					r_s = sclX * r.value.x,
					r_h = sclY * r.value.y,
					dir = l2w.value.c1.xyz / sclY,
				};
			}).ScheduleParallel(Dependency);

			// Box
			dLst[2] = Entities.ForEach((
				ref Body_Raw_Box rc,
				in Body_CurL2W l2w,
				in Body_R r
			)=>{
				var sclX = length( l2w.value.c0.xyz );
				var sclY = length( l2w.value.c1.xyz );
				var sclZ = length( l2w.value.c2.xyz );
				rc.value = new RawCollider.Box() {
					pos = l2w.value.c3.xyz,
					xAxis = l2w.value.c0.xyz / sclX,
					yAxis = l2w.value.c1.xyz / sclY,
					zAxis = l2w.value.c2.xyz / sclZ,
					r = r.value * float3(sclX, sclY, sclZ),
				};
			}).ScheduleParallel(Dependency);

			// Plane
			dLst[3] = Entities.ForEach((
				ref Body_Raw_Plane rc,
				in Body_CurL2W l2w,
				in Body_R r
			)=>{
				rc.value = new RawCollider.Plane() {
					pos = l2w.value.c3.xyz,
					dir = l2w.value.c2.xyz / length( l2w.value.c2.xyz ),
				};
			}).ScheduleParallel(Dependency);

			// 全ての処理の完了を待つ
			Dependency = JobHandle.CombineDependencies(dLst);
			dLst.Dispose();
		}
	}

}
}
