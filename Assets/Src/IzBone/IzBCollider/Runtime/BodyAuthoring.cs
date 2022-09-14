using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace IzBone.IzBCollider {
	using Common;

	/** IzBone専用のコライダー */
	[AddComponentMenu("IzBone/IzBone_Collider")]
	public sealed class BodyAuthoring : MonoBehaviour {
		// ------------------------------- inspectorに公開しているフィールド ------------------------

		[SerializeField] ShapeType _shapeType = ShapeType.Sphere;
		[SerializeField] float3 _center = float3(0,0,0);
		[SerializeField] float3 _r = float3(1,1,1);
		[SerializeField] quaternion _rot = Unity.Mathematics.quaternion.identity;


		// --------------------------------------- publicメンバ -------------------------------------

		public ShapeType mode => _shapeType;
		public float3 center => _center;
		public float3 r => _r;
		public quaternion rot => _rot;


		// ----------------------------------- private/protected メンバ -------------------------------

		[NonSerialized] internal float4x4 l2wMtx;
		[NonSerialized] internal float3 l2wMtxClmNorm;

		new Transform transform {get{
			if (_transform == null) _transform = ((MonoBehaviour)this).transform;
			return _transform;
		}}
		Transform _transform;

		float3 _ctrCache = default;
		quaternion rotCache = Unity.Mathematics.quaternion.identity;
		void checkRebuildL2GMat() {
			var trans = transform;
			if (_shapeType == ShapeType.Sphere) {
				if (!trans.hasChanged && _ctrCache.Equals(_center)) return;
				var tr = Unity.Mathematics.float4x4.identity;
				tr.c3.xyz = _center;
				l2wMtx = mul(trans.localToWorldMatrix, tr);
			} else {
				if (!trans.hasChanged && _ctrCache.Equals(_center) && rotCache.Equals(_rot)) return;
				var tr = float4x4(_rot, _center);
				l2wMtx = mul(trans.localToWorldMatrix, tr);
				rotCache = _rot;
			}
			_ctrCache = _center;

			l2wMtxClmNorm = float3(
				length( l2wMtx.c0.xyz ),
				length( l2wMtx.c1.xyz ),
				length( l2wMtx.c2.xyz )
			);
		}


		// --------------------------------------------------------------------------------------------
#if UNITY_EDITOR
		internal List<BodiesPackAuthoring> __parents = new List<BodiesPackAuthoring>();
		void OnValidate() {
			foreach (var i in __parents) i.__onValidateBody();
		}

		// Editorでのギズモを表示する処理。
		// OnDrawGizmosで呼べるようにここに定義しているが、
		// .Editorアセンブリからのみ呼んでいる現状、ここではなくEditorアセンブリに移動するべきかもしれない。
		internal void DEBUG_drawGizmos() {
//			if ( !Application.isPlaying ) checkRebuildL2GMat();
			checkRebuildL2GMat();
			Gizmos8.drawMode = Gizmos8.DrawMode.Handle;

			Gizmos8.color = Gizmos8.Colors.Collider;

			if (_shapeType == ShapeType.Sphere) {
				var pos = l2wMtx.c3.xyz;
				var size = mul( (float3x3)l2wMtx, (float3)(_r.x / sqrt(3)) );
				Gizmos8.drawWireSphere( pos, length(size) );

			} else if (_shapeType == ShapeType.Capsule) {
				var pos = l2wMtx.c3.xyz;
				var l2gMat3x3 = (float3x3)l2wMtx;
				var sizeX  = mul( l2gMat3x3, float3(_r.x,0,0) );
				var sizeY0 = mul( l2gMat3x3, float3(0,_r.y-_r.x,0) );
				var sizeY1 = mul( l2gMat3x3, float3(0,_r.x,0) );
				var sizeZ  = mul( l2gMat3x3, float3(0,0,_r.x) );
				float3 p0 = default;
				float3 p1 = default;
				for (int i=0; i<=30; ++i) {
					var theta = (float)i/30 * (PI*2);
					var c = cos(theta);
					var s = sin(theta);
					var pN0 = pos + sizeY0 + c*sizeX + s*sizeZ;
					var pN1 = pos - sizeY0 + c*sizeX + s*sizeZ;
					if (i!=0) {
						Gizmos8.drawLine( p0, pN0 );
						Gizmos8.drawLine( p1, pN1 );
					}
					p0 = pN0;
					p1 = pN1;
				}
				for (int i=0; i<=15; ++i) {
					var theta = (float)i/15 * PI;
					var c = cos(theta);
					var s = sin(theta);
					var pN0 = c*sizeX + s*sizeY1;
					var pN1 = c*sizeZ + s*sizeY1;
					if (i!=0) {
						Gizmos8.drawLine(pos + sizeY0+p0, pos + sizeY0+pN0 );
						Gizmos8.drawLine(pos + sizeY0+p1, pos + sizeY0+pN1 );
						Gizmos8.drawLine(pos - sizeY0-p0, pos - sizeY0-pN0 );
						Gizmos8.drawLine(pos - sizeY0-p1, pos - sizeY0-pN1 );
					}
					p0 = pN0;
					p1 = pN1;
				}
				Gizmos8.drawLine( pos +sizeY0+sizeX, pos -sizeY0+sizeX );
				Gizmos8.drawLine( pos +sizeY0-sizeX, pos -sizeY0-sizeX );
				Gizmos8.drawLine( pos +sizeY0+sizeZ, pos -sizeY0+sizeZ );
				Gizmos8.drawLine( pos +sizeY0-sizeZ, pos -sizeY0-sizeZ );

			} else if (_shapeType == ShapeType.Box) {
				var pos = l2wMtx.c3.xyz;
				var l2gMat3x3 = (float3x3)l2wMtx;
				var sizeX = mul( l2gMat3x3, float3(_r.x,0,0) );
				var sizeY = mul( l2gMat3x3, float3(0,_r.y,0) );
				var sizeZ = mul( l2gMat3x3, float3(0,0,_r.z) );
				var ppp =  sizeX +sizeY +sizeZ + pos;
				var ppm =  sizeX +sizeY -sizeZ + pos;
				var pmp =  sizeX -sizeY +sizeZ + pos;
				var pmm =  sizeX -sizeY -sizeZ + pos;
				var mpp = -sizeX +sizeY +sizeZ + pos;
				var mpm = -sizeX +sizeY -sizeZ + pos;
				var mmp = -sizeX -sizeY +sizeZ + pos;
				var mmm = -sizeX -sizeY -sizeZ + pos;
				Gizmos8.drawLine( ppp, ppm );
				Gizmos8.drawLine( pmp, pmm );
				Gizmos8.drawLine( mpp, mpm );
				Gizmos8.drawLine( mmp, mmm );
				Gizmos8.drawLine( ppp, pmp );
				Gizmos8.drawLine( ppm, pmm );
				Gizmos8.drawLine( mpp, mmp );
				Gizmos8.drawLine( mpm, mmm );
				Gizmos8.drawLine( ppp, mpp );
				Gizmos8.drawLine( ppm, mpm );
				Gizmos8.drawLine( pmp, mmp );
				Gizmos8.drawLine( pmm, mmm );

			} else if (_shapeType == ShapeType.Plane) {
				var pos = l2wMtx.c3.xyz;
				var l2gMat3x3 = (float3x3)l2wMtx;
				var x = mul( l2gMat3x3, float3(0.05f,0,0) );
				var y = mul( l2gMat3x3, float3(0,0.05f,0) );
				var z = mul( l2gMat3x3, float3(0,0,0.02f) );
				Gizmos8.drawLine( pos-x, pos+x );
				Gizmos8.drawLine( pos-y, pos+y );
				Gizmos8.drawLine( pos, pos-z );

			} else { throw new InvalidProgramException(); }
		}
#endif
	}

}

