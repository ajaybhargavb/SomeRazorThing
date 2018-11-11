using System.Collections.Generic;
using System.Text;

namespace Razorlab.Shared
{
    public class Node
    {
        public string Id => "Node-" + GetHashCode();
        public string Content { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public IList<Node> Children { get; set; } = new List<Node>();

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Content);
            builder.Append(" - ");
            builder.Append($"{Start}::{Length}");
            builder.Append(" - ");
            builder.Append($"Children: {Children.Count}");

            return builder.ToString();
        }
    }
}