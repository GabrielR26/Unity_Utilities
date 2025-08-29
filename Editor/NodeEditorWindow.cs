
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class NodeEditorWindow : EditorWindow
{
	Vector2 offset = Vector2.zero;
	Vector2 drag = Vector2.zero;
	float zoom = 1f;
	protected List<Node> nodes = new();
	protected List<NodeConnection> connections = new();

	void OnEnable()
	{
		EditorUtility.SetDirty(this);
	}

	void OnGUI()
	{
		DrawGrid(20, 0.1f, Color.grey);
		DrawGrid(100, 0.2f, Color.grey);
		DrawUtilities();

		DrawNodes();
		DrawConnections();

		HandleNodeEvents(Event.current);
		HandleEvents(Event.current);

		if (GUI.changed)
			Repaint();
	}

	void DrawGrid(float _spacing, float _opacity, Color _color)
	{
		float _zoomedSpacing = _spacing * zoom;
		float _widthGap = position.width / _zoomedSpacing;
		float _heightGap = position.height / _zoomedSpacing;

		Handles.BeginGUI();
		Handles.color = new Color(_color.r, _color.g, _color.b, _opacity);
		Vector3 _newOffset = new Vector3(offset.x % _zoomedSpacing, offset.y % _zoomedSpacing, 0);

		for (int i = 0; i < _widthGap + 1; i++)
		{
			float x = i * _zoomedSpacing + _newOffset.x;
			Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, position.height + _zoomedSpacing, 0));
		}

		for (int j = 0; j < _heightGap + 1; j++)
		{
			float y = j * _zoomedSpacing + _newOffset.y;
			Handles.DrawLine(new Vector3(0, y, 0), new Vector3(position.width + _zoomedSpacing, y, 0));
		}

		Handles.color = Color.white;
		Handles.EndGUI();
	}

	protected virtual void DrawUtilities()
	{
		GUILayout.Label($"Zoom : {zoom:0.00}");
	}

	void DrawNodes()
	{
		if (nodes.Count == 0)
			return;
		for (int i = 0; i < nodes.Count; i++)
			nodes[i].Draw(offset, zoom);
	}

	void DrawConnections()
	{
		if (connections.Count == 0)
			return;
		for (int i = 0; i < connections.Count; i++)
			connections[i].Draw(offset, zoom);
	}

	void HandleEvents(Event _e)
	{
		drag = Vector2.zero;

		switch (_e.type)
		{
			case EventType.ContextClick:
				ShowContextMenu(_e.mousePosition);
				GUI.changed = true; // == Repaint
				break;

			case EventType.MouseDrag:
				if (_e.button == 0 || _e.button == 2)
				{
					drag = _e.delta;
					offset += drag;
					_e.Use();
				}
				break;

			case EventType.ScrollWheel:
				float _oldZoom = zoom;
				zoom = Mathf.Clamp(zoom - (_e.delta.y * 0.02f), 0.1f, 2.2f);

				Vector2 _mousePos = _e.mousePosition;
				Vector2 _deltaPos = _mousePos - offset;
				offset += _deltaPos - _deltaPos * (zoom / _oldZoom);

				_e.Use();
				break;
		}
	}

	void HandleNodeEvents(Event _e)
	{
		if (nodes.Count == 0)
			return;
		for (int i = nodes.Count - 1; i >= 0; i--)
		{
			bool _guiChanged = nodes[i].ProcessEvents(_e);
			if (_guiChanged)
				GUI.changed = true;
		}
	}

	void ShowContextMenu(Vector2 _mousePosition)
	{
		GenericMenu _menu = new GenericMenu();
		_menu.AddItem(new GUIContent("Add Node"), false, () => AddNode(EditoWindowUtils.ScreenToWindowPosition(_mousePosition, offset, zoom)));
		_menu.ShowAsContext();
	}

	void AddNode(Vector2 _position)
	{
		nodes.Add(new Node(_position, 200, 100, "Node"));
	}
}


public class Node
{
	public Rect rect;
	public string title;
	Vector2 offset;
	float zoom;
	bool isDragged;
	bool isSelected;

	public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

	public Node(Vector2 _position, float _width, float _height, string _title)
	{
		rect = new Rect(_position.x, _position.y, _width, _height);
		title = _title;
	}

	public void Drag(Vector2 _delta)
	{
		rect.position += _delta / zoom;
	}

	public void Draw(Vector2 _offset, float _zoom)
	{
		offset = _offset;
		zoom = _zoom;

		Rect _zoomedRect = rect;
		_zoomedRect.size *= zoom;
		_zoomedRect.position = _zoomedRect.position * zoom + offset;

		if (isSelected)
		{
			Color _prevColor = GUI.color;
			GUI.color = Color.cyan;
			GUI.Box(_zoomedRect, title, EditorStyles.helpBox);
			GUI.color = _prevColor;
		}
		else
		{
			GUI.Box(_zoomedRect, title, EditorStyles.helpBox);
		}

		Vector3 _pos = _zoomedRect.center;
		Handles.DrawSolidDisc(_pos, new Vector3(5, 5, 100), 15);
	}

	public bool ProcessEvents(Event _e)
	{
		switch (_e.type)
		{
			case EventType.MouseDown:
				if (_e.button == 0 && rect.Contains(EditoWindowUtils.ScreenToWindowPosition(_e.mousePosition, offset, zoom)))
				{
					isDragged = true;
					GUI.changed = true;
				}
				break;

			case EventType.MouseUp:
				isDragged = false;
				break;

			case EventType.MouseDrag:
				if (_e.button == 0 && isDragged)
				{
					Drag(_e.delta);
					_e.Use();
					return true;
				}
				break;
		}

		return false;
	}
}

public class NodeConnection
{
	public static event Action onDestroy;
	public static event Action onConnected;

	NodePin startPin;
	NodePin endPin;
	Vector2 offset;
	float zoom;
	bool isDragged;
	bool isHovered;
	bool inDragging;

	public NodeConnection(NodePin _startPin)
	{
		startPin = _startPin;
	}

	public void Draw(Vector2 _offset, float _zoom)
	{
		offset = _offset;
		zoom = _zoom;

		if (startPin == null && endPin == null)
		{
			onDestroy?.Invoke();
			return;
		}

		Vector2 _mousePos = EditoWindowUtils.ScreenToWindowPosition(Event.current.mousePosition, offset, zoom);
		Vector2 _start = startPin != null ? startPin.Center : _mousePos;
		Vector2 _end = endPin != null ? endPin.Center : _mousePos;
		Vector2 _dir = (_end - _start).normalized;
		float _dist = Vector2.Distance(_start, _end) * 0.5f;
		Vector2 _startTangent = _start + _dist * _dir;
		Vector2 _endTangent = _end + _dist * -_dir;

		float _bezierWidth = 3f;
		isHovered = ConnectionIsHovered(_mousePos, _start, _end, _dist, _startTangent, _endTangent, _bezierWidth);
		_bezierWidth = isHovered ? _bezierWidth * 2 : _bezierWidth;

		Handles.BeginGUI();
		Handles.DrawBezier(_start, _end, _startTangent, _endTangent, Color.cyan, null, _bezierWidth);
		Handles.EndGUI();
	}

	bool ConnectionIsHovered(Vector2 _mousePos, Vector2 _start, Vector2 _end, float _dist, Vector2 _startTangent, Vector2 _endTangent, float _bezierWidth)
	{
		int _steps = Mathf.CeilToInt(_dist / 20f);
		Vector3[] _points = Handles.MakeBezierPoints(_start, _end, _startTangent, _endTangent, _steps);
		for (int i = 1; i < _steps; i++)
		{
			float dist = HandleUtility.DistancePointToLineSegment(_mousePos, _points[i - 1], _points[i]);
			if (dist <= _bezierWidth)
				return true;
		}
		return false;
	}

	public bool ProcessEvents(Event _e)
	{
		switch (_e.type)
		{
			case EventType.MouseDown:
				if (_e.button == 1 && isHovered) // Right-click on connection
				{
					onDestroy?.Invoke();
				}
				// Dans EditorWindow
				//if (_e.button == 0 && startPin.Rect.Contains(EditoWindowUtils.ScreenToWindowPosition(_e.mousePosition, offset, zoom)) && !inDragging) // Drag from pin
				//{
				//	startPin = null;
				//	inDragging = true;
				//}
				break;
		}
		return false;
	}
}

public class NodePin
{
	Rect rect;
	NodeConnection connection;
	Texture2D circleTex;
	bool inputPin;

	public Vector2 Center => rect.center;
	public Rect Rect => rect;
	public NodeConnection Connection { get { return connection; } set { value = connection; } }


	public NodePin(Vector2 _pos, bool _inputPin = true)
	{
		rect = new Rect(_pos, Vector2.one * 10);
		inputPin = _inputPin;
		SetCircleTexture();
	}

	void SetCircleTexture()
	{
		if (circleTex == null)
		{
			circleTex = EditorGUIUtility.Load("builtin skins/darkskin/images/Knob.psd") as Texture2D;

			if (circleTex == null)
			{
				circleTex = new Texture2D(1, 1);
				circleTex.SetPixel(0, 0, Color.white);
				circleTex.Apply();
			}
		}
		return;
	}
}

public class NodeField
{
	Rect rect;
}

public static class EditoWindowUtils
{
	public static Vector2 ScreenToWindowPosition(Vector2 _screen, Vector2 _offset, float _zoom)
	{
		return (_screen - _offset) / _zoom;
	}
}

// TODO NODE-EDITOR : UPDATE
/// systeme load/unload via json
/// pair ScriptObj - node
/// vérif nbr json == nbr ScriptObj
/// vérif ScriptObj modif hors Editor => apply change
/// classement editor/ScriptObj