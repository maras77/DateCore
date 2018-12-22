using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace DateCore.API.Controllers
{
    public class FallbackController : Controller
    {
        public IActionResult Index()
        {
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
        }
     
    }
}
