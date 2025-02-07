using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_PSI
{
    public class Graphe
    {
        private Dictionary<int, Noeud> noeuds;
        private List<Lien> liens;

        public Dictionary<int, Noeud> Noeuds 
        { 
            get { return noeuds; } 
        }
        public List<Lien> Liens 
        { 
            get { return liens; } 
        }

        public Graphe()
        {
            this.noeuds = new Dictionary<int, Noeud>();
            this.liens = new List<Lien>();
        }

        public void AjouterNoeud(int id, string nom)
        {
            if (!noeuds.ContainsKey(id))
            {
                noeuds[id] = new Noeud(id, nom);
            }
        }

        public void AjouterLien(int id1, int id2, int poids = 1)
        {
            if (noeuds.ContainsKey(id1) && noeuds.ContainsKey(id2))
            {
                Noeud n1 = noeuds[id1];
                Noeud n2 = noeuds[id2];
                Lien lien = new Lien(n1, n2, poids);
                liens.Add(lien);
                n1.AjouterVoisin(n2);
                n2.AjouterVoisin(n1);
            }
        }

        public void AfficherGraphe()
        {
            foreach (var noeud in noeuds.Values)
            {
                Console.Write(noeud.Nom + " -> ");
                foreach (var voisin in noeud.Voisins)
                {
                    Console.Write(voisin.Nom + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
