using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BackendApi.Tests;

public class TodoApiIntegrationTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;

    public async Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private static StringContent ToJsonContent(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    [Fact]
    public async Task TodoLifecycle_CreateListUpdateToggleDelete_PersistsExpectedState()
    {
        var createResponse = await _client.PostAsync("/todos", ToJsonContent(new { Title = "Draft tests" }));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync()).RootElement;
        var id = created.GetProperty("id").GetString();

        var listResponse = await _client.GetAsync("/todos");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listed = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync()).RootElement;
        Assert.Contains(listed.EnumerateArray(), todo =>
            todo.GetProperty("id").GetString() == id &&
            todo.GetProperty("title").GetString() == "Draft tests" &&
            todo.GetProperty("completed").GetBoolean() == false);

        var updateResponse = await _client.PutAsync($"/todos/{id}/title", ToJsonContent(new { Title = "Ship integration tests" }));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = JsonDocument.Parse(await updateResponse.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal("Ship integration tests", updated.GetProperty("title").GetString());

        var toggleResponse = await _client.PutAsync($"/todos/{id}/toggle", null);
        Assert.Equal(HttpStatusCode.OK, toggleResponse.StatusCode);
        var toggled = JsonDocument.Parse(await toggleResponse.Content.ReadAsStringAsync()).RootElement;
        Assert.True(toggled.GetProperty("completed").GetBoolean());

        var deleteResponse = await _client.DeleteAsync($"/todos/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var finalListResponse = await _client.GetAsync("/todos");
        var finalList = JsonDocument.Parse(await finalListResponse.Content.ReadAsStringAsync()).RootElement;
        Assert.DoesNotContain(finalList.EnumerateArray(), todo => todo.GetProperty("id").GetString() == id);
    }

    [Fact]
    public async Task CreateTodo_WithValidTitle_Returns201Created()
    {
        // Arrange
        var request = new { Title = "Test todo" };
        var content = ToJsonContent(request);

        // Act
        var response = await _client.PostAsync("/todos", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        Assert.True(json.RootElement.TryGetProperty("id", out var id));
        Assert.True(json.RootElement.TryGetProperty("title", out var title));
        Assert.Equal("Test todo", title.GetString());
        Assert.True(json.RootElement.TryGetProperty("completed", out var completed));
        Assert.False(completed.GetBoolean());
    }

    [Fact]
    public async Task CreateTodo_WithEmptyTitle_Returns400BadRequest()
    {
        // Arrange
        var request = new { Title = "" };
        var content = ToJsonContent(request);

        // Act
        var response = await _client.PostAsync("/todos", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        Assert.True(json.RootElement.TryGetProperty("statusCode", out var statusCode));
        Assert.Equal(400, statusCode.GetInt32());
    }

    [Fact]
    public async Task ListTodos_NoFilter_ReturnsAll()
    {
        // Arrange: create some todos
        await _client.PostAsync("/todos", ToJsonContent(new { Title = "Todo 1" }));
        await _client.PostAsync("/todos", ToJsonContent(new { Title = "Todo 2" }));

        // Act
        var response = await _client.GetAsync("/todos");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        Assert.True(json.RootElement.ValueKind == JsonValueKind.Array);
        Assert.True(json.RootElement.GetArrayLength() >= 2);
    }

    [Fact]
    public async Task ListTodos_FilterByCompleted_ReturnsOnlyCompleted()
    {
        // Arrange: create two todos
        var createResp1 = await _client.PostAsync("/todos", ToJsonContent(new { Title = "Todo 1" }));
        var createResp2 = await _client.PostAsync("/todos", ToJsonContent(new { Title = "Todo 2" }));

        var body1 = await createResp1.Content.ReadAsStringAsync();
        var id1 = JsonDocument.Parse(body1).RootElement.GetProperty("id").GetString();

        var body2 = await createResp2.Content.ReadAsStringAsync();
        var id2 = JsonDocument.Parse(body2).RootElement.GetProperty("id").GetString();

        // Toggle first todo to completed
        await _client.PutAsync($"/todos/{id1}/toggle", null);

        // Act: filter by completed=true
        var response = await _client.GetAsync("/todos?completed=true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        var todos = json.RootElement;
        Assert.True(todos.GetArrayLength() >= 1);
        Assert.All(todos.EnumerateArray(), todo =>
        {
            Assert.True(todo.TryGetProperty("completed", out var completed));
            Assert.True(completed.GetBoolean());
        });
    }

    [Fact]
    public async Task ListTodos_SearchByTitle_ReturnsMatching()
    {
        // Arrange
        await _client.PostAsync("/todos", ToJsonContent(new { Title = "Buy milk" }));
        await _client.PostAsync("/todos", ToJsonContent(new { Title = "Buy groceries" }));
        await _client.PostAsync("/todos", ToJsonContent(new { Title = "Walk dog" }));

        // Act
        var response = await _client.GetAsync("/todos?search=Buy");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        var todos = json.RootElement;
        Assert.Equal(2, todos.GetArrayLength());
    }

    [Fact]
    public async Task UpdateTodoTitle_WithValidTitle_Returns200OK()
    {
        // Arrange
        var createResp = await _client.PostAsync("/todos", ToJsonContent(new { Title = "Old title" }));
        var body = await createResp.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(body).RootElement.GetProperty("id").GetString();

        var updateRequest = new { Title = "New title" };
        var content = ToJsonContent(updateRequest);

        // Act
        var response = await _client.PutAsync($"/todos/{id}/title", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        Assert.True(json.RootElement.TryGetProperty("title", out var title));
        Assert.Equal("New title", title.GetString());
    }

    [Fact]
    public async Task UpdateTodoTitle_TodoNotFound_Returns404NotFound()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        var content = ToJsonContent(new { Title = "New title" });

        // Act
        var response = await _client.PutAsync($"/todos/{fakeId}/title", content);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        Assert.True(json.RootElement.TryGetProperty("statusCode", out var statusCode));
        Assert.Equal(404, statusCode.GetInt32());
    }

    [Fact]
    public async Task ToggleTodoCompleted_Toggles_Returns200OK()
    {
        // Arrange
        var createResp = await _client.PostAsync("/todos", ToJsonContent(new { Title = "Test" }));
        var body = await createResp.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(body).RootElement.GetProperty("id").GetString();

        // Act: toggle to completed
        var response1 = await _client.PutAsync($"/todos/{id}/toggle", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var responseBody1 = await response1.Content.ReadAsStringAsync();
        var json1 = JsonDocument.Parse(responseBody1);
        Assert.True(json1.RootElement.TryGetProperty("completed", out var completed1));
        Assert.True(completed1.GetBoolean());

        // Act: toggle back to incomplete
        var response2 = await _client.PutAsync($"/todos/{id}/toggle", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var responseBody2 = await response2.Content.ReadAsStringAsync();
        var json2 = JsonDocument.Parse(responseBody2);
        Assert.True(json2.RootElement.TryGetProperty("completed", out var completed2));
        Assert.False(completed2.GetBoolean());
    }

    [Fact]
    public async Task DeleteTodo_ById_Returns204NoContent()
    {
        // Arrange
        var createResp = await _client.PostAsync("/todos", ToJsonContent(new { Title = "To delete" }));
        var body = await createResp.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(body).RootElement.GetProperty("id").GetString();

        // Act
        var response = await _client.DeleteAsync($"/todos/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's gone
        var listResp = await _client.GetAsync("/todos");
        var listBody = await listResp.Content.ReadAsStringAsync();
        var listJson = JsonDocument.Parse(listBody);
        var todos = listJson.RootElement;
        Assert.DoesNotContain(todos.EnumerateArray(), t =>
            t.TryGetProperty("id", out var todoId) && todoId.GetString() == id);
    }

    [Fact]
    public async Task BulkDeleteTodos_WithMultipleIds_Returns204NoContent()
    {
        // Arrange
        var resp1 = await _client.PostAsync("/todos", ToJsonContent(new { Title = "Todo 1" }));
        var resp2 = await _client.PostAsync("/todos", ToJsonContent(new { Title = "Todo 2" }));
        var resp3 = await _client.PostAsync("/todos", ToJsonContent(new { Title = "Todo 3" }));

        var id1 = JsonDocument.Parse(await resp1.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetString();
        var id2 = JsonDocument.Parse(await resp2.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetString();
        var id3 = JsonDocument.Parse(await resp3.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetString();

        // Act: delete first two
        var response = await _client.DeleteAsync($"/todos?ids={id1},{id2}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify only third remains
        var listResp = await _client.GetAsync("/todos");
        var listBody = await listResp.Content.ReadAsStringAsync();
        var listJson = JsonDocument.Parse(listBody);
        var todos = listJson.RootElement;
        Assert.Single(todos.EnumerateArray());
        Assert.Equal(id3, todos.EnumerateArray().First().GetProperty("id").GetString());
    }

    [Fact]
    public async Task BulkDeleteTodos_NoIds_Returns400BadRequest()
    {
        // Act
        var response = await _client.DeleteAsync("/todos?ids=");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
