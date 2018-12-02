using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace DatingApp.API.Controllers
{
    public class FallBack: Controller
    {
        public IActionResult Index() 
        {
            //send request to angualar application (wwwwroot/index.html) - this is where it was deployed
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
            "wwwroot", "index.html"), "text/HTML");
        
        }
    }
}