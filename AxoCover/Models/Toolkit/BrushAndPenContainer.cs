using System.Windows.Media;

namespace AxoCover.Models.Toolkit
{
  public class BrushAndPenContainer
  {
    public BrushAndPenContainer(Color color, double thickness)
    {
      _color = color;
      _thickness = thickness;
      UpdateResources();
    }

    private double _thickness;
    public double Thickness
    {
      get { return _thickness; }
      set
      {
        _thickness = value;
        UpdateResources();
      }
    }

    private Color _color;
    public Color Color
    {
      get { return _color; }
      set
      {
        _color = value;
        UpdateResources();
      }
    }

    private Brush _brush;
    public Brush Brush
    {
      get { return _brush; }
    }

    private Pen _pen;
    public Pen Pen
    {
      get { return _pen; }
    }

    private void UpdateResources()
    {
      _brush = new SolidColorBrush(_color);
      _brush.Freeze();
      _pen = new Pen(_brush, _thickness);
      _pen.Freeze();
    }
  }
}
