using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HT2000Viewer.Controls
{
    public class QTimeAxis
    {
        public QTimeAxis(TimeSpan ts) {
            TimeSpan = ts;
        }
        public TimeSpan TimeSpan { get; }
    }
}
