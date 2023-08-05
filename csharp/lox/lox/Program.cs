namespace lox
{
    internal class Program
    {
        static bool _hadError = false;
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
                Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            var fullPath = Path.GetFullPath(path);
            string content = File.ReadAllText(fullPath);
            Run(content);

            if(_hadError) Environment.Exit(65);
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

            foreach (Token token in tokens)
            {
                Console.WriteLine($"{token}");
            }
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
    }
}