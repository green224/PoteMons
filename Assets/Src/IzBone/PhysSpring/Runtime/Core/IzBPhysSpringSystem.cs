//#define WITH_DEBUG
using UnityEngine.Jobs;
using Unity.Jobs;

using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Runtime.CompilerServices;



namespace IzBone.PhysSpring.Core {
using Common;
using Common.Field;

[UpdateInGroup(typeof(IzBoneSystemGroup))]
[UpdateAfter(typeof(IzBCollider.Core.IzBColliderSystem))]
[AlwaysUpdateSystem]
public sealed partial class IzBPhysSpringSystem
: PhysBone.Body.Core.BodySystemBase<RootAuthoring>
{

	/** 指定のRootAuthの物理状態をリセットする */
	override public void reset(
		Common.Entities8.EntityRegistererBase<RootAuthoring>.RegLink regLink
	) {
		var etp = _entityReg.etPacks;
		for (int i=0; i<regLink.etpIdxs.Count; ++i) {
			var etpIdx = regLink.etpIdxs[i];
			var e = etp.Entities[ etpIdx ];
			var t = etp.Transforms[ etpIdx ];

			SetComponent(e, new Ptcl_Velo());

			var defState = GetComponent<Ptcl_DefState>(e);
			var childWPos = t.localToWorldMatrix.MultiplyPoint(defState.childDefPos);

			var wPosCache = GetComponent<Ptcl_LastWPos>(e);
			wPosCache.value = childWPos;
			SetComponent(e, wPosCache);
		}
	}

	/** 現在のTransformをすべてのECSへ転送する処理 */
#if WITH_DEBUG
	struct MngTrans2ECSJob
#else
	[BurstCompile]
	struct MngTrans2ECSJob : IJobParallelForTransform
#endif
	{
		[ReadOnly] public NativeArray<Entity> entities;

		[NativeDisableParallelForRestriction]
		[WriteOnly] public ComponentDataFromEntity<CurTrans> curTranss;
		[NativeDisableParallelForRestriction]
		public ComponentDataFromEntity<Root> roots;

#if WITH_DEBUG
		public void Execute(int index, UnityEngine.Transform transform)
#else
		public void Execute(int index, TransformAccess transform)
#endif
		{
			var entity = entities[index];

			curTranss[entity] = new CurTrans{
				lPos = transform.localPosition,
				wPos = transform.position,
				lRot = transform.localRotation,
				lScl = transform.localScale,
			};
			if (roots.HasComponent(entity)) {
				// 最親の場合はL2Wも同期
				var a = roots[entity];
				a.l2w = transform.localToWorldMatrix;
				roots[entity] = a;
			}
		}
	}

	/** ECSで得た結果の回転を、マネージドTransformに反映させる処理 */
#if WITH_DEBUG
	struct SpringTransUpdateJob
#else
	[BurstCompile]
	struct SpringTransUpdateJob : IJobParallelForTransform
#endif
	{
		[ReadOnly] public NativeArray<Entity> entities;
		[ReadOnly] public ComponentDataFromEntity<CurTrans> curTranss;

#if WITH_DEBUG
		public void Execute(int index, UnityEngine.Transform transform)
#else
		public void Execute(int index, TransformAccess transform)
#endif
		{
			var entity = entities[index];
			var curTrans = curTranss[entity];

			// 適応
			transform.localPosition = curTrans.lPos;
			transform.localRotation = curTrans.lRot;
		}
	}


	protected override void OnCreate() {
		_entityReg = new EntityRegisterer();
	}

	protected override void OnDestroy() {
		_entityReg.Dispose();
	}

	override protected void OnUpdate() {
//		var deltaTime = World.GetOrCreateSystem<Time8.TimeSystem>().DeltaTime;
		var deltaTime = Time.DeltaTime;

		// 追加・削除されたAuthの情報をECSへ反映させる
		_entityReg.apply(EntityManager);

// TODO : 風・重力の処理は別システムへ一括して移す

		{// 風の影響を決定する処理。
			// TODO : これは今はWithoutBurstだが、後で何とかする
			Entities.ForEach((
				Entity entity,
				in Root_M2D rootM2D
			)=>{
				SetComponent(entity, new Root_Air{
					winSpd = rootM2D.auth.windSpeed,
					airDrag = rootM2D.auth.airDrag,
				});
			}).WithoutBurst().Run();
		}


		{// 重力加速度を決定する
			var defG = (float3)UnityEngine.Physics.gravity;
			Dependency = Entities.ForEach((ref Root_G g)=>{
				g.value = g.src.evaluate(defG);
			}).Schedule( Dependency );
		}


		// 空気抵抗関係の値を事前計算しておく
	#if WITH_DEBUG
		Entities.ForEach((
	#else
		Dependency = Entities.ForEach((
	#endif
			Entity entity,
			ref Root_Air air
		)=>{
			var airResRateIntegral =
				HalfLifeDragAttribute.evaluateIntegral(air.airDrag, deltaTime);

			air.winSpdIntegral = air.winSpd * (deltaTime - airResRateIntegral);
			air.airResRateIntegral = airResRateIntegral;
	#if WITH_DEBUG
		}).WithoutBurst().Run();
	#else
		}).Schedule( Dependency );
	#endif



		// 現在のTransformをすべてのECSへ転送
		var etp = _entityReg.etPacks;
		if (etp.Length != 0) {
		#if WITH_DEBUG
			var a = new MngTrans2ECSJob{
		#else
			Dependency = new MngTrans2ECSJob{
		#endif
				entities = etp.Entities,
				curTranss = GetComponentDataFromEntity<CurTrans>(false),
				roots = GetComponentDataFromEntity<Root>(false),
		#if WITH_DEBUG
			};
			for (int i=0; i<etp.Transforms.length; ++i) {
				a.Execute( i, etp.Transforms[i] );
			}
		#else
			}.Schedule( etp.Transforms, Dependency );
		#endif
		}


		// 子供のパーティクルまでの距離を計算する。
		// これはスケールが変わったときにも変わらず動作するためで、毎フレーム行う必要がある
	#if WITH_DEBUG
		Entities.ForEach((
	#else
		Dependency = Entities.ForEach((
	#endif
			Entity entity,
			in Ptcl_Child child
		)=>{
			var curTrans = GetComponent<CurTrans>(entity);
			var childTrans = GetComponent<CurTrans>(child.value);

			var toChildDist = length(childTrans.wPos - curTrans.wPos);
			SetComponent(entity, new Ptcl_ToChildWDist{value=toChildDist});
	#if WITH_DEBUG
		}).WithoutBurst().Run();
	#else
		}).Schedule(Dependency);
	#endif



		// デフォルト姿勢等を更新する処理。
		// 現在位置をデフォルト位置として再計算する必要がある場合は、
		// このタイミングで再計算を行う
	#if WITH_DEBUG
		Entities.ForEach((
	#else
		Dependency = Entities.ForEach((
	#endif
#if false
			Entity entity,
			ref DefaultState defState,
			in CurTrans curTrans,
			in Ptcl_Child child,
			in Ptcl_Root root
		)=>{
			if (!GetComponent<Root_WithAnimation>(root.value).value) return;

			var childTrans = GetComponent<CurTrans>(child.value);

			// 初期位置情報を更新
			defState.defRot = curTrans.lRot;
			defState.defPos = curTrans.lPos;
			defState.childDefPos = childTrans.lPos;
			defState.childDefPosMPR = mul(curTrans.lRot, curTrans.lScl * childTrans.lPos);
#else
			Entity entity,
			in Root_WithAnimation withAnimation
		)=>{
			if (!withAnimation.value) return;

			// WithAnimの場合のみ、Root以下の全Ptclに対して処理
			entity = GetComponent<Root_FirstPtcl>(entity).value;
			var curTrans = GetComponent<CurTrans>(entity);
			while (true) {
				if (!HasComponent<Ptcl_Child>(entity)) break;
				var childEntity = GetComponent<Ptcl_Child>(entity).value;
				var childTrans = GetComponent<CurTrans>(childEntity);

				// 初期位置情報を更新
				var defState = GetComponent<Ptcl_DefState>(entity);
				defState.defRot = curTrans.lRot;
				defState.defPos = curTrans.lPos;
				defState.childDefPos = childTrans.lPos;
				defState.childDefPosMPR = mul(curTrans.lRot, curTrans.lScl * childTrans.lPos);
				SetComponent(entity, defState);

				// ループを次に進める
				entity = childEntity;
				curTrans = childTrans;
			}
#endif
	#if WITH_DEBUG
		}).WithoutBurst().Run();
	#else
		}).Schedule(Dependency);
	#endif



		// 環境による位置更新
	#if WITH_DEBUG
		Entities.ForEach((
	#else
		Dependency = Entities.ForEach((
	#endif
			Entity entity,
			ref Ptcl_LastWPos lastWPos,
			in Ptcl_Child child,
			in Ptcl_Root root
		)=>{
			// 重力・空気関係の値
			var g = GetComponent<Root_G>(root.value).value;
			var air = GetComponent<Root_Air>(root.value);

			// 前フレームにキャッシュされた位置を重力・風の影響によりずらず
			lastWPos.value +=
				(g*deltaTime) * air.airResRateIntegral +
				air.winSpdIntegral;

			// コライダの衝突判定
			var collider = GetComponent<Root_ColliderPack>(root.value).value;
			if (collider != Entity.Null) {

				var r = GetComponent<Ptcl_R>(entity).value;
				r *= GetComponent<Ptcl_ToChildWDist>(entity).value;

				// 前フレームにキャッシュされた位置にパーティクルが移動したとして、
				// その位置でコライダとの衝突解決をしておく
				var isCol = false;
				var sp = new IzBCollider.RawCollider.Sphere{pos=lastWPos.value, r=r};
				unsafe { do {
					var bp = GetComponent<IzBCollider.Core.BodiesPack>(collider);

					float3 n=0; float d=0;
					for (
						var e = bp.firstSphere;
						e != Entity.Null;
						e = GetComponent<IzBCollider.Core.Body_Next>(e).value
					) {
						var rc = GetComponent<IzBCollider.Core.Body_Raw_Sphere>(e);
						if ( rc.value.solve(&sp,&n,&d) ) {
							isCol |= true;
							sp.pos += n * d;
						}
					}
					for (
						var e = bp.firstCapsule;
						e != Entity.Null;
						e = GetComponent<IzBCollider.Core.Body_Next>(e).value
					) {
						var rc = GetComponent<IzBCollider.Core.Body_Raw_Capsule>(e);
						if ( rc.value.solve(&sp,&n,&d) ) {
							isCol |= true;
							sp.pos += n * d;
						}
					}
					for (
						var e = bp.firstBox;
						e != Entity.Null;
						e = GetComponent<IzBCollider.Core.Body_Next>(e).value
					) {
						var rc = GetComponent<IzBCollider.Core.Body_Raw_Box>(e);
						if ( rc.value.solve(&sp,&n,&d) ) {
							isCol |= true;
							sp.pos += n * d;
						}
					}
					for (
						var e = bp.firstPlane;
						e != Entity.Null;
						e = GetComponent<IzBCollider.Core.Body_Next>(e).value
					) {
						var rc = GetComponent<IzBCollider.Core.Body_Raw_Plane>(e);
						if ( rc.value.solve(&sp,&n,&d) ) {
							isCol |= true;
							sp.pos += n * d;
						}
					}

					collider = bp.next;
				} while(collider != Entity.Null); }

				if (isCol) {lastWPos.value = sp.pos;}

			}
	#if WITH_DEBUG
		}).WithoutBurst().Run();
	#else
		}).Schedule(Dependency);
	#endif


		{// シミュレーションの本更新処理
		var vs = GetComponentDataFromEntity<Ptcl_Velo>();
		var lastWPoss = GetComponentDataFromEntity<Ptcl_LastWPos>();
		var curTranss = GetComponentDataFromEntity<CurTrans>();
	#if WITH_DEBUG
		Entities.ForEach((
	#else
		Dependency = Entities
		.WithNativeDisableParallelForRestriction(vs)
		.WithNativeDisableParallelForRestriction(lastWPoss)
		.WithNativeDisableParallelForRestriction(curTranss)
		.ForEach((
	#endif
			Entity entity,
			in Root root
		)=>{
			var iterationNum = root.iterationNum;
			var dt = deltaTime / iterationNum;

			// 一繋ぎ分のSpringの情報をまとめて取得しておく
			var buf_spring    = new NativeArray<Ptcl_Spring>(root.depth, Allocator.Temp);
			var buf_v         = new NativeArray<Ptcl_Velo>(root.depth, Allocator.Temp);
			var buf_defState  = new NativeArray<Ptcl_DefState>(root.depth, Allocator.Temp);
			var buf_lastWPos  = new NativeArray<Ptcl_LastWPos>(root.depth, Allocator.Temp);
			var buf_curTrans  = new NativeArray<CurTrans>(root.depth, Allocator.Temp);
			var buf_entity    = new NativeArray<Entity>(root.depth, Allocator.Temp);
			var buf_restoreRate = new NativeArray<float>(root.depth, Allocator.Temp);
			{
				var e = GetComponent<Root_FirstPtcl>(entity).value;
				for (int i=0;; ++i) {
					buf_entity[i]   = e;
					buf_spring[i] = GetComponent<Ptcl_Spring>(e);
					buf_v[i] = vs[e];
					buf_defState[i] = GetComponent<Ptcl_DefState>(e);
					buf_lastWPos[i] = lastWPoss[e];
					buf_curTrans[i] = curTranss[e];
					buf_restoreRate[i] = GetComponent<Ptcl_RestoreHL>(e).value.evaluate(dt);
					if (i == root.depth-1) break;
					e = GetComponent<Ptcl_Child>(e).value;
				}
			}

			// 本更新処理
			var rsRate = root.rsRate;
			var airResRateIntegral =
				HalfLifeDragAttribute.evaluateIntegral(
					GetComponent<Root_Air>(entity).airDrag,
					dt
				);
//			var collider = GetComponent<Root_ColliderPack>(entity).value;
			for (int itr=0; itr<iterationNum; ++itr) {
				var l2w = root.l2w;
				for (int i=0; i<root.depth; ++i) {

					// OneSpringごとのコンポーネントを取得
					var spring = buf_spring[i];
					var v = buf_v[i];
					var defState = buf_defState[i];
					var lastWPos = buf_lastWPos[i];

//					// 前フレームにキャッシュされた位置にパーティクルが移動したとして、
//					// その位置でコライダとの衝突解決をしておく
//					if (collider != Entity.Null) {
//						var r = GetComponent<Ptcl_R>(buf_entity[i]).value;
//						for (
//							var e = collider;
//							e != Entity.Null;
//							e = GetComponent<IzBCollider.Core.Body_Next>(e).value
//						) {
//							var rc = GetComponent<IzBCollider.Core.Body_RawCollider>(e);
//							var st = GetComponent<IzBCollider.Core.Body_ShapeType>(e).value;
//							rc.solveCollision( st, ref lastWPos.value, r );
//						}
//					}

					// 前フレームにキャッシュされた位置を先端目標位置として、
					// 先端目標位置へ移動した結果の移動・姿勢ベクトルを得る
					var r = length( defState.childDefPos );
					float3 sftVec, rotVec;
					{
						// ワールド座標をボーンローカル座標に変換する
//						var lastLPos = lastWPos.value - ppL2W.c3.xyz;
//						lastLPos = float3(
//							dot(lastLPos, ppL2W.c0.xyz),
//							dot(lastLPos, ppL2W.c1.xyz),
//							dot(lastLPos, ppL2W.c2.xyz)
//						);
//						var tgtBPos = lastLPos - defState.defPos;
						var w2l = inverse(l2w);
						var tgtBPos = mulMxPos(w2l, lastWPos.value) - defState.defPos;

						// 移動と回転の影響割合を考慮してシミュレーション結果を得る
						var cdpMPR = defState.childDefPosMPR;
						if ( rsRate < 0.001f ) {
							rotVec = getRotVecFromTgtBPos( tgtBPos, ref spring, cdpMPR );
							sftVec = Unity.Mathematics.float3.zero;
						} else if ( 0.999f < rsRate ) {
							rotVec = Unity.Mathematics.float3.zero;
							sftVec = spring.getPosRange(r).local2global(tgtBPos - cdpMPR);
						} else {
							rotVec = getRotVecFromTgtBPos( tgtBPos, ref spring, cdpMPR ) * (1f - rsRate);
							sftVec = spring.getPosRange(r).local2global((tgtBPos - cdpMPR) * rsRate);
						}
					}

					// バネ振動を更新
					if ( rsRate <= 0.999f ) {
						updateSpring(
							dt,
							ref rotVec, ref v.omg,
							spring.aglMax, spring.maxV,
							spring.springPow,
							airResRateIntegral,
							buf_restoreRate[i]
						);
					}
					if ( 0.001f <= rsRate ) {
						updateSpring(
							dt,
							ref sftVec, ref v.v,
							spring.posMax * r, spring.maxV * r,
							spring.springPow * r,
							airResRateIntegral,
							buf_restoreRate[i]
						);
					}

					// 現在の姿勢情報から、Transformに設定するための情報を構築
					quaternion rot;
					float3 trs;
					if ( rsRate <= 0.999f ) {
						var theta = length(rotVec);
						if ( theta < 0.001f ) {
							rot = defState.defRot;
						} else {
							var axis = rotVec / theta;
							var q = Unity.Mathematics.quaternion.AxisAngle(axis, theta);
							rot = mul(q, defState.defRot);
						}
					} else {
						rot = defState.defRot;
					}
					if ( 0.001f <= rsRate ) {
						trs = defState.defPos + sftVec;
					} else {
						trs = defState.defPos;
					}
					var result = buf_curTrans[i];
					result.lRot = rot;
					result.lPos = trs;


					// L2W行列をここで再計算する。
					// このL2WはTransformには直接反映されないので、2重計算になってしまうが、
					// 親から順番に処理を進めないといけないし、Transformへの値反映はここからは出来ないので
					// 仕方なくこうしている。
					{
						var rotMtx = new float3x3(rot);
						var scl = result.lScl;
						var l2p = float4x4(
							float4( rotMtx.c0*scl.x, 0 ),
							float4( rotMtx.c1*scl.y, 0 ),
							float4( rotMtx.c2*scl.z, 0 ),
							float4( trs, 1 )
						);
						l2w = mul(l2w, l2p);
					}

					// 現在のワールド位置を保存
					// これは正確なChildの現在位置ではなく、位置移動のみ考慮から外している。
					// 位置移動が入っている正確な現在位置で計算すると、位置Spring計算が正常に出来ないためである。
					lastWPos.value = mulMxPos(l2w, defState.childDefPos);

					// バッファを更新
					buf_v[i] = v;
					buf_curTrans[i] = result;
					buf_lastWPos[i] = lastWPos;
				}
			}


			// コンポーネントへ値を反映
			for (int i=0; i<root.depth; ++i) {
				var e = buf_entity[i];
				vs[e] = buf_v[i];
				curTranss[e] = buf_curTrans[i];
				lastWPoss[e] = buf_lastWPos[i];
			}
			buf_spring.Dispose();
			buf_v.Dispose();
			buf_defState.Dispose();
			buf_lastWPos.Dispose();
			buf_curTrans.Dispose();
			buf_entity.Dispose();
			buf_restoreRate.Dispose();

	#if WITH_DEBUG
		}).WithoutBurst().Run();
	#else
		}).ScheduleParallel(Dependency);
	#endif
		}



		// マネージド空間へ、結果を同期する
		// 本当はこれは並列にするまでもないが、
		// IJobParallelForTransformを使用しないとそもそもスレッド化できず、
		// そうなるとECSに乗せる事自体が瓦解するので、仕方なくこうしている
		if (etp.Length != 0) {
		#if WITH_DEBUG
			var a = new SpringTransUpdateJob{
		#else
			Dependency = new SpringTransUpdateJob{
		#endif
				entities = etp.Entities,
				curTranss = GetComponentDataFromEntity<CurTrans>(true),
		#if WITH_DEBUG
			};
			for (int i=0; i<etp.Transforms.length; ++i) {
				a.Execute( i, etp.Transforms[i] );
			}
		#else
			}.Schedule( etp.Transforms, Dependency );
		#endif
		}

	}

	/** 同次変換行列に位置を掛けて変換後の位置を得る処理 */
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static float3 mulMxPos(float4x4 mtx, float3 pos) => mul( mtx, float4(pos,1) ).xyz;

	// 先端目標位置へ移動した結果の姿勢ベクトルを得る処理
	static float3 getRotVecFromTgtBPos(
		float3 tgtBPos,
		ref Ptcl_Spring spring,
		float3 childDefPosMPR
	) {
		var childDefDir = normalize( childDefPosMPR );
		float3 ret;
		tgtBPos = normalize(tgtBPos);
		
		var crs = cross(childDefDir, tgtBPos);
		var crsNrm = length(crs);
		if ( crsNrm < 0.001f ) {
			ret = Unity.Mathematics.float3.zero;
		} else {
			var theta = acos( dot(childDefDir, tgtBPos) );
			theta = spring.getAglRange().local2global( theta );
			ret = crs * (theta/crsNrm);
		}

		return ret;
	}


	/** バネ更新処理 */
	static void updateSpring(
		float dt,
		ref float3 x, ref float3 v,	// 位置と速度
		float3 maxX, float3 maxV,	// 位置・速度最大値
		float kpm,					// バネ係数/質量
		float airResRateIntegral,	// 空気抵抗を積分したもの
		float restoreRate			// 強制復元力による戻し
	) {
		if (dt < 0.000001f) return;

		// バネ振動による加速度と空気抵抗から、新しい位置を算出
		var a = -x * kpm;
		var dX = (v + a/2*dt) * airResRateIntegral;

		// 新しい速度に直線的に遷移したと仮定して、速度を更新
		x += dX;
		v = dX/dt*2 - v;

		// 強制復元力による復元処理
		x *= restoreRate;

		// 範囲情報でクリッピング
		x = clamp(x, -maxX, maxX);
		v = clamp(v, -maxV, maxV);
	}

}
}
