// Projet_PSI/Noeud.cs
using System.Collections.Generic;

namespace Projet_PSI
{
    public class Noeud<T>
    {
        public int Id { get; set; }
        public T Data { get; set; }

        // Dans un graphe orienté, Voisins représente les successeurs (nœuds vers lesquels on peut aller)
        public List<Noeud<T>> Voisins { get; private set; }

        public Noeud(int id, T data)
        {
            Id = id;
            Data = data;
            Voisins = new List<Noeud<T>>();
        }

        // Ajoute un voisin/successeur
        public void AjouterVoisin(Noeud<T> voisin)
        {
            if (!Voisins.Contains(voisin))
            {
                Voisins.Add(voisin);
            }
        }

        public void RetirerVoisin(Noeud<T> voisin)
        {
            Voisins.Remove(voisin);
        }


        public override string ToString()
        {
            return $"Noeud {Id} : {Data}";
        }
    }
}