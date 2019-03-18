using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HT2000Viewer.Models
{
    public class Measurement
    {
        public int Id { get; set; }
        public DateTime Tik { get; set; }
        public double CO2 { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}
