namespace lox
{
    public class Interpreter : Expr.IVisitor<object>
    {
        public void Interpret(Expr expr) {
            try
            {
                object value = Evaluate(expr);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeError error)
            {
                Program.RuntimeError(error);
                throw;
            }
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
            object left = Evaluate(expr.Left);
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
    }
}
