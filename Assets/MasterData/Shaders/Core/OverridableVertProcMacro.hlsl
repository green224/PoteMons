//
// 頂点処理をカスタムしたい場合に上書きする用のマクロの定義。
//
// ここで定義しているマクロを、読み込み前に定義しておくと、
// 呼び出し側の好きなように頂点処理を変更することができる。
// class method の override 的なノリで使う
//


// 頂点処理に加工を挟みたい場合はこれらのDefineを定義する

// オブジェクト位置からワールド座標などへ変換する処理
#if !defined(GET_VERTEX_POSITION_INPUTS)
	#define GET_VERTEX_POSITION_INPUTS(attr) GetVertexPositionInputs(attr.positionOS.xyz)
#endif

// オブジェクト法線からワールド法線へ変換する処理
#if !defined(GET_WORLD_NORMAL)
	#define GET_WORLD_NORMAL(attr) TransformObjectToWorldNormal(attr.normalOS)
#endif

// 頂点バッファの拡張
#if !defined(ADDITIONAL_ATTRIBUTES)
	#define ADDITIONAL_ATTRIBUTES
#endif

// 頂点シェーダからピクセルシェーダへ渡すバッファの拡張
#if !defined(ADDITIONAL_VARYINGS)
	#define ADDITIONAL_VARYINGS
#endif
