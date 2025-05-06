namespace Projet_PSI
{
    public static class Session
    {
        // ID de l'utilisateur connecté
        public static int IdUtilisateur { get; set; }

        // Email de connexion (utile pour affichage)
        public static string Email { get; set; }

        // Rôle possible : "admin", "client", "cuisinier"
        public static string Role { get; set; }

        // Réinitialise la session (utile à la déconnexion)
        public static void Reinitialiser()
        {
            IdUtilisateur = -1;
            Email = null;
            Role = null;
        }

        // Vérifie si un utilisateur est connecté
        public static bool EstConnecte()
        {
            return Role != null;
        }

        // Affiche une info utile (debug ou menu)
        public static string Info()
        {
            return $"Connecté : {Email} ({Role})";
        }
    }
}
