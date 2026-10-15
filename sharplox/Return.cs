namespace sharplox;

public class Return(object value) : Exception
{
    public readonly object value = value;
}
