namespace sharplox;

public class Token(TokenType tokenType, string lexeme, object literal, int line)
{
    readonly TokenType _tokenType = tokenType;
    public readonly string _lexeme = lexeme;
    readonly object _literal = literal;
    readonly int _line = line;

    public override string ToString()
    {
        return $"{_tokenType} {_lexeme} {_literal}";
    }
}
