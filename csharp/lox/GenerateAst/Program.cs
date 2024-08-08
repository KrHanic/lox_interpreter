namespace GenerateAst
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length != 1)
            //{
            //    Console.WriteLine("Usage: generate_ast <output directory>");
            //    Environment.Exit(64);
            //}
            string outputDir = "D:\\projects\\lox\\lox_interpreter\\csharp\\lox\\lox";//args[0];
            try
            {
                DefineAst(outputDir, "Expr", new List<string> {
                    "Assign   : Token name, Expr value",
                    "Binary   : Expr Left, Token Operator, Expr Right",
                    "Grouping : Expr Expression",
                    "Literal  : object Value",
                    "Unary    : Token Operator, Expr Right",
                    "Variable : Token name"
                });

                DefineAst(outputDir, "Stmt", new List<string> { 
                    "Block      : List<Stmt> statements",
                    "Expression : Expr expression",
                    "Print      : Expr expression",
                    "Var        : Token name, Expr initializer"
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            string path = Path.Combine(outputDir, baseName + ".cs");
            string fileContent = "";
            AddLine(ref fileContent, "namespace lox");
            AddLine(ref fileContent, "{");

            AddLine(ref fileContent, $"public abstract class {baseName}", 1);
            AddLine(ref fileContent, "{", 1);

            DefineVisitor(ref fileContent, baseName, types);

            foreach (string type in types)
            {
                string className = type.Split(':')[0].Trim();
                string fields = type.Split(":")[1].Trim();
                DefineType(ref fileContent, baseName, className, fields);
            }
            AddLine(ref fileContent, "public abstract T Accept<T>(IVisitor<T> visitor);", 2);
            AddLine(ref fileContent, "}", 1);
            AddLine(ref fileContent, "}");

            File.WriteAllText(path, fileContent);
        }

        private static void DefineVisitor(ref string fileContent, string baseName, List<string> types)
        {
            AddLine(ref fileContent, "public interface IVisitor<T> {", 2);
            foreach (string type in types)
            {
                string typeName = type.Split(":")[0].Trim();
                AddLine(ref fileContent, $"public T Visit{typeName}{baseName}({typeName} {baseName});", 3);
            }
            AddLine(ref fileContent, "}", 2);
        }

        private static void AddLine(ref string fileContent, string text, int numOfTabs = 0)
        {
            string tabSpace = String.Concat(Enumerable.Repeat(" ", numOfTabs*4));
            fileContent += $"{tabSpace}{text}\n";
        }

        private static void DefineType(ref string fileContent, string baseName, string className, string fieldList)
        {
            AddLine(ref fileContent, $"public class {className} : {baseName}", 2);
            AddLine(ref fileContent, "{", 2);

            AddLine(ref fileContent, $"public {className}({fieldList})", 3);
            AddLine(ref fileContent, "{", 3);
            string[] fields = fieldList.Split(", ");
            foreach (string field in fields) 
            { 
                string name = field.Split(' ')[1];
                AddLine(ref fileContent, $"this.{name} = {name};", 4);
            }

            AddLine(ref fileContent, "}", 3);
            AddLine(ref fileContent, "");

            AddLine(ref fileContent, "public override T Accept<T>(IVisitor<T> visitor) {", 3);
            AddLine(ref fileContent, $"return visitor.Visit{className}{baseName}(this);", 4);
            AddLine(ref fileContent, "}", 3);

            foreach (string field in fields)
            {
                AddLine(ref fileContent, $"public {field};", 3);
            }
            AddLine(ref fileContent, "}", 2);
            AddLine(ref fileContent, "");
        }
    }
}