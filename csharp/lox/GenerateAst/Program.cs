﻿namespace GenerateAst
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: generate_ast <output directory>");
                Environment.Exit(64);
            }
            string outputDir = args[0];
            try
            {
                DefineAst(outputDir, "Expr", new List<string> {
                    "Binary   : Expr Left, Token Operator, Expr Right",
                    "Grouping : Expr Expression",
                    "Literal  : object Value",
                    "Unary    : Token Operator, Expr Right"
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

            foreach (string type in types)
            {
                string className = type.Split(':')[0].Trim();
                string fields = type.Split(":")[1].Trim();
                DefineType(ref fileContent, baseName, className, fields);
            }

            AddLine(ref fileContent, "}", 1);
            AddLine(ref fileContent, "}");

            File.WriteAllText(path, fileContent);
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
            foreach (string field in fields)
            {
                AddLine(ref fileContent, $"public {field};", 3);
            }
            AddLine(ref fileContent, "}", 2);
            AddLine(ref fileContent, "");
        }
    }
}