using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_PSI
{
    public class Noeud
    {
        private int id; // Identifiant unique du noeud
        private string nom; // Nom du noeud
        private List<Noeud> voisins; // Liste des noeuds voisins

        /// <summary>
        /// Obtient l'identifiant du noeud
        /// </summary>
        public int Id
        {
            get { return id; }
        }

        /// <summary>
        /// Obtient le nom du noeud
        /// </summary>
        public string Nom
        {
            get { return nom; }
        }

        /// <summary>
        /// Obtient la liste des voisins du noeud
        /// </summary>
        public List<Noeud> Voisins
        {
            get { return voisins; }
        }

        /// <summary>
        /// Constructeur de la classe Noeud
        /// </summary>
        /// <param name="id">Identifiant unique du noeud</param>
        /// <param name="nom">Nom du noeud</param>
        public Noeud(int id, string nom)
        {
            this.id = id;
            this.nom = nom;
            this.voisins = new List<Noeud>();
        }

        /// <summary>
        /// Ajoute un voisin au noeud
        /// </summary>
        /// <param name="voisin">Noeud voisin a ajouter</param>
        public void AjouterVoisin(Noeud voisin)
        {
            if (!voisins.Contains(voisin))
            {
                voisins.Add(voisin);
            }
        }

        /// <summary>
        /// Retourne une representation du noeud
        /// </summary>
        /// <returns>Une chaine de caracteres representant le noeud</returns>
        public override string ToString()
        {
            return $"Noeud {id}: {nom}";
        }
    }
}
