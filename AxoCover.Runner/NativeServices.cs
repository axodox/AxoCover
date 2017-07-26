using AxoCover.Common.Models;
using AxoCover.Common.Runner;
using AxoCover.Native;
using System;
using System.IO;
using System.Reflection;

namespace AxoCover.Runner
{
  static class NativeServices
  {
    private static bool _isInitialized = false;

    private static bool _isAvailable = false;

    public static void ExecuteWithFileRedirection(TestAdapterOptions adapterOptions, Action action, Action<TestMessageLevel, string> log)
    {
      if (!adapterOptions.IsRedirectingAssemblies || !ExecuteCall(() => ExecuteWithFileRedirectionInternal(adapterOptions, action, log), log))
      { 
        log(TestMessageLevel.Informational, "Global file redirection is turned off.");
        action();
      }
    }
    
    private static bool ExecuteCall(Action action, Action<TestMessageLevel, string> log)
    {
      if(!_isInitialized)
      {
        _isInitialized = true;
        try
        {
          var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
          Assembly.LoadFrom(Path.Combine(root, Environment.Is64BitProcess ? "x64" : "x86", "AxoCover.Native.dll"));
          _isAvailable = true;
        }
        catch
        {
          _isAvailable = false;
        }
      }

      if(_isAvailable)
      {
        action();
        return true;
      }
      else
      {
        log(TestMessageLevel.Warning, "Could not initialize native services! Maybe you are missing Visual C++ 2012 Redistributable x86 and/or x64? Please visit https://www.microsoft.com/en-us/download/details.aspx?id=30679 to download & install these.");
        return false;
      }
    }

    private static void ExecuteWithFileRedirectionInternal(TestAdapterOptions adapterOptions, Action action, Action<TestMessageLevel, string> log)
    {
      try
      {
        if (adapterOptions.IsRedirectingAssemblies)
        {
          log(TestMessageLevel.Informational, "File redirection is enabled for the following files:");
          foreach (var file in adapterOptions.RedirectedAssemblies)
          {
            log(TestMessageLevel.Informational, file);
          }
        }
        else
        {
          log(TestMessageLevel.Informational, "File redirection is disabled.");
        }

        if (adapterOptions.IsRedirectingAssemblies)
        {
          log(TestMessageLevel.Informational, "Setting up file redirection hooks...");
          if (FileRemapper.TryRedirectFiles(adapterOptions.RedirectedAssemblies))
          {
            log(TestMessageLevel.Informational, "File redirection hooks are enabled.");
          }
          else
          {
            log(TestMessageLevel.Warning, "File redirection hooks failed!");
          }

          FileRemapper.ExcludeNonexistentDirectories = adapterOptions.RedirectionOptions.HasFlag(FileRedirectionOptions.ExcludeNonexistentDirectories);
          FileRemapper.ExcludeNonexistentFiles = adapterOptions.RedirectionOptions.HasFlag(FileRedirectionOptions.ExcludeNonexistentFiles);
          FileRemapper.IncludeBaseDirectory = adapterOptions.RedirectionOptions.HasFlag(FileRedirectionOptions.IncludeBaseDirectory);
        }

        action();
      }
      finally
      {
        if (adapterOptions.IsRedirectingAssemblies)
        {
          FileRemapper.TryRedirectFiles(new string[0]);
          log(TestMessageLevel.Informational, "File redirection rules are cleared.");
        }
      }
    }
  }
}
