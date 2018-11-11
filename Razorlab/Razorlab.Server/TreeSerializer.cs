using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.AspNetCore.Razor.Language.Syntax;
using Razorlab.Shared;

namespace Razorlab.Server
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

            builder.Append(" - ");
            builder.Append($"[{node.ToFullString()}]");

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
    }
}
