using System;
using System.Collections.Generic;

namespace Ds4a.EnciclaWeb.Models.Domain
{
    public partial class Inventory
    {
        public Inventory()
        {
            Prediction = new HashSet<Prediction>();
        }

        public string InventoryId { get; set; }
        public DateTime? Date { get; set; }
        public short? StationId { get; set; }
        public int? StationBikes { get; set; }
        public int? StationPlaces { get; set; }
        public string StationBikesState { get; set; }
        public string StationPlacesState { get; set; }
        public int? StationClosed { get; set; }
        public int? StationCdo { get; set; }

        public Station Station { get; set; }
        public ICollection<Prediction> Prediction { get; set; }
        public ICollection<PredictionOneHour> PredictionOneHour { get; set; }
    }
}
