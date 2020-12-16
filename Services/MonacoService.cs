using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using OmniSharp.Models.v1.Completion;
using OmniSharp.Options;
using OneDas.DataManagement.Explorer.Core;
using OneDas.DataManagement.Explorer.Omnisharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneDas.DataManagement.Explorer.Services
{
    public class MonacoService
    {
        #region "Events"

        public event EventHandler<List<Diagnostic>> DiagnosticsUpdated;

        #endregion

        #region Fields

        private RoslynProject _completionProject;
        private RoslynProject _diagnosticProject;
        private OmniSharpCompletionService _completionService;

        #endregion

        #region Records

        public record Diagnostic()
        {
            public LinePosition Start { get; init; }
            public LinePosition End { get; init; }
            public string Message { get; init; }
            public int Severity { get; init; }
        }

        #endregion

        #region Constructors

        public MonacoService()
        {
            this.DefaultCode =
$@"using System; 
                 
namespace {nameof(OneDas)}.{nameof(DataManagement)}.{nameof(Explorer)}
{{
    class FilterChannel
    {{
        public void Filter(DateTime begin, DateTime end, double[] result)
        {{
            
        }}
    }}
}}
";

            _completionProject = new RoslynProject("completion", this.DefaultCode);
            _diagnosticProject = new RoslynProject("diagnostics", this.DefaultCode);
            _diagnosticProject = _completionProject;

            var loggerFactory = LoggerFactory.Create(configure => { });
            var formattingOptions = new FormattingOptions();

            _completionService = new OmniSharpCompletionService(_completionProject.Workspace, formattingOptions, loggerFactory);
        }

        #endregion

        #region Properties

        public string DefaultCode { get; init; }

        #endregion

        #region Methods

        [JSInvokable]
        public async Task<CompletionResponse> GetCompletionAsync(string code, CompletionRequest completionRequest)
        {
            Console.WriteLine("Invoking GetCompletionsAsnyc() ...");

            _completionProject.UpdateCode(_completionProject.DocumentId, code);

            var document = _completionProject.Workspace.CurrentSolution.GetDocument(_completionProject.DocumentId);
            var completionResponse = await _completionService.Handle(completionRequest, document);

            Console.WriteLine("Invoking GetCompletionsAsnyc() ... Done.");
            return completionResponse;
        }

        [JSInvokable]
        public async Task UpdateDiagnosticsAsync(string code = null)
        {
            Console.WriteLine("Invoking UpdateDiagnosticsAsync() ...");

            _diagnosticProject.UpdateCode(_diagnosticProject.DocumentId, code);

            var compilation = await _diagnosticProject.Workspace.CurrentSolution.Projects.First().GetCompilationAsync();
            var dotnetDiagnostics = compilation.GetDiagnostics();

            var diagnostics = dotnetDiagnostics.Select(current =>
            {
                var lineSpan = current.Location.GetLineSpan();

                return new Diagnostic()
                {
                    Start = lineSpan.StartLinePosition,
                    End = lineSpan.EndLinePosition,
                    Message = current.GetMessage(),
                    Severity = this.GetSeverity(current.Severity)
                };
            }).ToList();

            // remove warnings
            diagnostics = diagnostics
                .Where(diagnostic => diagnostic.Severity > 1)
                .ToList();

            this.OnDiagnosticsUpdated(diagnostics);
            Console.WriteLine("Invoking UpdateDiagnosticsAsync() ... Done.");
        }

        private void OnDiagnosticsUpdated(List<Diagnostic> diagnostics)
        {
            this.DiagnosticsUpdated?.Invoke(this, diagnostics);
        }

        private int GetSeverity(DiagnosticSeverity severity)
        {
            return severity switch
            {
                DiagnosticSeverity.Hidden => 1,
                DiagnosticSeverity.Info => 2,
                DiagnosticSeverity.Warning => 4,
                DiagnosticSeverity.Error => 8,
                _ => throw new Exception("Unknown diagnostic severity.")
            };
        }

        #endregion
    }
}