using System;
using System.Collections.Generic;

namespace Ds4a.EnciclaWeb.Models
{
    public class TwoDimensionalPlot<TX, TY>
    {
        public List<TX> X { get; set; }
        public List<TY> Y { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Mode { get; set; }
    }
}
