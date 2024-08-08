namespace lox
{
    public class Environment
    {
        public Environment Enclosing;
        private Dictionary<string, object> _values = new();

        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment env)
        {
            Enclosing = env;
        }

        public void Define(string name, object value) {
            if (_values.ContainsKey(name)) { 
                _values[name] = value;
            }
            else
            {
                _values.Add(name, value);
            }
        }

        public object Get(Token name) {
            if (_values.ContainsKey(name.Lexeme)) { 
                return _values[name.Lexeme];
            }

            if(Enclosing != null) return Enclosing.Get(name);

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public void Assign(Token name, object value) {
            if (_values.ContainsKey(name.Lexeme)) { 
                _values[name.Lexeme] = value;
                return;
            }

            if (Enclosing != null) { 
                Enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}
