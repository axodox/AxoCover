using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AxoCover.Models.Toolkit
{
  public static class DispatcherHelper
  {
    public static void InvokeAsnyc(Action action)
    {
      Application.Current.Dispatcher.BeginInvoke(action);
    }

    public static void Invoke(Action action)
    {
      if (Application.Current.Dispatcher.CheckAccess())
      {
        action();
      }
      else
      {
        Application.Current.Dispatcher.Invoke(action);
      }
    }
  }
}
