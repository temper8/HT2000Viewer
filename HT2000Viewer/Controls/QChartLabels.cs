using HT2000Viewer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HT2000Viewer.Controls
{
    public class QChartLabels: Observable
    {
        string _LeftBottomText;
        public string LeftBottomText
        {
            get => _LeftBottomText;
            set => Set(ref _LeftBottomText, value);
        }
    }
}
