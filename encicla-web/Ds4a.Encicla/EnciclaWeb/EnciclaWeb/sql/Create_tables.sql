-- DROP TABLE ds4a_encicla_schema.Zone;
CREATE TABLE ds4a_encicla_schema.Zone (
	zone_id int2 NOT NULL,
	name varchar(50) NOT NULL,
	description varchar(200),
	CONSTRAINT "PK_Zone_Zone_id" PRIMARY KEY (zone_id)
);

-- DROP TABLE ds4a_encicla_schema.Station;
CREATE TABLE ds4a_encicla_schema.Station (
	station_id int2 NOT NULL,
	name varchar(50) NOT NULL,
	description varchar(200),
	address varchar(200),
	zone_id int2,
	station_order int2,
	latitude numeric NOT NULL,
	longitude numeric NOT NULL,
	type varchar(30),
	capacity int2 not null,
	picture varchar(200),
	CONSTRAINT "PK_Station_Station_id" PRIMARY KEY (station_id)
);

ALTER TABLE ds4a_encicla_schema.Station ADD CONSTRAINT "FK_Station_Zone_Zone_id" FOREIGN KEY (zone_id) REFERENCES ds4a_encicla_schema.Zone(zone_id);

-- DROP TABLE ds4a_encicla_schema.inventory;
CREATE TABLE ds4a_encicla_schema.Inventory (
	inventory_id varchar(200) NOT NULL,
	"date" timestamp NULL,
	station_id int2 NULL,
	station_bikes int4 NULL,
	station_places int4 NULL,
	station_bikes_state varchar NULL,
	station_places_state varchar NULL,
	station_closed int4 NULL,
	station_cdo int4 NULL,
	CONSTRAINT "PK_Inventory_Inventory_id" PRIMARY KEY (inventory_id)
);

-- ALTER TABLE ds4a_encicla_schema.Inventory DROP CONSTRAINT "FK_Inventory_Station_station_id";
ALTER TABLE ds4a_encicla_schema.Inventory ADD CONSTRAINT "FK_Inventory_Station_station_id" FOREIGN KEY (station_id) REFERENCES ds4a_encicla_schema.Station(station_id);

CREATE INDEX CONCURRENTLY inventory_date_station_index ON ds4a_encicla_schema.Inventory (date, station_id);

-- DROP TABLE ds4a_encicla_schema.Weather;
CREATE TABLE ds4a_encicla_schema.Weather (
	weather_id varchar(200) NOT NULL,
	station_id int2 NOT NULL,
	date timestamp NOT NULL,
	weather_main varchar NULL,
	weather_description varchar NULL,
	main_temp_kelvin float8 NULL,
	main_feels_like_kelvin float8 NULL,
	main_temp_min_kelvin float8 NULL,
	main_temp_max_kelvin float8 NULL,
	main_pressure int4 NULL,
	main_humidity int4 NULL,
	visibility int4 NULL,
	wind_speed float8 NULL,
	wind_deg int4 NULL,
	clouds_all int4 NULL,
	CONSTRAINT "PK_Weather_Weather_id" PRIMARY KEY (weather_id)
);

-- ALTER TABLE ds4a_encicla_schema.Weather DROP CONSTRAINT "FK_Weather_Station_station_id";
ALTER TABLE ds4a_encicla_schema.Weather ADD CONSTRAINT "FK_Weather_Station_station_id" FOREIGN KEY (station_id) REFERENCES ds4a_encicla_schema.Station(station_id);

CREATE INDEX CONCURRENTLY weather_date_station_index ON ds4a_encicla_schema.Weather (date, station_id);