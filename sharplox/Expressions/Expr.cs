namespace sharplox.Expressions;

public abstract class Expr{

    public abstract T Accept<T>(IVisitor<T> visitor);

    public interface IVisitor<T>{
        T VisitAssignExpr(Assign expr);
        T VisitBinaryExpr(Binary expr);
        T VisitCallExpr(Call expr);
        T VisitGroupingExpr(Grouping expr);
        T VisitLiteralExpr(Literal expr);
        T VisitLogicalExpr(Logical expr);
        T VisitUnaryExpr(Unary expr);
        T VisitVariableExpr(Variable expr);
    }

    public class Assign(Token name, Expr value) : Expr
    {
        public readonly Token _name = name;
        public readonly Expr _value = value;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitAssignExpr(this);
        }
    }
    public class Binary(Expr left, Token op, Expr right) : Expr
    {
        public readonly Expr _left = left;
        public readonly Token _op = op;
        public readonly Expr _right = right;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitBinaryExpr(this);
        }
    }
    public class Call(Expr callee, Token paren, List<Expr> arguments) : Expr
    {
        public readonly Expr _callee = callee;
        public readonly Token _paren = paren;
        public readonly List<Expr> _arguments = arguments;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitCallExpr(this);
        }
    }
    public class Grouping(Expr expression) : Expr
    {
        public readonly Expr _expression = expression;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitGroupingExpr(this);
        }
    }
    public class Literal(Object value) : Expr
    {
        public readonly Object _value = value;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitLiteralExpr(this);
        }
    }
    public class Logical(Expr left, Token op, Expr right) : Expr
    {
        public readonly Expr _left = left;
        public readonly Token _op = op;
        public readonly Expr _right = right;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitLogicalExpr(this);
        }
    }
    public class Unary(Token op, Expr right) : Expr
    {
        public readonly Token _op = op;
        public readonly Expr _right = right;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitUnaryExpr(this);
        }
    }
    public class Variable(Token name) : Expr
    {
        public readonly Token _name = name;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitVariableExpr(this);
        }
    }
}
