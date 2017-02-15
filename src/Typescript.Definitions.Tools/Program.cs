using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Typescript.Definitions.Tools
{
    public class Program
    {
        private static Type _startupType;

        public static void Main(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            var dotnetParams = new DotnetBaseParams("dotnet-tsd", "Typescript Definition NET Core Tools", "Creates typescript definition files from .NET classes.");
            
            dotnetParams.Parse(args);
            
            if (dotnetParams.IsHelp)
            {
                return;
            }

            var startupProjectPath = dotnetParams.ProjectPath ?? Directory.GetCurrentDirectory();
            var startupProject = new ProjectContextFactory().Create(startupProjectPath,
                    dotnetParams.Framework,
                    dotnetParams.Config,
                    dotnetParams.BuildBasePath);

            try
            {
                if (!dotnetParams.NoBuild)
                {
                    DotNetProjectBuilder.EnsureBuild(startupProject);
                }
                
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(startupProject.Assembly.Location);
                
                _startupType = assembly.GetLoadableDefinedTypes().Where(t => typeof(ITypedef).IsAssignableFrom(t.AsType()))
                    .Select(t => t.AsType())
                    .FirstOrDefault();

                var services = new ServiceCollection();

                var definitionsBuilder = new TypescriptDefinitionBuilder();

                services.AddSingleton<IDefinitionBuilder, TypescriptDefinitionBuilder>((p) => definitionsBuilder);

                var typedef = ActivatorUtilities.GetServiceOrCreateInstance(services.BuildServiceProvider(), _startupType) as ITypedef;

                typedef?.Configure(definitionsBuilder);

                foreach (var builder in definitionsBuilder.Builders)
                {
                    var files = builder.Generate(startupProjectPath);
                    Console.WriteLine($"Created {string.Join(" & ", files)}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }
        }
        
    }
}
