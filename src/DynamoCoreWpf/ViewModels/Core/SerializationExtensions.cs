using Dynamo.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Dynamo.Wpf.ViewModels.Core.Converters;

namespace Dynamo.Wpf.ViewModels.Core
{
    /// <summary>
    /// SerializationExtensions contains methods for serializing a WorkspaceViewModel to json.
    /// </summary>
    public static class SerializationExtensions
    {
        static private JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Error = (sender, args) =>
            {
                args.ErrorContext.Handled = true;
                Console.WriteLine(args.ErrorContext.Error);
            },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>{
                    new WorkspaceViewWriteConverter(),
                    new AnnotationViewModelConverter()
                }
        };

        /// <summary>
        /// Serialize the WorkspaceViewModel to JSON.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>A JSON string representing the WorkspaceViewModel</returns>
        internal static string ToJson(this WorkspaceViewModel viewModel)
        {
        
            return JsonConvert.SerializeObject(viewModel, settings);
        }

        internal static string ToJson(this NodeViewModel viewModel)
        {

            return JsonConvert.SerializeObject(viewModel, settings);
        }

        internal static string ToJson(this AnnotationViewModel viewModel)
        {

            return JsonConvert.SerializeObject(viewModel, settings);
        }

        internal static string ToJson(this NoteViewModel viewModel)
        {

            return JsonConvert.SerializeObject(viewModel, settings);
        }

        internal static string ToJson(this ViewModelBase viewModel)
        {
            return JsonConvert.SerializeObject(viewModel, settings);
        }

    }
}
