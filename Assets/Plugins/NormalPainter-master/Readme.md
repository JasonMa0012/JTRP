# NormalPainter
[English](https://translate.google.com/translate?sl=ja&tl=en&u=https://github.com/unity3d-jp/NormalPainter) (by Google Translate)  
![](https://user-images.githubusercontent.com/1488611/27468607-b3e9e4d0-5825-11e7-954d-fca1a7a50417.gif)

Unity 上でポリゴンモデルの法線編集を可能にするツールです。

Unity 2017.1 系以上、Windows (64bit) or Mac、**D3D11 世代以降の Graphics API** の環境で動作します。
(ターゲットプラットフォームが Standalone ではない場合、D3D9 世代に機能が限定されて正常動作しなくなることがあるのでご注意ください)

## 使い方
- [NormalPainter.unitypackage](https://github.com/unity3d-jp/NormalPainter/releases/download/20180116/NormalPainter.unitypackage) をプロジェクトにインポート
  - Unity 2018.3 以降の場合、このリポジトリを直接インポートすることもできます。プロジェクト内にある Packages/manifest.json をテキストエディタで開き、"dependencies" に以下の行を加えます。
  > "com.utj.normalpainter": "https://github.com/unity3d-jp/NormalPainter.git",

- Window -> Normal Painter でツールウィンドウを開く
- MeshRenderer か SkinnedMeshRenderer を持つオブジェクトを選択するとツールウィンドウに "Add NormalPainter" が出てくるのでそれでコンポーネントを追加


## 実装されている機能
- 頂点、法線、タンジェント等の可視化
- 法線のペイント、回転、スケーリング等 (上の動画参照)
  - 選択範囲によるマスキング
- 法線のミラーリング (Misc -> Mirroring)
- 法線の転写 (Edit -> Projection)
  - 各頂点から法線方向にレイを飛ばし、指定オブジェクトに当たった位置の法線を拾ってきます

- import / export
  - 法線 <-> 頂点カラーの相互変換
  - 法線をテクスチャとしてエクスポート
  - 法線マップを頂点の法線としてインポート
  - .obj ファイルとしてエクスポート


## License
[MIT](LICENSE.txt)
