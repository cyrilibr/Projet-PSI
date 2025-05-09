using Projet_PSI.Utils;
using System;
using System.Collections.Generic;

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
                Console.WriteLine("6. Gérer mes plats");
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
                    case "6": GererMesPlats(); break; 
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


        private static void GererMesPlats()
        {
            bool retourMenuCuisinier = false;
            while (!retourMenuCuisinier)
            {
                Console.Clear();
                Console.WriteLine("--- Gestion de mes Plats ---");
                Console.WriteLine("1. Afficher mes plats");
                Console.WriteLine("2. Ajouter un nouveau plat");
                Console.WriteLine("0. Retour au menu précédent");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        AfficherMesPlats();
                        break;
                    case "2":
                        AjouterPlat();
                        break;
                    case "0":
                        retourMenuCuisinier = true;
                        break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
                if (!retourMenuCuisinier)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            }
        }

        private static void AfficherMesPlats()
        {
            Console.Clear();
            Console.WriteLine("--- Mes Plats ---");
            string req = $@"
                SELECT ID, Type, NbDePersonnes, PrixParPersonne, Nationalite, RegimeAlimentaire, 
                       DateDePeremption, Disponibilite, Popularite 
                FROM Mets 
                WHERE ID_Cuisinier = {Session.IdUtilisateur}";

            using (var reader = Bdd.Lire(req))
            {
                if (!reader.HasRows)
                {
                    Console.WriteLine("Vous n'avez aucun plat enregistré.");
                    reader.Close();
                    return;
                }
                while (reader.Read())
                {
                    Console.WriteLine($"\nID Plat: {reader.GetString("ID")}");
                    Console.WriteLine($"  Type: {reader.GetString("Type")}, Pour: {reader.GetInt32("NbDePersonnes")} personne(s)");
                    Console.WriteLine($"  Prix/Personne: {reader.GetDecimal("PrixParPersonne"):C}, Nationalité: {reader.GetString("Nationalite")}");
                    Console.WriteLine($"  Régime: {reader.GetString("RegimeAlimentaire")}, Péremption: {reader.GetDateTime("DateDePeremption"):yyyy-MM-dd}");
                    Console.WriteLine($"  Disponibilité: {(reader.GetBoolean("Disponibilite") ? "Oui" : "Non")}, Popularité: {reader.GetInt32("Popularite")}");
                    // Afficher les ingrédients du plat
                    AfficherIngredientsPourPlat(reader.GetString("ID"));
                }
                reader.Close();
            }
        }

        private static void AfficherIngredientsPourPlat(string idMet)
        {
            Console.WriteLine("  Ingrédients:");
            string req = $@"
                SELECT I.NomIngredient, Q.NbIngredients 
                FROM Quantite Q
                JOIN Ingredients I ON Q.IDIngredient = I.IDIngredient
                WHERE Q.ID_Mets = '{idMet}'";

            using (var reader = Bdd.Lire(req))
            {
                if (!reader.HasRows)
                {
                    Console.WriteLine("    - Aucun ingrédient associé.");
                    reader.Close();
                    return;
                }
                while (reader.Read())
                {
                    Console.WriteLine($"    - {reader.GetString("NomIngredient")}: {reader.GetInt32("NbIngredients")}");
                }
                reader.Close();
            }
        }


        private static bool MetIdExiste(string idMet)
        {
            string req = $"SELECT 1 FROM Mets WHERE ID = '{idMet}'";
            using (var reader = Bdd.Lire(req))
            {
                bool existe = reader.Read();
                reader.Close();
                return existe;
            }
        }

        private static bool IngredientIdExiste(string idIngredient)
        {
            string req = $"SELECT 1 FROM Ingredients WHERE IDIngredient = '{idIngredient}'";
            using (var reader = Bdd.Lire(req))
            {
                bool existe = reader.Read();
                reader.Close();
                return existe;
            }
        }


        private static void AjouterPlat()
        {
            Console.Clear();
            Console.WriteLine("--- Ajouter un Nouveau Plat ---");

            string idMet;
            do
            {
                Console.Write("ID du plat (ex: M4, M_SPECIAL_NOEL): ");
                idMet = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(idMet))
                {
                    Console.WriteLine("L'ID du plat ne peut pas être vide.");
                }
                else if (MetIdExiste(idMet))
                {
                    Console.WriteLine("Cet ID de plat existe déjà. Veuillez en choisir un autre.");
                    idMet = null; // pour que la boucle continue
                }
            } while (string.IsNullOrWhiteSpace(idMet));

            Console.Write("Type (Entrée, Plat principal, Dessert): "); string type = Console.ReadLine();
            Console.Write("Nombre de personnes: "); int nbPersonnes = int.Parse(Console.ReadLine());
            Console.Write("Prix par personne: "); decimal prixParPersonne = decimal.Parse(Console.ReadLine().Replace(",", ".")); // Gérer la virgule
            Console.Write("Nationalité: "); string nationalite = Console.ReadLine();
            Console.Write("Régime alimentaire: "); string regime = Console.ReadLine();
            Console.Write("Date de péremption (YYYY-MM-DD): "); string datePeremption = Console.ReadLine();
            Console.Write("Date de fabrication (YYYY-MM-DD): "); string dateFabrication = Console.ReadLine();
            Console.Write("Photo (nom du fichier/URL, ex: mon_plat.jpg): "); string photo = Console.ReadLine();
            Console.Write("Disponibilité (true/false): "); bool dispo = bool.Parse(Console.ReadLine());
            Console.Write("Popularité (0-100): "); int popularite = int.Parse(Console.ReadLine());

            try
            {
                string reqMet = $@"
                    INSERT INTO Mets (ID, Type, NbDePersonnes, PrixParPersonne, Nationalite, RegimeAlimentaire, DateDePeremption, Photo, DateDeFabrication, Disponibilite, Popularite, ID_Cuisinier) 
                    VALUES ('{idMet}', '{type}', {nbPersonnes}, {prixParPersonne.ToString(System.Globalization.CultureInfo.InvariantCulture)}, '{nationalite}', '{regime}', '{datePeremption}', '{photo}', '{dateFabrication}', {dispo}, {popularite}, {Session.IdUtilisateur});";
                Bdd.Executer(reqMet);
                Console.WriteLine($"Plat '{idMet}' ajouté avec succès !");

                // Ajout des ingrédients pour ce plat
                Console.WriteLine("\n--- Ajout des ingrédients pour ce plat ---");
                AfficherIngredientsDisponibles();

                string idIngredient;
                while (true)
                {
                    Console.Write("ID de l'ingrédient à ajouter (ou 'fin' pour terminer): ");
                    idIngredient = Console.ReadLine();
                    if (idIngredient.ToLower() == "fin") break;

                    if (!IngredientIdExiste(idIngredient))
                    {
                        Console.WriteLine($"L'ingrédient avec ID '{idIngredient}' n'existe pas. Veuillez vérifier l'ID ou l'ajouter (fonctionnalité admin).");
                        continue;
                    }

                    Console.Write($"Quantité de '{idIngredient}' nécessaire: ");
                    if (!int.TryParse(Console.ReadLine(), out int nbIngredients) || nbIngredients <= 0)
                    {
                        Console.WriteLine("Quantité invalide. Doit être un nombre positif.");
                        continue;
                    }

                    string reqQuantite = $@"
                        INSERT INTO Quantite (ID_Mets, IDIngredient, NbIngredients)
                        VALUES ('{idMet}', '{idIngredient}', {nbIngredients});";
                    Bdd.Executer(reqQuantite);
                    Console.WriteLine($"Ingrédient '{idIngredient}' ajouté au plat '{idMet}'.");
                }
                Console.WriteLine("Ingrédients pour le plat enregistrés.");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'ajout du plat ou de ses ingrédients : " + ex.Message);
            }
        }

        private static void AfficherIngredientsDisponibles()
        {
            Console.WriteLine("\n--- Liste des Ingrédients Disponibles ---");
            string req = "SELECT IDIngredient, NomIngredient FROM Ingredients ORDER BY NomIngredient;";
            using (var reader = Bdd.Lire(req))
            {
                if (!reader.HasRows)
                {
                    Console.WriteLine("Aucun ingrédient disponible dans la base.");
                    reader.Close();
                    return;
                }
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader.GetString("IDIngredient")} - Nom: {reader.GetString("NomIngredient")}");
                }
                reader.Close();
            }
            Console.WriteLine("-----------------------------------------\n");
        }

    }
}