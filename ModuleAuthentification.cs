using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    public static class ModuleAuthentification
    {
        public static void Lancer(Graphe<Station> graphe)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Connexion ===");
                Console.Write("Email ou nom : ");
                string email = Console.ReadLine();
                Console.Write("Mot de passe : ");
                string mdp = Console.ReadLine();

                // Mode admin (hardcodé)
                if (email == "admin" && mdp == "adminpass")
                {
                    Session.IdUtilisateur = 0;
                    Session.Email = "admin@psi.local";
                    Session.Role = "admin";
                    Console.WriteLine("Connexion admin réussie !");
                    Console.ReadKey();
                    ModulePrincipal.Lancer(graphe); // ancien menu admin
                    return;
                }

                // Recherche dans la base Tier
                string requete = $"SELECT ID FROM Tier WHERE EMAIL = '{email}' AND MDP = '{mdp}'";
                using var reader = Bdd.Lire(requete);

                if (!reader.Read())
                {
                    Console.WriteLine("Identifiants incorrects.");
                    reader.Close();
                    Console.ReadKey();
                    continue;
                }

                int id = reader.GetInt32("ID");
                reader.Close();

                // Vérifie si client
                bool estClient = ExisteDansTable("Client", id);
                bool estCuisinier = ExisteDansTable("Cuisinier", id);

                if (estClient)
                {
                    Session.IdUtilisateur = id;
                    Session.Email = email;
                    Session.Role = "client";
                    Console.WriteLine("Connexion client réussie !");
                    Console.ReadKey();
                    ModuleClient.LancerClient(graphe);
                    return;
                }
                else if (estCuisinier)
                {
                    Session.IdUtilisateur = id;
                    Session.Email = email;
                    Session.Role = "cuisinier";
                    Console.WriteLine("Connexion cuisinier réussie !");
                    Console.ReadKey();
                    ModuleCuisinier.LancerCuisinier(graphe);
                    return;
                }
                else
                {
                    Console.WriteLine("Aucun rôle associé à ce compte.");
                    Console.ReadKey();
                }
            }
        }

        private static bool ExisteDansTable(string table, int id)
        {
            string requete = $"SELECT 1 FROM {table} WHERE ID = {id} LIMIT 1";
            using var reader = Bdd.Lire(requete);
            bool existe = reader.Read();
            reader.Close();
            return existe;
        }
    }
}
