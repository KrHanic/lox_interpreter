namespace lox
{
    public abstract class Expr
    {
        public interface IVisitor<T> {
            public T VisitAssignExpr(Assign Expr);
            public T VisitBinaryExpr(Binary Expr);
            public T VisitGroupingExpr(Grouping Expr);
            public T VisitLiteralExpr(Literal Expr);
            public T VisitLogicalExpr(Logical Expr);
            public T VisitUnaryExpr(Unary Expr);
            public T VisitVariableExpr(Variable Expr);
        }
        public class Assign : Expr
        {
            public Assign(Token name, Expr value)
            {
                this.name = name;
                this.value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitAssignExpr(this);
            }
            public Token name;
            public Expr value;
        }

        public class Binary : Expr
        {
            public Binary(Expr Left, Token Operator, Expr Right)
            {
                this.Left = Left;
                this.Operator = Operator;
                this.Right = Right;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitBinaryExpr(this);
            }
            public Expr Left;
            public Token Operator;
            public Expr Right;
        }

        public class Grouping : Expr
        {
            public Grouping(Expr Expression)
            {
                this.Expression = Expression;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitGroupingExpr(this);
            }
            public Expr Expression;
        }

        public class Literal : Expr
        {
            public Literal(object Value)
            {
                this.Value = Value;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitLiteralExpr(this);
            }
            public object Value;
        }

        public class Logical : Expr
        {
            public Logical(Expr left, Token Operator, Expr right)
            {
                this.left = left;
                this.Operator = Operator;
                this.right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitLogicalExpr(this);
            }
            public Expr left;
            public Token Operator;
            public Expr right;
        }

        public class Unary : Expr
        {
            public Unary(Token Operator, Expr Right)
            {
                this.Operator = Operator;
                this.Right = Right;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitUnaryExpr(this);
            }
            public Token Operator;
            public Expr Right;
        }

        public class Variable : Expr
        {
            public Variable(Token name)
            {
                this.name = name;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitVariableExpr(this);
            }
            public Token name;
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}
