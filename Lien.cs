namespace Projet_PSI
{
    /// <summary>
    /// Classe générique représentant un lien (arête) entre deux nœuds.
    /// </summary>
    /// <typeparam name="T">Le type de donnée contenu dans les nœuds</typeparam>
    public class Lien<T>
    {
        public Noeud<T> noeud1;
        public Noeud<T> noeud2;
        public int poids;
        public bool bidirectionnel;

        /// <summary>
        /// Obtient le premier nœud du lien.
        /// </summary>
        public Noeud<T> Noeud1
        {
            get { return noeud1; }
        }

        /// <summary>
        /// Obtient le deuxième nœud du lien.
        /// </summary>
        public Noeud<T> Noeud2
        {
            get { return noeud2; }
        }

        /// <summary>
        /// Obtient le poids du lien.
        /// </summary>
        public int Poids
        {
            get { return poids; }
        }

        /// <summary>
        /// Indique si la liaison est à double sens
        /// </summary>
        public bool Bidirectionnel
        {
            get { return bidirectionnel; }
        }

        /// <summary>
        /// Constructeur du lien.
        /// </summary>
        /// <param name="noeud1">Premier nœud</param>
        /// <param name="noeud2">Deuxième nœud</param>
        /// <param name="poids">Poids de l’arête (ex : temps de trajet)</param>
        /// <param name="bidirectionnel">Indique si la liaison est à double sens</param>
        public Lien(Noeud<T> noeud1, Noeud<T> noeud2, int poids = 1, bool bidirectionnel = false)
        {
            this.noeud1 = noeud1;
            this.noeud2 = noeud2;
            this.poids = poids;
            this.bidirectionnel = bidirectionnel;
        }

        /// <summary>
        /// Retourne une représentation textuelle du lien.
        /// </summary>
        /// <returns>Une chaîne de caractères représentant le lien.</returns>
        public override string ToString()
        {
            return $"Lien entre {noeud1.Data} et {noeud2.Data}, Poids: {poids}";
        }
    }
}