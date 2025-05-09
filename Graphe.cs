// Projet_PSI/Graphe.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace Projet_PSI
{
    public class Graphe<T>
    {
        private Dictionary<int, Noeud<T>> noeuds;
        private List<Lien<T>> liens; // Conserve une trace de tous les liens pour certains algos
        private int[,] matriceAdjacence; // Représente les poids des arêtes orientées noeud_i -> noeud_j
        private int taille; // Taille max (utilisée pour la matrice)

        public Dictionary<int, Noeud<T>> Noeuds => noeuds;
        public List<Lien<T>> Liens => liens;
        public int Taille => taille;


        public Graphe(int maxTaille) // La taille est pour la matrice d'adjacence
        {
            this.taille = maxTaille;
            noeuds = new Dictionary<int, Noeud<T>>();
            liens = new List<Lien<T>>();
            // Initialiser la matrice avec 0 (pas de lien) ou une valeur signalant l'absence de lien si 0 est un poids valide.
            // Pour Dijkstra, int.MaxValue est mieux si le poids ne peut pas être 0.
            // Mais ici, 0 dans matrice = pas de lien. Poids > 0 = lien avec ce poids.
            matriceAdjacence = new int[this.taille, this.taille];
        }

        public void AjouterNoeud(int id, T data)
        {
            if (id >= taille)
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"L'ID du noeud ({id}) dépasse la taille maximale du graphe ({taille - 1}).");
            }
            if (!noeuds.ContainsKey(id))
            {
                noeuds[id] = new Noeud<T>(id, data);
            }
        }

        // Par défaut, un lien est orienté de id1 vers id2.
        // Si bidirectionnel est true, un lien est aussi créé de id2 vers id1 implicitement (pour les algos)
        // ou explicitement si l'appelant le gère (deux appels à AjouterLien).
        // Je vais opter pour: cette méthode ajoute un lien orienté (id1 -> id2).
        // L'appelant (GrapheLoader) doit faire un second appel si la liaison est bidirectionnelle.
        public void AjouterLienOriente(int idOrigine, int idDestination, int poids = 1)
        {
            if (poids <= 0) throw new ArgumentException("Le poids d'un lien doit être positif.", nameof(poids));
            if (idOrigine >= taille || idDestination >= taille)
            {
                throw new ArgumentOutOfRangeException("L'ID d'un des noeuds dépasse la taille maximale du graphe.");
            }

            if (noeuds.ContainsKey(idOrigine) && noeuds.ContainsKey(idDestination))
            {
                var noeudOrigine = noeuds[idOrigine];
                var noeudDestination = noeuds[idDestination];

                // Vérifier si un lien existe déjà dans cette direction pour éviter les doublons dans la liste des liens
                // (la matrice d'adjacence l'écraserait simplement)
                if (!liens.Any(l => l.Noeud1 == noeudOrigine && l.Noeud2 == noeudDestination))
                {
                    liens.Add(new Lien<T>(noeudOrigine, noeudDestination, poids, bidirectionnel: false)); // Marqué comme non-bidirectionnel par défaut
                }

                noeudOrigine.AjouterVoisin(noeudDestination); // NoeudDestination est un successeur de NoeudOrigine
                matriceAdjacence[idOrigine, idDestination] = poids;
            }
            else
            {
                Console.WriteLine($"Attention: Nœud(s) non trouvé(s) pour le lien {idOrigine}->{idDestination}. Lien non ajouté.");
            }
        }

        public bool ExisteLien(int id1, int id2)
        {
            if (id1 >= taille || id2 >= taille) return false;
            return matriceAdjacence[id1, id2] > 0; // Poids > 0 signifie lien
        }

        public string AfficherListeAdjacence()
        {
            // Affiche les successeurs
            return string.Join("\n", noeuds.Values.OrderBy(n => n.Id).Select(kvp => $"{kvp.Data} -> {string.Join(", ", kvp.Voisins.Select(v => v.Data))}"));
        }

        public string AfficherMatriceAdjacence()
        {
            var result = "    ";
            for (int j = 0; j < taille && noeuds.ContainsKey(j); j++) result += $"{j,3} ";
            result += "\n----";
            for (int j = 0; j < taille && noeuds.ContainsKey(j); j++) result += "----";
            result += "\n";

            for (int i = 0; i < taille; i++)
            {
                if (!noeuds.ContainsKey(i)) continue; // Ne pas afficher lignes pour noeuds non existants
                result += $"{i,3}| ";
                for (int j = 0; j < taille; j++)
                {
                    if (!noeuds.ContainsKey(j)) continue;
                    result += $"{matriceAdjacence[i, j],3} ";
                }
                result += "\n";
            }
            return result;
        }

        public bool EstConnexe() // Pour un graphe orienté, on parle plutôt de "fortement connexe"
        {
            // Cette implémentation vérifie la connexité faible (si le graphe sous-jacent non orienté est connexe)
            if (noeuds.Count == 0) return false;
            var visite = new HashSet<int>();
            var file = new Queue<int>();
            var depart = noeuds.Keys.First();
            file.Enqueue(depart);
            visite.Add(depart);

            while (file.Count > 0)
            {
                var courantId = file.Dequeue();
                var courantNoeud = noeuds[courantId];

                // Parcours des successeurs
                foreach (var voisin in courantNoeud.Voisins)
                {
                    if (visite.Add(voisin.Id))
                        file.Enqueue(voisin.Id);
                }
                // Parcours des prédécesseurs (pour connexité faible)
                foreach (var noeudPotentielPredecesseur in noeuds.Values)
                {
                    if (noeudPotentielPredecesseur.Voisins.Contains(courantNoeud)) // Si courantNoeud est un successeur de noeudPotentielPredecesseur
                    {
                        if (visite.Add(noeudPotentielPredecesseur.Id))
                            file.Enqueue(noeudPotentielPredecesseur.Id);
                    }
                }
            }
            return visite.Count == noeuds.Count;
        }

        public bool ContientCycle()
        {
            var enCoursDeVisite = new HashSet<int>(); // Noeuds dans la pile de récursion DFS actuelle
            var visite = new HashSet<int>();          // Noeuds déjà complètement visités

            foreach (var id in noeuds.Keys)
            {
                if (!visite.Contains(id))
                {
                    if (DFS_Cycle_Oriente(id, visite, enCoursDeVisite))
                        return true;
                }
            }
            return false;
        }

        private bool DFS_Cycle_Oriente(int idNoeud, HashSet<int> visite, HashSet<int> enCoursDeVisite)
        {
            visite.Add(idNoeud);
            enCoursDeVisite.Add(idNoeud);

            if (noeuds.TryGetValue(idNoeud, out Noeud<T> noeudCourant))
            {
                foreach (var voisin in noeudCourant.Voisins) // Voisins sont des successeurs
                {
                    if (!visite.Contains(voisin.Id))
                    {
                        if (DFS_Cycle_Oriente(voisin.Id, visite, enCoursDeVisite))
                            return true;
                    }
                    else if (enCoursDeVisite.Contains(voisin.Id)) // Arc arrière vers un noeud en cours de visite
                    {
                        return true;
                    }
                }
            }
            enCoursDeVisite.Remove(idNoeud); // Retirer de la pile de récursion
            return false;
        }


        public List<int> CheminDijkstra(int startId, int endId)
        {
            if (!noeuds.ContainsKey(startId) || !noeuds.ContainsKey(endId))
                return new List<int>(); // Départ ou arrivée non existant

            var dist = noeuds.ToDictionary(n => n.Key, _ => int.MaxValue);
            var prev = noeuds.ToDictionary(n => n.Key, _ => (int?)null);
            // Optimisation : utiliser une PriorityQueue (ou simuler avec SortedSet/SortedList)
            var pq = new SortedSet<(int d, int id)>(Comparer<(int d, int id)>.Create((a, b) =>
                a.d == b.d ? a.id.CompareTo(b.id) : a.d.CompareTo(b.d)
            ));


            dist[startId] = 0;
            pq.Add((0, startId));

            while (pq.Count > 0)
            {
                var (d_u, u) = pq.Min;
                pq.Remove(pq.Min);

                if (u == endId) break; // Chemin trouvé
                if (d_u == int.MaxValue) break; // Autres noeuds inatteignables

                if (!noeuds.ContainsKey(u)) continue; // Précaution

                foreach (var voisin in noeuds[u].Voisins) // Voisins sont les successeurs
                {
                    int poidsArete = matriceAdjacence[u, voisin.Id];
                    if (poidsArete > 0) // S'il y a un lien
                    {
                        int alt = dist[u] + poidsArete;
                        if (alt < dist[voisin.Id])
                        {
                            pq.Remove((dist[voisin.Id], voisin.Id)); // Nécessaire pour màj dans SortedSet
                            dist[voisin.Id] = alt;
                            prev[voisin.Id] = u;
                            pq.Add((alt, voisin.Id));
                        }
                    }
                }
            }

            var chemin = new List<int>();
            if (dist[endId] == int.MaxValue)
                return chemin; // Pas de chemin

            for (int? at = endId; at != null; at = prev[at.Value])
                chemin.Add(at.Value);
            chemin.Reverse();
            return chemin;
        }

        // Bellman-Ford et Floyd-Warshall restent largement similaires
        // Bellman-Ford itère sur tous les `liens`.
        // Floyd-Warshall utilise `matriceAdjacence`.
        // Ils fonctionneront correctement avec la `matriceAdjacence` orientée.

        public List<int> CheminBellmanFord(int startId, int endId)
        {
            if (!noeuds.ContainsKey(startId) || !noeuds.ContainsKey(endId)) return new List<int>();

            var dist = noeuds.ToDictionary(n => n.Key, _ => (long)int.MaxValue); // Use long for intermediate sums
            var prev = noeuds.ToDictionary(n => n.Key, _ => (int?)null);
            dist[startId] = 0;

            for (int i = 0; i < noeuds.Count - 1; i++)
            {
                foreach (var lien in liens) // Les liens sont déjà orientés Noeud1 -> Noeud2
                {
                    int u = lien.Noeud1.Id;
                    int v = lien.Noeud2.Id;
                    int w = lien.Poids;

                    if (dist.ContainsKey(u) && dist[u] != int.MaxValue && dist[u] + w < dist[v])
                    {
                        dist[v] = dist[u] + w;
                        prev[v] = u;
                    }
                    // Si le graphe était non-orienté ET que les liens stockés ne sont qu'un sens,
                    // il faudrait aussi vérifier le lien inverse. Mais ici, `liens` doit contenir tous les liens orientés.
                    if (lien.Bidirectionnel) // Si le Lien a été marqué comme bidirectionnel à la création
                    {
                        if (dist.ContainsKey(v) && dist[v] != int.MaxValue && dist[v] + w < dist[u])
                        {
                            dist[u] = dist[v] + w;
                            prev[u] = v;
                        }
                    }
                }
            }

            // Vérification de cycle négatif (optionnel ici si poids tjrs > 0)
            foreach (var lien in liens)
            {
                int u = lien.Noeud1.Id;
                int v = lien.Noeud2.Id;
                int w = lien.Poids;
                if (dist.ContainsKey(u) && dist[u] != int.MaxValue && dist[u] + w < dist[v])
                {
                    Console.WriteLine("Attention : Cycle de poids négatif détecté par Bellman-Ford.");
                    return new List<int>(); // Ou gérer autrement
                }
            }


            var chemin = new List<int>();
            if (!dist.ContainsKey(endId) || dist[endId] == int.MaxValue) return chemin; // Pas de chemin

            for (int? at = endId; at != null; at = prev[at.Value])
                chemin.Add(at.Value);
            chemin.Reverse();
            return chemin;
        }


        public List<int> CheminFloydWarshall(int startId, int endId)
        {
            if (startId >= taille || endId >= taille || !noeuds.ContainsKey(startId) || !noeuds.ContainsKey(endId)) return new List<int>();

            var dist = new long[taille, taille];
            var next = new int[taille, taille]; // next[i,j] est le prochain noeud sur le chemin de i à j

            for (int i = 0; i < taille; i++)
            {
                for (int j = 0; j < taille; j++)
                {
                    if (i == j)
                    {
                        dist[i, j] = 0;
                        next[i, j] = j;
                    }
                    else if (matriceAdjacence[i, j] > 0) // Lien direct existe
                    {
                        dist[i, j] = matriceAdjacence[i, j];
                        next[i, j] = j;
                    }
                    else
                    {
                        dist[i, j] = int.MaxValue; // Simule l'infini
                        next[i, j] = -1; // Pas de chemin direct
                    }
                }
            }

            // Il faut itérer sur les ID réels des noeuds présents plutôt que 0..taille-1
            // Ou s'assurer que les IDs de noeuds sont compacts 0..N-1
            // Pour simplifier, si on utilise Graphe(maxIdStation + 1), c'est bon.

            List<int> idsNoeudsPresents = noeuds.Keys.ToList();

            foreach (int k_nodeId in idsNoeudsPresents) // k est un noeud intermédiaire
            {
                foreach (int i_nodeId in idsNoeudsPresents) // i est le départ
                {
                    foreach (int j_nodeId in idsNoeudsPresents) // j est l'arrivée
                    {
                        if (dist[i_nodeId, k_nodeId] != int.MaxValue && dist[k_nodeId, j_nodeId] != int.MaxValue &&
                            dist[i_nodeId, k_nodeId] + dist[k_nodeId, j_nodeId] < dist[i_nodeId, j_nodeId])
                        {
                            dist[i_nodeId, j_nodeId] = dist[i_nodeId, k_nodeId] + dist[k_nodeId, j_nodeId];
                            next[i_nodeId, j_nodeId] = next[i_nodeId, k_nodeId];
                        }
                    }
                }
            }

            if (dist[startId, endId] == int.MaxValue || next[startId, endId] == -1)
                return new List<int>(); // Pas de chemin

            var chemin = new List<int> { startId };
            int u = startId;
            while (u != endId)
            {
                u = next[u, endId];
                if (u == -1) return new List<int>(); // Chemin cassé (ne devrait pas arriver si dist[startId,endId] n'est pas infini)
                chemin.Add(u);
            }
            return chemin;
        }
    }
}