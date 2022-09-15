using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Collections.Generic;

namespace IzBone.IzBCollider {

	/** IzBone専用のコライダーの対象コライダーを一つにパックするためのコンポーネント */
	[AddComponentMenu("IzBone/IzBone_CollidersPack")]
	public sealed class BodiesPackAuthoring : MonoBehaviour {
		// ------------------------------- inspectorに公開しているフィールド ------------------------

		[SerializeField] BodyAuthoring[] _bodies = new BodyAuthoring[0];
		[SerializeField] internal UniColCollectorAuthoring _uniColCollector = null;


		// --------------------------------------- publicメンバ -------------------------------------

		public BodyAuthoring[] Bodies => _bodies;

		// このBodiesPackの対応するルートのEntity
		public Entity RootEntity => _rootEntity;

		// このBodiesPackの対応するルートのEntityを設定する。
		// もしまだ未精製の場合は、生成時にコールバックを実行する
		public void getRootEntity(
			EntityManager em,
			Action< EntityManager, Entity > onSetRootEntity
		) {
			if (_rootEntity == Entity.Null) {_onSetRootEntity += onSetRootEntity; return;}
			onSetRootEntity(em, _rootEntity);
		}


		// ----------------------------------- private/protected メンバ -------------------------------

		/** ECSで得た結果をマネージドTransformに反映するためのバッファのリンク情報。System側から設定・参照される */
		Core.EntityRegisterer.RegLink _erRegLink = new Core.EntityRegisterer.RegLink();

		Entity _rootEntity = Entity.Null;		// このBodiesPackの対応するルートのEntity
		event Action< EntityManager, Entity > _onSetRootEntity;		// RootEntityが設定されたときのコールバック



		/** RootEntityを設定する処理。EntityRegistererから呼ばれる */
		internal void setRootEntity(Entity entity, EntityManager em) {
			if (_rootEntity != Entity.Null) throw new InvalidProgramException();
			_rootEntity = entity;
			_onSetRootEntity?.Invoke(em, entity);
			_onSetRootEntity = null;
		}

		/** メインのシステムを取得する */
		Core.IzBColliderSystem GetSys() {
			var w = World.DefaultGameObjectInjectionWorld;
			if (w == null) return null;
			return w.GetOrCreateSystem<Core.IzBColliderSystem>();
		}

		void OnEnable() {
			var sys = GetSys();
			if (sys != null) sys.register(this, _erRegLink);

#if UNITY_EDITOR
			foreach (var i in _bodies) i.__parents.Add(this);
#endif
		}

		void OnDisable() {
			var sys = GetSys();
			if (sys != null) sys.unregister(this, _erRegLink);
			_rootEntity = Entity.Null;

#if UNITY_EDITOR
			foreach (var i in _bodies) i.__parents.Remove(this);
#endif
		}


		// --------------------------------------------------------------------------------------------
#if UNITY_EDITOR
		// BodyのOnValidate時に、Bodyから呼ばれるコールバック
		internal void __onValidateBody() {
			if (Application.isPlaying) {
				var sys = GetSys();
				if (sys != null) sys.resetParameters(_erRegLink);
			}
		}

		[ContextMenu("配下のコライダを自動収集する")]
		void autoCollect() {

			static void collectChildren(Transform trans, List<BodyAuthoring> result) {
				for (int i=0; i<trans.childCount; ++i)
					collectChildren( trans.GetChild(i), result );

				var a = trans.GetComponents<BodyAuthoring>();
				foreach (var i in a) result.Add(i);
			}

			var ret = new List<BodyAuthoring>();
			collectChildren(transform, ret);

			_bodies = ret.ToArray();
		}
#endif
	}

}

