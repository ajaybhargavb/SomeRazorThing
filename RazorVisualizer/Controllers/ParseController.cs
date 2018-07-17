using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RazorVisualizer
{
    public class ApiController : Controller
    {
        [HttpPost("/[controller]/[action]")]
        public IActionResult Parse([FromBody] Source source)
        {
            var document = RazorSourceDocument.Create(source.Content, fileName: null);
            var options = RazorParserOptions.Create(builder => {
                foreach (var directive in GetDirectives())
                {
                    builder.Directives.Add(directive);
                }
            });
            var parser = new RazorParser(options);
            var tree = parser.Parse(document);
            var result = TreeSerializer.Serialize(tree);
            return Content(result);
        }

        private static IEnumerable<DirectiveDescriptor> GetDirectives()
        {
            var directives = new DirectiveDescriptor[]
            {
                DirectiveDescriptor.CreateDirective(
                    "inject",
                    DirectiveKind.SingleLine,
                    builder =>
                    {
                        builder
                            .AddTypeToken()
                            .AddMemberToken();

                        builder.Usage = DirectiveUsage.FileScopedMultipleOccurring;
                    }),
                DirectiveDescriptor.CreateDirective(
                    "model",
                    DirectiveKind.SingleLine,
                    builder =>
                    {
                        builder.AddTypeToken();
                        builder.Usage = DirectiveUsage.FileScopedSinglyOccurring;
                    }),
                DirectiveDescriptor.CreateDirective(
                    "namespace",
                    DirectiveKind.SingleLine,
                    builder =>
                    {
                        builder.AddNamespaceToken();
                        builder.Usage = DirectiveUsage.FileScopedSinglyOccurring;
                    }),
                DirectiveDescriptor.CreateDirective(
                    "page",
                    DirectiveKind.SingleLine,
                    builder =>
                    {
                        builder.AddOptionalStringToken();
                        builder.Usage = DirectiveUsage.FileScopedSinglyOccurring;
                    }),
                DirectiveDescriptor.CreateDirective(
                    SyntaxConstants.CSharp.FunctionsKeyword,
                    DirectiveKind.CodeBlock,
                    builder =>
                    {
                    }),
                DirectiveDescriptor.CreateDirective(
                    SyntaxConstants.CSharp.InheritsKeyword,
                    DirectiveKind.SingleLine,
                    builder =>
                    {
                        builder.AddTypeToken();
                        builder.Usage = DirectiveUsage.FileScopedSinglyOccurring;
                    }),
                DirectiveDescriptor.CreateDirective(
                    SyntaxConstants.CSharp.SectionKeyword,
                    DirectiveKind.RazorBlock,
                    builder =>
                    {
                        builder.AddMemberToken();
                    }),
            };

            return directives;
        }

        public class Source
        {
            public string Content { get; set; }
        }
    }
}