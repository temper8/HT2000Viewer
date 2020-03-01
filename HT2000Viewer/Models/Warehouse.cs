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
        public MeasurementCollection(string name, TimeSpan ts)
        {
            Name = name;
            TimeSpan = ts;
            MaxCount = (int)(ts.TotalSeconds / MemorySize);
            // Get a collection (or create, if doesn't exist)
            LiteCollection = (LiteCollection<Measurement>) Warehouse.db.GetCollection<Measurement>(Name);

            LiteCollection.EnsureIndex(x => x.Tik, true);
            MeasurementData = new ObservableCollection<Measurement>(LiteCollection.FindAll());

        }
        public ObservableCollection<Measurement> MeasurementData { get; set; } = new ObservableCollection<Measurement>();
        string Name;
        LiteCollection<Measurement> LiteCollection;
        public TimeSpan TimeSpan;
        int TikCounter = 0;
        public void Add(Measurement m)
        {
            if (TikCounter-- > 0) return;
            LiteCollection.Insert(m);
            MeasurementData.Add(m);

            TikCounter = MaxCount;
            if (MeasurementData.Count > MemorySize)
                Remove();
        }
        public void Remove()
        {
            Measurement old = MeasurementData[0];
            LiteCollection.Delete(old.Id);
            MeasurementData.RemoveAt(0);
        }
        public void Drop()
        {
            Warehouse.db.DropCollection(Name);
            LiteCollection = (LiteCollection<Measurement>)Warehouse.db.GetCollection<Measurement>(Name);
            LiteCollection.EnsureIndex(x => x.Tik, true);
            MeasurementData = new ObservableCollection<Measurement>();
        }
    }

    public class Warehouse : Observable
    {
        public MeasurementCollection[] mc = new MeasurementCollection[6];

        public Warehouse()
        {

        }

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



        public async Task InitDB()
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var folderPath = localFolder.Path;
            var filePath = Path.Combine(folderPath, @"ts.db");
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

            mc[0] = new MeasurementCollection("fast_ts", new TimeSpan(0, 5, 0));
            mc[1] = new MeasurementCollection("normal_ts", new TimeSpan(1, 0, 0));
            mc[2] = new MeasurementCollection("slow_ts", new TimeSpan(12, 0, 0));
            mc[3] = new MeasurementCollection("quarter_ts", new TimeSpan(6, 0, 0));
            mc[4] = new MeasurementCollection("day_ts", new TimeSpan(24, 0, 0));
            mc[5] = new MeasurementCollection("week_ts", new TimeSpan(7, 0, 0, 0));

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

        int RCounter = 600;
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
            Debug.WriteLine(RCounter);
            if (RCounter-- <0)
            {
                RCounter = 600;
                db.Rebuild();
            }
        }
    }
        
}
