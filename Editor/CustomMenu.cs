
using UnityEditor;
using UnityEngine;

public class CustomMenu : MonoBehaviour
{
	[MenuItem("Tools/Restart")]
	private static void Restart()
	{
        //string _projectPath = Directory.GetCurrentDirectory();
        //string _editorPath = EditorApplication.applicationPath;

        //// Save scenes
        //EditorSceneManager.SaveOpenScenes();

        //// Quit current editor
        //EditorApplication.Exit(0);

        //// Restart current project
        //Process.Start(_editorPath, "-projectPath \"" + _projectPath + "\"");

        UnityEngine.Debug.Log("Restart => WIP");
	}
}
