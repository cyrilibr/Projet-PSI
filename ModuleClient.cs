using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    public static class ModuleClient
    {
        public static void Lancer()
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine("--- Module Client ---");
                Console.WriteLine("1. Afficher clients par nom");
                Console.WriteLine("2. Afficher clients par rue");
                Console.WriteLine("3. Afficher clients par montant total d'achats");
                Console.WriteLine("4. Ajouter un client");
                Console.WriteLine("5. Supprimer un client");
                Console.WriteLine("6. Modifier un client");
                Console.WriteLine("0. Retour menu principal");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1": AfficherClients("ORDER BY T.NOMT, T.PRENOMT"); break;
                    case "2": AfficherClients("ORDER BY T.ADR"); break;
                    case "3": AfficherClients("ORDER BY C.MontantTotalAchats DESC"); break;
                    case "4": AjouterClient(); break;
                    case "5": SupprimerClient(); break;
                    case "6": ModifierClient(); break;
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

        private static void AfficherClients(string orderClause)
        {
            Console.Clear();
            Console.WriteLine("--- Liste des Clients ---\n");
            string query = $@"
                SELECT T.NOMT, T.PRENOMT, T.ADR, C.MontantTotalAchats 
                FROM Client C 
                JOIN Tier T ON C.ID = T.ID 
                {orderClause};";

            using var reader = Bdd.Lire(query);
            while (reader.Read())
            {
                string nom = reader.GetString("NOMT");
                string prenom = reader.GetString("PRENOMT");
                string adresse = reader.GetString("ADR");
                decimal total = reader.GetDecimal("MontantTotalAchats");

                Console.WriteLine($"{prenom} {nom} - {adresse} - Total Achats: {total:C}");
            }
            reader.Close();
        }

        private static void AjouterClient()
        {
            Console.Clear();
            Console.WriteLine("--- Ajout d'un nouveau client ---");
            Console.Write("ID unique : "); string id = Console.ReadLine();
            Console.Write("Mot de passe : "); string mdp = Console.ReadLine();
            Console.Write("Nom : "); string nom = Console.ReadLine();
            Console.Write("Prénom : "); string prenom = Console.ReadLine();
            Console.Write("Adresse : "); string adr = Console.ReadLine();
            Console.Write("Code Postal : "); string cp = Console.ReadLine();
            Console.Write("Ville : "); string ville = Console.ReadLine();
            Console.Write("Email : "); string email = Console.ReadLine();
            Console.Write("Téléphone : "); string tel = Console.ReadLine();
            Console.Write("Note (optionnelle) : "); string note = Console.ReadLine();
            Console.Write("Type : "); string type = Console.ReadLine();
            Console.Write("Montant total des achats : "); string total = Console.ReadLine();

            string insertTier = $@"
                INSERT INTO Tier VALUES ('{id}', '{mdp}', '{nom}', '{prenom}', '{adr}', '{cp}', '{ville}', '{email}', '{tel}', FALSE, '');";
            string insertClient = $@"
                INSERT INTO Client VALUES ('{id}', '{note}', '{type}', {total}, '');";

            try
            {
                Bdd.Executer(insertTier);
                Bdd.Executer(insertClient);
                Console.WriteLine("Client ajouté avec succès.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

        private static void SupprimerClient()
        {
            Console.Clear();
            Console.WriteLine("--- Suppression d'un client ---");
            Console.Write("ID du client à supprimer : ");
            string id = Console.ReadLine();

            try
            {
                Bdd.Executer($"DELETE FROM Tier WHERE ID = '{id}'");
                Console.WriteLine("Client supprimé avec succès.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

        private static void ModifierClient()
        {
            Console.Clear();
            Console.WriteLine("--- Modification d'un client ---");
            Console.Write("ID du client à modifier : "); string id = Console.ReadLine();
            Console.Write("Nouveau téléphone : "); string tel = Console.ReadLine();
            Console.Write("Nouvelle adresse : "); string adr = Console.ReadLine();
            Console.Write("Nouvelle ville : "); string ville = Console.ReadLine();

            try
            {
                string req = $@"UPDATE Tier SET TEL = '{tel}', ADR = '{adr}', VILLE = '{ville}' WHERE ID = '{id}'";
                Bdd.Executer(req);
                Console.WriteLine("Client modifié avec succès.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }
    }
}
