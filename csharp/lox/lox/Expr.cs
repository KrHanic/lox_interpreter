namespace lox
{
    public abstract class Expr
    {
        public interface IVisitor<T> {
            public T VisitAssignExpr(Assign Expr);
            public T VisitBinaryExpr(Binary Expr);
            public T VisitCallExpr(Call Expr);
            public T VisitGetExpr(Get Expr);
            public T VisitGroupingExpr(Grouping Expr);
            public T VisitLiteralExpr(Literal Expr);
            public T VisitLogicalExpr(Logical Expr);
            public T VisitSetExpr(Set Expr);
            public T VisitSuperExpr(Super Expr);
            public T VisitThisExpr(This Expr);
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

        public class Call : Expr
        {
            public Call(Expr callee, Token paren, List<Expr> arguments)
            {
                this.callee = callee;
                this.paren = paren;
                this.arguments = arguments;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitCallExpr(this);
            }
            public Expr callee;
            public Token paren;
            public List<Expr> arguments;
        }

        public class Get : Expr
        {
            public Get(Expr obj, Token name)
            {
                this.obj = obj;
                this.name = name;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitGetExpr(this);
            }
            public Expr obj;
            public Token name;
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

        public class Set : Expr
        {
            public Set(Expr obj, Token name, Expr value)
            {
                this.obj = obj;
                this.name = name;
                this.value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitSetExpr(this);
            }
            public Expr obj;
            public Token name;
            public Expr value;
        }

        public class Super : Expr
        {
            public Super(Token keyword, Token method)
            {
                this.keyword = keyword;
                this.method = method;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitSuperExpr(this);
            }
            public Token keyword;
            public Token method;
        }

        public class This : Expr
        {
            public This(Token keyword)
            {
                this.keyword = keyword;
            }

            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitThisExpr(this);
            }
            public Token keyword;
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
