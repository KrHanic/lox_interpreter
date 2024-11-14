namespace lox {
    public class LoxClass : ILoxCallable {
	public string _name;
	public LoxClass _superclass;
	private Dictionary<string, LoxFunction> _methods;

	public LoxClass(string name, LoxClass superclass, Dictionary<string, LoxFunction> methods) {
	   _name = name; 
	   _superclass = superclass;
	   _methods = methods;
	}

	public LoxFunction FindMethod(string name) {
	    if (_methods.ContainsKey(name)) {
		return _methods[name];
	    }

	    if (_superclass is not null) {
		return _superclass.FindMethod(name);
	    }

	    return null;
	}

	public object Call(Interpreter interpreter, List<object> arguments) {
	    LoxInstance instance = new(this);
	    LoxFunction initializer = FindMethod("init");
	    if (initializer is not null) {
		initializer.Bind(instance).Call(interpreter, arguments);
	    }

	    return instance;
	}

	public int Arity() {
	    LoxFunction initializer = FindMethod("init");
	    if (initializer is null) return 0;
	    return initializer.Arity();
	}

	public override string ToString() {
	    return _name;
	}
    }
}
