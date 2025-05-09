// Projet_PSI/Modules/ModuleChemin.cs
using System;
using System.Linq;
using System.Windows.Forms;
using Projet_PSI.Utils;

namespace Projet_PSI.Modules
{
    /// <summary>
    /// Module responsable du calcul et de l'affichage du chemin de livraison
    /// entre un cuisinier et un client via le réseau de stations.
    /// </summary>
    public static class ModuleChemin
    {
        /// <summary>
        /// Lance la procédure de calcul du chemin de livraison.
        /// </summary>
        /// <param name="graphe">Le graphe des stations utilisé pour le calcul.</param>
        public static void Lancer(Graphe<Station> graphe)
        {
            Console.Clear();
            Console.WriteLine("--- Calcul de Chemin de Livraison ---\n");

            Console.Write("ID du cuisinier : ");
            string idCuisinier = Console.ReadLine();

            Console.Write("ID du client : ");
            string idClient = Console.ReadLine();

            string adresseCuisinier = ObtenirAdresseComplete(idCuisinier);
            string adresseClient = ObtenirAdresseComplete(idClient);

            if (string.IsNullOrEmpty(adresseCuisinier) || string.IsNullOrEmpty(adresseClient))
            {
                Console.WriteLine("Impossible de récupérer les adresses complètes associées.");
                Console.ReadKey();
                return;
            }

            int idStationDep = GeoUtils.StationLaPlusProche(graphe, adresseCuisinier);
            int idStationArr = GeoUtils.StationLaPlusProche(graphe, adresseClient);

            if (idStationDep == -1 || idStationArr == -1)
            {
                Console.WriteLine("Aucune station trouvée correspondant aux adresses fournies.");
                Console.ReadKey();
                return;
            }

            var chemin = graphe.CheminDijkstra(idStationDep, idStationArr);

            if (chemin.Count == 0)
            {
                Console.WriteLine("\nAucun chemin trouvé entre ces stations.");
                Console.ReadKey();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var fen = new FenetreGraphe<Station>(graphe, idStationDep, idStationArr))
            {
                fen.ShowDialog();
            }
        }

        /// <summary>
        /// Récupère l'adresse complète (rue, code postal, ville) pour un utilisateur.
        /// </summary>
        /// <param name="id">Identifiant de l'utilisateur (cuisinier ou client).</param>
        /// <returns>Chaîne formatée "Adresse, CodePostal Ville" ou null si non trouvé.</returns>
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
