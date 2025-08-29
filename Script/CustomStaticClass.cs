
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Handle more options for debug log
/// </summary>
public static class DebugLog
{
	const bool DISPLAY_LOG = true;
	const string ORANGE_HEXA = "FFA500";

	#region Log
	public static void Log(object _message)
	{
		if (DISPLAY_LOG)
			Debug.Log(_message);
	}
	public static void Log(string _verbosity, object _message)
	{
		if (DISPLAY_LOG)
		{
			string _log = $"{Verbosity(_verbosity)} {_message}";
			Debug.Log(_log);
		}
	}
	public static void Log(string _verbosity, object _message, int _size)
	{
		if (DISPLAY_LOG)
		{
			string _log = RichText.Size($"{Verbosity(_verbosity)} {_message}", _size);
			Debug.Log(_log);
		}
	}
	public static void Log(object _message, Color _color, int _size)
	{
		if (DISPLAY_LOG)
		{
			string _log = RichText.Color(RichText.Size(_message.ToString(), _size), _color);
			Debug.Log(_log);
		}
	}
	public static void Log(string _verbosity, object _message, Color _color)
	{
		if (DISPLAY_LOG)
		{
			string _log = RichText.Color($"{Verbosity(_verbosity)} {_message}", _color);
			Debug.Log(_log);
		}
	}
	public static void Log(string _verbosity, object _message, Color _color, int _size)
	{
		if (DISPLAY_LOG)
		{
			string _log = RichText.Color(RichText.Size($"{Verbosity(_verbosity)} {_message}", _size), _color);
			Debug.Log(_log);
		}
	}
	#endregion

	#region LogWarning
	public static void LogWarning(object _message)
	{
		string _log = RichText.Color(_message.ToString(), ORANGE_HEXA);
		Debug.LogWarning(_log);
	}
	public static void LogWarning(string _verbosity, object _message)
	{
		string _log = RichText.Color($"{Verbosity(_verbosity)} {_message}", ORANGE_HEXA);
		Debug.LogWarning(_log);
	}
	public static void LogWarning(string _verbosity, object _message, int _size)
	{
		string _log = RichText.Color(RichText.Size($"{Verbosity(_verbosity)} {_message}", _size), ORANGE_HEXA);
		Debug.LogWarning(_log);
	}
	#endregion

	#region LogError
	public static void LogError(object _message)
	{
		string _log = RichText.Color(_message.ToString(), Color.red);
		Debug.LogError(_log);
	}
	public static void LogError(string _verbosity, object _message)
	{
		string _log = RichText.Color($"{Verbosity(_verbosity)} {_message}", Color.red);
		Debug.LogError(_log);
	}
	public static void LogError(string _verbosity, object _message, int _size)
	{
		string _log = RichText.Color(RichText.Size($"{Verbosity(_verbosity)} {_message}", _size), Color.red);
		Debug.LogError(_log);
	}
	#endregion

	static string Verbosity(string _verbosity)
	{
		return RichText.Bold($"[{RichText.Italic(_verbosity)}]");
	}
}

/// <summary>
/// Wrap text for RichText options
/// </summary>
public static class RichText
{
	public static string Bold(string _text)
	{
		return $"<b>{_text}</b>";
	}
	public static string Italic(string _text)
	{
		return $"<i>{_text}</i>";
	}
	public static string Color(string _text, Color _color)
	{
		string _hexaColor = ColorUtility.ToHtmlStringRGBA(_color);
		return Color(_text, _hexaColor);
	}
	public static string Color(string _text, string _hexaColor)
	{
		return $"<color=#{_hexaColor}>{_text}</color>";
	}
	public static string Size(string _text, int _size)
	{
		return $"<size={_size}>{_text}</size>";
	}
	public static string Style(string _text, string _style)
	{
		return $"<style=\"{_style}\">{_text}</style>";
	}
	public static string Material(string _text, Material _mat)
	{
		return $"<material={_mat}>{_text}</material>";
	}
	public static string Quad(Sprite _sprite)
	{
		return $"<quad>{_sprite}</quad>";
	}
}

public class BooleanExpressionParser
{
	static readonly Dictionary<string, int> operatorPriority = new Dictionary<string, int>
	{
		// Arithmetic operation
		{ "+", 7 }, { "-", 7 }, { "*", 7 }, { "/", 7 },
		// Boolean operation
		{ "!", 6 },
		{ "==", 5 }, { "!=", 5 }, { "<=", 5 }, { ">=", 5 }, { "<", 5 }, { ">", 5 },
		{ "^", 4 },
		{ "&", 3 },
		{ "|", 2 },
		{ "&&", 1 },
		{ "||", 0 },
	};

	// Pattern to separate var and operator of expression
	static readonly Regex TokenizerRegex = new(@"(-?\d+(\.\d+)?[fF]?)|(\|\||&&|==|!=|<=|>=|<|>|[!^\-+\*/()])|\w\w*(\.\w*)?", RegexOptions.Compiled);
	// Pattern to validate variable (digit, enum, fieldName)
	public static readonly Regex ValidVariableRegex = new(@"^(-?\d+(\.\d+)?[fF]?)|([A-Za-z_]\w*(\.[A-Za-z_]\w*)?)$", RegexOptions.Compiled);
	// Pattern for boolean operator
	public static readonly Regex BooleanOperatorRegex = new(@"(\|\||&&|==|!=|<=|>=|<|>|[!^])", RegexOptions.Compiled);
	// Pattern for Arithmetic operator
	public static readonly Regex ArithmeticOperandRegex = new(@"([#-]?\d+(\.\d+)?[fF]?)|([+\-*/])|\w*(\.\w*)?", RegexOptions.Compiled);
	// Pattern for digit
	public static readonly Regex DigitRegex = new(@"^(-?\d+(\.\d+)?[fF]?)$", RegexOptions.Compiled);
	// Pattern for bolean
	public static readonly Regex BooleanRegex = new(@"^(true|True|false|False)$", RegexOptions.Compiled);
	// Pattern for unique variable
	public static readonly Regex VariableRegex = new(@"^\w+$", RegexOptions.Compiled);

	/// <summary>
	/// Parse a string boolean expression in list of atomic expressions
	/// </summary>
	/// <param name="_expression"> The boolean expression </param>
	/// <returns> The list of atomic expressions </returns>
	public static List<string> ParseBooleanExpression(string _expression)
	{
		if (string.IsNullOrEmpty(_expression.Trim()))
			throw new Exception($"Boolean expression is null or empty");

		_expression = _expression.Trim();
		List<string> _tokens = Tokenize(_expression);
		ValidateExpression(_expression, _tokens);
		List<string> _sortedTokens = SortOperations(_tokens);
		return BuildOperationStep(_sortedTokens);
	}

	/// <summary>
	/// Check if a string boolean expression is valid
	/// </summary>
	/// <param name="_expression"> The boolean expression </param>
	/// <returns> True if valid, false otherwise </returns>
	static void ValidateExpression(string _expression, List<string> _tokens)
	{
		if (_tokens.Count == 1)
		{
			if (!VariableRegex.IsMatch(_tokens[0])) // Not unique variable
				throw new Exception($"The expression \"{_expression}\" isn't a valid boolean expression");

			// Add operator and right operand for parsing
			_tokens.Add("==");
			_tokens.Add("true");
			return;
		}

		// Check parenthesis
		if (_expression.Contains("(") || _expression.Contains(")"))
		{
			int _count = 0;
			foreach (char _c in _expression)
			{
				if (_c == '(')
					_count++;
				else if (_c == ')')
					_count--;
				if (_count < 0) // A ")" without a "(" before
					throw new Exception("Boolean expression is invalid : closed parenthesis witout its opened parenthesis");
			}
			if (_count != 0) // More "(" than ")"
				throw new Exception("Boolean expression is invalid : More opened parenthesis than closed parenthesis");
		}

		StringBuilder _concatTokens = new StringBuilder();
		string _previousToken = null;
		foreach (string _token in _tokens)
		{
			if (_token == "(") // Not Preceded by var
			{
				if (_previousToken != null && !operatorPriority.ContainsKey(_previousToken) && _previousToken != "(")
					throw new Exception($"Boolean expression is invalid : an opened parenthsis is preceded by a variable '{_previousToken}'");
			}
			else if (_token == ")") // Not preceded by operator or "("
			{
				if (operatorPriority.ContainsKey(_previousToken))
					throw new Exception($"Boolean expression is invalid : a closed parenthesis is preceded by a operator '{_previousToken}'");
				else if (_previousToken == "(")
					throw new Exception("Boolean expression is invalid : an opened/closed parenthesis contains nothing");
			}
			else if (operatorPriority.ContainsKey(_token)) // Operator, not preceded by nth, operator or "("
			{
				if (_token == "!")
				{
					if (_previousToken == "!")
						throw new Exception("Boolean expression is invalid : two consecutive '!' operators are not allowed");
				}
				else
				{
					if (_previousToken == null)
						throw new Exception($"Boolean expression is invalid : an operator '{_token}' is preceded by nothing");
					else if (operatorPriority.ContainsKey(_previousToken))
						throw new Exception($"Boolean expression is invalid : an operator '{_token}' is preceded by another operator '{_previousToken}'");
					else if (_previousToken == "(")
						throw new Exception($"Boolean expression is invalid : an operator '{_token}' is preceded by an opened parenthesis");
				}
			}
			else // Var, not Preceded by var or ")"
			{
				// Check variable validity (Ex: 123var, .var, var.var.var => invalid)
				if (!ValidVariableRegex.IsMatch(_token))
					throw new Exception($"Boolean expression is invalid : variable '{_token}' is invalid");

				if (_previousToken != null && ValidVariableRegex.IsMatch(_previousToken))
					throw new Exception($"Boolean expression is invalid : a variable '{_token}' is preceded by another vairable '{_previousToken}'");
				else if (_previousToken == ")")
					throw new Exception($"Boolean expression is invalid : a variable '{_token}' is preceded by a closed parenthesis");
			}

			_previousToken = _token;
			_concatTokens.Append(_token);
		}

		// Finished by operator
		if (operatorPriority.ContainsKey(_previousToken))
			throw new Exception($"Boolean expression is invalid : the expression is finished by an operator '{_previousToken}'");

		// Contains a forbiden special character or format
		string _compactExpression = Regex.Replace(_expression, @"\s+", ""); // Delete space
		if (_concatTokens.ToString() != _compactExpression)
			throw new Exception("Boolean expression is invalid : forbidden special character or format detected");
	}

	/// <summary>
	/// Tokenize the boolean expression
	/// </summary>
	/// <param name="_expression"> The boolean expression </param>
	/// <returns> The list of string tokens </returns>
	static List<string> Tokenize(string _expression)
	{
		List<string> _tokens = new();
		MatchCollection _matches = TokenizerRegex.Matches(_expression);
		foreach (Match _match in _matches)
			_tokens.Add(_match.Value);
		return _tokens;
	}

	/// <summary>
	/// Sort the boolean expression operation (a == b, c && d, ...) by priority
	/// </summary>
	/// <param name="_tokens"> The list of expression's tokens </param>
	/// <returns> The list of sorted string tokens </returns>
	static List<string> SortOperations(List<string> _tokens)
	{
		List<string> _output = new();
		Stack<string> _stack = new();

		foreach (string _token in _tokens)
		{
			if (operatorPriority.ContainsKey(_token)) // operator
			{
				while (_stack.Count > 0 && _stack.Peek() != "(" && operatorPriority[_stack.Peek()] >= operatorPriority[_token]) // highest operator
				{
					_output.Add(_stack.Pop());
				}
				_stack.Push(_token);
			}
			else if (_token == "(")
			{
				_stack.Push(_token);
			}
			else if (_token == ")")
			{
				while (_stack.Count > 0 && _stack.Peek() != "(") // parenthesis inside
				{
					_output.Add(_stack.Pop());
				}
				_stack.Pop(); // remove "("
			}
			else // variables
			{
				_output.Add(_token);
			}
		}

		while (_stack.Count > 0)
		{
			_output.Add(_stack.Pop());
		}

		return _output;
	}

	/// <summary>
	/// Build boolean step operation of boolean expression
	/// </summary>
	/// <param name="_sortedTokens"> The list of sorted tokens </param>
	/// <returns> The list of boolean operation step by step </returns>
	static List<string> BuildOperationStep(List<string> _sortedTokens)
	{
		List<string> _operations = new();
		Stack<string> _stack = new();

		// Cache operation's index to bring its result later
		int _operationIndex = 0;

		foreach (string _token in _sortedTokens)
		{
			if (operatorPriority.ContainsKey(_token)) // Operator
			{
				if (_token == "!") // "!" operator
				{
					string _res = $"#{_operationIndex++}";
					string _operand = _stack.Pop();
					string _step = $"{_token}{_operand}";
					_stack.Push(_res);
					_operations.Add(_step);
				}
				else
				{
					string _res = $"#{_operationIndex++}";
					string _right = _stack.Pop();
					string _left = _stack.Pop();
					string _step = $"{_left}{_token}{_right}";
					_stack.Push(_res);
					_operations.Add(_step);
				}
			}
			else // Variable
			{
				_stack.Push(_token);
			}
		}

		return _operations;
	}
}

// Regex Cheatsheet
// Métacaractères
//	Symbole		Signification										Exemple
//	.			N’importe quel caractère (sauf retour ligne)		a.c → "abc", "axc"
//	^			Début de chaîne										^abc → "abcdef"
//	$			Fin de chaîne										abc$ → "123abc"
//	|			OU logique											a|b → "a" ou "b"
//	\			Échappe un caractère spécial						\. → "."

// Classes de caractères []
//	Exemple				Signification
//	[abc]				"a" ou "b" ou "c"
//	[a-z]				une lettre minuscule
//	[A-Z]				une lettre majuscule
//	[0-9]				un chiffre
//	[a-zA-Z0-9_]		identifiant classique
//	[^0-9]				tout sauf un chiffre

// Quantificateurs
//	Symbole		Signification			Exemple
//	*			0 ou plus				a* → "", "a", "aaa"
//	+			1 ou plus				a+ → "a", "aaa"
//	?			0 ou 1					a? → "", "a"
//	{n}			exactement n fois		[0-9]{3} → "123"
//	{n,}		au moins n fois			[0-9]{2,} → "12", "1234"
//	{n,m}		entre n et m fois		[0-9]{2,4} → "12", "123", "1234"

// Ancres
//	Symbole		Signification			Exemple
//	^			début de ligne			^Hello
//	$			fin de ligne			world$
//	\b			limite de mot			\bcat\b → "cat" mais pas "scatter"
//	\B			pas limite de mot		\Bcat\B → "concatenate"

// Groupes et alternatives
//	Exemple		Signification
//	(abc)		groupe capturé
//	(?:abc)		groupe non capturé
//	a|b			soit "a" soit "b"

// Raccourcis
//	Symbole		Équivaut à						Exemple
//	\d			[0-9]							\d + → "123"
//	\D			[^0-9]							\D + → "abc"
//	\w			[a-zA-Z0-9_]					\w + → "abc_123"
//	\W			[^a-zA-Z0-9_]					\W + → "!$%"
//	\s			espace, tab, retour ligne		\s+
//	\S			tout sauf espace				\S+

// Greedy vs Lazy
//	Exemple		Résultat
//	a.*b		prend le plus possible → "axxxbzzz b"
//	a.*?b		prend le moins possible → "axxxb"