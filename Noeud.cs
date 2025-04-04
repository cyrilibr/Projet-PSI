using System.Collections.Generic;

namespace Projet_PSI
{
    /// <summary>
    /// Représente un nœud générique dans un graphe.
    /// </summary>
    /// <typeparam name="T">Type des données contenues dans le nœud</typeparam>
    public class Noeud<T>
    {
        /// <summary>
        /// Identifiant unique du nœud.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Données associées au nœud.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Liste des nœuds voisins.
        /// </summary>
        public List<Noeud<T>> Voisins { get; private set; }

        /// <summary>
        /// Constructeur d’un nœud.
        /// </summary>
        /// <param name="id">Identifiant du nœud</param>
        /// <param name="data">Données du nœud</param>
        public Noeud(int id, T data)
        {
            Id = id;
            Data = data;
            Voisins = new List<Noeud<T>>();
        }

        /// <summary>
        /// Ajoute un voisin s’il n’est pas déjà présent.
        /// </summary>
        /// <param name="voisin">Nœud voisin à ajouter</param>
        public void AjouterVoisin(Noeud<T> voisin)
        {
            if (!Voisins.Contains(voisin))
            {
                Voisins.Add(voisin);
            }
        }

        public override string ToString()
        {
            return $"Noeud {Id} : {Data}";
        }
    }
}