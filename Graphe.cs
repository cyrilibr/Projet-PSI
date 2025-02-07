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
        private int[,] matriceAdjacence;
        private int taille;

        public Dictionary<int, Noeud> Noeuds { get { return noeuds; } }
        public List<Lien> Liens { get { return liens; } }

        public Graphe(int taille)
        {
            this.taille = taille;
            this.noeuds = new Dictionary<int, Noeud>();
            this.liens = new List<Lien>();
            this.matriceAdjacence = new int[taille, taille];
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

                matriceAdjacence[id1, id2] = poids;
                matriceAdjacence[id2, id1] = poids;
            }
        }

        public bool ExisteLien(int id1, int id2)
        {
            return matriceAdjacence[id1, id2] != 0;
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

        public void AfficherMatriceAdjacence()
        {
            Console.WriteLine("Matrice d'Adjacence:");
            for (int i = 0; i < taille; i++)
            {
                for (int j = 0; j < taille; j++)
                {
                    Console.Write(matriceAdjacence[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        // Parcours en largeur (BFS)
        public void ParcoursLargeur(int idDepart)
        {
            if (!noeuds.ContainsKey(idDepart)) return;
            HashSet<int> visite = new HashSet<int>();
            Queue<Noeud> file = new Queue<Noeud>();

            file.Enqueue(noeuds[idDepart]);
            visite.Add(idDepart);

            while (file.Count > 0)
            {
                Noeud courant = file.Dequeue();
                Console.Write(courant.Nom + " ");

                foreach (var voisin in courant.Voisins)
                {
                    if (!visite.Contains(voisin.Id))
                    {
                        visite.Add(voisin.Id);
                        file.Enqueue(voisin);
                    }
                }
            }
            Console.WriteLine();
        }

        // Parcours en profondeur (DFS)
        public void ParcoursProfondeur(int idDepart)
        {
            if (!noeuds.ContainsKey(idDepart)) return;
            HashSet<int> visite = new HashSet<int>();
            Stack<Noeud> pile = new Stack<Noeud>();

            pile.Push(noeuds[idDepart]);

            while (pile.Count > 0)
            {
                Noeud courant = pile.Pop();
                if (!visite.Contains(courant.Id))
                {
                    visite.Add(courant.Id);
                    Console.Write(courant.Nom + " ");

                    foreach (var voisin in courant.Voisins)
                    {
                        if (!visite.Contains(voisin.Id))
                        {
                            pile.Push(voisin);
                        }
                    }
                }
            }
            Console.WriteLine();
        }

    }
}
