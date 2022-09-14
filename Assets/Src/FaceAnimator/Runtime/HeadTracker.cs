using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace FaceAnimator {

/** ヘッドトラッキング用モジュール */
[AddComponentMenu("FaceAnimator/HeadTracker")]
public sealed class HeadTracker: MonoBehaviour {
	//--------------------------- インスペクタに公開しているフィールド ---------------------------

	[Serializable] sealed class BendingSegment {
		public Transform firstTransform = null;
		public Transform lastTransform = null;

		public float minAglDiff = 0;
		public float bendRate = 0.6f;
		public Vector2 maxAglDiff = new Vector2( 30, 30 );
		public Vector2 maxBendAgl = new Vector2( 80, 80 );
		public float spd = 5;

		internal float angleH;
		internal float angleV;
		internal Vector3 dirUp;
		internal Vector3 referenceLookDir;
		internal Vector3 referenceUpDir;
		internal int chainLength;
		internal Quaternion[] origRotations;
	}

	[Serializable] sealed class NonAffectedJoints {
		public Transform joint = null;
		public float effect = 0;
		internal Vector3 dirCache;
	}
	
	[SerializeField] Transform rootNode = null;
	[SerializeField] BendingSegment[] segments = null;
	[UnityEngine.Serialization.FormerlySerializedAs("nonAffectedJoints")]
	[SerializeField] NonAffectedJoints[] _nonAfcJnt = null;
	[SerializeField] Vector3 headLookVector = Vector3.forward;
	[SerializeField] Vector3 headUpVector = Vector3.up;
	[SerializeField] Vector2 fadeOutRange = new Vector2( 90, 10 );			//!< 顔を後ろと上下の方向に、ヘッドトラックの影響をフェードアウトする範囲


	//-------------------------------------- public メンバ ---------------------------------------

	public Vector3 lookTgtPos;
	public Vector3 lookTgtDir;			//!< Desired look direction in world space
	[Range(0,1)] public float lookPosDirRate = 0;	//!< 1でDir100%、0でPos100%になる
	public float effect = 1;
	public bool overrideAnimation = false;

	public bool autoUpdate = true;			//!< 自動でUpdateを行うか否か


	/** 指定のTransformから見た、ワールド座標での目標の方向 */
	public Vector3 calcLookWDir(Transform src) {
		if ( 0.9999f < lookPosDirRate ) {
			return lookTgtDir;
		} else {
			var dirFromPos = (lookTgtPos - src.position).normalized;
			return Vector3.Lerp( dirFromPos, lookTgtDir, lookPosDirRate );
		}
	}

	/** 更新処理。UseAutoUpdateがfalseのときは、手動で呼ぶこと */
	public void update(float dt) {
		if (dt <= 0.0001f) return;
		if (effect <= 0.0001f) return;
		
		// Remember initial directions of joints that should not be affected
		for (int i=0; i<_nonAfcJnt.Length; ++i) {
			foreach (Transform child in _nonAfcJnt[i].joint) {
				_nonAfcJnt[i].dirCache =
					child.position - _nonAfcJnt[i].joint.position;
				break;
			}
		}
		
		// Handle each segment
		float? fadeOutRate = null;
		foreach (var segment in segments) {
			var t = segment.lastTransform;
			if (overrideAnimation) {
				for (int i=segment.chainLength-1; i>=0; --i) {
					t.localRotation = segment.origRotations[i];
					t = t.parent;
				}
			}
			
			var parentRot = segment.firstTransform.parent.rotation;
			var parentRotInv = Quaternion.Inverse(parentRot);

			// 目標位置を指定された場合は、そこから目標向きを算出
			var fixedLookWDir = calcLookWDir( segment.lastTransform );
			
			// Desired look directions in neck parent space
			var lookDirGoal = parentRotInv * fixedLookWDir;
			
			// Get the horizontal and vertical rotation angle to look at the target
			float hAngle = AngleAroundAxis(
				segment.referenceLookDir, lookDirGoal, segment.referenceUpDir
			);
			
			var rightOfTarget = Vector3.Cross(segment.referenceUpDir, lookDirGoal);
			
			var lookDirGoalinHPlane =
				lookDirGoal - Vector3.Project(lookDirGoal, segment.referenceUpDir);
			
			var vAngle = AngleAroundAxis(
				lookDirGoalinHPlane, lookDirGoal, rightOfTarget
			);

			if ( fadeOutRate == null ) {
				fadeOutRate =
					min( ( 180f - abs( hAngle ) ) / fadeOutRange.x, 1 ) *
					min( (  90f - abs( vAngle ) ) / fadeOutRange.y, 1 );
			}
			hAngle *= fadeOutRate.Value;
			vAngle *= fadeOutRate.Value;

			// Handle threshold angle difference, bending multiplier,
			// and max angle difference here
			hAngle = max(
				0,
				max(
					( abs(hAngle) - segment.minAglDiff ) * segment.bendRate,
					abs(hAngle) - segment.maxAglDiff.x
				)
			) * sign(hAngle);
			vAngle = max(
				0,
				max(
					( abs(vAngle) - segment.minAglDiff ) * segment.bendRate,
					abs(vAngle) - segment.maxAglDiff.y
				)
			) * sign(vAngle);
			
			// Handle max bending angle here
			hAngle = clamp(hAngle, -segment.maxBendAgl.x, segment.maxBendAgl.x);
			vAngle = clamp(vAngle, -segment.maxBendAgl.y, segment.maxBendAgl.y);
			
			var referenceRightDir =
				Vector3.Cross(segment.referenceUpDir, segment.referenceLookDir);
			
			// Lerp angles
			segment.angleH = lerp(
				segment.angleH, hAngle, dt * segment.spd
			);
			segment.angleV = lerp(
				segment.angleV, vAngle, dt * segment.spd
			);
			
			// Get direction
			lookDirGoal = Quaternion.AngleAxis(segment.angleH, segment.referenceUpDir)
				* Quaternion.AngleAxis(segment.angleV, referenceRightDir)
				* segment.referenceLookDir;
			
			// Make look and up perpendicular
			var upDirGoal = segment.referenceUpDir;
			Vector3.OrthoNormalize(ref lookDirGoal, ref upDirGoal);
			
			// Interpolated look and up directions in neck parent space
			var lookDir = lookDirGoal;
			segment.dirUp = Vector3.Slerp(segment.dirUp, upDirGoal, dt*5);
			Vector3.OrthoNormalize(ref lookDir, ref segment.dirUp);
			
			// Look rotation in world space
			var lookRot = (
				(parentRot * Quaternion.LookRotation(lookDir, segment.dirUp))
				* Quaternion.Inverse(
					parentRot * Quaternion.LookRotation(
						segment.referenceLookDir, segment.referenceUpDir
					)
				)
			);
			
			// Distribute rotation over all joints in segment
			var dividedRotation =
				Quaternion.Slerp(Quaternion.identity, lookRot, effect / segment.chainLength);
			t = segment.lastTransform;
			for (int i=0; i<segment.chainLength; ++i) {
				t.rotation = dividedRotation * t.rotation;
				t = t.parent;
			}
		}
		
		// Handle non affected joints
		for (int i=0; i<_nonAfcJnt.Length; ++i) {
			var newJointDirection = Vector3.zero;
			
			foreach (Transform child in _nonAfcJnt[i].joint) {
				newJointDirection = child.position - _nonAfcJnt[i].joint.position;
				break;
			}
			
			var combinedJointDirection = Vector3.Slerp(
				_nonAfcJnt[i].dirCache, newJointDirection, _nonAfcJnt[i].effect
			);
			
			_nonAfcJnt[i].joint.rotation = Quaternion.FromToRotation(
				newJointDirection, combinedJointDirection
			) * _nonAfcJnt[i].joint.rotation;
		}
	}
	
	//-------------------------------------- private メンバ --------------------------------------
	

	void Start () {
		if (rootNode == null) rootNode = transform;
		
		// Setup segments
		foreach (var segment in segments) {
			var parentRot = segment.firstTransform.parent.rotation;
			var parentRotInv = Quaternion.Inverse(parentRot);
			segment.referenceLookDir =
				parentRotInv * rootNode.rotation * headLookVector.normalized;
			segment.referenceUpDir =
				parentRotInv * rootNode.rotation * headUpVector.normalized;
			segment.angleH = 0;
			segment.angleV = 0;
			segment.dirUp = segment.referenceUpDir;
			
			segment.chainLength = 1;
			var t = segment.lastTransform;
			while (t != segment.firstTransform && t != t.root) {
				segment.chainLength++;
				t = t.parent;
			}
			
			segment.origRotations = new Quaternion[segment.chainLength];
			t = segment.lastTransform;
			for (int i=segment.chainLength-1; i>=0; --i) {
				segment.origRotations[i] = t.localRotation;
				t = t.parent;
			}
		}
	}
	
	void LateUpdate() {
		if (autoUpdate) update(Time.deltaTime);
	}
	
	// The angle between dirA and dirB around axis
	static float AngleAroundAxis (Vector3 dirA, Vector3 dirB, Vector3 axis) {
		// Project A and B onto the plane orthogonal target axis
		dirA = dirA - Vector3.Project(dirA, axis);
		dirB = dirB - Vector3.Project(dirB, axis);
		
		// Find (positive) angle between A and B
		var angle = Vector3.Angle(dirA, dirB);
		
		// Return angle multiplied with 1 or -1
		return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
	}


	//--------------------------------------------------------------------------------------------

#if UNITY_EDITOR
	void OnDrawGizmos() {
//		_ctrl.drawGizmos();
	}
	
	[CustomEditor(typeof(HeadTracker))]
	sealed class CustomInspector: Editor {
		void OnSceneGUI() {
			var tgt = target as HeadTracker;
			if ( tgt == null ) return;

			// 目標視点
			var ltdLen = tgt.lookTgtDir.magnitude;
			if ( ltdLen < 0.001f ) tgt.lookTgtDir = Vector3.forward;
			else tgt.lookTgtDir.Normalize();
			var dstLTP = tgt.lookTgtPos;
			var srcLTD = Quaternion.LookRotation( tgt.lookTgtDir, tgt.headUpVector );
			var dstLTD = srcLTD;
			Handles.TransformHandle( ref dstLTP, ref dstLTD );
			if ( dstLTP != tgt.lookTgtPos || dstLTD != srcLTD ) {
				tgt.lookTgtPos = dstLTP;
				tgt.lookTgtDir = dstLTD * Quaternion.Inverse(srcLTD) * tgt.lookTgtDir;
			}

		}
	}
#endif
}
}
