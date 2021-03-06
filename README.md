# ABANDONED

I have decided to abandon this project. I recently came across the [Typewriter project](https://frhagn.github.io/Typewriter/) which with a little work can spit out definition files exactly the same as TypeLITE. It has support for VS2017 and has worked for me for all my needs. I've even been able to get it to generate api classes for each asp.net controller action available which is where I wanted this project to eventually get to. 

# Typescript.Definitions.Tools

This is currently a Work in Progress....

A helper library to generate typescript definition files and typescript files from c# code specifically .NET Core Web applications.

This project took much of its inspiration from TypeLITE and the EntityFramework Core Tools project.

[TypeLITE](http://type.litesolutions.net/)

[EntityFrameworkCore](https://github.com/aspnet/EntityFramework)

# Installation NetCore 1.0

After creating a new .Net core 1.0 application add the following to dependecies and tools in `project.json`
    
    "Typescript.Definitions.Tools": "1.0.1"

Add the following method to `Startup.cs`

    public void ConfigureDefinitions(IDefinitionBuilder definitonBuilder)
    {
    	/* Configure definitions here. */
    }
    
# Installation NetCore 1.1

After creating a new .Net core 1.1 application add the following to dependecies and tools in `project.json`
    
    "Typescript.Definitions.Tools": "1.1.0-preview1-final"

Create a new class and have it implement `ITypedef`, example below.

    public class Typedef : ITypedef
    {
        public void Configure(IDefinitionBuilder builder)
        {
        	/* Configure definitions here. */
        }
    }

# Definition Configuration

Inside the ConfigureDefinitions method you can create any number of definiton files. There is only a single method on IDefinitionBuilder `AddDefinition` you can use it like so.

    definitionBuilder.AddDefinition(
        def => def.For<ExampleClass>(optionalConfig => optionalConfig.Named("OverrideTypeName"))
                  .For(typeof(OtherClass))
                  .For(someAssemblyToScan)
                  //Use the following method to change the name of the definition and constants files that are generated.
                  .Filename(definitionFilename: "differentDefinitionName", constantsFilename: "differentConstantsName") 
                  //Use the following method to change the outdir of the definition files, NOT YET IMPLEMENTED/TESTED MAY NOT WORK
                  .OutDir("C:\\SomeOtherDir")
                  )

Most of the methods are exactly the same as you would use in TypeLite. The only difference is the filename method, outdir method, and an optional parameter on the `For` methods that allows you to configure the type. Typelite had you chaining off the `For` to modify the type you just added it but I found the syntax for that to be confusing to read.

Assembly scanning works off of attributes, they are exactly the same (and should work the same) as TypeLite.

# Generating Definitions

Once your project startup file is configured you can have the tool generate the definition files in one of two ways.

Manually execute the following from a cmd line in the Project Directory.
    
    dotnet tsd

Add the following to the scripts section of `project.json` (this should regenerate the definitions after each successful build)

    "postcompile": [ "dotnet tsd --no-build" ]
