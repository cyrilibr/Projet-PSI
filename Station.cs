// Projet_PSI/Station.cs
using System.Collections.Generic; // Ajout
using System.Linq; // Ajout

namespace Projet_PSI
{
    public class Station
    {
        public int ID { get; set; }
        public string Libelle { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string CommuneNom { get; set; }
        public int CommuneCode { get; set; }
        // Precedent et Suivant ne sont plus pertinents directement sur Station si GrapheLoader gère
        // On les garde pour la compatibilité initiale mais le Graphe orienté est la clé
        public int Precedent { get; set; } 
        public int Suivant { get; set; }

        // TempsChangement original est lié à une entrée station_metro spécifique.
        // Une station unifiée peut avoir plusieurs TempsChangement selon la ligne
        // On va stocker un TempsMoyenChangement pour la station unifiée.
        public int TempsMoyenChangement { get; set; } 
        
        public bool EstOriente { get; set; } // Pertinent pour la ligne source, moins pour la station unifiée
        
        public string LibelLigne { get; set; } // Ligne "principale" ou première trouvée
        public HashSet<string> LignesDesservies { get; set; } // Nouvel attribut

        public Station()
        {
            LignesDesservies = new HashSet<string>();
        }

        public override string ToString()
        {
            return Libelle;
        }
    }
}
