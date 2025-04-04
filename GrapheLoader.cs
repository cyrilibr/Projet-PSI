using Projet_PSI.Utils;
using System;
using System.Collections.Generic;

namespace Projet_PSI
{
    public static class GrapheLoader
    {
        public static Graphe<Station> ChargerDepuisBDD()
        {
            var graphe = new Graphe<Station>(1000); // taille arbitraire
            var stations = new Dictionary<int, Station>();

            // Charger les stations
            using (var reader = Bdd.Lire("SELECT IDStation, LibelleStation, Latitude, Longitude FROM stations_metro"))
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32("IDStation");
                    string libelle = reader.GetString("LibelleStation");
                    double lat = reader.GetDouble("Latitude");
                    double lon = reader.GetDouble("Longitude");

                    var station = new Station
                    {
                        ID = id,
                        Libelle = libelle,
                        Latitude = lat,
                        Longitude = lon
                    };

                    graphe.AjouterNoeud(id, station);
                    stations[id] = station;
                }
                reader.Close();
            }

            // Charger les liens (liaisons entre stations)
            using (var reader = Bdd.Lire("SELECT IDStation, Precedent, Suivant FROM stations_metro"))
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32("IDStation");
                    int precedent = reader.IsDBNull(reader.GetOrdinal("Precedent")) ? -1 : reader.GetInt32("Precedent");
                    int suivant = reader.IsDBNull(reader.GetOrdinal("Suivant")) ? -1 : reader.GetInt32("Suivant");

                    if (precedent != -1 && stations.ContainsKey(precedent) && stations.ContainsKey(id))
                        graphe.AjouterLien(precedent, id);

                    if (suivant != -1 && stations.ContainsKey(suivant) && stations.ContainsKey(id))
                        graphe.AjouterLien(id, suivant);
                }
                reader.Close();
            }

            return graphe;
        }
    }
}
