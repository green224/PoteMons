using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysBone.Body {
	using Common;
	using Common.Field;

	/**
	 * PhysClothやPhysSpringの派生元となる物理ボーン表現の基底クラス。
	 * 実際にはこちらから直接派生せずに、BaseAuthoringTの方から派生すること
	 */
	public abstract class BaseAuthoring : MonoBehaviour
	{
		// --------------------------- インスペクタに公開しているフィールド -----------------------------

		// 衝突検出を行う対象のコライダー
		[SerializeField] IzBCollider.BodiesPackAuthoring _collider = null;


		// ------------------------------------- public メンバ ----------------------------------------

		// 衝突検出を行う対象のコライダー
		public IzBCollider.BodiesPackAuthoring Collider => _collider;

		// バウンダリー球の半径
		public float BoundaryR {get; protected set;}

		/** 物理状態をリセットする */
		[ContextMenu("reset")]
		abstract public void reset();


		// --------------------------------- private / protected メンバ -------------------------------

		/**
		 * パラメータを再構築する処理。派生先で実装すること。
		 * この中でバウンダリー級の半径も計算する。
		 */
		abstract protected void rebuildParameters();



		// --------------------------------------------------------------------------------------------
	}


	/**
	 * PhysClothやPhysSpringの派生元となる物理ボーン表現の基底クラス
	 */
	public abstract class BaseAuthoringT<Impl, ImplSystem> : BaseAuthoring
	where Impl : BaseAuthoringT<Impl, ImplSystem>
	where ImplSystem : Core.BodySystemBase<Impl>
	{
		// --------------------------- インスペクタに公開しているフィールド -----------------------------


		// ------------------------------------- public メンバ ----------------------------------------

		/** 物理状態をリセットする */
		override public void reset() {
			GetSys().reset(_erRegLink);
		}


		// --------------------------------- private / protected メンバ -------------------------------

		/** ECSで得た結果をマネージドTransformに反映するためのバッファのリンク情報。System側から設定・参照される */
		Common.Entities8.EntityRegistererBase<Impl>.RegLink
			_erRegLink = new Common.Entities8.EntityRegistererBase<Impl>.RegLink();

		/** メインのシステムを取得する */
		protected ImplSystem GetSys() {
			var w = World.DefaultGameObjectInjectionWorld;
			if (w == null) return null;
			return w.GetOrCreateSystem<ImplSystem>();
		}

		virtual protected void OnEnable() {
			var sys = GetSys();
			if (sys != null) sys.register((Impl)this, _erRegLink);
		}

		virtual protected void OnDisable() {
			var sys = GetSys();
			if (sys != null) sys.unregister((Impl)this, _erRegLink);
		}


		// --------------------------------------------------------------------------------------------
	#if UNITY_EDITOR
		virtual protected void LateUpdate() {
			if (__need2syncManage) {
				__need2syncManage = false;
				rebuildParameters();
				var sys = GetSys();
				if (sys != null) sys.resetParameters(_erRegLink);
			}
		}

		// 実行中にプロパティが変更された場合は、次回Update時に同期を行う
		bool __need2syncManage = false;
		virtual protected void OnValidate() {
			if (Application.isPlaying) __need2syncManage = true;
		}
	#endif
	}



}
