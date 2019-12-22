//------------------------------------------------------------------------------
// Copyright (C) 2015 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : Line Chart Control
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraCharts;

namespace Ulee.Chart
{
    public partial class UlLineChart : ChartControl
    {
        public UlDoubleBufferedLineChart BufferedChart { get; set; }
            
        private UlChartZoom zoom;
		public UlChartZoom Zooming
		{ get { return zoom; } }

        private UlChartCursor vertCursor;
        public UlChartCursor VertCursor
        { get { return vertCursor; } }

        private UlChartMarkLine markLine;
        public UlChartMarkLine MarkLine
        { get { return markLine; } }

        private UlChartGuideLine guideLine;
        public UlChartGuideLine GuideLine
        { get { return guideLine; } }

        private UlChartDescription desc;
		public UlChartDescription Description
		{ get { return desc; } }

		private Rectangle clipRect;
		public Rectangle ClipRect
		{ get { return clipRect; } }

        public List<Axis2D> AxesX
        {
            get
            {
                SwiftPlotDiagram diagram = (SwiftPlotDiagram)Diagram;

                return diagram.GetAllAxesX();
            }
        }

        public List<Axis2D> AxesY
        {
            get
            {
                SwiftPlotDiagram diagram = (SwiftPlotDiagram)Diagram;

                return diagram.GetAllAxesY();
            }
        }

        public Font LegendFont { get; set; }

        public int SeriesVisibleCount
		{ get { return GetSeriesVisibleCount(); } }

        private volatile bool visualRangeChanging;
        public bool VisualRangeChanging
        {
            get { return visualRangeChanging; }
            set { visualRangeChanging = value; }
        }

		public UlLineChart()
		{
			Initialize();
			zoom = null;
			vertCursor = null;
            markLine = null;
            guideLine = null;
			desc = null;
			clipRect = new Rectangle();
            LegendFont = new Font("Arial", 8, FontStyle.Regular);
        }

        private void Initialize()
		{
            BufferedChart = null;

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

		public virtual void Prepare()
		{
            CalcClipRect();

			zoom = new UlChartZoom(this);
            vertCursor = new UlChartCursor(this);
            markLine = new UlChartMarkLine(this);
            guideLine = new UlChartGuideLine(this);
            desc = new UlChartDescription(this);

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

		public virtual void Clear()
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

        public void SetSeriesAxisX(int aSeriesIndex, int aAxisIndex)
        {
            if (aAxisIndex == 0)
            {
                ((SwiftPlotSeriesView)Series[aSeriesIndex].View).AxisX =
                    ((SwiftPlotDiagram)Diagram).AxisX;
            }
            else
            {
                ((SwiftPlotSeriesView)Series[aSeriesIndex].View).AxisX = 
                    ((SwiftPlotDiagram)Diagram).SecondaryAxesX[aAxisIndex - 1];
            }
        }

        public void SetSeriesAxisX(string seriesName, int aAxisIndex)
        {
            if (aAxisIndex == 0)
            {
                ((SwiftPlotSeriesView)Series[seriesName].View).AxisX =
                    ((SwiftPlotDiagram)Diagram).AxisX;
            }
            else
            {
                ((SwiftPlotSeriesView)Series[seriesName].View).AxisX =
                    ((SwiftPlotDiagram)Diagram).SecondaryAxesX[aAxisIndex - 1];
            }
        }

        public void SetSeriesAxisX(string seriesName, string axisName)
        {
            try
            {
                ((SwiftPlotSeriesView)Series[seriesName].View).AxisX =
                    ((SwiftPlotDiagram)Diagram).SecondaryAxesX[axisName];
            }
            catch
            {
                ((SwiftPlotSeriesView)Series[seriesName].View).AxisX =
                    ((SwiftPlotDiagram)Diagram).AxisX;
            }
        }

        public void SetSeriesAxisY(int aSeriesIndex, int aAxisIndex)
		{
            if (aAxisIndex == 0)
            {
                ((SwiftPlotSeriesView)Series[aSeriesIndex].View).AxisY =
                    ((SwiftPlotDiagram)Diagram).AxisY;
            }
            else
            {
                ((SwiftPlotSeriesView)Series[aSeriesIndex].View).AxisY =
                    ((SwiftPlotDiagram)Diagram).SecondaryAxesY[aAxisIndex - 1];
            }
		}

        public void SetSeriesAxisY(string seriesName, int aAxisIndex)
        {
            if (aAxisIndex == 0)
            {
                ((SwiftPlotSeriesView)Series[seriesName].View).AxisY =
                    ((SwiftPlotDiagram)Diagram).AxisY;
            }
            else
            {
                ((SwiftPlotSeriesView)Series[seriesName].View).AxisY =
                    ((SwiftPlotDiagram)Diagram).SecondaryAxesY[aAxisIndex - 1];
            }
        }

        public void SetSeriesAxisY(string seriesName, string axisName)
        {
            try
            {
                ((SwiftPlotSeriesView)Series[seriesName].View).AxisY =
                    ((SwiftPlotDiagram)Diagram).SecondaryAxesY[axisName];
            }
            catch
            {
                ((SwiftPlotSeriesView)Series[seriesName].View).AxisY =
                    ((SwiftPlotDiagram)Diagram).AxisY;
            } 
        }

        public void SetPrimaryAxisX(AxisAlignment aAlign, string aText, StringAlignment aTextAlign, object aMin, object aMax)
		{
			SwiftPlotDiagram diagram = (SwiftPlotDiagram)Diagram;

			if (diagram == null) return;

			SetGridLinesAxisX(false);

			diagram.AxisX.Label.Font = LegendFont;
			diagram.AxisX.Label.TextColor = Color.Black;
			diagram.AxisX.Label.EnableAntialiasing = DefaultBoolean.False;
			diagram.AxisX.Label.Visible = true;

			diagram.AxisX.Title.Text = aText;
			diagram.AxisX.Title.Font = LegendFont;
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

            diagram.AxisY.Label.Font = LegendFont;
			diagram.AxisY.Label.TextColor = Color.Black;
			diagram.AxisY.Label.EnableAntialiasing = DefaultBoolean.False;
			diagram.AxisY.Label.Visible = true;

            diagram.AxisY.Title.Text = aText;
			diagram.AxisY.Title.Font = LegendFont;
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

			cAxisX.Label.Font = LegendFont;
			cAxisX.Label.TextColor = Color.Black;
			cAxisX.Label.EnableAntialiasing = DefaultBoolean.False;
			cAxisX.Label.Visible = true;

			cAxisX.Title.Text = aText;
			cAxisX.Title.Font = LegendFont;
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

			cAxisY.Label.Font = LegendFont;
			cAxisY.Label.TextColor = Color.Black;
			cAxisY.Label.EnableAntialiasing = DefaultBoolean.False;
			cAxisY.Label.Visible = true;

			cAxisY.Title.Text = aText;
			cAxisY.Title.Font = LegendFont;
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
            if (diagram.AxisX == null) return;

            Point pt = diagram.DiagramToPoint((double)diagram.AxisX.VisualRange.MinValueInternal,
                (double)diagram.AxisY.VisualRange.MaxValueInternal, diagram.AxisX, diagram.AxisY).Point;

            clipRect.X = pt.X;
            clipRect.Y = pt.Y;

            pt = diagram.DiagramToPoint((double)diagram.AxisX.VisualRange.MaxValueInternal,
                (double)diagram.AxisY.VisualRange.MinValueInternal, diagram.AxisX, diagram.AxisY).Point;

            clipRect.Width = Math.Abs(pt.X - clipRect.X);
            clipRect.Height = Math.Abs(pt.Y - clipRect.Y);
        }

        protected int GetSeriesVisibleCount()
		{
			int nCount = 0;

			for (int i=0; i<Series.Count; i++)
			{
				if (Series[i].Visible == true) nCount++;
			}

			return nCount;
		}

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (DesignMode == true) return;
            if (Diagram == null) return;
            if (e.Button != MouseButtons.Left) return;

            vertCursor.MouseDown(e);

            if (vertCursor.IsGrab() == false)
            {
                vertCursor.Reset();
                markLine.MouseDown(e);

                if (markLine.IsGrab() == false)
                {
                    desc.PointLock = true;
                    zoom.MouseDown(e);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (DesignMode == true) return;
			if (Diagram == null) return;
            if (e.Button != MouseButtons.Left) return;

            if (vertCursor.IsGrab() == true)
            {
                vertCursor.MouseUp(e);
            }
            else if (markLine.IsGrab() == true)
            {
                markLine.MouseUp(e);
            }
            else
            {
                zoom.MouseUp(e);
                desc.PointLock = false;
            }

			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (DesignMode == true) return;
			if (Diagram == null) return;

            if (zoom.PointEnabled == true)
            {
                zoom.MouseMove(e);
            }
            else
            {
                markLine.MouseMove(e);
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

			Invalidate();
		}

        protected override void OnCustomPaint(CustomPaintEventArgs e)
		{
            DXCustomPaintEventArgs args = e as DXCustomPaintEventArgs;

            if (args == null) return;
            if (Diagram == null) return;
            if (DesignMode == true) return;

            CalcClipRect();
            base.OnCustomPaint(e);

            markLine.Paint(args.Cache);

            if (zoom.PointEnabled == true)
            {
                desc.Reset();
                zoom.Paint(args.Cache);
            }
            else
            {
                vertCursor.Paint(args.Cache);
                guideLine.Paint(args.Cache);
                desc.Paint(args.Cache);
            }
		}
	}
	
	public class UlChartRangeValue
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

		public UlChartRangeValue(object aMin, object aMax)
		{
			min = aMin;
			max = aMax;
		}
	}

	public class UlChartAxisRange
	{
        public bool Enabled { get; set; }

		private UlChartRangeValue visualRange;
		public UlChartRangeValue VisualRange
		{
			get { return visualRange; }
			set { visualRange = value; }
		}

		private UlChartRangeValue wholeRange;
		public UlChartRangeValue WholeRange
		{
			get { return wholeRange; }
			set { wholeRange = value; }
		}

		public UlChartAxisRange()
		{
            Enabled = true;
			wholeRange = new UlChartRangeValue(0, 0);
			visualRange = new UlChartRangeValue(0, 0);
		}

		public UlChartAxisRange(object aVisualMin, object aVisualMax, object aWholeMin, object aWholeMax)
		{
            Enabled = true;
            wholeRange = new UlChartRangeValue(aWholeMin, aWholeMax);
			visualRange = new UlChartRangeValue(aVisualMin, aVisualMax);
		}
	}

	public class UlChartAxisRangeList
	{
		private List<UlChartAxisRange> xList;
		public List<UlChartAxisRange> XList
		{
			get { return xList; }
		}

		private List<UlChartAxisRange> yList;
		public List<UlChartAxisRange> YList
		{
			get { return yList; }
		}

		public UlChartAxisRangeList()
		{
			xList = new List<UlChartAxisRange>();
			yList = new List<UlChartAxisRange>();
		}

		public void Clear()
		{
			xList.Clear();
			yList.Clear();
		}
	}

	public class UlChartBase
	{
		protected UlLineChart chart;
		protected SwiftPlotDiagram diagram;
		protected Rectangle ClipRect
		{ get { return chart.ClipRect; } }

		public UlChartBase(UlLineChart aChart)
		{
			chart = aChart;
			diagram = (SwiftPlotDiagram)chart.Diagram;
		}

		public Color InvertColor(Color c)
		{
			return Color.FromArgb((byte)~c.R, (byte)~c.G, (byte)~c.B);
		}

		public virtual void CalcClipRect()
		{
			chart.CalcClipRect();
		}

		public virtual void Paint(GraphicsCache g)
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

	public class UlChartCursorPoint
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

		private UlChartDescription desc;
		public UlChartDescription Desc
		{
			get { return desc; }
			set { desc = value; }
		}

		public UlChartCursorPoint(int aX, int aY, UlLineChart aChart)
		{
			grab = false;
			point = new Point(aX, aY);
			desc = new UlChartDescription(aChart);
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

    public class UlChartVertLine
    {
        private bool grab;
        public bool Grab
        {
            get { return grab; }
            set { grab = value; }
        }

        private Pen pen;
        public Pen Pen
        {
            get { return pen; }
        }

        public SolidBrush Brush { get; }

        public SolidBrush TextBrush { get; }

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

        private double value;
        public double Value
        {
            get { return this.value; }
            set
            {
                prevValue = this.value;
                this.value = value;
            }
        }

        private double prevValue;
        public double PrevValue
        {
            get { return prevValue; }
            set { prevValue = value; }
        }

        public bool IsChanged
        {
            get
            {
                if (prevValue != value) return true;
                if (prevVisible != visible) return true;

                return false;
            }
        }

        public string Text { get; set; }

        private Color lineColor;
        public Color LineColor
        {
            get { return lineColor; }
            set
            {
                lineColor = value;
                pen = new Pen(new SolidBrush(lineColor));
            }
        }

        private bool visible;
        public bool Visible
        {
            get { return visible; }
            set
            {
                prevVisible = visible;
                visible = value;
            }
        }

        private bool prevVisible;
        public bool PrevVisible
        {
            get { return prevVisible; }
            set { prevVisible = value; }
        }

        public UlChartVertLine(double value, Color lineColor, bool visible=true)
        {
            this.grab = false;
            this.value = value;
            this.lineColor = lineColor;
            this.visible = visible;
            this.Brush = new SolidBrush(lineColor);
            this.TextBrush = new SolidBrush(Color.White);
            this.pen = new Pen(this.Brush, 1);
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

    public class UlChartGuideLine : UlChartBase
    {
        private List<UlChartVertLine> lines;
        public List<UlChartVertLine> Lines
        {
            get { return lines; }
        }

        public UlChartGuideLine(UlLineChart aChart) : base(aChart)
        {
            lines = new List<UlChartVertLine>();
        }

        public void Clear()
        {
            lines.Clear();
        }

        public override void Paint(GraphicsCache g)
        {
            if (diagram == null) return;

            CalcLines();
            foreach (UlChartVertLine line in lines)
            {
                if (line.Visible == true)
                {
                    if ((line.X >= ClipRect.X) && (line.X < ClipRect.X + ClipRect.Width))
                    {
                        g.DrawLine(line.X, line.Y, line.X, line.Y + ClipRect.Height, line.Pen.Color, (int)line.Pen.Width);
                    }
                }
            }
        }

        private void CalcLines()
        {
            //base.CalcClipRect();

            foreach (UlChartVertLine line in lines)
            {
                line.X = diagram.DiagramToPoint(line.Value, 0.0, diagram.AxisX, diagram.AxisY).Point.X;
                line.Y = ClipRect.Y;
            }
        }
    }

    public class ShowValueMarkPointEventArgs : EventArgs
    {
        public ShowValueMarkPointEventArgs()
        {
            ValueA = -1;
            ValueB = -1;
            Visible = false;
        }

        public bool Visible { get; set; }
        public float ValueA { get; set; }
        public float ValueB { get; set; }
    }

    public delegate void ShowValueMarkPointEventHandler(object sender, ShowValueMarkPointEventArgs e);

    public class UlChartMarkLine : UlChartBase
    {
        private bool visible;
        public bool Visible
        {
            get { return visible; }
            set
            {
                visible = value;

                if (visible == true)
                {
                    ResetLines();
                }
                else
                {
                    lines[0].Value = -1;
                    lines[1].Value = -1;
                }

                OnShowValueMarkPoint();
                chart.Invalidate();
            }
        }

        public Pen Pen { get; set; }

        public int GrabWidth { get; set; }

        public Brush MarkBrush { get; set; }

        public Color MarkColor { get; set; }

        private List<UlChartVertLine> lines;

        private ShowValueMarkPointEventArgs showValueArgs;

        public event ShowValueMarkPointEventHandler ShowValueMarkPoint = null;
        protected void OnShowValueMarkPoint()
        {
            showValueArgs.Visible = visible;
            showValueArgs.ValueA = (float)lines[0].Value;
            showValueArgs.ValueB = (float)lines[1].Value;

            ShowValueMarkPoint?.Invoke(this, showValueArgs);
        }

        public UlChartMarkLine(UlLineChart aChart) : base(aChart)
        {
            GrabWidth = 3;

            MarkColor = Color.FromArgb(192, 222, 57, 205);
            Pen = new Pen(new SolidBrush(MarkColor));
            MarkBrush = new SolidBrush(MarkColor);
            showValueArgs = new ShowValueMarkPointEventArgs();

            lines = new List<UlChartVertLine>();
            lines.Add(new UlChartVertLine(0, MarkColor));
            lines.Add(new UlChartVertLine(0, MarkColor));
            lines[0].Text = "A";
            lines[1].Text = "B";

            Visible = false;
        }

        private void ResetLines()
        {
            Axis2D axis = chart.AxesX[0];
            double baseTime = chart.BufferedChart.BaseTime;
            double start = axis.VisualRange.MinValueInternal;
            double width = axis.VisualRange.MaxValueInternal - start;
            double value1 = start + width / 4;
            double value2 = start + width / 4 * 3;

            lines[0].Value = Math.Round(value1 / baseTime) * baseTime;
            lines[1].Value = Math.Round(value2 / baseTime) * baseTime;
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
                    nX = (e.X < ClipRect.X) ? ClipRect.X : ClipRect.X + ClipRect.Width;
                    chart.Cursor = Cursors.Default;
                }

                MoveTo(nX);
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

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].IsGrabArea(e.X, GrabWidth) == true)
                {
                    chart.Cursor = Cursors.VSplit;
                    lines[i].Grab = true;
                    MoveTo(e.X);
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

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Grab == true)
                {
                    chart.Cursor = Cursors.Default;
                    MoveTo(nX);
                    lines[i].Grab = false;
                }
            }
        }

        public override void Paint(GraphicsCache g)
        {
            if (diagram == null) return;

            if (visible == true)
            {
                CalcLines();

                foreach (UlChartVertLine line in lines)
                {
                    if (line.X < ClipRect.X) line.X = ClipRect.X;
                    if (line.X > (ClipRect.X + ClipRect.Width)) line.X = ClipRect.X + ClipRect.Width;

                    if (line.Visible == true)
                    {
                        SizeF size = g.CalcTextSize(line.Text, chart.LegendFont);
                        int halfWidth = (int)(size.Width / 2);
                        int x = line.X - halfWidth - 2;
                        int y = line.Y - ((int)size.Height - 1);

                        g.FillRectangle(line.Brush, new Rectangle(x, y, (int)size.Width + 4, (int)size.Height - 1));
                        g.DrawString(line.Text, chart.LegendFont, line.TextBrush, x + 2, y);
                        g.DrawLine(line.X, line.Y, line.X, line.Y + ClipRect.Height, line.Pen.Color, (int)line.Pen.Width);
                    }
                }
            }

            if ((lines[0].IsChanged == true) || (lines[1].IsChanged == true))
            {
                OnShowValueMarkPoint();
            }
        }

        private void CalcLines()
        {
            Axis2D axis = chart.AxesX[0];
            double baseTime = chart.BufferedChart.BaseTime;

            foreach (UlChartVertLine line in lines)
            {
                if ((line.Value < axis.VisualRange.MinValueInternal) ||
                    (line.Value > axis.VisualRange.MaxValueInternal))
                {
                    line.Visible = false;
                }
                else
                {
                    line.Visible = true;
                    line.Value = Math.Round(line.Value / baseTime) * baseTime;
                    line.X = diagram.DiagramToPoint(line.Value, 0.0, diagram.AxisX, diagram.AxisY).Point.X;
                    line.Y = ClipRect.Y;
                }
            }
        }

        public bool IsGrab()
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Grab == true)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsGrabArea(MouseEventArgs e)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].IsGrabArea(e.X, GrabWidth) == true)
                {
                    return true;
                }
            }

            return false;
        }

        private void MoveTo(int aX)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Grab == true)
                {
                    lines[i].Value = diagram.PointToDiagram(new Point(aX, lines[i].Y)).NumericalArgument;
                }
            }
        }
    }

    public class UlChartCursor : UlChartBase
	{
		private bool visible;
		public bool Visible
		{
			get { return visible; }
			set 
			{ 
				visible = value;
                if (visible == true) RecalcPoints();

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

		private UlChartCursorPoint[] points;
		public UlChartCursorPoint[] Points
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

		public UlChartCursor(UlLineChart aChart) : base(aChart)
		{
			visible = false;
			height = 0;
			grabWidth = 3;

			pen = new Pen(new SolidBrush(Color.FromArgb(192, 222, 57, 205)));
			markBrush = new SolidBrush(Color.FromArgb(192, 222, 57, 205));
			markPoints = new Point[4];

            points = new UlChartCursorPoint[2];
            points[0] = new UlChartCursorPoint(ClipRect.X + ClipRect.Width / 4, ClipRect.Y, chart);
            points[1] = new UlChartCursorPoint(ClipRect.X + ClipRect.Width / 4 * 3, ClipRect.Y, chart);
        }

        public void Reset()
		{
			points[0].Desc.Reset();
			points[1].Desc.Reset();
		}

        public void RecalcPoints()
        {
            CalcClipRect();

            points[0].X = ClipRect.X + ClipRect.Width / 4;
            points[0].Y = ClipRect.Y;
            points[0].Desc.Reset();

            points[1].X = ClipRect.X + ClipRect.Width / 4 * 3;
            points[1].Y = ClipRect.Y;
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

		public override void Paint(GraphicsCache g)
		{
			if (diagram == null) return;
			if (visible == false) return;

            CalcClipRect();

            for (int i = 0; i < 2; i++)
            {
                if (points[i].X < ClipRect.X) points[i].X = ClipRect.X;
                if (points[i].X > (ClipRect.X + ClipRect.Width)) points[i].X = ClipRect.X + ClipRect.Width;
            }

            markPoints[0] = new Point(points[0].X - 4, points[0].Y - 5);
			markPoints[1] = new Point(points[0].X + 5, points[0].Y - 5);
			markPoints[2] = new Point(points[0].X, points[0].Y);
			markPoints[3] = new Point(points[0].X - 4, points[0].Y - 5);

			if (points[0].Desc.Index == -1)
			{
				g.DrawLine(points[0].X, points[0].Y, points[0].X, points[0].Y + height, pen.Color, (int)pen.Width);
				g.FillPolygon(markPoints, pen.Color);
			}
			else
			{
				Brush cBrush = new SolidBrush(points[0].Desc.SeriesColor);
				Pen cPen = new Pen(cBrush);

				g.DrawLine(points[0].X, points[0].Y, points[0].X, points[0].Y + height, cPen.Color, (int)cPen.Width);
				g.FillPolygon(markPoints, cPen.Color);
			}

			markPoints[0] = new Point(points[1].X - 4, points[1].Y - 5);
			markPoints[1] = new Point(points[1].X + 5, points[1].Y - 5);
			markPoints[2] = new Point(points[1].X, points[1].Y);
			markPoints[3] = new Point(points[1].X - 4, points[1].Y - 5);

			if (points[1].Desc.Index == -1)
			{
				g.DrawLine(points[1].X, points[1].Y, points[1].X, points[1].Y + height, pen.Color, (int)pen.Width);
				g.FillPolygon(markPoints, pen.Color);
			}
			else
			{
				Brush cBrush = new SolidBrush(points[1].Desc.SeriesColor);
				Pen cPen = new Pen(cBrush);

				g.DrawLine(points[1].X, points[1].Y, points[1].X, points[1].Y + height, cPen.Color, (int)cPen.Width);
				g.FillPolygon(markPoints, cPen.Color);
			}

			points[0].Desc.Paint(g, points[1].Desc);
			points[1].Desc.Paint(g, points[0].Desc);
		}

		public override void CalcClipRect()
		{
			//base.CalcClipRect();
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

    public enum EZoomAxis { Both, AxisX, AxisY }

    public class ZoomStackChangedEventArgs : EventArgs
    {
        public ZoomStackChangedEventArgs()
        {
            Value = 0;
        }

        public int Value { get; set; }
    }

    public delegate void ZoomStackChangedEventHandler(object sender, ZoomStackChangedEventArgs e);

    public class UlChartZoom : UlChartBase
	{
		private bool enabled;
		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}

        public bool PointEnabled { get; set; }

        public EZoomAxis ZoomAxis { get; set; }

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

		private Stack<UlChartAxisRangeList> axisRangeStack;
		public Stack<UlChartAxisRangeList> AxisRangeStack
		{
			get { return axisRangeStack; }
		}

        public event EventHandler InvalidSeriesPoint = null;
        protected void OnInvalidSeriesPoint()
        {
            InvalidSeriesPoint?.Invoke(this, null);
        }

        private ZoomStackChangedEventArgs zoomStackArgs;

        public event ZoomStackChangedEventHandler ZoomStackChanged = null;
        protected void OnZoomStackChanged()
        {
            zoomStackArgs.Value = axisRangeStack.Count;
            ZoomStackChanged?.Invoke(this, zoomStackArgs);
        }

        public UlChartZoom(UlLineChart aChart) : base(aChart)
		{
			chart = aChart;
			diagram = (SwiftPlotDiagram)chart.Diagram;
            enabled = true;
			PointEnabled = false;
            ZoomAxis = EZoomAxis.Both;

			brush = new SolidBrush(Color.FromArgb(32, InvertColor(chart.BackColor)));
			pen = new Pen(new SolidBrush(Color.FromArgb(64, InvertColor(chart.BackColor))));

			points = new Point[2];
			points[0] = new Point(0, 0);
			points[1] = new Point(0, 0);

            zoomStackArgs = new ZoomStackChangedEventArgs();
			axisRangeStack = new Stack<UlChartAxisRangeList>();
		}

        public override void MouseDown(MouseEventArgs e)
        {
            if (diagram == null) return;
            if (enabled == false) return;
            if (IsZoomingValid() == false) return;

            DiagramCoordinates cCoord1 = diagram.PointToDiagram(e.Location);

            if (cCoord1.IsEmpty == false)
            {
                if ((ClipRect.Top > 0) && (ClipRect.Left > 0))
                {
                    PointEnabled = true;

                    switch (ZoomAxis)
                    {
                        case EZoomAxis.Both:
                            points[0] = e.Location;
                            points[1] = e.Location;
                            break;

                        case EZoomAxis.AxisX:
                            points[0].X = e.X;
                            points[0].Y = ClipRect.Top;
                            points[1].X = e.X;
                            points[1].Y = ClipRect.Bottom;
                            break;

                        case EZoomAxis.AxisY:
                            points[0].X = ClipRect.Left;
                            points[0].Y = e.Y;
                            points[1].X = ClipRect.Right;
                            points[1].Y = e.Y;
                            break;
                    }
                }
            }
        }

        public override void MouseMove(MouseEventArgs e)
		{
			if (diagram == null) return;
			if (PointEnabled == false) return;

            if (IsValidPoint(e.Location) == true)
            {
                switch (ZoomAxis)
                {
                    case EZoomAxis.Both:
                        points[1] = e.Location;
                        break;

                    case EZoomAxis.AxisX:
                        points[1].X = e.X;
                        points[1].Y = ClipRect.Bottom;
                        break;

                    case EZoomAxis.AxisY:
                        points[1].X = ClipRect.Right;
                        points[1].Y = e.Y;
                        break;
                }
            }
            else
            {
                switch (ZoomAxis)
                {
                    case EZoomAxis.Both:
                        if (IsValidPoint(new Point(e.X, ClipRect.Y + ClipRect.Height / 2)) == true)
                            points[1].X = e.X;
                        else
                            points[1].X = (e.X >= ClipRect.Right) ? ClipRect.Right : ClipRect.Left;

                        if (IsValidPoint(new Point(ClipRect.X + ClipRect.Width / 2, e.Y)) == true)
                            points[1].Y = e.Y;
                        else
                            points[1].Y = (e.Y >= ClipRect.Bottom) ? ClipRect.Bottom : ClipRect.Top;
                        break;

                    case EZoomAxis.AxisX:
                        if (IsValidPoint(new Point(e.X, ClipRect.Y + ClipRect.Height / 2)) == true)
                            points[1].X = e.X;
                        else
                            points[1].X = (e.X >= ClipRect.Right) ? ClipRect.Right : ClipRect.Left;

                        points[1].Y = ClipRect.Bottom;
                        break;

                    case EZoomAxis.AxisY:
                        points[1].X = ClipRect.Right;

                        if (IsValidPoint(new Point(ClipRect.X + ClipRect.Width / 2, e.Y)) == true)
                            points[1].Y = e.Y;
                        else
                            points[1].Y = (e.Y >= ClipRect.Bottom) ? ClipRect.Bottom : ClipRect.Top;
                        break;
                }
            }
		}

		public override void MouseUp(MouseEventArgs e)
		{
			if (diagram == null) return;
			if (PointEnabled == false) return;

			DiagramCoordinates cCoord1 = diagram.PointToDiagram(e.Location);

            if (cCoord1.IsEmpty == false)
			{
                switch (ZoomAxis)
                {
                    case EZoomAxis.Both:
                        points[1] = e.Location;
                        break;

                    case EZoomAxis.AxisX:
                        points[1].X = e.X;
                        points[1].Y = ClipRect.Bottom;
                        break;

                    case EZoomAxis.AxisY:
                        points[1].X = ClipRect.Right;
                        points[1].Y = e.Y;
                        break;
                }
			}
			else
			{
                switch (ZoomAxis)
                {
                    case EZoomAxis.Both:
                        if (IsValidPoint(new Point(e.X, ClipRect.Y + ClipRect.Height / 2)) == true)
                            points[1].X = e.X;
                        else
                            points[1].X = (e.X >= ClipRect.Right) ? ClipRect.Right : ClipRect.Left;

                        if (IsValidPoint(new Point(ClipRect.X + ClipRect.Width / 2, e.Y)) == true)
                            points[1].Y = e.Y;
                        else
                            points[1].Y = (e.Y >= ClipRect.Bottom) ? ClipRect.Bottom : ClipRect.Top;
                        break;

                    case EZoomAxis.AxisX:
                        if (IsValidPoint(new Point(e.X, ClipRect.Y + ClipRect.Height / 2)) == true)
                            points[1].X = e.X;
                        else
                            points[1].X = (e.X >= ClipRect.Right) ? ClipRect.Right : ClipRect.Left;

                        points[1].Y = ClipRect.Bottom;
                        break;

                    case EZoomAxis.AxisY:
                        points[1].X = ClipRect.Right;

                        if (IsValidPoint(new Point(ClipRect.X + ClipRect.Width / 2, e.Y)) == true)
                            points[1].Y = e.Y;
                        else
                            points[1].Y = (e.Y >= ClipRect.Bottom) ? ClipRect.Bottom : ClipRect.Top;
                        break;
                }
            }

            Execute();
			PointEnabled = false;
		}

		public override void Paint(GraphicsCache g)
		{
			if (diagram == null) return;
			if (PointEnabled == false) return;

			CalcRect();
			g.FillRectangle(brush, rect);
			g.DrawRectangle(pen, rect);
		}

        public void SetVisualRangeAxisX(int index, double max, double min)
        {
            List<Axis2D> cXList = diagram.GetAllAxesX();

            cXList[index].WholeRange.SetMinMaxValues(min, max);
            cXList[index].VisualRange.SetMinMaxValues(min, max);

            SetAxisLabelTextPattern(0, cXList[index]);
        }

        public void SetVisualRangeAxisY(int index, double max, double min)
        {
            List<Axis2D> cYList = diagram.GetAllAxesY();

            cYList[index].WholeRange.SetMinMaxValues(min, max);
            cYList[index].VisualRange.SetMinMaxValues(min, max);

            SetAxisLabelTextPattern(0, cYList[index]);
        }

        private bool IsZoomingValid()
        {
            bool ret = false;

            foreach (Series series in chart.Series)
            {
                if (series.Visible == true)
                {
                    ret = (series.Points.Count > 6) ? true : false;
                }
            }

            return ret;
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

            chart.VisualRangeChanging = true;

            diagram.AxisX.VisualRange.SetMinMaxValues((object)cCoord1.NumericalArgument, (object)cCoord2.NumericalArgument);
            diagram.AxisY.VisualRange.SetMinMaxValues((object)cCoord2.NumericalValue, (object)cCoord1.NumericalValue);

            for (int i=0; i<diagram.SecondaryAxesY.Count; i++)
            {
                diagram.SecondaryAxesY[i].VisualRange.SetMinMaxValues(
                    (object)cCoord2.GetAxisValue((diagram as SwiftPlotDiagram).SecondaryAxesY[i]).NumericalValue,
                    (object)cCoord1.GetAxisValue((diagram as SwiftPlotDiagram).SecondaryAxesY[i]).NumericalValue);
            }

            chart.VisualRangeChanging = false;

            SetSeriesMarkers();
            SetAllAxesLabelTextPattern();

            OnInvalidSeriesPoint();
        }

        private void SetSeriesMarkers()
		{
            //DefaultBoolean bMarker = ((SwiftPlotSeriesView)chart.Series[0].View).MarkerVisibility;

            //Point pt1 = diagram.DiagramToPoint(chart.Series[0].Points[0].NumericalArgument,
            //    chart.Series[0].Points[0].Values[0], diagram.AxisX, diagram.AxisY).Point;

            //Point pt2 = diagram.DiagramToPoint(chart.Series[0].Points[1].NumericalArgument,
            //    chart.Series[0].Points[1].Values[0], diagram.AxisX, diagram.AxisY).Point;

            //DefaultBoolean bState = (Math.Abs(pt1.X - pt2.X) > 4) ? DefaultBoolean.True : DefaultBoolean.False;

            //for (int i = 0; i < chart.Series.Count; i++)
            //{
            //    ((SwiftPlotSeriesView)chart.Series[i].View).MarkerVisibility = bState;
            //}
        }

		private void SetAllAxesLabelTextPattern()
		{
            //List<Axis2D> cXList = diagram.GetAllAxesX();
            //for (int i = 0; i < cXList.Count; i++)
            //{
            //    SetAxisLabelTextPattern(0, cXList[i]);
            //}

            //List<Axis2D> cYList = diagram.GetAllAxesY();
            //for (int i = 0; i < cYList.Count; i++)
            //{
            //    SetAxisLabelTextPattern(1, cYList[i]);
            //}
        }

        private void SetAxisLabelTextPattern(int aIndex, Axis2D aAxis)
        {
            //int len;

            //if (aAxis.NumericScaleOptions.GridSpacing >= 1) len = 1;
            //else
            //{
            //    string str = aAxis.NumericScaleOptions.GridSpacing.ToString();
            //    len = str.IndexOf(".");

            //    if (len == -1)
            //    {
            //        len = Math.Abs(int.Parse(str.Substring(str.IndexOf("E") + 1)));
            //    }
            //    else
            //    {
            //        len = str.Substring(len).Length - 1;
            //    }
            //}

            //if (aIndex == 0)
            //    aAxis.Label.TextPattern = "{A:F" + len.ToString() + "}";
            //else
            //    aAxis.Label.TextPattern = "{V:F" + len.ToString() + "}";
        }

        public void Clear()
        {
            axisRangeStack.Clear();
            OnZoomStackChanged();
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

        public void AutoSet(double xMagine = 2, double yMagine = 2)
        {
            Push();

            switch (ZoomAxis)
            {
                case EZoomAxis.Both:
                    AutoSetAxisX(xMagine);
                    AutoSetAxisY(yMagine);
                    break;

                case EZoomAxis.AxisX:
                    AutoSetAxisX(xMagine);
                    break;

                case EZoomAxis.AxisY:
                    AutoSetAxisY(yMagine);
                    break;
            }

            SetSeriesMarkers();
            SetAllAxesLabelTextPattern();
            OnInvalidSeriesPoint();
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

                if (fMin == fMax) continue;
                if ((fMin == double.MaxValue) || (fMax == double.MinValue)) continue;

				double fDiff = fMax - fMin;
				fMin -= i * fDiff;
				fMax += (cYList.Count - 1 - i) * fDiff;

				cYList[i].WholeRange.SetMinMaxValues(fMin, fMax);
				cYList[i].VisualRange.SetMinMaxValues(fMin, fMax);
			}
        }

        public void Push()
		{
			UlChartAxisRangeList cRangeList = new UlChartAxisRangeList();

			List<Axis2D> cXList = diagram.GetAllAxesX();
			for (int i = 0; i < cXList.Count; i++)
			{
				UlChartAxisRange cXRange = new UlChartAxisRange();

                cXRange.Enabled = (ZoomAxis == EZoomAxis.AxisY) ? false : true;
				cXRange.VisualRange.Min = cXList[i].VisualRange.MinValue;
				cXRange.VisualRange.Max = cXList[i].VisualRange.MaxValue;
				cXRange.WholeRange.Min = cXList[i].WholeRange.MinValue;
				cXRange.WholeRange.Max = cXList[i].WholeRange.MaxValue;
				cRangeList.XList.Add(cXRange);
			}

			List<Axis2D> cYList = diagram.GetAllAxesY();
			for (int i = 0; i < cYList.Count; i++)
			{
				UlChartAxisRange cYRange = new UlChartAxisRange();

                cYRange.Enabled = (ZoomAxis == EZoomAxis.AxisX) ? false : true;
                cYRange.VisualRange.Min = cYList[i].VisualRange.MinValue;
				cYRange.VisualRange.Max = cYList[i].VisualRange.MaxValue;
				cYRange.WholeRange.Min = cYList[i].WholeRange.MinValue;
				cYRange.WholeRange.Max = cYList[i].WholeRange.MaxValue;
				cRangeList.YList.Add(cYRange);
			}

			axisRangeStack.Push(cRangeList);
            OnZoomStackChanged();
		}

		public void Pop()
		{
			if (axisRangeStack.Count == 0) return;

            chart.VisualRangeChanging = true;
			UlChartAxisRangeList cList = axisRangeStack.Pop();

            List<Axis2D> cXList = diagram.GetAllAxesX();
            for (int i = 0; i < cList.XList.Count; i++)
            {
                if ((cList.XList[i].Enabled == true) || (axisRangeStack.Count == 0))
                {
                    if (i < cXList.Count)
                    {
                        cXList[i].WholeRange.SetMinMaxValues(cList.XList[i].WholeRange.Min, cList.XList[i].WholeRange.Max);
                        cXList[i].VisualRange.SetMinMaxValues(cList.XList[i].VisualRange.Min, cList.XList[i].VisualRange.Max);

                        SetAxisLabelTextPattern(0, cXList[i]);
                    }
                }
            }

			List<Axis2D> cYList = diagram.GetAllAxesY();
			for (int i = 0; i < cList.YList.Count; i++)
			{
                if (cList.YList[i].Enabled == true)
                {
                    if (i < cYList.Count)
                    {
                        cYList[i].WholeRange.SetMinMaxValues(cList.YList[i].WholeRange.Min, cList.YList[i].WholeRange.Max);
                        cYList[i].VisualRange.SetMinMaxValues(cList.YList[i].VisualRange.Min, cList.YList[i].VisualRange.Max);

                        SetAxisLabelTextPattern(1, cYList[i]);
                    }
                }
			}

            chart.VisualRangeChanging = false;

            SetSeriesMarkers();
            OnInvalidSeriesPoint();
            OnZoomStackChanged();
        }

        public void SetAllAxisXEnabledInStack(bool enabled=false)
        {
            foreach (UlChartAxisRangeList rangeList in axisRangeStack)
            {
                rangeList.XList[0].Enabled = enabled;
            }
        }

        private void AutoSetAxisX(double magine)
		{
            magine = magine / 100.0 / 2.0;
			List<Axis2D> cXList = diagram.GetAllAxesX();

			for (int i = 0; i < cXList.Count; i++)
			{
				double fMin = double.MaxValue;
				double fMax = double.MinValue;

				GetXAxisSeriesPointMinMax(cXList[i], ref fMin, ref fMax);
                double diff = (fMax - fMin) * magine;

				cXList[i].WholeRange.SetMinMaxValues(fMin - diff, fMax + diff);
				cXList[i].VisualRange.SetMinMaxValues(fMin - diff, fMax + diff);

                SetAxisLabelTextPattern(0, cXList[i]);
            }
		}

		private void AutoSetAxisY(double tolerance)
		{
            tolerance /= 100.0;
            List<Axis2D> cYList = diagram.GetAllAxesY();

			for (int i = 0; i < cYList.Count; i++)
			{
				double fMin = double.MaxValue;
				double fMax = double.MinValue;

				GetYAxisSeriesPointMinMax(cYList[i], ref fMin, ref fMax);

                if (fMin == fMax) continue;
                if ((fMin == double.MaxValue) || (fMax == double.MinValue)) continue;

                double diff = (fMax - fMin) * tolerance;

                cYList[i].WholeRange.SetMinMaxValues(fMin - diff, fMax + diff);
                cYList[i].VisualRange.SetMinMaxValues(fMin - diff, fMax + diff);

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
                if (chart.Series[i].Visible == false) continue;

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

    public class CustomDescriptionLabelEventArgs : EventArgs
    {
        public CustomDescriptionLabelEventArgs()
        {
            Value = 0;
            Text = "0:00";
        }

        public double Value { get; set; }
        public string Text { get; set; }
    }

    public delegate void CustomDescriptionLabelEventHandler(object sender, CustomDescriptionLabelEventArgs e);

    public class UlChartDescription : UlChartBase
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

        private CustomDescriptionLabelEventArgs descLabelArgs;

        public CustomDescriptionLabelEventHandler CustomAxisXDescriptionLabel = null;
        protected void OnCustomAxisXDescriptionLabel()
        {
            CustomAxisXDescriptionLabel?.Invoke(null, descLabelArgs);
        }

        public CustomDescriptionLabelEventHandler CustomAxisYDescriptionLabel = null;
        protected void OnCustomAxisYDescriptionLabel()
        {
            CustomAxisXDescriptionLabel?.Invoke(null, descLabelArgs);
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

        private Dictionary<int, List<int>> dicXY;

        public UlChartDescription(UlLineChart aChart) : base(aChart)
		{
			enabled = true;
			pointLock = false;
			index = -1;
			grabWidth = 3;
			grabHeight = 3;

            dicXY = new Dictionary<int, List<int>>();

            descLabelArgs = new CustomDescriptionLabelEventArgs();

			balloonFont = new Font("Arial", 8);
			balloonXFormat = "X : {0:0.00}";
			balloonYFormat = "Y : {0:0.00}";
			balloondXFormat = "dX : {0:0.00}";
			balloondYFormat = "dY : {0:0.00}";
		}

        public override void MouseMove(MouseEventArgs e)
        {
            if (diagram == null) return;
            if (pointLock == true) return;

            DiagramCoordinates cCoord1 =
                ((SwiftPlotDiagram)chart.Diagram).PointToDiagram(e.Location);

            if (cCoord1.IsEmpty == true) return;

            index = -1;
            series = null;
            int nDist = int.MaxValue;

            dicXY.Clear();
            GetIndexListOfAxisX(e.X, dicXY);

            if (dicXY.Count > 0)
            {
                for (int i = 0; i < chart.Series.Count; i++)
                {
                    if (dicXY.ContainsKey(i) == true)
                    {
                        Series theSeries = chart.Series[i];
                        List<int> yList = dicXY[i];

                        for (int j = 0; j < yList.Count; j++)
                        {
                            Point pt = ((SwiftPlotDiagram)chart.Diagram).DiagramToPoint(
                                theSeries.Points[yList[j]].NumericalArgument,
                                theSeries.Points[yList[j]].Values[0],
                                ((SwiftPlotSeriesView)theSeries.View).AxisX,
                                ((SwiftPlotSeriesView)theSeries.View).AxisY).Point;

                            if ((e.Y >= (pt.Y - grabHeight)) && (e.Y <= (pt.Y + grabHeight)))
                            {
                                if (Math.Abs(pt.Y - grabHeight) < nDist)
                                {
                                    index = yList[j];
                                    series = theSeries;
                                    nDist = Math.Abs(pt.Y - grabHeight);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetIndexListOfAxisX(int aX, Dictionary<int, List<int>> aDic)
        {
            aDic.Clear();

            for (int i = 0; i < chart.Series.Count; i++)
            {
                Series theSeries = chart.Series[i];

                if (theSeries.Visible == true)
                {
                    List<int> xList = new List<int>();

                    for (int j = 0; j < theSeries.Points.Count; j++)
                    {
                        Point pt = ((SwiftPlotDiagram)chart.Diagram).DiagramToPoint(
                            theSeries.Points[j].NumericalArgument,
                            theSeries.Points[j].Values[0],
                            ((SwiftPlotSeriesView)theSeries.View).AxisX,
                            ((SwiftPlotSeriesView)theSeries.View).AxisY).Point;

                        if ((aX >= (pt.X - grabWidth)) && (aX <= (pt.X + grabWidth)))
                        {
                            xList.Add(j);
                        }
                        else
                        {
                            if (xList.Count > 0)
                            {
                                aDic.Add(i, xList);
                                break;
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

		public override void Paint(GraphicsCache g)
		{
			Paint(g, null);
		}

		public void Paint(GraphicsCache g, UlChartDescription aDesc)
		{
			if (diagram == null) return;
			if (enabled == false) return;
            if (series == null) return;
            if (series.Points.Count == 0) return;
			if (index < 0) return;

			Brush cBalloonBrush = new SolidBrush(Color.FromArgb(64, InvertColor(chart.BackColor)));
			Pen cBalloonPen = new Pen(new SolidBrush(Color.FromArgb(64, SeriesColor)));
			Brush cBalloonFontBrush = new SolidBrush(InvertColor(chart.BackColor));
			Pen cMarkPen = new Pen(new SolidBrush(SeriesColor));

			Point pt = ((SwiftPlotDiagram)chart.Diagram).DiagramToPoint(
				series.Points[index].NumericalArgument,
				series.Points[index].Values[0],
				((SwiftPlotSeriesView)series.View).AxisX,
				((SwiftPlotSeriesView)series.View).AxisY).Point;

			string sName = series.Name;
			SizeF theNameSize = g.CalcTextSize(sName, balloonFont);

			double fXValue1, fXValue2, fYValue1, fYValue2;
            string sXValue1, sXValue2, sYValue1, sYValue2;
            SizeF theXValueSize1, theYValueSize1;

            fXValue1 = series.Points[index].NumericalArgument;
            sXValue1 = "";
            fYValue1 = series.Points[index].Values[0];
            sYValue1 = "";

            if (CustomAxisXDescriptionLabel == null)
            {
                theXValueSize1 = g.CalcTextSize(string.Format(balloonXFormat, fXValue1), balloonFont);
            }
            else
            {
                descLabelArgs.Value = fXValue1;
                OnCustomAxisXDescriptionLabel();
                sXValue1 = descLabelArgs.Text;
                theXValueSize1 = g.CalcTextSize(string.Format(balloonXFormat, sXValue1), balloonFont);
            }

            if (CustomAxisYDescriptionLabel == null)
            {
                theYValueSize1 = g.CalcTextSize(string.Format(balloonYFormat, fYValue1), balloonFont);
            }
            else
            {
                descLabelArgs.Value = fYValue1;
                OnCustomAxisYDescriptionLabel();
                sYValue1 = descLabelArgs.Text;
                theYValueSize1 = g.CalcTextSize(string.Format(balloonYFormat, sYValue1), balloonFont);
            }

            fXValue2 = 0;
            sXValue2 = "";
			fYValue2 = 0;
            sYValue2 = "";

            SizeF theXValueSize2 = new SizeF(0, 0);
			SizeF theYValueSize2 = new SizeF(0, 0);

			if ((aDesc != null) && (aDesc.Index != -1))
			{
				fXValue2 = fXValue1 - aDesc.Series.Points[aDesc.Index].NumericalArgument;

                if (CustomAxisXDescriptionLabel == null)
                {
                    theXValueSize2 = g.CalcTextSize(string.Format(balloondXFormat, fXValue2), balloonFont);
                }
                else
                {
                    descLabelArgs.Value = fXValue2;
                    OnCustomAxisXDescriptionLabel();
                    sXValue2 = descLabelArgs.Text;
                    theXValueSize2 = g.CalcTextSize(string.Format(balloonXFormat, sXValue2), balloonFont);
                }

                fYValue2 = fYValue1 - aDesc.Series.Points[aDesc.Index].Values[0];

                if (CustomAxisYDescriptionLabel == null)
                {
                    theYValueSize2 = g.CalcTextSize(string.Format(balloondYFormat, fYValue2), balloonFont);
                }
                else
                {
                    descLabelArgs.Value = fYValue2;
                    OnCustomAxisYDescriptionLabel();
                    sYValue2 = descLabelArgs.Text;
                    theYValueSize2 = g.CalcTextSize(string.Format(balloonYFormat, sYValue2), balloonFont);
                }
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

                if (string.IsNullOrWhiteSpace(sXValue1) == true)
				    g.DrawString(string.Format(balloonXFormat, fXValue1), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight);
                else
                    g.DrawString(string.Format(balloonXFormat, sXValue1), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight);

                if (string.IsNullOrWhiteSpace(sYValue1) == true)
                    g.DrawString(string.Format(balloonYFormat, fYValue1), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight * 2);
                else
                    g.DrawString(string.Format(balloonYFormat, sYValue1), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight * 2);

                if ((aDesc != null) && (aDesc.Index != -1))
				{
                    if (string.IsNullOrWhiteSpace(sXValue2) == true)
                        g.DrawString(string.Format(balloondXFormat, fXValue2), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight * 3);
                    else
                        g.DrawString(string.Format(balloondXFormat, sXValue2), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight * 3);

                    if (string.IsNullOrWhiteSpace(sYValue2) == true)
                        g.DrawString(string.Format(balloondYFormat, fYValue2), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight * 4);
                    else
                        g.DrawString(string.Format(balloondYFormat, sYValue2), balloonFont, cBalloonFontBrush, nX + 8, nY + 8 + fHeight * 4);
                }
            }
		}
	}
}
