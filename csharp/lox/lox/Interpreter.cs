namespace lox
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        public static Environment _globals = new();
        private Environment _env = _globals;
        private Dictionary<Expr, int> _locals = new();

        public Interpreter()
        {
            _globals.Define("Clock", new LoxClock()); //C# cannot create anonymous classes that implment an interface, so we created a real class for 'clock'.
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt stmt in statements)
                {
                    Execute(stmt);
                }
            }
            catch (RuntimeError error)
            {
                Program.RuntimeError(error);
            }
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private string Stringify(object value)
        {
            if (value is null) return "nil";

            if (value.GetType() == typeof(double))
            {
                string text = value.ToString();
                if (text.EndsWith(".0"))
                {
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
                    if (left.GetType() == typeof(double) && right.GetType() == typeof(double))
                    {
                        return (double)left + (double)right;
                    }

                    if (left.GetType() == typeof(string) && right.GetType() == typeof(string))
                    {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.");
            }

            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new();
            foreach (Expr argument in expr.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (!(callee is ILoxCallable))
            {
                throw new RuntimeError(expr.paren, "Can only call functions and classes.");
            }

            ILoxCallable function = (ILoxCallable)callee;
            if (arguments.Count != function.Arity())
            {
                throw new RuntimeError(expr.paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            object obj = Evaluate(expr.obj);
            if (obj is LoxInstance)
            {
                return ((LoxInstance)obj).Get(expr.name);
            }

            throw new RuntimeError(expr.name, "Only instances have properties.");
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

        private static bool IsTruthy(object value)
        {
            if (value is null) return false;
            if (value.GetType() == typeof(bool)) return (bool)value;
            return true;
        }

        private static bool IsEqual(object a, object b)
        {
            if (a is null && b is null) return true;
            if (a is null) return false;

            return a.Equals(b);
        }

        private static void CheckNumberOperand(Token operator_, object operand)
        {
            if (operand.GetType() == typeof(double)) return;
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

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            LoxFunction function = new(stmt, _env, false);
            _env.Define(stmt.name.Lexeme, function);
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

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            _env.Define(stmt.name.Lexeme, value);

            // Stmt.IVisitor<void> is not possible so we do
            // Stmt.IVisitor<object> and return null.
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            int distance = -1;
            if (_locals.ContainsKey(expr)) distance = _locals[expr];
            if (distance > -1)
            {
                return _env.GetAt(distance, name.Lexeme);
            }
            else
            {
                return _globals.Get(name);
            }
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);

            int distance = -1;
            if (_locals.ContainsKey(expr)) distance = _locals[expr];
            if (distance > -1)
            {
                _env.AssignAt(distance, expr.name, value);
            }
            else
            {
                _globals.Assign(expr.name, value);
            }

            return value;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(_env));
            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            object superclass = null;
            if (stmt.superclass is not null)
            {
                superclass = Evaluate(stmt.superclass);
                if (!(superclass is LoxClass))
                {
                    throw new RuntimeError(stmt.superclass.name, "Superclass must be a class.");
                }
            }

            _env.Define(stmt.name.Lexeme, null);

            if (stmt.superclass is not null)
            {
                _env = new(_env);
                _env.Define("super", superclass);
            }

            Dictionary<string, LoxFunction> methods = new();
            foreach (Stmt.Function method in stmt.methods)
            {
                LoxFunction function = new(method, _env, method.name.Lexeme.Equals("init"));
                methods.Add(method.name.Lexeme, function); // IMPORTANT: Java version uses .put() here, which is like AddOrUpdate(). So this might be a bug.
            }

            LoxClass klass = new(stmt.name.Lexeme, (LoxClass)superclass, methods);

            if (superclass is not null)
            {
                _env = _env.Enclosing;
            }

            _env.Assign(stmt.name, klass);
            return null;
        }

        public void ExecuteBlock(List<Stmt> statements, Environment env)
        {
            Environment previous = _env;

            try
            {
                _env = env;
                foreach (Stmt stmt in statements)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                _env = previous;
            }
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch is not null)
            {
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

        public object VisitSetExpr(Expr.Set expr)
        {
            object obj = Evaluate(expr.obj);

            if (!(obj is LoxInstance))
            {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            object value = Evaluate(expr.value);
            ((LoxInstance)obj).Set(expr.name, value);
            return value;
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            int distance = _locals[expr];
            LoxClass superclass = (LoxClass)_env.GetAt(distance, "super");

            LoxInstance obj = (LoxInstance)_env.GetAt(distance - 1, "this");

            LoxFunction method = superclass.FindMethod(expr.method.Lexeme);

            if (method is null)
            {
                throw new RuntimeError(expr.method, $"Undefined property '{expr.method.Lexeme}'.");
            }

            return method.Bind(obj);
        }

        public object VisitThisExpr(Expr.This expr)
        {
            return LookUpVariable(expr.keyword, expr);
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }
            return null;
        }

        public void Resolve(Expr expr, int depth)
        {
            _locals.Add(expr, depth);
        }
    }
}
