namespace lox {
    public class LoxClock : ILoxCallable {
	public int Arity() {return 0;}

	public object Call(Interpreter interpreter, List<object> arguments) {
	    return (double)DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
	}

	public override string ToString() {return "<native fn>";}
    }
}
