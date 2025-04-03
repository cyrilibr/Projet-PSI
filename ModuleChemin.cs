using Projet_PSI.Utils;
using Projet_PSI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Projet_PSI.Modules
{
    public static class ModuleChemin
    {
        public static void Lancer()
        {
            Console.Clear();
            Console.WriteLine("--- Chemin le plus court entre deux utilisateurs ---");
            Console.Write("ID de l'expéditeur (client ou cuisinier) : "); string idExpediteur = Console.ReadLine();
            Console.Write("ID du destinataire (client ou cuisinier) : "); string idDestinataire = Console.ReadLine();

            try
            {
                string villeExp = ObtenirVille(idExpediteur);
                string villeDest = ObtenirVille(idDestinataire);

                if (villeExp == null || villeDest == null)
                {
                    Console.WriteLine("Impossible de récupérer les villes.");
                    return;
                }

                int idStationDep = TrouverStationParVille(villeExp);
                int idStationArr = TrouverStationParVille(villeDest);

                if (idStationDep == -1 || idStationArr == -1)
                {
                    Console.WriteLine("Aucune station trouvée correspondant à la ville.");
                    return;
                }

                var graphe = GrapheLoader.ChargerDepuisBDD();
                var distances = graphe.Dijkstra(idStationDep);
                var predecesseurs = graphe.CalculerPredecesseurs(idStationDep);

                Console.WriteLine($"\nDistance minimale entre stations : {distances[idStationArr]} unités de temps\n");

                List<string> chemin = new List<string>();
                int courant = idStationArr;
                while (courant != idStationDep && predecesseurs.ContainsKey(courant))
                {
                    chemin.Insert(0, graphe.Noeuds[courant].Data.Libelle);
                    courant = predecesseurs[courant];
                }
                chemin.Insert(0, graphe.Noeuds[idStationDep].Data.Libelle);

                Console.WriteLine("Stations parcourues :");
                foreach (var nom in chemin)
                    Console.WriteLine(" - " + nom);

                string cheminStr = string.Join(" -> ", chemin);
                double tempsEstime = distances[idStationArr];
                double distanceKm = chemin.Count * 0.5;

                Console.Write("\nNuméro de livraison à mettre à jour : ");
                if (int.TryParse(Console.ReadLine(), out int livraisonID))
                {
                    string req = $@"
                        UPDATE Trajet 
                        SET Distance = {distanceKm}, TempsEstime = SEC_TO_TIME({tempsEstime * 60}), CheminPris = '{cheminStr}' 
                        WHERE NumerodeLivraison = {livraisonID};";

                    Bdd.Executer(req);
                    Console.WriteLine("\nTrajet mis à jour dans la base de données.");
                }
                else
                {
                    Console.WriteLine("Entrée invalide pour le numéro de livraison.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
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
