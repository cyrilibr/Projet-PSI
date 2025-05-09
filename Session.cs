namespace Projet_PSI
{
    /// <summary>
    /// Classe statique gérant les informations de session de l'utilisateur.
    /// </summary>
    public static class Session
    {
        private static int idUtilisateur;
        private static string email;
        private static string role;

        /// <summary>
        /// Obtient ou définit l'identifiant de l'utilisateur connecté.
        /// </summary>
        public static int IdUtilisateur
        {
            get { return idUtilisateur; }
            set { idUtilisateur = value; }
        }

        /// <summary>
        /// Obtient ou définit l'email de connexion de l'utilisateur (affichage).
        /// </summary>
        public static string Email
        {
            get { return email; }
            set { email = value; }
        }

        /// <summary>
        /// Obtient ou définit le rôle de l'utilisateur ("admin", "client", "cuisinier").
        /// </summary>
        public static string Role
        {
            get { return role; }
            set { role = value; }
        }

        /// <summary>
        /// Réinitialise la session, en réinitialisant les attributs aux valeurs par défaut.
        /// </summary>
        public static void Reinitialiser()
        {
            idUtilisateur = -1;
            email = null;
            role = null;
        }

        /// <summary>
        /// Vérifie si un utilisateur est connecté.
        /// </summary>
        /// <returns>True si un rôle est défini, sinon false.</returns>
        public static bool EstConnecte()
        {
            return role != null;
        }

        /// <summary>
        /// Fournit des informations de session.
        /// </summary>
        public static string Info()
        {
            return $"Connecté : {email} ({role})";
        }
    }
}
