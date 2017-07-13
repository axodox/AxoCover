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

    public static void ExecuteWithFileRedirection(TestAdapterOptions adapterOptions, Action action, Action<TestMessageLevel, string> log)
    {
      if (adapterOptions.IsRedirectingAssemblies)
      {
        InitializeIfNeeded();
        ExecuteWithFileRedirectionInternal(adapterOptions, action, log);
      }
      else
      {
        log(TestMessageLevel.Informational, "Global file redirection is turned off.");
        action();
      }
    }

    private static void InitializeIfNeeded()
    {
      if(!_isInitialized)
      {
        _isInitialized = true;
        var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        Assembly.LoadFrom(Path.Combine(root, Environment.Is64BitProcess ? "x64" : "x86", "AxoCover.Native.dll"));
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
