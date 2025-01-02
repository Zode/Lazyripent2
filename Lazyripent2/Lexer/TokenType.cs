namespace Lazyripent2.Lexer;

public enum TokenType
{
	//single characters
	LeftParenthesis, RightParenthesis, // ( )
	LeftBrace, RightBrace, // { }
	LeftBracket, RightBracket, // [ ]
	Comma, Dot, Colon, Semicolon, // . , : ;
	Minus, Plus, Slash, Star, // - + / *
	BackwardsSlash, AtSign, Tilde, // \ @ ~

	//single or two characters
	Bang, Equal, Greater, Less, // ! = > <
	
	//two caracters
	BangEqual, EqualEqual, GreaterEqual, LessEqual, // != == >= <=

	//literals
	Identifier, String, Number,

	//keywords
	NullTerminator,

	//special
	Whitespace,
}
