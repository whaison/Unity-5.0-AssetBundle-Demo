using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class NamedFolderToAssetBundleNameRenamer:EditorWindow
{
	const string kAssetBundlesOutputPath = "AssetBundles";

	void OnGUI ()
	{
		GUILayout.Label ("NamedFolderToAssetBundleNameRenamer V0.1.1", EditorStyles.boldLabel);
		GUILayout.Space (10f);
		//csvAsset = EditorGUILayout.ObjectField ("CSV Text", csvAsset, typeof(TextAsset), false) as TextAsset;
		GUILayout.Label ("AssetBundle Top Dir", EditorStyles.label);
		myAssetBundleTopDir=GUILayout.TextArea (myAssetBundleTopDir, GUILayout.Width (240f));
		//if (csvAsset == null) {
		//	GUILayout.Label ("Set CSV Data. Extension need .txt");
		//}
		GUILayout.Space (10f);
		//modelAsset = EditorGUILayout.ObjectField ("3D Model", modelAsset, typeof(GameObject), false) as GameObject;
		//if (modelAsset == null) {
		//	GUILayout.Label ("Set FBX or other 3D model object.");
		//}
		GUILayout.Space (20f);
		if (GUILayout.Button ("Set AssetBundleName", GUILayout.Width (240f))) {
			DirToReCallDirAndSetAssetBundleName ();
		}
	}

	public static List<string> HDfilePassNameList;

	public static List<string> OutOfContentsFolderList;
	public static List<string> OutOfContentsExtentionList;
	public static List<string> OutOfContentsList;

	public void DirToReCallDirAndSetAssetBundleName(){

		HDfilePassNameList = new List<string> ();

		//OutOfContents=
		OutOfContentsFolderList= new List<string> ();
		OutOfContentsFolderList.Add ("temp");
		OutOfContentsExtentionList= new List<string> ();
		OutOfContentsExtentionList.Add ("meta");
		OutOfContentsList= new List<string> ();
		OutOfContentsList.Add ("temp");

		Debug.Log("DirToReCallDirAndSetAssetBundleName()--------------------------------------再帰start--------------------");
		HDfilePassNameList=Directory_Have_Re_caller.Directory_Have_Re_callStart (HDAssetBundleTopDir,OutOfContentsFolderList,OutOfContentsExtentionList,OutOfContentsList);
		Debug.Log("DirToReCallDirAndSetAssetBundleName()----------------------------------------再帰end------------------");
		Debug.Log("DirToReCallDirAndSetAssetBundleName() .HDfilePassNameList.Count="+HDfilePassNameList.Count);


		for(int i = 0; i < HDfilePassNameList.Count; i++)
		{
			Debug.Log("HDfilePassNameList["+i+"]="+HDfilePassNameList[i]);
			//HDfilePassNameList[0]=Assets/MyServerData/AssetBundles/Folder1st/Folder2nd/Folder3rd/my_12345_prefab.prefab 
			//HDfilePassNameList[1]=Assets/MyServerData/AssetBundles/Folder1st/Folder2nd/Folder3rd/my_67890_prefab.prefab
			//HDfilePassNameList[0]=/works/UnityProject/Unity-5.0-AssetBundle-Demo/unity5_assetbundle-demo/Assets/MyServerData/AssetBundles/Folder1st/Folder2nd/Folder3rd/my_12345_prefab.prefab
			//HDfilePassNameList[1]=/works/UnityProject/Unity-5.0-AssetBundle-Demo/unity5_assetbundle-demo/Assets/MyServerData/AssetBundles/Folder1st/Folder2nd/Folder3rd/my_67890_prefab.prefab

			string AssetPath;
			string AssetBundleName;

			AssetPath = "Assets"+HDfilePassNameList[i].Replace(Application.dataPath, "");
			Debug.Log("AssetPath="+AssetPath);
			AssetBundleName = HDfilePassNameList[i].Replace(HDAssetBundleTopDir, "");
			///   AssetBundleName=Folder1st/Folder2nd/Folder3rd/my_67890_prefab.prefab
			string[] filepassNameArr= AssetBundleName.Split("/"[0]);

			string tempPass="";
			for(int d = 0; d < filepassNameArr.Length;d++)
			{
				if (d < filepassNameArr.Length - 1) {
					tempPass = tempPass + filepassNameArr [d];
				}
				if (d < filepassNameArr.Length-2) {

					tempPass = tempPass + "/";
				}
			}
			AssetBundleName = tempPass;
			AssetBundleName = AssetBundleName+".unity3d";
			Debug.Log("AssetBundleName="+AssetBundleName);

			///////////////////////////////////////
			AssetBundleNameChengeOneFile (AssetPath, AssetBundleName);
			////////////////////////////////////////

		}
	}



	// アセットバンドル化するフォルダの設置場所
	private static string myAssetBundleTopDir="/MyServerData/AssetBundles/";
	private static string HDAssetBundleTopDir = Application.dataPath+myAssetBundleTopDir;     // ※大文字小文字は区別される
	private static string AssetBundleTopDir = "Assets"+myAssetBundleTopDir;     // ※大文字小文字は区別される

	[MenuItem( "Tools/AssetBundles/Build AssetBundles" )]
	public static void BuildAssetBundles()
	{
		// Choose the output path according to the build target.
		string outputPath = Path.Combine(kAssetBundlesOutputPath, GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget) );
		if (!Directory.Exists(outputPath) )
			Directory.CreateDirectory (outputPath);

		BuildPipeline.BuildAssetBundles (outputPath, 0, EditorUserBuildSettings.activeBuildTarget);
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

	/*
	public static void BuildAssetBundles()
	{
		// Choose the output path according to the build target.
		string outputPath = Path .Combine(kAssetBundlesOutputPath, "");
		if (!Directory.Exists (outputPath)) {
			Directory.CreateDirectory (outputPath);
		}

		BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
	}
	*/

	public void AssetBundleNameChengeOneFile(string AssetPath,string AssetBundleName){

		//var path = AssetDatabase.GetAssetPath(obj);
		// AssetImporterも取得
		AssetImporter importer = AssetImporter.GetAtPath(AssetPath);

		string myAssetBundleName = AssetBundleName.ToLower();

		importer.assetBundleName = myAssetBundleName;
		importer.assetBundleVariant = "";
	}
	/*
	private static void SetAssetName(Object obj)
	{
		var path = AssetDatabase.GetAssetPath(obj);
		// AssetImporterも取得
		AssetImporter importer = AssetImporter.GetAtPath(path);

		if (path.IndexOf ("Resources/") >= 0) {
			return;
		}

		string abname = path.Replace(assetTopDir, "");
		int idx = abname.LastIndexOf('.' );
		if (idx != -1)
		{
			abname = abname.Substring(0, idx) + ".unity3d";
		}
		else
		{
			abname = path;
		}
		importer.assetBundleName = abname;
		importer.assetBundleVariant = "";

	}
	*/


	//[MenuItem( "Assets/Set Assetbundle name" )]
	public static void SelectionAsset()
	{
		// Build the resource file from the active selection.
		Object[] selection = Selection.GetFiltered(typeof( Object), SelectionMode .DeepAssets);

		foreach (var obj in selection)
		{
			//SetAssetName(obj);
		}

	}



	////////////////////////////////////////////////////////////////////////////////

	#region Static
	/// <summary>
	/// Open the tool window
	/// </summary>
	[MenuItem("Tools/AssetBundles/NamedFolderToAssetBundleNameRenamer")]
	static public void OpenWindow ()
	{
		EditorWindow.GetWindow<NamedFolderToAssetBundleNameRenamer> (true, "Model Animation Splitter With AnimeController", true);
	}
	#endregion
}