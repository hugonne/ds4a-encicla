CREATE OR REPLACE FUNCTION public.log_inventory_inserts()
  RETURNS trigger
  LANGUAGE 'plpgsql'
AS $$
BEGIN
	 INSERT INTO ds4a_encicla_schema.inventory (inventory_id, "date", station_id, station_bikes, station_places, station_bikes_state, station_places_state, station_closed, station_cdo)
     VALUES(substring(NEW.date, 1, 10)||'_'||substring(NEW.date, 12, 8)||'_'||NEW.station_id,
            TO_TIMESTAMP(substring(NEW.date, 1, 10)||' '||substring(NEW.date, 12, 8),'YYYY-MM-DD HH24:MI:SS'),
            NEW.station_id,
            NEW.station_bikes,
            NEW.station_places,
            NEW.station_bikes_state,
            NEW.station_places_state,
            NEW.station_closed,
            NEW.station_cdo);
    RETURN NEW;
END;
$$
;

-- DROP TRIGGER inventory_changes ON public.inventory;
CREATE TRIGGER inventory_changes
  AFTER INSERT
  ON public.inventory
  FOR EACH ROW
  EXECUTE PROCEDURE log_inventory_inserts();

CREATE OR REPLACE FUNCTION public.log_weather_inserts()
  RETURNS trigger
  LANGUAGE 'plpgsql'
AS $$
BEGIN
        BEGIN
            INSERT INTO ds4a_encicla_schema.weather (weather_id, station_id, "date", weather_main, weather_description, main_temp_kelvin, main_feels_like_kelvin, main_temp_min_kelvin, main_temp_max_kelvin, main_pressure, main_humidity, visibility, wind_speed, wind_deg, clouds_all)
            VALUES(substring(NEW.datetime, 1, 10)||' '||substring(NEW.datetime, 12, 8)||'_'||NEW."﻿Station_id",
                   NEW."﻿Station_id",
                   TO_TIMESTAMP(substring(NEW.datetime, 1, 10)||' '||substring(NEW.datetime, 12, 8),'YYYY-MM-DD HH24:MI:SS'),
                   NEW.weather_main,
                   NEW.weather_description,
                   NEW.main_temp_kelvin,
                   NEW.main_feels_like_kelvin,
                   NEW.main_temp_min_kelvin,
                   NEW.main_temp_max_kelvin,
                   NEW.main_pressure,
                   NEW.main_humidity,
                   NEW.visibility,
                   NEW.wind_speed,
                   NEW.wind_deg,
                   NEW.clouds_all);
        EXCEPTION WHEN unique_violation THEN NULL; --ignore error
        END;
        RETURN NEW;
END;
$$
;

-- DROP TRIGGER weather_changes ON public.weather;
CREATE TRIGGER weather_changes
  AFTER INSERT
  ON public.weather
  FOR EACH ROW
  EXECUTE PROCEDURE log_weather_inserts();
