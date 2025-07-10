
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HideConditionAttribute))]
public class HideConditionDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		HideConditionAttribute _hideCondition = (HideConditionAttribute)attribute;
		/// Replace last field in path by condition field
		int _indexPropertyName = property.propertyPath.LastIndexOf('.') + 1; 
		string _targetPath = property.propertyPath.Substring(0, _indexPropertyName) + _hideCondition.conditionField;
		SerializedProperty _conditionProperty = property.serializedObject.FindProperty(_targetPath);

		if (_conditionProperty != null)
		{
			bool _canHide = false;
			switch (_conditionProperty.propertyType)
			{
				case SerializedPropertyType.Boolean:
					bool _boolVal = _conditionProperty.boolValue;
					_canHide = _hideCondition.compareValue == null ? _boolVal : _boolVal.Equals(_hideCondition.compareValue);
					break;

				case SerializedPropertyType.Integer:
					int _intVal = _conditionProperty.intValue;
					_canHide = _hideCondition.compareValue == null ? _intVal != 0 : _intVal.Equals(_hideCondition.compareValue);
					break;

				case SerializedPropertyType.Float:
					float _floatVal = _conditionProperty.floatValue;
					_canHide = _hideCondition.compareValue == null ? _floatVal != 0 : _floatVal.Equals(_hideCondition.compareValue);
					break;

				case SerializedPropertyType.String:
					string _stringVal = _conditionProperty.stringValue;
					_canHide = _hideCondition.compareValue == null ? string.IsNullOrEmpty(_stringVal) : _stringVal.Equals(_hideCondition.compareValue);
					break;

				case SerializedPropertyType.Enum:
					int _enumIndex = _conditionProperty.enumValueIndex;
					_canHide = _hideCondition.compareValue == null ? false : _enumIndex.Equals(_hideCondition.compareValue);
					break;
			}

			if (_hideCondition.inverse == _canHide)
					EditorGUI.PropertyField(position, property, label, true);
		}
		else
		{
			EditorGUI.LabelField(position, label.text, "Failed to get hide condition");
		}
	}

	/// <summary>
	/// Change Height to reduce if hidden
	/// </summary>
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		HideConditionAttribute _hideCondition = (HideConditionAttribute)attribute;
		/// Replace last field in path by condition field
		int _indexPropertyName = property.propertyPath.LastIndexOf('.') + 1;
		string _targetPath = property.propertyPath.Substring(0, _indexPropertyName) + _hideCondition.conditionField;
		SerializedProperty _conditionProperty = property.serializedObject.FindProperty(_targetPath);

		if (_conditionProperty != null)
		{
			bool _canHide = false;
			switch (_conditionProperty.propertyType)
			{
				case SerializedPropertyType.Boolean:
					bool _boolVal = _conditionProperty.boolValue;
					_canHide = _hideCondition.compareValue == null ? _boolVal : _boolVal.Equals(_hideCondition.compareValue);
					break;

				case SerializedPropertyType.Integer:
					int _intVal = _conditionProperty.intValue;
					_canHide = _hideCondition.compareValue == null ? _intVal != 0 : _intVal.Equals(_hideCondition.compareValue);
					break;

				case SerializedPropertyType.Float:
					float _floatVal = _conditionProperty.floatValue;
					_canHide = _hideCondition.compareValue == null ? _floatVal != 0 : _floatVal.Equals(_hideCondition.compareValue);
					break;

				case SerializedPropertyType.String:
					string _stringVal = _conditionProperty.stringValue;
					_canHide = _hideCondition.compareValue == null ? string.IsNullOrEmpty(_stringVal) : _stringVal.Equals(_hideCondition.compareValue);
					break;

				case SerializedPropertyType.Enum:
					int _enumIndex = _conditionProperty.enumValueIndex;
					_canHide = _hideCondition.compareValue == null ? false : _enumIndex.Equals(_hideCondition.compareValue);
					break;
			}

			return _hideCondition.inverse == _canHide ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
		}

		return EditorGUI.GetPropertyHeight(property, label, true);
	}
}

[CustomPropertyDrawer(typeof(EditConditionAttribute))]
public class EditConditionDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditConditionAttribute _editCondition = (EditConditionAttribute)attribute;
		/// Replace last field in path by condition field
		int _indexPropertyName = property.propertyPath.LastIndexOf('.') + 1;
		string _targetPath = property.propertyPath.Substring(0, _indexPropertyName) + _editCondition.conditionField;
		SerializedProperty _conditionProperty = property.serializedObject.FindProperty(_targetPath);

		if (_conditionProperty != null)
		{
			bool _canEdit = false;
			switch (_conditionProperty.propertyType)
			{
				case SerializedPropertyType.Boolean:
					bool _boolVal = _conditionProperty.boolValue;
					_canEdit = _editCondition.compareValue == null ? _boolVal : _boolVal.Equals(_editCondition.compareValue);
					break;

				case SerializedPropertyType.Integer:
					int _intVal = _conditionProperty.intValue;
					_canEdit = _editCondition.compareValue == null ? _intVal != 0 : _intVal.Equals(_editCondition.compareValue);
					break;

				case SerializedPropertyType.Float:
					float _floatVal = _conditionProperty.floatValue;
					_canEdit = _editCondition.compareValue == null ? _floatVal != 0 : _floatVal.Equals(_editCondition.compareValue);
					break;

				case SerializedPropertyType.String:
					string _stringVal = _conditionProperty.stringValue;
					_canEdit = _editCondition.compareValue == null ? string.IsNullOrEmpty(_stringVal) : _stringVal.Equals(_editCondition.compareValue);
					break;

				case SerializedPropertyType.Enum:
					int _enumIndex = _conditionProperty.enumValueIndex;
					_canEdit = _editCondition.compareValue == null ? false : _enumIndex.Equals(_editCondition.compareValue);
					break;
			}

			GUI.enabled = _canEdit ^ _editCondition.inverse;
			EditorGUI.PropertyField(position, property, label, true);
		}
		else
		{
			EditorGUI.LabelField(position, label.text, "Failed to get edit condition property");
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}
}

#region Math
[CustomPropertyDrawer(typeof(ClampMinAttribute))]
public class ClampMinDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		ClampMinAttribute _clampMin = (ClampMinAttribute)attribute;

		if (property.propertyType == SerializedPropertyType.Integer)
		{
			int _clampValue = EditorGUI.IntField(position, label, property.intValue);
			if (_clampValue < _clampMin.iMin)
				property.intValue = _clampMin.iMin;
			else
				property.intValue = _clampValue;
		}
		else if (property.propertyType == SerializedPropertyType.Float)
		{
			float _clampValue = EditorGUI.FloatField(position, label, property.floatValue);
			if (_clampValue < _clampMin.fMin)
				property.floatValue = _clampMin.fMin;
			else
				property.floatValue = _clampValue;
		}
		else
		{
			EditorGUI.LabelField(position, label.text, "Clamp only 'int' or 'float' value");
		}
	}
}

[CustomPropertyDrawer(typeof(ClampMaxAttribute))]
public class ClampMaxDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		ClampMaxAttribute _clampMax = (ClampMaxAttribute)attribute;

		if (property.propertyType == SerializedPropertyType.Integer)
		{
			int _clampValue = EditorGUI.IntField(position, label, property.intValue);
			if (_clampValue > _clampMax.iMax)
				property.intValue = _clampMax.iMax;
			else
				property.intValue = _clampValue;
		}
		else if (property.propertyType == SerializedPropertyType.Float)
		{
			float _clampValue = EditorGUI.FloatField(position, label, property.floatValue);
			if (_clampValue > _clampMax.fMax)
				property.floatValue = _clampMax.fMax;
			else
				property.floatValue = _clampValue;
		}
		else
		{
			EditorGUI.LabelField(position, label.text, "Clamp only 'int' or 'float' value");
		}
	}
}
#endregion