using System.Text;

public class Program
{
    static bool _hadError = false;

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: SharpLox [script]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            RunFiles(args[0]);
        }
        else
        {
            RunPrompt();
        }   
    }

    private static void RunFiles(string path)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Run(Encoding.Default.GetString(bytes));

            if(_hadError)
            {
                Environment.Exit(65);
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

            Run(line);
            
            _hadError = false;
        }
    }

    private static void Run(string source)
    {
        // Scanner scanner = new(source);
        // List<Token> tokens = scanner.ScanTokens();

        // foreach(var token in tokens)
        // {
        //     Console.WriteLine(token);
        // }
    }

    private static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error {where}: {message}");
        var hadError = true;
    }
}