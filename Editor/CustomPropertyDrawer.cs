
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

#region CustomDraw
[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
public class SerializableDictionaryDrawer : PropertyDrawer
{
	private float lineHeight = EditorGUIUtility.singleLineHeight;
	private float paddingHeight = 3f;
	private float spacing = 5f;
	private float minButtonWidth = 20f;
	private bool foldoutExpand = true;

	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		EditorGUI.BeginProperty(_position, _label, _property);

		// Get keys/values list
		SerializedProperty _keys = _property.FindPropertyRelative("keys");
		SerializedProperty _values = _property.FindPropertyRelative("values");
		if (_keys == null || _values == null)
		{
			EditorGUI.LabelField(_position, "Failed to get keys or values list");
			EditorGUI.EndProperty();
		}

		int _keysSize = _keys.arraySize;
		float _xPos = _position.x + spacing;
		float _width = _position.width - spacing * 2;

		// Foldout button
		Rect _foldoutRect = new(_position.x - 2f, _position.y, 15f, lineHeight);
		string _foldoutIconPath = "Textures/";
		_foldoutIconPath += foldoutExpand ? "FoldoutExpand" : "FoldoutNotExpand";
		Texture2D _foldoutIcon = Resources.Load<Texture2D>(_foldoutIconPath);
		if (GUI.Button(_foldoutRect, _foldoutIcon, GUI.skin.label))
		{
			foldoutExpand = !foldoutExpand;
		}

		// Property field name
		Rect _labelRect = new(_foldoutRect.xMax, _position.y, _position.width - (minButtonWidth * 1.5f), lineHeight);
		EditorGUI.LabelField(_labelRect, _label);

		// Dictionary Count
		Rect _countRect = new(_labelRect.xMax, _position.y, minButtonWidth, lineHeight);
		EditorGUI.LabelField(_countRect, _keysSize.ToString());

		// Expand foldout or not
		if (foldoutExpand)
		{
			// Display key's field, value's field and delete button
			for (int i = 0; i < _keysSize; i++)
			{
				int _index = i;
				float _yPos = _position.y + (_index + 1) * (lineHeight + paddingHeight);
				Rect _fieldsRect = new(_xPos, _yPos, _width, lineHeight);

				float _fieldWidth = (_fieldsRect.width - (minButtonWidth + spacing * 2)) / 2f;

				Rect _keyRect = new(_fieldsRect.x, _fieldsRect.y, _fieldWidth, lineHeight);
				EditorGUI.PropertyField(_keyRect, _keys.GetArrayElementAtIndex(_index), GUIContent.none);

				Rect _valueRect = new(_keyRect.xMax + spacing, _fieldsRect.y, _fieldWidth, lineHeight);
				EditorGUI.PropertyField(_valueRect, _values.GetArrayElementAtIndex(_index), GUIContent.none);

				Rect _deleteButtonRect = new(_valueRect.xMax + spacing, _fieldsRect.y, minButtonWidth, lineHeight);
				if (GUI.Button(_deleteButtonRect, "–"))
				{
					_keys.DeleteArrayElementAtIndex(_index);
					_values.DeleteArrayElementAtIndex(_index);
					break;
				}
			}

			// Display add button
			Rect _addButtonRect = new(_xPos, _position.y + (_keysSize + 1) * (lineHeight + paddingHeight), _width, lineHeight);
			if (GUI.Button(_addButtonRect, "+ Add Entry"))
			{
				int _newIndex = _keysSize;
				_keys.InsertArrayElementAtIndex(_newIndex);
				_values.InsertArrayElementAtIndex(_newIndex);

				SerializedProperty _newKey = _keys.GetArrayElementAtIndex(_newIndex);
				// Non-duplicate key
				switch (_newKey.propertyType)
				{
					case SerializedPropertyType.String:
						string _string = "Default";
						int _index = 1;
						while (Contains(_keys, _keysSize, _string))
							_string = "Default" + _index++;
						_newKey.stringValue = _string;
						break;
					case SerializedPropertyType.Integer:
						int _int = 0;
						while (Contains(_keys, _keysSize, _int))
							_int++;
						_newKey.intValue = _int;
						break;
					case SerializedPropertyType.Float:
						float _float = 0f;
						while (Contains(_keys, _keysSize, _float))
							_float++;
						_newKey.floatValue = _float;
						break;
					case SerializedPropertyType.Enum:
						int _size = _newKey.enumNames.Length;
						int _indexEnum = 0;
						while (_indexEnum < _size && ContainsEnum(_keys, _keysSize, _indexEnum))
							_indexEnum++;
						_newKey.enumValueIndex = Mathf.Clamp(_indexEnum, 0, _size - 1);
						break;
				}

				SerializedProperty _newVal = _values.GetArrayElementAtIndex(_newIndex);
				// Default value
				switch (_newVal.propertyType)
				{
					case SerializedPropertyType.String:
						_newVal.stringValue = "Default";
						break;
					case SerializedPropertyType.Integer:
						_newVal.intValue = 0;
						break;
					case SerializedPropertyType.Float:
						_newVal.floatValue = 0;
						break;
					case SerializedPropertyType.Enum:
						_newVal.enumValueIndex = 0;
						break;
					case SerializedPropertyType.ObjectReference:
						_newVal.objectReferenceValue = null;
						break;
				}
			}
		}

		EditorGUI.EndProperty();
	}

	private static bool Contains(SerializedProperty _keys, int _keySize, string _string)
	{
		for (int i = 0; i < _keySize; i++)
		{
			if (_keys.GetArrayElementAtIndex(i).stringValue == _string)
				return true;
		}
		return false;
	}
	private static bool Contains(SerializedProperty _keys, int _keySize, int _int)
	{
		for (int i = 0; i < _keySize; i++)
		{
			if (_keys.GetArrayElementAtIndex(i).intValue == _int)
				return true;
		}
		return false;
	}
	private static bool Contains(SerializedProperty _keys, int _keySize, float _float)
	{
		for (int i = 0; i < _keySize; i++)
		{
			if (_keys.GetArrayElementAtIndex(i).floatValue == _float)
				return true;
		}
		return false;
	}
	private static bool ContainsEnum(SerializedProperty _keys, int _keySize, int _index)
	{
		for (int i = 0; i < _keySize; i++)
		{
			if (_keys.GetArrayElementAtIndex(i).enumValueIndex == _index)
				return true;
		}
		return false;
	}

	public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
	{
		SerializedProperty _keys = _property.FindPropertyRelative("keys");
		if (_keys == null)
			EditorGUI.GetPropertyHeight(_property, _label, true);
		float _height = (_keys.arraySize + 2) * (lineHeight + paddingHeight);
		return foldoutExpand ? _height : lineHeight;
	}
}

[CustomPropertyDrawer(typeof(DayTime))]
public class DayTimeDrawer : PropertyDrawer
{
	private float lineHeight = EditorGUIUtility.singleLineHeight;
	private float paddingHeight = 3f;
	private float spacing = 5f;
	private float resetButtonWidth = 50f;
	private float fieldWidth = 25f;

	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		EditorGUI.BeginProperty(_position, _label, _property);

		// Get hour and minute properties
		SerializedProperty _hour = _property.FindPropertyRelative("hour");
		SerializedProperty _minute = _property.FindPropertyRelative("minute");
		if (_hour == null || _minute == null)
		{
			EditorGUI.LabelField(_position, "Failed to get hour or minute");
			EditorGUI.EndProperty();
		}

		// Property field name
		Rect _labelRect = new(_position.x, _position.y, _position.width, lineHeight);
		EditorGUI.LabelField(_labelRect, _label);

		// Display hour's field, minute's field and reset button
		float _xPos = _position.x + spacing;
		float _yPos = _position.y + lineHeight + paddingHeight;
		float _width = _position.width - (spacing * 2);
		Rect _fieldsRect = new(_xPos, _yPos, _width, lineHeight);

		// Hour
		Rect _hourRect = new(_fieldsRect.x, _fieldsRect.y, fieldWidth, lineHeight);
		EditorGUI.PropertyField(_hourRect, _hour, GUIContent.none);

		// :
		Rect _separationRect = new(_hourRect.xMax + spacing, _fieldsRect.y, 5f, lineHeight);
		EditorGUI.LabelField(_separationRect, ":");

		// Minute
		Rect _minuteRect = new(_separationRect.xMax + spacing, _fieldsRect.y, fieldWidth, lineHeight);
		EditorGUI.PropertyField(_minuteRect, _minute, GUIContent.none);

		// Reset
		Rect _resetButtonRect = new(_minuteRect.xMax + (spacing * 3), _fieldsRect.y, resetButtonWidth, lineHeight);
		if (GUI.Button(_resetButtonRect, "Reset"))
		{
			_hour.intValue = 0;
			_minute.intValue = 0;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
	{
		float _height = (lineHeight * 2) + paddingHeight;
		return _height;
	}
}

[CustomPropertyDrawer(typeof(RangeBetweenAttribute))]
public class RangeBetweenDrawer : PropertyDrawer
{
	private float spacing = 5f;

	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		// Get RangeBetween attribute
		RangeBetweenAttribute _rangeBetween = (RangeBetweenAttribute)attribute;

		// Get min and max properties
		SerializedProperty _min = _property.FindPropertyRelative("min");
		SerializedProperty _max = _property.FindPropertyRelative("max");
		if (_min == null || _max == null)
		{
			EditorGUI.LabelField(_position, "Failed to get clampMin or clampMax. Use attribute on RangeBetween struct");
			EditorGUI.EndProperty();
		}
		float _minValue = _min.intValue;
		float _maxValue = _max.intValue;

		// Clamp
		_minValue = Mathf.Clamp(_minValue, _rangeBetween.minLimit, _maxValue);
		_maxValue = Mathf.Clamp(_maxValue, _minValue, _rangeBetween.maxLimit);

		EditorGUI.BeginProperty(_position, _label, _property);

		// Property field name
		Rect _labelRect = new(_position.x, _position.y, EditorGUIUtility.labelWidth, _position.height);
		EditorGUI.PrefixLabel(_labelRect, _label);

		// Property field minValue
		Rect _minValueRect = new Rect(_labelRect.x + _labelRect.width, _position.y, EditorGUIUtility.fieldWidth, _position.height);
		_minValue = EditorGUI.IntField(_minValueRect, (int)_minValue);

		// Property field slider
		float _sliderWidth = _position.width - _labelRect.width - (EditorGUIUtility.fieldWidth * 2) - (spacing * 2);
		float _sliderWidthClamp = Mathf.Clamp(_sliderWidth, EditorGUIUtility.fieldWidth, _sliderWidth);
		Rect _sliderRect = new(_minValueRect.x + _minValueRect.width + spacing, _position.y, _sliderWidthClamp, _position.height);
		EditorGUI.MinMaxSlider(_sliderRect, ref _minValue, ref _maxValue, _rangeBetween.minLimit, _rangeBetween.maxLimit);

		// Property field maxValue
		Rect _maxValueRect = new Rect(_sliderRect.x + _sliderRect.width + spacing, _position.y, EditorGUIUtility.fieldWidth, _position.height);
		_maxValue = EditorGUI.IntField(_maxValueRect, (int)_maxValue);

		EditorGUI.EndProperty();

		// Save
		_min.intValue = (int)_minValue;
		_max.intValue = (int)_maxValue;
	}

	public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
	{
		return EditorGUI.GetPropertyHeight(_property, _label, true);
	}
}
#endregion

#region Condition
[CustomPropertyDrawer(typeof(HideConditionAttribute))]
public class HideConditionDrawer : PropertyDrawer
{
	static readonly Dictionary<string, BooleanOperator> BooleanOperatorMap = new(){
		{ "!", BooleanOperator.Not },
		{ "==", BooleanOperator.Equal },
		{ "!=", BooleanOperator.NotEqual },
		{ "<", BooleanOperator.Less },
		{ "<=", BooleanOperator.LessEqual },
		{ ">", BooleanOperator.Greater },
		{ ">=", BooleanOperator.GreaterEqual },
		{ "&&", BooleanOperator.And },
		{ "||", BooleanOperator.Or },
		{ "^", BooleanOperator.Xor }
	};

	static readonly Dictionary<string, ArithmeticOperator> ArithmeticOperatorMap = new(){
		{ "+", ArithmeticOperator.Plus },
		{ "-", ArithmeticOperator.Minus },
		{ "*", ArithmeticOperator.Times },
		{ "/", ArithmeticOperator.Divide }
	};

	static readonly Dictionary<string, Type> EnumCache = new();

	Rect position;
	SerializedProperty property;
	GUIContent label;
	List<object> stepsResult;
	bool canHide = false;

	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		// Reset properties
		position = _position;
		property = _property;
		label = _label;
		stepsResult = new();

		// Check expression
		HideConditionAttribute _hideCondition = (HideConditionAttribute)attribute;
		try
		{
			List<string> _stepsExpression = BooleanExpressionParser.ParseBooleanExpression(_hideCondition.booleanExpression);
			for (int i = 0; i < _stepsExpression.Count; i++)
			{
				string _step = _stepsExpression[i];

				if (BooleanExpressionParser.BooleanOperatorRegex.IsMatch(_step)) // Boolean expression
				{
					string[] _splitedOperation = BooleanExpressionParser.BooleanOperatorRegex.Split(_step);
					// Always 3 item : 0{Left} 1{Operator} 2{Right}
					string _leftOperand = _splitedOperation[0]; // Can be null
					string _operator = _splitedOperation[1];
					string _rightOperand = _splitedOperation[2];

					// Boolean operator
					BooleanOperator _booleanOperator = GetBooleanOperator(_operator);
					if (_booleanOperator == BooleanOperator.None)
					{
						string _error = $"Failed to get boolean operator '{_operator}' for EditCondition of property field '{property.name}'";
						EditorGUI.LabelField(position, label.text, $"{_error}");
						Debug.LogError(_error);
						return;
					}

					// Left/Right operand
					object _leftResult = null;
					if (!string.IsNullOrWhiteSpace(_leftOperand))
						_leftResult = GetOperandValue(_leftOperand);
					object _rightResult = GetOperandValue(_rightOperand);

					bool _booleanExpressionResult = BooleanExpressionResult(_booleanOperator, _leftResult, _rightResult);
					//DebugLog.Log("TEST", $"(Boolean) {_step} : Left: '{_leftResult}'; Right: '{_rightResult}' = {_booleanExpressionResult}");
					stepsResult.Add(_booleanExpressionResult);
				}
				else // Arithmetic expression
				{
					MatchCollection _matches = BooleanExpressionParser.ArithmeticOperandRegex.Matches(_step);
					// Always 3 item : 0{Left} 1{Operator} 2{Right}
					string _leftOperand = _matches[0].Value;
					string _operator = _matches[1].Value;
					string _rightOperand = _matches[2].Value;

					// Arithmetic operator
					ArithmeticOperator _ArithmeticOperator = GetArithmeticOperator(_operator);
					if (_ArithmeticOperator == ArithmeticOperator.None)
					{
						string _error = $"Failed to get Arithmetic operator '{_operator}' for EditCondition of property field '{property.name}'";
						EditorGUI.LabelField(position, label.text, $"{_error}");
						Debug.LogError(_error);
						return;
					}

					// Left/Right operand
					object _leftResult = GetOperandValue(_leftOperand);
					object _rightResult = GetOperandValue(_rightOperand);

					double _ArithmeticExpressionResult = ArithmeticExpressionResult(_ArithmeticOperator, _leftResult, _rightResult);
					//DebugLog.Log("TEST", $"(Arithmetic) {_step} : Left: '{_leftResult}'; Right: '{_rightResult}' = {_ArithmeticExpressionResult}");
					stepsResult.Add(_ArithmeticExpressionResult);
				}
			}
		}
		catch
		{
			string _error = $"'HideCondition' attribute is invalid";
			EditorGUI.LabelField(position, label.text, _error);
			throw;
		}

		// Get final result
		canHide = (bool)stepsResult[stepsResult.Count - 1];
		if (!canHide)
			EditorGUI.PropertyField(position, property, label, true);
	}

	public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
	{
		// Reduce property's height if hidden
		return !canHide ? EditorGUI.GetPropertyHeight(_property, _label, true) : 0f;
	}


	/// <summary>
	/// Get operand's value by its type
	/// </summary>
	/// <param name="_operand"> The operand string </param>
	/// <returns> The operand's value</returns>
	/// <exception cref="Exception"> Error exception </exception>
	object GetOperandValue(string _operand)
	{
		// Boolean value
		if (BooleanExpressionParser.BooleanRegex.IsMatch(_operand))
			return Convert.ToBoolean(_operand);

		// Digit
		if (BooleanExpressionParser.DigitRegex.IsMatch(_operand))
		{
			string _digit = _operand.TrimEnd('f', 'F'); // Remove float mark
			if (float.TryParse(_digit, NumberStyles.Float, CultureInfo.InvariantCulture, out float _value))
				return _value;
			else
			{
				string _error = $"Digit operand '{_operand}' isn't supported";
				throw new Exception(_error);
			}
		}

		// Enum
		if (_operand.Contains('.')) // Always after Digit because of point
		{
			// Always 2 items: 0{EnumName} 1{EnumValue}
			string[] _splitedEnum = _operand.Split('.');
			string _enumName = _splitedEnum[0];
			string _enumValue = _splitedEnum[1];

			Type _enumType = GetEnumType(_enumName);
			if (_enumType == null)
				throw new Exception($"Enum '{_enumName}' not found");
			if (_enumType.IsEnumDefined(_enumValue))
			{
				if (Enum.TryParse(_enumType, _enumValue, true, out object _value))
					return (int)_value;
				else
				{
					string _error = $"Failed to get enum value '{_enumValue}'";
					throw new Exception(_error);
				}
			}
			else
			{
				string _error = $"Enum value '{_enumName}.{_enumValue}' is undefined";
				throw new Exception(_error);
			}
		}

		//Previous result
		if (_operand.StartsWith('#'))
		{
			string _resultIndex = _operand.TrimStart('#'); // Remove '#'
			if (int.TryParse(_resultIndex, out int _index))
			{
				if (_index < 0 || _index >= stepsResult.Count)
				{
					string _error = $"Index out of bounds '{_index}'";
					throw new Exception(_error);
				}
				return stepsResult[_index];
			}
			else
			{
				string _error = $"Failed to get previous result '{_operand}'";
				throw new Exception(_error);
			}
		}

		// Field value
		SerializedProperty _property = GetObjectProperty(property, _operand);
		if (_property == null)
			throw new Exception($"Field property '{_operand}' not found");
		return _property?.propertyType switch
		{
			SerializedPropertyType.Boolean => _property.boolValue,
			SerializedPropertyType.Integer => _property.intValue,
			SerializedPropertyType.Float => _property.floatValue,
			SerializedPropertyType.Enum => _property.enumValueIndex,
			_ => throw new Exception($"Unsupported property type for '{_operand}'")
		};
	}

	/// <summary>
	/// Get the type of an enum
	/// </summary>
	/// <param name="_enumName"> The enum's name </param>
	/// <returns> The Type if find, false otherwise </returns>
	Type GetEnumType(string _enumName)
	{
		// Search if enum cached
		if (EnumCache.TryGetValue(_enumName, out var _type))
			return _type;

		// Search in the current assembly
		_type = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(a => a.GetTypes())
			.FirstOrDefault(t => t.IsEnum && t.FullName == _enumName);

		// Cache enum
		if (_type != null)
			EnumCache[_enumName] = _type;

		return _type;
	}

	/// <summary>
	/// Get the result of a boolean expression
	/// </summary>
	/// <param name="_booleanOperator"> The boolean operator </param>
	/// <param name="_left"> The left operand of expression, can bu null </param>
	/// <param name="_right"> The right operand of expression </param>
	/// <returns> The expression's result </returns>
	bool BooleanExpressionResult(BooleanOperator _booleanOperator, object _left, object _right)
	{
		bool _result = false;
		switch (_booleanOperator)
		{
			case BooleanOperator.Not:
				_result = !Convert.ToBoolean(_right);
				break;
			case BooleanOperator.Equal:
				_result = _left.Equals(_right);
				break;
			case BooleanOperator.NotEqual:
				_result = !_left.Equals(_right);
				break;
			case BooleanOperator.Less:
				_result = Convert.ToDouble(_left) < Convert.ToDouble(_right);
				break;
			case BooleanOperator.LessEqual:
				_result = Convert.ToDouble(_left) <= Convert.ToDouble(_right);
				break;
			case BooleanOperator.Greater:
				_result = Convert.ToDouble(_left) > Convert.ToDouble(_right);
				break;
			case BooleanOperator.GreaterEqual:
				_result = Convert.ToDouble(_left) >= Convert.ToDouble(_right);
				break;
			case BooleanOperator.And:
				_result = Convert.ToBoolean(_left) && Convert.ToBoolean(_right);
				break;
			case BooleanOperator.Or:
				_result = Convert.ToBoolean(_left) || Convert.ToBoolean(_right);
				break;
			case BooleanOperator.Xor:
				_result = Convert.ToBoolean(_left) ^ Convert.ToBoolean(_right);
				break;
		}

		return _result;
	}

	/// <summary>
	/// Get the result of an arithmetic expression
	/// </summary>
	/// <param name="_booleanOperator"> The arithmetic operator </param>
	/// <param name="_left"> The left operand of expression </param>
	/// <param name="_right"> The right operand of expression </param>
	/// <returns> The expression's result </returns>
	double ArithmeticExpressionResult(ArithmeticOperator _operator, object _left, object _right)
	{
		double _result = 0;
		switch (_operator)
		{
			case ArithmeticOperator.Plus:
				_result = Convert.ToDouble(_left) + Convert.ToDouble(_right);
				break;
			case ArithmeticOperator.Minus:
				_result = Convert.ToDouble(_left) - Convert.ToDouble(_right);
				break;
			case ArithmeticOperator.Times:
				_result = Convert.ToDouble(_left) * Convert.ToDouble(_right);
				break;
			case ArithmeticOperator.Divide:
				_result = Convert.ToDouble(_left) / Convert.ToDouble(_right);
				break;
		}

		return _result;
	}

	/// <summary>
	/// Get the boolean operator's type
	/// </summary>
	/// <param name="_operator"> The operator in string </param>
	/// <returns> The BooleanOperator type if find, None otherwise </returns>
	BooleanOperator GetBooleanOperator(string _operator) => BooleanOperatorMap.TryGetValue(_operator, out var _result) ? _result : BooleanOperator.None;
	/// <summary>
	/// Get the arithmetic operator's type
	/// </summary>
	/// <param name="_operator"> The operator in string </param>
	/// <returns> The ArithmeticOperator type if find, None otherwise </returns>
	ArithmeticOperator GetArithmeticOperator(string _operator) => ArithmeticOperatorMap.TryGetValue(_operator, out var _result) ? _result : ArithmeticOperator.None;

	/// <summary>
	/// Get a serialized property on current serialized object
	/// </summary>
	/// <param name="_startedProperty"> The serialized property drawed </param>
	/// <param name="_propertyName"> The property's name to get </param>
	/// <returns> The SerializedProperty if find, null otherwise </returns>
	SerializedProperty GetObjectProperty(SerializedProperty _startedProperty, string _propertyName)
	{
		string _targetPath = _startedProperty.propertyPath;
		if (_targetPath.EndsWith(']')) // Don't use _property.isArray because it don't work with ScriptableObject, GameObject, MonoBehaviour, ...
		{
			// Replace path's last array field by condition field		
			int _indexArrayField = _targetPath.LastIndexOf(".Array.data");
			_targetPath = _targetPath.Substring(0, _indexArrayField);
		}
		// Replace path's last field by condition field
		int _indexPropertyName = _targetPath.LastIndexOf('.') + 1;
		_targetPath = _targetPath.Substring(0, _indexPropertyName) + _propertyName;
		return _startedProperty.serializedObject.FindProperty(_targetPath);
	}
}

[CustomPropertyDrawer(typeof(EditConditionAttribute))]
public class EditConditionDrawer : PropertyDrawer
{
	static readonly Dictionary<string, BooleanOperator> BooleanOperatorMap = new(){
		{ "!", BooleanOperator.Not },
		{ "==", BooleanOperator.Equal },
		{ "!=", BooleanOperator.NotEqual },
		{ "<", BooleanOperator.Less },
		{ "<=", BooleanOperator.LessEqual },
		{ ">", BooleanOperator.Greater },
		{ ">=", BooleanOperator.GreaterEqual },
		{ "&&", BooleanOperator.And },
		{ "||", BooleanOperator.Or },
		{ "^", BooleanOperator.Xor }
	};

	static readonly Dictionary<string, ArithmeticOperator> ArithmeticOperatorMap = new(){
		{ "+", ArithmeticOperator.Plus },
		{ "-", ArithmeticOperator.Minus },
		{ "*", ArithmeticOperator.Times },
		{ "/", ArithmeticOperator.Divide }
	};

	static readonly Dictionary<string, Type> EnumCache = new();

	Rect position;
	SerializedProperty property;
	GUIContent label;
	List<object> stepsResult;

	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		// Reset properties
		position = _position;
		property = _property;
		label = _label;
		stepsResult = new();

		// Check expression
		EditConditionAttribute _editCondition = (EditConditionAttribute)attribute;
		try
		{
			List<string> _stepsExpression = BooleanExpressionParser.ParseBooleanExpression(_editCondition.booleanExpression);
			for (int i = 0; i < _stepsExpression.Count; i++)
			{
				string _step = _stepsExpression[i];

				if (BooleanExpressionParser.BooleanOperatorRegex.IsMatch(_step)) // Boolean expression
				{
					string[] _splitedOperation = BooleanExpressionParser.BooleanOperatorRegex.Split(_step);
					// Always 3 item : 0{Left} 1{Operator} 2{Right}
					string _leftOperand = _splitedOperation[0]; // Can be null
					string _operator = _splitedOperation[1];
					string _rightOperand = _splitedOperation[2];

					// Boolean operator
					BooleanOperator _booleanOperator = GetBooleanOperator(_operator);
					if (_booleanOperator == BooleanOperator.None)
					{
						string _error = $"Failed to get boolean operator '{_operator}' for EditCondition of property field '{property.name}'";
						EditorGUI.LabelField(position, label.text, $"{_error}");
						Debug.LogError(_error);
						return;
					}

					// Left/Right operand
					object _leftResult = null;
					if (!string.IsNullOrWhiteSpace(_leftOperand))
						_leftResult = GetOperandValue(_leftOperand);
					object _rightResult = GetOperandValue(_rightOperand);

					bool _booleanExpressionResult = BooleanExpressionResult(_booleanOperator, _leftResult, _rightResult);
					//DebugLog.Log("TEST", $"(Boolean) {_step} : Left: '{_leftResult}'; Right: '{_rightResult}' = {_booleanExpressionResult}");
					stepsResult.Add(_booleanExpressionResult);
				}
				else // Arithmetic expression
				{
					MatchCollection _matches = BooleanExpressionParser.ArithmeticOperandRegex.Matches(_step);
					// Always 3 item : 0{Left} 1{Operator} 2{Right}
					string _leftOperand = _matches[0].Value;
					string _operator = _matches[1].Value;
					string _rightOperand = _matches[2].Value;

					// Arithmetic operator
					ArithmeticOperator _ArithmeticOperator = GetArithmeticOperator(_operator);
					if (_ArithmeticOperator == ArithmeticOperator.None)
					{
						string _error = $"Failed to get Arithmetic operator '{_operator}' for EditCondition of property field '{property.name}'";
						EditorGUI.LabelField(position, label.text, $"{_error}");
						Debug.LogError(_error);
						return;
					}

					// Left/Right operand
					object _leftResult = GetOperandValue(_leftOperand);
					object _rightResult = GetOperandValue(_rightOperand);

					double _ArithmeticExpressionResult = ArithmeticExpressionResult(_ArithmeticOperator, _leftResult, _rightResult);
					//DebugLog.Log("TEST", $"(Arithmetic) {_step} : Left: '{_leftResult}'; Right: '{_rightResult}' = {_ArithmeticExpressionResult}");
					stepsResult.Add(_ArithmeticExpressionResult);
				}
			}
		}
		catch
		{
			string _error = $"'EditCondition' attribute is invalid";
			EditorGUI.LabelField(position, label.text, _error);
			throw;
		}

		// Get final result
		bool _canEdit = (bool)stepsResult[stepsResult.Count - 1];

		// Apply
		GUI.enabled = _canEdit;
		EditorGUI.PropertyField(position, property, label, true);
	}

	/// <summary>
	/// Get operand's value by its type
	/// </summary>
	/// <param name="_operand"> The operand string </param>
	/// <returns> The operand's value</returns>
	/// <exception cref="Exception"> Error exception </exception>
	object GetOperandValue(string _operand)
	{
		// Boolean value
		if (BooleanExpressionParser.BooleanRegex.IsMatch(_operand))
			return Convert.ToBoolean(_operand);

		// Digit
		if (BooleanExpressionParser.DigitRegex.IsMatch(_operand))
		{
			string _digit = _operand.TrimEnd('f', 'F'); // Remove float mark
			if (float.TryParse(_digit, NumberStyles.Float, CultureInfo.InvariantCulture, out float _value))
				return _value;
			else
			{
				string _error = $"Digit operand '{_operand}' isn't supported";
				throw new Exception(_error);
			}
		}

		// Enum
		if (_operand.Contains('.')) // Always after Digit because of point
		{
			// Always 2 items: 0{EnumName} 1{EnumValue}
			string[] _splitedEnum = _operand.Split('.');
			string _enumName = _splitedEnum[0];
			string _enumValue = _splitedEnum[1];

			Type _enumType = GetEnumType(_enumName);
			if (_enumType == null)
				throw new Exception($"Enum '{_enumName}' not found");
			if (_enumType.IsEnumDefined(_enumValue))
			{
				if (Enum.TryParse(_enumType, _enumValue, out object _value))
					return (int)_value;
				else
				{
					string _error = $"Failed to get enum value '{_enumValue}'";
					throw new Exception(_error);
				}
			}
			else
			{
				string _error = $"Enum value '{_enumValue}' is undefined";
				throw new Exception(_error);
			}
		}

		//Previous result
		if (_operand.StartsWith('#'))
		{
			string _resultIndex = _operand.TrimStart('#'); // Remove '#'
			if (int.TryParse(_resultIndex, out int _index))
			{
				if (_index < 0 || _index >= stepsResult.Count)
				{
					string _error = $"Index out of bounds '{_index}'";
					throw new Exception(_error);
				}
				return stepsResult[_index];
			}
			else
			{
				string _error = $"Failed to get previous result '{_operand}'";
				throw new Exception(_error);
			}
		}

		// Field value
		SerializedProperty _property = GetObjectProperty(property, _operand);
		if (_property == null)
			throw new Exception($"Field property '{_operand}' not found");
		return _property?.propertyType switch
		{
			SerializedPropertyType.Boolean => _property.boolValue,
			SerializedPropertyType.Integer => _property.intValue,
			SerializedPropertyType.Float => _property.floatValue,
			SerializedPropertyType.Enum => _property.enumValueIndex,
			_ => throw new Exception($"Unsupported property type for '{_operand}'")
		};
	}

	/// <summary>
	/// Get the type of an enum
	/// </summary>
	/// <param name="enumName"> The enum's name </param>
	/// <returns> The Type if find, false otherwise </returns>
	Type GetEnumType(string enumName)
	{
		if (EnumCache.TryGetValue(enumName, out var type))
			return type;

		// Search in all types of the current assembly
		type = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(a => a.GetTypes())
			.FirstOrDefault(t => t.IsEnum && t.Name == enumName);

		if (type != null)
			EnumCache[enumName] = type;

		return type;
	}

	/// <summary>
	/// Get the result of a boolean expression
	/// </summary>
	/// <param name="_booleanOperator"> The boolean operator </param>
	/// <param name="_left"> The left operand of expression, can bu null </param>
	/// <param name="_right"> The right operand of expression </param>
	/// <returns> The expression's result </returns>
	bool BooleanExpressionResult(BooleanOperator _booleanOperator, object _left, object _right)
	{
		bool _result = false;
		switch (_booleanOperator)
		{
			case BooleanOperator.Not:
				_result = !Convert.ToBoolean(_right);
				break;
			case BooleanOperator.Equal:
				_result = _left.Equals(_right);
				break;
			case BooleanOperator.NotEqual:
				_result = !_left.Equals(_right);
				break;
			case BooleanOperator.Less:
				_result = Convert.ToDouble(_left) < Convert.ToDouble(_right);
				break;
			case BooleanOperator.LessEqual:
				_result = Convert.ToDouble(_left) <= Convert.ToDouble(_right);
				break;
			case BooleanOperator.Greater:
				_result = Convert.ToDouble(_left) > Convert.ToDouble(_right);
				break;
			case BooleanOperator.GreaterEqual:
				_result = Convert.ToDouble(_left) >= Convert.ToDouble(_right);
				break;
			case BooleanOperator.And:
				_result = Convert.ToBoolean(_left) && Convert.ToBoolean(_right);
				break;
			case BooleanOperator.Or:
				_result = Convert.ToBoolean(_left) || Convert.ToBoolean(_right);
				break;
			case BooleanOperator.Xor:
				_result = Convert.ToBoolean(_left) ^ Convert.ToBoolean(_right);
				break;
		}

		return _result;
	}

	/// <summary>
	/// Get the result of an arithmetic expression
	/// </summary>
	/// <param name="_booleanOperator"> The arithmetic operator </param>
	/// <param name="_left"> The left operand of expression </param>
	/// <param name="_right"> The right operand of expression </param>
	/// <returns> The expression's result </returns>
	double ArithmeticExpressionResult(ArithmeticOperator _operator, object _left, object _right)
	{
		double _result = 0;
		switch (_operator)
		{
			case ArithmeticOperator.Plus:
				_result = Convert.ToDouble(_left) + Convert.ToDouble(_right);
				break;
			case ArithmeticOperator.Minus:
				_result = Convert.ToDouble(_left) - Convert.ToDouble(_right);
				break;
			case ArithmeticOperator.Times:
				_result = Convert.ToDouble(_left) * Convert.ToDouble(_right);
				break;
			case ArithmeticOperator.Divide:
				_result = Convert.ToDouble(_left) / Convert.ToDouble(_right);
				break;
		}

		return _result;
	}

	/// <summary>
	/// Get the boolean operator's type
	/// </summary>
	/// <param name="_operator"> The operator in string </param>
	/// <returns> The BooleanOperator type if find, None otherwise </returns>
	BooleanOperator GetBooleanOperator(string _operator) => BooleanOperatorMap.TryGetValue(_operator, out var _result) ? _result : BooleanOperator.None;
	/// <summary>
	/// Get the arithmetic operator's type
	/// </summary>
	/// <param name="_operator"> The operator in string </param>
	/// <returns> The ArithmeticOperator type if find, None otherwise </returns>
	ArithmeticOperator GetArithmeticOperator(string _operator) => ArithmeticOperatorMap.TryGetValue(_operator, out var _result) ? _result : ArithmeticOperator.None;

	/// <summary>
	/// Get a serialized property on current serialized object
	/// </summary>
	/// <param name="_startedProperty"> The serialized property drawed </param>
	/// <param name="_propertyName"> The property's name to get </param>
	/// <returns> The SerializedProperty if find, null otherwise </returns>
	SerializedProperty GetObjectProperty(SerializedProperty _startedProperty, string _propertyName)
	{
		string _targetPath = _startedProperty.propertyPath;
		if (_targetPath.EndsWith(']')) // Don't use _property.isArray because it don't work with ScriptableObject, GameObject, MonoBehaviour, ...
		{
			// Replace path's last array field by condition field		
			int _indexArrayField = _targetPath.LastIndexOf(".Array.data");
			_targetPath = _targetPath.Substring(0, _indexArrayField);
		}
		// Replace path's last field by condition field
		int _indexPropertyName = _targetPath.LastIndexOf('.') + 1;
		_targetPath = _targetPath.Substring(0, _indexPropertyName) + _propertyName;
		return _startedProperty.serializedObject.FindProperty(_targetPath);
	}
}

enum BooleanOperator
{
	None,
	Not,
	Equal,
	NotEqual,
	Less,
	LessEqual,
	Greater,
	GreaterEqual,
	And,
	Or,
	Xor
}
enum ArithmeticOperator
{
	None,
	Plus,
	Minus,
	Times,
	Divide
}
#endregion

#region Math
[CustomPropertyDrawer(typeof(ClampMinAttribute))]
public class ClampMinDrawer : PropertyDrawer
{
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		ClampMinAttribute _clampMin = (ClampMinAttribute)attribute;

		if (_property.propertyType == SerializedPropertyType.Integer)
		{
			int _clampValue = EditorGUI.IntField(_position, _label, _property.intValue);
			if (_clampValue < _clampMin.min)
				_property.intValue = (int)_clampMin.min;
			else
				_property.intValue = _clampValue;
		}
		else if (_property.propertyType == SerializedPropertyType.Float)
		{
			float _clampValue = EditorGUI.FloatField(_position, _label, _property.floatValue);
			if (_clampValue < _clampMin.min)
				_property.floatValue = _clampMin.min;
			else
				_property.floatValue = _clampValue;
		}
		else
		{
			EditorGUI.LabelField(_position, _label.text, "Clamp only 'int' or 'float' value");
		}
	}
}

[CustomPropertyDrawer(typeof(ClampMaxAttribute))]
public class ClampMaxDrawer : PropertyDrawer
{
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		ClampMaxAttribute _clampMax = (ClampMaxAttribute)attribute;

		if (_property.propertyType == SerializedPropertyType.Integer)
		{
			int _clampValue = EditorGUI.IntField(_position, _label, _property.intValue);
			if (_clampValue > _clampMax.max)
				_property.intValue = (int)_clampMax.max;
			else
				_property.intValue = _clampValue;
		}
		else if (_property.propertyType == SerializedPropertyType.Float)
		{
			float _clampValue = EditorGUI.FloatField(_position, _label, _property.floatValue);
			if (_clampValue > _clampMax.max)
				_property.floatValue = _clampMax.max;
			else
				_property.floatValue = _clampValue;
		}
		else
		{
			EditorGUI.LabelField(_position, _label.text, "Clamp only 'int' or 'float' value");
		}
	}
}

[CustomPropertyDrawer(typeof(ClampMinMaxAttribute))]
public class ClampMinMaxDrawer : PropertyDrawer
{
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		ClampMinMaxAttribute _clamp = (ClampMinMaxAttribute)attribute;

		if (_property.propertyType == SerializedPropertyType.Integer)
		{
			int _clampValue = EditorGUI.IntField(_position, _label, _property.intValue);
			_property.intValue = (int)Mathf.Clamp(_clampValue, _clamp.min, _clamp.max);
		}
		else if (_property.propertyType == SerializedPropertyType.Float)
		{
			float _clampValue = EditorGUI.FloatField(_position, _label, _property.floatValue);
			_property.floatValue = Mathf.Clamp(_clampValue, _clamp.min, _clamp.max);
		}
		else
		{
			EditorGUI.LabelField(_position, _label.text, "Clamp only 'int' or 'float' value");
		}
	}
}
#endregion
