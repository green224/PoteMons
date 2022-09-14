
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Collections;


namespace IzBone.PhysCloth.Core {
	using Common.Entities8;

	/** BodyAuthのデータをもとに、Entityを生成するモジュール */
	public sealed class EntityRegisterer : EntityRegistererBase<Authoring.BaseAuthoring>
	{
		// ------------------------------------- public メンバ ----------------------------------------

		public EntityRegisterer() : base(128) {}


		// --------------------------------- private / protected メンバ -------------------------------

		/** Auth1つ分の変換処理 */
		override protected void convertOne(
			Authoring.BaseAuthoring auth,
			RegLink regLink,
			EntityManager em
		) {

			// ParticleのEntity一覧をまず作成
			var ptclEntities = new NativeArray<Entity>( auth._particles.Length, Allocator.Temp );
			for (int i=0; i<auth._particles.Length; ++i) {
				var entity = em.CreateEntity();
				ptclEntities[i] = entity;
			}

			// ParticleのEntityの中身を作成
			for (int i=0; i<auth._particles.Length; ++i) {
				var mp = auth._particles[i];
				var entity = ptclEntities[i];

				// コンポーネントを割り当て
				em.AddComponentData(entity, new Ptcl());
				var nextEnt = i==ptclEntities.Length-1 ? Entity.Null : ptclEntities[i+1];
				em.AddComponentData(entity, new Ptcl_Next{value = nextEnt});
				var parentEnt = mp.parent==null ? Entity.Null : ptclEntities[mp.parent.idx];
				em.AddComponentData(entity, new Ptcl_Parent{value = parentEnt});
				em.AddComponentData(entity, new Ptcl_Root{value = ptclEntities[0]});
				em.AddComponentData(entity, new Ptcl_DefaultHeadL2W());
				em.AddComponentData(entity, new Ptcl_DefaultHeadL2P(mp.transHead));
				em.AddComponentData(entity, new Ptcl_DefaultTailLPos{value = mp.defaultTailLPos});
				em.AddComponentData(entity, new Ptcl_DefaultTailWPos());
				em.AddComponentData(entity, new Ptcl_ToChildWDist());
				em.AddComponentData(entity, new Ptcl_CurHeadTrans());
				em.AddComponentData(entity, new Ptcl_WPos{value = mp.getTailWPos()});
				em.AddComponentData(entity, new Ptcl_InvM(mp.m));
				em.AddComponentData(entity, new Ptcl_R{value = mp.radius});
				em.AddComponentData(entity, new Ptcl_Velo());
				em.AddComponentData(entity, new Ptcl_DWRot());
				em.AddComponentData(entity, new Ptcl_MaxAngle{value = radians(mp.maxAngle)});
				em.AddComponentData(entity, new Ptcl_AngleCompliance{value = mp.angleCompliance});
				em.AddComponentData(entity, new Ptcl_RestoreHL{value = mp.restoreHL});
				em.AddComponentData(entity, new Ptcl_MaxMovableRange{value = mp.maxMovableRange});
				em.AddComponentData(entity, new Ptcl_CldCstLmd());
				em.AddComponentData(entity, new Ptcl_AglLmtLmd());
				em.AddComponentData(entity, new Ptcl_MvblRngLmd());
				em.AddComponentData(entity, new Ptcl_M2D{auth = mp});

				// Entity・Transformを登録
				addEntityCore(entity, regLink);
				var transHead = mp.transHead == null ? mp.transTail[0].parent : mp.transHead;
				addETPCore(entity, transHead, regLink);
			}

			// ConstraintのEntityを作成
			for (int i=0; i<auth._constraints.Length; ++i) {
				var mc = auth._constraints[i];

				// ここで生成しておかないと、パラメータの再設定ができなくなるので、
				// Editor中は常に生成しておく
			#if !UNITY_EDITOR
				// 強度があまりにも弱い場合は拘束条件を追加しない
				if (Authoring.ComplianceAttribute.LEFT_VAL*0.98f < mc.compliance) continue;
			#endif

				Entity entity = Entity.Null;
				switch (mc.mode) {
				case Authoring.ConstraintMng.Mode.Distance:
					{// 距離拘束

						// 対象パーティクルがどちらも固定されている場合は無効
						var srcMP = auth._particles[mc.srcPtclIdx];
						var dstMP = auth._particles[mc.dstPtclIdx];
						if (srcMP.m < Ptcl_InvM.MinimumM && dstMP.m < Ptcl_InvM.MinimumM) break;

						// DefaultLenをHeadToTailの割合で計算する
						var defLen = mc.param.x / srcMP.headToTailWDist;
						
						// コンポーネントを割り当て
						entity = em.CreateEntity();
						em.AddComponentData(entity, new DistCstr());
						var srcEnt = ptclEntities[mc.srcPtclIdx];
						var dstEnt = ptclEntities[mc.dstPtclIdx];
						
						em.AddComponentData(entity, new Cstr_Target{src=srcEnt, dst=dstEnt});
						em.AddComponentData(entity, new Cstr_Compliance{value = mc.compliance});
						em.AddComponentData(entity, new Cstr_DefaultLen{value = defLen});
						em.AddComponentData(entity, new Cstr_Lmd());

					} break;
				case Authoring.ConstraintMng.Mode.MaxDistance:
					{// 最大距離拘束
// 未対応
//						var b = new MaxDistance{
//							compliance = i.compliance,
//							src = i.param.xyz,
//							tgt = pntsPtr + i.srcPtclIdx,
//							maxLen = i.param.w,
//						};
//						if ( b.isValid() ) md.Add( b );
					} break;
				case Authoring.ConstraintMng.Mode.Axis:
					{// 稼働軸拘束
// 未対応
//						var b = new Axis{
//							compliance = i.compliance,
//							src = pntsPtr + i.srcPtclIdx,
//							dst = pntsPtr + i.dstPtclIdx,
//							axis = i.param.xyz,
//						};
//						if ( b.isValid() ) a.Add( b );
					} break;
				default:throw new System.InvalidProgramException();
				}
				if (entity == Entity.Null) continue;
				em.AddComponentData(entity, new Cstr_M2D{auth = mc});

				// Entity・Transformを登録
				addEntityCore(entity, regLink);
			}



			{// OneClothをECSへ変換
				var rootEntity = ptclEntities[0];
				em.AddComponentData(rootEntity, new Root());
				em.AddComponentData(rootEntity, new Root_UseSimulation{value = auth.useSimulation});
				em.AddComponentData(rootEntity, new Root_G{src = auth.g});
				em.AddComponentData(rootEntity, new Root_Air());
				em.AddComponentData(rootEntity, new Root_MaxSpd{value = auth.maxSpeed});
				em.AddComponentData(rootEntity, new Root_WithAnimation{value = auth.withAnimation});
				em.AddComponentData(rootEntity, new Root_ColliderPack());
				em.AddComponentData(rootEntity, new Root_M2D{auth = auth});
				auth._rootEntity = rootEntity;

				// ColliderPackを設定する処理。これは遅延して実行される可能性もある
				if (auth.Collider != null) {
					auth.Collider.getRootEntity( em, (em, cpEnt)=>
						em.SetComponentData(
							rootEntity,
							new Root_ColliderPack{value=cpEnt}
						)
					);
				}
			}


			ptclEntities.Dispose();
		}

		/** 指定Entityの再変換処理 */
		override protected void reconvertOne(Entity entity, EntityManager em) {
			if (em.HasComponent<Ptcl_M2D>(entity)) {

				var mp = em.GetComponentData<Ptcl_M2D>(entity).auth;
				em.SetComponentData(entity, new Ptcl_InvM(mp.m));
				em.SetComponentData(entity, new Ptcl_R{value = mp.radius});
				em.SetComponentData(entity, new Ptcl_MaxAngle{value = radians(mp.maxAngle)});
				em.SetComponentData(entity, new Ptcl_AngleCompliance{value = mp.angleCompliance});
				em.SetComponentData(entity, new Ptcl_RestoreHL{value = mp.restoreHL});
				em.SetComponentData(entity, new Ptcl_MaxMovableRange{value = mp.maxMovableRange});

				if (em.HasComponent<Root_M2D>(entity)) {
					var mr = em.GetComponentData<Root_M2D>(entity).auth;
					em.SetComponentData(entity, new Root_UseSimulation{value = mr.useSimulation});
					em.AddComponentData(entity, new Root_G{src = mr.g});
//					em.SetComponentData(entity, new Root_Air());
					em.SetComponentData(entity, new Root_MaxSpd{value = mr.maxSpeed});
					em.SetComponentData(entity, new Root_WithAnimation{value = mr.withAnimation});
				}

			} else if (em.HasComponent<Cstr_M2D>(entity)) {

				var mc = em.GetComponentData<Cstr_M2D>(entity).auth;
				// とりあえず今は全部DistanceConstraintとして処理
				em.SetComponentData(entity, new Cstr_Compliance{value = mc.compliance});
			}
		}
		

		// --------------------------------------------------------------------------------------------
	}
}