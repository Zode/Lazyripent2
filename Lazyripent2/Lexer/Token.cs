namespace Lazyripent2.Lexer;

public class Token(TokenType type, int startIndex, int endIndex, object? literal, int line)
{
    public TokenType Type {get; private set;} = type;
    public int StartIndex {get; private set;} = startIndex;
    public int EndIndex {get; private set;} = endIndex;
	public object? Literal {get; private set;} = literal;
	public int Line {get; private set;} = line;

	public string GetLexeme(string source)
	{
		return source[StartIndex..EndIndex];
	}

	public string ToString(string source)
	{
		if(Type == TokenType.NullTerminator)
		{
			return ToString();
		}

		if(Literal is not null)
		{
			return $"[Token of type: {Type}, lexeme {GetLexeme(source)}, line {Line}, Literal: {Literal} ({Literal.GetType()})]";
		}

		return $"[Token of type: {Type}, lexeme {GetLexeme(source)}, line {Line}]";
	}

	public override string ToString()
	{
		if(Literal is not null)
		{
			return $"[Token of type: {Type}, indices: {StartIndex} to {EndIndex}, line {Line}, Literal: {Literal} ({Literal.GetType()})]";
		}

		return $"[Token of type: {Type}, indices: {StartIndex} to {EndIndex}, line {Line}]";
	}
}