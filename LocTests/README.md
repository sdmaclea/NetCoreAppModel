The LocTests directory contains test focused on verifying low level 
localization support.

Its original intent was to answer the question whether `AssemblyLoadContext`
could be used with Satellite assemblies and provide proper isolation.

To answer the isolation question this project need to create two "plugins"
with identical simple names.  So that they would conflict in the Default ALC.

I had intended to include this as a CoreCLR test, but was unable to get 
the resources building correctly in the CoreCLR test build environment.

There is a solution file which builds most of the projects, but the duplicate 
project must be built separately (since sln does not like two projects with
same simple name).

The code assume a directory structure for where to find the plugin assemblies 
and their satellites.   I tested this on Linux with softlinks.  


```bash
cd ClassLibVer2/
dotnet build
cd -
dotnet build
cd /home/stmaclea/git/NetCoreAppModel/LocTests/SatelliteALCIsolation/bin/Debug/netcoreapp2.1/
ln -s ../../../../ClassLibWithSatellite/bin/Debug/netstandard2.0 ClassLibVer1
ln -s ../../../../ClassLibVer2/bin/Debug/netstandard2.0 ClassLibVer2
cd -
dotnet run --project SatelliteALCIsolation
```

Windows should also work with similar procdure (copy instead of soft link)
