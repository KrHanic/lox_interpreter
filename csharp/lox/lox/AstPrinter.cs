using System.Text;

namespace lox
{
    public class AstPrinter : Expr.IVisitor<string>
    {
        public string Print(Expr expr) { 
            return expr.Accept(this);
        }

        public string VisitAssignExpr(Expr.Assign Expr)
        {
            throw new NotImplementedException();
        }

        public string VisitBinaryExpr(Expr.Binary Expr)
        {
            return Parenthesize(Expr.Operator.Lexeme, Expr.Left, Expr.Right);
        }

        public string VisitGroupingExpr(Expr.Grouping Expr)
        {
            return Parenthesize("group", Expr.Expression);
        }

        public string VisitLiteralExpr(Expr.Literal Expr)
        {
            if (Expr.Value is null) return "nil";
            return Expr.Value.ToString()!;
        }

        public string VisitLogicalExpr(Expr.Logical Expr)
        {
            throw new NotImplementedException();
        }

        public string VisitUnaryExpr(Expr.Unary Expr)
        {
            return Parenthesize(Expr.Operator.Lexeme, Expr.Right);
        }

        public string VisitVariableExpr(Expr.Variable Expr)
        {
            throw new NotImplementedException();
        }

	public string VisitCallExpr(Expr.Call call){
	    throw new NotImplementedException();
	}

        private string Parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder sb = new();
            sb.Append('(').Append(name);
            foreach (Expr e in exprs)
            {
                sb.Append(' ');
                sb.Append(e.Accept(this));
            }
            sb.Append(')');

            return sb.ToString();
        }
    }
}
