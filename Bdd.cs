using MySql.Data.MySqlClient;
using System;

namespace Projet_PSI.Utils
{
    public static class Bdd
    {
        private static string connStr = "server=localhost;user=root;password=root;database=psi;";

        public static MySqlConnection Connexion()
        {
            var conn = new MySqlConnection(connStr);
            conn.Open();
            return conn;
        }

        public static void Executer(string requete)
        {
            using var conn = Connexion();
            using var cmd = new MySqlCommand(requete, conn);
            cmd.ExecuteNonQuery();
        }

        public static MySqlDataReader Lire(string requete)
        {
            var conn = Connexion();
            var cmd = new MySqlCommand(requete, conn);
            return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
    }
}