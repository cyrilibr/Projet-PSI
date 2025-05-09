using MySql.Data.MySqlClient;
using System;

namespace Projet_PSI.Utils
{
    /// <summary>
    /// Fournit des méthodes pour se connecter et exécuter des requêtes SQL
    /// contre la base de données MySQL du projet.
    /// </summary>
    public static class Bdd
    {
        /// <summary>
        /// Chaîne de connexion vers la base de données MySQL.
        /// </summary>
        private static string connStr = "server=localhost;user=root;password=root;database=psi;";

        /// <summary>
        /// Ouvre et retourne une connexion MySQL ouverte.
        /// </summary>
        /// <returns>Instance de MySqlConnection ouverte.</returns>
        public static MySqlConnection Connexion()
        {
            var conn = new MySqlConnection(connStr);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Exécute une commande SQL ne retournant pas de résultat (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="requete">Requête SQL à exécuter.</param>
        public static void Executer(string requete)
        {
            using var conn = Connexion();
            using var cmd = new MySqlCommand(requete, conn);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Exécute une requête SQL et retourne un lecteur MySqlDataReader pour parcourir le résultat.
        /// </summary>
        /// <param name="requete">Requête SQL SELECT à exécuter.</param>
        /// <returns>Instance de MySqlDataReader, la connexion se ferme à la fin du parcours.</returns>
        public static MySqlDataReader Lire(string requete)
        {
            var conn = Connexion();
            var cmd = new MySqlCommand(requete, conn);
            return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
    }
}
