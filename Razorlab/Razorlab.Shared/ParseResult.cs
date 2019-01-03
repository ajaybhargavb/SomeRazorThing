namespace Razorlab.Shared
{
    public class ParseResult
    {
        public Node SyntaxTreeRoot { get; set; }

        public Node IntermediateRoot { get; set; }

        public GeneratedCodeResult GeneratedCode { get; set; }
    }
}