using UnityEngine;
using System.Collections;
#if UNITY_EDITOR	
using UnityEditor;
#endif

public class BaseLoader : MonoBehaviour {

	const string kAssetBundlesPath = "/AssetBundles/";

	// Use this for initialization.
	IEnumerator Start ()
	{
		yield return StartCoroutine(Initialize() );
	}

	// Initialize the downloading url and AssetBundleManifest object.
	protected IEnumerator Initialize()
	{
		// Don't destroy the game object as we base on it to run the loading script.
		DontDestroyOnLoad(gameObject);
		
#if UNITY_EDITOR
		//Debug.Log ("-------BaceLoader.cs--------We are " + (AssetBundleManager.Instance.ABM_00_SimulateAssetBundleInEditor ? "in Editor simulation mode" : "in normal mode") );
#endif

		string platformFolderForAssetBundles = 
#if UNITY_EDITOR
			GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
			GetPlatformFolderForAssetBundles(Application.platform);
#endif

		// Set base downloading url.
		string relativePath = GetRelativePath();
		AssetBundleManager.BaseDownloadingURL = relativePath + kAssetBundlesPath + platformFolderForAssetBundles + "/";

		// Initialize AssetBundleManifest which loads the AssetBundleManifest object.
		var request = AssetBundleManager.Instance.ABM_02_Initialize(platformFolderForAssetBundles);
		if (request != null)
			yield return StartCoroutine(request);
	}

	public string GetRelativePath()
	{
		string serverPath = "";
		serverPath="http://192.168.117.17";
		if (Application.isEditor)
		{	//return "file://" +  System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
			//return Application.streamingAssetsPath;
				//return "http://192.168.117.10";
			return serverPath;
		}else if (Application.isWebPlayer)
		{	
				//return System.IO.Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/")+ "/StreamingAssets";
				return serverPath;
		}else if (Application.isMobilePlatform || Application.isConsolePlatform)
		{	
				//return Application.streamingAssetsPath;
				return serverPath;
		}else // For standalone player.
		{
				//return "file://" +  Application.streamingAssetsPath;
				return serverPath;
		}
	}

#if UNITY_EDITOR
	public static string GetPlatformFolderForAssetBundles(BuildTarget target)
	{
		switch(target)
		{
		case BuildTarget.Android:
			return "Android";
		case BuildTarget.iOS:
			return "iOS";
		case BuildTarget.WebPlayer:
			return "WebPlayer";
		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			return "Windows";
		case BuildTarget.StandaloneOSXIntel:
		case BuildTarget.StandaloneOSXIntel64:
		case BuildTarget.StandaloneOSXUniversal:
			return "OSX";
			// Add more build targets for your own.
			// If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
		default:
			return null;
		}
	}
#endif

	static string GetPlatformFolderForAssetBundles(RuntimePlatform platform)
	{
		switch(platform)
		{
		case RuntimePlatform.Android:
			return "Android";
		case RuntimePlatform.IPhonePlayer:
			return "iOS";
		case RuntimePlatform.WindowsWebPlayer:
		case RuntimePlatform.OSXWebPlayer:
			return "WebPlayer";
		case RuntimePlatform.WindowsPlayer:
			return "Windows";
		case RuntimePlatform.OSXPlayer:
			return "OSX";
			// Add more build platform for your own.
			// If you add more platforms, don't forget to add the same targets to GetPlatformFolderForAssetBundles(BuildTarget) function.
		default:
			return null;
		}
	}

	protected IEnumerator Load (string assetBundleName, string assetName )
	{
		Debug.Log("-------BaceLoader.cs---------Start to load " + assetName + " at frame " + Time.frameCount);

		// Load asset from assetBundle.
		AssetBundleLoadAssetOperation request = AssetBundleManager.Instance.ABM_10_LoadAssetAsync(assetBundleName, assetName, typeof(GameObject) );
		if (request == null)
			yield break;
		yield return StartCoroutine(request);

		Debug.Log(" -----------BaceLoader.cs------------request= "+request+"--- request.GetAsset<GameObject> ()="+request.GetAsset<GameObject> ());
		// Get the asset.
		GameObject prefab = request.GetAsset<GameObject> ();
		Debug.Log(assetName + (prefab == null ? " isn't" : " is")+ " loaded successfully at frame " + Time.frameCount );

		if (prefab != null)
			Debug.Log(" ---------------BaceLoader.cs----------------- Instantiate  prefab="+prefab);
		GameObject.Instantiate(prefab);
		//instanceRef=GameObject.Instantiate(prefab);
	}
	protected IEnumerator LoadToInstance (string assetBundleName, string assetName )
	{

		Debug.Log("-------BaceLoader.cs---------Start to load " + assetName + " at frame " + Time.frameCount);

		// Load asset from assetBundle.
		AssetBundleLoadAssetOperation request = AssetBundleManager.Instance.ABM_10_LoadAssetAsync(assetBundleName, assetName, typeof(GameObject) );
		if (request == null)
			yield break;
		yield return StartCoroutine(request);

		Debug.Log(" -----------BaceLoader.cs------------request= "+request+"--- request.GetAsset<GameObject> ()="+request.GetAsset<GameObject> ());
		// Get the asset.
		GameObject prefab = request.GetAsset<GameObject> ();
		Debug.Log(assetName + (prefab == null ? " isn't" : " is")+ " loaded successfully at frame " + Time.frameCount );

		if (prefab != null)
			Debug.Log(" ---------------BaceLoader.cs----------------- Instantiate  prefab="+prefab);

		//GameObject.Instantiate(prefab);
		GameObject instanceRef=GameObject.Instantiate(prefab);
		instanceRef.transform.position = new Vector3 (-10000, -10000, 0);
		////http://qiita.com/chiepomme/items/2bccc5c6f5b803df8e57      3. IEnumerator#Current を使用する
		yield return instanceRef;
		//instanceRef=GameObject.Instantiate(prefab);
	}

	protected IEnumerator LoadLevel (string assetBundleName, string levelName, bool isAdditive)
	{
		Debug.Log("-------BaceLoader.cs--------Start to load scene " + levelName + " at frame " + Time.frameCount);

		// Load level from assetBundle.
		AssetBundleLoadOperation request = AssetBundleManager.LoadLevelAsync(assetBundleName, levelName, isAdditive);
		if (request == null)
			yield break;
		yield return StartCoroutine(request);

		// This log will only be output when loading level additively.
		Debug.Log("-------BaceLoader.cs--------Finish loading scene " + levelName + " at frame " + Time.frameCount);
	}

	// Update is called once per frame
	protected void Update () {
	}
}
