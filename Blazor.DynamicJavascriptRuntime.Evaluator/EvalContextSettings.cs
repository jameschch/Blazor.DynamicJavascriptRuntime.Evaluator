using System;
using System.Collections.Generic;
using System.Text;

namespace Blazor.DynamicJavascriptRuntime.Evaluator
{
    public class EvalContextSettings
    {

        /// <summary>
        /// If set to true will disable replacing instance of the space character placeholder with a space
        /// </summary>
        public bool DisableSpaceCharacterPlaceholderReplacement { get; set; }

        /// <summary>
        /// Allows the definition of a placeholder for the space character. Defaults to underscore.
        /// </summary>
        public string SpaceCharacterPlaceholder { get; set; } = "_";


        /// <summary>
        /// Forces executed Javascript to be logged to the debug output
        /// </summary>
        public bool EnableDebugLogging { get; set; }

    }
}
