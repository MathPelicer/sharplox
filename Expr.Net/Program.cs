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

        DefineAst(outputDir, "Stmt", [
            "Expression : Expr expression",
            "Print: Expr expression"
        ]);
    }

    private static void DefineAst(string outputDir, string baseName, List<string> types)
    {
        string path = $"{outputDir}/{baseName}.cs";

        var writer = new StreamWriter(path);

        writer.WriteLine("namespace sharplox.Expressions;");
        writer.WriteLine();
        writer.WriteLine($"public abstract class {baseName}" + "{");

        writer.WriteLine();
        writer.WriteLine("    public abstract T Accept<T>(IVisitor<T> visitor);");
        writer.WriteLine();

        DefineVisitor(writer, baseName, types);

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
            writer.WriteLine($"        public readonly {type} _{name} = {name};");
        }

        writer.WriteLine("");
        writer.WriteLine("        public override T Accept<T>(IVisitor<T> visitor){");
        writer.WriteLine($"            return visitor.Visit{className}{baseName}(this);");
        writer.WriteLine("        }");
        

        writer.WriteLine("    }");
    }

    public static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
    {
        writer.WriteLine("    public interface IVisitor<T>{");

        foreach(var type in types)
        {
            string typeName = type.Split(":")[0].Trim();
            writer.WriteLine($"        T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");    
        }

        writer.WriteLine("    }");
        writer.WriteLine();
    }
}