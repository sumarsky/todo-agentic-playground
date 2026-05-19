using Xunit;

namespace BackendApi.Tests;

[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialCollection : ICollectionFixture<object>
{
}
