using Microsoft.VisualBasic;

namespace sharplox;

public class LoxInstance(LoxClass loxClass)
{
    private readonly LoxClass loxClass = loxClass;

    private readonly Dictionary<string, object> fields = [];

    internal object Get(Token name)
    {
        if (fields.ContainsKey(name.lexeme))
        {
            fields.TryGetValue(name.lexeme, out var fieldValue);
            return fieldValue!;
        }

        LoxFunction method = loxClass.FindMethod(name.lexeme);

        if(method is not null)
        {
            return method.Bind(this);
        }

        throw new RuntimeError(name, $"Undefined property '{name.lexeme}'.");
    }

    internal void Set(Token name, object value)
    {
        fields[name.lexeme] = value;
    }

    public override string ToString()
    {
        return $"{loxClass.name} instance";
    }
}