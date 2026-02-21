using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blazor.DynamicJavascriptRuntime.Evaluator.JavascriptLiteral
{
    public class LiteralConverter : JsonConverter<Literal>
    {
        public override Literal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Literal value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(value.Value, true);
        }
    }
}
