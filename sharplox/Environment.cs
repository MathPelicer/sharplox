using System.ComponentModel;
using System.Net;

namespace sharplox;

public class Environment
{
    public readonly Environment? enclosing;
    private readonly Dictionary<string, object> _values = [];

    public Environment()
    {
        enclosing = null;
    }

    public Environment(Environment enclosing)
    {
        this.enclosing = enclosing;
    }

    public void Define(string name, object value)
    {
        _values[name] = value;
    }

    public object Get(Token name)
    {
        if (_values.ContainsKey(name.lexeme))
        {
            return _values[name.lexeme];    
        }

        if(enclosing != null)
        {
            return enclosing.Get(name);
        }
        
        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    internal void Assign(Token name, object value)
    {
        if (_values.ContainsKey(name.lexeme))
        {
            _values[name.lexeme] = value;
            return;
        }

        if(enclosing != null)
        {
            enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    internal void AssignAt(int distance, Token name, object value)
    {
        Ancestor(distance)._values.Add(name.lexeme, value);
    }

    internal object GetAt(int distance, string name)
    {
        return Ancestor(distance)._values[name];
    }

    // walks up the parents of the environments as much as needed (distance)
    private Environment Ancestor(int distance)
    {
        Environment environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.enclosing;
        }

        return environment;
    }
}
