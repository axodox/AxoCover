using AxoCover.Models;
using AxoCover.Models.Data;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AxoCover
{
  public class LineCoverageAdornment
  {
    private const double _sequenceCoverageLineWidth = 4d;
    private const double _branchCoverageSpotGap = 0d;
    private const double _branchCoverageSpotHeightDivider = 4d;
    private const double _branchCoverageSpotBorderThickness = 0.5d;

    private readonly ICoverageProvider _coverageProvider;
    private readonly IWpfTextView _textView;
    private readonly IAdornmentLayer _adornmentLayer;
    private readonly ITextDocumentFactoryService _documentFactory;

    private FileCoverage _fileCoverage = FileCoverage.Empty;

    private Dictionary<CoverageState, Brush> _brushes = new Dictionary<CoverageState, Brush>()
    {
      { CoverageState.Unknown, Brushes.Transparent },
      { CoverageState.Uncovered, Brushes.Red },
      { CoverageState.Mixed, Brushes.Yellow },
      { CoverageState.Covered, Brushes.Green }
    };

    private Dictionary<CoverageState, Pen> _pens = new Dictionary<CoverageState, Pen>()
    {
      { CoverageState.Unknown, new Pen(Brushes.Transparent, _branchCoverageSpotBorderThickness) },
      { CoverageState.Uncovered, new Pen(Brushes.Red, _branchCoverageSpotBorderThickness) },
      { CoverageState.Mixed, new Pen(Brushes.Yellow, _branchCoverageSpotBorderThickness) },
      { CoverageState.Covered, new Pen(Brushes.Green, _branchCoverageSpotBorderThickness) }
    };

    public LineCoverageAdornment(IWpfTextView textView, ITextDocumentFactoryService documentFactory)
    {
      foreach (var pen in _pens.Values)
      {
        pen.Freeze();
      }

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

      if (coverage.SequenceCoverageState == CoverageState.Unknown)
        return;

      AddSequenceAdornment(line, span, coverage);
      AddUncoveredAdornment(line, span, coverage);
      AddBranchAdornment(line, span, coverage);
    }

    private void AddSequenceAdornment(ITextViewLine line, SnapshotSpan span, LineCoverage coverage)
    {
      var rect = new Rect(0d, line.Top, _sequenceCoverageLineWidth, line.Height);
      var geometry = new RectangleGeometry(rect);
      geometry.Freeze();

      var drawing = new GeometryDrawing(_brushes[coverage.SequenceCoverageState], null, geometry);
      drawing.Freeze();

      var drawingImage = new DrawingImage(drawing);
      drawingImage.Freeze();

      var image = new Image() { Source = drawingImage };

      Canvas.SetLeft(image, geometry.Bounds.Left);
      Canvas.SetTop(image, geometry.Bounds.Top);

      _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
    }

    private void AddUncoveredAdornment(ITextViewLine line, SnapshotSpan span, LineCoverage coverage)
    {
      if (coverage.SequenceCoverageState == CoverageState.Mixed)
      {
        foreach (var uncoveredSection in coverage.UncoveredSections)
        {
          var highlighSpan = new SnapshotSpan(
            _textView.TextSnapshot, Span.FromBounds(
              line.Start + (uncoveredSection.Start == 0 ? 0 : uncoveredSection.Start),
              line.Start + (uncoveredSection.End == 0 ? line.Length : uncoveredSection.End))
              );

          var geometry = _textView.TextViewLines.GetMarkerGeometry(highlighSpan);
          geometry.Freeze();

          var drawing = new GeometryDrawing(null, _pens[CoverageState.Mixed], geometry);
          drawing.Freeze();

          var drawingImage = new DrawingImage(drawing);
          drawingImage.Freeze();

          var image = new Image() { Source = drawingImage };

          Canvas.SetLeft(image, geometry.Bounds.Left);
          Canvas.SetTop(image, geometry.Bounds.Top);

          _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
        }
      }
    }

    private void AddBranchAdornment(ITextViewLine line, SnapshotSpan span, LineCoverage coverage)
    {
      var diameter = _textView.LineHeight / _branchCoverageSpotHeightDivider;
      var spacing = _branchCoverageSpotGap + diameter;
      var top = (line.Height - diameter) / 2d;

      var brush = _brushes[coverage.BranchCoverageState];
      var pen = _pens[coverage.BranchCoverageState];

      var left = _sequenceCoverageLineWidth * 1.5d;
      foreach (var branchPoint in coverage.BranchesVisited)
      {
        foreach (var branch in branchPoint)
        {
          var rect = new Rect(left, line.Top + top, diameter, diameter);
          var geometry = new EllipseGeometry(rect);
          geometry.Freeze();

          var drawing = new GeometryDrawing(branch ? brush : null, pen, geometry);
          drawing.Freeze();

          var drawingImage = new DrawingImage(drawing);
          drawingImage.Freeze();

          var image = new Image() { Source = drawingImage };

          Canvas.SetLeft(image, geometry.Bounds.Left);
          Canvas.SetTop(image, geometry.Bounds.Top);

          _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);

          left += spacing;
        }

        left += spacing;
      }
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
