using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using sharplox.Expressions;

namespace sharplox;

public class Parser(List<Token> tokens)
{
    internal class ParseError() : Exception {}

    private readonly List<Token> _tokens = tokens;

    private int current = 0;

    internal bool isRepl = false;

    public List<Stmt> Parse()
    {
        List<Stmt> statements = [];
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    // declaration -> funDecl
    //              | varDecl
    //              | statement ;
    private Stmt Declaration()
    {
        try
        {
            if (Match(TokenType.FUN))
            {
                return Function("function");
            }
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

    // function -> IDENTIFIER "(" parameters? ")" block ;
    private Stmt Function(string kind)
    {
        Token name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");

        Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");

        // parameters -> IDENTIFIER ( "," IDENTIFIER )* ; 
        List<Token> parameters = [];

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if(parameters.Count > 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (Match(TokenType.COMMA));    
        }

        Consume(TokenType.RIGHT_PAREN, $"Expect ')' after parameters.");
        Consume(TokenType.LEFT_BRACE, "Expect '{' before" + kind + "body.");

        List<Stmt> body = Block();
        return new Stmt.Function(name, parameters, body);
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect variable name");

        Expr? initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration");
        return new Stmt.Var(name, initializer);
    }

    // statement -> exprStmt
    //            | IfStmt
    //            | fotStmt
    //            | printStmt
    //            | whileStmt
    //            | returnStmt
    //            | block ;
    private Stmt Statement()
    {
        if (Match(TokenType.FOR))
        {
            return ForStatement();
        }
        if (Match(TokenType.IF))
        {
            return IfStatement();
        }
        if (Match(TokenType.PRINT))
        {
            return PrintStatement();
        }
        if (Match(TokenType.WHILE))
        {
            return WhileStatement();
        }
        if (Match(TokenType.LEFT_BRACE))
        {
            return new Stmt.Block(Block());
        }
        if (Match(TokenType.RETURN))
        {
            return ReturnStatement();
        }

        return ExpressionStatement();
    }

    // returnStmt -> "return" expression? ";"
    private Stmt ReturnStatement()
    {
        Token keyword = Previous();
        Expr value = null;

        if (!Check(TokenType.SEMICOLON))
        {
            value = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after return value");
        return new Stmt.Return(keyword, value);
    }

    // forStmt -> "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" expression ";" statement ;
    private Stmt ForStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'");

        // (";" | varDecl | exprStmt )
        Stmt initializer;
        if (Match(TokenType.SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(TokenType.VAR))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        // expression?
        Expr condition = null;

        if (!Check(TokenType.SEMICOLON))
        {
            condition = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

        // expression?
        Expr increment = null;

        if (!Check(TokenType.RIGHT_PAREN))
        {
            increment = Expression();
        }

        Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

        // statement
        Stmt body = Statement();

        if(increment != null)
        {
            body = new Stmt.Block([body, new Stmt.Expression(increment)]);
        }

        condition ??= new Expr.Literal(true);

        body = new Stmt.While(condition, body);

        if(initializer != null)
        {
            body = new Stmt.Block([initializer, body]);
        }

        return body;
    }

    // ifStmt -> "if" "(" condition ")" statement ("else" statement)? ;
    private Stmt IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        Expr condition = Expression();

        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

        Stmt thenBranch = Statement();
        Stmt elseBranch = null;
        if (Match(TokenType.ELSE))
        {
            elseBranch = Statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = [];

        while(!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        
        return statements;
    }

    // exprStmt -> expression ";" ;
    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();

        if (isRepl)
        {
            return new Stmt.Expression(expr);
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after expression");

        return new Stmt.Expression(expr);
    }

    // whileStmt -> "while" "(" expression ")" statement ;
    private Stmt WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition");

        Stmt body = Statement();
        return new Stmt.While(condition, body);
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
    //             | logic_or ;
    private Expr Assignment()
    {
        Expr expr = Or();

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

    // logical_or -> logic_and ("OR" logic_and)* ;
    private Expr Or()
    {
        Expr expr = And();

        while (Match(TokenType.OR))
        {
            Token op = Previous();
            Expr right = And();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    // logic_and -> equality ("AND" equality)* ;
    private Expr And()
    {
        Expr expr = Equality();

        while (Match(TokenType.AND))
        {
            Token op = Previous();
            Expr right = Equality();
            expr = new Expr.Logical(expr, op, right);
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
    //        |  call ;
    private Expr Unary()
    {
        if(Match(TokenType.BANG, TokenType.MINUS))
        {
            Token op = Previous();
            Expr right = Unary();
            return new Expr.Unary(op, right);
        }

        return Call();
    }

    // call -> primary ( "(" arguments? ")" )* ;
    private Expr Call()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    // 
    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = [];

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if(arguments.Count >= 255)
                {
                    Error(Peek(), " Can't have mor than 255 arguments.");
                }
                arguments.Add(Expression());
            } while (Match(TokenType.COMMA));
        }

        Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

        return new Expr.Call(callee, paren, arguments);
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