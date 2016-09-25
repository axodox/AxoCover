using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;

namespace AxoCover
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class OpenAxoCoverCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 0x0100;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("713f743a-d55e-47be-bfc4-4f4259f6fee6");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package _package;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAxoCoverCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private OpenAxoCoverCommand(Package package)
    {
      if (package == null)
      {
        throw new ArgumentNullException("package");
      }

      _package = package;

      OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if (commandService != null)
      {
        var menuCommandID = new CommandID(CommandSet, CommandId);
        var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
        commandService.AddCommand(menuItem);
      }
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static OpenAxoCoverCommand Instance
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private IServiceProvider ServiceProvider
    {
      get
      {
        return _package;
      }
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static void Initialize(Package package)
    {
      Instance = new OpenAxoCoverCommand(package);
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void MenuItemCallback(object sender, EventArgs e)
    {
      var window = _package.FindToolWindow(typeof(TestExplorerToolWindow), 0, true);
      (window.Frame as IVsWindowFrame).ShowNoActivate();
    }
  }
}
