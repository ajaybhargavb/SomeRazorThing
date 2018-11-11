using System.Threading.Tasks;
using Microsoft.JSInterop;
using Razorlab.Client.Pages;

namespace Razorlab.Client.JsInterop
{
    public class RazorlabJs
    {
        public static Task<string> GetElementValue(string element)
        {
            return JSRuntime.Current.InvokeAsync<string>(
                "razorlabJs.getElementValue",
                element);
        }

        public static Task InitializeCodeMirror(string element, Index index)
        {
            return JSRuntime.Current.InvokeAsync<string>(
                "razorlabJs.initializeCodeMirror",
                element,
                new DotNetObjectRef(index));
        }

        public static Task MarkText(string element, int start, int length)
        {
            return JSRuntime.Current.InvokeAsync<string>(
                "razorlabJs.markText",
                element,
                start,
                length);
        }

        public static Task ResetMark(string element, int start, int length)
        {
            return JSRuntime.Current.InvokeAsync<string>(
                "razorlabJs.resetMark",
                element,
                start,
                length);
        }
    }
}