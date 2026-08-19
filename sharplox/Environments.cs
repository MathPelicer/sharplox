namespace sharplox;

public class Environments
{
    private readonly Dictionary<string, object> _values = [];

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
        
        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    } 
}
