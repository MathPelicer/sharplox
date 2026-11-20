namespace sharplox;

public class LoxInstance(LoxClass loxClass)
{
    private readonly LoxClass loxClass = loxClass;

    public override string ToString()
    {
        return $"{loxClass.name} instance";
    }
}
