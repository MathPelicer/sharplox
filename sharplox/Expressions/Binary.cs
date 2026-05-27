namespace sharplox.Expressions;

public class Binary(Expr left, Token binary, Expr right) : Expr
{
    private readonly Expr _left = left;

    private readonly Token _operator = binary;

    private readonly Expr _right = right;
}
