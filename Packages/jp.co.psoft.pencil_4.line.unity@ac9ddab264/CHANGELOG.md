# Changelog - PSOFT Pencil+ 4 Line

## [4.1.0] - 2021-02-16

### Package Manager としての初回リリース

### 修正点
- プレハブ編集時にPencil+ ノードオブジェクトを生成するとき、プレハブ内ではなくシーン上にオブジェクトが生成されてしまう不具合を修正
- 特定の条件PencilLineEffect により意図せず Line List Node が生成されてしまう不具合を修正
- Unity 2019.3 以降でインスペクタ上のオブジェクトフィールドが正しく表示されない不具合を修正
- Line Node のインスペクタ表示で、選択したラインセットの Brush Settings Node が存在しない場合にエラーが発生する不具合を修正
- Line Set Node の [Reduction Settings] をオンにするたびに Reduction Settings Node が新規生成される不具合を修正
- Texture Map Node の [Texture UV] を [Screen] に設定したとき、テクスチャの垂直方向が反転してしまう不具合を修正

## [4.0.4] - 2019-10-03

### 修正点
- カメラの Culling Mask 設定をライン描画に反映するように修正
- Physical Camera の Lens Shift を使用したときにラインの描画結果がずれてしまう不具合を修正
- Pencil Line Effect の Mode を PostProcessing_RenderingEvent に設定したときにクラッシュする場合がある不具合を修正
- Line List Node のGUIを操作したときに ID が意図せず変更されてしまう不具合を修正


## [4.0.3] - 2019-06-19

### 機能追加
- ラインのみの描画結果を Render Texture に出力する Render Element 機能を追加
- 複数のオブジェクトを1つのアウトライン単位として扱うためのグループ機能を追加

### 修正点
- GUIのリスト表示の項目を複数選択できるように改良
- Line List Node のGUI表示の改良
- ビルドに失敗する不具合を修正
- インポート設定の [Read/Write Enabled] を無効したオブジェクトをシーン上に配置するとエラーが発生する場合がある不具合を修正（詳細はマニュアルのFAQをご覧ください）


## [4.0.2] - 2019-03-28

### 機能追加
- ラインの設定をインポート/エクスポートする Bridge 機能を追加
- Unity 2018.3 以降に対応
- Line Set Nodeに[Open Edge][Self Intersection]の設定を追加
- 線で構成されているメッシュの描画に対応

### 修正点
- Post-processing Stack v2 用として別々に存在していたパッケージの統合
- パッケージのフォルダ構成の修正
- Pencil+ 関連のプレハブを廃止し、各種 Node を生成するためのスクリプト機能を新たに追加
- Pencil+ 関連のノードを生成するためのメニュー表示の改善
- Pencil Line Effect コンポーネントの RequireComponent に Camera を設定
- Line List Node の Line List / Material Line Functions List と Line Node の Line Set List の削除ボタンを押したとき、意図せずオブジェクトが破棄されてしまう動作を改善
- Line List Node の Line List / Material Line Functions List と Line Node の Line Set List にオブジェクトをドラッグアンドドロップしても反映されない不具合を修正
- Material Line Functions Node の Draw Hidden Lines を使用したとき、アウトラインの判定が正常に行われなくなる不具合を修正
- Texture Map Node が正しく描画に反映されず、場合によってはクラッシュする不具合を修正
- 軽微なGUIの修正


## [4.0.1] - 2018-07-30

### 機能追加
- Post-processing Stack v2に対応

### 修正点
- ライン検出精度と処理速度の向上
- ラインのオフセットと角度に関連する一部のパラメーターの正負が反転してレンダリングに反映される不具合を修正
- Brush Detail Node の Length の最小値を1.0から0.001へ変更
- Brush Detail Node の Distortion Map の Map Amount の最大値を100から1000へ変更
- Reduction Settings Node の Refer Object にメッシュを持たないオブジェクトを設定できない不具合を修正
- Line Setのオブジェクト/マテリアル リストに、異なる親の Line Node によって管理されているオブジェクト/マテリアルを登録できな不具合を修正
- シーン上の SkinnedMeshRenderer の boneWeights が空のときにクラッシュする場合がる問題に対応
- パラメーターとして動作していなかったBrush Settings Node の Blend Mode をエディターGUIから除去
- Linear Color Space 動作時のライン出力色の調整
- Scripting Runtime Versionを.NET 4.xにしたときに発生するエラーを修正

2018年7月30日 株式会社ピー・ソフトハウス


## [4.0.0] - 2017-08-03

### 機能追加
- オブジェクト選択ウィンドウ、マテリアル選択ウィンドウを改良
- Texture Map Node を追加
- Line Node, Line Set Node のインスペクタに Color Map と Size Map の表示を追加

### 修正点
- スタンドアロンライセンスの認証方法を変更
- Line List Node の階層外にノードが存在した場合にラインの描画が正しく動作しない不具合を修正
- Line List Node の階層外のノードを Line List に設定してもインスペクターが正常に動作するように修正
- Line Node の階層外のノードを Line Set List に設定してもインスペクターが正常に動作するように修正
- ラインが全く描画されない設定の時の処理負荷の改善
- メッシュの多いシーンに対するライン描画処理負荷を軽減
- ゲームの実行中に C# スクリプトから PencilLineEffect を AddComponent した時、 LineListNode が自動的に生成されないように修正
- Unity2017 で Curve の編集が反映されない問題を修正
- 古いパッケージを適用済みのプロジェクトに対し、パッケージを再インポートした場合に発生する不具合を修正

## [4.0.0-RC1] - 2017-02-13

### 修正点
- 処理負荷の軽減
- ライン描画結果の調整
- [Line Set]の[Object List]と[Material List]において、オブジェクトやマテリアルを追加してもリストに反映されない場合がある不具合を修正
- 一部のGUI表記を修正
- 軽微な不具合を修正

## [4.0.0-Beta2] - 2016-09-26

### 初回公開
