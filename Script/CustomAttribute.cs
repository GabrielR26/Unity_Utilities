using UnityEngine;

public class HideConditionAttribute : PropertyAttribute
{
	public string conditionField;
	public object compareValue;
	public bool inverse;

	public HideConditionAttribute(string conditionField)
	{
		this.conditionField = conditionField;
		this.inverse = false;
	}
	public HideConditionAttribute(string conditionField, object compareValue)
	{
		this.conditionField = conditionField;
		this.compareValue = compareValue;
		this.inverse = false;
	}
	public HideConditionAttribute(string conditionField, object compareValue, bool reverse)
	{
		this.conditionField = conditionField;
		this.compareValue = compareValue;
		this.inverse = reverse;
	}
}

public class EditConditionAttribute : PropertyAttribute
{
	public string conditionField;
	public object compareValue;
	public bool inverse;

	public EditConditionAttribute(string conditionField)
	{
		this.conditionField = conditionField;
		this.inverse = false;
	}
	public EditConditionAttribute(string conditionField, object compareValue)
	{
		this.conditionField = conditionField;
		this.compareValue = compareValue;
		this.inverse = false;
	}
	public EditConditionAttribute(string conditionField, object compareValue, bool reverse)
	{
		this.conditionField = conditionField;
		this.compareValue = compareValue;
		this.inverse = reverse;
	}
}

#region Math
public class ClampMinAttribute : PropertyAttribute
{
	public readonly int iMin;
	public readonly float fMin;

	public ClampMinAttribute(int min)
	{
		this.iMin = min;
	}
	public ClampMinAttribute(float min)
	{
		this.fMin = min;
	}
}

public class ClampMaxAttribute : PropertyAttribute
{
	public readonly int iMax;
	public readonly float fMax;

	public ClampMaxAttribute(int max)
	{
		this.iMax = max;
	}
	public ClampMaxAttribute(float max)
	{
		this.fMax = max;
	}
}
#endregion