using System;
using System.ComponentModel.DataAnnotations;

namespace Ds4a.EnciclaWeb.Models.Domain
{
    public partial class PredictionOneHour
    {
        [Key]
        public string PredictionId { get; set; }
        public string InventoryId { get; set; }
        public DateTime PredictDate { get; set; }
        public int? PredictBikes { get; set; }

        public Inventory Inventory { get; set; }
    }
}
