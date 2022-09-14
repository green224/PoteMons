using System;
using UnityEngine;

using System.Linq;
using Unity.Mathematics;
using static Unity.Mathematics.math;

using UnityEngine.UIElements;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
using System.Runtime.CompilerServices;
using UnityEditor.UIElements;
#endif


namespace FaceAnimator.Core {

/**
 * Bodyを扱いやすく操作するためのコントローラ
 */
[AddComponentMenu("FaceAnimator/SimpleController")]
[ExecuteAlways]
sealed class SimpleController : MonoBehaviour {
	//--------------------------- インスペクタに公開しているフィールド ---------------------------

	[SerializeField] Body _body = null;		//!< 対象のBody

	// 操作対象のアニメーション名(正規表現)
	[SerializeField] string _an_EyeTrack_C = "EyeTrack_(C|Idle)";	//!< 視線操作:中央
	[SerializeField] string _an_EyeTrack_L = "EyeTrack_L";			//!< 視線操作:左
	[SerializeField] string _an_EyeTrack_R = "EyeTrack_R";			//!< 視線操作:右
	[SerializeField] string _an_EyeTrack_U = "EyeTrack_U";			//!< 視線操作:上
	[SerializeField] string _an_EyeTrack_D = "EyeTrack_D";			//!< 視線操作:下
	[SerializeField] string _an_FacialEye_Idle      = "Eye_Idle";		//!< 目回り表情:通常
	[SerializeField] string _an_FacialEye_Close     = "Eye_Close";		//!< 目回り表情:閉じ
	[SerializeField] string _an_FacialEye_CloseHalf = "Eye_CloseHalf";	//!< 目回り表情:閉じ掛け
	[SerializeField] string _an_FacialEye_Half      = "Eye_Half";		//!< 目回り表情:ジト目(まばたき中の閉じ掛けではなく、脱力によるもの)
	[SerializeField] string _an_FacialMouth_Close = "Mouth_Close";	//!< 口回り表情:閉じ
	[SerializeField] string _an_FacialMouth_E     = "Mouth_E";		//!< 口回り表情:発音E (通常時)
	[SerializeField] string _an_FacialMouth_A     = "Mouth_A";		//!< 口回り表情:発音A
	[SerializeField] string _an_FacialMouth_O     = "Mouth_O";		//!< 口回り表情:発音O

	[SerializeField] float _eyeTrackD_HalfRate = 0.47f;		//!< 下を向いたときの半目具合
	[SerializeField] float _eyeBlink_MinSepT = 2;			//!< 自動まばたきの最小時間間隔
	[SerializeField] float _eyeBlink_MaxSepT = 10;			//!< 自動まばたきの最大時間間隔
	AnimationCurve _eyeBlink_AniCrv =			//!< 自動まばたきのアニメーションカーブ
		new AnimationCurve(new [] {
			new Keyframe(0, 1),
			new Keyframe(0.02f, 0),
			new Keyframe(0.07f, 0),
			new Keyframe(0.16f, 0.9f, 7,7, 1, 0.03f),
			new Keyframe(0.6f, 1),
		});


	//-------------------------------------- public メンバ ---------------------------------------

	public Vector2 eyeTrackPos = Vector2.zero;	//!< 視線
	public float eyeOpenRate = 1;				//!< 目の開き具合
	public int mouthType = 0;					//!< 口の種類

	public bool eyeBlink_autoMode = true;		//!< 自動まばたきが有効か否か

	public bool autoUpdate = true;				//!< 自動でupdateを呼ぶか否か

	/** 更新処理。手動で呼ぶこともできる */
	public void update(float dt) {
		if (_body == null) return;
		if (!_isInisilized) setup();

		// 視線
		if (
			_wi_EyeTrack_C != -1 &&
			_wi_EyeTrack_L != -1 &&
			_wi_EyeTrack_R != -1 &&
			_wi_EyeTrack_U != -1 &&
			_wi_EyeTrack_D != -1
		) {
			float w_c, w_l, w_r, w_u, w_d;
			w_l = max(0, eyeTrackPos.x);
			w_u = max(0, eyeTrackPos.y);
			w_r = max(0, -eyeTrackPos.x);
			w_d = max(0, -eyeTrackPos.y);
			var a = (w_l + w_r + w_u + w_d);
			var b = max(1,a);
			w_l /= b;
			w_r /= b;
			w_u /= b;
			w_d /= b;
			w_c = 1f - min(a,1);

			_body.weight[_wi_EyeTrack_C] = w_c;
			_body.weight[_wi_EyeTrack_L] = w_l;
			_body.weight[_wi_EyeTrack_R] = w_r;
			_body.weight[_wi_EyeTrack_U] = w_u;
			_body.weight[_wi_EyeTrack_D] = w_d;
		}

		// 自動まばたき
		var eyeOpRateWB = eyeOpenRate;
		if (
			eyeBlink_autoMode
#if UNITY_EDITOR
			&& Application.isPlaying
#endif
		) {
			eyeBlinkTCnt_ += dt;
			if (eyeBlinkTMax_ < eyeBlinkTCnt_) {
				eyeBlinkTMax_ = lerp(_eyeBlink_MinSepT, _eyeBlink_MaxSepT, eyeBlinkRnd_.NextFloat(1));
				eyeBlinkTCnt_ = 0;
			}

			eyeOpRateWB = lerp(
				0, eyeOpRateWB,
				saturate( _eyeBlink_AniCrv.Evaluate(eyeBlinkTCnt_) )
			);
		}

		// 目回りの表情
		if (
			_wi_FacialEye_Idle != -1 &&
			_wi_FacialEye_Close != -1 &&
			_wi_FacialEye_CloseHalf != -1 &&
			_wi_FacialEye_Half != -1
		) {
			float w_c, w_i2h;
			w_c = eyeOpRateWB<0.01f ? 1 : 0;
			w_i2h = max(0, -eyeTrackPos.y) * _eyeTrackD_HalfRate;	// 下を向いている時は半目にする

			_body.weight[_wi_FacialEye_Close]     = w_c;
			_body.weight[_wi_FacialEye_CloseHalf] = (1f-w_c) * (1f-eyeOpRateWB);
			_body.weight[_wi_FacialEye_Idle]      = (1f-w_c) * eyeOpRateWB      * (1f-w_i2h);
			_body.weight[_wi_FacialEye_Half]      = (1f-w_c) * eyeOpRateWB      * w_i2h;
		}

		// 口回りの表情
		if (
			_wi_FacialMouth_Close != -1 &&
			_wi_FacialMouth_E != -1 &&
			_wi_FacialMouth_A != -1 &&
			_wi_FacialMouth_O != -1
		) {
			_body.weight[_wi_FacialMouth_Close] = mouthType==0 ? 1 : 0;
			_body.weight[_wi_FacialMouth_E]     = mouthType==1 ? 1 : 0;
			_body.weight[_wi_FacialMouth_A]     = mouthType==2 ? 1 : 0;
			_body.weight[_wi_FacialMouth_O]     = mouthType==3 ? 1 : 0;
		}

		// 本体の更新
		_body.autoUpdate = false;
		_body.update();
	}


	//-------------------------------------- private メンバ --------------------------------------

	float eyeBlinkTCnt_, eyeBlinkTMax_; 	//!< 自動まばたき用時間カウント
	Unity.Mathematics.Random eyeBlinkRnd_;	//!< 自動まばたき用乱数
	bool _isInisilized = false;				//!< 初期化済みか否か

	// ウェイトインデックス
	int _wi_EyeTrack_C,			//!< 視線操作:中央
		_wi_EyeTrack_L,			//!< 視線操作:左
		_wi_EyeTrack_R,			//!< 視線操作:右
		_wi_EyeTrack_U,			//!< 視線操作:上
		_wi_EyeTrack_D,			//!< 視線操作:下
		_wi_FacialEye_Idle,			//!< 目回り表情:通常
		_wi_FacialEye_Close,		//!< 目回り表情:閉じ
		_wi_FacialEye_CloseHalf,	//!< 目回り表情:閉じ掛け
		_wi_FacialEye_Half,			//!< 目回り表情:ジト目(まばたき中の閉じ掛けではなく、脱力によるもの)
		_wi_FacialMouth_Close,	//!< 口回り表情:閉じ
		_wi_FacialMouth_E,		//!< 口回り表情:発音E (通常時)
		_wi_FacialMouth_A,		//!< 口回り表情:発音A
		_wi_FacialMouth_O;		//!< 口回り表情:発音O

	void Update() {
		if (!autoUpdate) return;
#if UNITY_EDITOR
		// Editor停止中は、こちらのUpdate経由では更新しない
		if (!Application.isPlaying) return;
#endif

		if (_body == null) return;
		update(Time.deltaTime);
	}

	/** 初期化処理。対象のアニメーション名から操作対象のウェイト番号を名前解決する */
	void setup() {
		if (_body == null) return;

		Func<string, int> searchWeightIdx = nameFlt => {
			var poses = _body.MD.poseSets;
			var reg = new Regex(nameFlt);
			for (int i=0; i<poses.Length; ++i)
				if (reg.IsMatch( poses[i].name )) return i;
			return -1;
		};

		_wi_EyeTrack_C = searchWeightIdx(_an_EyeTrack_C);
		_wi_EyeTrack_L = searchWeightIdx(_an_EyeTrack_L);
		_wi_EyeTrack_R = searchWeightIdx(_an_EyeTrack_R);
		_wi_EyeTrack_U = searchWeightIdx(_an_EyeTrack_U);
		_wi_EyeTrack_D = searchWeightIdx(_an_EyeTrack_D);
		_wi_FacialEye_Idle      = searchWeightIdx(_an_FacialEye_Idle);
		_wi_FacialEye_Close     = searchWeightIdx(_an_FacialEye_Close);
		_wi_FacialEye_CloseHalf = searchWeightIdx(_an_FacialEye_CloseHalf);
		_wi_FacialEye_Half      = searchWeightIdx(_an_FacialEye_Half);
		_wi_FacialMouth_Close = searchWeightIdx(_an_FacialMouth_Close);
		_wi_FacialMouth_E     = searchWeightIdx(_an_FacialMouth_E);
		_wi_FacialMouth_A     = searchWeightIdx(_an_FacialMouth_A);
		_wi_FacialMouth_O     = searchWeightIdx(_an_FacialMouth_O);

		eyeBlinkRnd_ = new Unity.Mathematics.Random(
			(uint)UnityEngine.Random.Range(int.MinValue,int.MaxValue)
		);
		eyeBlinkTMax_ = lerp(_eyeBlink_MinSepT, _eyeBlink_MaxSepT, eyeBlinkRnd_.NextFloat(1));
		eyeBlinkTCnt_ = 0;

		_isInisilized = true;
	}


	//--------------------------------------------------------------------------------------------

#if UNITY_EDITOR
	void editorUpdate() {
		if (!autoUpdate) return;
		if (Application.isPlaying) return;
		if (_body == null) return;
		setup();		// エディタ停止中は毎フレーム初期化
		update(0);
	}

	void OnEnable() {
		if (!Application.isPlaying)
			UnityEditor.EditorApplication.update += editorUpdate;
	}

	void OnDisable() {
		if (!Application.isPlaying)
			UnityEditor.EditorApplication.update -= editorUpdate;
	}

	[CustomEditor(typeof(SimpleController))]
	sealed class CustomInspector : Editor {
		public override VisualElement CreateInspectorGUI() {
			var rootElem = new VisualElement();
			var vtree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( makePathByRelPath("SimpleController.uxml") );
			vtree.CloneTree(rootElem);

			// コントローラを割り当てる

			// 操作対象
			var tgtField = rootElem.Q<ObjectField>("Target");
			tgtField.BindProperty( serializedObject.FindProperty("_body") );

			// マッピング先アニメーション名リスト
			var manField = rootElem.Q<Foldout>("MappingAnimNameFO");
			Action<string, string> addANField = (lbl, propPath) => {
				var field = new TextField(lbl);
				field.BindProperty(serializedObject.FindProperty(propPath));
				manField.Add(field);
			};
			addANField( "視線操作:中央",		"_an_EyeTrack_C" );
			addANField( "視線操作:左",			"_an_EyeTrack_L" );
			addANField( "視線操作:右",			"_an_EyeTrack_R" );
			addANField( "視線操作:上",			"_an_EyeTrack_U" );
			addANField( "視線操作:下",			"_an_EyeTrack_D" );
			addANField( "目回り表情:通常",		"_an_FacialEye_Idle" );
			addANField( "目回り表情:閉じ",		"_an_FacialEye_Close" );
			addANField( "目回り表情:閉じ掛け",	"_an_FacialEye_CloseHalf" );
			addANField( "目回り表情:ジト目",	"_an_FacialEye_Half" );
			addANField( "口回り表情:閉じ",		"_an_FacialMouth_Close" );
			addANField( "口回り表情:発音E",		"_an_FacialMouth_E" );
			addANField( "口回り表情:発音A",		"_an_FacialMouth_A" );
			addANField( "口回り表情:発音O",		"_an_FacialMouth_O" );

			// 下向き時の半目具合
			rootElem.Q<Slider>("EyeTrackD_HalfRate")
				.BindProperty(serializedObject.FindProperty("_eyeTrackD_HalfRate"));

			// まばたき間隔とカーブ
			rootElem.Q<FloatField>("EyeBlink_MinSepT")
				.BindProperty(serializedObject.FindProperty("_eyeBlink_MinSepT"));
			rootElem.Q<FloatField>("EyeBlink_MaxSepT")
				.BindProperty(serializedObject.FindProperty("_eyeBlink_MaxSepT"));
			rootElem.Q<CurveField>("EyeBlink_AniCrv")
//				.BindProperty(serializedObject.FindProperty("_eyeBlink_AniCrv"));
				.value= ((SimpleController)target)._eyeBlink_AniCrv;

			// EyeTrack
			var etProp = serializedObject.FindProperty("eyeTrackPos");
			var etField = rootElem.Q<Vector2Field>("EyeTrack");
			etField.BindProperty( etProp );

			// EyeTrackArea
			var etaField = rootElem.Q<VisualElement>("EyeTrack_Area");
			var etcField = rootElem.Q<VisualElement>("EyeTrack_Center");
			Action updateEtcField = () => {
				serializedObject.Update();
				var a = etProp.vector2Value;
				a.y = -a.y;
				etcField.transform.position = a * 80;
			};
			etField.RegisterValueChangedCallback( _ => updateEtcField() );
			updateEtcField();
			Action<float2> updateEtaField = pos => {
				pos -= 107;
				pos = clamp( pos/80, -1, 1 );
				pos.y = -pos.y;
				etProp.vector2Value = pos;
				serializedObject.ApplyModifiedProperties();
			};
			etaField.RegisterCallback<MouseDownEvent>(e => {
				if ((e.pressedButtons & 0x1) != 0) updateEtaField(e.localMousePosition);
			});
			etaField.RegisterCallback<MouseMoveEvent>(e => {
				if ((e.pressedButtons & 0x1) != 0) updateEtaField(e.localMousePosition);
			});


			return rootElem;
		}

		/** 自分自身のCSファイルパスを取得する処理 */
		static string getSelfCSFilePath(
			[CallerFilePath] string sourceFilePath = ""		//呼び出し元のファイルのパス
		) => new Uri(Application.dataPath)
			.MakeRelativeUri(new Uri(sourceFilePath))
			.ToString();

		/** 自分自身のCSファイルからの相対パスで、パスを構築する */
		static string makePathByRelPath( string relPath ) {
			var selfPath = getSelfCSFilePath();
			return selfPath.Substring(0, selfPath.LastIndexOf('/')+1) + relPath;
		}
	}
#endif
}

}