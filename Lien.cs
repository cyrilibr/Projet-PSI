namespace Projet_PSI
{
    /// <summary>
    /// Classe générique représentant un lien (arête) entre deux nœuds.
    /// </summary>
    /// <typeparam name="T">Le type de donnée contenu dans les nœuds</typeparam>
    public class Lien<T>
    {
        public Noeud<T> Noeud1 { get; private set; }
        public Noeud<T> Noeud2 { get; private set; }
        public int Poids { get; private set; }
        public bool Bidirectionnel { get; private set; }

        /// <summary>
        /// Constructeur du lien.
        /// </summary>
        /// <param name="noeud1">Premier nœud</param>
        /// <param name="noeud2">Deuxième nœud</param>
        /// <param name="poids">Poids de l’arête (ex : temps de trajet)</param>
        /// <param name="bidirectionnel">Indique si la liaison est à double sens</param>
        public Lien(Noeud<T> noeud1, Noeud<T> noeud2, int poids = 1, bool bidirectionnel = false)
        {
            Noeud1 = noeud1;
            Noeud2 = noeud2;
            Poids = poids;
            Bidirectionnel = bidirectionnel;
        }

        public override string ToString()
        {
            return $"Lien entre {Noeud1.Data} et {Noeud2.Data}, Poids: {Poids}";
        }
    }
}
