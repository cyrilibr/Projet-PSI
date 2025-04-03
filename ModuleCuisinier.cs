using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    public static class ModuleCuisinier
    {
        public static void Lancer()
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine("--- Module Cuisinier ---");
                Console.WriteLine("1. Ajouter un cuisinier");
                Console.WriteLine("2. Supprimer un cuisinier");
                Console.WriteLine("3. Modifier un cuisinier");
                Console.WriteLine("4. Afficher clients servis");
                Console.WriteLine("5. Afficher plats par fréquence");
                Console.WriteLine("6. Afficher plat du jour");
                Console.WriteLine("0. Retour menu principal");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1": AjouterCuisinier(); break;
                    case "2": SupprimerCuisinier(); break;
                    case "3": ModifierCuisinier(); break;
                    case "4": AfficherClientsServis(); break;
                    case "5": AfficherPlatsParFrequence(); break;
                    case "6": AfficherPlatDuJour(); break;
                    case "0": retour = true; break;
                    default: Console.WriteLine("Choix invalide."); break;
                }

                if (!retour)
                {
                    Console.WriteLine("Appuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            }
        }

        private static void AjouterCuisinier()
        {
            Console.Clear();
            Console.WriteLine("--- Ajout d'un cuisinier ---");
            Console.Write("ID : "); string id = Console.ReadLine();
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
                string req1 = $@"INSERT INTO Tier VALUES ('{id}', '{mdp}', '{nom}', '{prenom}', '{adr}', '{cp}', '{ville}', '{email}', '{tel}', FALSE, '')";
                string req2 = $@"INSERT INTO Cuisinier VALUES ('{id}', '{note}', FALSE, 0, '{zone}')";
                Bdd.Executer(req1);
                Bdd.Executer(req2);
                Console.WriteLine("Cuisinier ajouté.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

        private static void SupprimerCuisinier()
        {
            Console.Clear();
            Console.WriteLine("--- Suppression d'un cuisinier ---");
            Console.Write("ID du cuisinier : ");
            string id = Console.ReadLine();

            try
            {
                Bdd.Executer($"DELETE FROM Tier WHERE ID = '{id}'");
                Console.WriteLine("Cuisinier supprimé.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

        private static void ModifierCuisinier()
        {
            Console.Clear();
            Console.WriteLine("--- Modifier un cuisinier ---");
            Console.Write("ID : "); string id = Console.ReadLine();
            Console.Write("Nouvelle zone de livraison : "); string zone = Console.ReadLine();

            try
            {
                Bdd.Executer($"UPDATE Cuisinier SET ZoneLivraison = '{zone}' WHERE ID = '{id}'");
                Console.WriteLine("Zone de livraison modifiée.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

        private static void AfficherClientsServis()
        {
            Console.Clear();
            Console.WriteLine("--- Clients servis par un cuisinier ---");
            Console.Write("ID du cuisinier : "); string id = Console.ReadLine();

            string query = $@"
                SELECT DISTINCT T.NOMT, T.PRENOMT FROM Recoit R
                JOIN Client C ON R.ID_Client = C.ID
                JOIN Tier T ON C.ID = T.ID
                JOIN CommandeLivraison CL ON CL.NumerodeLivraison = R.NumerodeLivraison
                WHERE CL.ID_Cuisinier = '{id}';";

            using var reader = Bdd.Lire(query);
            while (reader.Read())
            {
                string nom = reader.GetString("NOMT");
                string prenom = reader.GetString("PRENOMT");
                Console.WriteLine($"{prenom} {nom}");
            }
            reader.Close();
        }

        private static void AfficherPlatsParFrequence()
        {
            Console.Clear();
            Console.WriteLine("--- Plats par fréquence ---");
            Console.Write("ID du cuisinier : "); string id = Console.ReadLine();

            string query = $@"
                SELECT M.ID, COUNT(*) as Frequence
                FROM Contient C
                JOIN Mets M ON C.ID_Mets = M.ID
                WHERE M.ID_Cuisinier = '{id}'
                GROUP BY M.ID
                ORDER BY Frequence DESC;";

            using var reader = Bdd.Lire(query);
            while (reader.Read())
            {
                string idMets = reader.GetString("ID");
                int freq = reader.GetInt32("Frequence");
                Console.WriteLine($"Plat {idMets} - {freq} commandes");
            }
            reader.Close();
        }

        private static void AfficherPlatDuJour()
        {
            Console.Clear();
            Console.WriteLine("--- Plat du jour ---");
            Console.Write("ID du cuisinier : "); string id = Console.ReadLine();

            string query = $@"
                SELECT ID, Type, PrixParPersonne FROM Mets
                WHERE ID_Cuisinier = '{id}' AND Disponibilite = TRUE
                ORDER BY Popularite DESC LIMIT 1;";

            using var reader = Bdd.Lire(query);
            if (reader.Read())
            {
                string idPlat = reader.GetString("ID");
                string type = reader.GetString("Type");
                decimal prix = reader.GetDecimal("PrixParPersonne");
                Console.WriteLine($"Plat du jour : {type} ({idPlat}) - {prix:C}");
            }
            else
            {
                Console.WriteLine("Aucun plat disponible.");
            }
            reader.Close();
        }
    }
}
