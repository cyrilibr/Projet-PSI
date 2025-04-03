using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    public static class ModuleStatistiques
    {
        public static void Lancer()
        {
            Console.Clear();
            Console.WriteLine("--- Statistiques Globales ---\n");

            Afficher("Nombre de clients", "SELECT COUNT(*) FROM Client");
            Afficher("Nombre de cuisiniers", "SELECT COUNT(*) FROM Cuisinier");
            Afficher("Nombre de commandes", "SELECT COUNT(*) FROM CommandeLivraison");
            Afficher("Moyenne des prix de commande", "SELECT AVG(Prix) FROM CommandeLivraison", "€");
            Afficher("Montant total des ventes", "SELECT SUM(Prix) FROM CommandeLivraison", "€");

            Console.WriteLine("\n--- Clients les plus dépensiers ---");
            string topClients = @"
                SELECT T.PRENOMT, T.NOMT, C.MontantTotalAchats 
                FROM Client C JOIN Tier T ON C.ID = T.ID 
                ORDER BY C.MontantTotalAchats DESC LIMIT 5";
            using (var reader = Bdd.Lire(topClients))
            {
                while (reader.Read())
                {
                    string nom = reader.GetString("NOMT");
                    string prenom = reader.GetString("PRENOMT");
                    decimal total = reader.GetDecimal("MontantTotalAchats");
                    Console.WriteLine($"{prenom} {nom} - {total:C}");
                }
                reader.Close();
            }

            Console.WriteLine("\n--- Cuisiniers les plus actifs ---");
            string topCuisiniers = @"
                SELECT T.PRENOMT, T.NOMT, C.NombreLivraisons 
                FROM Cuisinier C JOIN Tier T ON C.ID = T.ID 
                ORDER BY C.NombreLivraisons DESC LIMIT 5";
            using (var reader = Bdd.Lire(topCuisiniers))
            {
                while (reader.Read())
                {
                    string nom = reader.GetString("NOMT");
                    string prenom = reader.GetString("PRENOMT");
                    int livraisons = reader.GetInt32("NombreLivraisons");
                    Console.WriteLine($"{prenom} {nom} - {livraisons} livraisons");
                }
                reader.Close();
            }

            Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
            Console.ReadKey();
        }

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
    }
}
