
using System;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Collections.Generic;


namespace IzBone.IzBCollider.Core {
	using Common.Entities8;

	/** UnityColliderCollectorAuthのデータをもとに、Entityを生成するモジュール */
	public sealed class UniColEntityRegisterer : EntityRegistererBase<UniColCollectorAuthoring>
	{
		// ------------------------------------- public メンバ ----------------------------------------

		public UniColEntityRegisterer(int cap) : base(cap) {
			_initRot = new [] {
				Unity.Mathematics.quaternion.AxisAngle( float3(0,0,1), PI/2 ),
				quaternion(0,0,0,1),
				Unity.Mathematics.quaternion.AxisAngle( float3(1,0,0), -PI/2 )
			};
		}


		/** 追加・削除されたAuthの情報をECSへ反映させる */
		override public void apply(EntityManager em) {

			// 今登録されているBodyを全削除
			foreach (var i in _lastBodies) em.DestroyEntity(i);
			_lastBodies.Clear();


			base.apply(em);


			// 今登録されているAuthのBodyを全登録
			// 登録予定の要素を全登録
			foreach (var i in _entities) {
				var m2d = em.GetComponentData<BodiesPack_UniCol_M2D>( i.e );
				var auth = m2d.auth;

				for (int j=0; j<_firstBody.Length; ++j) {
					_firstBody[j] = Entity.Null;
					_lastBody[j] = Entity.Null;
				}


				// コライダー本体のEntityを生成
				for (int j=0; j<auth._ovlResultCnt; ++j) {
					var a = auth._ovlResults[j];

					Entity entity;
					int shapeId;
					quaternion rot;
					float3 center;
					if (a is SphereCollider) {
						var b = (SphereCollider)a;
						shapeId = (int)ShapeType.Sphere;
						entity = em.CreateEntity();

						rot = default;
						center = b.center;
						em.AddComponentData(entity, new Body_R{value=b.radius});
						em.AddComponentData(entity, new Body_Raw_Sphere());

					} else if (a is CapsuleCollider) {
						var b = (CapsuleCollider)a;
						shapeId = (int)ShapeType.Capsule;
						entity = em.CreateEntity();

						rot = _initRot[b.direction];
						center = b.center;
						var r = float3(b.radius, b.height/2, 0);
						em.AddComponentData(entity, new Body_R{value=r});
						em.AddComponentData(entity, new Body_Raw_Capsule());

					} else if (a is BoxCollider) {
						var b = (BoxCollider)a;
						shapeId = (int)ShapeType.Box;
						entity = em.CreateEntity();

						rot = _initRot[2];
						center = b.center;
						var r = b.size / 2;
						em.AddComponentData(entity, new Body_R{value=r});
						em.AddComponentData(entity, new Body_Raw_Box());

					} else {
						continue;
					}


					// とりあえずここでL2Wまで計算してしまう
					float4x4 l2w;
					if ( rot.value.Equals(float4(0,0,0,0)) ) {
						l2w = Unity.Mathematics.float4x4.identity;
						l2w.c3.xyz = center;
					} else {
						l2w = float4x4(rot, center);
					}
					l2w = mul(a.transform.localToWorldMatrix, l2w);


					em.AddComponentData(entity, new Body_Center{value=center});
					em.AddComponentData(entity, new Body_Rot{value=rot});
					em.AddComponentData(entity, new Body());
					em.AddComponentData(entity, new Body_UniCol());
					em.AddComponentData(entity, new Body_CurL2W{value=l2w});
					if (_firstBody[shapeId]==Entity.Null) _firstBody[shapeId]=entity;
					if (_lastBody[shapeId] != Entity.Null)
						em.AddComponentData(_lastBody[shapeId], new Body_Next{value=entity});
					_lastBody[shapeId] = entity;
					_lastBodies.Add(entity);
				}


				for (int j=0; j<_lastBody.Length; ++j) {
					if (_lastBody[j] != Entity.Null)
						em.AddComponentData(_lastBody[j], new Body_Next());
				}


				em.SetComponentData(
					m2d.auth._entity,
					new BodiesPack{
						firstSphere  = _firstBody[0],
						firstCapsule = _firstBody[1],
						firstBox     = _firstBody[2],
						firstPlane   = _firstBody[3],
					}
				);
			}
		}


		// --------------------------------- private / protected メンバ -------------------------------

		List<Entity> _lastBodies = new List<Entity>();

		Entity[] _firstBody = new Entity[(int)ShapeType.MAX_COUNT];
		Entity[] _lastBody = new Entity[(int)ShapeType.MAX_COUNT];
		readonly quaternion[] _initRot;


		/** Auth1つ分の変換処理 */
		override protected void convertOne(
			UniColCollectorAuthoring auth,
			RegLink regLink,
			EntityManager em
		) {

			var entity = em.CreateEntity();

			em.AddComponentData(entity, new BodiesPack());
			em.AddComponentData(entity, new BodiesPack_UniCol_M2D{
				auth = auth,
			});

			auth._entity = entity;

			addEntityCore(entity, regLink);
			addETPCore(entity, auth.transform, regLink);
		}

		protected override void reconvertOne(Entity entity, EntityManager em)
			=> throw new InvalidOperationException();


		// --------------------------------------------------------------------------------------------
	}
}