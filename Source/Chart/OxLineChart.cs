using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.Utils;
using DevExpress.XtraCharts;

namespace OxLib.Chart
{
	public partial class OxLineChart : ChartControl
	{
		private OxChartZoom zoom;
		public OxChartZoom Zooming
		{ get { return zoom; } }

		private OxChartCursor vertCursor;
		public OxChartCursor VertCursor
		{ get { return vertCursor; } }

		private OxChartDescription desc;
		public OxChartDescription Description
		{ get { return desc; } }

		private Rectangle clipRect;
		public Rectangle ClipRect
		{ get { return clipRect; } }

		public int SeriesVisibleCount
		{ get { return GetSeriesVisibleCount(); } }

		public OxLineChart()
		{
			Initialize();
			zoom = null;
			vertCursor = null;
			desc = null;
			clipRect = new Rectangle();
		}

		private void Initialize()
		{
			if (DesignMode == true)
			{
				Series cSeries = new Series("Series1", ViewType.SwiftPlot);
				cSeries.ArgumentScaleType = ScaleType.Numerical;
				cSeries.ValueScaleType = ScaleType.Numerical;

				Clear();
				AddSeries(cSeries);
			}

			Legend.Visibility = DefaultBoolean.False;
			CrosshairEnabled = DefaultBoolean.False;
		}

		public void Prepare()
		{
			zoom = new OxChartZoom(this);
			vertCursor = new OxChartCursor(this);
			desc = new OxChartDescription(this);

			SwiftPlotDiagram diagram = (SwiftPlotDiagram)Diagram;

			diagram.DefaultPane.EnableAxisXScrolling = DefaultBoolean.True;
			diagram.DefaultPane.EnableAxisXZooming = DefaultBoolean.False;
			diagram.DefaultPane.EnableAxisYScrolling = DefaultBoolean.True;
			diagram.DefaultPane.EnableAxisYZooming = DefaultBoolean.False;

			diagram.ScrollingOptions.UseKeyboard = false;
			diagram.ScrollingOptions.UseMouse = false;
			diagram.ScrollingOptions.UseScrollBars = true;
			diagram.ScrollingOptions.UseTouchDevice = false;
		}

		public void Clear()
		{
			Series.Clear();
		}

		public void AddSeries(Series aSeries)
		{
			Series.Add(aSeries);
		}

		public void AddSeries(string aName, Color aColor)
		{
			Series cSeries = new Series(aName, ViewType.SwiftPlot);

			cSeries.ArgumentScaleType = ScaleType.Numerical;
			cSeries.ValueScaleType = ScaleType.Numerical;
			((SwiftPlotSeriesView)cSeries.View).Color = aColor;

			AddSeries(cSeries);
		}

		public void ClearSeriesPoint()
		{
			for (int i = 0; i < Series.Count; i++)
			{
				Series[i].Points.Clear();
			}
		}

		public void ClearSeriesPoint(int aIndex)
		{
			Series[aIndex].Points.Clear();
		}

		public void AddSeriesPoint(int aIndex, SeriesPoint aPoint)
		{
			Series[aIndex].Points.Add(aPoint);
		}

		public void AddSeriesPoint(int aIndex, SeriesPoint[] aPoint)
		{
			Series[aIndex].Points.AddRange(aPoint);
		}

		public void SetSeriesSencondaryAxisX(int aSeriesIndex, int aAxisIndex)
		{
			((SwiftPlotSeriesView)Series[aSeriesIndex].View).AxisX = ((SwiftPlotDiagram)Diagram).SecondaryAxesX[aAxisIndex];
		}

		public void SetSeriesSencondaryAxisY(int aSeriesIndex, int aAxisIndex)
		{
			((SwiftPlotSeriesView)Series[aSeriesIndex].View).AxisY = ((SwiftPlotDiagram)Diagram).SecondaryAxesY[aAxisIndex];
		}

		public void SetPrimaryAxisX(AxisAlignment aAlign, string aText, StringAlignment aTextAlign, object aMin, object aMax)
		{
			SwiftPlotDiagram diagram = (SwiftPlotDiagram)Diagram;

			if (diagram == null) return;

			SetGridLinesAxisX(false);

			diagram.AxisX.Label.Font = new Font("Arial", 8);
			diagram.AxisX.Label.TextColor = Color.Black;
			diagram.AxisX.Label.EnableAntialiasing = DefaultBoolean.False;
			diagram.AxisX.Label.Visible = true;

			diagram.AxisX.Title.Text = aText;
			diagram.AxisX.Title.Font = new Font("Arial", 8);
			diagram.AxisX.Title.TextColor = Color.Black;
			diagram.AxisX.Title.EnableAntialiasing = DefaultBoolean.False;
			diagram.AxisX.Title.Alignment = aTextAlign;
			diagram.AxisX.Title.Visibility = DefaultBoolean.True;

			diagram.AxisX.Tickmarks.Length = 5;
			diagram.AxisX.Tickmarks.Thickness = 1;
			diagram.AxisX.Tickmarks.CrossAxis = false;
			diagram.AxisX.Tickmarks.Visible = true;
			diagram.AxisX.Tickmarks.MinorLength = 2;
			diagram.AxisX.Tickmarks.MinorThickness = 1;
			diagram.AxisX.Tickmarks.MinorVisible = true;

			diagram.AxisX.WholeRange.SideMarginsValue = 0;
			diagram.AxisX.WholeRange.SetMinMaxValues(aMin, aMax);
			diagram.AxisX.VisualRange.SideMarginsValue = 0;
			diagram.AxisX.VisualRange.SetMinMaxValues(aMin, aMax);

			diagram.AxisX.Alignment = aAlign; 
			diagram.AxisX.Thickness = 1;
			diagram.AxisX.MinorCount = 4;
			diagram.AxisX.Visibility = DefaultBoolean.True;
		}

		public void SetPrimaryAxisY(AxisAlignment aAlign, string aText, StringAlignment aTextAlign, object aMin, object aMax)
		{
			SwiftPlotDiagram diagram = (SwiftPlotDiagram)Diagram;

			if (diagram == null) return;

			SetGridLinesAxisY(false);

			diagram.AxisY.Label.Font = new Font("Arial", 8);
			diagram.AxisY.Label.TextColor = Color.Black;
			diagram.AxisY.Label.EnableAntialiasing = DefaultBoolean.False;
			diagram.AxisY.Label.Visible = true;

			diagram.AxisY.Title.Text = aText;
			diagram.AxisY.Title.Font = new Font("Arial", 8);
			diagram.AxisY.Title.TextColor = Color.Black;
			diagram.AxisY.Title.EnableAntialiasing = DefaultBoolean.False;
			diagram.AxisY.Title.Alignment = aTextAlign;
			diagram.AxisY.Title.Visibility = DefaultBoolean.True;

			diagram.AxisY.Tickmarks.Length = 5;
			diagram.AxisY.Tickmarks.Thickness = 1;
			diagram.AxisY.Tickmarks.CrossAxis = false;
			diagram.AxisY.Tickmarks.Visible = true;
			diagram.AxisY.Tickmarks.MinorLength = 2;
			diagram.AxisY.Tickmarks.MinorThickness = 1;
			diagram.AxisY.Tickmarks.MinorVisible = true;

			diagram.AxisY.WholeRange.SideMarginsValue = 0;
			diagram.AxisY.WholeRange.SetMinMaxValues(aMin, aMax);
			diagram.AxisY.VisualRange.SideMarginsValue = 0;
			diagram.AxisY.VisualRange.SetMinMaxValues(aMin, aMax);

			diagram.AxisY.Alignment = aAlign;
			diagram.AxisY.Thickness = 1;
			diagram.AxisY.MinorCount = 4;
			diagram.AxisY.Visibility = DefaultBoolean.True;
		}

		public void AddSecondaryAxisX(AxisAlignment aAlign, string aText, StringAlignment aTextAlign, object aMin, object aMax)
		{
			var cAxisX = new SwiftPlotDiagramSecondaryAxisX();

			cAxisX.Label.Font = new Font("Arial", 8);
			cAxisX.Label.TextColor = Color.Black;
			cAxisX.Label.EnableAntialiasing = DefaultBoolean.False;
			cAxisX.Label.Visible = true;

			cAxisX.Title.Text = aText;
			cAxisX.Title.Font = new Font("Arial", 8);
			cAxisX.Title.TextColor = Color.Black;
			cAxisX.Title.EnableAntialiasing = DefaultBoolean.False;
			cAxisX.Title.Alignment = aTextAlign;
			cAxisX.Title.Visibility = DefaultBoolean.True;

			cAxisX.Tickmarks.Length = 5;
			cAxisX.Tickmarks.Thickness = 1;
			cAxisX.Tickmarks.CrossAxis = false;
			cAxisX.Tickmarks.Visible = true;
			cAxisX.Tickmarks.MinorLength = 2;
			cAxisX.Tickmarks.MinorThickness = 1;
			cAxisX.Tickmarks.MinorVisible = true;

			cAxisX.WholeRange.SideMarginsValue = 0;
			cAxisX.WholeRange.SetMinMaxValues(aMin, aMax);
			cAxisX.VisualRange.SideMarginsValue = 0;
			cAxisX.VisualRange.SetMinMaxValues(aMin, aMax);

			cAxisX.Alignment = aAlign;
			cAxisX.Thickness = 1;
			cAxisX.MinorCount = 4;
			cAxisX.Visibility = DefaultBoolean.True;

			((SwiftPlotDiagram)Diagram).SecondaryAxesX.Add(cAxisX);
		}

		public void AddSecondaryAxisY(AxisAlignment aAlign, string aText, StringAlignment aTextAlign, object aMin, object aMax)
		{
			var cAxisY = new SwiftPlotDiagramSecondaryAxisY();

			cAxisY.Label.Font = new Font("Arial", 8);
			cAxisY.Label.TextColor = Color.Black;
			cAxisY.Label.EnableAntialiasing = DefaultBoolean.False;
			cAxisY.Label.Visible = true;

			cAxisY.Title.Text = aText;
			cAxisY.Title.Font = new Font("Arial", 8);
			cAxisY.Title.TextColor = Color.Black;
			cAxisY.Title.EnableAntialiasing = DefaultBoolean.False;
			cAxisY.Title.Alignment = aTextAlign;
			cAxisY.Title.Visibility = DefaultBoolean.True;

			cAxisY.Tickmarks.Length = 5;
			cAxisY.Tickmarks.Thickness = 1;
			cAxisY.Tickmarks.CrossAxis = false;
			cAxisY.Tickmarks.Visible = true;
			cAxisY.Tickmarks.MinorLength = 2;
			cAxisY.Tickmarks.MinorThickness = 1;
			cAxisY.Tickmarks.MinorVisible = true;

			cAxisY.WholeRange.SideMarginsValue = 0;
			cAxisY.WholeRange.SetMinMaxValues(aMin, aMax);
			cAxisY.VisualRange.SideMarginsValue = 0;
			cAxisY.VisualRange.SetMinMaxValues(aMin, aMax);

			cAxisY.Alignment = aAlign;
			cAxisY.Thickness = 1;
			cAxisY.MinorCount = 4;
			cAxisY.Visibility = DefaultBoolean.True;

			((SwiftPlotDiagram)Diagram).SecondaryAxesY.Add(cAxisY);
		}

		public void SetGridLinesAxisX(bool aVisible, DashStyle aStyle = DashStyle.Dot, int aThickness = 1)
		{
			SwiftPlotDiagram diagram = (SwiftPlotDiagram)Diagram;

			diagram.AxisX.GridLines.LineStyle.DashStyle = aStyle;
			diagram.AxisX.GridLines.LineStyle.Thickness = aThickness;
			diagram.AxisX.GridLines.Visible = aVisible;
		}

		public void SetGridLinesAxisY(bool aVisible, DashStyle aStyle = DashStyle.Dot, int aThickness = 1)
		{
			SwiftPlotDiagram diagram = (SwiftPlotDiagram)Diagram;
			
			diagram.AxisY.GridLines.LineStyle.DashStyle = aStyle;
			diagram.AxisY.GridLines.LineStyle.Thickness = aThickness;
			diagram.AxisY.GridLines.Visible = aVisible;
		}

		public void CalcClipRect()
		{
			if (DesignMode == true) return;

			SwiftPlotDiagram diagram = (SwiftPlotDiagram)Diagram;

			if (diagram == null) return;
			if (diagram.AxisX.VisualRange.MinValue == null) return;

			Point pt = diagram.DiagramToPoint((double)diagram.AxisX.VisualRange.MinValue,
				(double)diagram.AxisY.VisualRange.MaxValue, diagram.AxisX, diagram.AxisY).Point;

			clipRect.X = pt.X;
			clipRect.Y = pt.Y;

			pt = diagram.DiagramToPoint((double)diagram.AxisX.VisualRange.MaxValue,
				(double)diagram.AxisY.VisualRange.MinValue, diagram.AxisX, diagram.AxisY).Point;

			clipRect.Width = Math.Abs(pt.X - clipRect.X);
			clipRect.Height = Math.Abs(pt.Y - clipRect.Y);
		}

		protected int GetSeriesVisibleCount()
		{
			int nCount = 0;

			for (int i=0; i<Series.Count; i++)
			{
				if (Series[i].Visible == true)
					nCount++;
			}

			return nCount;
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (DesignMode == true) return;
			if (Diagram == null) return;

			if (vertCursor.IsGrab() == true)
			{
				vertCursor.MouseUp(e);
			}
			else
			{
				zoom.MouseUp(e);
			}

			Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (DesignMode == true) return;
			if (Diagram == null) return;

			vertCursor.MouseDown(e);
			if (vertCursor.IsGrab() == false)
			{
				vertCursor.Reset();
				zoom.MouseDown(e);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (DesignMode == true) return;
			if (Diagram == null) return;

			if (zoom.Enabled == true)
			{
				zoom.MouseMove(e);
			}
			else
			{
				vertCursor.MouseMove(e);

				if (vertCursor.IsGrab() == false)
				{
					desc.MouseMove(e);
				}
			}

			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			if (Diagram == null) return;
			if (DesignMode == true) return;
	
			desc.Reset();
			Invalidate();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			if (Diagram == null) return;
			if (DesignMode == true) return;
			
			CalcClipRect();
			Invalidate();
		}

		protected override void OnCustomPaint(CustomPaintEventArgs e)
		{
			base.OnCustomPaint(e);

			if (Diagram == null) return;
			if (DesignMode == true) return;

			if (zoom.Enabled == true) desc.Reset();

			zoom.Paint(e.Graphics);
			vertCursor.Paint(e.Graphics);
			desc.Paint(e.Graphics);
		}
	}
	
	public class OxChartRangeValue
	{
		private object max;
		public object Max
		{
			get { return max; }
			set { max = value; }
		}

		private object min;
		public object Min
		{
			get { return min; }
			set { min = value; }
		}

		public OxChartRangeValue(object aMin, object aMax)
		{
			min = aMin;
			max = aMax;
		}
	}

	public class OxChartAxisRange
	{
		private OxChartRangeValue visualRange;
		public OxChartRangeValue VisualRange
		{
			get { return visualRange; }
			set { visualRange = value; }
		}

		private OxChartRangeValue wholeRange;
		public OxChartRangeValue WholeRange
		{
			get { return wholeRange; }
			set { wholeRange = value; }
		}

		public OxChartAxisRange()
		{
			wholeRange = new OxChartRangeValue(0, 0);
			visualRange = new OxChartRangeValue(0, 0);
		}

		public OxChartAxisRange(object aVisualMin, object aVisualMax, object aWholeMin, object aWholeMax)
		{
			wholeRange = new OxChartRangeValue(aWholeMin, aWholeMax);
			visualRange = new OxChartRangeValue(aVisualMin, aVisualMax);
		}
	}

	public class OxChartAxisRangeList
	{
		private List<OxChartAxisRange> xList;
		public List<OxChartAxisRange> XList
		{
			get { return xList; }
		}

		private List<OxChartAxisRange> yList;
		public List<OxChartAxisRange> YList
		{
			get { return yList; }
		}

		public OxChartAxisRangeList()
		{
			xList = new List<OxChartAxisRange>();
			yList = new List<OxChartAxisRange>();
		}

		public void Clear()
		{
			xList.Clear();
			yList.Clear();
		}
	}

	public class OxChartBase
	{
		protected OxLineChart chart;
		protected SwiftPlotDiagram diagram;
		protected Rectangle ClipRect
		{ get { return chart.ClipRect; } }

		public OxChartBase(OxLineChart aChart)
		{
			chart = aChart;
			diagram = (SwiftPlotDiagram)chart.Diagram;
		}

		protected Color InvertColor(Color c)
		{
			return Color.FromArgb((byte)~c.R, (byte)~c.G, (byte)~c.B);
		}

		public virtual void CalcClipRect()
		{
			chart.CalcClipRect();
		}

		public virtual void Paint(Graphics g)
		{ 
		}

		public virtual void MouseUp(MouseEventArgs e)
		{
		}

		public virtual void MouseDown(MouseEventArgs e)
		{
		}

		public virtual void MouseMove(MouseEventArgs e)
		{
		}
	}

	public class OxChartCursorPoint
	{
		private bool grab;
		public bool Grab
		{
			get { return grab; }
			set { grab = value; }
		}

		private Point point;
		public int X
		{
			get { return point.X; }
			set { point.X = value; }
		}
		public int Y
		{
			get { return point.Y; }
			set { point.Y = value; }
		}

		private OxChartDescription desc;
		public OxChartDescription Desc
		{
			get { return desc; }
			set { desc = value; }
		}

		public OxChartCursorPoint(int aX, int aY, OxLineChart aChart)
		{
			grab = false;
			point = new Point(aX, aY);
			desc = new OxChartDescription(aChart);
		}

		public bool IsGrabArea(int aX, int aWidth)
		{
			if ((aX < point.X - aWidth) || (aX > point.X + aWidth))
			{
				return false;
			}

			return true;
		}
	}

	public class OxChartCursor : OxChartBase
	{
		private bool visible;
		public bool Visible
		{
			get { return visible; }
			set 
			{ 
				visible = value;
				chart.Invalidate();
			}
		}

		private Pen pen;
		public Pen Pen
		{
			get { return pen; }
			set { pen = value; }
		}

		private Point[] markPoints;

		private Brush markBrush;
		public Brush MarkBrush
		{
			get { return markBrush; }
			set { markBrush = value; }
		}

		private OxChartCursorPoint[] points;
		public OxChartCursorPoint[] Points
		{
			get { return points; }
		}

		private int grabWidth;
		public int GrabWidth
		{
			get { return grabWidth; }
			set { grabWidth = value; }
		}

		private int height;
		public int Height
		{
			get { return height; }
			set { height = value; }
		}

		public OxChartCursor(OxLineChart aChart) : base(aChart)
		{
			visible = false;
			height = 0;
			grabWidth = 3;

			pen = new Pen(new SolidBrush(Color.FromArgb(192, 222, 57, 205)));
			markBrush = new SolidBrush(Color.FromArgb(192, 222, 57, 205));
			markPoints = new Point[4];

			CalcClipRect();

			points = new OxChartCursorPoint[2];
			points[0] = new OxChartCursorPoint(ClipRect.X + ClipRect.Width / 4, ClipRect.Y, chart);
			points[1] = new OxChartCursorPoint(ClipRect.X + ClipRect.Width / 4 * 3, ClipRect.Y, chart);
		}

		public void Reset()
		{
			points[0].Desc.Reset();
			points[1].Desc.Reset();
		}

		public override void MouseMove(MouseEventArgs e)
		{
			if (diagram == null) return;
			if (visible == false) return;

			DiagramCoordinates cCoord1 = diagram.PointToDiagram(e.Location);

			if (IsGrab() == true)
			{
				int nX;

				if (cCoord1.IsEmpty == false)
				{
					nX = e.X;
					chart.Cursor = Cursors.VSplit;
				}
				else
				{
					CalcClipRect();
					nX = (e.X < ClipRect.X) ? ClipRect.X : ClipRect.X + ClipRect.Width;
					chart.Cursor = Cursors.Default;
				}

				MoveTo(nX, e);
			}
			else
			{
				if (cCoord1.IsEmpty == false)
				{
					chart.Cursor = (IsGrabArea(e) == true) ? Cursors.Hand : Cursors.Default;
				}
				else
				{
					if (chart.Cursor == Cursors.Hand)
					{
						chart.Cursor = Cursors.Default;
					}
				}
			}
		}

		public override void MouseDown(MouseEventArgs e)
		{
			if (diagram == null) return;
			if (visible == false) return;

			DiagramCoordinates cCoord1 = diagram.PointToDiagram(e.Location);
			if (cCoord1.IsEmpty == true) return;

			for (int i = 0; i < points.Length; i++)
			{
				if (points[i].IsGrabArea(e.X, grabWidth) == true)
				{
					chart.Cursor = Cursors.VSplit;
					points[i].X = e.X;
					points[i].Grab = true;
					break;
				}
			}
		}

		public override void MouseUp(MouseEventArgs e)
		{
			if (diagram == null) return;
			if (visible == false) return;
			if (IsGrab() == false) return;

			int nX;

			DiagramCoordinates cCoord1 =
				diagram.PointToDiagram(new Point(e.X, ClipRect.Y + ClipRect.Height / 2));

			if (cCoord1.IsEmpty == false)
				nX = e.X;
			else
				nX = (e.X < ClipRect.X) ? ClipRect.X : ClipRect.X + ClipRect.Width;

			for (int i = 0; i < points.Length; i++)
			{
				if (points[i].Grab == true)
				{
					chart.Cursor = Cursors.Default;
					points[i].X = nX;
					points[i].Grab = false;
				}
			}
		}

		public override void Paint(Graphics g)
		{
			if (diagram == null) return;
			if (visible == false) return;

			CalcClipRect();

			markPoints[0] = new Point(points[0].X - 4, points[0].Y - 5);
			markPoints[1] = new Point(points[0].X + 5, points[0].Y - 5);
			markPoints[2] = new Point(points[0].X, points[0].Y);
			markPoints[3] = new Point(points[0].X - 4, points[0].Y - 5);

			if (points[0].Desc.Index == -1)
			{
				g.DrawLine(pen, points[0].X, points[0].Y, points[0].X, points[0].Y + height);
				g.FillPolygon(markBrush, markPoints);
			}
			else
			{
				Brush cBrush = new SolidBrush(points[0].Desc.SeriesColor);
				Pen cPen = new Pen(cBrush);

				g.DrawLine(cPen, points[0].X, points[0].Y, points[0].X, points[0].Y + height);
				g.FillPolygon(cBrush, markPoints);
			}

			markPoints[0] = new Point(points[1].X - 4, points[1].Y - 5);
			markPoints[1] = new Point(points[1].X + 5, points[1].Y - 5);
			markPoints[2] = new Point(points[1].X, points[1].Y);
			markPoints[3] = new Point(points[1].X - 4, points[1].Y - 5);

			if (points[1].Desc.Index == -1)
			{
				g.DrawLine(pen, points[1].X, points[1].Y, points[1].X, points[1].Y + height);
				g.FillPolygon(markBrush, markPoints);
			}
			else
			{
				Brush cBrush = new SolidBrush(points[1].Desc.SeriesColor);
				Pen cPen = new Pen(cBrush);

				g.DrawLine(cPen, points[1].X, points[1].Y, points[1].X, points[1].Y + height);
				g.FillPolygon(cBrush, markPoints);
			}

			points[0].Desc.Paint(g, points[1].Desc);
			points[1].Desc.Paint(g, points[0].Desc);
		}

		public override void CalcClipRect()
		{
			base.CalcClipRect();
			height = ClipRect.Height;
		}

		public bool IsGrab()
		{
			for (int i = 0; i < points.Length; i++)
			{
				if (points[i].Grab == true)
				{
					return true;
				}
			}

			return false;
		}

		private bool IsGrabArea(MouseEventArgs e)
		{
			for (int i = 0; i < points.Length; i++)
			{
				if (points[i].IsGrabArea(e.X, grabWidth) == true)
				{
					return true;
				}
			}

			return false;
		}

		private void MoveTo(int aX, MouseEventArgs e)
		{
			for (int i = 0; i < points.Length; i++)
			{
				if (points[i].Grab == true)
				{
					points[i].X = aX;
					points[i].Desc.MouseMove(e);
				}
			}
		}
	}

	public class OxChartZoom : OxChartBase
	{
		private bool enabled;
		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}

		private Pen pen;
		public Pen Pen
		{
			get { return pen; }
			set { pen = value; }
		}

		private Brush brush;
		public Brush Brush
		{
			get { return brush; }
			set { brush = value; }
		}

		private Rectangle rect;
		public Rectangle Rect
		{
			get { return rect; }
		}

		private Point[] points;
		public Point[] Points
		{
			get { return points; }
		}

		private Stack<OxChartAxisRangeList> axisRangeStack;
		public Stack<OxChartAxisRangeList> AxisRangeStack
		{
			get { return axisRangeStack; }
		}

		public OxChartZoom(OxLineChart aChart) : base(aChart)
		{
			chart = aChart;
			diagram = (SwiftPlotDiagram)chart.Diagram;
			enabled = false;

			brush = new SolidBrush(Color.FromArgb(32, InvertColor(chart.BackColor)));
			pen = new Pen(new SolidBrush(Color.FromArgb(64, InvertColor(chart.BackColor))));

			points = new Point[2];
			points[0] = new Point(0, 0);
			points[1] = new Point(0, 0);

			axisRangeStack = new Stack<OxChartAxisRangeList>();

			CalcClipRect();
		}

		public override void MouseMove(MouseEventArgs e)
		{
			if (diagram == null) return;
			if (enabled == false) return;
			if (e.Button != MouseButtons.Left) return;

			if (IsValidPoint(e.Location) == true)
			{
				points[1] = e.Location;
			}
			else
			{
				CalcClipRect();

				if (IsValidPoint(new Point(e.X, ClipRect.Y + ClipRect.Height / 2)) == true)
					points[1].X = e.X;
				else
					points[1].X = (e.X >= ClipRect.Right) ? ClipRect.Right : ClipRect.Left;

				if (IsValidPoint(new Point(ClipRect.X + ClipRect.Width / 2, e.Y)) == true)
					points[1].Y = e.Y;
				else
					points[1].Y = (e.Y >= ClipRect.Bottom) ? ClipRect.Bottom : ClipRect.Top;
			}
		}

		public override void MouseDown(MouseEventArgs e)
		{
			if (diagram == null) return;

			DiagramCoordinates cCoord1 = diagram.PointToDiagram(e.Location);

			if (cCoord1.IsEmpty == false)
			{
				enabled = true;
				points[0] = e.Location;
				points[1] = e.Location;
			}
		}

		public override void MouseUp(MouseEventArgs e)
		{
			if (diagram == null) return;
			if (enabled == false) return;

			DiagramCoordinates cCoord1 = diagram.PointToDiagram(e.Location);

			if (cCoord1.IsEmpty == false)
			{
				points[1] = e.Location;
			}
			else
			{
				CalcClipRect();

				if (ClipRect.Contains(e.X, ClipRect.Y) == true)
					points[1].X = e.X;
				else
					points[1].X = (e.X > ClipRect.Right) ? ClipRect.Right : ClipRect.Left;

				if (ClipRect.Contains(ClipRect.X, e.Y) == true)
					points[1].Y = e.Y;
				else
					points[1].Y = (e.Y > ClipRect.Bottom) ? ClipRect.Bottom : ClipRect.Top;
			}

			Execute();
			enabled = false;
		}

		public override void Paint(Graphics g)
		{
			if (diagram == null) return;
			if (enabled == false) return;

			CalcRect();
			g.FillRectangle(brush, rect);
			g.DrawRectangle(pen, rect);
		}

		private bool IsValidPoint(Point pt)
		{
			if (diagram == null) return false;
			
			return !diagram.PointToDiagram(pt).IsEmpty;
		}

		private void Execute()
		{
			CalcRect();
			if ((rect.Width < 8) || (rect.Height < 8)) return;

			Push();

			DiagramCoordinates cCoord1 = diagram.PointToDiagram(new Point(rect.X, rect.Y));
			DiagramCoordinates cCoord2 = diagram.PointToDiagram(new Point(rect.X + rect.Width, rect.Y + rect.Height));

			diagram.AxisX.VisualRange.MinValue = (object)cCoord1.NumericalArgument;
			diagram.AxisX.VisualRange.MaxValue = (object)cCoord2.NumericalArgument;

			diagram.AxisY.VisualRange.MinValue = (object)cCoord2.NumericalValue;
			diagram.AxisY.VisualRange.MaxValue = (object)cCoord1.NumericalValue;

			SetSeriesMarkers();
			SetAllAxesLabelTextPattern();
		}

		private void SetSeriesMarkers()
		{
			//DefaultBoolean bMarker = ((SwiftPlotSeriesView)chart.Series[0].View).MarkerVisibility;

			//Point pt1 = diagram.DiagramToPoint(chart.Series[0].Points[0].NumericalArgument,
			//	chart.Series[0].Points[0].Values[0], diagram.AxisX, diagram.AxisY).Point;

			//Point pt2 = diagram.DiagramToPoint(chart.Series[0].Points[1].NumericalArgument,
			//	chart.Series[0].Points[1].Values[0], diagram.AxisX, diagram.AxisY).Point;

			//DefaultBoolean bState = (Math.Abs(pt1.X - pt2.X) > 4) ? DefaultBoolean.True : DefaultBoolean.False;

			//for (int i = 0; i < chart.Series.Count; i++)
			//{
			//	((SwiftPlotSeriesView)chart.Series[i].View).MarkerVisibility = bState;
			//}
		}

		private void SetAllAxesLabelTextPattern()
		{
			List<Axis2D> cXList = diagram.GetAllAxesX();
			for (int i = 0; i < cXList.Count; i++)
			{
				SetAxisLabelTextPattern(0, cXList[i]);
			}

			List<Axis2D> cYList = diagram.GetAllAxesY();
			for (int i = 0; i < cYList.Count; i++)
			{
				SetAxisLabelTextPattern(1, cYList[i]);
			}
		}

		private void SetAxisLabelTextPattern(int aIndex, Axis2D aAxis)
		{
			string sPattern = (aIndex == 0) ? "{A:n" : "{V:n";

			for (int i = -4; i <= 0; i++)
			{
				if (aAxis.NumericScaleOptions.GridSpacing < Math.Pow(10, i))
				{
					aAxis.Label.TextPattern = sPattern + Math.Abs(i - 1).ToString() + "}";
					return;
				}
			}

			aAxis.Label.TextPattern = sPattern + "0}";
		}

		public void Reset()
		{
			while (axisRangeStack.Count > 1)
			{
				axisRangeStack.Pop();
			}

			Pop();
		}

		public void Out()
		{
			Pop();
		}

		public void AutoSet()
		{
			Push();
			AutoSetAxisX();
			AutoSetAxisY();
			SetSeriesMarkers();
		}

		public void Stack()
		{
			Push();

			List<Axis2D> cYList = diagram.GetAllAxesY();
			double fEnlageRatio = cYList.Count;

			for (int i = 0; i < cYList.Count; i++)
			{
				double fMin = double.MaxValue;
				double fMax = double.MinValue;
				GetYAxisSeriesPointMinMax(cYList[i], ref fMin, ref fMax);

				double fDiff = fMax - fMin;
				fMin -= i * fDiff;
				fMax += (cYList.Count - 1 - i) * fDiff;

				cYList[i].WholeRange.MinValue = fMin;
				cYList[i].WholeRange.MaxValue = fMax;
				cYList[i].VisualRange.MinValue = fMin;
				cYList[i].VisualRange.MaxValue = fMax;
			}
		}

		private void Push()
		{
			OxChartAxisRangeList cRangeList = new OxChartAxisRangeList();

			List<Axis2D> cXList = diagram.GetAllAxesX();
			for (int i = 0; i < cXList.Count; i++)
			{
				OxChartAxisRange cXRange = new OxChartAxisRange();

				cXRange.VisualRange.Min = cXList[i].VisualRange.MinValue;
				cXRange.VisualRange.Max = cXList[i].VisualRange.MaxValue;
				cXRange.WholeRange.Min = cXList[i].WholeRange.MinValue;
				cXRange.WholeRange.Max = cXList[i].WholeRange.MaxValue;
				cRangeList.XList.Add(cXRange);
			}

			List<Axis2D> cYList = diagram.GetAllAxesY();
			for (int i = 0; i < cYList.Count; i++)
			{
				OxChartAxisRange cYRange = new OxChartAxisRange();

				cYRange.VisualRange.Min = cYList[i].VisualRange.MinValue;
				cYRange.VisualRange.Max = cYList[i].VisualRange.MaxValue;
				cYRange.WholeRange.Min = cYList[i].WholeRange.MinValue;
				cYRange.WholeRange.Max = cYList[i].WholeRange.MaxValue;
				cRangeList.YList.Add(cYRange);
			}

			axisRangeStack.Push(cRangeList);
		}

		private void Pop()
		{
			if (axisRangeStack.Count == 0) return;

			OxChartAxisRangeList cList = axisRangeStack.Pop();

			List<Axis2D> cXList = diagram.GetAllAxesX();
			for (int i = 0; i < cList.XList.Count; i++)
			{
				if (i < cXList.Count)
				{
					cXList[i].WholeRange.MinValue = cList.XList[i].WholeRange.Min;
					cXList[i].WholeRange.MaxValue = cList.XList[i].WholeRange.Max;
					cXList[i].VisualRange.MinValue = cList.XList[i].VisualRange.Min;
					cXList[i].VisualRange.MaxValue = cList.XList[i].VisualRange.Max;

					SetAxisLabelTextPattern(0, cXList[i]);
				}
			}

			List<Axis2D> cYList = diagram.GetAllAxesY();
			for (int i = 0; i < cList.YList.Count; i++)
			{
				if (i < cYList.Count)
				{
					cYList[i].WholeRange.MinValue = cList.YList[i].WholeRange.Min;
					cYList[i].WholeRange.MaxValue = cList.YList[i].WholeRange.Max;
					cYList[i].VisualRange.MinValue = cList.YList[i].VisualRange.Min;
					cYList[i].VisualRange.MaxValue = cList.YList[i].VisualRange.Max;

					SetAxisLabelTextPattern(1, cYList[i]);
				}
			}

			SetSeriesMarkers();
		}

		private void AutoSetAxisX()
		{
			List<Axis2D> cXList = diagram.GetAllAxesX();

			for (int i = 0; i < cXList.Count; i++)
			{
				double fMin = double.MaxValue;
				double fMax = double.MinValue;

				GetXAxisSeriesPointMinMax(cXList[i], ref fMin, ref fMax);

				cXList[i].WholeRange.MinValue = fMin;
				cXList[i].WholeRange.MaxValue = fMax;
				cXList[i].VisualRange.MinValue = fMin;
				cXList[i].VisualRange.MaxValue = fMax;

				SetAxisLabelTextPattern(0, cXList[i]);
			}
		}

		private void AutoSetAxisY()
		{
			List<Axis2D> cYList = diagram.GetAllAxesY();

			for (int i = 0; i < cYList.Count; i++)
			{
				double fMin = double.MaxValue;
				double fMax = double.MinValue;

				GetYAxisSeriesPointMinMax(cYList[i], ref fMin, ref fMax);

				cYList[i].WholeRange.MinValue = fMin;
				cYList[i].WholeRange.MaxValue = fMax;
				cYList[i].VisualRange.MinValue = fMin;
				cYList[i].VisualRange.MaxValue = fMax;

				SetAxisLabelTextPattern(1, cYList[i]);
			}
		}

		private void GetXAxisSeriesPointMinMax(Axis2D aAxis, ref double aMin, ref double aMax)
		{
			for (int i = 0; i < chart.Series.Count; i++)
			{
				SwiftPlotSeriesView cView = (SwiftPlotSeriesView)chart.Series[i].View;

				if (cView.AxisX == aAxis)
				{
					SeriesPointCollection cPointList = chart.Series[i].Points;

					for (int j = 0; j < cPointList.Count; j++)
					{
						if (cPointList[j].NumericalArgument < aMin)
						{
							aMin = cPointList[j].NumericalArgument;
						}
						else if (cPointList[j].NumericalArgument > aMax)
						{
							aMax = cPointList[j].NumericalArgument;
						}
					}
				}
			}
		}

		private void GetYAxisSeriesPointMinMax(Axis2D aAxis, ref double aMin, ref double aMax)
		{
			for (int i = 0; i < chart.Series.Count; i++)
			{
				SwiftPlotSeriesView cView = (SwiftPlotSeriesView)chart.Series[i].View;

				if (cView.AxisY == aAxis)
				{
					SeriesPointCollection cPointList = chart.Series[i].Points;

					for (int j = 0; j < cPointList.Count; j++)
					{
						if (cPointList[j].Values[0] < aMin)
						{
							aMin = cPointList[j].Values[0];
						}
						else if (cPointList[j].Values[0] > aMax)
						{
							aMax = cPointList[j].Values[0];
						}
					}
				}
			}
		}

		private void CalcRect()
		{
			rect.X = (points[0].X < points[1].X) ? points[0].X : points[1].X;
			rect.Y = (points[0].Y < points[1].Y) ? points[0].Y : points[1].Y;

			rect.Width = Math.Abs(points[1].X - points[0].X);
			rect.Height = Math.Abs(points[1].Y - points[0].Y);
		}
	}

	public class OxChartDescription : OxChartBase
	{
		private bool visible;

		public bool Visible
		{
			get { return visible; }
			set { visible = value; }
		}
		private bool enabled;
		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}

		private bool pointLock;
		public bool PointLock
		{
			get { return pointLock; }
			set { pointLock = value; }
		}

		private int index;
		public int Index
		{
			get { return index; }
		}

		private Series series;
		public Series Series
		{
			get { return series; }
		}

		public Color SeriesColor
		{
			get { return (series.View.Color); }
		}

		private Font balloonFont;
		public Font BalloonFont
		{
			get { return balloonFont; }
			set { balloonFont = value; }
		}

		private string balloonXFormat;
		public string BalloonXFormat
		{
			get { return balloonXFormat; }
			set { balloonXFormat = value; }
		}

		private string balloonYFormat;
		public string BalloonYFormat
		{
			get { return balloonYFormat; }
			set { balloonYFormat = value; }
		}

		private string balloondXFormat;
		public string BalloondXFormat
		{
			get { return balloondXFormat; }
			set { balloondXFormat = value; }
		}

		private string balloondYFormat;
		public string BalloondYFormat
		{
			get { return balloondYFormat; }
			set { balloondYFormat = value; }
		}

		private Rectangle rect;

		private int grabWidth;
		public int GrabWidth
		{
			get { return grabWidth; }
			set { grabWidth = value; }
		}

		private int grabHeight;
		public int GrabHeight
		{
			get { return grabHeight; }
			set { grabHeight = value; }
		}

		public OxChartDescription(OxLineChart aChart) : base(aChart)
		{
			enabled = true;
			pointLock = false;
			index = -1;
			grabWidth = 3;
			grabHeight = 3;

			balloonFont = new Font("Arial", 8);
			balloonXFormat = "X : {0:0.00}";
			balloonYFormat = "Y : {0:0.00}";
			balloondXFormat = "dX : {0:0.00}";
			balloondYFormat = "dY : {0:0.00}";

			CalcClipRect();
		}

		public override void MouseMove(MouseEventArgs e)
		{
			if (diagram == null) return;

			DiagramCoordinates cCoord1 =
				((SwiftPlotDiagram)chart.Diagram).PointToDiagram(e.Location);

			if (pointLock == true) return;
			if (cCoord1.IsEmpty == true) return;

			series = null;
			int nDist = int.MaxValue;
			List<int> cXList = new List<int>();

			GetIndexListOfAxisX(e.X, cXList);

			if (cXList.Count > 0)
			{
				for (int i = 0; i < chart.Series.Count; i++)
				{
					Series theSeries = chart.Series[i];

					if (theSeries.Visible == true)
					{
						for (int j = 0; j < cXList.Count; j++)
						{
							Point pt = ((SwiftPlotDiagram)chart.Diagram).DiagramToPoint(
								theSeries.Points[cXList[j]].NumericalArgument,
								theSeries.Points[cXList[j]].Values[0],
								((SwiftPlotSeriesView)theSeries.View).AxisX,
								((SwiftPlotSeriesView)theSeries.View).AxisY).Point;

							if ((e.Y >= (pt.Y - grabHeight)) && (e.Y <= (pt.Y + grabHeight)))
							{
								if (Math.Abs(pt.Y - grabHeight) < nDist)
								{
									index = cXList[j];
									series = theSeries;
									nDist = Math.Abs(pt.Y - grabHeight);
								}
							}
						}
					}
				}
			}

			if (series == null)
			{
				index = -1;
			}
		}

		private void GetIndexListOfAxisX(int aX, List<int> aList)
		{
			aList.Clear();

			for (int i = 0; i < chart.Series.Count; i++)
			{
				Series theSeries = chart.Series[i];

				if (theSeries.Visible == true)
				{
					for (int j = 0; j < theSeries.Points.Count; j++)
					{
						Point pt = ((SwiftPlotDiagram)chart.Diagram).DiagramToPoint(
							theSeries.Points[j].NumericalArgument,
							theSeries.Points[j].Values[0],
							((SwiftPlotSeriesView)theSeries.View).AxisX,
							((SwiftPlotSeriesView)theSeries.View).AxisY).Point;

						if ((aX >= (pt.X - grabWidth)) && (aX <= (pt.X + grabWidth)))
						{
							aList.Add(j);
						}
						else
						{
							if (aList.Count > 0)
							{
								return;
							}
						}
					}
				}
			}
		}

		public void Reset()
		{
			index = -1;
		}

		public override void Paint(Graphics g)
		{
			Paint(g, null);
		}

		public void Paint(Graphics g, OxChartDescription aDesc)
		{
			if (diagram == null) return;
			if (enabled == false) return;
			if (index < 0) return;

			Brush cBalloonBrush = new SolidBrush(Color.FromArgb(64, InvertColor(chart.BackColor)));
			Pen cBalloonPen = new Pen(new SolidBrush(Color.FromArgb(64, SeriesColor)));
			Brush cBalloonFontBrush = new SolidBrush(SeriesColor);
			Pen cMarkPen = new Pen(new SolidBrush(SeriesColor));

			Point pt = ((SwiftPlotDiagram)chart.Diagram).DiagramToPoint(
				series.Points[index].NumericalArgument,
				series.Points[index].Values[0],
				((SwiftPlotSeriesView)series.View).AxisX,
				((SwiftPlotSeriesView)series.View).AxisY).Point;

			string sName = series.Name;
			SizeF theNameSize = g.MeasureString(sName, balloonFont);

			double fXValue1, fXValue2, fYValue1, fYValue2;

			fXValue1 = series.Points[index].NumericalArgument;
			SizeF theXValueSize1 = g.MeasureString(string.Format(balloonXFormat, fXValue1), balloonFont);

			fYValue1 = series.Points[index].Values[0];
			SizeF theYValueSize1 = g.MeasureString(string.Format(balloonYFormat, fYValue1), balloonFont);

			fXValue2 = 0;
			fYValue2 = 0;

			SizeF theXValueSize2 = new SizeF(0, 0);
			SizeF theYValueSize2 = new SizeF(0, 0);


			if ((aDesc != null) && (aDesc.Index != -1))
			{
				fXValue2 = fXValue1 - aDesc.Series.Points[aDesc.Index].NumericalArgument;
				theXValueSize2 = g.MeasureString(string.Format(balloondXFormat, fXValue2), balloonFont);

				fYValue2 = fYValue1 - aDesc.Series.Points[aDesc.Index].Values[0];
				theYValueSize2 = g.MeasureString(string.Format(balloondYFormat, fYValue2), balloonFont);
			}

			int nX = pt.X - grabWidth;
			int nY = pt.Y - grabHeight;

			if ((nX >= 0) && (nY >= 0))
			{
				rect = new Rectangle(nX, nY, grabWidth * 2, grabHeight * 2);
				g.DrawRectangle(cMarkPen, rect);
			}

			int nWidth = Math.Max((int)theNameSize.Width, (int)theXValueSize1.Width);
			nWidth = Math.Max(nWidth, (int)theYValueSize1.Width);
			nWidth = Math.Max(nWidth, (int)theXValueSize2.Width);
			nWidth = Math.Max(nWidth, (int)theYValueSize2.Width) + 16;

			int nHeight = ((aDesc != null) && (aDesc.Index != -1)) ? (int)theNameSize.Height * 5 + 32 : (int)theNameSize.Height * 3 + 24;

			if (ClipRect.IsEmpty == true)
			{
				nX = pt.X + grabWidth;
				nY = pt.Y + grabHeight;
			}
			else
			{
				nX = (pt.X < ((ClipRect.X + ClipRect.Width) / 2)) ? (pt.X + grabWidth + 1) : (pt.X - grabWidth - nWidth - 1);
				nY = (pt.Y < ((ClipRect.Y + ClipRect.Height) / 2)) ? (pt.Y + grabHeight + 1) : (pt.Y - grabHeight - nHeight - 1);
			}


			if ((nX >= 0) && (nY >= 0))
			{
				float fHeight = theNameSize.Height + 4;
				rect = new Rectangle(nX, nY, nWidth, nHeight);

				g.FillRectangle(cBalloonBrush, rect);
				g.DrawRectangle(cBalloonPen, rect);

				g.DrawString(sName, balloonFont, cBalloonFontBrush, nX + 8, nY + 8);
				g.DrawString(string.Format(balloonXFormat, fXValue1), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight);
				g.DrawString(string.Format(balloonYFormat, fYValue1), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight * 2);

				if ((aDesc != null) && (aDesc.Index != -1))
				{
					g.DrawString(string.Format(balloondXFormat, fXValue2), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight * 3);
					g.DrawString(string.Format(balloondYFormat, fYValue2), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight * 4);
				}
			}
		}
	}
}
