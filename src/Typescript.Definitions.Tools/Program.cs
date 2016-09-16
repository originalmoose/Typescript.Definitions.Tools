using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Typescript.Definitions.Tools
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

            var dotnetParams = new DotnetBaseParams("dotnet-tsd", "Typescript Definition NET Core Tools", "Creates typescript definition files from .NET classes.");
            
            dotnetParams.Parse(args);
            
            if (dotnetParams.IsHelp)
            {
                return;
            }

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

                var definitionsBuilder = new TypescriptDefinitionBuilder();

                services.AddSingleton<IDefinitionBuilder, TypescriptDefinitionBuilder>((p) => definitionsBuilder);

                Invoke(_startupType, new[] { "ConfigureDefinitions" }, services);

                foreach (var builder in definitionsBuilder.Builders)
                {
                    var files = builder.Generate(startupProjectPath);
                    Console.WriteLine($"Created {string.Join(" & ", files)}");
                }
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
                    ? ActivatorUtilities.GetServiceOrCreateInstance(services.BuildServiceProvider(), type)
                    : null;

                var parameters = method.GetParameters();
                var arguments = new object[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;
                    arguments[i] = parameterType == typeof(IServiceCollection)
                        ? services
                        : ActivatorUtilities.GetServiceOrCreateInstance(services.BuildServiceProvider(), parameterType);
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
    }
}
