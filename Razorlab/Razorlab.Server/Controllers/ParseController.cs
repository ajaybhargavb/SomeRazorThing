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
        [HttpPost]
        public ParseResult Index([FromBody] Input input)
        {
            var document = CreateSourceDocument(input.Content);
            var options = RazorParserOptions.Create(builder => {
                foreach (var directive in GetDirectives())
                {
                    builder.Directives.Add(directive);
                }

                builder.SetDesignTime(input.DesignTime);
            });

            // Legacy Syntax Tree
            var parser = new RazorParser(options);
            var tree = parser.Parse(document, legacy: true);
            var legacySyntaxTreeRoot = LegacyTreeSerializer.Serialize(tree);

            // Syntax tree
            var context = new ParserContext(document, options);
            var codeParser = new CSharpCodeParser(GetDirectives(), context);
            var markupParser = new HtmlMarkupParser(context);
            codeParser.HtmlParser = markupParser;
            markupParser.CodeParser = codeParser;
            var root = markupParser.ParseDocument().CreateRed();

            // IR tree
            RazorCodeDocument codeDocument;
            var engine = RazorProjectEngine.Create();
            if (input.DesignTime)
            {
                codeDocument = engine.ProcessDesignTime(document, null, TestTagHelpers.GetDescriptors());
            }
            else
            {
                codeDocument = engine.Process(document, null, TestTagHelpers.GetDescriptors());
            }
            if (input.TagHelperPhase)
            {
                root = codeDocument.GetSyntaxTree().Root;
            }

            var syntaxTreeRoot = TreeSerializer.Serialize(root);

            var intermediateNode = codeDocument.GetDocumentIntermediateNode();
            var intermediateRoot = IRSerializer.Serialize(intermediateNode);
            
            // Generated code
            var cSharpDocument = codeDocument.GetCSharpDocument();
            var generatedCode = new GeneratedCodeResult {
                Code = cSharpDocument.GeneratedCode
            };

            return new ParseResult
            {
                SyntaxTreeRoot = syntaxTreeRoot,
                LegacySyntaxTreeRoot = legacySyntaxTreeRoot,
                IntermediateRoot = intermediateRoot,
                GeneratedCode = generatedCode
            };
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
