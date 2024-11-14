namespace lox {
    public class LoxFunction : ILoxCallable {
	private static Stmt.Function _declaration;
	private Environment _closure;
	private bool _isInitializer;

	public LoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer) {
	    _declaration = declaration;
	    _closure = closure;
	    _isInitializer = isInitializer;
	}

	public LoxFunction Bind(LoxInstance instance) {
	    Environment env = new(_closure);
	    env.Define("this", instance);
	    return new LoxFunction(_declaration, env, _isInitializer);
	}

	public object Call(Interpreter interpreter, List<object> arguments) {
	    Environment environment = new(_closure);
	    for (int i = 0; i < _declaration.parameters.Count; i++) {
		environment.Define(_declaration.parameters[i].Lexeme, arguments[i]);
	    }

	    try {
	        interpreter.ExecuteBlock(_declaration.body, environment);
	    } catch (Return returnValue) {
		if (_isInitializer) return _closure.GetAt(0, "this");
		return returnValue._value;
	    }

	    if (_isInitializer) return _closure.GetAt(0, "this");
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
