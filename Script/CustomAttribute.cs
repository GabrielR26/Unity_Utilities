
using UnityEngine;

/// <summary>
/// Hide a variable by boolean expression
/// </summary>
public class HideConditionAttribute : PropertyAttribute
{
	public string booleanExpression = string.Empty;

	public HideConditionAttribute(string _conditionField, bool _isCollection = false) : base(_isCollection)
	{
		booleanExpression = _conditionField;
	}
}

/// <summary>
/// Allow edition of a property by boolean expression
/// </summary>
public class EditConditionAttribute : PropertyAttribute
{
	public string booleanExpression = "";

	public EditConditionAttribute(string _conditionField) : base(true)
	{
		booleanExpression = _conditionField;
	}
}

public class FixedListAttribute : PropertyAttribute
{
	public int size = 0;

	public FixedListAttribute(int _size) 
	{ 
		size = _size; 
	}
}

public class PrefabWithComponentAttribute : PropertyAttribute
{
    public System.Type RequiredComponentType;

    public PrefabWithComponentAttribute(System.Type componentType)
    {
        RequiredComponentType = componentType;
    }
}

#region Math
/// <summary>
/// Allow edition of RangeBetween struc
/// </summary>
public class RangeBetweenAttribute : PropertyAttribute
{
	public int minLimit = 0;
	public int maxLimit = 10;

	public RangeBetweenAttribute(int _clamMin, int _clampMax)
	{
		minLimit = _clamMin;
		maxLimit = _clampMax;
	}
}

/// <summary>
/// Clamp an int or float var with min value
/// </summary>
public class ClampMinAttribute : PropertyAttribute
{
	public readonly float min = int.MinValue;

	public ClampMinAttribute(int _min)
	{
		min = _min;
	}
	public ClampMinAttribute(float _min)
	{
		min = _min;
	}
}

/// <summary>
/// Clamp an int or float var with max value
/// </summary>
public class ClampMaxAttribute : PropertyAttribute
{
	public readonly float max = float.MaxValue;

	public ClampMaxAttribute(int _max)
	{
		max = _max;
	}
	public ClampMaxAttribute(float _max)
	{
		max = _max;
	}
}

/// <summary>
/// Clamp an int or float var with min and max value
/// </summary>
public class ClampMinMaxAttribute : PropertyAttribute
{
	public readonly float min = float.MinValue;
	public readonly float max = float.MaxValue;

	public ClampMinMaxAttribute(int _min, int _max)
	{
		min = _min;
		max = _max;
	}
	public ClampMinMaxAttribute(float _min, float _max)
	{
		min = _min;
		max = _max;
	}
}
#endregion