using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.DependencyModel;
using System.Reflection;
using Microsoft.VisualBasic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Typescript.Definitions.Core
{
    public class Program
    {
        private static string _assemblyName;
        private static Type _startupType;
        private static string _environment;
        private static string _contentRootPath;

        public static void Main(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            Console.WriteLine("Waiting...");
            Console.ReadLine();

            var dotnetParams = new DotnetBaseParams("dotnet-tsd", "Typescript Definition NET Core Tools", "Creates typescript definition files from .NET classes.");

            dotnetParams.Parse(args);

            _environment = "Development";


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

                _contentRootPath = Path.GetDirectoryName(startupProject.ProjectFullPath);

                var assembly = startupProject.Assembly;

                _assemblyName = assembly.GetName().Name;

                _startupType = assembly.GetLoadableDefinedTypes().Where(t => typeof(IStartup).IsAssignableFrom(t.AsType()))
                    .Concat(assembly.GetLoadableDefinedTypes().Where(t => t.Name == "Startup"))
                    .Concat(assembly.GetLoadableDefinedTypes().Where(t => t.Name == "Program"))
                    .Concat(assembly.GetLoadableDefinedTypes().Where(t => t.Name == "App"))
                    .Select(t => t.AsType())
                    .FirstOrDefault();

                var services = ConfigureHostServices(new ServiceCollection());

                var builder = new TypescripDefinitionBuilder();

                services.AddSingleton(builder);

                var provider = Invoke(
                                   _startupType,
                                   new[] { "ConfigureDesignTimeServices" },
                                   services) as IServiceProvider
                               ?? services.BuildServiceProvider();
                

            }
            catch (Exception)
            {

                throw;
            }

        }


        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected static IServiceCollection ConfigureHostServices(IServiceCollection services)
        {
            services.AddSingleton<IHostingEnvironment>(
                new HostingEnvironment
                {
                    ContentRootPath = _contentRootPath,
                    EnvironmentName = _environment,
                    ApplicationName = _assemblyName
                });

            services.AddLogging();
            services.AddOptions();

            return services;
        }


        private static object Invoke(Type type, string[] methodNames, IServiceCollection services)
        {
            if (type == null)
            {
                return null;
            }

            MethodInfo method = null;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < methodNames.Length; i++)
            {
                method = type.GetTypeInfo().GetDeclaredMethod(methodNames[i]);
                if (method != null)
                {
                    break;
                }
            }

            if (method == null)
            {
                return null;
            }

            try
            {
                var instance = !method.IsStatic
                    ? ActivatorUtilities.GetServiceOrCreateInstance(GetHostServices(), type)
                    : null;

                var parameters = method.GetParameters();
                var arguments = new object[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;
                    arguments[i] = parameterType == typeof(IServiceCollection)
                        ? services
                        : ActivatorUtilities.GetServiceOrCreateInstance(GetHostServices(), parameterType);
                }

                return method.Invoke(instance, arguments);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                //_logger.Value.LogWarning(
                //    string.Format($"An error occurred while calling method '{method.Name}' on startup class '{type.ShortDisplayName()}'. Consider using IDbContextFactory to override the initialization of the DbContext at design-time. Error: {ex.Message}"));
                //_logger.Value.LogDebug(ex.ToString());

                return null;
            }
        }


        private static IServiceProvider GetHostServices()
            => ConfigureHostServices(new ServiceCollection()).BuildServiceProvider();
    }

    public class TypescripDefinitionBuilder
    {
        public IList<TypescriptDefinitionOption> Options { get; set; } = new List<TypescriptDefinitionOption>();
    }

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
