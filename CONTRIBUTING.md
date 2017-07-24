# Background knowledge on Visual Studio extension development
## Visual Studio components
This project contains both C# and C++/CLI code. Thus, you need to have C# development and C++ development (C++/CLI) related Visual Studio components installed so as to compile all projects.

Visual Studio SDK is needed to generate the installer package (.vsix).

### Visual Studio 2012
TODO

### Visual Studio 2013
TODO

### Visual Studio 2015
TODO

### Visual Studio 2017
In Visual Studio Installer, you need to enable

* .NET desktop development
* Desktop development with C++
* Visual Studio extension development

> Note that C++/CLI is an optional component, so you need to check the box under Summary section.

Besides, you need to install Visual Studio 2012 C++ tooling, by installing at least [Visual Studio 2012 Express for Windows Desktop](https://chocolatey.org/packages/VisualStudio2012WDX)

## Launch Visual Studio with debug build
To debug the code, you need to have a debug build, package it as .vsix, and install to Visual Studio, and then launch a VS instance. 

A batch file debug.vs2017.bat is prepared to automate the steps.

To attach to this instance and debug, you have the freedom to use either Visual Studio or Rider (from JetBrains).
