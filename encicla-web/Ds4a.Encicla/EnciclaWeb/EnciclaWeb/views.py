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
import urllib.parse
import io
import base64
import folium  #needed for interactive map
from folium.plugins import HeatMap
import psycopg2

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
    print("connecting...")
    conn = connect(param_dic)
    cursor = conn.cursor()
    try:
        print("excecuting query...")
        cursor.execute(select_query)
        print("finished query...")
    except (Exception, psycopg2.DatabaseError) as error:
        print("Error: %s" % error)
        cursor.close()
        return 1

    # Naturally we get a list of tupples
    tupples = cursor.fetchall()
    cursor.close()

    # We just need to turn it into a pandas dataframe
    print("query to pandas...")
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

@app.route('/')
@app.route('/home')
def home():
    """Renders the home page."""
    return render_template('index.html',
        title='Home Page',
        year=datetime.now().year)

@app.route('/plotTest')
def plotTest():
    """Renders the plot test page."""
    df_temp = df[['HOUR', 'Station_name', 'Station_bikes']][df['Station_bikes'] == 0].groupby('HOUR').agg({'Station_name':'size'}).reset_index()
    df_temp.columns = ['HOUR', 'Stations_without_bikes']
    plt.plot(df_temp['HOUR'], df_temp['Stations_without_bikes'])
    img = io.BytesIO()  # create the buffer
    plt.savefig(img, format='png')  # save figure to the buffer
    img.seek(0)  # rewind your buffer
    plot_data = urllib.parse.quote(base64.b64encode(img.read()).decode()) # base64 encode & URL-escape
    return render_template('plot-test.html',
        title='Some Plots',
        inventories = df_temp.head(10).to_html(classes='table table-striped'),
        plot_url = plot_data,
        year=datetime.now().year)

@app.route('/mapTest')
def mapTest():
    query_stations = "SELECT s.name, s.latitude, s.longitude, s.picture, z.description as zone, case when i.station_bikes is null then 0 else cast(i.station_bikes as FLOAT)/cast(s.capacity as FLOAT) end as perc_bikes FROM ds4a_encicla_schema.zone z, ds4a_encicla_schema.station s left join ds4a_encicla_schema.inventory i on (s.station_id = i.station_id) WHERE s.zone_id = z.zone_id and i.date = (select max(date) from ds4a_encicla_schema.inventory)"
    stations = postgresql_to_dataframe(param_dic, query_stations, ["name", "latitude", "longitude", "picture", "zone", "perc_bikes"])
    folium_map = folium.Map(location=[stations["latitude"].mean(), stations["longitude"].mean()],
                            zoom_start=13,
                            tiles="OpenStreetMap")

    for i in stations.index:
        perc_bikes = 1 if stations["perc_bikes"][i] > 1 else stations["perc_bikes"][i]
        color = 'black' if perc_bikes <= 0.10 else 'red' if (perc_bikes > 0.1 and perc_bikes <= 0.33) else 'orange' if (perc_bikes > 0.33 and perc_bikes <= 0.66) else 'green'
        popup_station = "<b>Name:</b> " + stations["name"][i]
        popup_station += "<br><b>Latitude:</b> " + str(stations["latitude"][i])
        popup_station += "<br><b>Longitude:</b> " + str(stations["longitude"][i])
        popup_station += "<br><b>Zone:</b> " + str(stations["zone"][i])
        popup_station += "<br><b>Disponibility:</b> <b><font color='" + color + "'>" + str(round(perc_bikes*100,2)) + "%</font></b>"
        popup_station += "<br><b>Picture:</b> <img src='" + str(stations["picture"][i]) + "'>"
        folium.Marker([stations["latitude"][i], stations["longitude"][i]], popup=popup_station, icon=folium.Icon(color=color, icon='info-sign')).add_to(folium_map)

    # first, force map to render as HTML, for us to dissect
    _ = folium_map._repr_html_()

    # get definition of map in body
    map_div = Markup(folium_map.get_root().html.render())

    # html to be included in header
    hdr_txt = Markup(folium_map.get_root().header.render())

    # html to be included in <script>
    script_txt = Markup(folium_map.get_root().script.render())

    return render_template('map-test.html',
        title='Station map',
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

