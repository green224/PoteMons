using System.Collections.Generic;

using Unity.Entities;
using Unity.Collections;



namespace IzBone.Common.Entities8 {

	/**
	 * Autheringコンポーネントのデータをもとに、Entityを生成するモジュール。
	 * これはシステムUpdate時に同期して行われる必要があるため、このモジュールを介して
	 * Authの追加・削除をスタック→同期反映するようにする。
	 */
	public abstract class EntityRegistererBase<AuthComp> : System.IDisposable
	where AuthComp : class
	{
		// ------------------------------------- public メンバ ----------------------------------------

		/** Authとのバッファのリンク情報 */
		public sealed class RegLink {
			public List<int> etpIdxs = new List<int>();		//!< EntityTransformPacksへの格納情報
			public List<int> entIdxs = new List<int>();		//!< Entityのリストへの格納情報
		}

		public EntityTransformPacks etPacks;	//!< Transformを変更する必要のあるEntityとTransformの対応リスト

		// Authコンポーネントを登録・登録解除する
		public void register(AuthComp auth, RegLink regLink)
			=> _addList.Add( (auth, regLink, true) );
		public void unregister(AuthComp auth, RegLink regLink)
			=> _addList.Add( (auth, regLink, false) );

		// 指定のAuthに対して、パラメータの再適応を予約する
		public void resetParameters(RegLink regLink) => _need2resetList.Add(regLink);

		/** 追加・削除されたAuthの情報をECSへ反映させる */
		virtual public void apply(EntityManager em) {

			// 再コンバートが予約されているものを処理
			foreach (var i in _need2resetList)
				foreach (var j in i.entIdxs)
					reconvertOne( _entities[j].e, em );
			_need2resetList.Clear();

			// 新規コンバートが予約されているものを処理
			foreach (var authAdd in _addList) {
				if (authAdd.isAdd)
					convertOne(authAdd.auth, authAdd.regLink, em);
				else
					removeOne(authAdd.regLink, em);
			}
			_addList.Clear();
		}

		public EntityRegistererBase(int initCapacity) {
			etPacks = new EntityTransformPacks(initCapacity);
		}

		public void Dispose() {
			etPacks.Dispose();
		}


		// --------------------------------- private / protected メンバ -------------------------------

		/**
		 * 生成したEntityのリスト。
		 * いらなくなった際に削除するためにリストアップしておく必要がある。
		 * また、RegLinkとのリンク情報も同時に格納してある。
		 * このリストは複数あるコンポ―ネントのEntityを一列に整列したバッファで扱う関係上、
		 * 常にリストのインデックスとRegLink上とのリンク関係が正しく対応している必要がある。
		 * そのため、ここにリンク情報を格納して、格納位置が変化した際に互いのリンクを更新できるようにしてある。
		 */
		protected List<(Entity e, RegLink regLink, int idxInRL)> _entities
			= new List<(Entity, RegLink, int)>();

		/**
		 * EntityTransformPacksのインデックスと登録されたRegLinkとのリンク情報。
		 * ETP側にはこういったリンク情報はないため、_entitiesと同様にここで管理する。
		 */
		List<(RegLink regLink, int idxInRL)> _etPackIdxs = new List<(RegLink, int)>();

		/** 次のApplyタイミングで追加する情報をため込んだバッファ */
		List<(AuthComp auth, RegLink regLink, bool isAdd)> _addList
			= new List<(AuthComp, RegLink, bool)>();
		/** 次のApplyタイミングでパラメータのリセットをするRegLinkのリスト */
		HashSet<RegLink> _need2resetList = new HashSet<RegLink>();

		/** Auth1つ分の変換処理。派生先で実装すること */
		abstract protected void convertOne(
			AuthComp auth,
			RegLink regLink,
			EntityManager em
		);

		/** 指定Entityの再変換処理。派生先で実装すること */
		abstract protected void reconvertOne(Entity entity, EntityManager em);

		/** Auth1つ分の削除処理 */
		void removeOne(RegLink regLink, EntityManager em) {

			// Entityのキャッシュを破棄
			for (int i=0; i<regLink.entIdxs.Count; ++i) {
				var e = rmvEntityCore( regLink.entIdxs[i] );
				em.DestroyEntity( e );
			}

			// Transformのキャッシュを破棄
			for (int i=0; i<regLink.etpIdxs.Count; ++i) {
				rmvEtpCore( regLink.etpIdxs[i] );
			}

			// リンク情報を初期化
			regLink.entIdxs.Clear();
			regLink.etpIdxs.Clear();
		}



		/** Entityのリンクを一つ追加する */
		protected void addEntityCore( Entity e, RegLink regLink ) {
			_entities.Add( (e, regLink, regLink.entIdxs.Count) );
			regLink.entIdxs.Add(_entities.Count - 1);
		}
		/** ETPのリンクを一つ追加する */
		protected void addETPCore( Entity e, UnityEngine.Transform transform, RegLink regLink ) {
			etPacks.add( e, transform );
			_etPackIdxs.Add( (regLink, regLink.etpIdxs.Count) );
			regLink.etpIdxs.Add(_etPackIdxs.Count - 1);
		}
		/** Entityのリンクを一つ削除する */
		Entity rmvEntityCore( int idx ) {
			var ret = _entities[idx].e;

			// リンクを保ったままスワップバックする
			if (idx != _entities.Count - 1) {
				var last = _entities[_entities.Count - 1];
				last.regLink.entIdxs[last.idxInRL] = idx;
			}
			_entities.RemoveAtSwapBack(idx);

			return ret;
		}
		/** ETPのリンクを一つ削除する */
		void rmvEtpCore( int idx ) {
			// リンクを保ったままスワップバックする
			if (idx != _etPackIdxs.Count - 1) {
				var last = _etPackIdxs[_etPackIdxs.Count - 1];
				last.regLink.etpIdxs[last.idxInRL] = idx;
			}
			_etPackIdxs.RemoveAtSwapBack(idx);
			etPacks.removeAtSwapBack(idx);
		}


		// --------------------------------------------------------------------------------------------
	}
}