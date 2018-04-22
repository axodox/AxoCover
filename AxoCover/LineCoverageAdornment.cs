using AxoCover.Commands;
using AxoCover.Common.Extensions;
using AxoCover.Controls;
using AxoCover.Models.Editor;
using AxoCover.Models.Storage;
using AxoCover.Models.Testing.Data;
using AxoCover.Models.Testing.Results;
using AxoCover.Models.Toolkit;
using AxoCover.ViewModels;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AxoCover
{
  public class LineCoverageAdornment
  {
    private const double _sequenceCoverageLineWidth = 4d;
    private const double _branchCoverageSpotGap = 0d;
    private const double _branchCoverageSpotHeightDivider = 4d;    
    private const double _branchCoverageSpotBorderThickness = 0.5d;
    private const double _modifiedOpacity = 0.25d;

    private readonly ICoverageProvider _coverageProvider;
    private readonly IResultProvider _resultProvider;
    private readonly IWpfTextView _textView;
    private readonly IAdornmentLayer _adornmentLayer;
    private readonly ITextDocumentFactoryService _documentFactory;
    private readonly IEditorContext _editorContext;

    private FileCoverage _fileCoverage = FileCoverage.Empty;
    private FileResults _fileResults = FileResults.Empty;

    private LineMapping _coverageMapping = new LineMapping(0);
    private LineMapping _resultMapping = new LineMapping(0);

    private readonly BrushAndPenContainer _selectedBrushAndPen;
    private readonly BrushAndPenContainer _coveredBrushAndPen;
    private readonly BrushAndPenContainer _mixedBrushAndPen;
    private readonly BrushAndPenContainer _uncoveredBrushAndPen;
    private readonly BrushAndPenContainer _exceptionOriginBrushAndPen;
    private readonly BrushAndPenContainer _exceptionTraceBrushAndPen;

    private readonly Dictionary<CoverageState, BrushAndPenContainer> _brushesAndPens;

    private ITextDocument _textDocument;

    private readonly SelectTestCommand _selectTestCommand;
    private readonly JumpToTestCommand _jumpToTestCommand;
    private readonly RunTestCommand _runTestCommand;
    private readonly CoverTestCommand _coverTestCommand;
    private readonly DebugTestCommand _debugTestCommand;
    private readonly IOptions _options;

    private static HashSet<TestMethod> _selectedTests;
    public static HashSet<TestMethod> SelectedTests
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

    private static TestSolutionViewModel _testSolution;
    public static TestSolutionViewModel TestSolution
    {
      get
      {
        return _testSolution;
      }
      set
      {
        _testSolution = value;
        _testSolutionChanged?.Invoke();
      }
    }

    public static event Action _testSolutionChanged;

    private Dictionary<int, TestItemViewModel> _testAnchors = new Dictionary<int, TestItemViewModel>();
    private LineMapping _testAnchorMapping = new LineMapping(0);

    private static bool _isEnabled = true;
    public static bool IsEnabled
    {
      get
      {
        return _isEnabled;
      }
      set
      {
        _isEnabled = value;
        _isHighlightingChanged?.Invoke();
      }
    }

    public static void SelectTestNode(TestItem testItem)
    {
      SelectedTests = new HashSet<TestMethod>(testItem?.Flatten(p => p.Children).OfType<TestMethod>() ?? new TestMethod[0]);
    }

    private static event Action _isHighlightingChanged;
    
    public LineCoverageAdornment(
      IWpfTextView textView,
      ITextDocumentFactoryService documentFactory,
      ICoverageProvider coverageProvider,
      IResultProvider resultProvider,
      IEditorContext editorContext,
      IOptions options,
      SelectTestCommand selectTestCommand,
      JumpToTestCommand jumpToTestCommand,
      RunTestCommand runTestCommand,
      CoverTestCommand coverTestCommand,
      DebugTestCommand debugTestCommand)
    {
      if (textView == null)
        throw new ArgumentNullException(nameof(textView));

      _editorContext = editorContext;
      _options = options;
      _selectedBrushAndPen = new BrushAndPenContainer(_options.SelectedColor, _branchCoverageSpotBorderThickness);
      _coveredBrushAndPen = new BrushAndPenContainer(_options.CoveredColor, _branchCoverageSpotBorderThickness);
      _mixedBrushAndPen = new BrushAndPenContainer(_options.MixedColor, _branchCoverageSpotBorderThickness);
      _uncoveredBrushAndPen = new BrushAndPenContainer(_options.UncoveredColor, _branchCoverageSpotBorderThickness);
      _exceptionOriginBrushAndPen = new BrushAndPenContainer(_options.ExceptionOriginColor, _branchCoverageSpotBorderThickness);
      _exceptionTraceBrushAndPen = new BrushAndPenContainer(_options.ExceptionTraceColor, _branchCoverageSpotBorderThickness);
      
      _brushesAndPens = new Dictionary<CoverageState, BrushAndPenContainer>()
      {
        { CoverageState.Unknown, new BrushAndPenContainer(Colors.Transparent, _branchCoverageSpotBorderThickness) },
        { CoverageState.Uncovered, _uncoveredBrushAndPen },
        { CoverageState.Mixed, _mixedBrushAndPen },
        { CoverageState.Covered, _coveredBrushAndPen }
      };

      _documentFactory = documentFactory;
      _textView = textView;
      _textView.TextBuffer.Changed += OnTextBufferChanged;
      _editorContext.BuildFinished += OnBuildFinished;
            
      _coverageProvider = coverageProvider;
      _resultProvider = resultProvider;

      _selectTestCommand = selectTestCommand;
      _jumpToTestCommand = jumpToTestCommand;

      _runTestCommand = runTestCommand;
      _coverTestCommand = coverTestCommand;
      _debugTestCommand = debugTestCommand;
      
      _adornmentLayer = _textView.GetAdornmentLayer(TextViewCreationListener.CoverageAdornmentLayerName);
      TryInitilaizeDocument();

      _textView.LayoutChanged += OnLayoutChanged;

      _coverageProvider.CoverageUpdated += OnCoverageUpdated;
      _resultProvider.ResultsUpdated += OnResultsUpdated;
      UpdateAnchors();
      UpdateCoverage();
      UpdateResults();      

      _options.PropertyChanged += OnOptionsPropertyChanged;
      _isHighlightingChanged += UpdateAllLines;
      _testSolutionChanged += UpdateAnchors;

      _textView.Closed += OnClosed;
    }

    public class LineStatus
    {
      public int Target { get; set; }

      public bool IsModified { get; set; }

      public LineStatus(int target)
      {
        Target = target;
      }
    }

    public class LineMapping
    {
      private readonly List<LineStatus> _lineMap;

      public LineStatus this[int lineNumber] => lineNumber >= 0 && lineNumber < _lineMap.Count ? _lineMap[lineNumber] : new LineStatus(-1);

      public LineMapping(int lineCount, bool isModified = false)
      {
        _lineMap = Enumerable
          .Range(0, lineCount)
          .Select(p => new LineStatus(p) { IsModified = isModified })
          .ToList();
      }

      public void ProcessChange(TextContentChangedEventArgs e)
      {
        foreach (var change in e.Changes)
        {
          var lineNumber = e.Before.GetLineNumberFromPosition(change.OldPosition);
          if (lineNumber < 0 || lineNumber >= _lineMap.Count)
          {
            GenericExtensions.Debug();
            return;
          }

          _lineMap[lineNumber].IsModified = true;
          if (change.LineCountDelta >= 0)
          {
            if (lineNumber + 1 > _lineMap.Count)
            {
              GenericExtensions.Debug();
              return;
            }
            _lineMap.InsertRange(lineNumber + 1, Enumerable.Range(0, change.LineCountDelta).Select(p => new LineStatus(-1)));
          }
          else
          {
            if (lineNumber + 1 > _lineMap.Count || lineNumber + 1 - change.LineCountDelta > _lineMap.Count)
            {
              GenericExtensions.Debug();
              return;
            }
            _lineMap.RemoveRange(lineNumber + 1, -change.LineCountDelta);
          }
        }
      }
    }

    private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
    {
      _resultMapping.ProcessChange(e);
      _coverageMapping.ProcessChange(e);
      _testAnchorMapping.ProcessChange(e);
    }
    
    private void OnBuildFinished(object sender, EventArgs e)
    {
      if (_editorContext.IsBuildSuccessful) UpdateAnchors();
    }

    private void OnResultsUpdated(object sender, EventArgs e)
    {
      UpdateResults();
    }

    private void OnCoverageUpdated(object sender, EventArgs e)
    {
      UpdateCoverage();
    }

    private void OnClosed(object sender, EventArgs e)
    {
      _textView.Closed -= OnClosed;      
      _textView.LayoutChanged -= OnLayoutChanged;
      _textView.TextBuffer.Changed -= OnTextBufferChanged;
      _editorContext.BuildFinished -= OnBuildFinished;
      _coverageProvider.CoverageUpdated -= OnCoverageUpdated;
      _resultProvider.ResultsUpdated -= OnResultsUpdated;
      _options.PropertyChanged -= OnOptionsPropertyChanged;
      _isHighlightingChanged -= UpdateAllLines;
      _testSolutionChanged -= UpdateAnchors;
      _adornmentLayer.RemoveAllAdornments();
    }

    private string[] _visualizationProperties = new[]
    {
      nameof(IOptions.SelectedColor),
      nameof(IOptions.CoveredColor),
      nameof(IOptions.MixedColor),
      nameof(IOptions.UncoveredColor),
      nameof(IOptions.ExceptionOriginColor),
      nameof(IOptions.ExceptionTraceColor),
      nameof(IOptions.IsShowingLineCoverage),
      nameof(IOptions.IsShowingPartialCoverage),
      nameof(IOptions.IsShowingBranchCoverage),
      nameof(IOptions.IsShowingExceptions),
      nameof(IOptions.IsShowingAnchors)
    };

    private void OnOptionsPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == null || _visualizationProperties.Contains(e.PropertyName))
      {
        _selectedBrushAndPen.Color = _options.SelectedColor;
        _coveredBrushAndPen.Color = _options.CoveredColor;
        _mixedBrushAndPen.Color = _options.MixedColor;
        _uncoveredBrushAndPen.Color = _options.UncoveredColor;
        _exceptionOriginBrushAndPen.Color = _options.ExceptionOriginColor;
        _exceptionTraceBrushAndPen.Color = _options.ExceptionTraceColor;

        UpdateAllLines();
      }
    }

    private bool TryInitilaizeDocument()
    {
      if (_textDocument == null)
      {
        _documentFactory.TryGetTextDocument(_textView.TextBuffer, out _textDocument);
      }
      return _textDocument != null;
    }
    
    private void UpdateAnchors()
    {
      if (!TryInitilaizeDocument() || _testSolution == null) return;

      var projectItem = _editorContext
        .Solution
        .FindProjectItem(_textDocument.FilePath);
      if (projectItem == null || projectItem.ContainingProject == null || projectItem.FileCodeModel == null) return;

      var projectModel = projectItem.ContainingProject;
      var testProject = _testSolution.Children.FirstOrDefault(p => p.CodeItem.Name == projectModel.Name);
      if (testProject == null) return;

      var testAnchors = new Dictionary<int, TestItemViewModel>();
      var classModels = projectItem.FileCodeModel.CodeElements.GetClasses();
      foreach (var classModel in classModels)
      {
        var path = classModel.FullName.Split('.');
        var target = testProject;
        foreach (var segment in path)
        {
          target = target.Children.FirstOrDefault(p => p.CodeItem.Name == segment);
          if (target == null) break;
        }

        if (target == null || target.CodeItem.Kind != CodeItemKind.Class) continue;

        var methodModels = classModel.GetMethods();
        var testMethods = target.Children;
        foreach (var testMehod in testMethods)
        {
          var methodModel = methodModels.FirstOrDefault(p => p.Name == testMehod.CodeItem.Name);
          if (methodModel == null) continue;

          testAnchors[methodModel.StartPoint.Line - 1] = testMehod;
        }
      }
      _testAnchors = testAnchors;
      _testAnchorMapping = new LineMapping(_textView.TextSnapshot.LineCount);
      UpdateAllLines();
    }

    private async void UpdateCoverage()
    {
      if (TryInitilaizeDocument())
      {
        var isModified = _textDocument.LastContentModifiedTime > _editorContext.LastBuildTime.ToUniversalTime();
        _coverageMapping = new LineMapping(_textView.TextSnapshot.LineCount, isModified);
        _fileCoverage = await _coverageProvider.GetFileCoverageAsync(_textDocument.FilePath);
        UpdateAllLines();
      }
    }

    private async void UpdateResults()
    {
      if (TryInitilaizeDocument())
      {
        var isModified = _textDocument.LastContentModifiedTime > _editorContext.LastBuildTime.ToUniversalTime();
        _resultMapping = new LineMapping(_textView.TextSnapshot.LineCount, isModified);
        _fileResults = await _resultProvider.GetFileResultsAsync(_textDocument.FilePath);
        UpdateAllLines();
      }
    }

    private void UpdateAllLines()
    {
      _adornmentLayer.RemoveAllAdornments();
      if (_textView.TextViewLines != null && IsEnabled)
      {
        foreach (ITextViewLine line in _textView.TextViewLines)
        {
          if (line.IsValid)
          {
            UpdateLine(line);
          }
        }
      }
    }

    private void UpdateLine(ITextViewLine line)
    {
      var span = new SnapshotSpan(_textView.TextSnapshot, Span.FromBounds(line.Start, line.End));
      _adornmentLayer.RemoveAdornmentsByVisualSpan(span);

      var lineNumber = _textView.TextSnapshot.GetLineNumberFromPosition(line.Start);

      var coverageLineStatus = _coverageMapping[lineNumber];
      var resultLineStatus = _resultMapping[lineNumber];
      var anchorLineStatus = _testAnchorMapping[lineNumber];

      var coverage = _fileCoverage[coverageLineStatus.Target];
      var results = _fileResults[resultLineStatus.Target];
      var anchor = _testAnchors.TryGetValue(anchorLineStatus.Target);

      var snapshotLine = _textView.TextSnapshot.GetLineFromLineNumber(lineNumber);

      if (coverage.SequenceCoverageState != CoverageState.Unknown)
      {
        if (_options.IsShowingLineCoverage)
        {
          AddSequenceAdornment(line, span, coverage, !coverageLineStatus.IsModified);
        }

        if (_options.IsShowingPartialCoverage && !coverageLineStatus.IsModified)
        {
          AddUncoveredAdornment(snapshotLine, span, coverage);
        }
      }

      if (line.IsFirstTextViewLineForSnapshotLine)
      {
        if (_options.IsShowingBranchCoverage && coverage.SequenceCoverageState != CoverageState.Unknown)
        {
          AddBranchAdornment(line, span, coverage, !coverageLineStatus.IsModified);
        }

        if (_options.IsShowingExceptions)
        {
          AddResultAnchorAdornment(line, span, results, !resultLineStatus.IsModified);
        }

        if (_options.IsShowingAnchors)
        {
          AddTestAnchorAdornment(line, span, anchor, !anchorLineStatus.IsModified);
        }
      }
    }

    private void AddSequenceAdornment(ITextViewLine line, SnapshotSpan span, LineCoverage coverage, bool isUpToDate)
    {
      var rect = new Rect(0d, line.Top, _sequenceCoverageLineWidth, line.Height);
      var geometry = new RectangleGeometry(rect);
      geometry.Freeze();

      var brush = _brushesAndPens[coverage.SequenceCoverageState].Brush;
      if (coverage.SequenceCoverageState == CoverageState.Covered &&
        coverage.LineVisitors.Any(p => SelectedTests.Contains(p)))
      {
        brush = _selectedBrushAndPen.Brush;
      }

      var drawing = new GeometryDrawing(brush, null, geometry);
      drawing.Freeze();

      var drawingImage = new DrawingImage(drawing);
      drawingImage.Freeze();

      var toolTip = new StackPanel();

      var header = new TextBlock()
      {
        Text = string.Format(Resources.VisitorCount, coverage.VisitCount),
        TextWrapping = TextWrapping.Wrap
      };
      toolTip.Children.Add(header);

      var image = new Image()
      {
        Source = drawingImage,
        ToolTip = toolTip,
        Opacity = isUpToDate ? 1d : _modifiedOpacity
      };
      Canvas.SetLeft(image, geometry.Bounds.Left);
      Canvas.SetTop(image, geometry.Bounds.Top);
      SharedDictionaryManager.InitializeDictionaries(image.Resources.MergedDictionaries);

      if (coverage.LineVisitors.Count > 0)
      {
        var description = new TextBlock()
        {
          Text = string.Join("\r\n", coverage.LineVisitors.Select(p => p.ShortName)),
          TextWrapping = TextWrapping.Wrap,
          Opacity = 0.7d
        };
        toolTip.Children.Add(description);

        image.Tag = coverage.LineVisitors.ToArray();
        image.MouseRightButtonDown += (o, e) => e.Handled = true;
        image.MouseRightButtonUp += OnTestItemRightButtonUp;

        image.MouseLeftButtonDown += (o, e) => e.Handled = true;
        image.MouseLeftButtonUp += (o, e) => _selectTestCommand.Execute(coverage.LineVisitors.First());
        image.Cursor = Cursors.Hand;
      }

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

    private void AddBranchAdornment(ITextViewLine line, SnapshotSpan span, LineCoverage coverage, bool isUpToDate)
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

          var image = new Image()
          {
            Source = drawingImage,
            Opacity = isUpToDate ? 1d : _modifiedOpacity
          };

          var testMethod = coverage.BranchVisitors[groupIndex][index].FirstOrDefault();
          if (testMethod != null)
          {
            image.MouseLeftButtonDown += (o, e) => e.Handled = true;
            image.MouseLeftButtonUp += (o, e) => _selectTestCommand.Execute(testMethod);
            image.Cursor = Cursors.Hand;
            image.Tag = coverage.BranchVisitors[groupIndex][index].ToArray();
            image.MouseRightButtonDown += (o, e) => e.Handled = true;
            image.MouseRightButtonUp += OnTestItemRightButtonUp;
            SharedDictionaryManager.InitializeDictionaries(image.Resources.MergedDictionaries);
          }

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

    private void AddResultAnchorAdornment(ITextViewLine line, SnapshotSpan span, LineResult[] lineResults, bool isUpToDate)
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
            Text = string.Join(Environment.NewLine, group.Select(p => p.TestMethod.ShortName).Distinct()),
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

        var viewBox = new Viewbox()
        {
          Width = _textView.LineHeight,
          Height = _textView.LineHeight,
          Stretch = Stretch.Uniform
        };

        var button = new ActionButton()
        {
          Icon = drawingImage,
          CommandParameter = lineResults.FirstOrDefault().TestMethod,
          Command = _selectTestCommand,
          ToolTip = toolTip,
          Cursor = Cursors.Hand,
          Tag = new AnchorData()
          {
            Targets = lineResults.Select(p => p.TestMethod).ToArray(),
            Type = AnchorType.Error
          },
          Opacity = isUpToDate ? 1 : _modifiedOpacity
        };
        button.MouseRightButtonDown += (o, e) => e.Handled = true;
        button.MouseRightButtonUp += OnTestItemRightButtonUp;
        viewBox.Child = button;

        Canvas.SetLeft(viewBox, _sequenceCoverageLineWidth);
        Canvas.SetTop(viewBox, line.Top);

        _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, viewBox, null);
      }
    }

    private void AddTestAnchorAdornment(ITextViewLine line, SnapshotSpan span, TestItemViewModel viewModel, bool isUpToDate)
    {
      if (viewModel == null) return;

      var viewBox = new Viewbox()
      {
        Width = _textView.LineHeight,
        Height = _textView.LineHeight,
        Stretch = Stretch.Uniform
      };

      var button = new ActionButton()
      {
        DataContext = viewModel,
        Opacity = isUpToDate ? 1 : _modifiedOpacity,
        CommandParameter = viewModel.CodeItem,
        Command = _selectTestCommand,
        Cursor = Cursors.Hand,
        Tag = new AnchorData()
        {
          Targets = new TestMethod[] { viewModel.CodeItem as TestMethod },
          Type = AnchorType.Test
        }
      };
      button.SetBinding(ActionButton.IconProperty,
        new Binding(nameof(viewModel.IconPath)));
      button.MouseRightButtonDown += (o, e) => e.Handled = true;
      button.MouseRightButtonUp += OnTestItemRightButtonUp;
      viewBox.Child = button;

      Canvas.SetLeft(viewBox, _sequenceCoverageLineWidth);
      Canvas.SetTop(viewBox, line.Top);

      _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, viewBox, null);
    }

    private class AnchorData
    {
      public AnchorType Type { get; set; }

      public TestMethod[] Targets { get; set; }
    }

    private enum AnchorType
    {
      Test,
      Error
    }

    private void OnTestItemRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      var button = sender as FrameworkElement;
      var anchorData = button.Tag as AnchorData;
      if (anchorData == null || anchorData.Targets.Length == 0) return;

      var contextMenu = new ContextMenu();
      if(anchorData.Type == AnchorType.Test)
      {
        AddSubMenu(contextMenu, anchorData.Targets, Resources.RunTest, "test", _runTestCommand);
        AddSubMenu(contextMenu, anchorData.Targets, Resources.CoverTest, "cover", _coverTestCommand);
      }
      AddSubMenu(contextMenu, anchorData.Targets, Resources.DebugTest, "debug", _debugTestCommand);
      if (anchorData.Type == AnchorType.Error)
      {
        AddSubMenu(contextMenu, anchorData.Targets, Resources.JumpToTest, "source", _jumpToTestCommand);
        AddSubMenu(contextMenu, anchorData.Targets, Resources.SelectTest, null, _selectTestCommand);
      }

      contextMenu.PlacementTarget = button;
      contextMenu.IsOpen = true;
      e.Handled = true;
    }

    private void AddSubMenu(ContextMenu contextMenu, TestMethod[] tests, string header, string icon, ICommand command)
    {
      var selectMenu = new MenuItem()
      {
        Header = header,
        Icon = icon == null ? null : new Image() { Source = new BitmapImage(new Uri(@"/AxoCover;component/Resources/" + icon + ".png", UriKind.Relative)) }
      };
      contextMenu.Items.Add(selectMenu);

      if (tests.Length > 1)
      {
        foreach (var test in tests)
        {
          var menuItem = new MenuItem()
          {
            Header = test.ShortName,
            CommandParameter = test,
            Command = command
          };
          selectMenu.Items.Add(menuItem);
        }
      }
      else
      {
        selectMenu.CommandParameter = tests[0];
        selectMenu.Command = command;
      }
    }

    private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
    {
      if (!IsEnabled) return;

      foreach (ITextViewLine line in e.NewOrReformattedLines)
      {
        UpdateLine(line);
      }
    }
  }
}
