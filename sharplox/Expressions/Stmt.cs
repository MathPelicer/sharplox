namespace sharplox.Expressions;

public abstract class Stmt
{
    public abstract T Accept<T>(IVisitor<T> visitor);

    public interface IVisitor<T>{
        T VisitExpressionStmt(Expression stmt);
        T VisitPrintStmt(Print stmt);
    }

    public class Expression(Expr expression) : Stmt
    {
        public readonly Expr _expression = expression;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitExpressionStmt(this);
        }
    }

    public class Print(Expr expression) : Stmt
    {
        public readonly Expr _expression = expression;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitPrintStmt(this);
        }
    }
}
