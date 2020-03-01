using HT2000Viewer.Common;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace HT2000Viewer.Models
{

    public class MeasurementCollection
    {
        const int MemorySize = 1000;
        int MaxCount = 0;
        TimeSeries TimeSeries;

        public MeasurementCollection(TimeSeries ts)
        {
            Name = ts.Name;
            TimeSpan = ts.TimeSpan;
            MaxCount = (int)(TimeSpan.TotalSeconds / MemorySize);
            TimeSeries = ts;
            MeasurementData = new ObservableCollection<Measurement>(TimeSeries.Measurements);
        }

        public ObservableCollection<Measurement> MeasurementData { get; set; } = new ObservableCollection<Measurement>();
        string Name;
       
        public TimeSpan TimeSpan;
        int TikCounter = 0;
        public void Add(Measurement m)
        {
            if (TikCounter-- > 0) return;
            TimeSeries.Measurements.Add(m);
            MeasurementData.Add(m);

            TikCounter = MaxCount;
            if (MeasurementData.Count > MemorySize)
                Remove();

            Warehouse.TimeSeriesCollection.Update(TimeSeries);
        }
        public void Remove()
        {
            TimeSeries.Measurements.RemoveAt(0);
            MeasurementData.RemoveAt(0);
        }
        public void Drop()
        {
            TimeSeries.Measurements.Clear();
            Warehouse.TimeSeriesCollection.Update(TimeSeries);
            MeasurementData.Clear();
        }
    }

    public class Warehouse : Observable
    {
        public MeasurementCollection[] mc = new MeasurementCollection[6];


        public static LiteDatabase db;

        public void DropCollections()
        {
            foreach (var c in mc)
                c.Drop();
        }

        public void Rebuild()
        {
            db.Rebuild();
        }

        public MeasurementCollection BuildMeasurementCollection(string name, TimeSpan timespan)
        {
            var r = TimeSeriesCollection.Find(x => (x.Name == name));

            TimeSeries ts;
            if (r.Count<TimeSeries>() == 0)
            {
                ts = new TimeSeries
                {
                    Name = name,
                    TimeSpan = timespan,
                    Measurements = new List<Measurement>()
                };
                TimeSeriesCollection.Insert(ts);
            }
            else
                ts = r.First();
            return new MeasurementCollection(ts);
        }

        public static LiteCollection<TimeSeries> TimeSeriesCollection;

        public async Task InitDB()
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var folderPath = localFolder.Path;
            var filePath = Path.Combine(folderPath, @"ts_db.db");
           // Debug.WriteLine(filePath);

            string connectionString = String.Format(@"Filename={0}; Upgrade=true", filePath);

            try
            {
                db = new LiteDatabase(connectionString);
            }
            catch (LiteException e)
            {
             //   Debug.WriteLine(e.Message);
                db = null;
                StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
                await file.DeleteAsync();
                db = new LiteDatabase(connectionString);
            }

            TimeSeriesCollection = (LiteCollection<TimeSeries>)Warehouse.db.GetCollection<TimeSeries>("timeseries");

            mc[0] = BuildMeasurementCollection("fast_ts", new TimeSpan(0, 5, 0));
            mc[1] = BuildMeasurementCollection("normal_ts", new TimeSpan(1, 0, 0));
            mc[2] = BuildMeasurementCollection("slow_ts", new TimeSpan(12, 0, 0));
            mc[3] = BuildMeasurementCollection("quarter_ts", new TimeSpan(6, 0, 0));
            mc[4] = BuildMeasurementCollection("day_ts", new TimeSpan(24, 0, 0));
            mc[5] = BuildMeasurementCollection("week_ts", new TimeSpan(7, 0, 0, 0));

        }

        double _CO2;
        public double CO2
        {
            get => _CO2;
            set => Set(ref _CO2, value);
        }


        double _Temperature;
        public double Temperature
        {
            get => _Temperature;
            set => Set(ref _Temperature, value);
        }

        double _Humidity;
        public double Humidity
        {
            get => _Humidity;
            set => Set(ref _Humidity, value);
        }

        public void AddState(Measurement m)
        {
            Temperature = m.Temperature;
            Humidity = m.Humidity;
            CO2 = m.CO2;
 
            mc[0].Add(m);
            mc[1].Add(m);
            mc[2].Add(m);
            mc[3].Add(m);
            mc[4].Add(m);
            mc[5].Add(m);
        }
    }
        
}
