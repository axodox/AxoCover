using AxoCover.Common.Extensions;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VSLangProj;

namespace AxoCover.Models.Extensions
{
  public static class ProjectExtensions
  {
    public static IEnumerable<Project> GetProjects(this Solution solution)
    {
      return solution.Projects
        .OfType<Project>()
        .Flatten(p => p.ProjectItems?
          .OfType<ProjectItem>()
          .Flatten(q => q.ProjectItems?.OfType<ProjectItem>())
          .Select(q => q.Object)
          .OfType<Project>());
    }

    public static IEnumerable<string> FindFiles(this Solution solution, Regex filter)
    {
      return solution
        .GetProjects()
        .SelectMany(p => p.FindFiles(filter));
    }

    public static IEnumerable<string> FindFiles(this Project project, Regex filter)
    {
      return project.ProjectItems?
        .OfType<ProjectItem>()
        .Flatten(p => p.Kind == Constants.vsProjectItemKindPhysicalFolder ? p.ProjectItems.OfType<ProjectItem>() : null)
        .SelectMany(p => Enumerable.Range(1, p.FileCount).Select(q => p.FileNames[(short)q]))
        .Where(p => filter.IsMatch(p ?? string.Empty)) ?? new string[0];
    }

    public static IEnumerable<FileCodeModel> GetSourceFiles(this Project project)
    {
      return project.ProjectItems?
        .OfType<ProjectItem>()
        .Flatten(p => p.Kind == Constants.vsProjectItemKindPhysicalFolder ? p.ProjectItems.OfType<ProjectItem>() : null)
        .Select(p => p.Try(q => q.FileCodeModel))
        .Where(p => p != null) ?? new FileCodeModel[0];
    }

    public static string GetFilePath(this CodeElement codeElement)
    {
      var document = codeElement?.StartPoint.Parent.Parent;
      return document != null ? Path.Combine(document.Path, document.Name) : null;
    }

    public static IEnumerable<CodeElement> GetClasses(this CodeElements codeElements)
    {
      return codeElements.GetCodeElements(vsCMElement.vsCMElementClass, vsCMElement.vsCMElementNamespace, vsCMElement.vsCMElementClass);
    }

    public static IEnumerable<CodeElement> GetMethods(this CodeElement codeElement)
    {
      if (codeElement.Kind != vsCMElement.vsCMElementClass &&
        codeElement.Kind != vsCMElement.vsCMElementInterface &&
        codeElement.Kind != vsCMElement.vsCMElementStruct)
        throw new ArgumentException(nameof(codeElement));

      return codeElement.Children.GetCodeElements(vsCMElement.vsCMElementFunction);
    }

    public static IEnumerable<CodeElement> GetCodeElements(this CodeElements codeElements, vsCMElement kind, params vsCMElement[] containers)
    {
      return codeElements
        .OfType<CodeElement>()
        .Flatten(p => containers.Contains(p.Kind) ? p.Children.OfType<CodeElement>() : null)
        .Where(p => p.Kind == kind);
    }

    public static bool TryGetReference(this Project project, string referenceName, out Reference reference)
    {
      reference = null;
      var dotNetProject = project.Object as VSProject;
      try
      {
        if (dotNetProject != null)
        {
          reference = dotNetProject.References
            .OfType<Reference>()
            .FirstOrDefault(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, referenceName));
        }
      }
      catch { }

      return reference != null;
    }

    public static string GetAssemblyName(this Project project)
    {
      return project
        .Properties
        .GetProperty<string>("AssemblyName");
    }

    public static T GetProperty<T>(this EnvDTE.Properties properties, string name)
    {
      try
      {
        return (T)properties?
          .Item(name)
          .Value;
      }
      catch
      {
        return default(T);
      }
    }

    public static Dictionary<string, object> GetAllProperties(this EnvDTE.Properties properties)
    {
      var propertyDictionary = new Dictionary<string, object>();

      if(properties != null)
      {
        foreach(Property property in properties)
        {
          try
          {
            propertyDictionary.Add(property.Name, property.Value);
          }
          catch
          {
            //Skip 
          }
        }
      }

      return propertyDictionary;
    }

    public static string GetOutputDllPath(this Project project)
    {
      var outputDirectoryPath = project?
        .ConfigurationManager?
        .ActiveConfiguration?
        .Properties
        .GetProperty<string>("OutputPath");

      if (outputDirectoryPath == null)
        return null;

      if (!Path.IsPathRooted(outputDirectoryPath))
      {
        outputDirectoryPath = Path.Combine(Path.GetDirectoryName(project.FullName), outputDirectoryPath);
      }

      var outputFileName = project.Properties.GetProperty<string>("OutputFileName");

      if (string.IsNullOrEmpty(outputFileName))
      {
        outputFileName = project.Properties.GetProperty<string>("AssemblyName") + ".dll";
      }

      if (string.IsNullOrEmpty(outputFileName))
        return null;

      return Path.Combine(outputDirectoryPath, outputFileName);
    }
  }
}
