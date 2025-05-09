using System;
using System.Collections.Generic;
using System.Linq;

namespace Projet_PSI
{
    public class Graphe<T>
    {
        private Dictionary<int, Noeud<T>> noeuds;// Associe ID -> Noeud<T>
        private List<Lien<T>> liens;// Liste des liens (arêtes)
        private int[,] matriceAdjacence;// Matrice d’adjacence
        private int taille;// Nombre maximal de nœuds (dimensions de la matrice)

        /// <summary>
        /// Dictionnaire des nœuds : associe un identifiant entier à un Noeud<T>.
        /// </summary>
        public Dictionary<int, Noeud<T>> Noeuds
        {
            get { return noeuds; }
        }

        /// <summary>
        /// Liste des liens du graphe.
        /// </summary>
        public List<Lien<T>> Liens
        {
            get { return liens; }
        }

        /// <summary>
        /// Constructeur du graphe générique.
        /// </summary>
        /// <param name="taille">Taille maximale (pour la matrice d’adjacence).</param>
        public Graphe(int taille)
        {
            this.taille = taille;
            this.noeuds = new Dictionary<int, Noeud<T>>();
            this.liens = new List<Lien<T>>();
            this.matriceAdjacence = new int[taille, taille];
        }

        /// <summary>
        /// Ajoute un nœud au graphe.
        /// </summary>
        /// <param name="id">Identifiant unique du nœud.</param>
        /// <param name="data">Donnée de type T à stocker dans le nœud.</param>
        public void AjouterNoeud(int id, T data)
        {
            if (!noeuds.ContainsKey(id))
            {
                noeuds[id] = new Noeud<T>(id, data);
            }
        }

        /// <summary>
        /// Ajoute un lien (arête) entre deux nœuds existants du graphe.
        /// </summary>
        /// <param name="id1">Identifiant du premier nœud.</param>
        /// <param name="id2">Identifiant du second nœud.</param>
        /// <param name="poids">Poids du lien (1 par défaut).</param>
        public void AjouterLien(int id1, int id2, int poids = 1)
        {
            if (!noeuds.ContainsKey(id1) || !noeuds.ContainsKey(id2)) return;

            var n1 = noeuds[id1];
            var n2 = noeuds[id2];

            liens.Add(new Lien<T>(n1, n2, poids));

            n1.AjouterVoisin(n2);
            n2.AjouterVoisin(n1);

            // Si un poids existe déjà, on garde le plus GRAND
            int actuel = matriceAdjacence[id1, id2];
            if (poids > actuel)
            {
                matriceAdjacence[id1, id2] = poids;
                matriceAdjacence[id2, id1] = poids;
            }
        }

        /// <summary>
        /// Vérifie si un lien existe entre deux nœuds (basé sur la matrice d’adjacence).
        /// </summary>
        /// <param name="id1">Identifiant du premier nœud.</param>
        /// <param name="id2">Identifiant du second nœud.</param>
        /// <returns>Vrai si un lien existe, faux sinon.</returns>
        public bool ExisteLien(int id1, int id2)
        {
            return matriceAdjacence[id1, id2] != 0;
        }

        /// <summary>
        /// Affiche la liste d’adjacence du graphe.
        /// </summary>
        /// <returns>Une chaîne représentant la liste d’adjacence.</returns>
        public string AfficherListeAdjacence()
        {
            string repo = "";
            foreach (var noeudKvp in noeuds)
            {
                var noeud = noeudKvp.Value;
                repo += $"\n {noeud.Data} -> ";
                foreach (var voisin in noeud.Voisins)
                {
                    repo += $"{voisin.Data}  ";
                }
            }
            return repo;
        }

        /// <summary>
        /// Affiche la matrice d’adjacence du graphe.
        /// </summary>
        /// <returns>Une chaîne représentant la matrice.</returns>
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
        /// Parcours en largeur (BFS) à partir du nœud d’identifiant idDepart.
        /// </summary>
        /// <param name="idDepart">Identifiant du nœud de départ.</param>
        /// <returns>Une chaîne représentant l’ordre de visite.</returns>
        public string ParcoursLargeur(int idDepart)
        {
            string repo = "\n";
            if (!noeuds.ContainsKey(idDepart)) return repo;

            HashSet<int> visite = new HashSet<int>();
            Queue<int> file = new Queue<int>();

            file.Enqueue(idDepart);
            visite.Add(idDepart);

            while (file.Count > 0)
            {
                int courantId = file.Dequeue();
                Noeud<T> courant = noeuds[courantId];

                repo += $"{courant.Data} ";

                foreach (var voisin in courant.Voisins)
                {
                    if (!visite.Contains(voisin.Id))
                    {
                        visite.Add(voisin.Id);
                        file.Enqueue(voisin.Id);
                    }
                }
            }
            return repo;
        }

        /// <summary>
        /// Parcours en profondeur (DFS) à partir du nœud d’identifiant idDepart.
        /// </summary>
        /// <param name="idDepart">Identifiant du nœud de départ.</param>
        /// <returns>Une chaîne représentant l’ordre de visite.</returns>
        public string ParcoursProfondeur(int idDepart)
        {
            string repo = "\n";
            if (!noeuds.ContainsKey(idDepart)) return repo;

            HashSet<int> visite = new HashSet<int>();
            Stack<int> pile = new Stack<int>();

            pile.Push(idDepart);

            while (pile.Count > 0)
            {
                int courantId = pile.Pop();
                if (!visite.Contains(courantId))
                {
                    visite.Add(courantId);
                    Noeud<T> courant = noeuds[courantId];
                    repo += $"{courant.Data} ";

                    foreach (var voisin in courant.Voisins)
                    {
                        if (!visite.Contains(voisin.Id))
                        {
                            pile.Push(voisin.Id);
                        }
                    }
                }
            }
            return repo;
        }

        /// <summary>
        /// Vérifie si le graphe est connexe : il existe un chemin entre tous les nœuds.
        /// </summary>
        /// <returns>Vrai si le graphe est connexe, faux sinon.</returns>
        public bool EstConnexe()
        {
            if (noeuds.Count == 0) return false;

            var visite = new HashSet<int>();
            var file = new Queue<int>();

            int premierNoeud = noeuds.Keys.First();

            file.Enqueue(premierNoeud);
            visite.Add(premierNoeud);

            while (file.Count > 0)
            {
                int courantId = file.Dequeue();
                Noeud<T> courantNode = noeuds[courantId];

                foreach (var voisin in courantNode.Voisins)
                {
                    if (!visite.Contains(voisin.Id))
                    {
                        visite.Add(voisin.Id);
                        file.Enqueue(voisin.Id);
                    }
                }
            }

            return (visite.Count == noeuds.Count);
        }

        /// <summary>
        /// Détermine si le graphe contient au moins un cycle.
        /// </summary>
        /// <returns>Vrai si un cycle est détecté, faux sinon.</returns>
        public bool ContientCycle()
        {
            var visite = new HashSet<int>();
            var recStack = new HashSet<int>();

            foreach (var idNoeud in noeuds.Keys)
            {
                if (!visite.Contains(idNoeud))
                {
                    if (DetecterCycle(idNoeud, visite, recStack, -1))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Fonction récursive pour détecter un cycle en DFS.
        /// </summary>
        /// <param name="idNoeud">Identifiant du nœud en cours de traitement.</param>
        /// <param name="visite">Ensemble des nœuds déjà visités.</param>
        /// <param name="recStack">Chemin actuel (pile de récursion).</param>
        /// <param name="parent">ID du nœud parent (pour éviter de confondre arête arrière et arête vers le parent).</param>
        private bool DetecterCycle(int idNoeud, HashSet<int> visite, HashSet<int> recStack, int parent)
        {
            visite.Add(idNoeud);
            recStack.Add(idNoeud);

            var courant = noeuds[idNoeud];
            foreach (var voisin in courant.Voisins)
            {
                if (!visite.Contains(voisin.Id))
                {
                    if (DetecterCycle(voisin.Id, visite, recStack, idNoeud))
                        return true;
                }
                else if (voisin.Id != parent && recStack.Contains(voisin.Id))
                {
                    return true;
                }
            }

            recStack.Remove(idNoeud);
            return false;
        }

        /// <summary>
        /// Renvoie la liste des nœuds (IDs) constituant le plus court chemin entre startId et endId,
        /// selon Dijkstra. (Suppose que les poids sont positifs.)
        /// </summary>
        public List<int> CheminDijkstra(int startId, int endId)
        {
            var dist = new Dictionary<int, int>();
            var prev = new Dictionary<int, int?>();
            var nonVisites = new HashSet<int>(noeuds.Keys);

            foreach (var id in noeuds.Keys)
            {
                dist[id] = int.MaxValue;
                prev[id] = null;
            }
            dist[startId] = 0;

            while (nonVisites.Count > 0)
            {
                int u = -1;
                int minDist = int.MaxValue;
                foreach (int id in nonVisites)
                {
                    if (dist[id] < minDist)
                    {
                        minDist = dist[id];
                        u = id;
                    }
                }

                if (u == -1 || u == endId) break;
                nonVisites.Remove(u);

                foreach (var voisin in noeuds[u].Voisins)
                {
                    int newDist = dist[u] + matriceAdjacence[u, voisin.Id];
                    if (newDist < dist[voisin.Id])
                    {
                        dist[voisin.Id] = newDist;
                        prev[voisin.Id] = u;
                    }
                }
            }

            var chemin = new List<int>();
            int? current = endId;
            while (current != null)
            {
                chemin.Add(current.Value);
                current = prev[current.Value];
            }
            chemin.Reverse();

            if (dist[endId] == int.MaxValue) chemin.Clear();

            return chemin;
        }

        /// <summary>
        /// Renvoie la liste (IDs) du plus court chemin entre startId et endId,
        /// selon Bellman-Ford (gère éventuellement des poids négatifs, sans cycle négatif).
        /// </summary>
        public List<int> CheminBellmanFord(int startId, int endId)
        {
            var dist = new Dictionary<int, int>();
            var prev = new Dictionary<int, int?>();

            foreach (var id in noeuds.Keys)
            {
                dist[id] = int.MaxValue;
                prev[id] = null;
            }
            dist[startId] = 0;

            for (int i = 0; i < noeuds.Count - 1; i++)
            {
                bool changed = false;
                foreach (var lien in liens)
                {
                    int u = lien.Noeud1.Id;
                    int v = lien.Noeud2.Id;
                    int w = lien.Poids;

                    if (dist[u] != int.MaxValue && dist[u] + w < dist[v])
                    {
                        dist[v] = dist[u] + w;
                        prev[v] = u;
                        changed = true;
                    }
                    if (dist[v] != int.MaxValue && dist[v] + w < dist[u])
                    {
                        dist[u] = dist[v] + w;
                        prev[u] = v;
                        changed = true;
                    }
                }
                if (!changed) break;
            }

            var chemin = new List<int>();
            int? current = endId;
            while (current != null)
            {
                chemin.Add(current.Value);
                current = prev[current.Value];
            }
            chemin.Reverse();

            if (dist[endId] == int.MaxValue) chemin.Clear();

            return chemin;
        }

        /// <summary>
        /// Renvoie la liste (IDs) du chemin entre startId et endId
        /// en utilisant l'algorithme Floyd-Warshall (all pairs shortest path).
        /// </summary>
        public List<int> CheminFloydWarshall(int startId, int endId)
        {
            int n = this.taille;
            int[,] distFW = new int[n, n];
            int[,] nextFW = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) distFW[i, j] = 0;
                    else
                    {
                        if (matriceAdjacence[i, j] == 0) distFW[i, j] = 100000000;
                        else distFW[i, j] = matriceAdjacence[i, j];
                    }
                    if (matriceAdjacence[i, j] != 0) nextFW[i, j] = j;
                    else nextFW[i, j] = -1;
                }
            }

            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (distFW[i, k] + distFW[k, j] < distFW[i, j])
                        {
                            distFW[i, j] = distFW[i, k] + distFW[k, j];
                            nextFW[i, j] = nextFW[i, k];
                        }
                    }
                }
            }

            if (distFW[startId, endId] >= 100000000) return new List<int>();

            var path = new List<int>();
            int current = startId;
            while (current != endId)
            {
                path.Add(current);
                current = nextFW[current, endId];
                if (current == -1) break;
            }
            path.Add(endId);

            return path;
        }
    }
}
