using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static System.Collections.Specialized.BitVector32;

namespace Projet_PSI
{
    internal class Program
    {
        /// <summary>
        /// Point d’entrée de l’application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string connStr = "server=localhost;user=root;password=root;database=psi;";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();

            // Lecture des stations avec leurs coordonnées
            Dictionary<int, Station> stationMap = new Dictionary<int, Station>();
            var cmd = new MySqlCommand("SELECT IDStation, LibelleStation, Latitude, Longitude FROM stations_metro", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32("IDStation");
                string libelle = reader.GetString("LibelleStation");
                double latitude = reader.GetDouble("Latitude");
                double longitude = reader.GetDouble("Longitude");
                stationMap[id] = new Station { ID = id, Libelle = libelle, Latitude = latitude, Longitude = longitude };
            }
            reader.Close();

            // Création du graphe générique avec des stations
            Graphe<Station> graphe = new Graphe<Station>();
            foreach (var pair in stationMap)
            {
                Noeud<Station> noeud = new Noeud<Station>(pair.Key, pair.Value);
                graphe.AjouterNoeud(noeud);
            }

            // Lecture des liens (les liaisons pouvant être non bidirectionnelles)
            cmd = new MySqlCommand("SELECT IDStation, Precedent, Suivant, TempsChangement FROM stations_metro", conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32("IDStation");
                int? precedent = reader.IsDBNull(reader.GetOrdinal("Precedent")) ? (int?)null : reader.GetInt32("Precedent");
                int? suivant = reader.IsDBNull(reader.GetOrdinal("Suivant")) ? (int?)null : reader.GetInt32("Suivant");
                int poids = reader.IsDBNull(reader.GetOrdinal("TempsChangement")) ? 1 : reader.GetInt32("TempsChangement");

                // On suppose ici que le lien "Precedent" signifie un lien allant de la station précédente vers la station courante.
                if (precedent.HasValue && stationMap.ContainsKey(precedent.Value))
                    graphe.AjouterLien(precedent.Value, id, poids, false);

                // Le lien "Suivant" est interprété comme allant de la station courante vers la station suivante.
                if (suivant.HasValue && stationMap.ContainsKey(suivant.Value))
                    graphe.AjouterLien(id, suivant.Value, poids, false);
            }
            reader.Close();
            conn.Close();

            Application.EnableVisualStyles();
            Application.Run(new FenetreGraphe(graphe));
        }
    }
}
