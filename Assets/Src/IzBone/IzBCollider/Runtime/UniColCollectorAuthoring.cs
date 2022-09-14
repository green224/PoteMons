using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.IzBCollider {

	/** UnityのColliderを収集するためのモジュール */
	public sealed class UniColCollectorAuthoring : MonoBehaviour {
		// ------------------------------- inspectorに公開しているフィールド ------------------------

		[SerializeField][Min(0)]  float _r = 1;
		[SerializeField] LayerMask _targetLayerMask = default;


		// --------------------------------------- publicメンバ -------------------------------------

		// ----------------------------------- private/protected メンバ -------------------------------

		/** ECSで得た結果をマネージドTransformに反映するためのバッファのリンク情報。System側から設定・参照される */
		Core.UniColEntityRegisterer.RegLink _erRegLink = new Core.UniColEntityRegisterer.RegLink();

		internal Entity _entity;		// このAuthoringの対応するルートのEntity

		internal Collider[] _ovlResults = new Collider[64];
		internal int _ovlResultCnt = 0;


		/** メインのシステムを取得する */
		Core.IzBColliderUniColSystem GetSys() {
			var w = World.DefaultGameObjectInjectionWorld;
			if (w == null) return null;
			return w.GetOrCreateSystem<Core.IzBColliderUniColSystem>();
		}

		void Update() {
			var l2w = (float4x4)transform.localToWorldMatrix;
			_ovlResultCnt = Physics.OverlapSphereNonAlloc(
				l2w.c3.xyz,
				length(l2w.c0) * _r,
				_ovlResults,
				_targetLayerMask
			);
		}

		void OnEnable() {
			var sys = GetSys();
			if (sys != null) sys.register(this, _erRegLink);
		}

		void OnDisable() {
			var sys = GetSys();
			if (sys != null) sys.unregister(this, _erRegLink);
			_entity = Entity.Null;
		}



		// --------------------------------------------------------------------------------------------
#if UNITY_EDITOR
		void OnDrawGizmos() {
		if (Common.Gizmos8.isSelectedTransformTree(UnityEditor.Selection.objects, transform))
		using (new Common.Gizmos8.DrawModeScope(Common.Gizmos8.DrawMode.Handle)) {

			// コレクション領域の球範囲
			Common.Gizmos8.color = Color.white;
			Common.Gizmos8.drawWireSphere(
				transform.position,
				transform.localToWorldMatrix.GetColumn(0).magnitude * _r
			);


			// コレクション結果のUnity標準Collider
			Common.Gizmos8.color = Color.green;
			for (int i=0; i<_ovlResultCnt; ++i) {
				var a = _ovlResults[i];
				
				var l2w = a.transform.localToWorldMatrix;
				if (a is SphereCollider) {
					var b = (SphereCollider)a;
					Common.Gizmos8.drawWireSphere(
						l2w.MultiplyPoint( b.center ),
						l2w.MultiplyVector( float3(b.radius,0,0) ).magnitude
					);

				} else if (a is CapsuleCollider) {
					var b = (CapsuleCollider)a;
					Common.Gizmos8.drawWireCapsule(
						l2w.MultiplyPoint( b.center ),
						l2w.MultiplyVector( float3(
							b.direction==0 ? 1 : 0,
							b.direction==1 ? 1 : 0,
							b.direction==2 ? 1 : 0
						) ).normalized,
						b.radius              * l2w.GetColumn((b.direction+2)%3).magnitude,
						(b.height/2-b.radius) * l2w.GetColumn(b.direction).magnitude
					);

				} else if (a is BoxCollider) {
					var b = (BoxCollider)a;
					Common.Gizmos8.drawWireCube(
						l2w.MultiplyPoint( b.center ),
						a.transform.rotation,
						float3(
							l2w.GetColumn(0).magnitude * b.size.x,
							l2w.GetColumn(1).magnitude * b.size.y,
							l2w.GetColumn(2).magnitude * b.size.z
						)
					);

				} else {
					continue;
				}

			}
		}}
#endif
	}

}

