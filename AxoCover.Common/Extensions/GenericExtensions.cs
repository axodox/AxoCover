using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace AxoCover.Common.Extensions
{
  public static class GenericExtensions
  {
    public static IEnumerable<Type> FindImplementers<T>(this Assembly assembly)
    {
      return assembly.GetExportedTypes()
        .Where(p => p.GetInterfaces().Any(q => q == typeof(T)));
    }

    public static string ToXml<T>(this T value)
    {
      var serializer = new XmlSerializer(typeof(T));
      using (var writer = new StringWriter())
      {
        serializer.Serialize(writer, value);
        writer.Flush();
        return writer.ToString();
      }
    }

    public static T ParseXml<T>(string fileName)
    {
      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        var result = (T)new XmlSerializer(typeof(T)).Deserialize(stream);
        if (result is IFileSource)
        {
          (result as IFileSource).FilePath = fileName;
        }
        return result;
      }
    }

    public static string GetDescription(this Exception exception)
    {
      var text = string.Empty;
      while (exception != null)
      {
        text += exception.GetType().Name + ": " + exception.Message + "\r\n";
        text += exception.StackTrace + "\r\n\r\n";
        exception = exception.InnerException;
      }
      return text;
    }

    public static string ToAbsolutePath(this string path)
    {
      if (!Path.IsPathRooted(path))
      {
        path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path);
      }
      return path;
    }

    public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
      TValue value;
      if (dictionary.TryGetValue(key, out value))
      {
        return value;
      }
      else
      {
        return default(TValue);
      }
    }

    public static bool CheckAs<T>(this object value, Func<T, bool> func)
    {
      return value is T ? func((T)value) : false;
    }

    public static IEnumerable<T> Flatten<T>(this T parent, Func<T, IEnumerable<T>> getChildren, bool includeParent = true)
    {
      var children = getChildren(parent).Flatten(getChildren);
      if (includeParent)
      {
        return new[] { parent }.Concat(children);
      }
      else
      {
        return children;
      }
    }

    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> enumeration, Func<T, IEnumerable<T>> getChildren)
    {
      var stack = new Stack<IEnumerator>();
      stack.Push(enumeration.GetEnumerator());
      while (stack.Count > 0)
      {
        var enumerator = stack.Peek();
        if (enumerator.MoveNext())
        {
          var item = (T)enumerator.Current;
          yield return item;

          var children = getChildren(item);
          if (children != null)
          {
            stack.Push(children.GetEnumerator());
          }
        }
        else
        {
          stack.Pop();
        }
      }
    }

    public static IEnumerable<T> Crawl<T>(this T item, Func<T, T> getLayer, bool includeThis = false)
      where T : class
    {
      if (item == null)
        throw new ArgumentNullException(nameof(item));

      if (includeThis)
        yield return item;

      item = getLayer(item);
      while (item != null)
      {
        yield return item;
        item = getLayer(item);
      }
    }

    public static void AddRange<T>(this ISet<T> set, IEnumerable<T> enumerable)
    {
      foreach (var item in enumerable)
      {
        set.Add(item);
      }
    }

    public static void BeginInvoke(this Dispatcher dispatcher, Action action)
    {
      dispatcher.BeginInvoke(action as Delegate);
    }

    public static string CreateTempDirectory(string prefix)
    {
      var tempDir = Path.GetTempPath();
      string path;
      for (int i = 0; ; i++)
      {
        path = Path.Combine(tempDir, prefix + i);
        if (!Directory.Exists(path))
        {
          Directory.CreateDirectory(path);
          break;
        }
      }
      return path;
    }

    public static void KillWithChildren(this Process process)
    {
      using (var query = new ManagementObjectSearcher(
        "SELECT * " +
        "FROM Win32_Process " +
        "WHERE ParentProcessId=" + process.Id))
      {
        var results = query.Get();

        try
        {
          process.Kill();
        }
        catch { }

        foreach (var result in results)
        {
          var childProcess = Process.GetProcessById((int)(uint)result["ProcessId"]);
          childProcess.KillWithChildren();
        }
      }
    }

    public static bool Contains(this string text, string value, StringComparison comparison)
    {
      return text.IndexOf(value, comparison) >= 0;
    }

    public static IEnumerable<T> DoIf<T>(this IEnumerable<T> enumeration, Predicate<T> predicate, Action<T> action)
    {
      var items = enumeration.ToArray();
      foreach (var item in items)
      {
        if (predicate(item))
        {
          action(item);
        }
      }
      return items;
    }

    public static void OrderedAdd<T>(this IList<T> list, T item, Comparison<T> onCompare, ReplacementBehavior replacementBehavior = ReplacementBehavior.KeepBoth)
    {
      var index = 0;
      while (index < list.Count && onCompare(list[index], item) <= 0)
      {
        index++;
      }

      switch (replacementBehavior)
      {
        case ReplacementBehavior.Ignore:
          if (index > 0 && onCompare(list[index - 1], item) == 0)
          {
            return;
          }
          goto default;
        case ReplacementBehavior.Replace:
          if (index > 0 && onCompare(list[index - 1], item) == 0)
          {
            list.RemoveAt(index - 1);
            index--;
          }
          goto default;
        default:
          list.Insert(index, item);
          break;
      }
    }
  }

  public interface IFileSource
  {
    string FilePath { get; set; }
  }

  public enum ReplacementBehavior
  {
    KeepBoth,
    Replace,
    Ignore
  }
}
