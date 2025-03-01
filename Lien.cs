using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_PSI
{
    public class Lien
    {
        private Noeud noeud1; // Premier noeud du lien
        private Noeud noeud2; // Deuxieme noeud du lien
        private int poids; // Poids du lien

        /// <summary>
        /// Obtient le premier noeud du lien
        /// </summary>
        public Noeud Noeud1
        {
            get { return noeud1; }
        }

        /// <summary>
        /// Obtient le deuxieme noeud du lien
        /// </summary>
        public Noeud Noeud2
        {
            get { return noeud2; }
        }

        /// <summary>
        /// Obtient le poids du lien
        /// </summary>
        public int Poids
        {
            get { return poids; }
        }

        /// <summary>
        /// Constructeur de la classe Lien
        /// </summary>
        /// <param name="noeud1">Premier noeud du lien</param>
        /// <param name="noeud2">Deuxieme noeud du lien</param>
        /// <param name="poids">Poids du lien (1 par defaut)</param>
        public Lien(Noeud noeud1, Noeud noeud2, int poids = 1)
        {
            this.noeud1 = noeud1;
            this.noeud2 = noeud2;
            this.poids = poids;
        }

        /// <summary>
        /// Retourne une representation du lien
        /// </summary>
        /// <returns>Une chaine de caracteres representant le lien</returns>
        public override string ToString()
        {
            return $"Lien entre {noeud1.Nom} et {noeud2.Nom}, Poids: {poids}";
        }
    }
}
