using System.Reflection;
using NuGet.Frameworks;

namespace Typescript.Definitions.Tools
{
    public interface IProjectContext
    {
        NuGetFramework TargetFramework { get; }
        bool IsClassLibrary { get; }
        string Config { get; }
        string DepsJson { get; }
        string RuntimeConfigJson { get; }
        string PackagesDirectory { get; }
        string AssemblyFullPath { get; }
        string ProjectName { get; }
        string Configuration { get; }
        string ProjectFullPath { get; }
        string RootNamespace { get; }
        string TargetDirectory { get; }

        Assembly Assembly { get; }
    }
}