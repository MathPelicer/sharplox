using sharplox.Expressions;

namespace sharplox;

public class LoxFunction(Stmt.Function declaration, Environment clojure) : ISharploxCallable
{
    private readonly Stmt.Function _declaration = declaration;

    private readonly Environment _clojure = clojure;

    public int Arity()
    {
        return _declaration._parameters.Count;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        Environment env = new(_clojure);
        for(var i = 0; i < _declaration._parameters.Count; i++)
        {
            env.Define(_declaration._parameters[i].lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration._body, env);    
        }
        catch (Return returnValue)
        {
            return returnValue.value;
        }
        
        return null;
    }

    public override string ToString()
    {
        return $"<fn {_declaration._name.lexeme}>";
    }
}
