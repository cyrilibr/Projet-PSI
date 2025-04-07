﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using Projet_PSI;

namespace Projet_PSI.Utils
{
    public static class GeoUtils
    {
        /// <summary>
        /// Utilise l’API Nominatim pour géocoder une adresse et retourner ses coordonnées.
        /// </summary>
        public static (double lat, double lon)? GeocoderAdresse(string adresse)
        {
            try
            {
                using var client = new HttpClient();
                string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(adresse)}&format=json&limit=1";
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Projet_PSI/1.0");

                var response = client.GetStringAsync(url).Result;
                var result = JsonSerializer.Deserialize<List<GeoResult>>(response);

                if (result != null && result.Count > 0)
                {
                    double lat = double.Parse(result[0].lat, CultureInfo.InvariantCulture);
                    double lon = double.Parse(result[0].lon, CultureInfo.InvariantCulture);
                    return (lat, lon);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur de géocodage : " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Retourne l'ID de la station la plus proche des coordonnées fournies.
        /// </summary>
        public static int TrouverStationLaPlusProche(Graphe<Station> graphe, double lat, double lon)
        {
            double minDist = double.MaxValue;
            int idProche = -1;

            foreach (var kvp in graphe.Noeuds)
            {
                var station = kvp.Value.Data;
                double dLat = lat - station.Latitude;
                double dLon = lon - station.Longitude;
                double dist = Math.Sqrt(dLat * dLat + dLon * dLon);

                if (dist < minDist)
                {
                    minDist = dist;
                    idProche = kvp.Key;
                }
            }

            return idProche;
        }

        /// <summary>
        /// Combine géocodage et recherche de station la plus proche.
        /// </summary>
        public static int StationLaPlusProche(Graphe<Station> graphe, string adresse)
        {
            var coord = GeocoderAdresse(adresse);
            if (coord == null) return -1;

            return TrouverStationLaPlusProche(graphe, coord.Value.lat, coord.Value.lon);
        }

        private class GeoResult
        {
            public string lat { get; set; }
            public string lon { get; set; }
        }
    }
}
