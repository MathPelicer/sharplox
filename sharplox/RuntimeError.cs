using System.Runtime.CompilerServices;

namespace sharplox;

public class RuntimeError(Token token, string message) : Exception(message)
{
    public readonly Token _token = token;
}
