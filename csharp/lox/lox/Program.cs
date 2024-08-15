using System.Diagnostics;

namespace lox
{
    internal class Program
    {
        //static void Main(string[] args)
        //{
        //    Expr expression = new Expr.Binary(
        //        new Expr.Unary(
        //            new Token(TokenType.MINUS, "-", null, 1),
        //            new Expr.Literal(123)),
        //        new Token(TokenType.STAR, "*", null, 1),
        //        new Expr.Grouping(new Expr.Literal(45.67))
        //        );

        //    Console.WriteLine(new AstPrinter().Print(expression));
        //}

        private static Interpreter _interpreter = new();
        static bool _hadError        = false;
        static bool _hadRuntimeError = false;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
                System.Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunFile("test.lox");
                //RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            var fullPath = Path.GetFullPath(path);
            string content = File.ReadAllText(fullPath);
            Run(content);

            if(_hadError)         System.Environment.Exit(65);
            if (_hadRuntimeError) System.Environment.Exit(70);
        }

        private static void RunPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line == null) break;
                Run(line);
                _hadError = false;
            }
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new(tokens);
            List<Stmt> statements = parser.Parse();

            if (_hadError) return;

	    Resolver resolver = new(_interpreter);
	    resolver.Resolve(statements);

	    if (_hadError) return;

            _interpreter.Interpret(statements);
        }

        public static void Error(int line, string msg)
        {
            Report(line, "", msg);
        }

        private static void Report(int line, string where, string msg)
        { 
            Console.WriteLine($"[line {line}] Error{where}: {msg}");
            _hadError = true;
        }

        public static void Error(Token token, string message) {
            if (token.Type == TokenType.EOF) Report(token.Line, " at end", message);
            else Report(token.Line, " at '" + token.Lexeme + "'", message);
        }

        public static void RuntimeError(RuntimeError error) {
            Console.WriteLine(error.Message + $"\n[line {error.Token.Line}]");
            _hadRuntimeError = true;
        }
    }
}
