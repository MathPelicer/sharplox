using System;

namespace sharplox;

public class SharploxCallable : ISharploxCallable
{
    public int Arity()
    {
        throw new NotImplementedException();
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        throw new NotImplementedException();
    }
}
