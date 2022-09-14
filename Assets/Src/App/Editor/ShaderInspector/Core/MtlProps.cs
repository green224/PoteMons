using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;


namespace App.Editor.ShaderInspector.Core {

	/**
	 * マテリアルを操作するためのモジュール。
	 */
	sealed class MtlProps {
		// ------------------------------------- public メンバ ----------------------------------------

		readonly public MaterialEditor editor;
		readonly public MaterialProperty[] props;
		readonly public Material[] mtls;

		public MtlProps(MaterialEditor mtlEdt, MaterialProperty[] props) {
			editor = mtlEdt;
			this.props = props;
			mtls = mtlEdt.targets.Cast<Material>().ToArray();
		}

		/** インデクサでFindPropertyを呼び出す */
		public MaterialProperty this[string name] {get{
			foreach (var i in props) if (i.name == name) return i;
			return null;
		}}


		// 各種のプロパティを表示する
		public MaterialProperty draw_Prop(string propName, string dispName=null) {
			if (!getPropWithExistWarn(out var prop, propName)) return null;
			dispName = dispName ?? prop.displayName;
			editor.ShaderProperty(prop, dispName);
			return prop;
		}
		public MaterialProperty draw_Tex(string propName, string dispName=null, bool scaleOffset=true) {
			if (!getPropWithExistWarn(out var prop, propName)) return null;
			dispName = dispName ?? prop.displayName;
			editor.TextureProperty(prop, dispName, scaleOffset);
			return prop;
		}
		public MaterialProperty draw_Col(string propName, string dispName=null, bool useHdrAlpha=false) {
			if (!getPropWithExistWarn(out var prop, propName)) return null;
			dispName = dispName ?? prop.displayName;

//			using (new EditorGUILayout.HorizontalScope()) {
				editor.ColorProperty(prop, dispName);

				// HDR用αコントローラを表示する場合は、ここで表示
				if (useHdrAlpha) {
//					var lastIndentLv = EditorGUI.indentLevel;
//					EditorGUI.indentLevel = 0;
					var val = prop.colorValue;
					using (new MixedValueScope(prop))
					using (var cc = new EditorGUI.ChangeCheckScope()) {
//						EditorGUILayout.Space( 10, false );
//						EditorGUILayout.LabelField( "α値", GUILayout.Width(24) );
//						val.a = EditorGUILayout.FloatField( GUIContent.none, val.a, GUILayout.Width(60) );

						++EditorGUI.indentLevel;
						val.a = EditorGUILayout.FloatField( "HDR α値", val.a );
						--EditorGUI.indentLevel;

						if (cc.changed) {
							prop.colorValue = val;
						}
					}
//					EditorGUI.indentLevel = lastIndentLv;
				}
//			}

			return prop;
		}
		public MaterialProperty draw_Vector2(string propName, string dispName=null) {
			if (!getPropWithExistWarn(out var prop, propName)) return null;
			dispName = dispName ?? prop.displayName;
			var val = (Vector2)prop.vectorValue;

			using (new MixedValueScope(prop))
			using (var cc = new EditorGUI.ChangeCheckScope()) {
				val = EditorGUILayout.Vector2Field( dispName, val );

				if (cc.changed) {
					prop.vectorValue = val;
				}
			}
			return prop;
		}
		public MaterialProperty draw_KeywordEnum(
			string propName,
			string[] EnumNames,
			string[] EnumDispNames,
			string dispName=null
		) {
			if (!getPropWithExistWarn(out var prop, propName)) return null;
			dispName = dispName ?? prop.displayName;
			var val = (int)prop.floatValue;

			using (new MixedValueScope(prop))
			using (var cc = new EditorGUI.ChangeCheckScope()) {
				var newVal = EditorGUILayout.Popup(
					dispName,
					val,
					EnumDispNames
				);

				if (cc.changed) {
					prop.floatValue = newVal;
					regiUndo( "change keyword enum:" + val + "->" + newVal );
					setKeywordEnable(
						false, (propName + "_" + EnumNames[val]).ToUpper(), false
					);
					setKeywordEnable(
						true, (propName + "_" + EnumNames[newVal]).ToUpper(), false
					);
				}
			}
			return prop;
		}


		/** 指定の名前のキーワードを有効・無効にする */
		public void setKeywordEnable(
			bool isEnable,
			string name,
			bool withRecordUndo=true
		) {
			// Undoを登録する
			// 参考：https://light11.hatenadiary.com/entry/2018/10/17/222641
			if (withRecordUndo)
				regiUndo( (isEnable?"enable":"disable") + " keyword:" + name );

			// キーワードを設定
			foreach (var i in mtls) {
				if (isEnable)	i.EnableKeyword(name);
				else			i.DisableKeyword(name);
			}
		}

		public void regiUndo(string name) => editor.RegisterPropertyChangeUndo(name);


		// --------------------------------- private / protected メンバ -------------------------------

		bool getPropWithExistWarn(out MaterialProperty prop, string name) {
			prop = this[name];
			if (prop == null) {
				Debug.LogWarning("Property [" + name + "] is not exist");
				return false;
			}
			return true;
		}


		// --------------------------------------------------------------------------------------------
	}

}
