using AxoCover.Models.Data;
using System.Linq;

namespace AxoCover.Models.Extensions
{
  public static class ModelExtensions
  {
    public static bool IsTest(this TestItem testItem)
    {
      return (testItem.Kind == CodeItemKind.Method && !testItem.Children.Any()) || testItem.Kind == CodeItemKind.Data;
    }
  }
}
