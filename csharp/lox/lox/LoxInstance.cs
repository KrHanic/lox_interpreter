namespace lox {
    public class LoxInstance {
	private LoxClass _klass;
	private Dictionary<string, object> _fields = new();

	public LoxInstance(LoxClass klass) {
	    _klass = klass;
	}

	public object Get(Token name) {
	    if (_fields.ContainsKey(name.Lexeme)) {
                return _fields[name.Lexeme];
	    }

	    LoxFunction method = _klass.FindMethod(name.Lexeme);
	    if (method is not null) return method.Bind(this);

	    throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
	}

	public void Set(Token name, object value) {
	    if(_fields.ContainsKey(name.Lexeme)) {
		_fields[name.Lexeme] = value;
	    } else {
		_fields.Add(name.Lexeme, value);
	    }
	}

	public override string ToString() {
	    return _klass._name + " instance";
	}
    }
}
