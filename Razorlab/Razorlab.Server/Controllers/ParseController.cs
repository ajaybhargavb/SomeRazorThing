using Razorlab.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using System.Text;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Razorlab.Server.Controllers
{
    [Route("api/[controller]")]
    public class ParseController : Controller
    {
        [HttpPost("[action]")]
        public Node GetSyntaxTree([FromBody] Input input)
        {
            var document = CreateSourceDocument(input.Content);
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

            if (input.TagHelperPhase)
            {
                var engine = RazorProjectEngine.Create();
                if (input.DesignTime)
                {
                    var codeDocument = engine.ProcessDesignTime(document, null, TestTagHelpers.GetDescriptors());
                    root = codeDocument.GetSyntaxTree().Root;
                }
                else
                {
                    var codeDocument = engine.Process(document, null, TestTagHelpers.GetDescriptors());
                    root = codeDocument.GetSyntaxTree().Root;
                }
            }

            var node = TreeSerializer.Serialize(root);
            return node;
        }

        [HttpPost("[action]")]
        public Node GetLegacySyntaxTree([FromBody] Input input)
        {
            var document = CreateSourceDocument(input.Content);
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

        [HttpPost("[action]")]
        public Node GetIRTree([FromBody] Input input)
        {
            var document = CreateSourceDocument(input.Content);
            var options = RazorParserOptions.Create(builder => {
                foreach (var directive in GetDirectives())
                {
                    builder.Directives.Add(directive);
                }

                builder.SetDesignTime(input.DesignTime);
            });

            IntermediateNode intermediateNode;
            var engine = RazorProjectEngine.Create();
            if (input.DesignTime)
            {
                var codeDocument = engine.ProcessDesignTime(document, null, TestTagHelpers.GetDescriptors());
                intermediateNode = codeDocument.GetDocumentIntermediateNode();
            }
            else
            {
                var codeDocument = engine.Process(document, null, TestTagHelpers.GetDescriptors());
                intermediateNode = codeDocument.GetDocumentIntermediateNode();
            }

            var node = IRSerializer.Serialize(intermediateNode);
            return node;
        }

        [HttpPost("[action]")]
        public GeneratedCodeResult GetGeneratedCode([FromBody] Input input)
        {
            var document = CreateSourceDocument(input.Content);
            var options = RazorParserOptions.Create(builder => {
                foreach (var directive in GetDirectives())
                {
                    builder.Directives.Add(directive);
                }

                builder.SetDesignTime(input.DesignTime);
            });

            RazorCSharpDocument cSharpDocument;
            var engine = RazorProjectEngine.Create();
            if (input.DesignTime)
            {
                var codeDocument = engine.ProcessDesignTime(document, null, TestTagHelpers.GetDescriptors());
                cSharpDocument = codeDocument.GetCSharpDocument();
            }
            else
            {
                var codeDocument = engine.Process(document, null, TestTagHelpers.GetDescriptors());
                cSharpDocument = codeDocument.GetCSharpDocument();
            }

            var result = new GeneratedCodeResult {
                Code = cSharpDocument.GeneratedCode
            };
            return result;
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

        public static RazorSourceDocument CreateSourceDocument(
            string content = "Hello, world!",
            Encoding encoding = null,
            string filePath = "test.cshtml",
            string relativePath = "test.cshtml")
        {
            var properties = new RazorSourceDocumentProperties(filePath, relativePath);
            return new StringSourceDocument(content, encoding ?? Encoding.UTF8, properties);
        }
    }
}
