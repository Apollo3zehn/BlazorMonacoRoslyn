using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace OneDas.DataManagement.Explorer.Core
{
    public class RoslynProject
    {
        #region Fields

        private string _name;

        #endregion

        #region Constructors

        public RoslynProject(string name, string defaultCode)
        {
            _name = name;

            var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);

            // workspace
            this.Workspace = new AdhocWorkspace(host);

            // project
            var filePath = typeof(object).Assembly.Location;
            var documentationProvider = XmlDocumentationProvider.CreateFromFile(@"./Resources/System.Runtime.xml");

            var projectInfo = ProjectInfo
                .Create(ProjectId.CreateNewId(), VersionStamp.Create(), "OneDas", "OneDas", LanguageNames.CSharp)
                .WithMetadataReferences(new[]
                {
                    MetadataReference.CreateFromFile(filePath, documentation: documentationProvider)
                })
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var project = this.Workspace.AddProject(projectInfo);

            // code
            this.UseOnlyOnceDocument = this.Workspace.AddDocument(project.Id, "Code.cs", SourceText.From(defaultCode));
            this.DocumentId = this.UseOnlyOnceDocument.Id;
        }

        #endregion

        #region Properties

        public AdhocWorkspace Workspace { get; init; }

        public Document UseOnlyOnceDocument { get; init; }

        public DocumentId DocumentId { get; init; }

        #endregion

        #region Methods

        public void UpdateCode(DocumentId documentId, string code)
        {
            if (code == null)
                return;

            Solution updatedSolution;

            do
            {
                updatedSolution = this.Workspace.CurrentSolution.WithDocumentText(documentId, SourceText.From(code));
            } while (!this.Workspace.TryApplyChanges(updatedSolution));
        }

        public void SetValues(string code, string sampleRate, List<string> requestedProjectIds)
        {
            // update code
            this.UpdateCode(this.DocumentId, code);
        }

        #endregion
    }
}