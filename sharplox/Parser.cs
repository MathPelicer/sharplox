using sharplox.Expressions;

namespace sharplox;

public class Parser(List<Token> tokens)
{
    internal class ParseError() : Exception {}

    private readonly List<Token> _tokens = tokens;

    private int current = 0;

    public List<Stmt> Parse()
    {
        List<Stmt> statements = [];
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    // declaration -> varDecl
    //              | statement ;
    private Stmt Declaration()
    {
        try
        {
            if (Match(TokenType.VAR))
            {
                return VarDeclaration();
            }

            return Statement();
        }
        catch (ParseError err)
        {
            Syncronize();
            return null;
        }
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect variable name");

        Expr? initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' adter variable declaration");
        return new Stmt.Var(name, initializer);
    }

    // statement -> exprStmt
    //            | printStmt
    private Stmt Statement()
    {
        if (Match(TokenType.PRINT))
        {
            return PrintStatement();
        }

        return ExpressionStatement();
    }

    // exprStmt -> expression ";" ;
    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();

        Consume(TokenType.SEMICOLON, "Expect ';' after expression");

        return new Stmt.Expression(expr);
    }

    // printStmt -> "print" expression ";" ; 
    private Stmt PrintStatement()
    {
        Expr value = Expression();

        Consume(TokenType.SEMICOLON, "Expect ';' after value");
        
        return new Stmt.Print(value);
    }

    // expression -> assingment ;
    private Expr Expression()
    {
        return Assignment();
    }

    // assignment -> IDENTIFIER "=" assignment
    //             | equality ;
    private Expr Assignment()
    {
        Expr expr = Equality();

        if (Match(TokenType.EQUAL))
        {
            Token equals = Previous();
            Expr value = Assignment();

            if(expr is Expr.Variable variable)
            {
                Token name = variable._name;
                return new Expr.Assign(name, value);
            }

            Error(equals, "Invalid assingment target.");
        }

        return expr;
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
        else if (Match(TokenType.IDENTIFIER))
        {
            return new Expr.Variable(Previous());
        }
        else if (Match(TokenType.LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, """Expect ')' after expression.""");
            return new Expr.Grouping(expr);
        }
        
        throw Error(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            return Advance();
        }

        throw Error(Peek(), message);
    }

    private Token Previous()
    {
        return _tokens[current-1];
    }

    private static ParseError Error(Token token, string message)
    {
        Program.Error(token, message);
        
        return new ParseError();
    }

    private void Syncronize()
    {
        Advance();
        while (!IsAtEnd())
        {
            if(Previous().tokenType == TokenType.SEMICOLON)
            {
                return;
            }

            switch (Peek().tokenType)
            {
                case TokenType.CLASS:
                case TokenType.FOR:
                case TokenType.FUN:
                case TokenType.IF:
                case TokenType.PRINT:
                case TokenType.RETURN:
                case TokenType.VAR:
                case TokenType.WHILE:
                    return;
            }

            Advance();
        }
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