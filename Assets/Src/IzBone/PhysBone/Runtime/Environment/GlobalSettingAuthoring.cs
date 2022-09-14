using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysBone.Environment {
	using Common;
	using Common.Field;


	/**
	 * グローバール環境の設定用コンポーネント
	 */
	[AddComponentMenu("IzBone/Environment/IzBone_Environment_Global")]
	public sealed class GlobalSettingAuthoring
	{
		// ------------------------------- inspectorに公開しているフィールド ------------------------
		// --------------------------------------- publicメンバ -------------------------------------

		public Gravity gravity = new Gravity(1);

		[HalfLifeDrag] public HalfLife airDrag = 0.1f;

		public float3 wind = 0;


		static public GlobalSettingAuthoring Instance {get; private set;}


		// ----------------------------------- private/protected メンバ -------------------------------

		void OnEnable() {
			Instance = this;
		}

		void OnDisable() {
			if (Instance == this) Instance = null;
		}



		// --------------------------------------------------------------------------------------------
	}


}
