
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysBone.Environment.Core {
	using Common.Entities8;

	/** LocalSettingAuthのデータをもとに、Entityを生成するモジュール */
	public sealed class EntityRegisterer : EntityRegistererBase<LocalSettingAuthoring>
	{
		// ------------------------------------- public メンバ ----------------------------------------

		public EntityRegisterer() : base(128) {}


		// --------------------------------- private / protected メンバ -------------------------------

		/** Auth1つ分の変換処理 */
		override protected void convertOne(
			LocalSettingAuthoring auth,
			RegLink regLink,
			EntityManager em
		) {

		}

		/** 指定Entityの再変換処理 */
		override protected void reconvertOne(Entity entity, EntityManager em) {
		}
		

		// --------------------------------------------------------------------------------------------
	}
}