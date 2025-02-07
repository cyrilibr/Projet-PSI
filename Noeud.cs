using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_PSI
{
    public class Noeud
    {
        private int id;
        private string nom;
        private List<Noeud> voisins;

        public int Id 
        { 
            get { return id; } 
        }
        public string Nom 
        { 
            get { return nom; } 
        }
        public List<Noeud> Voisins 
        { 
            get { return voisins; }
            set { voisins = value > 0 ? value : 0; }
        }

        public Noeud(int id, string nom)
        {
            this.id = id;
            this.nom = nom;
            this.voisins = new List<Noeud>();
        }

        public void AjouterVoisin(Noeud voisin)
        {
            if (!voisins.Contains(voisin))
            {
                voisins.Add(voisin);
            }
        }

        public override string ToString()
        {
            return $"Noeud {id}: {nom}";
        }
    }
}