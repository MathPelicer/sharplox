using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

if (args.Length > 1)
{
    Console.WriteLine("Usage: SharpLox [script]");
    Environment.Exit(64);
}
else if(args.Length == 1)
{
    RunFiles(args[0]);
}
else
{
    RunPrompt();
}

void RunFiles(string path)
{
    try
    {
        byte[] bytes = File.ReadAllBytes(path);  
        Run(Encoding.Default.GetString(bytes));
    }
    catch(Exception)
    {
        throw;
    }
}

void RunPrompt()
{
    TextReader textInput = Console.In;
    
    for(;;)
    {
        Console.WriteLine("> ");
        var line = textInput.ReadLine();
        if(line == null)
        {
            break;
        }
        Run(line);
    }
}

void Run(string source)
{
    // Scanner scanner = new(source);
    // List<Token> tokens = scanner.ScanTokens();

    // foreach(var token in tokens)
    // {
    //     Console.WriteLine(token);
    // }
}