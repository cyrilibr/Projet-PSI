using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_PSI
{
    public class Lien<T>
    {
        private Noeud<T> noeud1;//Premier nœud du lien
        private Noeud<T> noeud2;//Deuxième nœud du lien
        private int poids;//Poids du lien (coût, distance, etc.)

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
        /// Constructeur de la classe Lien.
        /// </summary>
        /// <param name="noeud1">Premier nœud du lien.</param>
        /// <param name="noeud2">Deuxième nœud du lien.</param>
        /// <param name="poids">Poids du lien (1 par défaut).</param>
        public Lien(Noeud<T> noeud1, Noeud<T> noeud2, int poids = 1)
        {
            this.noeud1 = noeud1;
            this.noeud2 = noeud2;
            this.poids = poids;
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
