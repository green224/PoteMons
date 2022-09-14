//#define WITH_DEBUG
using System;
using UnityEngine.Jobs;
using Unity.Jobs;

using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;




namespace IzBone.PhysBone.Body.Core {
	using Common.Entities8;

	/** Bodyが実装すべきSystemの基底クラス */
//	public interface IBodySystem<Authoring>
	public abstract partial class BodySystemBase<Authoring> : SystemBase
	where Authoring : BaseAuthoring
	{

		// Authoringを登録・登録解除する処理
		public void register(
			Authoring auth,
			EntityRegistererBase<Authoring>.RegLink regLink
		) =>_entityReg.register(auth, regLink);
		public void unregister(
			Authoring auth,
			EntityRegistererBase<Authoring>.RegLink regLink
		) => _entityReg.unregister(auth, regLink);
		public void resetParameters(
			EntityRegistererBase<Authoring>.RegLink regLink
		) => _entityReg.resetParameters(regLink);
		protected EntityRegistererBase<Authoring> _entityReg;

		/** 指定のAuthの物理状態をリセットする */
		abstract public void reset(
			EntityRegistererBase<Authoring>.RegLink regLink
		);


	}


}
