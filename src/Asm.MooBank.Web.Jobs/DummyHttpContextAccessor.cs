using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Web.Jobs
{
    internal class DummyHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get => null; set { } }
    }
}
