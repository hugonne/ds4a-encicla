using System;
using System.Collections.Generic;

namespace Ds4a.EnciclaWeb.Models
{
    public class TwoDimensionalPlotViewModel<TX, TY>
    {
        public TwoDimensionalPlotViewModel()
        {
        }

        public List<TX> X { get; set; }
        public List<TY> Y { get; set; }
        public string Type { get; set; }
        public string PlotName { get; set; }
    }
}
