using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    public static class ModuleCuisinier
    {
        // === MENU ADMIN ===
        public static void Lancer()
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine("--- Module Cuisinier (admin) ---");
                Console.WriteLine("1. Afficher tous les cuisiniers");
                Console.WriteLine("2. Ajouter un cuisinier");
                Console.WriteLine("3. Supprimer un cuisinier");
                Console.WriteLine("4. Modifier la zone de livraison");
                Console.WriteLine("0. Retour");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1": Afficher(); break;
                    case "2": Ajouter(); break;
                    case "3": Supprimer(); break;
                    case "4": Modifier(); break;
                    case "0": retour = true; break;
                    default: Console.WriteLine("Choix invalide."); break;
                }

                if (!retour)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            }
        }

        // === MENU CUISINIER CONNECTE ===
        public static void LancerCuisinier(Graphe<Station> graphe)
        {
            ModuleCuisinierConnecte.Lancer(graphe);
        }

        private static void Afficher()
        {
            Console.Clear();
            Console.WriteLine("--- Liste des Cuisiniers ---\n");

            string requete = @"
                SELECT T.ID, T.PRENOMT, T.NOMT, T.VILLE, C.Note, C.ZoneLivraison
                FROM Cuisinier C JOIN Tier T ON C.ID = T.ID;";

            using var reader = Bdd.Lire(requete);
            while (reader.Read())
            {
                int id = reader.GetInt32("ID");
                string prenom = reader.GetString("PRENOMT");
                string nom = reader.GetString("NOMT");
                string ville = reader.GetString("VILLE");
                string note = reader.GetString("Note");
                string zone = reader.GetString("ZoneLivraison");

                Console.WriteLine($"{id} - {prenom} {nom} ({ville}) - Note: {note} - Zone: {zone}");
            }
            reader.Close();
        }

        private static void Ajouter()
        {
            Console.Clear();
            Console.WriteLine("--- Ajouter un cuisinier ---");

            Console.Write("Mot de passe : "); string mdp = Console.ReadLine();
            Console.Write("Nom : "); string nom = Console.ReadLine();
            Console.Write("Prénom : "); string prenom = Console.ReadLine();
            Console.Write("Adresse : "); string adr = Console.ReadLine();
            Console.Write("Code postal : "); string cp = Console.ReadLine();
            Console.Write("Ville : "); string ville = Console.ReadLine();
            Console.Write("Email : "); string email = Console.ReadLine();
            Console.Write("Téléphone : "); string tel = Console.ReadLine();
            Console.Write("Note : "); string note = Console.ReadLine();
            Console.Write("Zone de livraison : "); string zone = Console.ReadLine();

            try
            {
                string reqTier = $@"
                    INSERT INTO Tier (MDP, NOMT, PRENOMT, ADR, CODEPOSTAL, VILLE, EMAIL, TEL, Radiation, Retour)
                    VALUES ('{mdp}', '{nom}', '{prenom}', '{adr}', '{cp}', '{ville}', '{email}', '{tel}', FALSE, '');";
                Bdd.Executer(reqTier);

                int idGenere = 0;
                using (var reader = Bdd.Lire("SELECT LAST_INSERT_ID() AS ID"))
                {
                    if (reader.Read())
                        idGenere = reader.GetInt32("ID");
                    reader.Close();
                }

                string reqCuisinier = $@"
                    INSERT INTO Cuisinier (ID, Note, CuisinierDuMois, NombreLivraisons, ZoneLivraison)
                    VALUES ({idGenere}, '{note}', FALSE, 0, '{zone}');";

                Bdd.Executer(reqCuisinier);
                Console.WriteLine($"Cuisinier ajouté avec succès (ID: {idGenere}) !");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

        private static void Supprimer()
        {
            Console.Clear();
            Console.WriteLine("--- Supprimer un cuisinier ---");
            Console.Write("ID du cuisinier : ");
            string id = Console.ReadLine();

            try
            {
                Bdd.Executer($"DELETE FROM Tier WHERE ID = {id}");
                Console.WriteLine("Cuisinier supprimé.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

        private static void Modifier()
        {
            Console.Clear();
            Console.WriteLine("--- Modifier la zone de livraison ---");
            Console.Write("ID du cuisinier : ");
            string id = Console.ReadLine();
            Console.Write("Nouvelle zone : ");
            string zone = Console.ReadLine();

            try
            {
                string requete = $@"
                    UPDATE Cuisinier SET ZoneLivraison = '{zone}' 
                    WHERE ID = {id};";
                Bdd.Executer(requete);
                Console.WriteLine("Zone mise à jour.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }
    }
}
