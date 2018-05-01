using AxoCover.Models;
using AxoCover.Models.Telemetry;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace AxoCover
{
  [Export(typeof(IWpfTextViewCreationListener))]
  [ContentType("text")]
  [TextViewRole(PredefinedTextViewRoles.Document)]
  public sealed class TextViewCreationListener : IWpfTextViewCreationListener
  {
    public const string CoverageAdornmentLayerName = "CoverageAdornment";

    private readonly ITextDocumentFactoryService _documentFactory;

#pragma warning disable 649, 169
    [Export(typeof(AdornmentLayerDefinition))]
    [Name(CoverageAdornmentLayerName)]
    [Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text)]
    private AdornmentLayerDefinition _coverageAdornmentLayer;
#pragma warning restore 649, 169

    [ImportingConstructor]
    public TextViewCreationListener(ITextDocumentFactoryService textDocumentFactoryService)
    {
      _documentFactory = textDocumentFactoryService;
    }

    public void TextViewCreated(IWpfTextView textView)
    {
      var telemetryManager = ContainerProvider.Container.Resolve<ITelemetryManager>();

      try
      {
        ContainerProvider.Container.Resolve<LineCoverageAdornment>(
          new ParameterOverride("textView", textView),
          new ParameterOverride("documentFactory", _documentFactory));
      }
      catch(Exception e)
      {
        telemetryManager.UploadExceptionAsync(e);
      }
    }
  }
}
