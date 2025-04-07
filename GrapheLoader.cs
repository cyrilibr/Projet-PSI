using System;
using System.Collections.Generic;
using System.Linq;
using Projet_PSI.Utils;

namespace Projet_PSI
{
    public static class GrapheLoader
    {
        public static Graphe<Station> ChargerDepuisBDD()
        {
            // Création du graphe avec une taille arbitraire.
            var graphe = new Graphe<Station>(1000);
            var stations = new Dictionary<int, Station>();

            // 1. Chargement de toutes les stations depuis la base de données.
            using (var reader = Bdd.Lire(@"
                SELECT IDStation, LibelleStation, Latitude, Longitude, CommuneNom, CommuneCode, Precedent, Suivant, TempsChangement, EstOriente 
                FROM stations_metro"))
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32("IDStation");
                    string libelle = reader.GetString("LibelleStation");
                    double lat = reader.GetDouble("Latitude");
                    double lon = reader.GetDouble("Longitude");

                    // Création d'une instance de station.
                    var station = new Station
                    {
                        ID = id,
                        Libelle = libelle,
                        Latitude = lat,
                        Longitude = lon,
                        // D'autres champs (comme CommuneNom) peuvent être ajoutés si besoin.
                    };

                    // Ajout de la station comme nœud dans le graphe.
                    graphe.AjouterNoeud(id, station);
                    stations[id] = station;
                }
                reader.Close();
            }

            // 2. Ajout des liaisons en fonction des champs Précédent et Suivant.
            using (var reader = Bdd.Lire("SELECT IDStation, Precedent, Suivant FROM stations_metro"))
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32("IDStation");
                    int precedent = reader.IsDBNull(reader.GetOrdinal("Precedent")) ? -1 : reader.GetInt32("Precedent");
                    int suivant = reader.IsDBNull(reader.GetOrdinal("Suivant")) ? -1 : reader.GetInt32("Suivant");

                    // Lien du précédent vers cette station.
                    if (precedent != -1 && stations.ContainsKey(precedent) && stations.ContainsKey(id))
                        graphe.AjouterLien(precedent, id);

                    // Lien de cette station vers la suivante.
                    if (suivant != -1 && stations.ContainsKey(suivant) && stations.ContainsKey(id))
                        graphe.AjouterLien(id, suivant);
                }
                reader.Close();
            }

            // 3. Ajout des correspondances entre les stations ayant le même libellé (même lieu physique).
            // Ces liaisons permettent de changer de ligne à l'intérieur de la même station.
            // Le poids est faible (ici 2) pour représenter un court temps de transfert.
            var groupes = stations.Values
                .GroupBy(s => s.Libelle)
                .Where(g => g.Count() > 1);

            foreach (var groupe in groupes)
            {
                var listeStations = groupe.ToList();
                for (int i = 0; i < listeStations.Count; i++)
                {
                    for (int j = i + 1; j < listeStations.Count; j++)
                    {
                        // Ajout d’un lien bidirectionnel entre les stations de correspondance.
                        graphe.AjouterLien(listeStations[i].ID, listeStations[j].ID, poids: 2);
                    }
                }
            }

            return graphe;
        }
    }
}
