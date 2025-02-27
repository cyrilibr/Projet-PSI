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

        public string AfficherListeAdjacence()
        {
            string repo = "";

            foreach (var noeud in noeuds.Values)
            {
                repo += $"\n{noeud.Nom} -> ";
                foreach (var voisin in noeud.Voisins)
                {
                    repo += $"{voisin.Nom} ";
                }
            }
            return repo;
        }

        public string AfficherMatriceAdjacence()
        {
            string repo = "\n";

            for (int i = 0; i < taille; i++)
            {
                for (int j = 0; j < taille; j++)
                {
                    repo += $"{matriceAdjacence[i, j]} ";
                }
                repo += $"\n";
            }
            return repo;
        }

        // Parcours en largeur (BFS)
        public string ParcoursLargeur(int idDepart)
        {
            string repo = "\n";

            if (!noeuds.ContainsKey(idDepart)) return repo;
            HashSet<int> visite = new HashSet<int>();
            Queue<Noeud> file = new Queue<Noeud>();

            file.Enqueue(noeuds[idDepart]);
            visite.Add(idDepart);

            while (file.Count > 0)
            {
                Noeud courant = file.Dequeue();
                repo += $"{courant.Nom} ";

                foreach (var voisin in courant.Voisins)
                {
                    if (!visite.Contains(voisin.Id))
                    {
                        visite.Add(voisin.Id);
                        file.Enqueue(voisin);
                    }
                }
            }
            repo += $"\n";
            return repo;
        }

        // Parcours en profondeur (DFS)
        public string ParcoursProfondeur(int idDepart)
        {
            string repo = "\n";

            if (!noeuds.ContainsKey(idDepart)) return repo;
            HashSet<int> visite = new HashSet<int>();
            Stack<Noeud> pile = new Stack<Noeud>();

            pile.Push(noeuds[idDepart]);

            while (pile.Count > 0)
            {
                Noeud courant = pile.Pop();
                if (!visite.Contains(courant.Id))
                {
                    visite.Add(courant.Id);
                    repo += $"{courant.Nom} ";

                    foreach (var voisin in courant.Voisins)
                    {
                        if (!visite.Contains(voisin.Id))
                        {
                            pile.Push(voisin);
                        }
                    }
                }
            }

            repo += $"\n";
            return repo;
        }

        public bool EstConnexe()
        {
            if (noeuds.Count == 0) return false;

            HashSet<int> visite = new HashSet<int>();
            Queue<int> file = new Queue<int>();
            int premierNoeud = noeuds.Keys.First();

            file.Enqueue(premierNoeud);
            visite.Add(premierNoeud);

            while (file.Count > 0)
            {
                int courant = file.Dequeue();
                foreach (var voisin in noeuds[courant].Voisins)
                {
                    if (!visite.Contains(voisin.Id))
                    {
                        visite.Add(voisin.Id);
                        file.Enqueue(voisin.Id);
                    }
                }
            }

            return visite.Count == noeuds.Count;
        }

        public bool ContientCycle()
        {
            HashSet<int> visite = new HashSet<int>();
            HashSet<int> recStack = new HashSet<int>();

            foreach (var noeud in noeuds.Keys)
            {
                if (DetecterCycle(noeud, visite, recStack, -1))
                    return true;
            }
            return false;
        }

        private bool DetecterCycle(int noeud, HashSet<int> visite, HashSet<int> recStack, int parent)
        {
            visite.Add(noeud);
            recStack.Add(noeud);

            foreach (var voisin in noeuds[noeud].Voisins)
            {
                if (!visite.Contains(voisin.Id))
                {
                    if (DetecterCycle(voisin.Id, visite, recStack, noeud))
                        return true;
                }
                else if (voisin.Id != parent && recStack.Contains(voisin.Id))
                {
                    return true;
                }
            }

            recStack.Remove(noeud);
            return false;
        }
    }
}
