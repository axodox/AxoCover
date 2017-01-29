using AxoCover.Models.Events;
using System;

namespace AxoCover.Views
{
  public interface IDialog
  {
    event EventHandler<ResultEventArgs<bool?>> ClosingDialog;

    string Title { get; }

    void OnClosing();
  }
}
