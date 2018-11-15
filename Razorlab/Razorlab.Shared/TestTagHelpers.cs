using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;

namespace Razorlab.Shared
{
    public static class TestTagHelpers
    {
        private static TagHelperDescriptor[] _tagHelpers = new[]
            {
                TagHelperDescriptorBuilder.Create("PTagHelper", "TestAssembly")
                    .TypeName("PTagHelper")
                    .BoundAttributeDescriptor(attribute => attribute
                        .Name("Foo")
                        .TypeName("System.Int32")
                        .PropertyName("FooProp"))
                    .BoundAttributeDescriptor(attribute => attribute
                        .Name("Bar")
                        .TypeName(typeof(string).FullName)
                        .PropertyName("BarProp"))
                    .TagMatchingRuleDescriptor(rule => rule.RequireTagName("p"))
                    .Build()
            };

        public static IReadOnlyList<TagHelperDescriptor> GetDescriptors()
        {
            return _tagHelpers;
        }
    }
}