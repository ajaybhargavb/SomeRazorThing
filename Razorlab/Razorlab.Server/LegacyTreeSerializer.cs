using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Razorlab.Shared;

namespace Razorlab.Server
{
    public class LegacyTreeSerializer
    {
        public static Node Serialize(RazorSyntaxTree syntaxTree)
        {
            var rootNode = new Node();
            Visit(syntaxTree.LegacyRoot, rootNode);

            return rootNode;
        }

        internal static void Visit(SyntaxTreeNode root, Node node)
        {
            node.Content = Normalize(root.ToString());
            node.Start = root.Start.AbsoluteIndex;
            node.Length = root.Length;

            if (root is Block block)
            {
                foreach (var child in block.Children)
                {
                    var n = new Node();
                    node.Children.Add(n);
                    Visit(child, n);
                }
            }
        }

        private static string Normalize(string content)
        {
            var result = content.Replace("\r\n", "\n");
            return result.Replace("\n", "LF");
        }
    }
}