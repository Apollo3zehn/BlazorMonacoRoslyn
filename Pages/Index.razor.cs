using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OneDas.DataManagement.Explorer.Core;
using OneDas.DataManagement.Explorer.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using static OneDas.DataManagement.Explorer.Services.MonacoService;

namespace Neuer_Ordner.Pages
{
    public partial class Index
    {
        #region Fields

        private string _editorId;
        private DotNetObjectReference<MonacoService> _objRef;

        #endregion

        #region Properties

        public List<Diagnostic> Diagnostics { get; set; }

        [Inject]
        private IJSRuntime JS { get; set; }

        [Inject]
        private MonacoService MonacoService { get; set; }

        #endregion

        #region Methods

        protected override void OnParametersSet()
        {
            this.MonacoService.DiagnosticsUpdated += this.OnDiagnosticsUpdated;
            base.OnParametersSet();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _objRef = DotNetObjectReference.Create(this.MonacoService);
                _editorId = "1";

                var options = new Dictionary<string, object>
                {
                    ["automaticLayout"] = true,
                    ["language"] = "csharp",
                    ["scrollBeyondLastLine"] = false,
                    ["value"] = this.MonacoService.DefaultCode,
                    ["theme"] = "vs-dark"
                };

                await this.JS.CreateMonacoEditorAsync(_editorId, options);
                await this.JS.RegisterMonacoProvidersAsync(_editorId, _objRef);
            }
        }

        #endregion

        #region EventHandlers

        private void OnDiagnosticsUpdated(object sender, List<Diagnostic> diagnostics)
        {
            this.Diagnostics = diagnostics;
            this.InvokeAsync(() => { this.StateHasChanged(); });

            _ = this.JS.SetMonacoDiagnosticsAsync(_editorId, diagnostics);
        }

        #endregion
    }
}
