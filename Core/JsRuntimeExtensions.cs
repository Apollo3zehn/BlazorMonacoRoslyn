using Microsoft.JSInterop;
using OneDas.DataManagement.Explorer.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using static OneDas.DataManagement.Explorer.Services.MonacoService;

namespace OneDas.DataManagement.Explorer.Core
{
    internal static class JsRuntimeExtensions
    {
        #region Methods

        public static async Task CreateMonacoEditorAsync(this IJSRuntime jsRuntime, string editorId, Dictionary<string, object> options)
        {
            await jsRuntime.InvokeVoidAsync("CreateMonacoEditor", editorId, options);
        }

        public static async Task RegisterMonacoProvidersAsync(this IJSRuntime jsRuntime, string editorId, DotNetObjectReference<MonacoService> dotnetHelper)
        {
            await jsRuntime.InvokeVoidAsync("RegisterMonacoProviders", editorId, dotnetHelper);
        }

        public static async Task SetMonacoValueAsync(this IJSRuntime jsRuntime, string editorId, string value)
        {
            await jsRuntime.InvokeVoidAsync("SetMonacoValue", editorId, value);
        }

        public static ValueTask<string> GetMonacoValueAsync(this IJSRuntime jsRuntime, string editorId)
        {
            return jsRuntime.InvokeAsync<string>("GetMonacoValue", editorId);
        }

        public static async Task SetMonacoDiagnosticsAsync(this IJSRuntime jsRuntime, string editorId, List<Diagnostic> diagnostics)
        {
            await jsRuntime.InvokeVoidAsync("SetMonacoDiagnostics", editorId, diagnostics);
        }

        #endregion
    }
}
