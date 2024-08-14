namespace lox {
    public class LoxFunction : ILoxCallable {
	private static Stmt.Function _declaration;
	private Environment _closure;

	public LoxFunction(Stmt.Function declaration, Environment closure) {
	    _declaration = declaration;
	    _closure = closure;
	}

	public object Call(Interpreter interpreter, List<object> arguments) {
	    Environment environment = new(_closure);
	    for (int i = 0; i < _declaration.parameters.Count; i++) {
		environment.Define(_declaration.parameters[i].Lexeme, arguments[i]);
	    }

	    try {
	        interpreter.ExecuteBlock(_declaration.body, environment);
	    } catch (Return returnValue) {
		return returnValue._value;
	    }
	    return null;
	}

	public int Arity() {
	    return _declaration.parameters.Count;
	}

	public override string ToString() {
	    return $"<fn {_declaration.name.Lexeme}>";
	}
    }
}
