using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blazor.DynamicJavascriptRuntime.Evaluator.JavascriptLiteral
{
    [JsonConverter(typeof(LiteralConverter))]
    public class Literal
    {
        public string Value { get; set; }
    }
}
