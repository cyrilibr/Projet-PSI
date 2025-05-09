using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    public static class ModuleCuisinierConnecte
    {
        public static void Lancer(Graphe<Station> graphe)
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine($"=== Menu Cuisinier ({Session.Email}) ===");
                Console.WriteLine("1. Afficher mes infos");
                Console.WriteLine("2. Modifier mes infos");
                Console.WriteLine("3. Simuler un trajet entre une station et un client");
                Console.WriteLine("4. Afficher mes commandes à livrer");
                Console.WriteLine("5. Valider une commande comme livrée");
                Console.WriteLine("0. Se déconnecter");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1": AfficherInfos(); break;
                    case "2": ModifierInfos(); break;
                    case "3": SimulerTrajet(graphe); break;
                    case "4": AfficherCommandes(); break;
                    case "5": ValiderCommande(); break;
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
            Console.Write("Adresse du client : ");
            string adresseClient = Console.ReadLine();

            string adresseCuisinier = "";
            using (var r = Bdd.Lire($"SELECT ADR, CODEPOSTAL, VILLE FROM Tier WHERE ID = {Session.IdUtilisateur}"))
            {
                if (r.Read())
                    adresseCuisinier = $"{r.GetString("ADR")}, {r.GetString("CODEPOSTAL")} {r.GetString("VILLE")}";
                r.Close();
            }

            int idDep = GeoUtils.StationLaPlusProche(graphe, adresseCuisinier);
            int idArr = GeoUtils.StationLaPlusProche(graphe, adresseClient);

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

            Console.WriteLine("\n--- Chemin vers client ---");
            foreach (var id in chemin)
                Console.WriteLine($" - {graphe.Noeuds[id].Data}");

            Console.WriteLine($"\nDistance estimée : {chemin.Count * 0.5} km");
            Console.WriteLine($"Temps estimé : {chemin.Count * 2} minutes");
        }

        private static void AfficherCommandes()
        {
            string req = $@"
                SELECT CL.NumerodeLivraison, CL.EtatdeLaCommande, T.NOMT, T.PRENOMT, T.ADR
                FROM CommandeLivraison CL
                JOIN Reçoit R ON CL.NumerodeLivraison = R.NumerodeLivraison
                JOIN Client C ON R.ID_Client = C.ID
                JOIN Tier T ON C.ID = T.ID
                WHERE CL.ID_Cuisinier = {Session.IdUtilisateur} AND CL.EtatdeLaCommande != 'Livrée';";

            using var r = Bdd.Lire(req);
            Console.WriteLine("\n--- Commandes à livrer ---");
            while (r.Read())
            {
                Console.WriteLine($"Commande {r.GetInt32("NumerodeLivraison")} - {r.GetString("EtatdeLaCommande")}");
                Console.WriteLine($"  Client : {r.GetString("PRENOMT")} {r.GetString("NOMT")}, {r.GetString("ADR")}");
            }
            r.Close();
        }

        private static void ValiderCommande()
        {
            Console.Write("Numéro de livraison à valider : ");
            string id = Console.ReadLine();

            string req = $"UPDATE CommandeLivraison SET EtatdeLaCommande = 'Livrée' WHERE NumerodeLivraison = {id} AND ID_Cuisinier = {Session.IdUtilisateur}";
            Bdd.Executer(req);
            Console.WriteLine("Commande validée comme livrée.");
        }
    }
}
