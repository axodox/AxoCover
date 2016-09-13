using AxoCover.Models;
using AxoCover.Models.Commands;
using AxoCover.Models.Data;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly IResultProvider _resultProvider;
    private readonly IWpfTextView _textView;
    private readonly IAdornmentLayer _adornmentLayer;
    private readonly ITextDocumentFactoryService _documentFactory;

    private FileCoverage _fileCoverage = FileCoverage.Empty;
    private FileResults _fileResults = FileResults.Empty;

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

    private static bool _isHighlighting = true;
    public static bool IsHighlighting
    {
      get
      {
        return _isHighlighting;
      }
      set
      {
        _isHighlighting = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private static event Action _isHighlightingChanged;
    private string _filePath;

    private readonly NavigateToTestCommand _navigateToTestCommand;

    public LineCoverageAdornment(IWpfTextView textView, ITextDocumentFactoryService documentFactory,
      NavigateToTestCommand navigateToTestCommand)
    {
      foreach (var pen in _pens.Values)
      {
        pen.Freeze();
      }

      if (textView == null)
        throw new ArgumentNullException(nameof(textView));

      _documentFactory = documentFactory;
      _textView = textView;
      _navigateToTestCommand = navigateToTestCommand;
      TryInitilaizeFilePath();

      _coverageProvider = ContainerProvider.Container.Resolve<ICoverageProvider>();
      _resultProvider = ContainerProvider.Container.Resolve<IResultProvider>();

      _adornmentLayer = _textView.GetAdornmentLayer(TextViewCreationListener.CoverageAdornmentLayerName);
      _textView.LayoutChanged += OnLayoutChanged;

      _coverageProvider.CoverageUpdated += (o, e) => UpdateCoverage();
      _resultProvider.ResultsUpdated += (o, e) => UpdateResults();
      UpdateCoverage();
      UpdateResults();

      _isHighlightingChanged += UpdateAllLines;
    }

    private bool TryInitilaizeFilePath()
    {
      if (_filePath == null)
      {
        ITextDocument textDocument;
        if (_documentFactory.TryGetTextDocument(_textView.TextBuffer, out textDocument))
        {
          _filePath = textDocument.FilePath;
        }
      }
      return _filePath != null;
    }

    private async void UpdateCoverage()
    {
      if (TryInitilaizeFilePath())
      {
        _fileCoverage = await _coverageProvider.GetFileCoverageAsync(_filePath);
        UpdateAllLines();
      }
    }

    private async void UpdateResults()
    {
      if (TryInitilaizeFilePath())
      {
        _fileResults = await _resultProvider.GetFileResultsAsync(_filePath);
        UpdateAllLines();
      }
    }

    private void UpdateAllLines()
    {
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

      if (!IsHighlighting)
        return;

      var lineNumber = _textView.TextSnapshot.GetLineNumberFromPosition(line.Start);

      var coverage = _fileCoverage[lineNumber];
      var results = _fileResults[lineNumber];

      if (coverage.SequenceCoverageState == CoverageState.Unknown)
        return;

      var snapshotLine = _textView.TextSnapshot.GetLineFromLineNumber(lineNumber);

      AddSequenceAdornment(line, span, coverage);
      AddUncoveredAdornment(snapshotLine, span, coverage);

      if (line.IsFirstTextViewLineForSnapshotLine)
      {
        AddBranchAdornment(line, span, coverage);
        AddLineResultAdornment(line, span, results);
      }
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

    private void AddUncoveredAdornment(ITextSnapshotLine line, SnapshotSpan span, LineCoverage coverage)
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
          if (geometry == null)
            continue;

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

    private void AddLineResultAdornment(ITextViewLine line, SnapshotSpan span, LineResult[] lineResults)
    {
      if (lineResults.Length > 0)
      {
        var geometry = Geometry.Parse("F1M9.4141,8L12.4141,11 11.0001,12.414 8.0001,9.414 5.0001,12.414 3.5861,11 6.5861,8 3.5861,5 5.0001,3.586 8.0001,6.586 11.0001,3.586 12.4141,5z");
        geometry.Freeze();

        var drawing = new GeometryDrawing(lineResults.Any(p => p.IsPrimary) ? Brushes.Red : Brushes.Yellow, null, geometry);
        drawing.Freeze();

        var drawingImage = new DrawingImage(drawing);
        drawingImage.Freeze();

        var toolTip = new StackPanel()
        {
          MaxWidth = 600
        };

        foreach (var group in lineResults.GroupBy(p => p.ErrorMessage))
        {
          var header = new TextBlock()
          {
            Text = string.Join(Environment.NewLine, group.Select(p => p.TestName).Distinct()),
            TextWrapping = TextWrapping.Wrap
          };

          var description = new TextBlock()
          {
            Text = group.Key,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.7d,
            Margin = new Thickness(0, 0, 0, 10)
          };
          toolTip.Children.Add(header);
          toolTip.Children.Add(description);
        }
        toolTip.Children.OfType<TextBlock>().Last().Margin = new Thickness();

        var button = new Controls.Button()
        {
          Icon = drawingImage,
          Width = _textView.LineHeight,
          Height = _textView.LineHeight,
          CommandParameter = lineResults.FirstOrDefault().TestName,
          Command = _navigateToTestCommand,
          ToolTip = toolTip
        };

        Canvas.SetLeft(button, _sequenceCoverageLineWidth);
        Canvas.SetTop(button, line.Top);

        _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, button, null);
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
