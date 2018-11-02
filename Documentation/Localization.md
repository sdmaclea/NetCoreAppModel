# .NET Core resource support

## `resx` resource file format

`resx` file format support is briefly mentioned in the the changes to the
dotnet cli default includes.
See https://docs.microsoft.com/en-us/dotnet/core/tools/csproj

With the default includes `**/*.resx` files will automatically be
treated as resources.

If you have disabled default includes, resource files can be
added by adding `<EmbeddedResource include="path.resx"/>` entries to
your csproj.

## `resx` file naming conventions

To access resources the typical methodology invloves creating a
`System.Resources.ResourceManager` instance.

```C#
using System.Resources;

namespace MyAssemblyName
{
    class MyType
    {
    }
}
.....
    ResourceManager rm = new ResourceManager(typeof(MyType));
.....
```

For this scenario the neutral language resource file is typically named

`MyType.resx`

For language or culture specific resources the file is named

`MyType.<CultureInfo.Name>.resx`

e.g.

`MyType.en.resx` for a generic English resource.

`MyType.en-US.resx` for an English as spoken in the United States resource.

## Build conventions

A resource file of the form `MyType.resx` is compiled and added to the
assembly `MyAssembly.dll` with the name `MyAssembly.MyType.resource`

A resource file of the form `MyType.<CultureInfo.Name>.resx` is compiled
and added to the satellite assembly `<CultureInfo.Name>\MyAssembly.resources.dll`
with the name `MyAssembly.MyType.<CultureInfo.Name>.resource`

## ResourceManager resource location

When the ResourceManager wants to find a string or an object it must load the
resources file associated with the requested culture.

For instance ResourceManager has two methods to get a string.

```
GetString(String)
GetString(String, CultureInfo)
```

In the first case the culture is determined implicitly using the value
of the `CultureInfo.CurrentUICulture`.  So in both cases CultureInfo is
used.

The search algorithm for finding the resources is:
* Find the owning Assembly
* Find the resource by name

To find the owning assembly the ResourceManager iterates on the current
`CultureInfo`.  It starts from the requested culture and iterates through
the `CultureInfo.Parent` until the current culture is equal to the
`Neutral` culture.

Each current culture iteration tries to load the the assembly.  It
indirectly triggers an AssemblyLoadContext.Load(AssemblyName name) with

```
AssemblyName name = {
  Name=myType.Assembly.GetName().Name + ".resources",
  CultureInfo = currentCulture
};
```

When it reaches the neutral culture, it typically simply uses `myType.Assembly`.

Once the owning assembly is found the resource must be loaded from the
assembly by name.

## Diagnosing resource location failures

For the current version of .NET Core, the exception thrown when a
resource is not found makes it difficult to determine the root cause of
the failure.  Specifically whether the assembly could not be found and
loaded, or the resource file could not be located within the assembly.

For debugging assembly load failures, adding a handler to
`AssemblyLoadContext.Default.Resolving` can be helpful.  This event is
called when the AssemblyLoadContext.Default fails to load an Assembly.
This event will not be called for any assembly which was sucessfully loaded.

```
using System.Runtime.Loader;

Assembly ResolveTrace(AssemblyLoadContext alc, AssemblyName assemblyName)
{
    Console.WriteLine($"ResolveTrace(AssemblyName {assemblyName})");

    return null;
}

AssemblyLoadContext.Default.Resolving += ResolveTrace;
```

If the assembly is loaded correctly, the next failure point would be the
resource name.  The simplest solution is to look for the resurce file in
the obj intermediate directory.

IL Spy has also been recommended as a solution.
