namespace lox
{
    public abstract class Expr
    {
        public class Binary : Expr
        {
            public Binary(Expr Left, Token Operator, Expr Right)
            {
                this.Left = Left;
                this.Operator = Operator;
                this.Right = Right;
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

            public Expr Expression;
        }

        public class Literal : Expr
        {
            public Literal(object Value)
            {
                this.Value = Value;
            }

            public object Value;
        }

        public class Unary : Expr
        {
            public Unary(Token Operator, Expr Right)
            {
                this.Operator = Operator;
                this.Right = Right;
            }

            public Token Operator;
            public Expr Right;
        }

    }
}
