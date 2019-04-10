using HT2000Viewer.Common;
using HT2000Viewer.Models;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace HT2000Viewer.Controls
{
    public sealed partial class QChart : UserControl
    {
        //  public ObservableCollection<object> DataSource { get; set; }
        public QChartLabels Labels { get; } = new QChartLabels();

        public QChart()
        {
            this.InitializeComponent();
            if (DataSource == null)
            {
             //   GenerateRandomData();
             //   TestTimer();
            }
            else
                CopyData();

        }
        public int XNumber { get; set; } = 10;

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(DataSource), typeof(IList), typeof(QChart), new PropertyMetadata(null, OnDataSourceChanged));

        public IList DataSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private void Invalidate() => ChartCanvas.Invalidate();

        private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chart = d as QChart;
            var collection = e.NewValue as IList;
            if (chart == null || collection == null) return;
            var observable = collection as INotifyCollectionChanged;
            if (observable != null)
                observable.CollectionChanged += (s, a) => chart.Invalidate();
            chart.Invalidate();
        }

        public static readonly DependencyProperty ValueNameProperty =
                                DependencyProperty.Register(nameof(ValueName), typeof(string), typeof(QChart), new PropertyMetadata(null));

        public string ValueName
        {
            get { return (string)GetValue(ValueNameProperty); }
            set { SetValue(ValueNameProperty, value); }
        }


        public static readonly DependencyProperty LeftTopTextProperty =
                                DependencyProperty.Register(nameof(LeftTopText), typeof(string), typeof(QChart), new PropertyMetadata(null));

        public string LeftTopText
        {
            get { return (string)GetValue(LeftTopTextProperty); }
            set { SetValue(LeftTopTextProperty, value); }
        }



        public static readonly DependencyProperty LeftBottomTextProperty =
                                DependencyProperty.Register(nameof(LeftBottomText), typeof(string), typeof(QChart), new PropertyMetadata(null));

        public string LeftBottomText
        {
            get { return (string)GetValue(LeftBottomTextProperty); }
            set { SetValue(LeftBottomTextProperty, value); }
        }


        public static readonly DependencyProperty TimeAxisProperty =
                        DependencyProperty.Register(nameof(TimeAxis), typeof(string), typeof(QTimeAxis), new PropertyMetadata(null));

        public QTimeAxis TimeAxis
        {
            get { return (QTimeAxis)GetValue(TimeAxisProperty); }
            set { SetValue(TimeAxisProperty, value); UpdateLabels(); }
        }

        void UpdateLabels()
        {

            if (TimeAxis != null)
            {
                Labels.LeftBottomText = TimeAxis.TimeSpan.ToString(@"hh\:mm\:ss");
            }
        }


        // public string RightTopText { get; set; }

        DispatcherTimer _timer = new DispatcherTimer();
        private void TestTimer()
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += AddRnd;
            _timer.Start();
        }
        private void CanvasControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            myDraw(args.DrawingSession, sender.ActualWidth, sender.ActualHeight);
        }
        Windows.UI.Color color = Colors.Blue;
        Windows.UI.Color colorGrid = Colors.LightBlue;
        Windows.UI.Color TextColor = Colors.Gray;
        int TextHeight = 16;

        private void myDraw(CanvasDrawingSession ds, double width, double height)
        {

         //   ds.DrawEllipse(155, 115, 80, 30, Colors.Black, 3);
         //   ds.DrawText("Hello, world!", 100, 100, Colors.Yellow);

            RenderGrid(ds,(float) width, (float) height - TextHeight);

            if (DataSource != null)
            {
                CopyData();
                RenderData(ds, width, height - TextHeight);
            }

        }

        void RenderGrid(CanvasDrawingSession ds, float width, float height)
        {
            if (TimeAxis == null) return;
            DateTime now = DateTime.Now;
            DateTime t0 = now - TimeAxis.TimeSpan;
            DateTime start = t0;
            TimeSpan ts = new TimeSpan(0);
            string TimeFormat = "{0:HH:mm}";
            switch (TimeAxis.TimeSpan)
            {
                case TimeSpan t when (t.TotalMinutes < 10):
                    ts = new TimeSpan(0, 1, 0);
                    start = t0.AddSeconds(-t0.Second);
                    break;
                case TimeSpan t when (t.TotalHours < 5):
                    ts = new TimeSpan(0, 15, 0);
                    start = t0.AddMinutes(-t0.Minute);
                    break;
                case TimeSpan t when (t.TotalHours < 13):
                    ts = new TimeSpan(1, 00, 0);
                    start = t0.AddMinutes(-t0.Minute);
                    break;
                case TimeSpan t when (t.TotalHours < 25):
                    ts = new TimeSpan(2, 0, 0);
                    start = t0.AddMinutes(-t0.Minute);
                    break;

                case TimeSpan t when (t.TotalHours < (25 *8 )):
                    ts = new TimeSpan(12, 0, 0);
                    start = t0.AddMinutes(-t0.Minute);
                    start = start.AddHours(-start.Hour);
                    TimeFormat = "{0:dd HH:mm}";
                    break;
            }

          //  Debug.WriteLine("Now = {0}, Start = {1}", now, start);
            int NX = XNumber;
            int NY = 10;
            float dx = width / NX;
            float dy = height / NY;

            var format = new CanvasTextFormat()
            {
                FontFamily = "Segoe UI",
                FontSize = 12,
                HorizontalAlignment = CanvasHorizontalAlignment.Center
            };

            for (DateTime t = start; t < now; t += ts)
            {
                float x = (float)(width*(t - t0).TotalMilliseconds / TimeAxis.TimeSpan.TotalMilliseconds);
                
                ds.DrawLine(x, 0, x, height, colorGrid);
                if ((width -x)>50) {
                    string st = String.Format(TimeFormat, t);
                    ds.DrawText(st, new Vector2(x, height + 2), TextColor, format);
                }
                
            }
            /*
            for (int i = 0; i < NX; i++)
            {
                ds.DrawLine(i * dx, 0, i * dx, height, colorGrid);
            }*/
            for (int i = 0; i < NY; i++)
            {
                ds.DrawLine(0, i*dy, width, i*dy, colorGrid);
            }

            ds.DrawRectangle(0.5f, 0.5f, width-0.5f, height, color, 1.0f);
        }
        List<double> axis = new List<double>();
        List<double> data = new List<double>();
        const int MaxCount = 300;
        Random rnd = new Random();
        private void GenerateRandomData()
        {       
            for (int i = 0; i < MaxCount; i++)
            {

                data.Add(rnd.NextDouble());
            }
        }

        public void AddRnd(object sender, object e)
        {
            data.RemoveAt(0);
            data.Add(rnd.NextDouble());
            ChartCanvas.Invalidate();
        }
        double min_v;
        double max_v;
        private void CopyData()
        {
            if (DataSource == null) return;
            if (ValueName == null) return;

            if (DataSource.Count == 0) return;
            Object o = DataSource[0];

            var vp = (typeof(Measurement)).GetProperty(ValueName);
            if (vp == null) return;

            var ap = (typeof(Measurement)).GetProperty("Tik");
            if (ap == null) return;

            //object fieldValue = ap.GetValue(DataSource[0]);
            max_v = -1000;
            min_v = 1000;
            DateTime now = DateTime.Now;
            axis.Clear();
            List<double> tmp = new List<double>();
            for (int i = 0; i < DataSource.Count; i++)
            {
                o = DataSource[i];
                double v =(double)vp.GetValue(o);
                if (v > max_v) max_v = v;
                if (v < min_v) min_v = v;
                tmp.Add(v);

                DateTime tik = (DateTime)ap.GetValue(o);
                double x = (now - tik) / TimeAxis.TimeSpan;
                axis.Add(1-x);
            }
            RightTopText.Text = max_v.ToString("F1");
            RightBottomText.Text = min_v.ToString("F1");

            double dv = max_v - min_v;
            if (dv == 0) dv = 1;
            data.Clear();

            for (int i = 0; i < tmp.Count; i++)
            {
                data.Add((tmp[i] - min_v) / dv);

            }
        }

        bool renderArea = false;

        private void RenderData(CanvasDrawingSession ds, double width, double height)
        {
            if (data.Count == 0) return;
            float thickness = 1;
            float w = (float)width * data.Count / 300;
            float xs = w / data.Count;
            float x0 = (float)width - w;
            
            using (var cpb = new CanvasPathBuilder(ds))
            {
                cpb.BeginFigure(new Vector2((float)(axis[0] * width), (float)(height * (1 - data[0]))));

                for (int i = 1; i < data.Count; i++)
                {
                    //cpb.AddLine(new Vector2(i*xs + x0, (float)(height * (1 - data[i]))));
                    cpb.AddLine(new Vector2((float)(axis[i] * width), (float)(height * (1 - data[i]))));
                }

                if (renderArea)
                {
                    cpb.AddLine(new Vector2(data.Count*xs + x0, (float)height));
                    cpb.AddLine(new Vector2(x0, (float)height));
                    cpb.EndFigure(CanvasFigureLoop.Closed);
                    ds.FillGeometry(CanvasGeometry.CreatePath(cpb), color);
                }
                else
                {
                    cpb.EndFigure(CanvasFigureLoop.Open);
                    ds.DrawGeometry(CanvasGeometry.CreatePath(cpb), color, thickness);
                }
            }
        }
    }
}
