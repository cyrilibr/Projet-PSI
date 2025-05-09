using Projet_PSI.Utils;
using System;

namespace Projet_PSI.Modules
{
    /// <summary>
    /// Module gérant l'authentification des utilisateurs et redirigeant
    /// vers leur interface respective (admin, client, cuisinier).
    /// </summary>
    public static class ModuleAuthentification
    {
        /// <summary>
        /// Lance la boucle de connexion et gère l'authentification selon le rôle.
        /// </summary>
        /// <param name="graphe">Le graphe des stations utilisé pour les modules suivants.</param>
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

                // Authentification admin (identifiants hardcodés)
                if (email == "admin" && mdp == "adminpass")
                {
                    Session.IdUtilisateur = 0;
                    Session.Email = "admin@psi.local";
                    Session.Role = "admin";
                    Console.WriteLine("Connexion admin réussie !");
                    Console.ReadKey();
                    ModulePrincipal.Lancer(graphe);
                    return;
                }

                // Recherche de l'utilisateur dans la table Tier
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

                // Détermination du rôle (client ou cuisinier)
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

        /// <summary>
        /// Vérifie l'existence d'un ID dans une table donnée.
        /// </summary>
        /// <param name="table">Nom de la table SQL.</param>
        /// <param name="id">Identifiant à vérifier.</param>
        /// <returns>True si l'enregistrement existe, False sinon.</returns>
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
