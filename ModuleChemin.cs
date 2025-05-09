using System;
using System.Linq;
using Projet_PSI.Utils;

namespace Projet_PSI.Modules
{
    public static class ModuleChemin
    {
        public static void Lancer(Graphe<Station> graphe)
        {
            Console.Clear();
            Console.WriteLine("--- Calcul de Chemin de Livraison ---\n");

            Console.Write("ID du cuisinier : ");
            string idCuisinier = Console.ReadLine();

            Console.Write("ID du client : ");
            string idClient = Console.ReadLine();

            // Récupération des adresses
            string adresseCuisinier = ObtenirAdresseComplete(idCuisinier);
            string adresseClient = ObtenirAdresseComplete(idClient);

            if (string.IsNullOrEmpty(adresseCuisinier) || string.IsNullOrEmpty(adresseClient))
            {
                Console.WriteLine("Impossible de récupérer les adresses complètes associées.");
                Console.ReadKey();
                return;
            }

            // Recherche des stations les plus proches
            int idStationDep = GeoUtils.StationLaPlusProche(graphe, adresseCuisinier);
            int idStationArr = GeoUtils.StationLaPlusProche(graphe, adresseClient);

            if (idStationDep == -1 || idStationArr == -1)
            {
                Console.WriteLine("Aucune station trouvée correspondant aux adresses fournies.");
                Console.ReadKey();
                return;
            }

            // Affichage des stations les plus proches
            Console.WriteLine($"\nStation la plus proche du cuisinier : {graphe.Noeuds[idStationDep].Data.Libelle} (ID: {idStationDep})");
            Console.WriteLine($"Station la plus proche du client : {graphe.Noeuds[idStationArr].Data.Libelle} (ID: {idStationArr})");

            // Recherche du chemin le plus court
            var chemin = graphe.CheminDijkstra(idStationDep, idStationArr);

            if (chemin.Count == 0)
            {
                Console.WriteLine("\nAucun chemin trouvé entre ces stations.");
            }
            else
            {
                Console.WriteLine("\n--- Chemin le plus court ---");
                foreach (var stationId in chemin)
                {
                    var station = graphe.Noeuds[stationId].Data;
                    Console.WriteLine($" - {station.Libelle} (Ligne : {station.LibelLigne})");
                }

                Console.WriteLine($"\nDistance estimée : {chemin.Count * 0.5} km");
                Console.WriteLine($"Temps estimé : {chemin.Count * 2} minutes");
            }

            Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
            Console.ReadKey();
        }

        private static string ObtenirAdresseComplete(string id)
        {
            string query = $"SELECT ADR, CODEPOSTAL, VILLE FROM Tier WHERE ID = '{id}'";
            using var reader = Bdd.Lire(query);
            if (reader.Read())
            {
                string adr = reader.GetString("ADR");
                string cp = reader.GetString("CODEPOSTAL");
                string ville = reader.GetString("VILLE");
                reader.Close();
                return $"{adr}, {cp} {ville}";
            }
            reader.Close();
            return null;
        }
    }
}
