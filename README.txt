AssetBundle 5.0のデモでは、我々は新しいAssetBundleシステムで何ができるかを実証します。
このデモを実行するには、Unityの5.0ベータ版21以上にしてください。

説明：
1。

TestScenesフォルダの下の3シーンがそこにいます：

1。
 AssetLoader.unityがAssetBundleから通常のアセットをロードする方法を示し、LoadAssets.csスクリプトを参照してください。
2。
SceneLoader.unityがAssetBundleからシーンをロードする方法を示し、LoadScenes.csスクリプトを参照してください。
3。

 VariantLoader.unityバリアントAssetBundleをロードする方法を示し、LoadVariants.csスクリプトを参照してください。
我々は、仮想資産と同じ結果を達成するためにAssetBundleバリアントを使用しています。デモでは、我々が構築します
1.「My Assets HD "フォルダ "variant/myassets.hd" AssetBundleへ。
2.「My Assets SD」フォルダ "variant/myassets.hd" AssetBundleへ。
3. "variants/variant-scene.unity3d”のなかに”variant-scene.unity" に実際に複合的にAssetBundleに依存するAssetBundleです。

アセットは正確にこれらのフォルダに一致していることを確認してください。
これらの二つの変種AssetBundles内のオブジェクトは、パイプラインを構築Unityが確保されている全く同じ内部IDを持つことになりますので、
別のバリアント拡張のAssetBundlesで任意にスイッチアウトすることができます。
ここでは、ファイル拡張子「HD」と「SD」は、私たちがバリアントを呼んでいるものです。あなたはLoadVariants.cs上にあるアクティブなバリアントを変更することができます。
また、アクティブな変形例によれば、正しいAssetBundleを解決する方法を確認するために）AssetBundleManager.RemapVariantName（を参照してください。
リマインダー：AssetBundle変異体は、エディタのシミュレーションと互換性がありません。

2。

自動的に依存AssetBundlesをダウンロード
新しいビルドシステムでは、AssetBundles間の依存性は、単一のマニフェストAssetBundleで追跡されます。だから、実行時にすべての依存関係を取得することができ、ベースURLに自動的にすべての依存関係をダウンロードしてください。
詳細については、デモでAssetBundleManager.LoadDependencies（）を確認してください。

3。

AssetBundlesメニューの下の3メニュー項目がそこにいます：
1。
 エディタでAssetBundleシミュレーションを制御するために使用される「AssetBundlesをシミュレート」。
エディタプレイモードでは、実際にそれらを構築​​することなく、AssetBundlesをシミュレートすることができます。
2。
 単にBuildPipeline.BuildAssetBundlesを呼び出す」AssetBundlesの構築」（）AssetBundle UIから設定されているAssetBundlesを構築するために、詳細についてはBuildScript.BuildAssetBundles（）を確認します。
3。
StreamingAssetsフォルダにAssetBundlesその後、詳細についてはBuildScript.BuildPlayer（）をチェックし、ビルド設定に応じてプレイヤーデータを構築するコピー」プレーヤーの構築」。

4。
ローディングAssetBundleとその依存関係の取りAssetBundleManagerクラスは、それが含まれています：
1。
初期化（）
AssetBundleマニフェストオブジェクトを初期化します。
2。
LoadAssetAsync（）
与えられたAssetBundleからのアセットをロードし、すべての依存関係を処理します。
3。
LoadLevelAsync（）
与えられたAssetBundleから与えられたシーンをロードし、すべての依存関係を処理します。
4。
LoadDependencies（）
指定されたAssetBundleためのすべての依存AssetBundlesをロードします。
5。
BaseDownloadingURL
依存関係を自動ダウンロードするために使用されるベースのダウンロードURLを設定します。
6。
SimulateAssetBundleInEditor
エディタプレイモードでAssetBundleをシミュレートする場合に設定します。
7。
バリアント
活性な変異体を設定します。
8。
RemapVariantName（）

アクティブな変形による正しいAssetBundleを解決します。

5。
資産\ ScriptsForAssetBundleSystemフォルダの下のスクリプトは、プロジェクトのために有用である可能性があります。独自のプロジェクトに統合/コピーすること自由に感じてください。


—————————————————————————————————————————

Demo for AssetBundle 5.0 to demonstrate what we can do in the new AssetBundle system.
Please base on Unity 5.0 beta 21 or above to run this demo.

Descriptions:
1. 

There're 3 scenes under TestScenes folder:


	1.

 AssetLoader.unity demonstrates how to load a normal asset from AssetBundle, please refer to LoadAssets.cs script.
	2. 

SceneLoader.unity demonstrates how to load a scene from AssetBundle, please refer to LoadScenes.cs script.
	3.

 VariantLoader.unity demonstrates how to load variant AssetBundle, please refer to LoadVariants.cs script.
		We use AssetBundle variants to achieve the same result as virtual assets. In the demo, we build
			1. "My Assets HD" folder into "variant/myassets.hd" AssetBundle.
			2. "My Assets SD" folder into "variant/myassets.sd" AssetBundle.
			3. "variant-scene.unity" into "variants/variant-scene.unity3d" AssetBundle which actually depends on variant AssetBundle.
		Please make sure the assets exactly match in these folders. 
		The objects in these two variant AssetBundles will have the exactly same internal IDs which is ensured by Unity build pipeline, so they can be switched out arbitrarily with AssetBundles of different variant extensions.
		Here the file extension "hd" and "sd" are what we call variants. You can change the active variant which is on LoadVariants.cs.
		Please also refer to AssetBundleManager.RemapVariantName() to see how to resolve the correct AssetBundle according to the active variant.
		Reminder: AssetBundle variant is not compatible with the Editor simulation.

2. 

Download dependent AssetBundles automatically
	In the new build system, the dependencies between AssetBundles are tracked in the single manifest AssetBundle. So you can get all the dependencies at runtime, and download all the dependencies automatically with a base url. 
	Please check AssetBundleManager.LoadDependencies() in the demo for more details.
	
3. 

There're 3 menu items under the AssetBundles menu:
	1.
 "Simulate AssetBundles" which is used to control AssetBundle simulation in the Editor.
		In Editor play mode, you can simulate the AssetBundles without actually building them.
	2.
 "Build AssetBundles" which simply calls BuildPipeline.BuildAssetBundles() to build the AssetBundles which are set from the AssetBundle UI, check BuildScript.BuildAssetBundles() for details.
	3. 
"Build Player" which copies the AssetBundles to StreamingAssets folder then build the player data according to the build settings, check BuildScript.BuildPlayer() for details.

4. 
AssetBundleManager class which takes of loading AssetBundle and its dependencies, it contains:
	1. 
Initialize()
		Initialize the AssetBundle manifest object.
	2. 
LoadAssetAsync()
		Load a given asset from a given AssetBundle and handle all the dependencies.
	3. 
LoadLevelAsync()
		Load a given scene from a given AssetBundle and handle all the dependencies.
	4. 
LoadDependencies()
		Load all the dependent AssetBundles for a given AssetBundle.
	5. 
BaseDownloadingURL
		Set the base downloading url which is used for dependencies automatic downloading.
	6. 
SimulateAssetBundleInEditor
		Set if simulating AssetBundle in Editor play mode.
	7. 
Variants
		Set the active variant.
	8. 
RemapVariantName()
		
Resolve the correct AssetBundle according to the active variant.
	
5. 
The scripts under Assets\ScriptsForAssetBundleSystem folder could be useful for your project. Please feel free to copy/integrate into your own projects.
	