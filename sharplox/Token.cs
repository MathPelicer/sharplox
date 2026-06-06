namespace sharplox;

public class Token(TokenType tokenType, string lexeme, object literal, int line)
{
    public readonly TokenType tokenType = tokenType;
    public readonly string lexeme = lexeme;
    public readonly object literal = literal;
    readonly int _line = line;

    public override string ToString()
    {
        return $"{tokenType} {lexeme} {literal}";
    }
}
