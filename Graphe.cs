using System;
using System.Collections.Generic;
using System.Linq;

namespace Projet_PSI
{
    public class Graphe<T>
    {

        private Dictionary<int, Noeud<T>> noeuds;
        private List<Lien<T>> liens;
        private int[,] matriceAdjacence;
        private int taille;

        public Dictionary<int, Noeud<T>> Noeuds => noeuds;
        public List<Lien<T>> Liens => liens;

        public Graphe(int taille)
        {
            this.taille = taille;
            noeuds = new Dictionary<int, Noeud<T>>();
            liens = new List<Lien<T>>();
            matriceAdjacence = new int[taille, taille];
        }

        public void AjouterNoeud(int id, T data)
        {
            if (!noeuds.ContainsKey(id))
                noeuds[id] = new Noeud<T>(id, data);
        }

        public void AjouterLien(int id1, int id2, int poids = 1)
        {
            if (noeuds.ContainsKey(id1) && noeuds.ContainsKey(id2))
            {
                var n1 = noeuds[id1];
                var n2 = noeuds[id2];
                liens.Add(new Lien<T>(n1, n2, poids));
                n1.AjouterVoisin(n2);
                n2.AjouterVoisin(n1);
                matriceAdjacence[id1, id2] = poids;
                matriceAdjacence[id2, id1] = poids;
            }
        }

        public bool ExisteLien(int id1, int id2) => matriceAdjacence[id1, id2] != 0;

        public string AfficherListeAdjacence()
        {
            return string.Join("\n", noeuds.Select(kvp => $"{kvp.Value.Data} -> {string.Join(", ", kvp.Value.Voisins.Select(v => v.Data))}"));
        }

        public string AfficherMatriceAdjacence()
        {
            var result = "";
            for (int i = 0; i < taille; i++)
            {
                for (int j = 0; j < taille; j++)
                    result += matriceAdjacence[i, j] + " ";
                result += "\n";
            }
            return result;
        }

        public bool EstConnexe()
        {
            if (noeuds.Count == 0) return false;
            var visite = new HashSet<int>();
            var file = new Queue<int>();
            var depart = noeuds.Keys.First();
            file.Enqueue(depart);
            visite.Add(depart);

            while (file.Count > 0)
            {
                var courant = file.Dequeue();
                foreach (var voisin in noeuds[courant].Voisins)
                {
                    if (visite.Add(voisin.Id))
                        file.Enqueue(voisin.Id);
                }
            }
            return visite.Count == noeuds.Count;
        }

        public bool ContientCycle()
        {
            var visite = new HashSet<int>();
            foreach (var id in noeuds.Keys)
            {
                if (!visite.Contains(id) && DFS_Cycle(id, visite, -1))
                    return true;
            }
            return false;
        }

        private bool DFS_Cycle(int id, HashSet<int> visite, int parent)
        {
            visite.Add(id);
            foreach (var voisin in noeuds[id].Voisins)
            {
                if (!visite.Contains(voisin.Id))
                {
                    if (DFS_Cycle(voisin.Id, visite, id)) return true;
                }
                else if (voisin.Id != parent)
                    return true;
            }
            return false;
        }

        public List<int> CheminDijkstra(int startId, int endId)
        {
            var dist = noeuds.ToDictionary(n => n.Key, _ => int.MaxValue);
            var prev = noeuds.ToDictionary(n => n.Key, _ => (int?)null);
            var nonVisites = new HashSet<int>(noeuds.Keys);
            dist[startId] = 0;

            while (nonVisites.Count > 0)
            {
                int u = nonVisites.OrderBy(id => dist[id]).FirstOrDefault();
                nonVisites.Remove(u);
                if (u == endId || dist[u] == int.MaxValue) break;

                foreach (var voisin in noeuds[u].Voisins)
                {
                    int alt = dist[u] + matriceAdjacence[u, voisin.Id];
                    if (alt < dist[voisin.Id])
                    {
                        dist[voisin.Id] = alt;
                        prev[voisin.Id] = u;
                    }
                }
            }

            var chemin = new List<int>();
            for (int? at = endId; at != null; at = prev[at.Value])
                chemin.Add(at.Value);
            chemin.Reverse();
            return dist[endId] == int.MaxValue ? new List<int>() : chemin;
        }

        public List<int> CheminBellmanFord(int startId, int endId)
        {
            var dist = noeuds.ToDictionary(n => n.Key, _ => int.MaxValue);
            var prev = noeuds.ToDictionary(n => n.Key, _ => (int?)null);
            dist[startId] = 0;

            for (int i = 0; i < noeuds.Count - 1; i++)
            {
                foreach (var lien in liens)
                {
                    int u = lien.Noeud1.Id, v = lien.Noeud2.Id, w = lien.Poids;
                    if (dist[u] != int.MaxValue && dist[u] + w < dist[v])
                    {
                        dist[v] = dist[u] + w;
                        prev[v] = u;
                    }
                    if (dist[v] != int.MaxValue && dist[v] + w < dist[u])
                    {
                        dist[u] = dist[v] + w;
                        prev[u] = v;
                    }
                }
            }

            var chemin = new List<int>();
            for (int? at = endId; at != null; at = prev[at.Value])
                chemin.Add(at.Value);
            chemin.Reverse();
            return dist[endId] == int.MaxValue ? new List<int>() : chemin;
        }

        public List<int> CheminFloydWarshall(int startId, int endId)
        {
            int n = taille;
            var dist = new int[n, n];
            var next = new int[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    dist[i, j] = i == j ? 0 : matriceAdjacence[i, j] == 0 ? int.MaxValue / 2 : matriceAdjacence[i, j];
                    next[i, j] = matriceAdjacence[i, j] != 0 ? j : -1;
                }

            for (int k = 0; k < n; k++)
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        if (dist[i, k] + dist[k, j] < dist[i, j])
                        {
                            dist[i, j] = dist[i, k] + dist[k, j];
                            next[i, j] = next[i, k];
                        }

            if (next[startId, endId] == -1) return new List<int>();
            var chemin = new List<int> { startId };
            while (startId != endId)
            {
                startId = next[startId, endId];
                if (startId == -1) break;
                chemin.Add(startId);
            }
            return chemin;
        }
    }
}
