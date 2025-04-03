using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    public static class ModuleCommande
    {
        public static void Lancer()
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine("--- Module Commande ---");
                Console.WriteLine("1. Créer une nouvelle commande");
                Console.WriteLine("2. Modifier une commande");
                Console.WriteLine("3. Calculer le prix d'une commande");
                Console.WriteLine("4. Afficher chemin de livraison (trajet)");
                Console.WriteLine("0. Retour menu principal");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1": CreerCommande(); break;
                    case "2": ModifierCommande(); break;
                    case "3": CalculerPrixCommande(); break;
                    case "4": AfficherTrajet(); break;
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
            Console.Write("Prix total estimé : "); string prix = Console.ReadLine();

            try
            {
                string insertCmd = $@"
                    INSERT INTO CommandeLivraison (NumeroDeCommande, DateDeLivraisonSouhaitee, FraisDeLivraison, Prix, EtatdeLaCommande, ID_Cuisinier)
                    VALUES (NULL, '{date}', {frais}, {prix}, 'En préparation', '{idCuisinier}');";

                Bdd.Executer(insertCmd);

                int livraisonID = 0;
                using var r = Bdd.Lire("SELECT MAX(NumerodeLivraison) as MaxID FROM CommandeLivraison");
                if (r.Read()) livraisonID = r.GetInt32("MaxID");
                r.Close();

                string insertRecoit = $@"INSERT INTO Recoit VALUES ('{idClient}', {livraisonID})";
                string insertTrajet = $@"INSERT INTO Trajet (AdresseArrivee, AdresseDepart, Distance, TempsEstime, CheminPris, NumerodeLivraison)
                                         VALUES ('{adresseClient}', '{adresseCuisinier}', 0, '00:00:00', '', {livraisonID})";
                Bdd.Executer(insertRecoit);
                Bdd.Executer(insertTrajet);
                Console.WriteLine("Commande créée avec succès.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

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

        private static void CalculerPrixCommande()
        {
            Console.Clear();
            Console.WriteLine("--- Calcul du prix d'une commande ---");
            Console.Write("Numéro de livraison : "); string num = Console.ReadLine();
            string req = $"SELECT Prix FROM CommandeLivraison WHERE NumerodeLivraison = {num}";

            using var r = Bdd.Lire(req);
            if (r.Read())
            {
                decimal prix = r.GetDecimal("Prix");
                Console.WriteLine($"Prix de la commande : {prix:C}");
            }
            else
            {
                Console.WriteLine("Commande non trouvée.");
            }
            r.Close();
        }

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
