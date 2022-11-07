using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Blazor.DynamicJavascriptRuntime.Evaluator
{
    public class EvalContextSettings
    {

        /// <summary>
        /// If set to true will disable replacing instance of the space character placeholder with a space
        /// </summary>
        public bool EnableSpaceCharacterPlaceholderReplacement { get; set; }

        /// <summary>
        /// Allows the definition of a placeholder for the space character. Defaults to underscore.
        /// </summary>
        public string SpaceCharacterPlaceholder { get; set; } = "_";


        /// <summary>
        /// Forces executed Javascript to be logged to the debug output
        /// </summary>
        public bool EnableDebugLogging { get; set; }

        /// <summary>
        /// Allows options to be specifed for Json Serialization of arguments
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, IgnoreNullValues = true };

        /// <summary>
        /// Specified types will be serialized into Javascript objects
        /// </summary>
        public IList<Type> SerializableTypes { get; } = new List<Type>();

    }
}
