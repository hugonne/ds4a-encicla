using Ds4a.EnciclaWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace Ds4a.EnciclaWeb.DataAccess
{
    public partial class EnciclaDbContext : DbContext
    {
        public EnciclaDbContext()
        {
        }

        public EnciclaDbContext(DbContextOptions<EnciclaDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Inventory> Inventories { get; set; }
        public virtual DbSet<Prediction> Predictions { get; set; }
        public virtual DbSet<Station> Stations { get; set; }
        public virtual DbSet<Weather> Weather { get; set; }
        public virtual DbSet<Zone> Zones { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(
                    "Host=ec2-54-94-126-38.sa-east-1.compute.amazonaws.com;Database=ds4a_encicla_db;Username=ds4a_encicla_user;Password=ds4a2020_encicla_user");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("inventory", "ds4a_encicla_schema");

                entity.HasIndex(e => new { e.Date, e.StationId })
                    .HasName("inventory_date_station_index");

                entity.Property(e => e.InventoryId)
                    .HasColumnName("inventory_id")
                    .HasMaxLength(200);

                entity.Property(e => e.Date).HasColumnName("date");

                entity.Property(e => e.StationBikes).HasColumnName("station_bikes");

                entity.Property(e => e.StationBikesState)
                    .HasColumnName("station_bikes_state")
                    .HasColumnType("character varying");

                entity.Property(e => e.StationCdo).HasColumnName("station_cdo");

                entity.Property(e => e.StationClosed).HasColumnName("station_closed");

                entity.Property(e => e.StationId).HasColumnName("station_id");

                entity.Property(e => e.StationPlaces).HasColumnName("station_places");

                entity.Property(e => e.StationPlacesState)
                    .HasColumnName("station_places_state")
                    .HasColumnType("character varying");

                entity.HasOne(d => d.Station)
                    .WithMany(p => p.Inventory)
                    .HasForeignKey(d => d.StationId)
                    .HasConstraintName("FK_Inventory_Station_station_id");
            });

            modelBuilder.Entity<Prediction>(entity =>
            {
                entity.ToTable("prediction", "ds4a_encicla_schema");

                entity.HasIndex(e => e.InventoryId)
                    .HasName("prediction_inventory_index");

                entity.HasIndex(e => e.PredictDate)
                    .HasName("prediction_date_index");

                entity.Property(e => e.PredictionId)
                    .HasColumnName("prediction_id")
                    .HasMaxLength(200);

                entity.Property(e => e.InventoryId)
                    .IsRequired()
                    .HasColumnName("inventory_id")
                    .HasMaxLength(200);

                entity.Property(e => e.PredictBikes).HasColumnName("predict_bikes");

                entity.Property(e => e.PredictDate).HasColumnName("predict_date");

                entity.HasOne(d => d.Inventory)
                    .WithMany(p => p.Prediction)
                    .HasForeignKey(d => d.InventoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Prediction_Inventory_inventory_id");
            });

            modelBuilder.Entity<Station>(entity =>
            {
                entity.ToTable("station", "ds4a_encicla_schema");

                entity.Property(e => e.StationId)
                    .HasColumnName("station_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasMaxLength(200);

                entity.Property(e => e.Capacity).HasColumnName("capacity");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(200);

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasColumnType("numeric");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasColumnType("numeric");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50);

                entity.Property(e => e.Picture)
                    .HasColumnName("picture")
                    .HasMaxLength(200);

                entity.Property(e => e.StationOrder).HasColumnName("station_order");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasMaxLength(30);

                entity.Property(e => e.ZoneId).HasColumnName("zone_id");
            });

            modelBuilder.Entity<Weather>(entity =>
            {
                entity.ToTable("weather", "ds4a_encicla_schema");

                entity.HasIndex(e => new { e.Date, e.StationId })
                    .HasName("weather_date_station_index");

                entity.Property(e => e.WeatherId)
                    .HasColumnName("weather_id")
                    .HasMaxLength(200);

                entity.Property(e => e.CloudsAll).HasColumnName("clouds_all");

                entity.Property(e => e.Date).HasColumnName("date");

                entity.Property(e => e.MainFeelsLikeKelvin).HasColumnName("main_feels_like_kelvin");

                entity.Property(e => e.MainHumidity).HasColumnName("main_humidity");

                entity.Property(e => e.MainPressure).HasColumnName("main_pressure");

                entity.Property(e => e.MainTempKelvin).HasColumnName("main_temp_kelvin");

                entity.Property(e => e.MainTempMaxKelvin).HasColumnName("main_temp_max_kelvin");

                entity.Property(e => e.MainTempMinKelvin).HasColumnName("main_temp_min_kelvin");

                entity.Property(e => e.StationId).HasColumnName("station_id");

                entity.Property(e => e.Visibility).HasColumnName("visibility");

                entity.Property(e => e.WeatherDescription)
                    .HasColumnName("weather_description")
                    .HasColumnType("character varying");

                entity.Property(e => e.WeatherMain)
                    .HasColumnName("weather_main")
                    .HasColumnType("character varying");

                entity.Property(e => e.WindDeg).HasColumnName("wind_deg");

                entity.Property(e => e.WindSpeed).HasColumnName("wind_speed");

                entity.HasOne(d => d.Station)
                    .WithMany(p => p.Weather)
                    .HasForeignKey(d => d.StationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Weather_Station_station_id");
            });

            modelBuilder.Entity<Zone>(entity =>
            {
                entity.ToTable("zone", "ds4a_encicla_schema");

                entity.Property(e => e.ZoneId)
                    .HasColumnName("zone_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(200);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
