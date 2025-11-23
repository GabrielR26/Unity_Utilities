
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class NodeEditorWindow : EditorWindow
{
	Vector2 offset = Vector2.zero;
	float zoom = 1f;
	protected List<Node> nodes = new();
	protected Node currentNode = null;
	protected List<NodeConnection> connections = new();

	void OnEnable()
	{
		EditorUtility.SetDirty(this);
	}

	void OnGUI()
	{
		DrawGrid(20, 0.05f, Color.grey);
		DrawGrid(100, 0.15f, Color.grey);

		DrawNodes();
		DrawConnections();
		DrawUtilities();

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

	void DrawUtilities()
	{
		GUILayout.Label($"Zoom : {zoom:0.00}");
		GUILayout.Label($"Offset : {offset}");
		GUILayout.Label($"Mouse : {Event.current.mousePosition}");

		Handles.color = Color.red;
		Handles.DrawSolidDisc(Event.current.mousePosition, Vector3.forward, 3);
		Handles.color = Color.white;
	}

	void DrawNodes()
	{
		for (int i = 0; i < nodes.Count; i++)
			nodes[i].Draw(offset, zoom);
	}

	void DrawConnections()
	{
		for (int i = 0; i < connections.Count; i++)
			connections[i].Draw(offset, zoom);
	}

	void HandleNodeEvents(Event _event)
	{
		for (int i = nodes.Count - 1; i >= 0; i--) // Reverse loop for overlapping nodes
		{
			nodes[i].HandleEvents(_event);
			//if (_guiChanged)
			//	GUI.changed = true;
		}
	}

	void HandleEvents(Event _event)
	{
		Vector2 _drag = Vector2.zero;

		switch (_event.type)
		{
			case EventType.ContextClick:
				ShowWindowContextMenu(_event.mousePosition);
				_event.Use();
				break;

			case EventType.MouseDrag:
				if (_event.button == 0 || _event.button == 2)
				{
					_drag = _event.delta;
					offset += _drag;
					_event.Use();
				}
				break;

			case EventType.ScrollWheel:
				float _oldZoom = zoom;
				zoom = Mathf.Clamp(zoom - (_event.delta.y * 0.02f), 0.1f, 2.2f);

				Vector2 _mousePos = _event.mousePosition;
				Vector2 _deltaPos = _mousePos - offset;
				offset += _deltaPos - _deltaPos * (zoom / _oldZoom);

				_event.Use();
				break;

			case EventType.KeyDown:
				if (_event.keyCode == KeyCode.Delete && currentNode != null)
				{
					NodeDeleted(currentNode);
				}
				_event.Use();
				break;
		}
	}

	void ShowWindowContextMenu(Vector2 _mousePosition)
	{
		GenericMenu _menu = new GenericMenu();
		_menu.AddItem(new GUIContent("Reset"), false, () => ResetWindow());
		_menu.AddItem(new GUIContent("Add Node"), false, () => AddNode(NodeEditorWindowHelper.ScreenToWindowPosition(_mousePosition, offset, zoom)));
		_menu.ShowAsContext();
	}

	void AddNode(Vector2 _position)
	{
		Node _node = new Node(_position, 200, 100, $"Node {nodes.Count}");
		nodes.Add(_node);
		_node.OnDeleted += NodeDeleted;
		_node.OnSelected += NodeSelected;
	}

	void NodeDeleted(Node _node)
	{
		string _title = "Delete selected node ?";
		string _message = $"{titleContent} : {_node.Title} \n\nYou cannot undo the delete node action.";
		if (EditorUtility.DisplayDialog(_title, _message, "Delete", "Cancel"))
		{
			if (currentNode == _node)
				currentNode = null;
			nodes.Remove(_node);
		}
	}

	void NodeSelected(Node _node)
	{
		if (currentNode == _node)
			return;

		if (currentNode != null)
			currentNode.IsSelected = false;
		currentNode = _node;
		currentNode.IsSelected = true;
	}

	void ResetWindow()
	{
		zoom = 1;
		offset = Vector2.zero;
	}
}


public class Node
{
	public event Action<Node> OnDeleted;
	public event Action<Node> OnSelected;

	public Rect rect = Rect.zero;
	public string title = string.Empty;
	Vector2 offset = Vector2.zero;
	float zoom = 0f;
	bool isNodeDragged = false;
	bool isPinDragged = false;
	bool isSelected = false;
	List<NodePin> pins = new();

	public string Title => title;
	public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

	public Node(Vector2 _position, float _width, float _height, string _title)
	{
		rect = new Rect(_position.x, _position.y, _width, _height);
		title = _title;
		pins.Add(new NodePin());
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

		for (int i = 0; i < pins.Count; i++)
		{
			NodePin _pin = pins[i];
			_pin.Draw(_zoomedRect.center, _zoom);
		}
	}

	public void HandleEvents(Event _event)
	{
		Vector2 _mousePos = NodeEditorWindowHelper.ScreenToWindowPosition(_event.mousePosition, offset, zoom);

		switch (_event.type)
		{
			case EventType.MouseDown:
				// Left click in node bonds
				if (_event.button == 0)
				{
					for (int i = 0; i < pins.Count; i++)
					{
						NodePin _pin = pins[i];
						Debug.Log($"{_pin.Rect.position}, {_pin.Rect.xMin} - {_pin.Rect.xMax}");
						if (_pin.Rect.Contains(_event.mousePosition))
						{	
							isPinDragged = true;
							_event.Use();
						}
					}
					if (rect.Contains(_mousePos))
					{
						OnSelected?.Invoke(this);
						isNodeDragged = true;
						_event.Use();
					}
					else
						isNodeDragged = false;
				}
				break;

			case EventType.ContextClick:
				if (rect.Contains(_mousePos))
				{
					ShowNodeContextMenu();
					_event.Use();
				}
				break;

			case EventType.MouseUp:
				isNodeDragged = false;
				break;

			case EventType.MouseDrag:
				if (isNodeDragged)
				{
					Drag(_event.delta);
					_event.Use();
				}
				break;
		}
	}

	void ShowNodeContextMenu()
	{
		GenericMenu _menu = new GenericMenu();
		_menu.AddItem(new GUIContent("Delete"), false, () => DeleteNode());
		_menu.ShowAsContext();
	}

	void DeleteNode()
	{
		OnDeleted?.Invoke(this);
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

		Vector2 _mousePos = NodeEditorWindowHelper.ScreenToWindowPosition(Event.current.mousePosition, offset, zoom);
		//Vector2 _start = startPin != null ? startPin.Center : _mousePos;
		//Vector2 _end = endPin != null ? endPin.Center : _mousePos;
		//Vector2 _dir = (_end - _start).normalized;
		//float _dist = Vector2.Distance(_start, _end) * 0.5f;
		//Vector2 _startTangent = _start + _dist * _dir;
		//Vector2 _endTangent = _end + _dist * -_dir;

		//float _bezierWidth = 3f;
		//isHovered = ConnectionIsHovered(_mousePos, _start, _end, _dist, _startTangent, _endTangent, _bezierWidth);
		//_bezierWidth = isHovered ? _bezierWidth * 2 : _bezierWidth;

		//Handles.BeginGUI();
		//Handles.DrawBezier(_start, _end, _startTangent, _endTangent, Color.cyan, null, _bezierWidth);
		//Handles.EndGUI();
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
				//if (_e.button == 0 && startPin.Rect.Contains(NodeEditorWindowHelper.ScreenToWindowPosition(_e.mousePosition, offset, zoom)) && !inDragging) // Drag from pin
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
	Rect rect = Rect.zero;
	Vector2 size = Vector2.zero;
	NodeConnection connection = null;
	float radius = 0f;
	bool inputPin = false;

	public Rect Rect => rect;
	public NodeConnection Connection { get { return connection; } set { value = connection; } }

	public NodePin(bool _inputPin = true, float _radius = 10f)
	{
		radius = _radius;
		size = Vector2.one * radius * 2f;
		inputPin = _inputPin;
	}

	public void Draw(Vector2 _position, float _zoom)
	{
		rect.size = size * _zoom;
		rect.position = new Vector2(_position.x - size.x, _position.y - size.y);

		Handles.DrawSolidDisc(_position, Vector3.forward, radius * _zoom);
	}
}

public class NodeField
{
	Rect rect;
}

// TODO NODE-EDITOR : UPDATE
/// systeme load/unload via json
/// pair ScriptObj - node
/// vérif nbr json == nbr ScriptObj
/// vérif ScriptObj modif hors Editor => apply change
/// classement editor/ScriptObj
