namespace lox
{
    public abstract class Stmt
    {
        public interface IVisitor<T> {
            public T VisitExpressionStmt(Expression Stmt);
            public T VisitPrintStmt(Print Stmt);
        }
        public class Expression : Stmt
        {
            public Expression(Expr expression)
            {
                this.expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitExpressionStmt(this);
            }
            public Expr expression;
        }

        public class Print : Stmt
        {
            public Print(Expr expression)
            {
                this.expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitPrintStmt(this);
            }
            public Expr expression;
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}
