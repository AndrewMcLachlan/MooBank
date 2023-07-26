namespace Asm.MooBank.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MetaController : Controller
{
    [HttpGet]
    public ActionResult<MetaModel> Get()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

        if (fileVersionInfo.FileVersion == null) return NotFound();

        return new MetaModel(new Version(fileVersionInfo.FileVersion!));
    }
}
