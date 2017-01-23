using System;

namespace AxoCover.Views
{
  public interface IDialog
  {
    event EventHandler<bool?> ClosingDialog;

    string Title { get; }

    void OnClosing();
  }
}
