namespace sharplox.Expressions;

public abstract class Stmt{

    public abstract T Accept<T>(IVisitor<T> visitor);

    public interface IVisitor<T>{
        T VisitBlockStmt(Block stmt);
        T VisitExpressionStmt(Expression stmt);
        T VisitIfStmt(If stmt);
        T VisitPrintStmt(Print stmt);
        T VisitWhileStmt(While stmt);
        T VisitVarStmt(Var stmt);
    }

    public class Block(List<Stmt> statements) : Stmt
    {
        public readonly List<Stmt> _statements = statements;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitBlockStmt(this);
        }
    }
    public class Expression(Expr expression) : Stmt
    {
        public readonly Expr _expression = expression;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitExpressionStmt(this);
        }
    }
    public class If(Expr condition, Stmt thenBranch, Stmt elseBranch) : Stmt
    {
        public readonly Expr _condition = condition;
        public readonly Stmt _thenBranch = thenBranch;
        public readonly Stmt _elseBranch = elseBranch;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitIfStmt(this);
        }
    }
    public class Print(Expr expression) : Stmt
    {
        public readonly Expr _expression = expression;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitPrintStmt(this);
        }
    }
    public class While(Expr condition, Stmt body) : Stmt
    {
        public readonly Expr _condition = condition;
        public readonly Stmt _body = body;

        public override T Accept<T>(IVisitor<T> visitor){
            return visitor.VisitWhileStmt(this);
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
