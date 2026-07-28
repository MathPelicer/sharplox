using System.Reflection.Metadata;
using sharplox.Expressions;

namespace sharplox;

public class Interpreter : Expr.IVisitor<Object>
{
    public void Interpret(Expr expr)
    {
        try
        {
            object value = Evaluate(expr);
            Console.WriteLine(Stringify(value));
        }
        catch (RuntimeError err)
        {
            Program.RuntimeError(err);
            throw;
        }
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
}