using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using sharplox.Expressions;
using static sharplox.Expressions.Stmt;

namespace sharplox;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
{
    private static readonly Environment _globals = new();
    private Environment environment = _globals;

    public Interpreter()
    {
        _globals.Define("clock", new ClockNativeFunction());
}

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }
        catch (RuntimeError err)
        {
            Program.RuntimeError(err);
            throw;
        }
    }

    public void InterpretRepl(List<Stmt> statements)
    {
        try
        {
            foreach (var stmt in statements)
            {
                if(stmt.GetType().ToString() == "sharplox.Expressions.Stmt+Expression")
                {
                    object obj = VisitExpressionStmtRepl((Expression)stmt);
                    Console.WriteLine(obj);
                }
                else
                {
                    Execute(stmt);
                }
            }
        }
        catch (RuntimeError err)
        {
            Program.RuntimeError(err);
            throw;
        }
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
        object value = Evaluate(expr._value);
        environment.Assign(expr._name, value);

        return value;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        object left = Evaluate(expr._left);
        object right = Evaluate(expr._right);

        switch (expr._op.tokenType)
        {
            case TokenType.EQUAL_EQUAL:
                return IsEqual(left, right);
            case TokenType.BANG_EQUAL:
                return !IsEqual(left, right);
            case TokenType.GREATER:
                CheckNumberOperands(expr._op, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(expr._op, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckNumberOperands(expr._op, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(expr._op, left, right);
                return (double)left <= (double)right;
            case TokenType.PLUS:
                if(left is double doubleLeft && right is double doubleRight)
                {
                    return doubleLeft + doubleRight;
                }

                if(left is string strLeft && right is string strRight)
                {
                    return strLeft + strRight;
                }

                throw new RuntimeError(expr._op, "Operands must be two numbers or two strings.");
            case TokenType.MINUS:
                CheckNumberOperands(expr._op, left, right);
                return (double)left - (double)right;
            case TokenType.SLASH:
                CheckNumberOperands(expr._op, left, right);
                IsNotDivisedByZero(expr._op, right);
                return (double)left / (double)right;
            case TokenType.STAR:
                CheckNumberOperands(expr._op, left, right);
                return (double)left * (double)right; 
        }

        return null;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        throw new NotImplementedException();
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr._value;
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        Object right = Evaluate(expr._right);

        switch (expr._op.tokenType)
        {
            case TokenType.MINUS:
                CheckNumberOperand(expr._op, right);
                return -(double)right;
            case TokenType.BANG:
                return !IsTruthy(right);
        }

        return null;
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
        var value = environment.Get(expr._name);

        if(value is null)
        {
            throw new RuntimeError(expr._name, "The variable must be initialized before accessed.");
        }

        return value;
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        object left = Evaluate(expr._left);

        if(expr._op.tokenType == TokenType.OR)
        {
            if (IsTruthy(left))
            {
                return left;
            }
        }
        else
        {
            if (IsTruthy(left))
            {
                return left;
            }
        }

        return Evaluate(expr._right);
    }

    public object VisitCallExpr(Expr.Call expr)
    {
        object callee = Evaluate(expr._callee);
        List<object> arguments = [];
        
        foreach (var argument in expr._arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if(!(callee is SharploxCallable callable))
        {
            throw new RuntimeError(expr._paren, "Can only call functions and classes");
        }

        ISharploxCallable function = (ISharploxCallable)callee;

        if(arguments.Count != function.Arity())
        {
            throw new RuntimeError(expr._paren, $"Expect {function.Arity()} arguments but got {arguments.Count}.");
        }
        return function.Call(this, arguments);
    }

    public object VisitExpressionStmtRepl(Stmt.Expression stmt)
    {
        return Evaluate(stmt._expression);
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt._expression);
        return null;
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
        object value = Evaluate(stmt._expression);
        Console.WriteLine(Stringify(value));

        return null;
    }

    public object VisitVarStmt(Stmt.Var stmt)
    {
        object value = null;

        if(stmt._initializer != null)
        {
            value = Evaluate(stmt._initializer);
        }

        environment.Define(stmt._name.lexeme, value);

        return null;
    }

    public object VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt._statements, new Environment(environment));
        return null;
    }

    public object VisitIfStmt(If stmt)
    {
        if (IsTruthy(Evaluate(stmt._condition)))
        {
            Execute(stmt._thenBranch);
        }
        else if(stmt._elseBranch is not null)
        {
            Execute(stmt._elseBranch);
        }

        return null;
    }

    public object VisitWhileStmt(While stmt)
    {
        while (IsTruthy(Evaluate(stmt._condition)))
        {
            Execute(stmt._body);
        }

        return null;
    }

    private void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        // set global environment to this variable
        Environment previous = this.environment;

        try
        {
            // now the global one is the scoped env
            this.environment = environment;

            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            // than the global becomes the outer env.
            this.environment = previous;
        }
    }

    private bool IsTruthy(object obj)
    {
        if(obj == null)
        {
            return false;
        }
        if(obj is bool boolObj)
        {
            return boolObj;
        }
        return true;
    }

    private Object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private static bool IsEqual(object? left, object? right)
    {
        if(left == null && right == null)
        {
            return true;
        }
        
        if(left == null)
        {
            return false;
        }

        return left.Equals(right);
    }

    private void CheckNumberOperand(Token op, object operand)
    {
        if(operand is double doubleOperand)
        {
            return;
        }

        throw new RuntimeError(op, "Operand must be a number.");
    }

    private void CheckNumberOperands(Token op, object left, object right)
    {
        if(left is double leftOperand && right is double rightOperand)
        {
            return;
        }

        throw new RuntimeError(op, "Operands must be a numbers.");
    }

    private static void IsNotDivisedByZero(Token op, object right)
    {
        if(op.tokenType == TokenType.SLASH && (double)right == 0)
        {
            throw new RuntimeError(op, "You cannot divide by zero");
        }

        return;
    }

    private static string Stringify(object value)
    {
        if(value is null)
        {
            return "nil";
        }

        if(value is double doubleValue)
        {
            string text = doubleValue.ToString();

            if (text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }

        return value.ToString();
    }

    public object VisitFunctionStmt(Function stmt)
    {
        throw new NotImplementedException();
    }
}