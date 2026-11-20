namespace sharplox;

public class LoxClass(string name) : ISharploxCallable
{
    public readonly string name = name;

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
}
