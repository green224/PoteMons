using System;
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysBone.Environment {
	using Common;
	using Common.Field;


	/**
	 * ローカル環境の設定用コンポーネント
	 */
	[AddComponentMenu("IzBone/Environment/IzBone_Environment_Local")]
	public sealed class LocalSettingAuthoring
	{
		// ------------------------------- inspectorに公開しているフィールド ------------------------

		[SerializeField][Min(0)] float _boundaryR = 1;
		enum BlendMode {Max, Min,}
		[SerializeField] BlendMode _colliderBlendMode = BlendMode.Min;
		[SerializeField] IzBCollider.BodiesPackAuthoring _collider = null;
		[SerializeField][Min(0)] float _blendDistance = 1;
		[SerializeField] int _priority = 0;


		[Space]
		[SerializeField] bool _overrideGravity = false;
		[SerializeField] Gravity _gravity = new Gravity(1);

		[SerializeField] bool _overrideAirDrag = false;
		[SerializeField][HalfLifeDrag] HalfLife _airDrag = 0.1f;

		[SerializeField] bool _overrideWind = false;
		[SerializeField] float3 _wind = 0;


		// --------------------------------------- publicメンバ -------------------------------------

		public Gravity? Gravity {
			get => _overrideGravity ? _gravity : (Gravity?)null;
			set {
				if (value.HasValue) {
					_overrideGravity = true;
					_gravity = value.Value;
				} else {
					_overrideGravity = false;
				}
			}
		}

		public HalfLife? AirDrag {
			get => _overrideAirDrag ? _airDrag : (HalfLife?)null;
			set {
				if (value.HasValue) {
					_overrideGravity = true;
					_airDrag = value.Value;
				} else {
					_overrideGravity = false;
				}
			}
		}

		public float3? Wind {
			get => _overrideWind ? _wind : (float3?)null;
			set {
				if (value.HasValue) {
					_overrideGravity = true;
					_wind = value.Value;
				} else {
					_overrideGravity = false;
				}
			}
		}



		// ----------------------------------- private/protected メンバ -------------------------------

		/** ECSで得た結果をマネージドTransformに反映するためのバッファのリンク情報。System側から設定・参照される */
		Core.EntityRegisterer.RegLink _erRegLink = new Core.EntityRegisterer.RegLink();

		/** メインのシステムを取得する */
		Core.IzBEnvironmentSystem GetSys() {
			var w = World.DefaultGameObjectInjectionWorld;
			if (w == null) return null;
			return w.GetOrCreateSystem<Core.IzBEnvironmentSystem>();
		}

		void OnEnable() {
		}

		void OnDisable() {
		}



		// --------------------------------------------------------------------------------------------
	}


}
