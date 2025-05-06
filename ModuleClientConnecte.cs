using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    public static class ModuleClientConnecte
    {
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
                Console.Write("Choix : ");
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

        private static void ModifierInfos()
        {
            Console.Write("Nouveau téléphone : "); string tel = Console.ReadLine();
            Console.Write("Nouvelle adresse : "); string adr = Console.ReadLine();
            Console.Write("Nouveau code postal : "); string cp = Console.ReadLine();
            Console.Write("Nouvelle ville : "); string ville = Console.ReadLine();

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

            Console.WriteLine("\n--- Chemin suggéré ---");
            foreach (var id in chemin)
                Console.WriteLine($" - {graphe.Noeuds[id].Data}");

            Console.WriteLine($"\nDistance estimée : {chemin.Count * 0.5} km");
            Console.WriteLine($"Temps estimé : {chemin.Count * 2} minutes");
        }

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
                using var r = Bdd.Lire("SELECT MAX(NumerodeLivraison) as MaxID FROM CommandeLivraison");
                if (r.Read()) livraisonID = r.GetInt32("MaxID");
                r.Close();

                Bdd.Executer($"INSERT INTO Reçoit VALUES ('{Session.IdUtilisateur}', {livraisonID})");
                Bdd.Executer($"INSERT INTO Trajet (AdresseArrivee, AdresseDepart, Distance, TempsEstime, CheminPris, NumerodeLivraison) VALUES ('{adresseClient}', '{adresseDep}', 0, '00:00:00', '', {livraisonID})");

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
    }
}