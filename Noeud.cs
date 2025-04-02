using System.Collections.Generic;

namespace Projet_PSI
{
    /// <summary>
    /// Classe générique représentant un nœud dans un graphe.
    /// </summary>
    /// <typeparam name="T">Le type de donnée contenu dans le nœud (ex. Station)</typeparam>
    public class Noeud<T>
    {
        public int Id { get; private set; }
        public T Data { get; private set; }
        public List<Noeud<T>> Voisins { get; private set; }

        public Noeud(int id, T data)
        {
            Id = id;
            Data = data;
            Voisins = new List<Noeud<T>>();
        }

        /// <summary>
        /// Ajoute un voisin si ce n'est pas déjà présent.
        /// </summary>
        public void AjouterVoisin(Noeud<T> voisin)
        {
            if (!Voisins.Contains(voisin))
            {
                Voisins.Add(voisin);
            }
        }

        public override string ToString()
        {
            return $"Noeud {Id}: {Data}";
        }
    }
}
