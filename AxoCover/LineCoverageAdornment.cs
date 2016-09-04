using AxoCover.Models;
using AxoCover.Models.Data;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AxoCover
{
  public class LineCoverageAdornment
  {
    private readonly ICoverageProvider _coverageProvider;
    private readonly IWpfTextView _textView;
    private readonly IAdornmentLayer _adornmentLayer;
    private readonly ITextDocumentFactoryService _documentFactory;

    private FileCoverage _fileCoverage = FileCoverage.Empty;

    public LineCoverageAdornment(IWpfTextView textView, ITextDocumentFactoryService documentFactory)
    {
      if (textView == null)
        throw new ArgumentNullException(nameof(textView));

      _coverageProvider = ContainerProvider.Container.Resolve<ICoverageProvider>();
      _textView = textView;
      _documentFactory = documentFactory;

      _adornmentLayer = _textView.GetAdornmentLayer(TextViewCreationListener.CoverageAdornmentLayerName);
      _textView.LayoutChanged += OnLayoutChanged;

      _coverageProvider.CoverageUpdated += (o, e) => UpdateCoverage();
      UpdateCoverage();
    }

    private void UpdateCoverage()
    {
      ITextDocument textDocument;
      if (_documentFactory.TryGetTextDocument(_textView.TextBuffer, out textDocument))
      {
        _fileCoverage = _coverageProvider.GetFileCoverage(textDocument.FilePath);
      }

      if (_textView.TextViewLines != null)
      {
        foreach (ITextViewLine line in _textView.TextViewLines)
        {
          UpdateLine(line);
        }
      }
    }

    private void UpdateLine(ITextViewLine line)
    {
      var span = new SnapshotSpan(_textView.TextSnapshot, Span.FromBounds(line.Start, line.End));
      _adornmentLayer.RemoveAdornmentsByVisualSpan(span);

      var lineNumber = _textView.TextSnapshot.GetLineNumberFromPosition(line.Start);
      var coverage = _fileCoverage[lineNumber];

      if (coverage.State == CoverageState.Unknown)
        return;

      var rect = new Rect(0, line.Top, 4, line.Height);
      var geometry = new RectangleGeometry(rect);
      geometry.Freeze();

      var brush = Brushes.Transparent;
      switch (coverage.State)
      {
        case CoverageState.Covered:
          brush = Brushes.Green;
          break;
        case CoverageState.Mixed:
          brush = Brushes.Yellow;
          break;
        case CoverageState.Uncovered:
          brush = Brushes.Red;
          break;
      }
      var drawing = new GeometryDrawing(brush, null, geometry);
      drawing.Freeze();

      var drawingImage = new DrawingImage(drawing);
      drawingImage.Freeze();

      var image = new Image() { Source = drawingImage };

      Canvas.SetLeft(image, geometry.Bounds.Left);
      Canvas.SetTop(image, geometry.Bounds.Top);

      _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
    }

    private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
    {
      foreach (ITextViewLine line in e.NewOrReformattedLines)
      {
        UpdateLine(line);
      }
    }
  }
}
