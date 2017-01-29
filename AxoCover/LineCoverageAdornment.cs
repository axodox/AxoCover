using AxoCover.Models;
using AxoCover.Models.Commands;
using AxoCover.Models.Data;
using AxoCover.Properties;
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

    private static readonly BrushAndPenContainer _selectedBrushAndPen = new BrushAndPenContainer(Settings.Default.SelectedColor, _branchCoverageSpotBorderThickness);
    public static Color SelectedColor
    {
      get { return _selectedBrushAndPen.Color; }
      set
      {
        _selectedBrushAndPen.Color = value;
        Settings.Default.SelectedColor = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private static readonly BrushAndPenContainer _coveredBrushAndPen = new BrushAndPenContainer(Settings.Default.CoveredColor, _branchCoverageSpotBorderThickness);
    public static Color CoveredColor
    {
      get { return _coveredBrushAndPen.Color; }
      set
      {
        _coveredBrushAndPen.Color = value;
        Settings.Default.CoveredColor = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private static readonly BrushAndPenContainer _mixedBrushAndPen = new BrushAndPenContainer(Settings.Default.MixedColor, _branchCoverageSpotBorderThickness);
    public static Color MixedColor
    {
      get { return _mixedBrushAndPen.Color; }
      set
      {
        _mixedBrushAndPen.Color = value;
        Settings.Default.MixedColor = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private static readonly BrushAndPenContainer _uncoveredBrushAndPen = new BrushAndPenContainer(Settings.Default.UncoveredColor, _branchCoverageSpotBorderThickness);
    public static Color UncoveredColor
    {
      get { return _uncoveredBrushAndPen.Color; }
      set
      {
        _uncoveredBrushAndPen.Color = value;
        Settings.Default.UncoveredColor = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private static readonly BrushAndPenContainer _exceptionOriginBrushAndPen = new BrushAndPenContainer(Settings.Default.ExceptionOriginColor, _branchCoverageSpotBorderThickness);
    public static Color ExceptionOriginColor
    {
      get { return _exceptionOriginBrushAndPen.Color; }
      set
      {
        _exceptionOriginBrushAndPen.Color = value;
        Settings.Default.ExceptionOriginColor = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private static readonly BrushAndPenContainer _exceptionTraceBrushAndPen = new BrushAndPenContainer(Settings.Default.ExceptionTraceColor, _branchCoverageSpotBorderThickness);
    public static Color ExceptionTraceColor
    {
      get { return _exceptionTraceBrushAndPen.Color; }
      set
      {
        _exceptionTraceBrushAndPen.Color = value;
        Settings.Default.ExceptionTraceColor = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private readonly static Dictionary<CoverageState, BrushAndPenContainer> _brushesAndPens = new Dictionary<CoverageState, BrushAndPenContainer>()
    {
      { CoverageState.Unknown, new BrushAndPenContainer(Colors.Transparent, _branchCoverageSpotBorderThickness) },
      { CoverageState.Uncovered, _uncoveredBrushAndPen },
      { CoverageState.Mixed, _mixedBrushAndPen },
      { CoverageState.Covered, _coveredBrushAndPen }
    };

    private static HashSet<string> _selectedTests;
    public static HashSet<string> SelectedTests
    {
      get
      {
        return _selectedTests;
      }
      set
      {
        _selectedTests = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private static bool _isShowingLineCoverage = Settings.Default.IsShowingLineCoverage;
    public static bool IsShowingLineCoverage
    {
      get
      {
        return _isShowingLineCoverage;
      }
      set
      {
        _isShowingLineCoverage = value;
        Settings.Default.IsShowingLineCoverage = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private static bool _isShowingPartialCoverage = Settings.Default.IsShowingPartialCoverage;
    public static bool IsShowingPartialCoverage
    {
      get
      {
        return _isShowingPartialCoverage;
      }
      set
      {
        _isShowingPartialCoverage = value;
        Settings.Default.IsShowingPartialCoverage = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private static bool _isShowingBranchCoverage = Settings.Default.IsShowingBranchCoverage;
    public static bool IsShowingBranchCoverage
    {
      get
      {
        return _isShowingBranchCoverage;
      }
      set
      {
        _isShowingBranchCoverage = value;
        Settings.Default.IsShowingBranchCoverage = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private static bool _isShowingExceptions = Settings.Default.IsShowingExceptions;
    public static bool IsShowingExceptions
    {
      get
      {
        return _isShowingExceptions;
      }
      set
      {
        _isShowingExceptions = value;
        Settings.Default.IsShowingExceptions = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    private static event Action _isHighlightingChanged;
    private string _filePath;

    private readonly NavigateToTestCommand _navigateToTestCommand;

    public LineCoverageAdornment(IWpfTextView textView, ITextDocumentFactoryService documentFactory,
      NavigateToTestCommand navigateToTestCommand)
    {
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
      _adornmentLayer.RemoveAllAdornments();
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
      var results = _fileResults[lineNumber];

      if (coverage.SequenceCoverageState == CoverageState.Unknown)
        return;

      var snapshotLine = _textView.TextSnapshot.GetLineFromLineNumber(lineNumber);

      if (IsShowingLineCoverage)
      {
        AddSequenceAdornment(line, span, coverage);
      }

      if (IsShowingPartialCoverage)
      {
        AddUncoveredAdornment(snapshotLine, span, coverage);
      }

      if (line.IsFirstTextViewLineForSnapshotLine)
      {
        if (IsShowingBranchCoverage)
        {
          AddBranchAdornment(line, span, coverage);
        }

        if (IsShowingExceptions)
        {
          AddLineResultAdornment(line, span, results);
        }
      }
    }

    private void AddSequenceAdornment(ITextViewLine line, SnapshotSpan span, LineCoverage coverage)
    {
      var rect = new Rect(0d, line.Top, _sequenceCoverageLineWidth, line.Height);
      var geometry = new RectangleGeometry(rect);
      geometry.Freeze();

      var brush = _brushesAndPens[coverage.SequenceCoverageState].Brush;
      if (coverage.SequenceCoverageState == CoverageState.Covered &&
        coverage.LineVisitors.Keys.Any(p => SelectedTests.Contains(p)))
      {
        brush = _selectedBrushAndPen.Brush;
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

    private void AddUncoveredAdornment(ITextSnapshotLine line, SnapshotSpan span, LineCoverage coverage)
    {
      if (coverage.SequenceCoverageState == CoverageState.Mixed)
      {
        foreach (var uncoveredSection in coverage.UncoveredSections)
        {
          var highlightStart = line.Start;
          if (uncoveredSection.Start == 0)
          {
            highlightStart += line.GetText().TakeWhile(p => char.IsWhiteSpace(p)).Count();
          }
          else
          {
            highlightStart += Math.Min(uncoveredSection.Start, line.Length);
          }

          var highlightEnd = line.Start + Math.Min(uncoveredSection.End, line.Length);

          if (highlightEnd <= highlightStart) continue;

          var highlightSpan = new SnapshotSpan(
            _textView.TextSnapshot, Span.FromBounds(
              highlightStart,
              highlightEnd
            ));

          var geometry = _textView.TextViewLines.GetMarkerGeometry(highlightSpan);
          if (geometry == null)
            continue;

          geometry.Freeze();

          var drawing = new GeometryDrawing(null, _brushesAndPens[CoverageState.Mixed].Pen, geometry);
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

      var brush = _brushesAndPens[coverage.BranchCoverageState].Brush;
      var pen = _brushesAndPens[coverage.BranchCoverageState].Pen;

      var groupIndex = 0;
      var index = 0;
      var left = _sequenceCoverageLineWidth * 1.5d;
      foreach (var branchPoint in coverage.BranchesVisited)
      {
        foreach (var branch in branchPoint)
        {
          var rect = new Rect(left, line.Top + top, diameter, diameter);
          var geometry = new EllipseGeometry(rect);
          geometry.Freeze();

          var brushOverride = brush;
          var penOverride = pen;
          if (branch && coverage.BranchVisitors[groupIndex][index].Any(p => SelectedTests.Contains(p)))
          {
            brushOverride = _selectedBrushAndPen.Brush;
            penOverride = _selectedBrushAndPen.Pen;
          }

          var drawing = new GeometryDrawing(branch ? brushOverride : null, penOverride, geometry);
          drawing.Freeze();

          var drawingImage = new DrawingImage(drawing);
          drawingImage.Freeze();

          var image = new Image() { Source = drawingImage };

          Canvas.SetLeft(image, geometry.Bounds.Left);
          Canvas.SetTop(image, geometry.Bounds.Top);

          _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);

          left += spacing;
          index++;
        }
        groupIndex++;
        index = 0;

        left += spacing;
      }
    }

    private void AddLineResultAdornment(ITextViewLine line, SnapshotSpan span, LineResult[] lineResults)
    {
      if (lineResults.Length > 0)
      {
        var geometry = Geometry.Parse("F1M9.4141,8L12.4141,11 11.0001,12.414 8.0001,9.414 5.0001,12.414 3.5861,11 6.5861,8 3.5861,5 5.0001,3.586 8.0001,6.586 11.0001,3.586 12.4141,5z");
        geometry.Freeze();

        var drawing = new GeometryDrawing(lineResults.Any(p => p.IsPrimary) ? _exceptionOriginBrushAndPen.Brush : _exceptionTraceBrushAndPen.Brush, null, geometry);
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

        var button = new Controls.ActionButton()
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
