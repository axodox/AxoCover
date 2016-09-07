using EnvDTE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSLangProj;
using VSLangProj80;

namespace AxoCover.Models.Extensions
{
  public static class ProjectExtensions
  {
    private const string _unitTestReference = "Microsoft.VisualStudio.QualityTools.UnitTestFramework";

    public static IEnumerable<Project> GetProjects(this Solution solution)
    {
      var stack = new Stack<IEnumerator>();
      foreach (Project project in solution.Projects)
      {
        yield return project;

        stack.Push(project.ProjectItems.GetEnumerator());

        while (stack.Count > 0)
        {
          var enumerator = stack.Peek();
          if (enumerator.MoveNext())
          {
            var projectItem = enumerator.Current as ProjectItem;
            if (projectItem != null)
            {
              var subProject = projectItem.Object as Project;
              if (subProject != null)
              {
                yield return subProject;
                stack.Push(subProject.ProjectItems.GetEnumerator());
              }
            }
          }
          else
          {
            stack.Pop();
          }
        }
      }
    }

    public static IEnumerable<FileCodeModel> GetSourceFiles(this Project project)
    {
      var stack = new Stack<IEnumerator>();
      stack.Push(project.ProjectItems.GetEnumerator());
      while (stack.Count > 0)
      {
        var enumerator = stack.Peek();
        if (enumerator.MoveNext())
        {
          var projectItem = enumerator.Current as ProjectItem;
          if (projectItem != null && projectItem.FileCodeModel != null)
          {
            yield return projectItem.FileCodeModel;
          }

          if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFolder)
          {
            stack.Push(projectItem.ProjectItems.GetEnumerator());
          }
        }
        else
        {
          stack.Pop();
        }
      }
    }

    public static string GetFilePath(this CodeElement codeElement)
    {
      var document = codeElement?.StartPoint.Parent.Parent;
      return document != null ? Path.Combine(document.Path, document.Name) : null;
    }

    public static IEnumerable<CodeElement> GetTopLevelClasses(this CodeElements codeElements)
    {
      return codeElements.GetCodeElements(vsCMElement.vsCMElementClass, vsCMElement.vsCMElementNamespace);
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
      var stack = new Stack<IEnumerator>();
      stack.Push(codeElements.GetEnumerator());
      while (stack.Count > 0)
      {
        var enumerator = stack.Peek();
        if (enumerator.MoveNext())
        {
          var codeElement = enumerator.Current as CodeElement;
          if (codeElement != null && codeElement.Kind == kind)
          {
            yield return codeElement;
          }

          if (containers.Contains(codeElement.Kind))
          {
            stack.Push(codeElement.Children.GetEnumerator());
          }
        }
        else
        {
          stack.Pop();
        }
      }
    }

    public static bool IsDotNetUnitTestProject(this Project project)
    {
      var dotNetProject = project.Object as VSProject2;

      return dotNetProject != null && dotNetProject.References
        .OfType<Reference>()
        .Any(p => p.Name == _unitTestReference);
    }

    public static string GetOutputDllPath(this Project project)
    {
      var outputDirectoryPath = project
        ?.ConfigurationManager
        ?.ActiveConfiguration
        ?.Properties
        .Item("OutputPath")
        .Value as string;

      if (outputDirectoryPath == null)
        return null;

      if (!Path.IsPathRooted(outputDirectoryPath))
      {
        outputDirectoryPath = Path.Combine(Path.GetDirectoryName(project.FullName), outputDirectoryPath);
      }

      var outputFileName = project.Properties.Item("OutputFileName").Value as string;
      return Path.Combine(outputDirectoryPath, outputFileName);
    }
  }
}
