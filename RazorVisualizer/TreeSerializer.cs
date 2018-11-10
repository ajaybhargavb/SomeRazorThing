using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RazorVisualizer
{
    public class TreeSerializer
    {
        public static string Serialize(RazorSyntaxTree syntaxTree)
        {
            var rootNode = new Node();
            Visit(syntaxTree.LegacyRoot, rootNode);
            var result = JsonConvert.SerializeObject(rootNode);

            return result;
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

    internal class Node
    {
        public Node()
        {
        }

        public Node(string content, int start, int length)
        {
            Content = content;
            Start = start;
            Length = length;
        }

        public string Content { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public IList<Node> Children { get; set; } = new List<Node>();
    }
}
