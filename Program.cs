namespace Projet_PSI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Graphe graphe = new Graphe(5);

            // Ajout des Noeuds
            graphe.AjouterNoeud(0, "Alice");
            graphe.AjouterNoeud(1, "Bob");
            graphe.AjouterNoeud(2, "Charlie");
            graphe.AjouterNoeud(3, "David");
            graphe.AjouterNoeud(4, "Emma");

            // Ajout des Liens
            graphe.AjouterLien(0, 1);
            graphe.AjouterLien(0, 2);
            graphe.AjouterLien(1, 3);
            graphe.AjouterLien(2, 4);
            graphe.AjouterLien(3, 4);

            // Affichage du Graphe (Liste d'adjacence)
            Console.WriteLine("Liste d'adjacence :");
            graphe.AfficherGraphe();

            // Vérification des liens avec la Matrice
            Console.WriteLine("\nVérification des connexions:");
            Console.WriteLine($"Alice est connectée à Bob ? {graphe.ExisteLien(0, 1)}");
            Console.WriteLine($"Alice est connectée à David ? {graphe.ExisteLien(0, 3)}");

            // Affichage de la Matrice d'Adjacence
            Console.WriteLine("\n");
            graphe.AfficherMatriceAdjacence();

            // Test des parcours
            Console.WriteLine("\nParcours en largeur (BFS) depuis Alice :");
            graphe.ParcoursLargeur(0);

            Console.WriteLine("\nParcours en profondeur (DFS) depuis Alice :");
            graphe.ParcoursProfondeur(0);
        }
    }
}
