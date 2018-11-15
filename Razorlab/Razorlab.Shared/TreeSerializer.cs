using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.AspNetCore.Razor.Language.Syntax;

namespace Razorlab.Shared
{
    public class TreeSerializer
    {
        internal static Node Serialize(SyntaxNode node)
        {
            var rootNode = new Node();
            Visit(node, rootNode);

            return rootNode;
        }

        internal static void Visit(SyntaxNode root, Node node)
        {
            if (!root.IsList)
            {
                var content = GetNodeContent(root);
                node.Content = Normalize(content);
                node.Start = root.Position;
                node.Length = root.FullWidth;
            }

            if (root.SlotCount > 0 &&
                !root.GetType().Name.EndsWith("LiteralSyntax"))
            {
                for (var i = 0; i < root.SlotCount; i++)
                {
                    var child = root.GetNodeSlot(i);
                    if (child == null)
                    {
                        continue;
                    }
                    if (!child.IsList)
                    {
                        var n = new Node();
                        node.Children.Add(n);
                        Visit(child, n);
                    }
                    else
                    {
                        Visit(child, node);
                    }
                }
            }
        }

        private static string GetNodeContent(SyntaxNode node)
        {
            if (node is SyntaxToken token)
            {
                return GetTokenContent(token);
            }

            var builder = new StringBuilder();
            builder.Append($"{typeof(SyntaxKind).Name}.{node.Kind}");
            builder.Append(" - ");
            builder.Append($"[{node.Position}..{node.EndPosition})::{node.FullWidth}");

            if (node is RazorDirectiveSyntax razorDirective)
            {
                WriteRazorDirective(razorDirective, builder);
            }
            else if (node is MarkupTagHelperElementSyntax tagHelperElement)
            {
                WriteTagHelperElement(tagHelperElement, builder);
            }
            else if (node is MarkupTagHelperAttributeSyntax tagHelperAttribute)
            {
                WriteTagHelperAttributeInfo(tagHelperAttribute.TagHelperAttributeInfo, builder);
            }
            else if (node is MarkupMinimizedTagHelperAttributeSyntax minimizedTagHelperAttribute)
            {
                WriteTagHelperAttributeInfo(minimizedTagHelperAttribute.TagHelperAttributeInfo, builder);
            }

            if (ShouldDisplayNodeContent(node))
            {
                builder.Append(" - ");
                builder.Append($"[{node.GetContent()}]");
            }

            var annotation = node.GetAnnotations().FirstOrDefault(a => a.Kind == SyntaxConstants.SpanContextKind);
            if (annotation != null && annotation.Data is SpanContext context)
            {
                builder.Append(" - ");
                builder.Append($"Gen<{context.ChunkGenerator}>");
                builder.Append(" - ");
                builder.Append(context.EditHandler);
            }

            return builder.ToString();
        }

        private static void WriteRazorDirective(RazorDirectiveSyntax node, StringBuilder builder)
        {
            if (node.DirectiveDescriptor == null)
            {
                return;
            }

            builder.Append("Directive:{");
            builder.Append(node.DirectiveDescriptor.Directive);
            builder.Append(";");
            builder.Append(node.DirectiveDescriptor.Kind);
            builder.Append(";");
            builder.Append(node.DirectiveDescriptor.Usage);
            builder.Append("}");

            var diagnostics = node.GetDiagnostics();
            if (diagnostics.Length > 0)
            {
                builder.Append(" [");
                var ids = string.Join(", ", diagnostics.Select(diagnostic => $"{diagnostic.Id}{diagnostic.Span}"));
                builder.Append(ids);
                builder.Append("]");
            }

            builder.Append(" - ");
        }

        private static void WriteTagHelperElement(MarkupTagHelperElementSyntax node, StringBuilder builder)
        {
            // Write tag name
            builder.Append(" - ");
            builder.Append($"{node.TagHelperInfo.TagName}[{node.TagHelperInfo.TagMode}]");

            // Write descriptors
            foreach (var descriptor in node.TagHelperInfo.BindingResult.Descriptors)
            {
                builder.Append(" - ");

                // Get the type name without the namespace.
                var typeName = descriptor.Name.Substring(descriptor.Name.LastIndexOf('.') + 1);
                builder.Append(typeName);
            }
        }

        private static void WriteTagHelperAttributeInfo(TagHelperAttributeInfo info, StringBuilder builder)
        {
            // Write attributes
            builder.Append(" - ");
            builder.Append(info.Name);
            builder.Append(" - ");
            builder.Append(info.AttributeStructure);
            builder.Append(" - ");
            builder.Append(info.Bound ? "Bound" : "Unbound");
        }

        private static string GetTokenContent(SyntaxToken token)
        {
            var content = token.IsMissing ? "<Missing>" : token.Content;
            var diagnostics = token.GetDiagnostics();
            var tokenString = $"{typeof(SyntaxKind).Name}.{token.Kind};[{content}];{string.Join(", ", diagnostics.Select(diagnostic => diagnostic.Id + diagnostic.Span))}";
            return tokenString;
        }

        private static string Normalize(string content)
        {
            var result = content.Replace("\r\n", "\n");
            return result.Replace("\n", "LF");
        }

        private static bool ShouldDisplayNodeContent(SyntaxNode node)
        {
            return node.Kind == SyntaxKind.MarkupTextLiteral ||
                node.Kind == SyntaxKind.MarkupEphemeralTextLiteral ||
                node.Kind == SyntaxKind.MarkupTagBlock ||
                node.Kind == SyntaxKind.MarkupAttributeBlock ||
                node.Kind == SyntaxKind.MarkupMinimizedAttributeBlock ||
                node.Kind == SyntaxKind.MarkupTagHelperAttribute ||
                node.Kind == SyntaxKind.MarkupMinimizedTagHelperAttribute ||
                node.Kind == SyntaxKind.MarkupLiteralAttributeValue ||
                node.Kind == SyntaxKind.MarkupDynamicAttributeValue ||
                node.Kind == SyntaxKind.CSharpStatementLiteral ||
                node.Kind == SyntaxKind.CSharpExpressionLiteral ||
                node.Kind == SyntaxKind.CSharpEphemeralTextLiteral ||
                node.Kind == SyntaxKind.UnclassifiedTextLiteral;
        }
    }
}
