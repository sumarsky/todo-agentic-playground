namespace BackendApi.Domain;

public readonly record struct TodoId(Guid Value) : IParsable<TodoId>
{
    public static TodoId New() => new(Guid.NewGuid());

    public static TodoId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s));

    public static bool TryParse(string? s, IFormatProvider? provider, out TodoId result)
    {
        if (Guid.TryParse(s, out var value))
        {
            result = new TodoId(value);
            return true;
        }

        result = default;
        return false;
    }

    public static implicit operator Guid(TodoId id) => id.Value;

    public override string ToString() => Value.ToString();
}
