using UnityEngine;
#if UNITY_EDITOR	
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

/*
 	In this demo, we demonstrate:
	1.	Automatic asset bundle dependency resolving & loading.
		It shows how to use the manifest assetbundle like how to get the dependencies etc.
	2.	Automatic unloading of asset bundles (When an asset bundle or a dependency thereof is no longer needed, the asset bundle is unloaded)
	3.	Editor simulation. A bool defines if we load asset bundles from the project or are actually using asset bundles(doesn't work with assetbundle variants for now.)
		With this, you can player in editor mode without actually building the assetBundles.
	4.	Optional setup where to download all asset bundles
	5.	Build pipeline build postprocessor, integration so that building a player builds the asset bundles and puts them into the player data (Default implmenetation for loading assetbundles from disk on any platform)
	6.	Use WWW.LoadFromCacheOrDownload and feed 128 bit hash to it when downloading via web
		You can get the hash from the manifest assetbundle.
	7.	AssetBundle variants. A prioritized list of variants that should be used if the asset bundle with that variant exists, first variant in the list is the most preferred etc.

このデモでは、実証します：
1。
自動アセットバンドルの依存関係の解決＆ロード。
これは、依存関係などを取得する方法の様子とマニフェストassetbundleを使用する方法を示しています

2。
アセットバンドルの自動アンロード（アセットバンドルまたはその依存関係が不要になった場合、アセットバンドルがアンロードします）

3。
エディタシミュレーション。我々はプロジェクトからアセットバンドルをロードしたり、実際にアセットバンドルを使用している場合はブール値を定義し（今のassetbundleバリアントでは動作しません。）
これにより、実際にassetBundlesを構築することなく、エディタモードでプレイすることができます。

4。
ここで、すべてのアセットバンドルをダウンロードするには、オプションのセットアップ
が必要です
5。
プレイヤーを構築する（任意のプラットフォーム上のディスクからassetbundlesをロードするためのデフォルトimplmenetation）アセットバンドルを構築し、プレイヤーデータにそれらを置くように、パイプラインのビルドポストプロセッサ、統合を構築

6。
WWW.LoadFromCacheOrDownloadを使用し、Web経由でダウンロードするときに、それに128ビットのハッシュを養います
あなたはマニフェストassetbundleからハッシュを得ることができます。

7。
AssetBundleの複合体。その例にアセットバンドルがリストの最初の複合体が最も好ましいなどで、存在する場合に使用されるべきである複合体の優先順位リスト

*/

// Loaded assetBundle contains the references count which can be used to unload dependent assetBundles automatically.
public class LoadedAssetBundle
{
	public AssetBundle m_AssetBundle;
	public int m_ReferencedCount;
	
	public LoadedAssetBundle(AssetBundle assetBundle)
	{
		Debug.Log ("---------------------LoadedAssetBundle-(assetBundle="+assetBundle+")----------------");
		m_AssetBundle = assetBundle;
		m_ReferencedCount = 1;
	}
}

// Class takes care of loading assetBundle and its dependencies automatically, loading variants automatically.
public class AssetBundleManager : MonoBehaviour
{
	static string m_BaseDownloadingURL = "";
	static string[] m_Variants =  {  };
	static AssetBundleManifest m_AssetBundleManifest = null;
#if UNITY_EDITOR	
	static int m_SimulateAssetBundleInEditor = -1;
	const string kSimulateAssetBundles = "SimulateAssetBundles";
#endif

	static Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle> ();
	static Dictionary<string, WWW> m_DownloadingWWWs = new Dictionary<string, WWW> ();
	static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string> ();
	static List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation> ();
	static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]> ();


	#region Singleton
	private static AssetBundleManager instance = null;

	public static AssetBundleManager Instance {
		get {
			if (instance == null) {
				instance = FindObjectOfType<AssetBundleManager> ();
			}
			return instance;
		}
	}
	#endregion

	void Awake ()
	{
		if (this != Instance) {
			Destroy (this.gameObject);
			return;
		}
			
	}


	// The base downloading url which is used to generate the full downloading url with the assetBundle names.
	public static string BaseDownloadingURL
	{
		get { return m_BaseDownloadingURL; }
		set { m_BaseDownloadingURL = value; 
			Debug.Log ("----------ABM_001----------set-BaseDownloadingURL-(BaseDownloadingURL="+BaseDownloadingURL+")----------------");
		}
	}

	// Variants which is used to define the active variants.
	public static string[] Variants
	{
		get { return m_Variants; }
		set { m_Variants = value; 
			Debug.Log ("-----------ABM_002---------set-m_Variants-(m_Variants="+m_Variants+")----------------");
		}
	}

	// AssetBundleManifest object which can be used to load the dependecies and check suitable assetBundle variants.
	public static AssetBundleManifest AssetBundleManifestObject
	{
		set {m_AssetBundleManifest = value; 
			Debug.Log ("---------ABM_003-----------set-m_AssetBundleManifest-(m_AssetBundleManifest="+m_AssetBundleManifest+")----------------");
		}
	}

#if UNITY_EDITOR
	// Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
	public static bool ABM_00_SimulateAssetBundleInEditor 
	{
		get
		{
			if (m_SimulateAssetBundleInEditor == -1)
				m_SimulateAssetBundleInEditor = EditorPrefs.GetBool(kSimulateAssetBundles, true) ? 1 : 0;
			
			return m_SimulateAssetBundleInEditor != 0;
		}
		set
		{
			int newValue = value ? 1 : 0;
			if (newValue != m_SimulateAssetBundleInEditor)
			{

				m_SimulateAssetBundleInEditor = newValue;
				EditorPrefs.SetBool(kSimulateAssetBundles, value);
				Debug.Log ("--------------------set-SimulateAssetBundleInEditor-(kSimulateAssetBundles="+kSimulateAssetBundles+")----------------");

			}
		}
	}
#endif

	// Get loaded AssetBundle, only return vaild object when all the dependencies are downloaded successfully.
	public LoadedAssetBundle ABM_01_GetLoadedAssetBundle (string assetBundleName, out string error)
	{
		Debug.Log ("--------------------ABM_01_GetLoadedAssetBundle-(assetBundleName="+assetBundleName+" error=)----------------");
		if (m_DownloadingErrors.TryGetValue (assetBundleName, out error)) {
			return null;
			Debug.Log ("--------------------ABM_01_GetLoadedAssetBundle-(assetBundleName="+assetBundleName+" error=)----------------");
		}
		LoadedAssetBundle bundle = null;
		m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
		if (bundle == null) {
			return null;
			Debug.Log ("--------------------ABM_01_GetLoadedAssetBundle-(assetBundleName="+assetBundleName+" bundle="+bundle+")----------------");
		}
		// No dependencies are recorded, only the bundle itself is required.
		string[] dependencies = null;
		if (!m_Dependencies.TryGetValue (assetBundleName, out dependencies)) {
			return bundle;
			Debug.Log ("--------------------ABM_01_GetLoadedAssetBundle-(assetBundleName=" + assetBundleName + " dependencies=" + dependencies + ")----------------");
		}
		// Make sure all dependencies are loaded
		foreach(var dependency in dependencies)
		{
			if (m_DownloadingErrors.TryGetValue(assetBundleName, out error) )
				return bundle;
			Debug.Log ("---------ABM_01_GetLoadedAssetBundle  for-(assetBundleName=" + assetBundleName + " error=" + error + ")----------------");
			// Wait all the dependent assetBundles being loaded.
			LoadedAssetBundle dependentBundle;
			m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
			if (dependentBundle == null) 
			{
				return null;
				Debug.Log ("---------ABM_01_GetLoadedAssetBundle  for-(dependency=" + dependency + " dependentBundle=" + dependentBundle + ")----------------");
			}
		}
		Debug.Log ("-----OK----ABM_01_GetLoadedAssetBundle  for-(bundle=" + bundle + ")----------------");
		return bundle;
	}

	// Load AssetBundleManifest.
	public AssetBundleLoadManifestOperation ABM_02_Initialize (string manifestAssetBundleName)
	{
		var go = new GameObject("AssetBundleManager", typeof(AssetBundleManager));
		DontDestroyOnLoad(go);
		Debug.Log ("---------ABM_02_Initialize  manifestAssetBundleName=" + manifestAssetBundleName + " go= "+go+ "----------------");
		/*
#if UNITY_EDITOR	
		// If we're in Editor simulation mode, we don't need the manifest assetBundle.
		if (ABM_00_SimulateAssetBundleInEditor)
		{	
			Debug.Log ("---------ABM_02_Initialize  ABM_00_SimulateAssetBundleInEditor=" + ABM_00_SimulateAssetBundleInEditor + ")----------------");
			return null;
		}
#endif
*/
		ABM_03_LoadAssetBundle(manifestAssetBundleName, true);
		var operation = new AssetBundleLoadManifestOperation (manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
		m_InProgressOperations.Add (operation);
		return operation;
		Debug.Log ("---------ABM_02_Initialize  operation=" + operation + ")----------------");
	}
	
	// Load AssetBundle and its dependencies.
	static protected void ABM_03_LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false)
	{
		/*
#if UNITY_EDITOR
		// If we're in Editor simulation mode, we don't have to really load the assetBundle and its dependencies.
		if (ABM_00_SimulateAssetBundleInEditor){
			Debug.Log("----------ABM_03_LoadAssetBundle-----------------assetBundleName = "+assetBundleName+"--isLoadingAssetBundleManifest= "+isLoadingAssetBundleManifest+"--------------------");
			return;
		}
			
#endif
*/
		if (!isLoadingAssetBundleManifest) {
			assetBundleName = ABM_04_RemapVariantName (assetBundleName);
			Debug.Log("----------ABM_03_LoadAssetBundle--------------RemapVariantName(---assetBundleName = "+assetBundleName+")--isLoadingAssetBundleManifest= "+isLoadingAssetBundleManifest+"--------------------");
		}
		// Check if the assetBundle has already been processed.
		Debug.Log("----------ABM_03_LoadAssetBundle----------------->>>>>>>    ABM_05_LoadAssetBundleInternal--------------");
		bool isAlreadyProcessed = ABM_05_LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);
		Debug.Log("----------ABM_03_LoadAssetBundle-----------------assetBundleName = "+assetBundleName+"--isLoadingAssetBundleManifest= "+isLoadingAssetBundleManifest+"-----isAlreadyProcessed ="+isAlreadyProcessed+"---------------");
		// Load dependencies.
		if (!isAlreadyProcessed && !isLoadingAssetBundleManifest) {
			Debug.Log ("----------ABM_03_LoadAssetBundle----------------->>>>>>>    ABM_06_LoadDependencies--------------");
			ABM_06_LoadDependencies (assetBundleName);
		}
	}
	
	// Remaps the asset bundle name to the best fitting asset bundle variant.
	static protected string ABM_04_RemapVariantName(string assetBundleName)
	{
		Debug.Log("----------ABM_04_RemapVariantName-----------------assetBundleName = "+assetBundleName+"---");
		string[] bundlesWithVariant = m_AssetBundleManifest.GetAllAssetBundlesWithVariant();

		// If the asset bundle doesn't have variant, simply return.
		if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0 )

			return assetBundleName;

		string[] split = assetBundleName.Split('.');

		int bestFit = int.MaxValue;
		int bestFitIndex = -1;
		// Loop all the assetBundles with variant to find the best fit variant assetBundle.
		Debug.Log("----------ABM_04_RemapVariantName-----------------assetBundleName = "+assetBundleName+"---bundlesWithVariant.Length= "+bundlesWithVariant.Length);
		for (int i = 0; i < bundlesWithVariant.Length; i++)
		{
			string[] curSplit = bundlesWithVariant[i].Split('.');
			if (curSplit[0] != split[0])
				continue;
			
			int found = System.Array.IndexOf(m_Variants, curSplit[1]);
			if (found != -1 && found < bestFit)
			{
				bestFit = found;
				bestFitIndex = i;
			}
		}


		if (bestFitIndex != -1)

			return bundlesWithVariant[bestFitIndex];
		else
			Debug.Log("----------ABM_04_RemapVariantName-----------------assetBundleName = "+assetBundleName+"---");
			return assetBundleName;
	}

	// Where we actuall call WWW to download the assetBundle.
	static protected bool ABM_05_LoadAssetBundleInternal (string assetBundleName, bool isLoadingAssetBundleManifest)
	{
		Debug.Log ("------------ABM_05_LoadAssetBundleInternal----------------------LoadFromCacheOrDownload--------------------- assetBundleName="+assetBundleName);
		// Already loaded.
		LoadedAssetBundle bundle = null;
		m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
		if (bundle != null)
		{
			bundle.m_ReferencedCount++;
			return true;
		}

		// @TODO: Do we need to consider the referenced count of WWWs?
		// In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
		// But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.
		if (m_DownloadingWWWs.ContainsKey(assetBundleName) )
			return true;

		WWW download = null;
		string url = m_BaseDownloadingURL + assetBundleName;
		Debug.Log ("------------ABM_05_LoadAssetBundleInternal----------------------LoadFromCacheOrDownload--------------------- url="+url);
		// For manifest assetbundle, always download it as we don't have hash for it.
		if (isLoadingAssetBundleManifest)
			download = new WWW(url);
		else
			download = WWW.LoadFromCacheOrDownload(url, m_AssetBundleManifest.GetAssetBundleHash(assetBundleName), 0); 

		m_DownloadingWWWs.Add(assetBundleName, download);

		return false;
	}

	// Where we get all the dependencies and load them all.
	static protected void ABM_06_LoadDependencies(string assetBundleName)
	{
		if (m_AssetBundleManifest == null)
		{
			Debug.LogError("--------------ABM_06_LoadDependencies----------AssetBundleManager.Instance.ABM_02_Initialize()を呼び出すことによってAssetBundleManifestを初期化してください。assetBundleName="+assetBundleName);
			//AssetBundleManager.Instance.ABM_02_Initialize (assetBundleName);
			return;
		}

		// Get dependecies from the AssetBundleManifest object..
		string[] dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
		if (dependencies.Length == 0) {
			Debug.Log ("------------ABM_06_LoadDependencies----------------------dependencies.Length=" + dependencies.Length);
			return;
		}	
		for (int i = 0; i < dependencies.Length; i++) {
			dependencies [i] = ABM_04_RemapVariantName (dependencies [i]);
			Debug.Log ("------------ABM_06_LoadDependencies--------->>>>ABM_04_RemapVariantName-------------dependencies [i] ="+dependencies [i] );
		}	
		// Record and load all dependencies.
		m_Dependencies.Add(assetBundleName, dependencies);
		for (int i = 0; i < dependencies.Length; i++) {

			ABM_05_LoadAssetBundleInternal (dependencies [i], false);
			Debug.Log ("------------ABM_06_LoadDependencies---------->>>ABM_05_LoadAssetBundleInternal------------dependencies [i] ="+dependencies [i] );
		}
		return;
	}

	// Unload assetbundle and its dependencies.
	public void ABM_07_UnloadAssetBundle(string assetBundleName)
	{
#if UNITY_EDITOR
		// If we're in Editor simulation mode, we don't have to load the manifest assetBundle.
		if (ABM_00_SimulateAssetBundleInEditor)
			Debug.Log ("------------ABM_00_SimulateAssetBundleInEditor----------------------assetBundleName ="+assetBundleName );
			return;
#endif

		//Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + assetBundleName);

		ABM_09_UnloadAssetBundleInternal(assetBundleName);
		ABM_08_UnloadDependencies(assetBundleName);

		//Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + assetBundleName);
	}

	static protected void ABM_08_UnloadDependencies(string assetBundleName)
	{
		string[] dependencies = null;
		if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies) )
			Debug.Log ("------------ABM_08_UnloadDependencies----------------------assetBundleName ="+assetBundleName );
			return;

		// Loop dependencies.
		foreach(var dependency in dependencies)
		{
			Debug.Log ("------------ABM_08_UnloadDependencies----------------------dependency ="+dependency );
			ABM_09_UnloadAssetBundleInternal(dependency);
		}

		m_Dependencies.Remove(assetBundleName);
	}

	static protected void ABM_09_UnloadAssetBundleInternal(string assetBundleName)
	{
		Debug.Log ("------------ABM_09_UnloadAssetBundleInternal----------------------assetBundleName ="+assetBundleName );
		string error;
		LoadedAssetBundle bundle = AssetBundleManager.Instance.ABM_01_GetLoadedAssetBundle(assetBundleName, out error);
		if (bundle == null)
			Debug.Log ("------------ABM_09_UnloadAssetBundleInternal----------------------bundle ="+bundle );
			return;

		if (--bundle.m_ReferencedCount == 0)
		{
			bundle.m_AssetBundle.Unload(false);
			m_LoadedAssetBundles.Remove(assetBundleName);
			//Debug.Log("AssetBundle " + assetBundleName + " has been unloaded successfully");
		}
	}

	void Update()
	{
		// Collect all the finished WWWs.
		var keysToRemove = new List<string>();
		foreach (var keyValue in m_DownloadingWWWs)
		{
			WWW download = keyValue.Value;

			// If downloading fails.
			if (download.error != null)
			{
				m_DownloadingErrors.Add(keyValue.Key, download.error);
				keysToRemove.Add(keyValue.Key);
				continue;
			}

			// If downloading succeeds.
			if(download.isDone)
			{
				Debug.Log ("------------Update----------------------download.isDone ="+download.isDone );
				//Debug.Log("Downloading " + keyValue.Key + " is done at frame " + Time.frameCount);
				m_LoadedAssetBundles.Add(keyValue.Key, new LoadedAssetBundle(download.assetBundle) );
				keysToRemove.Add(keyValue.Key);
			}
		}

		// Remove the finished WWWs.
		foreach( var key in keysToRemove)
		{
			Debug.Log ("------------Update----------------------key ="+key );
			WWW download = m_DownloadingWWWs[key];
			m_DownloadingWWWs.Remove(key);
			download.Dispose();
		}

		// Update all in progress operations
		for (int i=0;i<m_InProgressOperations.Count;)
		{
			Debug.Log ("------------Update----------------------m_InProgressOperations[i] ="+m_InProgressOperations[i] );
			if (!m_InProgressOperations[i].Update())
			{
				m_InProgressOperations.RemoveAt(i);
			}
			else
				i++;
		}
	}

	// Load asset from the given assetBundle.
	public AssetBundleLoadAssetOperation ABM_10_LoadAssetAsync (string assetBundleName, string assetName, System.Type type)
	{
		Debug.Log ("------------ABM_10_LoadAssetAsync----------------------assetBundleName ="+assetBundleName+" assetName= "+assetName +" type="+type);
		AssetBundleLoadAssetOperation operation = null;
		/*
#if UNITY_EDITOR
		if (ABM_00_SimulateAssetBundleInEditor)
		{
			Debug.Log ("------------ABM_10_LoadAssetAsync----------■------------ABM_00_SimulateAssetBundleInEditor ="+ABM_00_SimulateAssetBundleInEditor+" assetBundleName="+assetBundleName+" assetName= "+assetName +" type="+type);
			string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
			if (assetPaths.Length == 0)
			{
				Debug.Log ("------------ABM_10_LoadAssetAsync--------■-------------ABM_00_SimulateAssetBundleInEditor ="+ABM_00_SimulateAssetBundleInEditor+" assetBundleName="+assetBundleName+" assetName= "+assetName +" assetPaths.Length="+assetPaths.Length);
				Debug.LogError("There is no asset with name \"" + assetName + "\" in " + assetBundleName);
				return null;
			}

			// @TODO: Now we only get the main object from the first asset. Should consider type also.
			Object target = AssetDatabase.LoadMainAssetAtPath(assetPaths[0]);
			operation = new AssetBundleLoadAssetOperationSimulation (target);
			Debug.Log ("------------ABM_10_LoadAssetAsync---------■------------ABM_00_SimulateAssetBundleInEditor ="+ABM_00_SimulateAssetBundleInEditor+" assetBundleName="+assetBundleName+" assetName= "+assetName +" target"+target+" operation"+operation);
		}
		else
#endif
*/
		{
			Debug.Log ("------------ABM_10_LoadAssetAsync-----------else----------- assetBundleName="+assetBundleName+" assetName= "+assetName );
			ABM_03_LoadAssetBundle (assetBundleName);
			operation = new AssetBundleLoadAssetOperationFull (assetBundleName, assetName, type);

			m_InProgressOperations.Add (operation);
		}
		Debug.Log ("------------ABM_10_LoadAssetAsync------------------ assetBundleName="+assetBundleName+" assetName= "+assetName +" operation="+operation);
		return operation;
	}

	// Load level from the given assetBundle.
	static public AssetBundleLoadOperation LoadLevelAsync (string assetBundleName, string levelName, bool isAdditive)
	{
		Debug.Log ("------------ABM_10_LoadAssetAsync------------------- assetBundleName="+assetBundleName+" levelName= "+levelName +" isAdditive="+isAdditive);
		AssetBundleLoadOperation operation = null;
		/*
#if UNITY_EDITOR
		if (ABM_00_SimulateAssetBundleInEditor)
		{
			Debug.Log ("------------ABM_10_LoadAssetAsync------------ABM_00_SimulateAssetBundleInEditor="+ABM_00_SimulateAssetBundleInEditor+"------- assetBundleName="+assetBundleName+" levelName= "+levelName +" isAdditive="+isAdditive);
			string[] levelPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, levelName);
			if (levelPaths.Length == 0)
			{
				///@TODO: The error needs to differentiate that an asset bundle name doesn't exist
				//        from that there right scene does not exist in the asset bundle...
			
				Debug.LogError("There is no scene with name \"" + levelName + "\" in " + assetBundleName);
				return null;
			}

			if (isAdditive)
			{	Debug.Log ("------------ABM_10_LoadAssetAsync------------ABM_00_SimulateAssetBundleInEditor="+ABM_00_SimulateAssetBundleInEditor+"------- assetBundleName="+assetBundleName+" levelName= "+levelName +" isAdditive="+isAdditive);
				EditorApplication.LoadLevelAdditiveInPlayMode(levelPaths[0]);
			}else
			{	EditorApplication.LoadLevelInPlayMode(levelPaths[0]);
			}
			operation = new AssetBundleLoadLevelSimulationOperation();
		}
		else
#endif
*/
		{

			ABM_03_LoadAssetBundle (assetBundleName);
			operation = new AssetBundleLoadLevelOperation (assetBundleName, levelName, isAdditive);
			Debug.Log ("------------ABM_10_LoadAssetAsync------------assetBundleName="+assetBundleName+"------- levelName="+levelName+" isAdditive= "+isAdditive +" operation="+operation);

			m_InProgressOperations.Add (operation);
		}

		return operation;
	}
} // End of AssetBundleManager.