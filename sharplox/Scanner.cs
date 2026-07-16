namespace sharplox;

public class Scanner(string source)
{
    private readonly string _source = source;
    private readonly List<Token> _tokens = [];

    private int start = 0; // first char in the lexeme thats being scanned
    private int current = 0; // current char being considered

    private int lexemeSize = 0;
    private int line = 1;

    private static readonly Dictionary<string, TokenType> _keywords = new()
    {
        {"and", TokenType.AND},
        {"class", TokenType.CLASS},
        {"else", TokenType.ELSE},
        {"false", TokenType.FALSE},
        {"for", TokenType.FOR},
        {"fun", TokenType.FUN},
        {"if", TokenType.IF},
        {"nil", TokenType.NIL},
        {"or", TokenType.OR},
        {"print", TokenType.PRINT},
        {"return", TokenType.RETURN},
        {"super", TokenType.SUPER},
        {"this", TokenType.THIS},
        {"true", TokenType.TRUE},
        {"var", TokenType.VAR},
        {"while", TokenType.WHILE}
    };

    public List<Token> ScanTokens()
    {
        while(!IsAtEnd())
        {
            start = current;
            lexemeSize = 0;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.EOF, "", null, line));
        return _tokens;
    }

    private void ScanToken()
    {
        char test = _source.ElementAt(0);
        char test2 = _source.ElementAt(1);
        char c = Advance();

        switch (c)
        {
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '.': AddToken(TokenType.DOT); break;
            case '+': AddToken(TokenType.PLUS); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '*': AddToken(TokenType.STAR); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); 
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); 
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); 
                break; 
            case '/':
                if (Match('/')) // handling commentary
                {
                    while(Peek() != '\n' && !IsAtEnd())
                    {
                        Advance();
                    }
                }
                else
                {
                    AddToken(TokenType.SLASH);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                line++;
                break;
            case '"':
                StringScan();
                break;
            default:
                if (IsDigit(c))
                {
                    Number();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    Program.Error(line, "Unexpected character.");
                }
                break;
        }
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())){
            Advance();
        }

        string text = _source.Substring(start, current);
        
        if(!_keywords.TryGetValue(text, out TokenType tokenType))
        {
            tokenType = TokenType.IDENTIFIER;
        }

        AddToken(tokenType);
    }

    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
                c == '_';
    }

    private void Number()
    {
        while (IsDigit(Peek()))
        {
            Advance();
        }

        if(Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();
            
            while (IsDigit(Peek()))
            {
                Advance();
            }
        }

        AddToken(TokenType.NUMBER, double.Parse(_source.Substring(start, lexemeSize)));
    }

    private char PeekNext()
    {
        var nextCharIndex = current + 1;
        if (nextCharIndex >= _source.Length)
        {
            return '\0';
        }

        return _source.ElementAt(nextCharIndex);
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private void StringScan()
    {
        while(Peek() != '"' && !IsAtEnd())
        {
            if(Peek() == '\n')
            {
                line++;
            }
            
            Advance();
        }

        if(IsAtEnd())
        {
            Program.Error(line, "Unterminated string.");
            return;
        }

        Advance();

        string value = _source.Substring(start + 1, lexemeSize-2);
        AddToken(TokenType.STRING, value);
    }

    private char Peek()
    {
        if (IsAtEnd())
        {
            return '\0';
        }

        return _source.ElementAt(current);
    }

    private bool Match(char expected)
    {
        if (IsAtEnd())
        {
            return false;
        }

        if(_source.ElementAt(current) != expected)
        {
            return false;
        }

        // also advances the current count
        current++;
        return true;
    }

    private char Advance()
    {
        lexemeSize++;
        return _source.ElementAt(current++);
    }

    private void AddToken(TokenType tokenType)
    {
        AddToken(tokenType, null);
    }

    private void AddToken(TokenType tokenType, object literal)
    {
        string text = _source.Substring(start, lexemeSize);
        _tokens.Add(new Token(tokenType, text, literal, line));
    }

    private bool IsAtEnd()
    {
        return current >= _source.Length;
    }
}