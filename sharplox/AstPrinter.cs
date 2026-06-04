using System.Text;
using sharplox.Expressions;

namespace sharplox;

public class AstPrinter : Expr.IVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);    
    }

    public string VisitBinaryExpr(Expr.Binary expr)
    {
        return Parenthesize(expr._op.lexeme, expr._left, expr._right);
    }

    public string VisitGroupingExpr(Expr.Grouping expr)
    {
        return Parenthesize("group", expr._expression);
    }

    public string VisitLiteralExpr(Expr.Literal expr)
    {
        if(expr._value == null)
        {
            return "nil";
        }

        return expr._value.ToString();
    }

    public string VisitUnaryExpr(Expr.Unary expr)
    {
        return Parenthesize(expr._op.lexeme, expr._right);
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append('(').Append(name);

        foreach (Expr expr in exprs)
        {
            stringBuilder.Append(' ');
            stringBuilder.Append(expr.Accept(this));
        }

        stringBuilder.Append(')');

        return stringBuilder.ToString();
    }
}
