namespace Projet_PSI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Graphe graphe = new Graphe();

            // Ajout des Noeuds
            graphe.AjouterNoeud(1, "Alice");
            graphe.AjouterNoeud(2, "Bob");
            graphe.AjouterNoeud(3, "Charlie");

            // Ajout des Liens
            graphe.AjouterLien(1, 2);
            graphe.AjouterLien(2, 3);
            graphe.AjouterLien(1, 3);

            // Affichage du Graphe
            graphe.AfficherGraphe();
        }
    }
}
