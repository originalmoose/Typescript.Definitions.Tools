using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.DotNet.ProjectModel;
using NuGet.Frameworks;

namespace Typescript.Definitions.Tools
{
    public class ProjectContextFactory
    {
        public IProjectContext Create(
            string filePath,
            NuGetFramework framework,
            string configuration = null,
            string outputDir = null)
        {
            var project = SelectCompatibleFramework(framework,
                Microsoft.DotNet.ProjectModel.ProjectContext.CreateContextForEachFramework(filePath,
                    runtimeIdentifiers: RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers()));

            return new DotNetProjectContext(project,
                configuration ?? Microsoft.DotNet.Cli.Utils.Constants.DefaultConfiguration,
                outputDir);
        }

        private ProjectContext SelectCompatibleFramework(NuGetFramework target, IEnumerable<ProjectContext> contexts)
        {
            return NuGetFrameworkUtility.GetNearest(contexts, target ?? FrameworkConstants.CommonFrameworks.NetCoreApp10,
                       f => f.TargetFramework) ?? contexts.First();
        }
    }
}