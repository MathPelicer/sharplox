namespace sharplox.Expressions;

public abstract class Stmt{

    public abstract T Accept<T>(IVisitor<T> visitor);

    public interface IVisitor<T>{
        T VisitExpressionStmt(Expression stmt);
        T VisitPrintStmt(Print stmt);
        T VisitVarStmt(Var stmt);
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
    public class Var(Token name, Expr initializer) : Stmt
    {
        public readonly Token _name = name;
        public readonly Expr _initializer = initializer;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitVarStmt(this);
        }
    }
}
