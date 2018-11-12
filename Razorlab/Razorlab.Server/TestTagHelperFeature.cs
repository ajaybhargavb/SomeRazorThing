using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;

namespace Razorlab.Server
{
    public static class TestTagHelpers
    {
        private static TagHelperDescriptor[] _tagHelpers = new[]
            {
                TagHelperDescriptorBuilder.Create("TestTagHelper", "TestAssembly")
                    .TypeName("TestTagHelper")
                    .BoundAttributeDescriptor(attribute => attribute
                        .Name("Foo")
                        .TypeName("System.Int32")
                        .PropertyName("FooProp"))
                    .TagMatchingRuleDescriptor(rule => rule.RequireTagName("p"))
                    .Build()
            };

        public static IReadOnlyList<TagHelperDescriptor> GetDescriptors()
        {
            return _tagHelpers;
        }
    }
}