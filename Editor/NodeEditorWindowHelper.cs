
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class NodeEditorWindowHelper
{
	/// <summary>
	/// Open a NodeEditorWindow : focus on it if already exist, create it otherwise
	/// </summary>
	/// <typeparam name="T"> The NodeEditorWindow subType to open </typeparam>
	/// <param name="_title"> The window's title </param>
	/// <param name="_desiredDockNextTo"> The desired EditorWindow type array to dock next to, T if null </param>
	public static void OpenWindow<T>(string _title, Type[] _desiredDockNextTo = null) where T : NodeEditorWindow
	{
		// Try get EditorWindow target
		T _editorWindowTarget = Resources.FindObjectsOfTypeAll<T>()
			.FirstOrDefault(_editorWindow => _editorWindow.titleContent.text == _title);

		if (_editorWindowTarget != null)
			_editorWindowTarget.Focus();
		else
		{
			// Other T EditorWindow as default _desiredDockNextTo
			if (_desiredDockNextTo == null)
				_desiredDockNextTo = new[] { typeof(T) };

			T _newEditorWindow = EditorWindow.CreateWindow<T>(_title, _desiredDockNextTo);
			_newEditorWindow.Focus();
		}
	}

	public static Vector2 ScreenToWindowPosition(Vector2 _screen, Vector2 _offset, float _zoom)
	{
		return (_screen - _offset) / _zoom;
	}
}
