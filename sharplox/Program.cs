if(args.Length > 1)
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
    throw new NotImplementedException();
}

void RunPrompt()
{
    throw new NotImplementedException();
}