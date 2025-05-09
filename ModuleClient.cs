using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    /// <summary>
    /// Module de gestion des clients pour l'administrateur, incluant la consultation,
    /// l'ajout, la suppression et la modification des informations client.
    /// </summary>
    public static class ModuleClient
    {
        /// <summary>
        /// Lance le menu principal du module client (admin).
        /// </summary>
        public static void Lancer()
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine("--- Module Client (admin) ---");
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

        /// <summary>
        /// Affiche la liste des clients selon une clause ORDER spécifiée.
        /// </summary>
        /// <param name="orderClause">Clause SQL ORDER BY pour trier les résultats.</param>
        private static void AfficherClients(string orderClause)
        {
            Console.Clear();
            Console.WriteLine("--- Liste des Clients ---\n");
            string query = $@"
                SELECT T.ID, T.NOMT, T.PRENOMT, T.ADR, C.MontantTotalAchats 
                FROM Client C 
                JOIN Tier T ON C.ID = T.ID 
                {orderClause};";

            using var reader = Bdd.Lire(query);
            while (reader.Read())
            {
                int id = reader.GetInt32("ID");
                string nom = reader.GetString("NOMT");
                string prenom = reader.GetString("PRENOMT");
                string adresse = reader.GetString("ADR");
                decimal total = reader.GetDecimal("MontantTotalAchats");

                Console.WriteLine($"{prenom} {nom} (ID: {id}) - {adresse} - Total Achats: {total:C}");
            }
            reader.Close();
        }

        /// <summary>
        /// Ajoute un nouveau client en base, dans les tables Tier et Client.
        /// </summary>
        private static void AjouterClient()
        {
            Console.Clear();
            Console.WriteLine("--- Ajout d'un nouveau client ---");
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

            try
            {
                string insertTier = $@"
                    INSERT INTO Tier (MDP, NOMT, PRENOMT, ADR, CODEPOSTAL, VILLE, EMAIL, TEL, Radiation, Retour)
                    VALUES ('{mdp}', '{nom}', '{prenom}', '{adr}', '{cp}', '{ville}', '{email}', '{tel}', FALSE, '');";
                Bdd.Executer(insertTier);

                int idGenere = 0;
                using (var reader = Bdd.Lire("SELECT LAST_INSERT_ID() AS ID"))
                {
                    if (reader.Read())
                        idGenere = reader.GetInt32("ID");
                    reader.Close();
                }

                string insertClient = $@"
                    INSERT INTO Client VALUES ('{idGenere}', '{note}', '{type}', {total}, '');";

                Bdd.Executer(insertClient);
                Console.WriteLine($"Client ajouté avec succès (ID : {idGenere})");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

        /// <summary>
        /// Supprime un client de la base via son ID.
        /// </summary>
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

        /// <summary>
        /// Modifie les coordonnées d'un client existant.
        /// </summary>
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

        /// <summary>
        /// Redirige vers le menu du client connecté.
        /// </summary>
        /// <param name="graphe">Le graphe des stations pour la simulation de trajet.</param>
        public static void LancerClient(Graphe<Station> graphe)
        {
            ModuleClientConnecte.Lancer(graphe);
        }
    }
}
