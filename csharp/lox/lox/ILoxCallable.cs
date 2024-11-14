namespace lox {
    public interface ILoxCallable{
	int Arity();
	object Call(Interpreter interpreter, List<object> arguments);
    }
}
