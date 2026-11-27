using sharplox.Expressions;

namespace sharplox;

public class LoxFunction(Stmt.Function declaration, Environment clojure, bool isInitilizer) : ISharploxCallable
{
    private readonly Stmt.Function _declaration = declaration;

    private readonly Environment _clojure = clojure;

    private readonly bool IsInitializer = isInitilizer;

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

        if (IsInitializer)
        {
            return _clojure.GetAt(0, "this");
        }

        return null;
    }

    public override string ToString()
    {
        return $"<fn {_declaration._name.lexeme}>";
    }

    internal LoxFunction Bind(LoxInstance loxInstance)
    {
        Environment environment = new(_clojure);
        environment.Define("this", loxInstance);

        return new LoxFunction(_declaration, environment, IsInitializer);
    }
}
