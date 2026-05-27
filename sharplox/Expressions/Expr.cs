namespace sharplox.Expressions;

public class Expr{
    public class Binary(Expr left, Token op, Expr right) : Expr
    {
        private readonly Expr _left = left;
        private readonly Token _op = op;
        private readonly Expr _right = right;
    }

    public class Grouping(Expr expression) : Expr
    {
        private readonly Expr _expression = expression;
    }

    public class Literal(Object value) : Expr
    {
        private readonly Object _value = value;
    }

    public class Unary(Token op, Expr right) : Expr
    {
        private readonly Token _op = op;
        private readonly Expr _right = right;
    }

}
