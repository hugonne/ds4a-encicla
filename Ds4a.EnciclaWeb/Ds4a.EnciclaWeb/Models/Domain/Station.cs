using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ds4a.EnciclaWeb.Models.Domain
{
    public partial class Station
    {
        public Station()
        {
            //Inventory = new HashSet<Inventory>();
            Weather = new HashSet<Weather>();
        }

        public short StationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public short? ZoneId { get; set; }
        public short? StationOrder { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Type { get; set; }
        public short Capacity { get; set; }
        public string Picture { get; set; }

        public Zone Zone { get; set; }
        //public ICollection<Inventory> Inventory { get; set; }
        public ICollection<Weather> Weather { get; set; }

        #region Calculated Properties

        [NotMapped]
        public int CurrentBikes { get; set; }

        [NotMapped]
        public double CurrentAvailability => Capacity == 0 ? 0 : (double)CurrentBikes / Capacity;

        [NotMapped]
        public int PredictedBikes { get; set; }

        [NotMapped]
        public double PredictedAvailability => Capacity == 0 ? 0 : (double)PredictedBikes / Capacity;

        #endregion
    }
}
