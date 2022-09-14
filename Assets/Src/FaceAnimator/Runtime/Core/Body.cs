using System;
using UnityEngine;

using System.Linq;
using Unity.Mathematics;
using static Unity.Mathematics.math;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FaceAnimator.Core {

/**
 * FaceAnimatorを実際に使用する際にGameObjectに設定するコンポーネント
 */
[AddComponentMenu("FaceAnimator/Body")]
[ExecuteAlways]
sealed class Body : MonoBehaviour {
	//--------------------------- インスペクタに公開しているフィールド ---------------------------

	[SerializeField] MasterData _md = null;		//!< マスターデータ


	//-------------------------------------- public メンバ ---------------------------------------

	public float[] weight = null;		//!< ブレンド用のウェイト
	public bool autoUpdate = true;		//!< 自動でupdateを呼ぶか否か

	public MasterData MD => _md;		//!< マスターデータ

	/** 更新処理。手動で呼ぶことも出来る */
	public void update() {
		if (_md == null) return;

		// 初期化・再初期化
		if (_rd == null) reset();

#if UNITY_EDITOR
		// Editor停止中は、Weightの配列数が不正になる可能性もあるので、チェックする
		if (
			!Application.isPlaying &&
			(weight==null || weight.Length!=_rd.weight.Length)
		) reset();
#endif

		// 更新
		for (int i=0; i<weight.Length; ++i) _rd.weight[i] = weight[i];
		_rd.update();
	}


	//-------------------------------------- private メンバ --------------------------------------

	RuntimeData _rd = null;

	void LateUpdate() {
		if (!autoUpdate) return;
		update();
	}

	/** 再初期化する */
	void reset() {
		if (_rd == null) _rd = new RuntimeData();
		_rd.setup(_md, transform);

		if (weight==null || weight.Length!=_rd.weight.Length)
			weight = new float[_rd.weight.Length];
	}


	//--------------------------------------------------------------------------------------------

#if UNITY_EDITOR
	void OnValidate() {
		if (Application.isPlaying) return;
		if (_md == null) return;

		// Editor停止中は、Validate毎に再初期化
		reset();
		_rd.update();
	}

	void OnEnable() {
		if (!Application.isPlaying)
			UnityEditor.EditorApplication.update += LateUpdate;
	}

	void OnDisable() {
		if (!Application.isPlaying)
			UnityEditor.EditorApplication.update -= LateUpdate;
	}

	[CustomEditor(typeof(Body))]
	sealed class CustomInspector : Editor {
		public override void OnInspectorGUI() {
			serializedObject.Update();

			// MasterData
			EditorGUILayout.PropertyField(
				serializedObject.FindProperty("_md"),
				new GUIContent("MasterData")
			);

			// Weight
			using (new EditorGUILayout.VerticalScope("box")) {
				EditorGUILayout.LabelField("Weight");
				var wProp = serializedObject.FindProperty("weight");
				var rd = (target as Body)?._rd;
				if (rd != null && rd.MD != null && wProp.arraySize == rd.weight.Length) {
					for (int i=0; i<rd.MD.poseSets.Length; ++i) {
						var prop = wProp.FindPropertyRelative("Array.data["+i+"]");
						prop.floatValue = EditorGUILayout.Slider(
							rd.MD.poseSets[i].name,
							prop.floatValue,
							0,
							1
						);
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
#endif
}

}