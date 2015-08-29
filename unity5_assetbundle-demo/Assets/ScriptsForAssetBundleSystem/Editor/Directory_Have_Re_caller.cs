using UnityEngine;
using System;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
public class Directory_Have_Re_caller  {

	public static List<string> OutOfContentsFolderList;
	public static List<string> OutOfContentsExtentionList;
	public static List<string> OutOfContentsList;


	public static List<string> HDfilePassNameList;

	/// <summary>
	/// ##########################################################
	/// ---------------------------------------------------------------------------------------------------
	/// "HDpath" folder ReCalling .. folder tree under of folder . 探すフォルダパス 再帰して中身のフォルダまで下ります。
	/// ---------------------------------------------------------------------------------------------------
	/// "OutOfFolderList" is Out Of Contents Folder List. for example add("temp")  Un Call "temp_1234folderdir". 除外フォルダ名
	/// ---------------------------------------------------------------------------------------------------
	/// "OutOfFolderList" is Out Of Contents Extention List. for example add("txt")  Un Add "1234567.txt".除外拡張子
	/// ---------------------------------------------------------------------------------------------------
	/// "OutOfList" is Out Of Contents List. for example add("temp")    Un Add "temp_1234.txt".除外ファイル名
	/// ##########################################################
	/// 
	/// </summary>
	/// <param name="HDpath">HDpath.にどこのフォルダ以下か？</param>
	/// <param name="OutOfFolderList">Out of Contens folder list.</param>
	/// <param name="OutOfExtentionList">Out of Contens extention list.</param>
	/// <param name="OutOfList">Out Contens Name of list.</param>




	public static List<string> Directory_Have_Re_callStart(string HDpath,List<string> OutOfFolderList,List<string>OutOfExtentionList,List<string>OutOfList)
	{
	

		Debug.Log ("////////////////////////////////////////////////////////////////////////////");
		Debug.Log ("////////////////////////再帰呼び出し  はじめ///////////////////////////////////");
		Debug.Log ("////////////////////////////////////////////////////////////////////////////");
		OutOfContentsFolderList= new List<string> ();
		if (OutOfFolderList != null) {
			OutOfContentsFolderList = OutOfFolderList;
		}
		OutOfContentsExtentionList= new List<string> ();
		if (OutOfExtentionList != null) {
			OutOfContentsExtentionList = OutOfExtentionList;
		}
		OutOfContentsList= new List<string> ();
		if (OutOfList != null) {
			OutOfContentsList = OutOfList;
		}



		HDfilePassNameList = new List<string> ();
		Directory_Have_Re_call(HDpath);
		Debug.Log ("////////////////////////////////////////////////////////////////////////////");
		Debug.Log ("////////////////////////再帰呼び出し　おわり///////////////////////////////////");
		Debug.Log ("////////////////////////////////////////////////////////////////////////////");
		return HDfilePassNameList;
	}
	public static void Directory_Have_Re_call(string dir) 
	{

		string[] files = Directory.GetFiles(dir);
		foreach (string f in files) {
			bool CheckOK;

			CheckOK=CheckFileName(f);

			if (CheckOK == true) {
				//TextAssetList.Add (t);
				Debug.Log("Directory_Have_Re_call ファイル追加します！！f="+f);
				//fileNameList.Add (fileName);
				HDfilePassNameList.Add (f);
			}

		}

		string[] dirs = Directory.GetDirectories(dir);
		foreach (string d in dirs) {


			bool CheckOK = false;
			CheckOK = CheckDirName (d);

			if (CheckOK == true) {
				Debug.Log ("Directory_Have_Re_call ディレクトリ追従します！！d=" + d);
				Directory_Have_Re_call (d);
			} else {
				Debug.Log ("Directory_Have_Re_call ディレクトリd=" + d+ "   #########   フォルダー再帰から除外します。");
			}


		}
	}
	public static bool CheckFileName(string f){
		bool CheckOK=true;
		string[] filepassNameExArr= f.Split("."[0]);

		string Extention=filepassNameExArr [filepassNameExArr.Length - 1];
		//Debug.Log ("Extention="+Extention);

		string[] filepassNameArr= f.Split("/"[0]);

		string fileName=filepassNameArr [filepassNameArr.Length - 1];


		//除外する拡張子
		for(int i = 0; i < OutOfContentsExtentionList.Count; i++)
		{
			int CheckStrLength=OutOfContentsExtentionList [i].Length;
			//string nowCheckStr=fileName.Substring(0,CheckStrLength);
			//Debug.Log ("nowCheckStr="+nowCheckStr);

			//Debug.Log ("ディレクトリチェック　OutOfContentsList ["+i+"]="+OutOfContentsList [i]+"  と　nowCheckStr="+nowCheckStr);
			if (Extention == OutOfContentsExtentionList [i]) {
				Debug.Log ("------------------------------除外する拡張子ファイル発見+"+Extention+"    fileName="+fileName);
				CheckOK = false;
			} else {
				//CheckOK = true;
			}
			//Debug.Log("OutOfContentsFolderList: " + i);
		}
		//除外するファイル名
		for(int i = 0; i < OutOfContentsList.Count; i++)
		{
			int CheckStrLength=OutOfContentsList [i].Length;
			string nowCheckStr=fileName.Substring(0,CheckStrLength);
			//Debug.Log ("nowCheckStr="+nowCheckStr);

			//Debug.Log ("ディレクトリチェック　OutOfContentsList ["+i+"]="+OutOfContentsList [i]+"  と　nowCheckStr="+nowCheckStr);
			if (nowCheckStr == OutOfContentsList [i]) {
				Debug.Log ("------------------------------除外するファイル発見+"+nowCheckStr+"    fileName="+fileName);
				CheckOK = false;
			} else {
				//CheckOK = true;
			}
			//Debug.Log("OutOfContentsFolderList: " + i);
		}
			
		return CheckOK;
	}
	//public static List<string> OutOfContentsFolderList;
	//public static List<string> OutOfContentsExtentionList;
	//public static List<string> OutOfContentsList;

	public static bool CheckDirName(string d){
		bool CheckOK = true;
		string[] dirpassNameArr= d.Split("/"[0]);

		string dirName=dirpassNameArr [dirpassNameArr.Length - 1];


		string tempCheckStr=dirName.Substring(0,4);



		//OutOfContentsFolderList.Count
		//除外するフォルダー
		for(int i = 0; i < OutOfContentsFolderList.Count; i++)
		{
			int CheckStrLength=OutOfContentsFolderList [i].Length;
			string nowCheckStr=dirName.Substring(0,CheckStrLength);
			//Debug.Log ("nowCheckStr="+nowCheckStr);

			//Debug.Log ("ディレクトリチェック　OutOfContentsFolderList ["+i+"]="+OutOfContentsFolderList [i]+"  と　nowCheckStr="+nowCheckStr);
			if (nowCheckStr == OutOfContentsFolderList [i]) {
				Debug.Log ("------------------------------除外するフォルダ発見+"+nowCheckStr+"    dirName="+dirName);
				CheckOK = false;
			} else {
				//CheckOK = true;
			}
			//Debug.Log("OutOfContentsFolderList: " + i);
		}


		return CheckOK;
	}
}







