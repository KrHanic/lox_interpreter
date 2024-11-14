namespace lox {
    public class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object> {
	private Interpreter _interpreter;
	private Stack<Dictionary<string, bool>> _scopes = new();
	private FunctionType _currentFunction = FunctionType.NONE;
	private ClassType _currentClass = ClassType.NONE;

	public Resolver(Interpreter interpreter){
	    _interpreter = interpreter;
	}

	private enum FunctionType {
	    NONE,
	    FUNCTION,
	    METHOD,
	    INITIALIZER
	}

	private enum ClassType {
	    NONE,
	    CLASS
	}

	public object VisitBlockStmt(Stmt.Block stmt) {
	    BeginScope();
	    Resolve(stmt.statements);
	    EndScope();
	    return null;
	}

	public object VisitClassStmt(Stmt.Class stmt) {
            ClassType enclosingClass = _currentClass;
	    _currentClass = ClassType.CLASS;

	    Declare(stmt.name);
	    Define(stmt.name);

	    if (stmt.superclass is not null && stmt.name.Lexeme.Equals(stmt.superclass.name.Lexeme)) {
		Program.Error(stmt.superclass.name, "a class can't inherit from itself.");
	    }

	    if (stmt.superclass is not null) {
		Resolve(stmt.superclass);
	    }

	    if (stmt.superclass is not null) {
		BeginScope();
		_scopes.Peek().Add("super", true); // IMPORTANT: Java version uses .put(), not .Add().
	    }

	    BeginScope();
	    _scopes.Peek().Add("this", true); // IMPORTANT: This might be a bug, Java version calls .put(), not .Add().

	    foreach (Stmt.Function method in stmt.methods) {
		FunctionType declaration = FunctionType.METHOD;
		if (method.name.Lexeme.Equals("init")) {
		    declaration = FunctionType.INITIALIZER;
		}

		ResolveFunction(method, declaration);
	    }

	    EndScope();

	    if (stmt.superclass is not null) EndScope();

	    _currentClass = enclosingClass;
	    return null;
	}

	public object VisitExpressionStmt(Stmt.Expression stmt) {
	    Resolve(stmt.expression);
	    return null;
	}

	public object VisitFunctionStmt(Stmt.Function stmt) {
	    Declare(stmt.name);
	    Define(stmt.name);

	    ResolveFunction(stmt, FunctionType.FUNCTION);
	    return null;
	}

	public object VisitIfStmt(Stmt.If stmt) {
	    Resolve(stmt.condition);
	    Resolve(stmt.thenBranch);
	    if ( stmt.elseBranch is not null ) Resolve(stmt.elseBranch);
	    return null;
	}
	
	public object VisitPrintStmt(Stmt.Print stmt) {
	    Resolve(stmt.expression);
	    return null;
	}

	public object VisitReturnStmt(Stmt.Return stmt) {
	    if (_currentFunction == FunctionType.NONE) {
		Program.Error(stmt.keyword, "Can't return from top-level code.");
	    }
	    if (stmt.value is not null) {
		if (_currentFunction == FunctionType.INITIALIZER) {
		    Program.Error(stmt.keyword, "Can't return a value from an initializer.");
		}
		Resolve(stmt.value);
	    }
	    return null;
	}

	public object VisitVarStmt(Stmt.Var stmt) {
            Declare(stmt.name);
	    if (stmt.initializer is not null) {
		Resolve(stmt.initializer);
	    }
	    Define(stmt.name);
	    return null;
	}

	public object VisitWhileStmt(Stmt.While stmt) {
	    Resolve(stmt.condition);
	    Resolve(stmt.body);
	    return null;
	}

	public object VisitAssignExpr(Expr.Assign expr) {
	    Resolve(expr.value);
	    ResolveLocal(expr, expr.name);
	    return null;
	}

	public object VisitBinaryExpr(Expr.Binary expr) {
	    Resolve(expr.Left);
	    Resolve(expr.Right);
	    return null;
	}

	public object VisitCallExpr(Expr.Call expr) {
	    Resolve(expr.callee);

	    foreach (var argument in expr.arguments) {
		Resolve(argument);
	    }

	    return null;
	}

	public object VisitGetExpr(Expr.Get expr) {
	    Resolve(expr.obj);
	    return null;
	}

	public object VisitGroupingExpr(Expr.Grouping expr) {
	    Resolve(expr.Expression);
	    return null;
	}

	public object VisitLiteralExpr(Expr.Literal expr) {
	    return null;
	}

	public object VisitLogicalExpr(Expr.Logical expr) {
	    Resolve(expr.left);
	    Resolve(expr.right);
	    return null;
	}

	public object VisitSetExpr(Expr.Set expr) {
	    Resolve(expr.value);
	    Resolve(expr.obj);
	    return null;
	}

	public object VisitSuperExpr(Expr.Super expr) {
	    ResolveLocal(expr, expr.keyword);
	    return null;
	}

	public object VisitThisExpr(Expr.This expr) {
	    if (_currentClass == ClassType.NONE) {
		Program.Error(expr.keyword, "Can't use 'this' outside of a class.");
		return null;
	    }

	    ResolveLocal(expr, expr.keyword);
	    return null;
	}

	public object VisitUnaryExpr(Expr.Unary expr) {
	    Resolve(expr.Right);
	    return null;
	}

	public object VisitVariableExpr(Expr.Variable expr) {
	    if (_scopes.Count > 0 && _scopes.Peek().ContainsKey(expr.name.Lexeme) && _scopes.Peek()[expr.name.Lexeme] == false) {
		Program.Error(expr.name, "Can't read local variable in its own initializer.");
	    }

	    ResolveLocal(expr, expr.name);
	    return null;
	}

	public void Resolve(List<Stmt> statements) {
	    foreach (var stmt in statements) {
		Resolve(stmt);
	    }
	}

	private void Resolve(Stmt stmt) {
	    stmt.Accept(this);
	}

	private void Resolve(Expr expr) {
	    expr.Accept(this);
	}

	private void ResolveFunction(Stmt.Function function, FunctionType type) {
	    FunctionType enclosingFunction = _currentFunction;
	    _currentFunction = type;

	    BeginScope();
	    foreach ( Token param in function.parameters ) {
		Declare(param);
		Define(param);
	    }
	    Resolve(function.body);
	    EndScope();
	    _currentFunction = enclosingFunction;
	}

	private void BeginScope() {
	    _scopes.Push(new Dictionary<string, bool>());
	}

	private void EndScope() {
	    _scopes.Pop();
	}

	private void Declare(Token name) {
	    if (_scopes.Count == 0) return;

	    Dictionary<string, bool> scope = _scopes.Peek();
	    if (scope.ContainsKey(name.Lexeme)) {
		Program.Error(name, "Already a variable with this name in this scipe.");
	    }

	    scope.Add(name.Lexeme, false);
	}

	private void Define(Token name) {
	    if (_scopes.Count == 0) return;
	    _scopes.Peek()[name.Lexeme] = true;
	}

	private void ResolveLocal(Expr expr, Token name) {
	    // This is a workaround for c# inablity to index into Scope<T> directly.
	    // Also... Stack<T>.ToArray() returns items in Pop() order so we have to revese it.
	    var scopes = _scopes.ToArray().Reverse().ToArray();
	    for ( int i = scopes.Length - 1; i >= 0; i--) {
		if ( scopes[i].ContainsKey(name.Lexeme) ) {
		    _interpreter.Resolve(expr, scopes.Length - 1 - i);
		    return;
		}
	    }
	}
    }
}
