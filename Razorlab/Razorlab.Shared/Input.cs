using System.Text;

namespace Razorlab.Shared
{
    public class Input
    {
        public string Content { get; set; }
        public bool DesignTime { get; set; }
        public bool TagHelperPhase { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Content);
            builder.Append(" - ");
            builder.Append("DesignTime: " + DesignTime);
            builder.Append(" - ");
            builder.Append("TagHelperPhase: " + TagHelperPhase);

            return builder.ToString();
        }
    }
}