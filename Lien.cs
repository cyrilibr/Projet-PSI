using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_PSI
{
    public class Lien
    {
        private Noeud noeud1;
        private Noeud noeud2;
        private int poids;

        public Noeud Noeud1 
        { 
            get { return noeud1; } 
        }
        public Noeud Noeud2 
        { 
            get { return noeud2; } 
        }
        public int Poids 
        { 
            get { return poids; } 
        }

        public Lien(Noeud noeud1, Noeud noeud2, int poids = 1)
        {
            this.noeud1 = noeud1;
            this.noeud2 = noeud2;
            this.poids = poids;
        }

        public override string ToString()
        {
            return $"Lien entre {noeud1.Nom} et {noeud2.Nom}, Poids: {poids}";
        }
    }
}
