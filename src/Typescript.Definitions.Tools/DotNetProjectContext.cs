using System.Reflection;
using System.Runtime.Loader;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Loader;
using NuGet.Frameworks;

namespace Typescript.Definitions.Tools
{
    public class DotNetProjectContext : IProjectContext
    {
        private readonly bool _isExecutable;
        private readonly ProjectContext _project;
        private readonly OutputPaths _paths;
        private readonly AssemblyLoadContext _assemblyLoadContext;

        public DotNetProjectContext(ProjectContext wrappedProject, string configuration, string outputPath)
        {
            _project = wrappedProject;
            _paths = wrappedProject.GetOutputPaths(configuration, null, outputPath);

            _isExecutable = wrappedProject.ProjectFile.GetCompilerOptions(wrappedProject.TargetFramework, configuration).EmitEntryPoint ?? wrappedProject.ProjectFile.GetCompilerOptions(null, configuration).EmitEntryPoint.GetValueOrDefault();

            _assemblyLoadContext = wrappedProject.CreateLoadContext(configuration);

            Configuration = configuration;
        }

        public bool IsClassLibrary => !_isExecutable;

        public NuGetFramework TargetFramework => _project.TargetFramework;

        public Assembly Assembly => _assemblyLoadContext.LoadFromAssemblyPath(AssemblyFullPath);

        public string Config => _paths.RuntimeFiles.Config;

        public string DepsJson => _paths.RuntimeFiles.DepsJson;

        public string RuntimeConfigJson => _paths.RuntimeFiles.RuntimeConfigJson;

        public string PackagesDirectory => _project.PackagesDirectory;
        
        public string AssemblyFullPath =>
            _isExecutable && (_project.IsPortable || TargetFramework.IsDesktop())
                ? _paths.RuntimeFiles.Executable
                : _paths.RuntimeFiles.Assembly;

        public Project Project => _project.ProjectFile;

        public string ProjectFullPath => _project.ProjectFile.ProjectFilePath;

        public string ProjectName => _project.ProjectFile.Name;

        public string RootNamespace => _project.ProjectFile.Name;

        public string TargetDirectory => _paths.RuntimeOutputPath;

        public string Configuration { get; }
    }
}