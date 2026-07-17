using System.Reflection.Metadata;
using sharplox.Expressions;

namespace sharplox;

public class Interpreter : Expr.IVisitor<Object>
{
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
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                return (double)left >= (double)right;
            case TokenType.LESS:
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
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

                break;
            case TokenType.MINUS:
                return (double)left - (double)right;
            case TokenType.SLASH:
                return (double)left / (double)right;
            case TokenType.STAR:
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
                return -(double)right;
            case TokenType.BANG:
                return !IsTruthy(right);
        }

        return null;
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
}