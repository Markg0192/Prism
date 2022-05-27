using System;
using System.Collections;
using System.Globalization;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class MathEvaluate
	{
		public delegate Symbol EvaluateFunctionDelegate(string name, params object[] args);

		public enum Type
		{
			Variable,
			Value,
			Operator,
			Function,
			Result,
			Bracket,
			Comma,
			Error
		}

		public struct Symbol
		{
			public string SymbolName;

			public Type SymbolType;

			public double SymbolValue;

			public override string ToString()
			{
				return SymbolName;
			}
		}

		public const string DivideByZero = "Divide by Zero";

		private readonly ArrayList equation = new ArrayList();

		private readonly ArrayList postFix = new ArrayList();

		private EvaluateFunctionDelegate defaultFunctionEvaluation;

		private string errorDescription = "None";

		private bool evaluationError;

		private double evaluationResult;

		public EvaluateFunctionDelegate DefaultFunctionEvaluation
		{
			get
			{
				return defaultFunctionEvaluation;
			}
			set
			{
				defaultFunctionEvaluation = value;
			}
		}

		public ArrayList Equation => (ArrayList)equation.Clone();

		public bool Error => evaluationError;

		public string ErrorDescription => errorDescription;

		public ArrayList Postfix => (ArrayList)postFix.Clone();

		public double Result => evaluationResult;

		public ArrayList Variables
		{
			get
			{
				ArrayList arrayList = new ArrayList();
				foreach (Symbol item in equation)
				{
					if (item.SymbolType == Type.Variable && !arrayList.Contains(item))
					{
						arrayList.Add(item);
					}
				}
				return arrayList;
			}
			set
			{
				foreach (Symbol item in value)
				{
					for (int i = 0; i < postFix.Count; i++)
					{
						if (item.SymbolName == ((Symbol)postFix[i]).SymbolName && ((Symbol)postFix[i]).SymbolType == Type.Variable)
						{
							Symbol symbol2 = (Symbol)postFix[i];
							symbol2.SymbolValue = item.SymbolValue;
							postFix[i] = symbol2;
						}
					}
				}
			}
		}

		public void EvaluatePostfix()
		{
			Stack stack = new Stack();
			ArrayList arrayList = new ArrayList();
			evaluationError = false;
			foreach (Symbol item in postFix)
			{
				if (item.SymbolType == Type.Value || item.SymbolType == Type.Variable || item.SymbolType == Type.Result)
				{
					stack.Push(item);
				}
				else if (item.SymbolType == Type.Operator)
				{
					if (stack.Count < 2)
					{
						evaluationError = true;
						break;
					}
					Symbol sym = (Symbol)stack.Pop();
					Symbol sym2 = (Symbol)stack.Pop();
					Symbol symbol2 = Evaluate(sym2, item, sym);
					if (symbol2.SymbolType == Type.Error)
					{
						evaluationError = true;
						errorDescription = symbol2.SymbolName;
					}
					if (!evaluationError)
					{
						stack.Push(symbol2);
					}
				}
				else
				{
					if (item.SymbolType != Type.Function)
					{
						continue;
					}
					arrayList.Clear();
					Symbol sym = (Symbol)stack.Pop();
					if (sym.SymbolType == Type.Value || sym.SymbolType == Type.Variable || sym.SymbolType == Type.Result)
					{
						Symbol symbol2 = EvaluateFunction(item.SymbolName, sym);
						if (symbol2.SymbolType == Type.Error)
						{
							evaluationError = true;
							errorDescription = symbol2.SymbolName;
						}
						if (!evaluationError)
						{
							stack.Push(symbol2);
						}
					}
					else if (sym.SymbolType == Type.Comma)
					{
						while (sym.SymbolType == Type.Comma)
						{
							sym = (Symbol)stack.Pop();
							arrayList.Add(sym);
							sym = (Symbol)stack.Pop();
						}
						arrayList.Add(sym);
						Symbol symbol2 = EvaluateFunction(item.SymbolName, arrayList.ToArray());
						if (symbol2.SymbolType == Type.Error)
						{
							evaluationError = true;
							errorDescription = symbol2.SymbolName;
						}
						if (!evaluationError)
						{
							stack.Push(symbol2);
						}
					}
					else
					{
						stack.Push(sym);
						Symbol symbol2 = EvaluateFunction(item.SymbolName);
						if (symbol2.SymbolType == Type.Error)
						{
							evaluationError = true;
							errorDescription = symbol2.SymbolName;
						}
						if (!evaluationError)
						{
							stack.Push(symbol2);
						}
					}
				}
			}
			if (!evaluationError && stack.Count == 1)
			{
				evaluationResult = ((Symbol)stack.Pop()).SymbolValue;
			}
		}

		public void Infix2Postfix()
		{
			Stack stack = new Stack();
			foreach (Symbol item in equation)
			{
				if (item.SymbolType == Type.Value || item.SymbolType == Type.Variable)
				{
					postFix.Add(item);
					continue;
				}
				if (item.SymbolName == "(" || item.SymbolName == "[" || item.SymbolName == "{")
				{
					stack.Push(item);
					continue;
				}
				if (item.SymbolName == ")" || item.SymbolName == "]" || item.SymbolName == "}")
				{
					if (stack.Count > 0)
					{
						Symbol symbol2 = (Symbol)stack.Pop();
						while (symbol2.SymbolName != "(" && symbol2.SymbolName != "[" && symbol2.SymbolName != "{")
						{
							postFix.Add(symbol2);
							symbol2 = (Symbol)stack.Pop();
						}
					}
					continue;
				}
				if (stack.Count > 0)
				{
					Symbol symbol2 = (Symbol)stack.Pop();
					while (stack.Count != 0 && (symbol2.SymbolType == Type.Operator || symbol2.SymbolType == Type.Function || symbol2.SymbolType == Type.Comma) && Precedence(symbol2) >= Precedence(item))
					{
						postFix.Add(symbol2);
						symbol2 = (Symbol)stack.Pop();
					}
					if ((symbol2.SymbolType == Type.Operator || symbol2.SymbolType == Type.Function || symbol2.SymbolType == Type.Comma) && Precedence(symbol2) >= Precedence(item))
					{
						postFix.Add(symbol2);
					}
					else
					{
						stack.Push(symbol2);
					}
				}
				stack.Push(item);
			}
			while (stack.Count > 0)
			{
				Symbol symbol2 = (Symbol)stack.Pop();
				postFix.Add(symbol2);
			}
		}

		public bool Parse(string newEquation)
		{
			bool flag = true;
			int num = 1;
			string text = string.Empty;
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			CultureInfo provider = new CultureInfo("en-us");
			evaluationError = false;
			errorDescription = "None";
			equation.Clear();
			postFix.Clear();
			newEquation = newEquation.Trim();
			int startIndex;
			while ((startIndex = newEquation.IndexOf(' ')) != -1)
			{
				newEquation = newEquation.Remove(startIndex, 1);
			}
			Symbol symbol = default(Symbol);
			for (int i = 0; i < newEquation.Length; i++)
			{
				switch (num)
				{
				case 1:
					if (char.IsNumber(newEquation[i]))
					{
						num = 2;
						text += newEquation[i];
						break;
					}
					if (char.IsLetter(newEquation[i]))
					{
						num = 3;
						text += newEquation[i];
						break;
					}
					symbol.SymbolName = newEquation[i].ToString();
					symbol.SymbolValue = 0.0;
					switch (symbol.SymbolName)
					{
					case ",":
						symbol.SymbolType = Type.Comma;
						break;
					case "(":
					case ")":
					case "[":
					case "]":
					case "{":
					case "}":
						symbol.SymbolType = Type.Bracket;
						break;
					default:
						symbol.SymbolType = Type.Operator;
						break;
					}
					equation.Add(symbol);
					break;
				case 2:
					if (char.IsNumber(newEquation[i]) || newEquation[i] == '.' || (currentCulture.NumberFormat.NumberDecimalSeparator.Length == 1 && newEquation[i] == currentCulture.NumberFormat.NumberDecimalSeparator[0]))
					{
						text += newEquation[i];
					}
					else if (!char.IsLetter(newEquation[i]))
					{
						num = 1;
						symbol.SymbolName = text;
						flag = double.TryParse(text, NumberStyles.Any, currentCulture, out symbol.SymbolValue);
						if (!flag)
						{
							flag = double.TryParse(text, NumberStyles.Any, provider, out symbol.SymbolValue);
						}
						symbol.SymbolType = Type.Value;
						equation.Add(symbol);
						symbol.SymbolName = newEquation[i].ToString();
						symbol.SymbolValue = 0.0;
						switch (symbol.SymbolName)
						{
						case ",":
							symbol.SymbolType = Type.Comma;
							break;
						case "(":
						case ")":
						case "[":
						case "]":
						case "{":
						case "}":
							symbol.SymbolType = Type.Bracket;
							break;
						default:
							symbol.SymbolType = Type.Operator;
							break;
						}
						equation.Add(symbol);
						text = string.Empty;
					}
					break;
				case 3:
					if (char.IsLetterOrDigit(newEquation[i]))
					{
						text += newEquation[i];
						break;
					}
					num = 1;
					symbol.SymbolName = text;
					symbol.SymbolValue = 0.0;
					if (newEquation[i] == '(')
					{
						symbol.SymbolType = Type.Function;
					}
					else
					{
						if (symbol.SymbolName == "pi")
						{
							symbol.SymbolValue = Math.PI;
						}
						else if (symbol.SymbolName == "e")
						{
							symbol.SymbolValue = Math.E;
						}
						symbol.SymbolType = Type.Variable;
					}
					equation.Add(symbol);
					symbol.SymbolName = newEquation[i].ToString();
					symbol.SymbolValue = 0.0;
					switch (symbol.SymbolName)
					{
					case ",":
						symbol.SymbolType = Type.Comma;
						break;
					case "(":
					case ")":
					case "[":
					case "]":
					case "{":
					case "}":
						symbol.SymbolType = Type.Bracket;
						break;
					default:
						symbol.SymbolType = Type.Operator;
						break;
					}
					equation.Add(symbol);
					text = string.Empty;
					break;
				}
			}
			if (text != string.Empty)
			{
				symbol.SymbolName = text;
				if (num == 2)
				{
					flag = double.TryParse(text, NumberStyles.Any, currentCulture, out symbol.SymbolValue);
					if (!flag)
					{
						flag = double.TryParse(text, NumberStyles.Any, provider, out symbol.SymbolValue);
					}
					if (!flag)
					{
						evaluationError = true;
					}
					symbol.SymbolType = Type.Value;
				}
				else
				{
					if (symbol.SymbolName == "pi")
					{
						symbol.SymbolValue = Math.PI;
					}
					else if (symbol.SymbolName == "e")
					{
						symbol.SymbolValue = Math.E;
					}
					else
					{
						symbol.SymbolValue = 0.0;
					}
					symbol.SymbolType = Type.Variable;
				}
				equation.Add(symbol);
			}
			return flag;
		}

		protected Symbol Evaluate(Symbol sym1, Symbol opr, Symbol sym2)
		{
			Symbol result = default(Symbol);
			result.SymbolName = sym1.SymbolName + opr.SymbolName + sym2.SymbolName;
			result.SymbolType = Type.Result;
			result.SymbolValue = 0.0;
			switch (opr.SymbolName)
			{
			case "^":
				result.SymbolValue = Math.Pow(sym1.SymbolValue, sym2.SymbolValue);
				break;
			case "/":
				if (sym2.SymbolValue > 0.0)
				{
					result.SymbolValue = sym1.SymbolValue / sym2.SymbolValue;
					break;
				}
				result.SymbolName = "Divide by Zero";
				result.SymbolType = Type.Error;
				break;
			case "*":
				result.SymbolValue = sym1.SymbolValue * sym2.SymbolValue;
				break;
			case "%":
				result.SymbolValue = sym1.SymbolValue % sym2.SymbolValue;
				break;
			case "+":
				result.SymbolValue = sym1.SymbolValue + sym2.SymbolValue;
				break;
			case "-":
				result.SymbolValue = sym1.SymbolValue - sym2.SymbolValue;
				break;
			default:
				result.SymbolType = Type.Error;
				result.SymbolName = "Undefine operator: " + opr.SymbolName + ".";
				break;
			}
			return result;
		}

		protected Symbol EvaluateFunction(string name, params object[] args)
		{
			Symbol result = default(Symbol);
			result.SymbolName = string.Empty;
			result.SymbolType = Type.Result;
			result.SymbolValue = 0.0;
			switch (name)
			{
			case "cos":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Cos(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "sin":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Sin(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "tan":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Tan(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "cosh":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Cosh(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "sinh":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Sinh(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "tanh":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Tanh(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "log":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Log10(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "ln":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Log(((Symbol)args[0]).SymbolValue, 2.0);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "logn":
				if (args.Length == 2)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + "'" + ((Symbol)args[1]).SymbolValue + ")";
					result.SymbolValue = Math.Log(((Symbol)args[0]).SymbolValue, ((Symbol)args[1]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "sqrt":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Sqrt(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "abs":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Abs(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "acos":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Acos(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "asin":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Asin(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "atan":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Atan(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			case "exp":
				if (args.Length == 1)
				{
					result.SymbolName = name + "(" + ((Symbol)args[0]).SymbolValue + ")";
					result.SymbolValue = Math.Exp(((Symbol)args[0]).SymbolValue);
				}
				else
				{
					result.SymbolName = "Invalid number of parameters in: " + name + ".";
					result.SymbolType = Type.Error;
				}
				break;
			default:
				if (DefaultFunctionEvaluation != null)
				{
					result = DefaultFunctionEvaluation(name, args);
					return result;
				}
				result.SymbolName = "Function: " + name + ", not found.";
				result.SymbolType = Type.Error;
				break;
			}
			return result;
		}

		protected int Precedence(Symbol sym)
		{
			int result = -1;
			switch (sym.SymbolType)
			{
			case Type.Bracket:
				result = 5;
				break;
			case Type.Function:
				result = 4;
				break;
			case Type.Comma:
				result = 0;
				break;
			}
			switch (sym.SymbolName)
			{
			case "^":
				result = 3;
				break;
			case "/":
			case "*":
			case "%":
				result = 2;
				break;
			case "+":
			case "-":
				result = 1;
				break;
			}
			return result;
		}
	}
}
