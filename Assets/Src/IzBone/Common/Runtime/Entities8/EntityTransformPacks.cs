
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Entities;
using Unity.Collections;

namespace IzBone.Common.Entities8 {

	/** ECSで得た結果を、マネージドTransformに反映させるために使用するバッファ */
	public sealed class EntityTransformPacks : System.IDisposable
	{
		// ------------------------------------- public メンバ ----------------------------------------

		public NativeArray<Entity> Entities => _entities;
		public TransformAccessArray Transforms => _transforms;
		public int Length => _entities.Length;


		public EntityTransformPacks(int capacity) {
			_entities = new NativeList<Entity>(capacity, Allocator.Persistent);
			_transforms = new TransformAccessArray(capacity);
		}

		/** 指定位置に要素を追加する */
		public void add(Entity entity, Transform transform) {

			// キャパシティが足りていなかったら自動拡張する
			if (_entities.Length == _transforms.capacity) {
				var newT = new TransformAccessArray(_entities.Length*2);
				for (int i=0; i<_entities.Length; ++i) {
					newT.Add( _transforms[i] );
				}
				_transforms.Dispose();
				_transforms = newT;
			}

			// 要素を追加
			_entities.Add( entity );
			_transforms.Add( transform );
		}

		/** 指定位置の要素を削除する */
		public void removeAtSwapBack(int index) {
			_entities.RemoveAtSwapBack(index);
			_transforms.RemoveAtSwapBack(index);
		}

		/** 要素を全削除する */
		public void clear() {
			_entities.Clear();
			var cap = _transforms.capacity;
			_transforms.Dispose();
			_transforms = new TransformAccessArray(cap);
		}

		public void Dispose() {
			_entities.Dispose();
			_transforms.Dispose();
		}


		// --------------------------------- private / protected メンバ -------------------------------

		NativeList<Entity> _entities;
		TransformAccessArray _transforms;


		// --------------------------------------------------------------------------------------------
	}

}
