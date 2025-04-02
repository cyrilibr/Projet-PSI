using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projet_PSI
{
    /// <summary>
    /// Classe générique représentant un graphe.
    /// </summary>
    /// <typeparam name="T">Le type de donnée contenu dans chaque nœud</typeparam>
    public class Graphe<T>
    {
        public Dictionary<int, Noeud<T>> Noeuds { get; private set; }
        public List<Lien<T>> Liens { get; private set; }
        // Représente la matrice d’adjacence sous forme de dictionnaire.
        private Dictionary<int, Dictionary<int, int>> adjacence;

        public Graphe()
        {
            Noeuds = new Dictionary<int, Noeud<T>>();
            Liens = new List<Lien<T>>();
            adjacence = new Dictionary<int, Dictionary<int, int>>();
        }

        /// <summary>
        /// Ajoute un nœud dans le graphe.
        /// </summary>
        public void AjouterNoeud(Noeud<T> noeud)
        {
            if (!Noeuds.ContainsKey(noeud.Id))
            {
                Noeuds.Add(noeud.Id, noeud);
                adjacence[noeud.Id] = new Dictionary<int, int>();
            }
        }

        /// <summary>
        /// Ajoute un lien entre deux nœuds.
        /// </summary>
        /// <param name="id1">Identifiant du premier nœud</param>
        /// <param name="id2">Identifiant du deuxième nœud</param>
        /// <param name="poids">Poids de l’arête</param>
        /// <param name="bidirectionnel">Si vrai, ajoute le lien dans les deux sens</param>
        public void AjouterLien(int id1, int id2, int poids = 1, bool bidirectionnel = false)
        {
            if (Noeuds.ContainsKey(id1) && Noeuds.ContainsKey(id2))
            {
                Noeud<T> n1 = Noeuds[id1];
                Noeud<T> n2 = Noeuds[id2];
                Liens.Add(new Lien<T>(n1, n2, poids, bidirectionnel));
                n1.AjouterVoisin(n2);
                if (bidirectionnel)
                    n2.AjouterVoisin(n1);

                // Mise à jour de la structure d'adjacence
                adjacence[id1][id2] = poids;
                if (bidirectionnel)
                {
                    if (!adjacence[id2].ContainsKey(id1))
                        adjacence[id2][id1] = poids;
                }
            }
        }

        /// <summary>
        /// Affiche la liste d’adjacence.
        /// </summary>
        public string AfficherListeAdjacence()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var noeud in Noeuds.Values)
            {
                sb.AppendLine($"{noeud.Data} -> {string.Join(", ", noeud.Voisins.Select(v => v.Data.ToString()))}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Vérifie si un lien existe entre deux nœuds.
        /// </summary>
        public bool ExisteLien(int id1, int id2)
        {
            return adjacence.ContainsKey(id1) && adjacence[id1].ContainsKey(id2);
        }

        /// <summary>
        /// Parcours en largeur (BFS) à partir d’un nœud de départ.
        /// </summary>
        public string ParcoursLargeur(int idDepart)
        {
            StringBuilder sb = new StringBuilder();
            if (!Noeuds.ContainsKey(idDepart))
                return "";
            HashSet<int> visite = new HashSet<int>();
            Queue<Noeud<T>> queue = new Queue<Noeud<T>>();
            queue.Enqueue(Noeuds[idDepart]);
            visite.Add(idDepart);
            while (queue.Count > 0)
            {
                var courant = queue.Dequeue();
                sb.Append(courant.Data + " ");
                foreach (var voisin in courant.Voisins)
                {
                    if (!visite.Contains(voisin.Id))
                    {
                        visite.Add(voisin.Id);
                        queue.Enqueue(voisin);
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Parcours en profondeur (DFS) à partir d’un nœud de départ.
        /// </summary>
        public string ParcoursProfondeur(int idDepart)
        {
            StringBuilder sb = new StringBuilder();
            if (!Noeuds.ContainsKey(idDepart))
                return "";
            HashSet<int> visite = new HashSet<int>();
            Stack<Noeud<T>> stack = new Stack<Noeud<T>>();
            stack.Push(Noeuds[idDepart]);
            while (stack.Count > 0)
            {
                var courant = stack.Pop();
                if (!visite.Contains(courant.Id))
                {
                    visite.Add(courant.Id);
                    sb.Append(courant.Data + " ");
                    foreach (var voisin in courant.Voisins)
                    {
                        if (!visite.Contains(voisin.Id))
                            stack.Push(voisin);
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Vérifie si le graphe est connexe.
        /// </summary>
        public bool EstConnexe()
        {
            if (Noeuds.Count == 0)
                return false;
            HashSet<int> visite = new HashSet<int>();
            Queue<int> queue = new Queue<int>();
            int premier = Noeuds.Keys.First();
            queue.Enqueue(premier);
            visite.Add(premier);
            while (queue.Count > 0)
            {
                int courant = queue.Dequeue();
                foreach (var voisin in Noeuds[courant].Voisins)
                {
                    if (!visite.Contains(voisin.Id))
                    {
                        visite.Add(voisin.Id);
                        queue.Enqueue(voisin.Id);
                    }
                }
            }
            return visite.Count == Noeuds.Count;
        }

        /// <summary>
        /// Algorithme de Dijkstra pour calculer les plus courts chemins à partir d’un nœud source.
        /// </summary>
        public Dictionary<int, int> Dijkstra(int idDepart)
        {
            Dictionary<int, int> distances = new Dictionary<int, int>();
            HashSet<int> visited = new HashSet<int>();
            // Utilisation de la PriorityQueue (disponible à partir de .NET 6)
            PriorityQueue<int, int> pq = new PriorityQueue<int, int>();

            foreach (var noeud in Noeuds.Keys)
                distances[noeud] = int.MaxValue;
            distances[idDepart] = 0;
            pq.Enqueue(idDepart, 0);

            while (pq.Count > 0)
            {
                int courant = pq.Dequeue();
                if (visited.Contains(courant))
                    continue;
                visited.Add(courant);
                if (adjacence.ContainsKey(courant))
                {
                    foreach (var kv in adjacence[courant])
                    {
                        int voisinId = kv.Key;
                        int poids = kv.Value;
                        if (distances[courant] != int.MaxValue && distances[courant] + poids < distances[voisinId])
                        {
                            distances[voisinId] = distances[courant] + poids;
                            pq.Enqueue(voisinId, distances[voisinId]);
                        }
                    }
                }
            }
            return distances;
        }

        /// <summary>
        /// Algorithme de Bellman–Ford pour calculer les plus courts chemins à partir d’un nœud source.
        /// </summary>
        public Dictionary<int, int> BellmanFord(int idDepart)
        {
            Dictionary<int, int> distances = new Dictionary<int, int>();
            foreach (var noeud in Noeuds.Keys)
                distances[noeud] = int.MaxValue;
            distances[idDepart] = 0;

            for (int i = 0; i < Noeuds.Count - 1; i++)
            {
                foreach (var lien in Liens)
                {
                    int u = lien.Noeud1.Id;
                    int v = lien.Noeud2.Id;
                    int poids = lien.Poids;
                    if (distances[u] != int.MaxValue && distances[u] + poids < distances[v])
                        distances[v] = distances[u] + poids;
                }
            }

            // Vérification des cycles de poids négatif
            foreach (var lien in Liens)
            {
                int u = lien.Noeud1.Id;
                int v = lien.Noeud2.Id;
                int poids = lien.Poids;
                if (distances[u] != int.MaxValue && distances[u] + poids < distances[v])
                    throw new Exception("Cycle de poids négatif détecté !");
            }

            return distances;
        }

        /// <summary>
        /// Algorithme de Floyd–Warshall pour calculer la matrice des plus courts chemins.
        /// </summary>
        public Dictionary<int, Dictionary<int, int>> FloydWarshall()
        {
            Dictionary<int, Dictionary<int, int>> dist = new Dictionary<int, Dictionary<int, int>>();
            foreach (var i in Noeuds.Keys)
            {
                dist[i] = new Dictionary<int, int>();
                foreach (var j in Noeuds.Keys)
                {
                    if (i == j)
                        dist[i][j] = 0;
                    else if (adjacence.ContainsKey(i) && adjacence[i].ContainsKey(j))
                        dist[i][j] = adjacence[i][j];
                    else
                        dist[i][j] = int.MaxValue;
                }
            }

            foreach (var k in Noeuds.Keys)
            {
                foreach (var i in Noeuds.Keys)
                {
                    foreach (var j in Noeuds.Keys)
                    {
                        if (dist[i][k] != int.MaxValue && dist[k][j] != int.MaxValue &&
                            dist[i][k] + dist[k][j] < dist[i][j])
                        {
                            dist[i][j] = dist[i][k] + dist[k][j];
                        }
                    }
                }
            }
            return dist;
        }
    }
}
