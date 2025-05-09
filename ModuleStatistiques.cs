using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    /// <summary>
    /// Module permettant d'afficher diverses statistiques liées aux clients, cuisiniers et commandes.
    /// </summary>
    public static class ModuleStatistiques
    {
        /// <summary>
        /// Lance le menu interactif des statistiques.
        /// </summary>
        public static void Lancer()
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine("--- Statistiques ---\n");
                Console.WriteLine("1. Nombre de clients");
                Console.WriteLine("2. Nombre de cuisiniers");
                Console.WriteLine("3. Nombre de commandes");
                Console.WriteLine("4. Moyenne des prix de commande");
                Console.WriteLine("5. Montant total des ventes");
                Console.WriteLine("6. Commandes sur une période");
                Console.WriteLine("7. Moyenne des comptes clients");
                Console.WriteLine("8. Commandes d’un client par nationalité et période");
                Console.WriteLine("9. Top 5 clients les plus dépensiers");
                Console.WriteLine("10. Top 5 cuisiniers les plus actifs");
                Console.WriteLine("0. Retour");
                Console.Write("\nChoix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1": Afficher("Nombre de clients", "SELECT COUNT(*) FROM Client"); break;
                    case "2": Afficher("Nombre de cuisiniers", "SELECT COUNT(*) FROM Cuisinier"); break;
                    case "3": Afficher("Nombre de commandes", "SELECT COUNT(*) FROM CommandeLivraison"); break;
                    case "4": Afficher("Moyenne des prix de commande", "SELECT AVG(Prix) FROM CommandeLivraison", "€"); break;
                    case "5": Afficher("Montant total des ventes", "SELECT SUM(Prix) FROM CommandeLivraison", "€"); break;
                    case "6": CommandesParPeriode(); break;
                    case "7": MoyenneMontantClients(); break;
                    case "8": CommandesClientParNationalite(); break;
                    case "9": TopClients(); break;
                    case "10": TopCuisiniers(); break;
                    case "0": retour = true; break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }

                if (!retour)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Exécute une requête simple et affiche le résultat avec un titre.
        /// </summary>
        /// <param name="titre">Titre affiché avant la valeur</param>
        /// <param name="requete">Requête SQL à exécuter</param>
        /// <param name="suffixe">Unité ou symbole à ajouter à la fin</param>
        private static void Afficher(string titre, string requete, string suffixe = "")
        {
            using var reader = Bdd.Lire(requete);
            if (reader.Read())
            {
                object valeur = reader.GetValue(0);
                Console.WriteLine($"{titre} : {valeur}{suffixe}");
            }
            reader.Close();
        }

        /// <summary>
        /// Affiche le nombre de commandes entre deux dates données.
        /// </summary>
        private static void CommandesParPeriode()
        {
            Console.Write("Date début (yyyy-mm-dd) : ");
            string debut = Console.ReadLine();
            Console.Write("Date fin (yyyy-mm-dd) : ");
            string fin = Console.ReadLine();

            string requete = $@"
                SELECT COUNT(*) FROM CommandeLivraison
                WHERE DateDeLivraisonSouhaitee BETWEEN '{debut}' AND '{fin}'";

            using var reader = Bdd.Lire(requete);
            if (reader.Read())
            {
                int nb = reader.GetInt32(0);
                Console.WriteLine($"Nombre de commandes entre {debut} et {fin} : {nb}");
            }
            reader.Close();
        }

        /// <summary>
        /// Calcule la moyenne du montant total dépensé par les clients.
        /// </summary>
        private static void MoyenneMontantClients()
        {
            string requete = "SELECT AVG(MontantTotalAchats) FROM Client";
            using var reader = Bdd.Lire(requete);
            if (reader.Read())
            {
                decimal moyenne = reader.GetDecimal(0);
                Console.WriteLine($"Moyenne du montant total des clients : {moyenne:C}");
            }
            reader.Close();
        }

        /// <summary>
        /// Affiche les commandes passées par un client donné selon sa nationalité et une période.
        /// </summary>
        private static void CommandesClientParNationalite()
        {
            Console.Write("ID du client : ");
            string id = Console.ReadLine();
            Console.Write("Date début (yyyy-mm-dd) : ");
            string debut = Console.ReadLine();
            Console.Write("Date fin (yyyy-mm-dd) : ");
            string fin = Console.ReadLine();

            string requete = $@"
                SELECT M.Nationalite, CL.NumerodeLivraison, CL.DateDeLivraisonSouhaitee
                FROM CommandeLivraison CL
                JOIN Contient C ON C.NumerodeLivraison = CL.NumerodeLivraison
                JOIN Mets M ON M.ID = C.ID_Mets
                JOIN Reçoit R ON R.NumerodeLivraison = CL.NumerodeLivraison
                WHERE R.ID_Client = '{id}' AND CL.DateDeLivraisonSouhaitee BETWEEN '{debut}' AND '{fin}'
                ORDER BY M.Nationalite, CL.DateDeLivraisonSouhaitee";

            using var reader = Bdd.Lire(requete);
            Console.WriteLine($"\nCommandes du client {id} entre {debut} et {fin} :\n");

            while (reader.Read())
            {
                string nat = reader.GetString("Nationalite");
                DateTime date = reader.GetDateTime("DateDeLivraisonSouhaitee");
                int num = reader.GetInt32("NumerodeLivraison");
                Console.WriteLine($"Commande {num} - {nat} - {date:yyyy-MM-dd}");
            }

            reader.Close();
        }

        /// <summary>
        /// Affiche le top 5 des clients ayant dépensé le plus.
        /// </summary>
        private static void TopClients()
        {
            Console.WriteLine("\n--- Clients les plus dépensiers ---");
            string topClients = @"
                SELECT T.PRENOMT, T.NOMT, C.MontantTotalAchats 
                FROM Client C JOIN Tier T ON C.ID = T.ID 
                ORDER BY C.MontantTotalAchats DESC LIMIT 5";

            using var reader = Bdd.Lire(topClients);
            while (reader.Read())
            {
                string nom = reader.GetString("NOMT");
                string prenom = reader.GetString("PRENOMT");
                decimal total = reader.GetDecimal("MontantTotalAchats");
                Console.WriteLine($"{prenom} {nom} - {total:C}");
            }
            reader.Close();
        }

        /// <summary>
        /// Affiche le top 5 des cuisiniers ayant effectué le plus de livraisons.
        /// </summary>
        private static void TopCuisiniers()
        {
            Console.WriteLine("\n--- Cuisiniers les plus actifs ---");
            string topCuisiniers = @"
                SELECT T.PRENOMT, T.NOMT, C.NombreLivraisons 
                FROM Cuisinier C JOIN Tier T ON C.ID = T.ID 
                ORDER BY C.NombreLivraisons DESC LIMIT 5";

            using var reader = Bdd.Lire(topCuisiniers);
            while (reader.Read())
            {
                string nom = reader.GetString("NOMT");
                string prenom = reader.GetString("PRENOMT");
                int livraisons = reader.GetInt32("NombreLivraisons");
                Console.WriteLine($"{prenom} {nom} - {livraisons} livraisons");
            }
            reader.Close();
        }
    }
}
