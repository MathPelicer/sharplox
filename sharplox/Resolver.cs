using sharplox.Expressions;

namespace sharplox;

public class Resolver(Interpreter interpreter) : Expr.IVisitor<object>, Stmt.IVisitor<object>
{
    private readonly Interpreter _interpreter = interpreter;

    private readonly Stack<Dictionary<string, bool>> _scopes = new();

    public object VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr._value);
        ResolveLocal(expr, expr._name);
        return null;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
        throw new NotImplementedException();
    }

    public object VisitFunctionStmt(Stmt.Function stmt)
    {
        throw new NotImplementedException();
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        throw new NotImplementedException();
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
        throw new NotImplementedException();
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        throw new NotImplementedException();
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        throw new NotImplementedException();
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
        throw new NotImplementedException();
    }

    public object VisitReturnStmt(Stmt.Return stmt)
    {
        throw new NotImplementedException();
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        throw new NotImplementedException();
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

        Dictionary<string, bool> scope = [];

        scope.Add(name.lexeme, false);
    }

    public object VisitWhileStmt(Stmt.While stmt)
    {
        throw new NotImplementedException();
    }

    private void Resolve(List<Stmt> statements)
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
}
