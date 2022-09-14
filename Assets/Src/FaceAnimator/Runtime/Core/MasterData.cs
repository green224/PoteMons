using System;
using UnityEngine;

using System.Linq;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace FaceAnimator.Core {

/**
 * FaceAnimatorを使用するための、顔1つ当たりのマスターデータ
 */
sealed class MasterData : ScriptableObject {

	//--------------------------- インスペクタに公開しているフィールド ---------------------------

	[SerializeField] Path[] _paths;			//!< 操作対象のパスのリスト
	[SerializeField] PoseSet[] _poseSets;	//!< ポーズのリスト


	//-------------------------------------- public メンバ ---------------------------------------

	/**
	 * 操作対象のモード。
	 * リフレクションは遅いので、enumで分岐するためにここに定義。
	 *
	 * 同一のコンポーネントに対して複数の値を編集する事があるので、
	 * 複数の値をフラグで持てるようにしている。(現状Transformのみ)
	 */
	[Flags]
	public enum CtrlMode : int {
		Transform_Pos = 1,
		Transform_Rot = 1<<1,
		Transform_Scl = 1<<2,

		Renderer_Enable = 1<<3,
	}

	/** 操作対象までのパスの情報 */
	[Serializable]
	public sealed class Path {
		public string name;			//!< パス本体。スラッシュ区切りで階層を表現する
		public CtrlMode ctrlMode;	//!< 操作対象のモード

		public Path(string name, CtrlMode ctrlMode) {
			this.name = name;
			this.ctrlMode = ctrlMode;
		}
	}

	/** Dataを操作対象の数だけ纏めた、ポーズ一つ分の情報 */
	[Serializable]
	public sealed class PoseSet {
		public string name;			//!< ポーズ名

		/**
		 * 操作対象の数だけDataをリスト化したもの。Pathの数と同じであること。
		 * 補間の処理の関係や、高速化のために4x4行列で管理する。
		 * 操作対象ボーンでない場合は、0クリアしておけばOK
		 *
		 * transform_pos = (e00,e10,e20)
		 * transform_rot = (e01,e11,e21)
		 * transform_scl = (e02,e12,e22)
		 * renderer_enable = 0.5 < e30
		 * 使用するか否か = e31 [0 or 1]
		 */
		public float4x4[] datas;

		public PoseSet(string name, float4x4[] datas) {
			this.name = name;
			this.datas = datas;
		}
	}


	public Path[] paths => _paths;				//!< 操作対象のパスのリスト
	public PoseSet[] poseSets => _poseSets;		//!< ポーズのリスト


	/** 操作対象のパスのリストを初期化。ポーズ情報はすべてクリアされる */
	public void setup( Path[] paths ) {
		_paths = paths;
		_poseSets = new PoseSet[0];
	}

	/** ポーズを追加する */
	public void addPose( PoseSet pose ) {
		if (_paths==null || _poseSets==null) throw new InvalidProgramException();
		
		// バリデーションしておく
		if (pose.datas.Length != _paths.Length) throw new InvalidProgramException();
//		foreach (var i in pose.datas) {
//			if (i. != _paths.Length) throw new InvalidProgramException();
//		}

		// 結合。非効率だけどこれはランタイムじゃないので許容
		_poseSets = _poseSets.Concat( new []{pose} ).ToArray();
	}


	//-------------------------------------- private メンバ --------------------------------------
	//--------------------------------------------------------------------------------------------
}

}