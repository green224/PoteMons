using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common.Windows {
using LZ = Localization.GizmoOptionsWindow;
using Field;

/**
 * ギズモ表示のオプションウィンドウ
 */
public sealed class GizmoOptionsWindow : EditorWindow
{
	// ------------------------------------- public メンバ --------------------------------------------

	static public bool isShowPtclR => Instance._isShowPtclR;			// パーティクル半径を表示する
	static public bool isShowPtclV => Instance._isShowPtclV;			// パーティクル速度を表示する
	static public bool isShowConnections => Instance._isShowConnections;	// 接続関係を表示する
	static public bool isShowLimitAgl => Instance._isShowLimitAgl;		// 角度制限を表示する
	static public bool isShowLimitPos => Instance._isShowLimitPos;		// 位置制限を表示する
	static public bool isShowCollider => Instance._isShowCollider;		// コライダーを表示する

	/** シングルトン実装 */
	static public GizmoOptionsWindow Instance {get{
		if (s_instance == null) {
			var a = Resources.FindObjectsOfTypeAll<GizmoOptionsWindow>();
			if (a!=null && 0!=a.Length) s_instance = a[0];
			if (s_instance == null) s_instance = CreateInstance<GizmoOptionsWindow>();
		}
		return s_instance;
	}}

	/** ウィンドウを表示する */
	[MenuItem(LZ.MenuName_Open)]
	static public void open() {
		Instance.titleContent = new GUIContent(LZ.Title);
		Instance.Show();
	}


	// ------------------------------------- private メンバ --------------------------------------------

	sealed class MarginScope : IDisposable {
		public MarginScope(int2 margin) {
			_margin = margin;
			EditorGUILayout.BeginVertical();
			EditorGUILayout.Space(_margin.y, false);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space(_margin.x, false);
		}
		public void Dispose() {
			EditorGUILayout.Space(_margin.x, false);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space(_margin.y, false);
			EditorGUILayout.EndVertical();
		}

		int2 _margin;
	}

	static GizmoOptionsWindow s_instance;
	[LeftToggle][SerializeField] bool _isShowPtclR = true;
	[LeftToggle][SerializeField] bool _isShowPtclV = false;
	[LeftToggle][SerializeField] bool _isShowConnections = true;
	[LeftToggle][SerializeField] bool _isShowLimitAgl = true;
	[LeftToggle][SerializeField] bool _isShowLimitPos = true;
	[LeftToggle][SerializeField] bool _isShowCollider = true;

	void OnGUI() {
		var serializedObject = new SerializedObject(this);

		void showProp(string propName, string visName) {
			var prop = serializedObject.FindProperty(propName);
			EditorGUILayout.PropertyField( prop, new GUIContent(visName) );
		}

		using (new MarginScope(2))
		using (var cc = new EditorGUI.ChangeCheckScope()) {
			showMiniArea(
				LZ.ViewToggles,
				() => {
					showProp( "_isShowPtclR",		LZ.IsShowParticleR );
					showProp( "_isShowPtclV",		LZ.IsShowParticleV );
					showProp( "_isShowConnections",	LZ.IsShowConnections );
					showProp( "_isShowLimitAgl",	LZ.IsShowLimitAgl );
					showProp( "_isShowLimitPos",	LZ.IsShowLimitPos );
					showProp( "_isShowCollider",	LZ.IsShowCollider );
				}
			);

			if (cc.changed) SceneView.RepaintAll();
		}
		GUILayout.FlexibleSpace();

		serializedObject.ApplyModifiedProperties();
	}

	/**
	 * タイトル付きのエリアを表示する処理。内容物を表示するデリゲータを指定する。
	 * 必要であればトグルでの表示非表示を行うこともできる
	 */
	bool showMiniArea(
		string title,
		Action drawPropProc,
		bool? drawBody = null
	) {
		using (new EditorGUILayout.VerticalScope("OL Box")) {

			// タイトル部分を表示
			using (new EditorGUILayout.VerticalScope("GameViewBackground")) {
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space(2, false);

					if ( drawBody.HasValue ) {
						// トグルが必要な場合は、トグル付きで表示する
						drawBody = EditorGUILayout.ToggleLeft(title, drawBody.Value);
					} else {
						// トグルが不要な場合はそのまま表示する
						EditorGUILayout.LabelField(title);
						drawBody = true;
					}
				}
				EditorGUILayout.Space(4, false);
			}

			// 本体部分を表示
			if (drawBody.Value) {
				++EditorGUI.indentLevel;
				drawPropProc();
				--EditorGUI.indentLevel;
				EditorGUILayout.Space(2, false);
			}

			return drawBody.Value;
		}
	}


	// --------------------------------------------------------------------------------------------------
}

}
