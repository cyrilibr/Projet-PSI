using Projet_PSI.Utils;
using System;
using System.Collections.Generic;

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

            string villeCuisinier = ObtenirVille(idCuisinier);
            string villeClient = ObtenirVille(idClient);

            if (villeCuisinier == null || villeClient == null)
            {
                Console.WriteLine("Impossible de récupérer les villes associées.");
                Console.ReadKey();
                return;
            }

            int idStationDep = TrouverStationParVille(villeCuisinier);
            int idStationArr = TrouverStationParVille(villeClient);

            if (idStationDep == -1 || idStationArr == -1)
            {
                Console.WriteLine("Aucune station trouvée correspondant à la ville.");
                Console.ReadKey();
                return;
            }

            var chemin = graphe.CheminDijkstra(idStationDep, idStationArr);

            if (chemin.Count == 0)
            {
                Console.WriteLine("Aucun chemin trouvé entre ces stations.");
            }
            else
            {
                Console.WriteLine("\n--- Chemin le plus court ---");
                foreach (var id in chemin)
                {
                    Console.WriteLine($" - {graphe.Noeuds[id].Data.Libelle}");
                }

                Console.WriteLine($"\nDistance estimée : {chemin.Count * 0.5} km");
                Console.WriteLine($"Temps estimé : {chemin.Count * 2} minutes");
            }

            Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
            Console.ReadKey();
        }

        private static string ObtenirVille(string id)
        {
            using var reader = Bdd.Lire($"SELECT VILLE FROM Tier WHERE ID = '{id}'");
            if (reader.Read())
            {
                string ville = reader.GetString("VILLE");
                reader.Close();
                return ville;
            }
            reader.Close();
            return null;
        }

        private static int TrouverStationParVille(string ville)
        {
            using var reader = Bdd.Lire($@"
                SELECT IDStation FROM stations_metro
                WHERE CommuneNom LIKE CONCAT('%', '{ville}', '%')
                LIMIT 1;");

            if (reader.Read())
            {
                int idStation = reader.GetInt32("IDStation");
                reader.Close();
                return idStation;
            }
            reader.Close();
            return -1;
        }
    }
}
