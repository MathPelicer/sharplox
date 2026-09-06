namespace sharplox;

public interface ISharploxCallable
{
    object Call(Interpreter interpreter, List<object> arguments);
}
