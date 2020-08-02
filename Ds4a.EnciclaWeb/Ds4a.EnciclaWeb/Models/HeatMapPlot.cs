using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ds4a.EnciclaWeb.Models
{
    public class HeatMapPlot
    {
        public List<List<double>> Z { get; set; }
        public List<string> X { get; set; }
        public List<int> Y { get; set; }
        public string Type { get; set; }
        public bool Hoverongaps { get; set; }
    }
}
