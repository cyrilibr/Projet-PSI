using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_PSI
{
    public class Graphe
    {
        private Dictionary<int, Noeud> noeuds; // Dictionnaire des nœuds du graphe
        private List<Lien> liens; // Liste des liens entre les nœuds
        private int[,] matriceAdjacence; // Matrice d'adjacence pour représenter les connexions
        private int taille; // Nombre total de nœuds possibles

        /// <summary>
        /// Obtient les nœuds du graphe
        /// </summary>
        public Dictionary<int, Noeud> Noeuds 
        { 
            get { return noeuds; } 
        }

        /// <summary>
        /// Obtient la liste des liens du graphe
        /// </summary>
        public List<Lien> Liens 
        { 
            get { return liens; } 
        }

        /// <summary>
        /// Constructeur de la classe Graphe
        /// </summary>
        /// <param name="taille">Nombre maximal de nœuds dans le graphe</param>
        public Graphe(int taille)
        {
            this.taille = taille;
            this.noeuds = new Dictionary<int, Noeud>();
            this.liens = new List<Lien>();
            this.matriceAdjacence = new int[taille, taille];
        }

        /// <summary>
        /// Ajoute un nœud au graphe
        /// </summary>
        /// <param name="id">Identifiant unique du nœud</param>
        /// <param name="nom">Nom du nœud</param>
        public void AjouterNoeud(int id, string nom)
        {
            if (!noeuds.ContainsKey(id))
            {
                noeuds[id] = new Noeud(id, nom);
            }
        }

        /// <summary>
        /// Ajoute un lien entre deux nœuds du graphe
        /// </summary>
        /// <param name="id1">Identifiant du premier nœud</param>
        /// <param name="id2">Identifiant du second nœud</param>
        /// <param name="poids">Poids du lien (1 par défaut)</param>
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

        /// <summary>
        /// Verifie si un lien existe entre deux nœuds
        /// </summary>
        /// <param name="id1">Identifiant du premier nœud</param>
        /// <param name="id2">Identifiant du second nœud</param>
        /// <returns>Vrai si le lien existe, sinon faux</returns>
        public bool ExisteLien(int id1, int id2)
        {
            return matriceAdjacence[id1, id2] != 0;
        }

        /// <summary>
        /// Retourne la liste d'adjacence du graphe
        /// </summary>
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

        /// <summary>
        /// Retourne la matrice d'adjacence du graphe
        /// </summary>
        public string AfficherMatriceAdjacence()
        {
            string repo = "\n";
            for (int i = 0; i < taille; i++)
            {
                for (int j = 0; j < taille; j++)
                {
                    repo += $"{matriceAdjacence[i, j]} ";
                }
                repo += "\n";
            }
            return repo;
        }

        /// <summary>
        /// Effectue un parcours en largeur (BFS) a partir d'un nœud donne
        /// </summary>
        /// <param name="idDepart">Identifiant du nœud de depart</param>
        /// <returns>Ordre de parcours</returns>
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
            return repo;
        }

        /// <summary>
        /// Effectue un parcours en profondeur (DFS) a partir d'un nœud donne
        /// </summary>
        /// <param name="idDepart">Identifiant du nœud de depart</param>
        /// <returns>Ordre de parcours</returns>
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
            return repo;
        }

        /// <summary>
        /// Verifie si le graphe est connexe
        /// </summary>
        /// <returns>Vrai si le graphe est connexe, sinon faux</returns>
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

        /// <summary>
        /// Verifie si le graphe contient un cycle
        /// </summary>
        /// <returns>Vrai si le graphe contient un cycle, sinon faux</returns>
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
                if (!visite.Contains(voisin.Id) && DetecterCycle(voisin.Id, visite, recStack, noeud))
                    return true;
                else if (voisin.Id != parent && recStack.Contains(voisin.Id))
                    return true;
            }
            recStack.Remove(noeud);
            return false;
        }
    }
}
