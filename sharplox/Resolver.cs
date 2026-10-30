using sharplox.Expressions;

namespace sharplox;

public class Resolver(Interpreter interpreter) : Expr.IVisitor<object>, Stmt.IVisitor<object>
{
    private readonly Interpreter _interpreter = interpreter;

    private readonly Stack<Dictionary<string, bool>> _scopes = new();

    private FunctionType currentFunction = FunctionType.NONE;

    public object VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr._value);
        ResolveLocal(expr, expr._name);
        return null;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr._left);
        Resolve(expr._right);
        return null;
    }

    public object VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt._statements);
        EndScope();

        return null;
    }

    public object VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr._callee);

        foreach (var argument in expr._arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt._expression);
        return null;
    }

    public object VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt._name);
        Define(stmt._name);
        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr._expression);
        return null;
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt._condition);
        Resolve(stmt._thenBranch);
        
        if(stmt._elseBranch != null)
        {
            Resolve(stmt._elseBranch);
        }

        return null;
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr._left);
        Resolve(expr._right);
        return null;
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt._expression);
        return null;
    }

    public object VisitReturnStmt(Stmt.Return stmt)
    {
        if(currentFunction == FunctionType.NONE)
        {
            Program.Error(stmt._keyword, "Can't return from top-level code");    
        }

        if(stmt._value != null)
        {
            Resolve(stmt._value);
        }

        return null;
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr._right);
        return null;
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
        if(_scopes.Any() && _scopes.Peek()[expr._name.lexeme] == false)
        {
            Program.Error(expr._name, "Can't read variable in its own initializer");
        }

        ResolveLocal(expr, expr._name);
        return null;
    }

    public object VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt._name);
        
        if(stmt._initializer != null)
        {
            Resolve(stmt._initializer);    
        }

        Define(stmt._name);
        return null;
    }

    public object VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt._condition);
        Resolve(stmt._body);
        return null;
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void BeginScope()
    {
        _scopes.Push([]);
    }

    private void EndScope()
    {
        _scopes.Pop();
    }

    private void Define(Token name)
    {
        if (!_scopes.Any())
        {
            return;
        }

        _scopes.Peek().Add(name.lexeme, true);
    }

    private void Declare(Token name)
    {
        if (!_scopes.Any())
        {
            return;
        }

        Dictionary<string, bool> scope = _scopes.Peek();

        scope.Add(name.lexeme, false);
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes.ElementAt(i).ContainsKey(name.lexeme))
            {
                _interpreter.Resolve(expr, _scopes.Count - 1 - i);
                return;
            }
        }
    }

    private void ResolveFunction(Stmt.Function function, FunctionType functionType)
    {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = functionType;

        BeginScope();
        foreach (var parameter in function._parameters)
        {
            Declare(parameter);
            Define(parameter);
        }

        Resolve(function._body);
        EndScope();

        currentFunction = enclosingFunction;
    }
}
