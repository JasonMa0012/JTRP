// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Helper class to deal with template expressions

namespace ToonyColorsPro
{
	namespace Utilities
	{
		public static class ExpressionParser
		{
			//--------------------------------------------------------------------------------------------------------------------------------
			// High-Level: process line with /// condition tags

			public static string ProcessCondition(string line, List<string> features, ref int depth, ref List<bool> stack, ref List<bool> done)
			{
				//Safeguard for commented or special command lines
				if (line.Length > 0 && line[0] == '#')
				{
					return null;
				}

				//Remove white spaces
				line = line.Trim();

				var parts = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 1 && parts[0] == "///")  //END TAG
				{
					if (depth < 0)
					{
						return "Found end tag /// without any beginning";
					}

					stack.RemoveAt(depth);
					done.RemoveAt(depth);
					depth--;
				}
				else if (parts.Length >= 2)
				{
					if (parts[1] == "IF")
					{
						var cond = false;
						var error = EvaluateExpression(ref cond, features, parts);

						if (!string.IsNullOrEmpty(error))
							return error;

						depth++;
						stack.Add(cond && ((depth <= 0) ? true : stack[depth - 1]));
						done.Add(cond);
					}
					else if (parts[1] == "ELIF")
					{
						if (done[depth])
						{
							stack[depth] = false;
							return null;
						}

						var cond = false;
						var error = EvaluateExpression(ref cond, features, parts);

						if (!string.IsNullOrEmpty(error))
							return error;

						stack[depth] = cond && ((depth <= 0) ? true : stack[depth - 1]);
						done[depth] = cond;
					}
					else if (parts[1] == "ELSE")
					{
						if (done[depth])
						{
							stack[depth] = false;
							return null;
						}

						stack[depth] = ((depth <= 0) ? true : stack[depth - 1]);
						done[depth] = true;
					}
				}

				return null;
			}

			//New evaluation system with parenthesis and complex expressions support
			private static string EvaluateExpression(ref bool conditionResult, List<string> features, params string[] conditions)
			{
				if (conditions.Length <= 2)
				{
					return "Invalid condition block";
				}

				var expression = "";
				for (var n = 2; n < conditions.Length; n++)
				{
					expression += conditions[n];
				}

				var result = false;
				try
				{
					ExpressionLeaf.EvaluateFunction evalFunc = s => features.Contains(s);
					result = EvaluateExpression(expression, evalFunc);
				}
				catch (Exception e)
				{
					return "Incorrect condition in template file\nError returned:\n" + e.Message;
				}

				conditionResult = result;
				return null;
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			// Main Function to use

			public static bool EvaluateExpression(string expression, ExpressionLeaf.EvaluateFunction evalFunction)
			{
				//Remove white spaces and double && ||
				var cleanExpr = new StringBuilder();
				for (var i = 0; i < expression.Length; i++)
				{
					switch (expression[i])
					{
						case ' ': break;
						case '&': cleanExpr.Append(expression[i]); i++; break;
						case '|': cleanExpr.Append(expression[i]); i++; break;
						default: cleanExpr.Append(expression[i]); break;
					}
				}

				var tokens = new List<Token>();
				var reader = new StringReader(cleanExpr.ToString());
				Token t = null;
				do
				{
					t = new Token(reader);
					tokens.Add(t);
				} while (t.type != Token.TokenType.EXPR_END);

				var polishNotation = Token.TransformToPolishNotation(tokens);

				var enumerator = polishNotation.GetEnumerator();
				enumerator.MoveNext();
				var root = MakeExpression(ref enumerator, evalFunction);

				return root.Evaluate();
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			// Expression Token

			public class Token
			{
				bool StringToTokenType(char c)
				{
					switch (c)
					{
						case '(':
							type = TokenType.OPEN_PAREN;
							value = "(";
							return true;

						case ')':
							type = TokenType.CLOSE_PAREN;
							value = ")";
							return true;

						case '!':
							type = TokenType.UNARY_OP;
							value = "NOT";
							return true;

						case '&':
							type = TokenType.BINARY_OP;
							value = "AND";
							return true;

						case '|':
							type = TokenType.BINARY_OP;
							value = "OR";
							return true;
					}
					return false;
				}

				bool IsControlCharacter(char c)
				{
					return c == '(' || c == ')' || c == '!' || c == '&' || c == '|';
				}

				public enum TokenType
				{
					OPEN_PAREN,
					CLOSE_PAREN,
					UNARY_OP,
					BINARY_OP,
					LITERAL,
					EXPR_END
				}

				public TokenType type;
				public string value;

				public Token(StringReader s)
				{
					var c = s.Read();
					if (c == -1)
					{
						type = TokenType.EXPR_END;
						value = "";
						return;
					}

					var ch = (char)c;

					//Special case: solve bug where !COND_FALSE_1 && COND_FALSE_2 would return True
					bool embeddedNot = (ch == '!' && s.Peek() != '(');

					// Control character
					if (!embeddedNot && StringToTokenType(ch))
					{
						return;
					}

					// Literal expression
					var sb = new StringBuilder();
					sb.Append(ch);
					while (s.Peek() != -1 && !IsControlCharacter((char)s.Peek()))
					{
						sb.Append((char)s.Read());
					}
					type = TokenType.LITERAL;
					value = sb.ToString();
				}

				public static List<Token> TransformToPolishNotation(List<Token> infixTokenList)
				{
					var outputQueue = new Queue<Token>();
					var stack = new Stack<Token>();

					var index = 0;
					while (infixTokenList.Count > index)
					{
						var t = infixTokenList[index];

						switch (t.type)
						{
							case TokenType.LITERAL:
								outputQueue.Enqueue(t);
								break;
							case TokenType.BINARY_OP:
							case TokenType.UNARY_OP:
							case TokenType.OPEN_PAREN:
								stack.Push(t);
								break;
							case TokenType.CLOSE_PAREN:
								while (stack.Peek().type != TokenType.OPEN_PAREN)
								{
									outputQueue.Enqueue(stack.Pop());
								}
								stack.Pop();
								if (stack.Count > 0 && stack.Peek().type == TokenType.UNARY_OP)
								{
									outputQueue.Enqueue(stack.Pop());
								}
								break;
							default:
								break;
						}

						index++;
					}
					while (stack.Count > 0)
					{
						outputQueue.Enqueue(stack.Pop());
					}

					var list = new List<Token>(outputQueue);
					list.Reverse();
					return list;
				}
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			// Boolean Expression Classes

			public abstract class Expression
			{
				public abstract bool Evaluate();
			}

			public class ExpressionLeaf : Expression
			{
				public delegate bool EvaluateFunction(string content);
				private string content;
				private EvaluateFunction evalFunction;

				public ExpressionLeaf(EvaluateFunction _evalFunction, string _content)
				{
					evalFunction = _evalFunction;
					content = _content;
				}

				public override bool Evaluate()
				{
					//embedded not, see special case in Token declaration
					if (content.Length > 0 && content[0] == '!')
					{
						return !evalFunction(content.Substring(1));
					}

					return evalFunction(content);
				}
			}

			public class ExpressionAnd : Expression
			{
				private Expression left;
				private Expression right;

				public ExpressionAnd(Expression _left, Expression _right)
				{
					left = _left;
					right = _right;
				}

				public override bool Evaluate()
				{
					return left.Evaluate() && right.Evaluate();
				}
			}

			public class ExpressionOr : Expression
			{
				private Expression left;
				private Expression right;

				public ExpressionOr(Expression _left, Expression _right)
				{
					left = _left;
					right = _right;
				}

				public override bool Evaluate()
				{
					return left.Evaluate() || right.Evaluate();
				}
			}

			public class ExpressionNot : Expression
			{
				private Expression expr;

				public ExpressionNot(Expression _expr)
				{
					expr = _expr;
				}

				public override bool Evaluate()
				{
					return !expr.Evaluate();
				}
			}

			public static Expression MakeExpression(ref List<Token>.Enumerator polishNotationTokensEnumerator, ExpressionLeaf.EvaluateFunction _evalFunction)
			{
				if (polishNotationTokensEnumerator.Current.type == Token.TokenType.LITERAL)
				{
					Expression lit = new ExpressionLeaf(_evalFunction, polishNotationTokensEnumerator.Current.value);
					polishNotationTokensEnumerator.MoveNext();
					return lit;
				}

				if (polishNotationTokensEnumerator.Current.value == "NOT")
				{
					polishNotationTokensEnumerator.MoveNext();
					var operand = MakeExpression(ref polishNotationTokensEnumerator, _evalFunction);
					return new ExpressionNot(operand);
				}

				if (polishNotationTokensEnumerator.Current.value == "AND")
				{
					polishNotationTokensEnumerator.MoveNext();
					var left = MakeExpression(ref polishNotationTokensEnumerator, _evalFunction);
					var right = MakeExpression(ref polishNotationTokensEnumerator, _evalFunction);
					return new ExpressionAnd(left, right);
				}

				if (polishNotationTokensEnumerator.Current.value == "OR")
				{
					polishNotationTokensEnumerator.MoveNext();
					var left = MakeExpression(ref polishNotationTokensEnumerator, _evalFunction);
					var right = MakeExpression(ref polishNotationTokensEnumerator, _evalFunction);
					return new ExpressionOr(left, right);
				}
				return null;
			}
		}
	}
}