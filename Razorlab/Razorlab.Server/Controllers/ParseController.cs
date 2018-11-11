using Razorlab.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Razorlab.Server.Controllers
{
    [Route("api/[controller]")]
    public class ParseController : Controller
    {
        [HttpPost("[action]")]
        public Node GetSyntaxTree([FromBody] Input input)
        {
            var content = input.Content;
            var document = RazorSourceDocument.Create(content, fileName: null);
            var options = RazorParserOptions.Create(builder => {
                foreach (var directive in GetDirectives())
                {
                    builder.Directives.Add(directive);
                }

                builder.SetDesignTime(input.DesignTime);
            });

            var context = new ParserContext(document, options);
            var codeParser = new CSharpCodeParser(GetDirectives(), context);
            var markupParser = new HtmlMarkupParser(context);

            codeParser.HtmlParser = markupParser;
            markupParser.CodeParser = codeParser;

            var root = markupParser.ParseDocument().CreateRed();
            var node = TreeSerializer.Serialize(root);

            return node;
        }

        [HttpPost("[action]")]
        public Node GetLegacySyntaxTree([FromBody] Input input)
        {
            var content = input.Content;
            var document = RazorSourceDocument.Create(content, fileName: null);
            var options = RazorParserOptions.Create(builder => {
                foreach (var directive in GetDirectives())
                {
                    builder.Directives.Add(directive);
                }

                builder.SetDesignTime(input.DesignTime);
            });

            var parser = new RazorParser(options);
            var tree = parser.Parse(document, legacy: true);
            var node = LegacyTreeSerializer.Serialize(tree);

            return node;
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
    }
}
