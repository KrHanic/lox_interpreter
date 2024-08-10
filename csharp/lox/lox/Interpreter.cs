namespace lox
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private Environment _env = new();

        public void Interpret(List<Stmt> statements) {
            try
            {
                foreach (Stmt stmt in statements) {
                    Execute(stmt);
                }
            }
            catch (RuntimeError error)
            {
                Program.RuntimeError(error);
            }
        }

        private void Execute(Stmt stmt) { 
            stmt.Accept(this);
        }

        private string Stringify(object value) {
            if (value is null) return "nil";

            if (value.GetType() == typeof(double)) {
                string text = value.ToString();
                if (text.EndsWith(".0")) {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return value.ToString();
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left  = Evaluate(expr.Left);
            object right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                // equality
                case TokenType.BANG_EQUAL: return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL: return IsEqual(left, right);

                // comparison
                case TokenType.GREATER:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left <= (double)right;

                // Arithmetic
                case TokenType.MINUS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left.GetType() == typeof(double) && right.GetType() == typeof(double)) { 
                        return (double)left + (double)right;
                    }

                    if (left.GetType() == typeof(string) && right.GetType() == typeof(string)) { 
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.");
            }

            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Operator, right);
                    return -(double)right;
                default:
                    break;
            }

            return null;
        }

        private object Evaluate(Expr expr) { return expr.Accept(this); }

        private static bool IsTruthy(object value) { 
            if(value is null) return false;
            if (value.GetType() == typeof(bool)) return (bool)value;
            return true;
        }

        private static bool IsEqual(object a, object b) {
            if (a is null && b is null) return true;
            if (a is null) return false;

            return a.Equals(b);
        }

        private static void CheckNumberOperand(Token operator_, object operand) { 
            if(operand.GetType() == typeof(double)) return;
            throw new RuntimeError(operator_, "Operand must be a number.");
        }

        private static void CheckNumberOperands(Token operator_, object left, object right)
        {
            if (left.GetType() == typeof(double) && right.GetType() == typeof(double)) return;
            throw new RuntimeError(operator_, "Operands must be numbers.");
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);

            // Stmt.IVisitor<void> is not possible so we do
            // Stmt.IVisitor<object> and return null.
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));

            // Stmt.IVisitor<void> is not possible so we do
            // Stmt.IVisitor<object> and return null.
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null) { 
                value = Evaluate(stmt.initializer);
            }

            _env.Define(stmt.name.Lexeme, value);

            // Stmt.IVisitor<void> is not possible so we do
            // Stmt.IVisitor<object> and return null.
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return _env.Get(expr.name);
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            _env.Assign(expr.name, value);
            return value;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(_env));
            return null;
        }

        private void ExecuteBlock(List<Stmt> statements, Environment env) {
            Environment previous = _env;

            try
            {
                _env = env;
                foreach (Stmt stmt in statements)
                {
                    Execute(stmt);
                }
            }
            finally { 
                _env = previous;
            }
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition))) {
                Execute(stmt.thenBranch);
            } else if (stmt.elseBranch is not null) { 
                Execute(stmt.elseBranch);
            }

            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr.Operator.Type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else 
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition))) {
                Execute(stmt.body);
            }
            return null;
        }
    }
}
