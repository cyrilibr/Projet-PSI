// Projet_PSI/Station.cs
using System.Collections.Generic; // Ajout
using System.Linq; // Ajout

namespace Projet_PSI
{
    public class Station
    {
        private int id;
        private string libelle;
        private double longitude;
        private double latitude;
        private string communeNom;
        private int communeCode;
        private int precedent;
        private int suivant;
        private int tempsChangement;
        private bool estOriente;
        private string libelLigne;

        /// <summary>
        /// Obtient ou définit l'identifiant unique de la station.
        /// </summary>
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Obtient ou définit le libellé (nom) de la station.
        /// </summary>
        public string Libelle
        {
            get { return libelle; }
            set { libelle = value; }
        }

        /// <summary>
        /// Obtient ou définit la longitude géographique de la station.
        /// </summary>
        public double Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }

        /// <summary>
        /// Obtient ou définit la latitude géographique de la station.
        /// </summary>
        public double Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }

        /// <summary>
        /// Obtient ou définit le nom de la commune où se situe la station.
        /// </summary>
        public string CommuneNom
        {
            get { return communeNom; }
            set { communeNom = value; }
        }

        /// <summary>
        /// Obtient ou définit le code de la commune où se situe la station.
        /// </summary>
        public int CommuneCode
        {
            get { return communeCode; }
            set { communeCode = value; }
        }

        /// <summary>
        /// Obtient ou définit l'identifiant de la station précédente dans la même ligne.
        /// </summary>
        public int Precedent
        {
            get { return precedent; }
            set { precedent = value; }
        }

        /// <summary>
        /// Obtient ou définit l'identifiant de la station suivante dans la même ligne.
        /// </summary>
        public int Suivant
        {
            get { return suivant; }
            set { suivant = value; }
        }

        /// <summary>
        /// Obtient ou définit le temps de changement (en secondes) pour passer à une autre ligne.
        /// </summary>
        public int TempsChangement
        {
            get { return tempsChangement; }
            set { tempsChangement = value; }
        }

        /// <summary>
        /// Obtient ou définit un indicateur précisant si la station est orientée (sens unique).
        /// </summary>
        public bool EstOriente
        {
            get { return estOriente; }
            set { estOriente = value; }
        }

        /// <summary>
        /// Obtient ou définit le libellé de la ligne de métro à laquelle appartient la station.
        /// </summary>
        public string LibelLigne
        {
            get { return libelLigne; }
            set { libelLigne = value; }
        }

        /// <summary>
        /// Retourne une chaîne de caractères représentant la station.
        /// </summary>
        public override string ToString()
        {
            return Libelle;
        }
    }
}
