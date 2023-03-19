using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.WebJobs
{
    internal class DummyHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get => null; set { } }
    }
}
