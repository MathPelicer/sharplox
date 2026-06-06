using System.Runtime.InteropServices;
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

    // term -> factor ( ( "-" | "+" ) factor )* ;
    private Expr Term()
    {
        Expr expr = Factor();
        while(Match(TokenType.MINUS, TokenType.PLUS))
        {
            Token op = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    // factor -> unary ( ( "*" | "/" ) unary )* ;
    private Expr Factor()
    {
        Expr expr = Unary();
        while(Match(TokenType.STAR, TokenType.SLASH))
        {
            Token op = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    // unary -> ( "!" | "-" ) unary
    //        |  primary ;
    private Expr Unary()
    {
        if(Match(TokenType.BANG, TokenType.MINUS))
        {
            Token op = Previous();
            Expr right = Unary();
            return new Expr.Unary(op, right);
        }

        return Primary();
    }

    // primary -> NUMBER | STRING | "true" | "false" | "nil"
    //          | "(" expression ")" ;
    private Expr Primary()
    {
        if (Match(TokenType.FALSE))
        {
            return new Expr.Literal(false);
        }
        else if (Match(TokenType.TRUE))
        {
            return new Expr.Literal(true);
        }
        else if (Match(TokenType.NIL))
        {
            return new Expr.Literal(null);
        }

        else if(Match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Expr.Literal(Previous().literal);
        }
        else if (Match(TokenType.LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, """Expect ')' after expression.""");
            return new Expr.Grouping(expr);
        }
        return null;
    }

    private Token Consume(TokenType type, string message)
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
