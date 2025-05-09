using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    /// <summary>
    /// Module de gestion des commandes, permettant la création, la modification et
    /// l'affichage des trajets de livraison.
    /// </summary>
    public static class ModuleCommande
    {
        /// <summary>
        /// Lance le menu principal du module commande.
        /// </summary>
        public static void Lancer()
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine("--- Module Commande ---");
                Console.WriteLine("1. Créer une nouvelle commande");
                Console.WriteLine("2. Modifier une commande");
                Console.WriteLine("3. Afficher chemin de livraison (trajet)");
                Console.WriteLine("0. Retour menu principal");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1": CreerCommande(); break;
                    case "2": ModifierCommande(); break;
                    case "3": AfficherTrajet(); break;
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
        /// Crée une nouvelle commande avec le client, cuisinier, adresse et mets sélectionnés.
        /// </summary>
        private static void CreerCommande()
        {
            Console.Clear();
            Console.WriteLine("--- Création d'une commande ---");
            Console.Write("ID du client : "); string idClient = Console.ReadLine();
            Console.Write("ID du cuisinier : "); string idCuisinier = Console.ReadLine();
            Console.Write("Adresse d'arrivée : "); string adresseClient = Console.ReadLine();
            Console.Write("Adresse de départ : "); string adresseCuisinier = Console.ReadLine();
            Console.Write("Date de livraison souhaitée (yyyy-mm-dd hh:mm:ss) : "); string date = Console.ReadLine();
            Console.Write("Frais de livraison : "); string frais = Console.ReadLine();

            decimal prixTotal = 0;
            string metsChoisis = "";

            Console.WriteLine("Ajoutez des mets à la commande. Tapez 'fin' pour terminer.");
            while (true)
            {
                Console.Write("ID du mets : ");
                string idMets = Console.ReadLine();
                if (idMets.ToLower() == "fin") break;

                Console.Write("Quantité : ");
                if (!int.TryParse(Console.ReadLine(), out int quantite)) continue;

                string prixReq = $"SELECT PrixParPersonne FROM Mets WHERE ID = '{idMets}'";
                using var r = Bdd.Lire(prixReq);
                if (r.Read())
                {
                    decimal prixMets = r.GetDecimal("PrixParPersonne");
                    prixTotal += prixMets * quantite;
                    metsChoisis += $"INSERT INTO Contient VALUES ('{idMets}', @IDCMD, {quantite});\n";
                }
                r.Close();
            }

            prixTotal += Convert.ToDecimal(frais);

            try
            {
                string insertCmd = $@"
                    INSERT INTO CommandeLivraison (NumeroDeCommande, DateDeLivraisonSouhaitee, FraisDeLivraison, Prix, EtatdeLaCommande, ID_Cuisinier)
                    VALUES (NULL, '{date}', {frais}, {prixTotal}, 'En préparation', '{idCuisinier}');";
                Bdd.Executer(insertCmd);

                int livraisonID = 0;
                using var r2 = Bdd.Lire("SELECT MAX(NumerodeLivraison) AS MaxID FROM CommandeLivraison");
                if (r2.Read()) livraisonID = r2.GetInt32("MaxID");
                r2.Close();

                Bdd.Executer($"INSERT INTO Reçoit VALUES ('{idClient}', {livraisonID})");
                Bdd.Executer($"INSERT INTO Trajet (AdresseArrivee, AdresseDepart, Distance, TempsEstime, CheminPris, NumerodeLivraison) VALUES ('{adresseClient}', '{adresseCuisinier}', 0, '00:00:00', '', {livraisonID})");

                foreach (string ligne in metsChoisis.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(ligne))
                    {
                        string requete = ligne.Replace("@IDCMD", livraisonID.ToString());
                        Bdd.Executer(requete);
                    }
                }

                Console.WriteLine("Commande créée avec succès.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

        /// <summary>
        /// Modifie l'état d'une commande existante.
        /// </summary>
        private static void ModifierCommande()
        {
            Console.Clear();
            Console.WriteLine("--- Modification d'une commande ---");
            Console.Write("Numéro de livraison : "); string num = Console.ReadLine();
            Console.Write("Nouvel état (ex: Livrée) : "); string etat = Console.ReadLine();
            try
            {
                Bdd.Executer($"UPDATE CommandeLivraison SET EtatdeLaCommande = '{etat}' WHERE NumerodeLivraison = {num}");
                Console.WriteLine("Commande modifiée.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

        /// <summary>
        /// Affiche le trajet de livraison pour une commande donnée.
        /// </summary>
        private static void AfficherTrajet()
        {
            Console.Clear();
            Console.WriteLine("--- Affichage du trajet ---");
            Console.Write("Numéro de livraison : "); string num = Console.ReadLine();

            string req = $@"
                SELECT AdresseDepart, AdresseArrivee, Distance, TempsEstime, CheminPris
                FROM Trajet WHERE NumerodeLivraison = {num}";

            using var r = Bdd.Lire(req);
            if (r.Read())
            {
                string dep = r.GetString("AdresseDepart");
                string arr = r.GetString("AdresseArrivee");
                double dist = r.GetDouble("Distance");
                string temps = r.GetString("TempsEstime");
                string chemin = r.GetString("CheminPris");
                Console.WriteLine($"De : {dep}\nVers : {arr}\nDistance : {dist} km\nTemps estimé : {temps}\nChemin : {chemin}");
            }
            else
            {
                Console.WriteLine("Aucun trajet trouvé.");
            }
            r.Close();
        }
    }
}
