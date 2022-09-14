
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.IzBCollider.Core {
	using Common.Entities8;

	/** BodyAuthのデータをもとに、Entityを生成するモジュール */
	public sealed class EntityRegisterer : EntityRegistererBase<BodiesPackAuthoring>
	{
		// ------------------------------------- public メンバ ----------------------------------------

		public EntityRegisterer() : base(128) {}


		// --------------------------------- private / protected メンバ -------------------------------

		static readonly System.Action<EntityManager,Entity>[] _genRawBody = new System.Action<EntityManager,Entity>[] {
			(em,entity) => em.AddComponentData(entity, new Body_Raw_Sphere()),
			(em,entity) => em.AddComponentData(entity, new Body_Raw_Capsule()),
			(em,entity) => em.AddComponentData(entity, new Body_Raw_Box()),
			(em,entity) => em.AddComponentData(entity, new Body_Raw_Plane()),
		};

		Entity[] _firstBody = new Entity[(int)ShapeType.MAX_COUNT];
		Entity[] _lastBody = new Entity[(int)ShapeType.MAX_COUNT];

		/** Auth1つ分の変換処理 */
		override protected void convertOne(
			BodiesPackAuthoring auth,
			RegLink regLink,
			EntityManager em
		) {

			for (int i=0; i<_firstBody.Length; ++i) {
				_firstBody[i] = Entity.Null;
				_lastBody[i] = Entity.Null;
			}

			// 参照先のBodyをECSへ変換
			{
				foreach (var i in auth.Bodies) {

					int shapeId = (int)i.mode;

					// コンポーネントを割り当て
					var entity = em.CreateEntity();

					if (_firstBody[shapeId] == Entity.Null) _firstBody[shapeId] = entity;
					if (_lastBody[shapeId] != Entity.Null)
						em.AddComponentData(_lastBody[shapeId], new Body_Next{value=entity});
					em.AddComponentData(entity, new Body());
					em.AddComponentData(entity, new Body_Center{value=i.center});
					em.AddComponentData(entity, new Body_R{value=i.r});
					var rot = i.mode == ShapeType.Sphere ? default : i.rot;
					em.AddComponentData(entity, new Body_Rot{value=rot});
					_genRawBody[shapeId](em, entity);
					em.AddComponentData(entity, new Body_CurL2W());
					em.AddComponentData(entity, new Body_M2D{bodyAuth=i});

					// Entity・Transformを登録
					addEntityCore(entity, regLink);
					addETPCore(entity, i.transform, regLink);

					_lastBody[shapeId] = entity;
				}
				foreach (var i in _lastBody)
					if (i != Entity.Null) em.AddComponentData(i, new Body_Next());
			}


			{// BodiesPackをECSへ変換
				var entity = em.CreateEntity();
				var nextEnt = Entity.Null;
				if (auth._uniColCollector != null) nextEnt = auth._uniColCollector._entity;
				em.AddComponentData(entity, new BodiesPack{
					firstSphere  = _firstBody[0],
					firstCapsule = _firstBody[1],
					firstBox     = _firstBody[2],
					firstPlane   = _firstBody[3],
					next = nextEnt
				});
				auth.setRootEntity( entity, em );
			}
		}

		/** 指定Entityの再変換処理 */
		override protected void reconvertOne(Entity entity, EntityManager em) {
			if (!em.HasComponent<Body_M2D>(entity)) return;

			var auth = em.GetComponentData<Body_M2D>(entity).bodyAuth;
			em.SetComponentData(entity, new Body_Center{value=auth.center});
			em.SetComponentData(entity, new Body_R{value=auth.r});
			em.SetComponentData(entity, new Body_Rot{value=auth.rot});
		}
		

		// --------------------------------------------------------------------------------------------
	}
}