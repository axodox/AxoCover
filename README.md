# AxoTools
Nice and free .Net code coverage support for Visual Studio with OpenCover.

Features:
* Run, debug and cover unit tests in .Net projects
* Browse unit tests in a clean hierarchical view
* Display sequence and branch coverage in the code editor with detailed display for partially covered lines
* Analyze coverage by test
* Show exceptions encountered during testing in code, one click jump to the failed tests
* Display coverage report for the whole codebase
* Export coverage results to HTML
* Support for MSTest (V1 & V2), xUnit and NUnit test frameworks
* Clean test output with one click to free up space
* Works well with both dark and light themes

# How to build?
You will need Visual Studio 2012 or later with Visual Studio SDK installed. Using the Microsoft.Net.Compilers NuGet package new C# features are still usable, even if you build in older versions of Visual Studio. For compatibility reasons the DLLs shipping with VSSDK are not used anymore, instead compatible versions of Visual Studio assemblies are acquired from NuGet. This way even when building with latest version of Visual Studio 2017 compatibility is maintained with 2012.