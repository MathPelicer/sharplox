namespace sharplox;

public class LoxClass(string name)
{
    public readonly string name = name;

    public override string ToString()
    {
        return name;
    }
}
