using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_PSI
{
    public class Noeud<T>
    {
        private int id;
        private T data;// Données de type T associées au nœud
        private List<Noeud<T>> voisins;// Liste des nœuds voisins

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Obtient ou définit les données de type T associées au nœud.
        /// </summary>
        public T Data
        {
            get { return data; }
            set { data = value; }
        }

        /// <summary>
        /// Obtient la liste des voisins du nœud.
        /// </summary>
        public List<Noeud<T>> Voisins
        {
            get { return voisins; }
        }

        /// <summary>
        /// Constructeur d’un nœud générique.
        /// </summary>
        /// <param name="id">Identifiant unique.</param>
        /// <param name="data">Donnée générique pour le nœud.</param>
        public Noeud(int id, T data)
        {
            this.id = id;
            this.data = data;
            this.voisins = new List<Noeud<T>>();
        }

        /// <summary>
        /// Ajoute un voisin à la liste de voisins du nœud.
        /// </summary>
        /// <param name="voisin">Nœud voisin à ajouter.</param>
        public void AjouterVoisin(Noeud<T> voisin)
        {
            if (!voisins.Contains(voisin))
            {
                voisins.Add(voisin);
            }
        }

        /// <summary>
        /// Retourne une chaîne de caractères représentant le nœud.
        /// </summary>
        /// <returns>Une représentation textuelle du nœud.</returns>
        public override string ToString()
        {
            return $"Noeud {Id} : {Data}";
        }
    }
}