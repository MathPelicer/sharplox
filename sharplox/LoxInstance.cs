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

        throw new RuntimeError(name, $"Undefined property '{name.lexeme}'.");
    } 

    public override string ToString()
    {
        return $"{loxClass.name} instance";
    }
}