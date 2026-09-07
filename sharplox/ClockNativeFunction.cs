using System;

namespace sharplox;

public class ClockNativeFunction : ISharploxCallable
{
    public int Arity()
    {
        return 0;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        return DateTime.Now.Millisecond / 1000.0;
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}
