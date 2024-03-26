using Asm.MooBank.Security;

namespace Asm.MooBank.Modules.Tests.Extensions;
internal static class SecurityMockExtensions
{
    public static void Fail(this Mock<ISecurity> security, System.Linq.Expressions.Expression<Action<ISecurity>> expression)
    {
        security.Setup(expression).Throws(new NotAuthorisedException());
    }
}
