using System;
using System.Collections.Generic;

namespace Ds4a.EnciclaWeb.Models
{
    public partial class Prediction
    {
        public string PredictionId { get; set; }
        public string InventoryId { get; set; }
        public DateTime PredictDate { get; set; }
        public int? PredictBikes { get; set; }

        public Inventory Inventory { get; set; }
    }
}
