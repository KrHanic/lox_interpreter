namespace lox
{
    public class RuntimeError : Exception
    {
        public Token Token { get; set; }
        public RuntimeError(Token token, string msg) : base(msg)
        {
            Token = token;
        } 
    }
}
