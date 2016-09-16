using System;
using System.Collections.Generic;
using Microsoft.DotNet.Cli.Utils;

namespace Typescript.Definitions.Tools
{
    public class DotNetProjectBuilder
    {
        public static void EnsureBuild(IProjectContext project)
        {
            var buildExitCode = CreateBuildCommand(project).ForwardStdErr().ForwardStdOut().Execute().ExitCode;

            if (buildExitCode != 0)
            {
                throw new Exception($"Error building project '{project.ProjectName}'");
            }
        }

        private static ICommand CreateBuildCommand(IProjectContext projectContext)
        {

            if (!(projectContext is DotNetProjectContext))
            {
                throw new PlatformNotSupportedException("Currently only .NET Core Projects (project.json/xproj) are supported");
            }

            var args = new List<string>
            {
                projectContext.ProjectFullPath,
                "--configuration", projectContext.Configuration,
                "--framework", projectContext.TargetFramework.GetShortFolderName()
            };

            if (projectContext.TargetDirectory != null)
            {
                args.Add("--output");
                args.Add(projectContext.TargetDirectory);
            }

            return Command.CreateDotNet(
                "build",
                args,
                projectContext.TargetFramework,
                projectContext.Configuration);
        }
    }
}