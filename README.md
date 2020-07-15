# Reaction Splitter (C#)

Testing out cheminformatics in C# using Indigo  

API:  
https://lifescience.opensource.epam.com/indigo/api/index.html

## How I Almost Got Indigo to Work

1. I tried putting the NuGet dependency directly in my .csproj file. It compiled with warnings, but when i tried to run it I got this error:
```
/usr/local/share/dotnet/sdk/3.1.301/Microsoft.Common.CurrentVersion.targets(2081,5): warning MSB3246: Resolved file has a bad image, no metadata, or is otherwise inaccessible. Assembly file '/Users/john/.nuget/packages/indigo.net/1.4.0-beta.12/lib/netstandard2.0/vcruntime140_1.dll' could not be opened -- PE image doesn't contain managed metadata. [/Users/john/workspace/csrxnsplitter/RxnSplitter/RxnSplitter.csproj]
Unhandled exception. System.DllNotFoundException: Unable to load shared library 'indigo' or one of its dependencies. In order to help diagnose loading problems, consider setting the DYLD_PRINT_LIBRARIES environment variable: dlopen(libindigo, 1): image not found
   at com.epam.indigo.IndigoLib.indigoAllocSessionId()
   at com.epam.indigo.Indigo.init(String lib_path)
   at com.epam.indigo.Indigo..ctor(String lib_path)
   at com.epam.indigo.Indigo..ctor()
   at RxnSplitter.Program.Main(String[] args) in /Users/john/workspace/csrxnsplitter/RxnSplitter/Program.cs:line 10
john@Chrysologus RxnSplitter %
```
2. I tried building from source. I cloned the Indigo project and tried the latest directions in the README.
```
$ git clone ...
$ cd Indigo
$ python build_scripts/indigo-release-libs.py --preset=mac10.14
$ python build_scripts/indigo-release-utils.py --preset=mac10.14
```
3. The compiler failed saying there was no mac10.14 version of the compiler present. In the place where it looked, I only had mac10.15. I guess this hasn't been updated since the Catalina release of XCode. When I tried mac10.15, the Python code failed saying there was no value for that key. So I went into the `indigo-release-libs.py` and `indigo-release-utils.py` files and added a key for mac10.15 corresponding to the ones already present for mac10.14 and 10.12. Then the libs command ran without error and generated the Mac-specific dlls and dylibs. The utils command failed with a linker error, but since the files were compiled maybe I can get by with what I have.
```
Ld /Users/john/workspace/Indigo/build/indigo_utils_Xcode__DSUBSYSTEM_NAME_10.15/dist/Mac/10.15/lib/Release/indigo-deco normal x86_64
    cd /Users/john/workspace/Indigo/build_scripts/indigo-utils
    /Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang -target x86_64-apple-macos10.15 -isysroot /Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX10.15.sdk -L/Users/john/workspace/Indigo/build/indigo_utils_Xcode__DSUBSYSTEM_NAME_10.15/dist/Mac/10.15/lib/Release -F/Users/john/workspace/Indigo/build/indigo_utils_Xcode__DSUBSYSTEM_NAME_10.15/dist/Mac/10.15/lib/Release -filelist /Users/john/workspace/Indigo/build/indigo_utils_Xcode__DSUBSYSTEM_NAME_10.15/indigo-deco/IndigoUtils.build/Release/indigo-deco.build/Objects-normal/x86_64/indigo-deco.LinkFileList -pthread -Wl,-search_paths_first -Wl,-headerpad_max_install_names -lindigo -Xlinker -dependency_info -Xlinker /Users/john/workspace/Indigo/build/indigo_utils_Xcode__DSUBSYSTEM_NAME_10.15/indigo-deco/IndigoUtils.build/Release/indigo-deco.build/Objects-normal/x86_64/indigo-deco_dependency_info.dat -o /Users/john/workspace/Indigo/build/indigo_utils_Xcode__DSUBSYSTEM_NAME_10.15/dist/Mac/10.15/lib/Release/indigo-deco
Undefined symbols for architecture x86_64:
  "_osDirLastError", referenced from:
      _main in main.o
  "_osDirNext", referenced from:
      _main in main.o
  "_osDirSearch", referenced from:
      _main in main.o
ld: symbol(s) not found for architecture x86_64
clang: error: linker command failed with exit code 1 (use -v to see invocation)

** BUILD FAILED **
```
4. Then I ran `python build_scripts/indigo-make-by-libs.py --type=dotnet`. This gave me a warning about not finding files in a folder ending in mac10.7. I looked through the project and found a line in `api/make-dotnet-wrappers.py` that used a blanket '10.7' for anything related to mac libraries. I changed it to '10.15'. It got past that error, but then failed saying there was no powershell command on the path. I have PowerShell installed, but I invoke it with the pwsh command. So I changed `powershell` to `pwsh` in the PreBuild target of Indigo.Net.csproj. Then that command passed without error, finding the libraries and generating the stubs in lib/netstandard2.0 and an nupkg file at `api/dotnet/bin/Release/Indigo.Net.1.4.0-beta.17.nupkg`.
5. I used the nuget command to add it to a local NuGet filesystem repository:
```
nuget add api/dotnet/bin/Release/indigo.net-1.4.0-beta.17.nupkg -source /Users/john/tempNuget
```
6. I tried adding the version of Indigo I just built and deployed to my project. I first had to modify my ~/.nuget/NuGet/NuGet.Config to add the local repository:
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local" value="/Users/john/tempNuget" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```
Then in the project I was writing I did `dotnet add package Indigo.Net -s local -v 1.4.0-beta.17`, which modified the csproj file as expected.
7. Then I did `dotnet build && dotnet run`, and I got the same errors as at the start, about PE image not containing managed metadata. I guess this is a dead end.
