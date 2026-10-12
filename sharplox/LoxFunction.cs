using sharplox.Expressions;

namespace sharplox;

public class LoxFunction(Stmt.Function declaration) : ISharploxCallable
{
    private readonly Stmt.Function _declaration = declaration;

    public int Arity()
    {
        return _declaration._parameters.Count;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        Environment env = new(Interpreter.globals);
        for(var i = 0; i < _declaration._parameters.Count; i++)
        {
            env.Define(_declaration._parameters[i].lexeme, arguments[i]);
        }

        interpreter.ExecuteBlock(_declaration._body, env);
        return null;
    }

    public override string ToString()
    {
        return $"<fn {_declaration._name.lexeme}>";
    }
}
