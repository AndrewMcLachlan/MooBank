#nullable enable
using Asm.MooBank.Web.Api.Tests.Infrastructure;

namespace Asm.MooBank.Web.Api.Tests.Authorization;

/// <summary>
/// Collection definition for authorization tests.
/// All tests in this collection share the same WebApplicationFactory instance
/// and run sequentially to avoid race conditions.
/// </summary>
[CollectionDefinition(Name)]
public class AuthorizationTestCollection : ICollectionFixture<MooBankWebApplicationFactory>
{
    public const string Name = "Authorization";
}
