"""
Routes and views for the flask application.
"""

from datetime import datetime
from flask import render_template
from flask import Markup
from EnciclaWeb import app

import os
import pandas as pd
import numpy as np
# Required for basic python plotting functionality
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import matplotlib.dates as mdates
import urllib.parse
import io
import base64
import folium  #needed for interactive map
from folium.plugins import HeatMap
import psycopg2
import time

# Connection parameters, yours will be different
param_dic = {
    "host"      : "ec2-54-94-126-38.sa-east-1.compute.amazonaws.com",
    "database"  : "ds4a_encicla_db",
    "user"      : "ds4a_encicla_user",
    "password"  : "ds4a2020_encicla_user"
}
def connect(params_dic):
    """ Connect to the PostgreSQL database server """
    conn = None
    try:
        # connect to the PostgreSQL server
        conn = psycopg2.connect(**params_dic)
    except (Exception, psycopg2.DatabaseError) as error:
        print(error)
    return conn


def postgresql_to_dataframe(param_dic, select_query, column_names):
    """
    Tranform a SELECT query into a pandas dataframe
    """
    conn = connect(param_dic)
    cursor = conn.cursor()
    try:
        cursor.execute(select_query)
    except (Exception, psycopg2.DatabaseError) as error:
        print("Error: %s" % error)
        cursor.close()
        return 1

    # Naturally we get a list of tupples
    tupples = cursor.fetchall()
    cursor.close()

    # We just need to turn it into a pandas dataframe
    df = pd.DataFrame(tupples, columns=column_names)
    conn.close()
    return df

df = pd.read_csv("EnciclaWeb/data/inventory.csv", encoding='ISO-8859-1')
df['YYYY'] = df['Date'].str[:4]
df['MM'] = df['Date'].str[5:7]
df['DD'] = df['Date'].str[5:7]
df['TIME'] = df['Date'].str[11:16]
df['datetime'] = pd.to_datetime(df['YYYY'] + '-' + df['MM'] + '-' + df['DD'] + ' ' + df['TIME'], format='%Y-%m-%d %H:%M')
df['HOUR'] = pd.DatetimeIndex(df['datetime']).hour

query_stations = "SELECT s.station_id, s.name, s.latitude, s.longitude, s.picture, z.description as zone, case when i.station_bikes is null then 0 else cast(i.station_bikes as FLOAT)/cast(s.capacity as FLOAT) end as perc_bikes, i.station_bikes, (select weather_main from ds4a_encicla_schema.weather w where w.station_id = s.station_id and w.date = (select max(w2.date) from ds4a_encicla_schema.weather w2 where w2.station_id = s.station_id)) as weather FROM ds4a_encicla_schema.zone z, ds4a_encicla_schema.station s left join ds4a_encicla_schema.inventory i on (s.station_id = i.station_id) WHERE s.zone_id = z.zone_id and i.date = (select max(date) from ds4a_encicla_schema.inventory)"
stations = postgresql_to_dataframe(param_dic, query_stations, ["station_id", "name", "latitude", "longitude", "picture", "zone", "perc_bikes", "station_bikes", "weather"])
    
@app.route('/')
@app.route('/home')
def home():
    """Renders the home page."""
    return render_template('index.html',
        title='Home Page',
        year=datetime.now().year)

@app.route('/stationDetails', defaults={'id': None})
@app.route('/stationDetails/<id>')
def stationDetails(id):
    print(id)
    """Renders the plot test page."""
    # df_temp = df[['HOUR', 'Station_name', 'Station_bikes']][df['Station_bikes'] == 0].groupby('HOUR').agg({'Station_name':'size'}).reset_index()
    # df_temp.columns = ['HOUR', 'Stations_without_bikes']
    # plt.plot(df_temp['HOUR'], df_temp['Stations_without_bikes'])
    # img = io.BytesIO()  # create the buffer
    # plt.savefig(img, format='png')  # save figure to the buffer
    # img.seek(0)  # rewind your buffer
    # plot_data = urllib.parse.quote(base64.b64encode(img.read()).decode()) # base64 encode & URL-escape

    # query_av_station = "select i.station_id, i.date, i.station_bikes from ds4a_encicla_schema.inventory i where i.date > CURRENT_DATE"
    query_av_station = "select i.station_id, i.date, i.station_bikes from ds4a_encicla_schema.inventory i where i.date > (CURRENT_DATE - 1) and i.station_id = " + id
    availability_station = postgresql_to_dataframe(param_dic, query_av_station, ["station_id", "date", "station_bikes"])
    print(availability_station)
    # av_station = availability_station[availability_station['station_id'] == stations['station_id'][int(id)]].copy()

    fig, ax = plt.subplots(constrained_layout=True)
    locator = mdates.AutoDateLocator()
    formatter = mdates.ConciseDateFormatter(locator)
    ax.xaxis.set_major_locator(locator)
    ax.xaxis.set_major_formatter(formatter)
    ax.plot(availability_station['date'], availability_station['station_bikes'])
    ax.set_title('Availability by Hour')
    img = io.BytesIO()  # create the buffer
    plt.savefig(img, format='png')  # save figure to the buffer
    img.seek(0)  # rewind your buffer
    plot_data = urllib.parse.quote(base64.b64encode(img.read()).decode())  # base64 encode & URL-escape
    plt.close()
    # popup_station += "    <br><img src='data:image/png;base64, " + plot_data + "' width='350' class='img-fluid'>"

    return render_template('station-details.html',
        title='Some Plots',
        # inventories = df_temp.head(10).to_html(classes='table table-striped'),
        plot_url = plot_data,
        year=datetime.now().year)

@app.route('/stationMap')
def stationMap():
    # query_stations = "SELECT s.station_id, s.name, s.latitude, s.longitude, s.picture, z.description as zone, case when i.station_bikes is null then 0 else cast(i.station_bikes as FLOAT)/cast(s.capacity as FLOAT) end as perc_bikes, (select weather_main from ds4a_encicla_schema.weather w where w.station_id = s.station_id and w.date = (select max(w2.date) from ds4a_encicla_schema.weather w2 where w2.station_id = s.station_id)) as weather FROM ds4a_encicla_schema.zone z, ds4a_encicla_schema.station s left join ds4a_encicla_schema.inventory i on (s.station_id = i.station_id) WHERE s.zone_id = z.zone_id and i.date = (select max(date) from ds4a_encicla_schema.inventory)"
    # stations = postgresql_to_dataframe(param_dic, query_stations, ["station_id", "name", "latitude", "longitude", "picture", "zone", "perc_bikes", "weather"])
    folium_map = folium.Map(location=[stations["latitude"].mean(), stations["longitude"].mean()],
                            zoom_start=13,
                            tiles="OpenStreetMap")

    for i in stations.index:
        perc_bikes = 1 if stations["perc_bikes"][i] > 1 else stations["perc_bikes"][i]
        color = 'black' if perc_bikes <= 0.10 else 'red' if (perc_bikes > 0.1 and perc_bikes <= 0.33) else 'orange' if (perc_bikes > 0.33 and perc_bikes <= 0.66) else 'green'
        weather = stations["weather"][i]
        weather_icon = 'glyphicon-certificate' if weather == 'Clear' else 'glyphicon-flash' if weather == 'Thunderstorm' else 'glyphicon-cloud-download' if (weather == 'Rain' or weather == 'Drizzle') else 'glyphicon-cloud'
        weather_color = 'yellow' if weather == 'Clear' else 'blue'

        popup_station = "<div class='row' style='width: 300px'>"
        popup_station += "  <div class='col-md-6'>"
        popup_station += "    <b>Name:</b> " + stations["name"][i]
        popup_station += "    <br><b>Latitude:</b> " + str(stations["latitude"][i])
        popup_station += "    <br><b>Longitude:</b> " + str(stations["longitude"][i])
        popup_station += "    <br><b>Zone:</b> " + str(stations["zone"][i])
        popup_station += "    <br><b>Weather:</b> <font color='" + weather_color + "'><i class='glyphicon " + weather_icon + "'></i></font> " + weather
        popup_station += "    <br><b>Availability:</b> <b><font color='" + color + "'>" + str(stations["station_bikes"][i]) + " (" + str(round(perc_bikes * 100, 2)) + "%)</font></b>"
        popup_station += "  </div>"  # column
        popup_station += "  <div class='col-md-6'>"
        popup_station += "    <br><b>Picture:</b><br><img src='" + str(stations["picture"][i]) + "' class='img-fluid' width='120'>"
        popup_station += "   </div>"  # column
        popup_station += "</div>"  # row
        popup_station += "<div class='row'>"
        popup_station += "  <div class='col-md-12'>"
        popup_station += "    <br><b>Availability prediction (1 hour):</b> <b><font color=''>%</font></b>"
        popup_station += "  </div>"  # column
        popup_station += "</div>"  # row
        popup_station += "<div class='row'>"
        popup_station += "  <div class='col-md-12'>"
        popup_station += "    <br><a href='/stationDetails/" + str(i) + "'>Click here for more details</a>"
        popup_station += "  </div>"  # column
        popup_station += "</div>"  # row
        popup_station += "<div class='row'>"
        ##
        
        popup_station += "  </div>"  #column
        popup_station += "</div>"  #row
        popup_complete = folium.Popup(html=popup_station)

        folium.Marker([stations["latitude"][i], stations["longitude"][i]], 
                      popup=popup_complete, 
                      icon=folium.Icon(color=color, icon='info-sign'),
                      show=True).add_to(folium_map)

    # first, force map to render as HTML, for us to dissect
    _ = folium_map._repr_html_()

    # get definition of map in body
    map_div = Markup(folium_map.get_root().html.render())

    # html to be included in header
    hdr_txt = Markup(folium_map.get_root().header.render())

    # html to be included in <script>
    script_txt = Markup(folium_map.get_root().script.render())

    return render_template('station-map.html',
        title='Station Map',
        map_div = Markup(map_div),
        hdr_txt = Markup(hdr_txt),
        script_txt = Markup(script_txt),
        year=datetime.now().year,
        message='')

@app.route('/contact')
def contact():
    """Renders the contact page."""
    return render_template('contact.html',
        title='Contact',
        year=datetime.now().year,
        message='Your contact page.')

@app.route('/about')
def about():
    """Renders the about page."""
    return render_template('about.html',
        title='About',
        year=datetime.now().year,
        message='Your application description page.')

