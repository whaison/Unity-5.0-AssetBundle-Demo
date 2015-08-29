using System.Diagnostics;
using UnityEditor;

public class RunMoreUnity {

	#if UNITY_EDITOR_OSX

	[MenuItem("File/Start One More Unity")]
	private static void StartNewUnity() {
		var p = new ProcessStartInfo();
		//p.FileName = "/Applications/Unity/Unity.app/Contents/MacOS/Unity";
		p.FileName = "/Applications/Unity5.1.2f1/Unity5.1.2f1.app/Contents/MacOS/Unity";
		///Applications/Unity5.1.2f1/Unity5.1.2f1.app
		p.UseShellExecute = true;
		p.CreateNoWindow = true;
		Process.Start(p);
	}

	#endif
}