using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace RazorVisualizer
{
    public class ApiController : Controller
    {
        [HttpPost("/[controller]/[action]")]
        public IActionResult Parse([FromBody] Source source)
        {
            var document = RazorSourceDocument.Create(source.Content, fileName: null);
            var parser = new RazorParser();
            var tree = parser.Parse(document);
            var result = TreeSerializer.Serialize(tree);
            return Content(result);
        }

        public class Source
        {
            public string Content { get; set; }
        }
    }
}