namespace lox {
    public class Return : Exception {
	public object _value;

	public Return(object value) {
	   _value = value; 
	}
    }
}
