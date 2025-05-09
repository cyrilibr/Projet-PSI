// Projet_PSI/Lien.cs
namespace Projet_PSI
{
    public class Lien<T>
    {
        public Noeud<T> Noeud1 { get; private set; }
        public Noeud<T> Noeud2 { get; private set; } // Pour un lien orienté Noeud1 -> Noeud2
        public int Poids { get; private set; }
        public bool Bidirectionnel { get; private set; } // Indique si un lien inverse implicite de même poids existe

        public Lien(Noeud<T> noeud1, Noeud<T> noeud2, int poids = 1, bool bidirectionnel = false)
        {
            Noeud1 = noeud1;
            Noeud2 = noeud2;
            Poids = poids;
            Bidirectionnel = bidirectionnel;
        }

        public override string ToString()
        {
            string direction = Bidirectionnel ? "<->" : "->";
            return $"Lien {Noeud1.Data} {direction} {Noeud2.Data}, Poids: {Poids}";
        }
    }
}