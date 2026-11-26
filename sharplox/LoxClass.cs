namespace sharplox;

public class LoxClass(string name, Dictionary<string, LoxFunction> methods) : ISharploxCallable
{
    public readonly string name = name;

    private readonly Dictionary<string, LoxFunction> _methods = methods;

    // validate if the number of arguments passed is the right one.
    public int Arity()
    {
        return 0;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        LoxInstance instance = new(this);
        return instance;
    }

    public override string ToString()
    {
        return name;
    }

    internal LoxFunction FindMethod(string lexeme)
    {
        var isFound = _methods.TryGetValue(lexeme, out var method);
        
        if (isFound)
        {
            return method!;    
        }

        return null;
    }
}
