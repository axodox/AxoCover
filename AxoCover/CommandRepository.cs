using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;

namespace AxoCover
{
  internal sealed class CommandRepository
  {
    public enum CommandId : int
    {
      OpenAxoCover = 0x0100,
      ToggleCoverage = 0x0200
    }

    public static readonly Guid CommandSet = new Guid("713f743a-d55e-47be-bfc4-4f4259f6fee6");
    
    private readonly Package _package;
        
    private readonly OleMenuCommandService _commandService;
    
    private CommandRepository(Package package)
    {
      _package = package;
      _commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if (_commandService != null)
      {
        AddCommand(CommandId.OpenAxoCover, OnOpenAxoCover);
        AddCommand(CommandId.ToggleCoverage, OnToggleCoverage, p => p.Checked = true);
      }
    }
    
    private void AddCommand(CommandId id, EventHandler callback, Action<MenuCommand> initialize = null)
    {
      var menuCommandId = new CommandID(CommandSet, (int)id);
      var menuItem = new MenuCommand(callback, menuCommandId);
      _commandService.AddCommand(menuItem);
      initialize?.Invoke(menuItem);
    }
    
    public static CommandRepository Instance
    {
      get;
      private set;
    }
    
    private IServiceProvider ServiceProvider
    {
      get
      {
        return _package;
      }
    }
    
    public static void Initialize(Package package)
    {
      Instance = new CommandRepository(package);
    }
        
    private void OnOpenAxoCover(object sender, EventArgs e)
    {
      var window = _package.FindToolWindow(typeof(TestExplorerToolWindow), 0, true);
      (window.Frame as IVsWindowFrame).ShowNoActivate();
    }

    private void OnToggleCoverage(object sender, EventArgs e)
    {
      LineCoverageAdornment.IsEnabled = !LineCoverageAdornment.IsEnabled;
      (sender as MenuCommand).Checked = LineCoverageAdornment.IsEnabled;
    }
  }
}
