using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysCloth.Core.Constraint {
	
	/** 拘束条件のinterface */
	public interface IConstraint {
		// 与えられたパラメータで拘束条件をする意味があるか否かをチェックする
		public bool isValid();
		// 拘束条件を解決する処理
		public float solve(float sqDt, float lambda);
	}

}
