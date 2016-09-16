# Typescript.Definitions.Tools

This is currently a Work in Progress....

A helper library to generate typescript definition files and typescript files from c# code.

This project took much of its inspiration from TypeLITE and the EntityFramework Core Tools project.

[TypeLITE](http://type.litesolutions.net/)

[EntityFrameworkCore](https://github.com/aspnet/EntityFramework)

# Installation

After creating a new .Net core application add the following to dependecies and tools in `project.json`
    
    "Typescript.Definitions.Tools": "1.0.0"

Add the following method to `Startup.cs`

    public void ConfigureDefinitions(IDefinitionBuilder definitonBuilder)
    {
    }
    
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
