using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace FaceAnimator.Core {

/**
 * FaceAnimatorを使用するための、ランタイム時データ本体
 */
sealed class RuntimeData {
	//-------------------------------------- public メンバ ---------------------------------------

	public float[] weight = null;		//!< ブレンド用のウェイト

	public MasterData MD => _md;		//!< 参照しているマスターデータ

	/** 初期化する。これは何度でも呼べる */
	public void setup(MasterData md, Transform root) {
		_md = md;
		_root = root;

		// ウェイトリストのサイズが合っていない場合は初期化
		if (weight == null || weight.Length != md.poseSets.Length) {
			weight = new float[md.poseSets.Length];
			weight[0] = 1;
		}

		// 操作対象への参照を初期化
		if (_ctrlTgts==null || _ctrlTgts.Length!=md.paths.Length)
			_ctrlTgts = new CtrlTgt[md.paths.Length];
		for (int j=0; j<md.paths.Length; ++j) {
			var i = md.paths[j];
			var trans = root.Find( i.name );
			if (
				( i.ctrlMode & (
					MasterData.CtrlMode.Transform_Pos |
					MasterData.CtrlMode.Transform_Rot |
					MasterData.CtrlMode.Transform_Scl
				) ) != 0
			) {
				_ctrlTgts[j].reset( i.ctrlMode, trans );
			} else if ( i.ctrlMode == MasterData.CtrlMode.Renderer_Enable ) {
				Renderer a = trans.GetComponent<MeshRenderer>();
				if (a==null) a = trans.GetComponent<SkinnedMeshRenderer>();
				_ctrlTgts[j].reset( i.ctrlMode, a );
			} else {
				throw new SystemException( "Invalid CtrlMode:" + i.name + ":" + i.ctrlMode );
			}
		}
	}

	/** 現在のウェイト設定で表示をポーズを反映する */
	public void update() {
		if (_md == null) {UnityEngine.Debug.LogError( "not initialized" ); return;}

		// 再更新が必要か否かをチェック。Editor停止中の場合は、自動で再更新する。
		if (
			weight.Length != _md.poseSets.Length ||
			_ctrlTgts.Length != _md.poseSets[0].datas.Length
		) {
			if (!Application.isPlaying) {
				refresh();
			} else {
				UnityEngine.Debug.LogError( "need refresh" );
				return;
			}
		}

		for (int j=0; j<_ctrlTgts.Length; ++j) {

			// ウェイトを考慮して合成。
			var data = Unity.Mathematics.float4x4.zero;
			for (int i=0; i<weight.Length; ++i) {
				var w = weight[i];
				if (w<0.0001f) continue;

				data += _md.poseSets[i].datas[j] * w;
			}

			// ウェイトの正規化
			if (data.c1.w < 0.00001f) continue;
			data /= data.c1.w;

			// 目標へ反映
			var ct = _ctrlTgts[j];
			if ((ct.ctrlMode & MasterData.CtrlMode.Transform_Pos) != 0)
				ct.transform.localPosition = data.c0.xyz;
			if ((ct.ctrlMode & MasterData.CtrlMode.Transform_Rot) != 0)
				ct.transform.localRotation = Quaternion.Euler( data.c1.xyz );
			if ((ct.ctrlMode & MasterData.CtrlMode.Transform_Scl) != 0)
				ct.transform.localScale = data.c2.xyz;
			if ((ct.ctrlMode & MasterData.CtrlMode.Renderer_Enable) != 0)
				ct.renderer.enabled = 0.5f < data.c0.w;
		}
	}

	/** 今の設定のまま再セットアップする。これは結構重いしヒープも食うので注意すること */
	public void refresh() {
		if (_md == null) {UnityEngine.Debug.LogError( "not initialized" ); return;}
		setup(_md, _root);
	}


	//-------------------------------------- private メンバ --------------------------------------

	/** 操作対象 */
	struct CtrlTgt {
		public MasterData.CtrlMode ctrlMode;
		public Transform transform;
		public Renderer renderer;

		// 初期化処理。一応これは何度でも呼べるが、最初に一度だけ呼ぶこと
		public void reset(MasterData.CtrlMode ctrlMode, Transform tgt)
			{this.ctrlMode=ctrlMode; transform=tgt;}
		public void reset(MasterData.CtrlMode ctrlMode, Renderer tgt)
			{this.ctrlMode=ctrlMode; renderer=tgt;}
	}

	MasterData _md = null;			//!< マスターデータ
	Transform _root = null;			//!< 操作対象のルート
	CtrlTgt[] _ctrlTgts;			//!< 操作対象への参照のリスト


	//--------------------------------------------------------------------------------------------
}

}