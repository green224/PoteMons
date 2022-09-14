
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysSpring.Core {
	using Common.Entities8;
	using Common.Field;

	/** RootAuthのデータをもとに、Entityを生成するモジュール */
	public sealed class EntityRegisterer : EntityRegistererBase<RootAuthoring>
	{
		// ------------------------------------- public メンバ ----------------------------------------

		public EntityRegisterer() : base(128) {}


		// --------------------------------- private / protected メンバ -------------------------------

		/** Auth1つ分の変換処理 */
		override protected void convertOne(
			RootAuthoring auth,
			RegLink regLink,
			EntityManager em
		) {

			foreach (var bone in auth._bones)
			foreach (var j in bone.targets) {

				// 最も末端のTransformを得る
				var child = j.getEndOfBone(bone.depth);

				// 最も末端のTransformに対応するEntityを生成
				var childEntity = em.CreateEntity();
				{
					em.AddComponentData(childEntity, new CurTrans());
					addEntityCore(childEntity, regLink);
					addETPCore(childEntity, child, regLink);
				}

				// Particleを生成
				var rootEntity = em.CreateEntity();
				for (int i=0; i<bone.depth; ++i) {
					var parent = child.parent;

					// 無効なDepth値が指定されていた場合はエラーを出す
					if (parent == null) {
						UnityEngine.Debug.LogError("PhySpring:depth is too higher");
						continue;
					}

//					var dRate = (float)(bone.depth-1-i) / max(bone.depth-1, 1);
					var dRate = remap(0, bone.depth-1, 1, 0, i);

					// デフォルト姿勢を適応
					if (i == bone.depth-1) {
						if (!j.defaultRot.Equals(default))		// defaultRotが未初期化の場合があるので、そうでないかチェックする
							parent.localRotation *= j.defaultRot;
					}

					// コンポーネントを割り当て
					var entity = em.CreateEntity();
					em.AddComponentData(entity, new Ptcl());
					em.AddComponentData( entity, genOneSpring(bone, dRate) );
					em.AddComponentData(entity, new Ptcl_Velo());
					em.AddComponentData(entity, new Ptcl_DefState{
						defRot = parent.localRotation,
						defPos = parent.localPosition,
						childDefPos = child.localPosition,
						childDefPosMPR = mul(
							parent.localRotation,
							parent.localScale * (float3)child.localPosition
						),
					});
					em.AddComponentData(entity, new Ptcl_ToChildWDist());
					em.AddComponentData(entity, new Ptcl_LastWPos{value=child.position});
					em.AddComponentData(entity, new Ptcl_R{value=bone.radius.evaluate(dRate)});
					em.AddComponentData(entity, new Ptcl_InvM{value=1/bone.mass.evaluate(dRate)});
					em.AddComponentData(entity, new Ptcl_RestoreHL{
						value = HalfLifeDragAttribute.showValue2HalfLife( bone.restorePow.evaluate(dRate) )
					});
					em.AddComponentData(entity, new Ptcl_Root{value = rootEntity});
					em.AddComponentData(entity, new Ptcl_Child{value = childEntity});
					em.AddComponentData(entity, new CurTrans{});
					em.AddComponentData(entity, new Ptcl_M2D{
						boneAuth = bone,
						depthRate = dRate,
						parentTrans = parent,
						childTrans = child,
					});

					// ParticleのEntity・Transformを登録
					addEntityCore(entity, regLink);
					addETPCore(entity, parent, regLink);

					child = parent;
					childEntity = entity;
				}

				// Particleをまとめる最上位のコンポーネントを生成。
				// これは最上位の親のTransformと関連付ける必要があるため、別Entityで登録
				{
					em.AddComponentData(rootEntity, new Root{
						depth = bone.depth,
						iterationNum = bone.iterationNum,
						rsRate = bone.rotShiftRate,
					});
					em.AddComponentData(rootEntity, new Root_G{src=bone.g});
					em.AddComponentData(rootEntity, new Root_Air{
						winSpd = bone.windSpeed,
						airDrag = bone.airDrag,
					});
					em.AddComponentData(rootEntity, new Root_WithAnimation{value=bone.withAnimation});
					em.AddComponentData(rootEntity, new Root_FirstPtcl{value=childEntity});
					em.AddComponentData(rootEntity, new Root_ColliderPack{value=Entity.Null});
					em.AddComponentData(rootEntity, new CurTrans());
					em.AddComponentData(rootEntity, new Root_M2D{auth=bone});
					addEntityCore(rootEntity, regLink);
					addETPCore(rootEntity, child.parent, regLink);

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
			}
		}

		/** 指定Entityの再変換処理 */
		override protected void reconvertOne(Entity entity, EntityManager em) {
			// 出来るものだけ同期を行う
			if (em.HasComponent<Ptcl_M2D>(entity)) {
				var m2d = em.GetComponentData<Ptcl_M2D>(entity);
				em.SetComponentData(entity, genOneSpring(m2d.boneAuth, m2d.depthRate));
				em.SetComponentData(entity, new Ptcl_R{value=m2d.boneAuth.radius.evaluate(m2d.depthRate)});
				em.SetComponentData(entity, new Ptcl_RestoreHL{
					value = HalfLifeDragAttribute.showValue2HalfLife(
						m2d.boneAuth.restorePow.evaluate(m2d.depthRate)
					)
				});
			}

			if (em.HasComponent<Root>(entity)) {
				var m2d = em.GetComponentData<Root_M2D>(entity);
				var root = em.GetComponentData<Root>(entity);
				var auth = m2d.auth;
				root.iterationNum = auth.iterationNum;
				root.rsRate = auth.rotShiftRate;
				em.SetComponentData(entity, root);
				em.SetComponentData(entity, new Root_G{src=auth.g});
//				em.SetComponentData(entity, new Root_Air{
//					winSpd = auth.windSpeed,
//					airDrag = auth.airDrag,
//				});
				em.SetComponentData(entity, new Root_WithAnimation{value=auth.withAnimation});
			}
		}

		static Ptcl_Spring genOneSpring( RootAuthoring.Bone bone, float dRate ) {
			var rotMax = radians( bone.angleMax.evaluate(dRate) );
			var rotMargin = rotMax * bone.angleMargin.evaluate(dRate);
			var shiftMax = bone.shiftMax.evaluate(dRate);
			var shiftMargin = shiftMax * bone.shiftMargin.evaluate(dRate);

			return new Ptcl_Spring{
				aglMax = rotMax,
				aglMargin = rotMargin,
				posMax = shiftMax,
				posMargin = shiftMargin,
				springPow = lerp(
					0,
					1000,
					pow( bone.springPow.evaluate(dRate), 2f )
				),
				maxV = pow(
					10,
					lerp( 0, 100, bone.maxV.evaluate(dRate) )
				),
			};
		}


		// --------------------------------------------------------------------------------------------
	}
}