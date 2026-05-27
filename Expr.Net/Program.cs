using System.Net.Mail;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        if(args.Length != 1)
        {
            Console.WriteLine("Usage GenerateAST <output directory>");
            Environment.Exit(64);
        }

        string outputDir = args[0];

        DefineAst(outputDir, "Expr", [
            "Binary : Expr left, Token op, Expr right",
            "Grouping: Expr expression",
            "Literal: Object value",
            "Unary: Token op, Expr right"
        ]);
    }

    private static void DefineAst(string outputDir, string baseName, List<string> types)
    {
        string path = $"{outputDir}/{baseName}.cs";

        var writer = new StreamWriter(path);

        writer.WriteLine("namespace sharplox.Expressions;");
        writer.WriteLine();
        writer.WriteLine($"public class {baseName}" + "{");

        foreach(var type in types)
        {
            string className = type.Split(":")[0].Trim();
            string fields = type.Split(":")[1].Trim();

            DefineType(writer, baseName, className, fields);
        }

        writer.WriteLine("}");
        writer.Flush();
    }

    public static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
    {
        writer.WriteLine($"    public class {className}({fieldList}) : {baseName}");
        writer.WriteLine("    {");

        string[] fields = fieldList.Split(", ");

        foreach(var field in fields)
        {
            string type = field.Split(" ")[0];
            string name = field.Split(" ")[1];
            writer.WriteLine($"        private readonly {type} _{name} = {name};");
        }

        writer.WriteLine("    }");
        writer.WriteLine("");
    }
}