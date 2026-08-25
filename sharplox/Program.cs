using System.Text;
using sharplox;
using sharplox.Expressions;

public class Program
{
    static bool _hadError = false;

    static bool _hadRuntimeError = false;

    private static readonly Interpreter interpreter = new();

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: SharpLox [script]");
            System.Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            if(args[0] == "123")
            {
                Test();
            }
            else
            {
                RunFiles(args[0]);    
            }
        }
        else
        {
            RunPrompt();
        }   
    }

    private static void Test()
    {
        Expr expr = new Expr.Binary(
            new Expr.Unary(
                new Token(TokenType.MINUS, "-", null, 1),
                new Expr.Literal(123)
            ),
            new Token(TokenType.STAR, "*", null, 1),
            new Expr.Grouping(new Expr.Literal(45.67))
        );

        System.Console.WriteLine(new AstPrinter().Print(expr));
    }

    private static void RunFiles(string path)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Run(Encoding.Default.GetString(bytes));

            if(_hadError)
            {
                System.Environment.Exit(65);
            }
            if (_hadRuntimeError)
            {
                System.Environment.Exit(70);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static void RunPrompt()
    {
        TextReader textInput = Console.In;

        for (; ; )
        {
            Console.WriteLine("> ");
            var line = textInput.ReadLine();
            
            if (line == null)
            {
                break;
            }

            Run(line, isRepl: true);
            
            _hadError = false;
        }
    }

    private static void Run(string source, bool isRepl = false)
    {
        Scanner scanner = new(source);
        List<Token> tokens = scanner.ScanTokens();

        // foreach(var token in tokens)
        // {
        //     Console.WriteLine(token);
        // }

        Parser parser = new(tokens)
        {
            isRepl = isRepl
        };

        List<Stmt> statements = parser.Parse();

        if (_hadError)
        {
            return;
        }

        if (isRepl)
        {
            interpreter.InterpretRepl(statements);
        }
        else
        {
            interpreter.Interpret(statements);
        }
        

        //Console.WriteLine(new AstPrinter().Print(expr));
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void Error(Token token, string message)
    {
        if(token.tokenType == TokenType.EOF)
        {
            Report(token.line, " at end", message);
        }
        else
        {
            Report(token.line, $" at '{token.lexeme}'", message);
        }
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error {where}: {message}");
        _hadError = true;
    }

    internal static void RuntimeError(RuntimeError err)
    {
        Console.WriteLine($"{err.Message} \n [Line {err._token.line}]");
        _hadRuntimeError = true;
    }
}