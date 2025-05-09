// Projet_PSI/Modules/ModuleClientConnecte.cs
using System;
using System.Windows.Forms;
using Projet_PSI.Utils;

namespace Projet_PSI.Modules
{
    /// <summary>
    /// Module dédié au client connecté, lui permettant de gérer ses informations,
    /// simuler des trajets et passer des commandes.
    /// </summary>
    public static class ModuleClientConnecte
    {
        /// <summary>
        /// Lance le menu principal du client connecté.
        /// </summary>
        /// <param name="graphe">Le graphe des stations pour calculer les trajets.</param>
        public static void Lancer(Graphe<Station> graphe)
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine($"=== Menu Client ({Session.Email}) ===");
                Console.WriteLine("1. Afficher mes infos");
                Console.WriteLine("2. Modifier mes infos");
                Console.WriteLine("3. Simuler un trajet entre deux stations");
                Console.WriteLine("4. Calculer prix et passer commande");
                Console.WriteLine("0. Se déconnecter");
                Console.Write("\nChoix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1": AfficherInfos(); break;
                    case "2": ModifierInfos(); break;
                    case "3": SimulerTrajet(graphe); break;
                    case "4": PasserCommande(); break;
                    case "0":
                        Session.Reinitialiser();
                        retour = true;
                        break;
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
        /// Affiche les informations personnelles du client connecté.
        /// </summary>
        private static void AfficherInfos()
        {
            string req = $"SELECT * FROM Tier WHERE ID = {Session.IdUtilisateur}";
            using var r = Bdd.Lire(req);
            if (r.Read())
            {
                Console.WriteLine("\n--- Mes informations ---");
                Console.WriteLine($"Nom : {r.GetString("NOMT")}");
                Console.WriteLine($"Prénom : {r.GetString("PRENOMT")}");
                Console.WriteLine($"Adresse : {r.GetString("ADR")}, {r.GetString("CODEPOSTAL")} {r.GetString("VILLE")}");
                Console.WriteLine($"Email : {r.GetString("EMAIL")}");
                Console.WriteLine($"Téléphone : {r.GetString("TEL")}");
            }
            r.Close();
        }

        /// <summary>
        /// Permet au client de modifier ses coordonnées personnelles.
        /// </summary>
        private static void ModifierInfos()
        {
            Console.Write("Nouveau téléphone : ");
            string tel = Console.ReadLine();
            Console.Write("Nouvelle adresse : ");
            string adr = Console.ReadLine();
            Console.Write("Nouveau code postal : ");
            string cp = Console.ReadLine();
            Console.Write("Nouvelle ville : ");
            string ville = Console.ReadLine();

            string req = $@"
                UPDATE Tier SET
                    TEL = '{tel}',
                    ADR = '{adr}',
                    CODEPOSTAL = '{cp}',
                    VILLE = '{ville}'
                WHERE ID = {Session.IdUtilisateur}";

            Bdd.Executer(req);
            Console.WriteLine("Informations mises à jour.");
        }

        /// <summary>
        /// Simule un trajet entre deux adresses et affiche le résultat graphiquement.
        /// </summary>
        /// <param name="graphe">Le graphe des stations.</param>
        private static void SimulerTrajet(Graphe<Station> graphe)
        {
            Console.Write("Adresse de départ : ");
            string adresseDep = Console.ReadLine();
            Console.Write("Adresse d'arrivée : ");
            string adresseArr = Console.ReadLine();

            int idDep = GeoUtils.StationLaPlusProche(graphe, adresseDep);
            int idArr = GeoUtils.StationLaPlusProche(graphe, adresseArr);

            if (idDep == -1 || idArr == -1)
            {
                Console.WriteLine("Stations non trouvées.");
                return;
            }

            var chemin = graphe.CheminDijkstra(idDep, idArr);
            if (chemin.Count == 0)
            {
                Console.WriteLine("Aucun chemin trouvé.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var fen = new FenetreGraphe<Station>(graphe, idDep, idArr))
            {
                fen.ShowDialog();
            }
        }

        /// <summary>
        /// Calcule le prix total et crée une commande en base.
        /// </summary>
        private static void PasserCommande()
        {
            Console.Clear();
            Console.WriteLine("--- Simulation de commande ---");
            Console.Write("ID du cuisinier : ");
            string idCuisinier = Console.ReadLine();
            Console.Write("Adresse de départ (du cuisinier) : ");
            string adresseDep = Console.ReadLine();

            string adresseClient = "";
            using (var r = Bdd.Lire($"SELECT ADR, CODEPOSTAL, VILLE FROM Tier WHERE ID = {Session.IdUtilisateur}"))
            {
                if (r.Read())
                    adresseClient = $"{r.GetString("ADR")}, {r.GetString("CODEPOSTAL")} {r.GetString("VILLE")}";
                r.Close();
            }

            Console.Write("Date de livraison souhaitée (yyyy-MM-dd HH:mm:ss) : ");
            string date = Console.ReadLine();
            Console.Write("Frais de livraison : ");
            string frais = Console.ReadLine();

            decimal prixTotal = 0;
            string metsChoisis = "";

            Console.WriteLine("Ajoutez des mets à la commande (tapez 'fin' pour terminer)");
            while (true)
            {
                Console.Write("ID du mets : ");
                string idMets = Console.ReadLine();
                if (idMets.Equals("fin", StringComparison.OrdinalIgnoreCase)) break;

                Console.Write("Quantité : ");
                if (!int.TryParse(Console.ReadLine(), out int quantite)) continue;

                string prixReq = $"SELECT PrixParPersonne FROM Mets WHERE ID = '{idMets}'";
                using var r2 = Bdd.Lire(prixReq);
                if (r2.Read())
                {
                    decimal prixMets = r2.GetDecimal("PrixParPersonne");
                    prixTotal += prixMets * quantite;
                    metsChoisis += $"INSERT INTO Contient VALUES('{idMets}', @IDCMD, {quantite});\n";
                }
                r2.Close();
            }

            prixTotal += Convert.ToDecimal(frais, System.Globalization.CultureInfo.InvariantCulture);

            try
            {
                string insertCmd = $@"
                    INSERT INTO CommandeLivraison
                      (NumeroDeLivraison, DateDeLivraisonSouhaitee, FraisDeLivraison, Prix,
                       EtatdeLaCommande, ID_Cuisinier)
                    VALUES
                      (NULL, '{date}', {frais}, {prixTotal}, 'En préparation', {idCuisinier});";
                Bdd.Executer(insertCmd);

                int livraisonID;
                using var r3 = Bdd.Lire("SELECT MAX(NumerodeLivraison) AS MaxID FROM CommandeLivraison");
                r3.Read();
                livraisonID = r3.GetInt32("MaxID");
                r3.Close();

                Bdd.Executer($"INSERT INTO Reçoit VALUES({Session.IdUtilisateur}, {livraisonID});");
                Bdd.Executer($@"
                    INSERT INTO Trajet
                      (AdresseArrivee, AdresseDepart, Distance, TempsEstime, CheminPris, NumerodeLivraison)
                    VALUES
                      ('{adresseClient}', '{adresseDep}', 0, '00:00:00', '', {livraisonID});");

                foreach (var ligne in metsChoisis.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    Bdd.Executer(ligne.Replace("@IDCMD", livraisonID.ToString()));
                }

                Console.WriteLine("Commande créée avec succès.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }
    }
}
