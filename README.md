# AxoTools
Nice and free .Net code coverage support for Visual Studio with OpenCover.

[![Build status](https://ci.appveyor.com/api/projects/status/o315jyp6fswhf3ws/branch/master?svg=true)](https://ci.appveyor.com/project/axodox/axotools/branch/master)
[![Visual Studio Marketplace](https://img.shields.io/vscode-marketplace/d/axodox1.AxoCover.svg)](https://marketplace.visualstudio.com/items?itemName=axodox1.AxoCover)
[![Visual Studio Marketplace](https://img.shields.io/vscode-marketplace/v/axodox1.AxoCover.svg)](https://marketplace.visualstudio.com/items?itemName=axodox1.AxoCover)

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
You will need Visual Studio 2012 build tools for C++, Visual Studio and Windows SDK (8.1) installed. It is suggested to use Visual Studio 2017 for best IntelliSense support related to new C# features, but if you use earlier version, the Microsoft.Net.Compilers NuGet package makes new C# features still compile. For compatibility reasons the DLLs shipping with VSSDK are not used anymore, instead compatible versions of Visual Studio assemblies are acquired from NuGet. This way even when building with latest version of Visual Studio 2017 compatibility is maintained with 2012.
