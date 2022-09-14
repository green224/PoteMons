using System;
using Unity.Mathematics;
using System.Runtime.CompilerServices;



namespace IzBone.Common {
public static partial class Localization {


	// ギズモ表示のオプションウィンドウ
	public static class GizmoOptionsWindow
	{
		public const string MenuName_Open =
			target == Target.JP ? "Window/IzBone/ギズモ表示オプション" :
			"Window/IzBone/GizmoOptions";

		public const string Title =
			target == Target.JP ? "ギズモ表示オプション" :
			"GizmoOptions";

		public const string ViewToggles =
			target == Target.JP ? "表示非表示" :
			"Show/Hide";
		public const string IsShowParticleR =
			target == Target.JP ? "パーティクル半径" :
			"Particle radius";
		public const string IsShowParticleV =
			target == Target.JP ? "パーティクル速度" :
			"Particle velocity";
		public const string IsShowConnections =
			target == Target.JP ? "接続関係" :
			"Connections";
		public const string IsShowLimitAgl =
			target == Target.JP ? "角度制限" :
			"Limit rotation";
		public const string IsShowLimitPos =
			target == Target.JP ? "移動制限" :
			"Limit position";
		public const string IsShowCollider =
			target == Target.JP ? "コライダー" :
			"Collider";
	}


}}

