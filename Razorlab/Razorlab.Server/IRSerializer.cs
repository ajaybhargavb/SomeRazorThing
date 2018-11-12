using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Razorlab.Shared;

namespace Razorlab.Server
{
    public class IRSerializer
    {
        internal static Node Serialize(IntermediateNode node)
        {
            var rootNode = new Node();
            Visit(node, rootNode);

            return rootNode;
        }

        internal static void Visit(IntermediateNode root, Node node)
        {
            var formatter = new DebuggerDisplayFormatter();
            formatter.FormatNode(root);
            node.Content = formatter.ToString();
            node.Start = root.Source?.AbsoluteIndex ?? 0;
            node.Length = root.Source?.Length ?? 0;

            foreach (var child in root.Children)
            {
                var n = new Node();
                node.Children.Add(n);
                Visit(child, n);
            }
        }

        private static string Normalize(string content)
        {
            var result = content.Replace("\r\n", "\n");
            return result.Replace("\n", "LF");
        }
    }
}