namespace sharplox;

public interface ISharploxCallable
{
    int Arity();
    object Call(Interpreter interpreter, List<object> arguments);
}
