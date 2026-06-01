using BackendApi.Domain;

namespace BackendApi.Tests.Domain;

public class TodoValueTypesTests
{
    [Fact]
    public void TodoId_New_ReturnsNonEmptyId()
    {
        var id = TodoId.New();

        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void TodoId_Parse_RoundTripsGuid()
    {
        var guid = Guid.NewGuid();

        var id = TodoId.Parse(guid.ToString(), null);

        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void TodoId_TryParse_WithValidGuid_ReturnsTrueAndId()
    {
        var guid = Guid.NewGuid();

        var parsed = TodoId.TryParse(guid.ToString(), null, out var id);

        Assert.True(parsed);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void TodoId_TryParse_WithInvalidGuid_ReturnsFalseAndDefault()
    {
        var parsed = TodoId.TryParse("not-a-guid", null, out var id);

        Assert.False(parsed);
        Assert.Equal(default, id);
    }

    [Fact]
    public void TodoId_Equality_UsesWrappedGuid()
    {
        var guid = Guid.NewGuid();
        var first = new TodoId(guid);
        var second = new TodoId(guid);

        Assert.Equal(first, second);
    }

    [Fact]
    public void TodoId_ImplicitGuidConversion_ReturnsWrappedGuid()
    {
        var guid = Guid.NewGuid();
        var id = new TodoId(guid);

        Guid converted = id;

        Assert.Equal(guid, converted);
    }

    [Fact]
    public void TodoId_ToString_ReturnsGuidString()
    {
        var guid = Guid.NewGuid();
        var id = new TodoId(guid);

        Assert.Equal(guid.ToString(), id.ToString());
    }

    [Fact]
    public void TodoTitle_WithValidTitle_StoresValue()
    {
        var title = new TodoTitle("Buy milk");

        Assert.Equal("Buy milk", title.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TodoTitle_WithNullEmptyOrWhitespace_Throws(string? value)
    {
        var exception = Assert.Throws<ArgumentException>(() => new TodoTitle(value!));

        Assert.Equal("value", exception.ParamName);
        Assert.Contains("Title cannot be empty or null", exception.Message);
    }

    [Fact]
    public void TodoTitle_ImplicitStringConversion_ReturnsWrappedValue()
    {
        var title = new TodoTitle("Buy milk");

        string converted = title;

        Assert.Equal("Buy milk", converted);
    }
}
