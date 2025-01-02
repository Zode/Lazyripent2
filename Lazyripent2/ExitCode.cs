using System.Text;
using Lazyripent2.Lexer;

namespace Lazyripent2;

/// <summary>
/// Helper class to print output and exit the app
/// </summary>
public static class ExitCode
{
	public static void Cancelled()
	{
		Console.WriteLine("Cancelled");
		Environment.Exit((int)ExitCodes.Success);
	}

	public static void CheckWarningAsFatal(FileParser? fileParser = null)
	{
		if(!Options.WarningsAsFatal)
		{
			return;
		}

		Console.WriteLine("Treating warning as fatal");
		if(fileParser is not null)
		{
			ShowWhere(fileParser.GetCaret(), fileParser.GetLexer().Source);
		}

		Environment.Exit((int)ExitCodes.Success);
	}

	public static void InvalidCommandLine(string message)
	{
		ShowError($"Error: {message}");
		Program.ShowUsage();
		Environment.Exit((int)ExitCodes.InvalidCommandLine);
	}

	public static void Failure(Exception ex)
	{
		ShowError($"Error: {ex.Message}");
		ShowStacktrace(ex.StackTrace);
		Environment.Exit((int)ExitCodes.Failure);
	}

	public static void Failure(Exception ex, int caret, string source)
	{
		ShowError($"Error: {ex.Message}");
		ShowStacktrace(ex.StackTrace);
		ShowWhere(caret, source);
		Environment.Exit((int)ExitCodes.Failure);
	}

	public static void Failure(Exception ex, int caret, LexerScanner lexer, int tokenIndex)
	{
		ShowError($"Error: {ex.Message}");
		ShowStacktrace(ex.StackTrace);
		ShowLexerDump(tokenIndex, lexer);
		ShowWhere(caret, lexer.Source);
		Environment.Exit((int)ExitCodes.Failure);
	}

	public static void Failure(Exception ex, FileParser fileParser)
	{
		Failure(ex, fileParser.GetCaret(), fileParser.GetLexer(), fileParser.TokenIndex);
	}

	private static void ShowError(string message)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(message);
		Console.ForegroundColor = ConsoleColor.White;
	}

	private static void ShowWhere(int caret, string source)
	{
		ShowError($"Where in source:");
		StringBuilder sb = new();
		int indexFromLeft = 0;
		for(int i = Math.Max(0, caret - 128); i < caret; i++)
		{
			if(source[i] == '\n')
			{
				indexFromLeft = 0;
			}
			else
			{
				indexFromLeft++;
			}

			if(source[i] == '\t')
			{
				indexFromLeft += 4;
				sb.Append("    ");
				continue;
			}

			sb.Append(source[i]);
		}
		Console.WriteLine(sb.ToString());
		sb.Clear();

		for(int i = 0; i < indexFromLeft; i++)
		{
			sb.Append(' ');
		}

		sb.Append($"^~~ about here ({caret})");
		ShowError(sb.ToString());
		sb.Clear();

		for(int i = caret; i < Math.Min(source.Length, caret + 128); i++)
		{
			if(source[i] == '\t')
			{
				sb.Append("    ");
				continue;
			}
			
			sb.Append(source[i]);
		}

		Console.WriteLine(sb.ToString());
	}

	private static void ShowLexerDump(int tokenIndex, LexerScanner lexer)
	{
		if(!Options.Verbose)
		{
			return;
		}

		ShowError($"Lexer dump:");
		for(int i = Math.Max(0, tokenIndex - 32); i < tokenIndex; i++)
		{
			Console.WriteLine($"{i} : {lexer.Tokens[i].ToString(lexer.Source)}");
		}

		ShowError($"^^^ tokenIndex ({tokenIndex}) ^^^");
		for(int i = tokenIndex; i < Math.Min(lexer.Tokens.Count, tokenIndex + 32); i++)
		{
			Console.WriteLine($"{i} : {lexer.Tokens[i].ToString(lexer.Source)}");
		}
	}

	private static void ShowStacktrace(string? trace)
	{
		if(!Options.Verbose || trace is null)
		{
			return;
		}

		Console.ForegroundColor = ConsoleColor.DarkGray;
		Console.WriteLine(trace);
		Console.ForegroundColor = ConsoleColor.White;
	}
}