using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.AspNetCore.Mvc;

namespace RazorVisualizer
{
    public class ApiController : Controller
    {
        [HttpPost]
        [Route("/[controller]/[action]")]
        public IActionResult Parse(string source)
        {
            source = source ?? string.Empty;
            var document = RazorSourceDocument.Create(source, fileName: null);
            var parser = new RazorParser();
            var tree = parser.Parse(document);
            var result = Json(tree);
            return result;
        }
    }
}