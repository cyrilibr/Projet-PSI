using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projet_PSI
{
    public class Graphe<T>
    {
        public Dictionary<int, Noeud<T>> Noeuds { get; private set; }
        public List<Lien<T>> Liens { get; private set; }
        private Dictionary<int, Dictionary<int, int>> adjacence;

        public Graphe()
        {
            Noeuds = new Dictionary<int, Noeud<T>>();
            Liens = new List<Lien<T>>();
            adjacence = new Dictionary<int, Dictionary<int, int>>();
        }

        public void AjouterNoeud(Noeud<T> noeud)
        {
            if (!Noeuds.ContainsKey(noeud.Id))
            {
                Noeuds[noeud.Id] = noeud;
                adjacence[noeud.Id] = new Dictionary<int, int>();
            }
        }

        public void AjouterLien(int id1, int id2, int poids = 1, bool bidirectionnel = false)
        {
            if (Noeuds.ContainsKey(id1) && Noeuds.ContainsKey(id2))
            {
                Noeud<T> n1 = Noeuds[id1];
                Noeud<T> n2 = Noeuds[id2];
                Liens.Add(new Lien<T>(n1, n2, poids, bidirectionnel));
                n1.AjouterVoisin(n2);
                if (bidirectionnel) n2.AjouterVoisin(n1);
                adjacence[id1][id2] = poids;
                if (bidirectionnel && !adjacence[id2].ContainsKey(id1))
                    adjacence[id2][id1] = poids;
            }
        }

        public string AfficherListeAdjacence()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var noeud in Noeuds.Values)
            {
                sb.AppendLine($"{noeud.Data} -> {string.Join(", ", noeud.Voisins.Select(v => v.Data.ToString()))}");
            }
            return sb.ToString();
        }

        public bool ExisteLien(int id1, int id2)
        {
            return adjacence.ContainsKey(id1) && adjacence[id1].ContainsKey(id2);
        }

        public bool EstConnexe()
        {
            if (Noeuds.Count == 0) return false;
            HashSet<int> visite = new HashSet<int>();
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(Noeuds.Keys.First());
            while (queue.Count > 0)
            {
                int courant = queue.Dequeue();
                if (!visite.Contains(courant))
                {
                    visite.Add(courant);
                    foreach (var voisin in Noeuds[courant].Voisins)
                        queue.Enqueue(voisin.Id);
                }
            }
            return visite.Count == Noeuds.Count;
        }

        public Dictionary<int, int> Dijkstra(int idDepart)
        {
            Dictionary<int, int> distances = Noeuds.Keys.ToDictionary(k => k, k => int.MaxValue);
            distances[idDepart] = 0;
            var pq = new PriorityQueue<int, int>();
            pq.Enqueue(idDepart, 0);
            while (pq.Count > 0)
            {
                int courant = pq.Dequeue();
                foreach (var voisin in adjacence[courant])
                {
                    int alt = distances[courant] + voisin.Value;
                    if (alt < distances[voisin.Key])
                    {
                        distances[voisin.Key] = alt;
                        pq.Enqueue(voisin.Key, alt);
                    }
                }
            }
            return distances;
        }

        public Dictionary<int, int> CalculerPredecesseurs(int idDepart)
        {
            var distances = Noeuds.Keys.ToDictionary(k => k, k => int.MaxValue);
            var predecesseurs = new Dictionary<int, int>();
            distances[idDepart] = 0;
            var pq = new PriorityQueue<int, int>();
            pq.Enqueue(idDepart, 0);

            while (pq.Count > 0)
            {
                int courant = pq.Dequeue();
                foreach (var voisin in adjacence[courant])
                {
                    int alt = distances[courant] + voisin.Value;
                    if (alt < distances[voisin.Key])
                    {
                        distances[voisin.Key] = alt;
                        predecesseurs[voisin.Key] = courant;
                        pq.Enqueue(voisin.Key, alt);
                    }
                }
            }
            return predecesseurs;
        }

        public Dictionary<int, int> BellmanFord(int idDepart)
        {
            Dictionary<int, int> dist = Noeuds.Keys.ToDictionary(k => k, k => int.MaxValue);
            dist[idDepart] = 0;
            for (int i = 0; i < Noeuds.Count - 1; i++)
            {
                foreach (var lien in Liens)
                {
                    int u = lien.Noeud1.Id;
                    int v = lien.Noeud2.Id;
                    if (dist[u] != int.MaxValue && dist[u] + lien.Poids < dist[v])
                        dist[v] = dist[u] + lien.Poids;
                }
            }
            foreach (var lien in Liens)
            {
                int u = lien.Noeud1.Id;
                int v = lien.Noeud2.Id;
                if (dist[u] != int.MaxValue && dist[u] + lien.Poids < dist[v])
                    throw new Exception("Cycle négatif détecté");
            }
            return dist;
        }

        public Dictionary<int, Dictionary<int, int>> FloydWarshall()
        {
            var dist = new Dictionary<int, Dictionary<int, int>>();
            foreach (var i in Noeuds.Keys)
            {
                dist[i] = new Dictionary<int, int>();
                foreach (var j in Noeuds.Keys)
                {
                    if (i == j) dist[i][j] = 0;
                    else if (adjacence.ContainsKey(i) && adjacence[i].ContainsKey(j))
                        dist[i][j] = adjacence[i][j];
                    else dist[i][j] = int.MaxValue;
                }
            }
            foreach (var k in Noeuds.Keys)
                foreach (var i in Noeuds.Keys)
                    foreach (var j in Noeuds.Keys)
                        if (dist[i][k] != int.MaxValue && dist[k][j] != int.MaxValue && dist[i][j] > dist[i][k] + dist[k][j])
                            dist[i][j] = dist[i][k] + dist[k][j];
            return dist;
        }
    }
}