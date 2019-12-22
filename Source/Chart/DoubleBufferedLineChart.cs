using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraCharts;

using Ulee.DllImport.Win32;
using Ulee.Utils;

namespace Ulee.Chart
{
    public enum EChartMode { Static, Dynamic }

    public class UlDoubleBufferedLineChart : UlLineChart
    {
        public UlDoubleBufferedLineChart()
        {
            BufferedChart = this;
            BufferedSeries = new UlDoubleBufferedSeriesCollection();

            Mode = EChartMode.Static;

            AxisVisualRangeChanged += DoAxisVisualRangeChanged;
            CustomDrawAxisLabel += DoCustomDrawAxisLabel;
        }

        public EChartMode Mode { get; set; }

        public float BaseTime
        {
            get { return BufferedSeries.BaseTime; }
            set { BufferedSeries.BaseTime = value; }
        }

        public int BlockLength
        {
            get { return BufferedSeries.BlockLength; }
            set { BufferedSeries.BlockLength = value; }
        }

        public UlDoubleBufferedSeriesCollection BufferedSeries { get; set; }

        private void DoAxisVisualRangeChanged(object sender, AxisRangeChangedEventArgs e)
        {
            if (VisualRangeChanging == true) return;

            if (e.Axis == ((SwiftPlotDiagram)Diagram).AxisX)
            {
                InvalidateSeries();
            }
        }

        private void DoCustomDrawAxisLabel(object sender, CustomDrawAxisLabelEventArgs e)
        {
            if (AxesX.Count == 0) return;
            if (AxesY.Count == 0) return;

            if (e.Item.Axis == ((SwiftPlotDiagram)Diagram).AxisX)
            {
                double value = (double)e.Item.AxisValue;
                long secTicks = (long)value % 60000;
                long minTick = (long)value - secTicks;
                long msec = secTicks % 1000;
                long sec = secTicks / 1000;
                long min = minTick / 60000;

                e.Item.Text = $"{min}:{sec:d2}";
                if (msec > 0) e.Item.Text += $".{msec:d3}";
            }
        }

        private void DoCustomAxisXDescriptionLabel(object sender, CustomDescriptionLabelEventArgs e)
        {
            bool minus = (e.Value < 0) ? true : false;

            e.Value = Math.Abs(e.Value);

            long secTicks = (long)e.Value % 60000;
            long minTick = (long)e.Value - secTicks;
            long msec = secTicks % 1000;
            long sec = secTicks / 1000;
            long min = minTick / 60000;

            if (minus == true)
               e.Text = $"-{min}:{sec:d2}";
            else
               e.Text = $"{min}:{sec:d2}";

            if (msec > 0) e.Text += $".{msec:d3}";
        }

        public void InvalidateSeries(object sender=null, EventArgs args=null)
        {
            if (Zooming.PointEnabled == true) return;

            switch (Mode)
            {
                case EChartMode.Static:
                    InvalidateStaticSeries();
                    break;

                case EChartMode.Dynamic:
                    InvalidateDynamicSeries();
                    break;
            }
        }

        private void InvalidateStaticSeries()
        {
            if (ClipRect.Width == 0) return;
            if (BufferedSeries.PointsCount == 0) return;
            if ((SwiftPlotDiagram)Diagram == null) return;

            Series.BeginUpdate();

            try
            {
                double startTime, time;
                float[] values;
                int start, count;
                SeriesPoint[] points;
                SwiftPlotDiagramAxisX axisX = ((SwiftPlotDiagram)Diagram).AxisX;
                double visualMin = (double)axisX.VisualRange.MinValue;
                double visualMax = (double)axisX.VisualRange.MaxValue;
                double visualWidth = visualMax - visualMin;
                double wholeMin = (double)axisX.WholeRange.MinValue;
                double wholeMax = (double)axisX.WholeRange.MaxValue;
                double wholeWidth = wholeMax - wholeMin;
                double step;

                if (visualWidth == wholeWidth)
                {
                    start = 0;
                    step = (double)BufferedSeries.PointsCount / (double)ClipRect.Width;

                    if (step > 1)
                    {
                        count = ClipRect.Width;
                    }
                    else
                    {
                        step = 1;
                        count = BufferedSeries.PointsCount;
                    }
                }
                else
                {
                    start = (int)(visualMin / BaseTime);
                    count = (int)(visualWidth / BaseTime);
                    step = (count <= ClipRect.Width) ? 1 : ((double)count / (double)ClipRect.Width);
                    count = (int)(count / step) + 4;

                    if (start - step >= 0)
                    {
                        start -= (int)step;
                        count++;
                    }
                }

                startTime = start * BaseTime;
                ClearSeriesPoint();

                BufferedSeries.Lock();

                try
                {
                    foreach (UlDoubleBufferedSeries bufferedSeries in BufferedSeries.IndexList)
                    {
                        Series series = Series[bufferedSeries.Name];

                        if (series.Visible == true)
                        {
                            points = new SeriesPoint[count];
                            values = bufferedSeries.Points.ToArray(start, step, count);

                            for (int i = 0; i < values.Length; i++)
                            {
                                time = startTime + Math.Round(i * step) * BaseTime;
                                points[i] = new SeriesPoint(time, values[i]);
                            }

                            series.Points.AddRange(points);
                        }

                        Win32.SwitchToThread();
                    }
                }
                finally
                {
                    BufferedSeries.Unlock();
                }
            }
            finally
            {
                Series.EndUpdate();
            }
        }

        private void InvalidateDynamicSeries()
        {
            if (ClipRect.Width == 0) return;
            if (Zooming.PointEnabled == true) return;
            if (BufferedSeries.PointsCount == 0) return;
            if ((SwiftPlotDiagram)Diagram == null) return;

            SwiftPlotDiagramAxisX axisX = ((SwiftPlotDiagram)Diagram).AxisX;

            double visualMin = (double)axisX.VisualRange.MinValue;
            double visualMax = (double)axisX.VisualRange.MaxValue;
            double visualWidth = visualMax - visualMin;

            if (visualWidth == 0) return;

            double step = (visualWidth / BaseTime) / ClipRect.Width;
            step = (step < 1) ? 1 : step;

            int start = (int)(visualMin / BaseTime);
            int stop = (int)Math.Ceiling(visualMax / BaseTime);
            stop = (stop < BufferedSeries.PointsCount) ? stop : BufferedSeries.PointsCount;

            int count = (int)((double)(stop - start) / step);

            if (count <= 0) return;
            
            if (start >= step)
            {
                start -= (int)step;
                count++;
            }

            if ((start + count * step) < BufferedSeries.PointsCount) count++;

            double time;
            double startTime = start * BaseTime;
            float[] values;
            SeriesPoint[] points;

            Series.BeginUpdate();

            try
            {
                ClearSeriesPoint();
                BufferedSeries.Lock();

                try
                {
                    foreach (UlDoubleBufferedSeries bufferedSeries in BufferedSeries.IndexList)
                    {
                        Series series = Series[bufferedSeries.Name];

                        if (series.Visible == true)
                        {
                            points = new SeriesPoint[count];
                            values = bufferedSeries.Points.ToArray(start, step, count);

                            for (int i = 0; i < values.Length; i++)
                            {
                                time = startTime + Math.Round(i * step) * BaseTime;
                                points[i] = new SeriesPoint(time, values[i]);
                            }

                            series.Points.AddRange(points);
                        }

                        Win32.SwitchToThread();
                    }
                }
                finally
                {
                    BufferedSeries.Unlock();
                }
            }
            finally
            {
                Series.EndUpdate();
            }
        }

        public override void Prepare()
        {
            base.Prepare();

            Zooming.InvalidSeriesPoint += InvalidateSeries;

            for (int i = 0; i < 2; i++)
            {
                VertCursor.Points[i].Desc.BalloonXFormat = "X : {0}";
                VertCursor.Points[i].Desc.BalloondXFormat = "dX : {0}";
                VertCursor.Points[i].Desc.CustomAxisXDescriptionLabel += DoCustomAxisXDescriptionLabel;
            }

            Description.BalloonXFormat = "X : {0}";
            Description.BalloondXFormat = "dX : {0}";
            Description.CustomAxisXDescriptionLabel += DoCustomAxisXDescriptionLabel;
        }

        public void PrepareSeries()
        {
            Series.Clear();

            for (int i=0; i<BufferedSeries.Count; i++)
            {
                Series series = new Series(BufferedSeries[i].Name, ViewType.SwiftPlot);
                series.ArgumentScaleType = ScaleType.Numerical;
                series.ValueScaleType = ScaleType.Numerical;
                ((SwiftPlotSeriesView)series.View).Color = BufferedSeries[i].Color;
                Series.Add(series);
            }
        }

        public void SetBufferedSeries(UlDoubleBufferedSeriesCollection series, bool prepare=true)
        {
            BufferedSeries = series;
            if (prepare == true) PrepareSeries();
        }

        public override void Clear()
        {
            base.Clear();
            BufferedSeries.Clear();
        }

        public void ClearPoints()
        {
            ClearSeriesPoint();
            BufferedSeries.ClearPoints();
        }
    }

    public class UlDoubleBufferedSeriesCollection
    {
        public UlDoubleBufferedSeriesCollection()
        {
            BaseTime = 1000;
            BlockLength = 600;
            IndexList = new List<UlDoubleBufferedSeries>();
            NameList = new Dictionary<string, UlDoubleBufferedSeries>();
        }

        public List<UlDoubleBufferedSeries> IndexList;
        public Dictionary<string, UlDoubleBufferedSeries> NameList;

        public UlDoubleBufferedSeries this[int index]
        { get { return IndexList[index]; } }

        public UlDoubleBufferedSeries this[string name]
        { get { return NameList[name]; } }

        public int Count
        { get { return IndexList.Count; } }

        public int PointsCount
        { get { return IndexList[0].Points.Count; } }

        public float BaseTime { get; set; }

        public int BlockLength { get; set; }

        public void Lock()
        {
            Monitor.Enter(this);
        }

        public void Unlock()
        {
            Monitor.Exit(this);
        }

        public void Clear()
        {
            lock (this)
            {
                IndexList.Clear();
                NameList.Clear();
            }
        }

        public void ClearPoints()
        {
            lock (this)
            {
                foreach (UlDoubleBufferedSeries bufferedSeries in IndexList)
                {
                    bufferedSeries.Points.Clear();
                }
            }
        }

        public void Add(UlDoubleBufferedSeries bufferedSeries)
        {
            lock (this)
            {
                IndexList.Add(bufferedSeries);
                NameList.Add(bufferedSeries.Name, bufferedSeries);
            }
        }
    }

    public class UlDoubleBufferedSeries
    {
        public UlDoubleBufferedSeries(string name, Color color, int length=600)
        {
            BaseSeries = new Series(name, ViewType.SwiftPlot);

            BaseSeries.ArgumentScaleType = ScaleType.Numerical;
            BaseSeries.ValueScaleType = ScaleType.Numerical;
            ((SwiftPlotSeriesView)BaseSeries.View).Color = color;

            Points = new UlBlockList<float>(length);
        }

        public Series BaseSeries { get; private set; }

        public string Name
        {
            get { return BaseSeries.Name; }
            set { BaseSeries.Name = value; }
        }

        public Color Color
        {
            get { return ((SwiftPlotSeriesView)BaseSeries.View).Color; }
            set { ((SwiftPlotSeriesView)BaseSeries.View).Color = value; }
        }

        public UlBlockList<float> Points { get; private set; }

        public void Clear()
        {
            BaseSeries.Points.Clear();
            Points.Clear();
        }
    }
}
