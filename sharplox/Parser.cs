using sharplox.Expressions;

namespace sharplox;

public class Parser(List<Token> tokens)
{
    private readonly List<Token> _tokens = tokens;

    private int current = 0;

    private Expr Expression()
    {
        return Equality();
    }


    // equality -> comparison ( ( "!=" | "==" ) comparison )* ; 
    private Expr Equality()
    {
        Expr expr = Comparison();
        while(Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            Token op = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    // comparison -> term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
    private Expr Comparison()
    {
        Expr expr = Term();
        while(Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            Token op = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        throw new NotImplementedException();
    }

    private Token Previous()
    {
        return _tokens[current-1];
    }

    private bool Match(params TokenType[] types)
    {
        foreach(var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            current++;
        }
        return Previous();
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd())
        {
            return false;
        }

        return Peek().tokenType == type;
    }

    private Token Peek()
    {
        return _tokens[current];
    }

    private bool IsAtEnd()
    {
        return Peek().tokenType == TokenType.EOF;
    }
}
