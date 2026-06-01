namespace BackendApi.Domain;

public readonly record struct TodoTitle
{
    public string Value { get; }

    public TodoTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Title cannot be empty or null", nameof(value));

        Value = value;
    }

    public static implicit operator string(TodoTitle title) => title.Value;

    public override string ToString() => Value;
}
