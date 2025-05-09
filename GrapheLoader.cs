using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Projet_PSI.Utils;

namespace Projet_PSI
{
    public static class GrapheLoader
    {
        // Classe temporaire pour stocker les données brutes extraites de la BDD
        private class DonneeStationBrute
        {
            public int Id { get; set; }                // Identifiant original en base
            public string Nom { get; set; }           // Libellé de la station
            public double Latitude { get; set; }      // Coordonnée latitude
            public double Longitude { get; set; }     // Coordonnée longitude
            public string NomCommune { get; set; }    // Nom de la commune
            public int CodeCommune { get; set; }      // Code INSEE de la commune
            public int? Precedent { get; set; }       // ID BDD de la station précédente (null si terminus)
            public int? Suivant { get; set; }         // ID BDD de la station suivante (null si terminus)
            public int TempsChangement { get; set; }   // Temps de correspondance (en secondes)
            public bool EstOriente { get; set; }      // Orientation (non utilisé pour le graphe unifié)
            public string NomLigne { get; set; }      // Nom de la ligne (par exemple "Ligne 1")
        }

        public static Graphe<Station> ChargerDepuisBDD()
        {
            // 1. Chargement des données brutes depuis la base de données
            var listeBrute = new List<DonneeStationBrute>();
            using (var lecteur = Bdd.Lire(@"
                SELECT IDStation, LibelleStation, Latitude, Longitude,
                       CommuneNom, CommuneCode, Precedent, Suivant,
                       TempsChangement, EstOriente, LibelLigne
                  FROM stations_metro"))
            {
                while (lecteur.Read())
                {
                    listeBrute.Add(new DonneeStationBrute
                    {
                        Id = lecteur.GetInt32("IDStation"),
                        Nom = lecteur.GetString("LibelleStation"),
                        Latitude = lecteur.GetDouble("Latitude"),
                        Longitude = lecteur.GetDouble("Longitude"),
                        NomCommune = lecteur.GetString("CommuneNom"),
                        CodeCommune = lecteur.GetInt32("CommuneCode"),
                        Precedent = lecteur.IsDBNull("Precedent") ? (int?)null : lecteur.GetInt32("Precedent"),
                        Suivant = lecteur.IsDBNull("Suivant") ? (int?)null : lecteur.GetInt32("Suivant"),
                        TempsChangement = lecteur.GetInt32("TempsChangement"),
                        EstOriente = lecteur.GetBoolean("EstOriente"),
                        NomLigne = lecteur.GetString("LibelLigne")
                    });
                }
            }

            // 2. Unification des stations physiques par libellé
            // Regrouper toutes les entrées de même nom
            var groupes = listeBrute.GroupBy(s => s.Nom).ToList();
            int nStationsUnifiees = groupes.Count;
            var graphe = new Graphe<Station>(nStationsUnifiees);

            // Dictionnaire pour traduire chaque ID BDD en ID unifié
            var mapIdBddVersUnifie = new Dictionary<int, int>(listeBrute.Count);

            // Création des nœuds unifiés dans le graphe
            for (int idUnifie = 0; idUnifie < nStationsUnifiees; idUnifie++)
            {
                var premiereEntree = groupes[idUnifie].First();
                var stationUnifiee = new Station
                {
                    ID = idUnifie,
                    Libelle = premiereEntree.Nom,
                    Latitude = premiereEntree.Latitude,
                    Longitude = premiereEntree.Longitude,
                    CommuneNom = premiereEntree.NomCommune,
                    CommuneCode = premiereEntree.CodeCommune,
                    LibelLigne = premiereEntree.NomLigne,
                    Precedent = -1,
                    Suivant = -1,
                    TempsChangement = premiereEntree.TempsChangement,
                    EstOriente = false
                };
                graphe.AjouterNoeud(idUnifie, stationUnifiee);

                // Associer chaque enregistrement brut à cet ID unifié
                foreach (var raw in groupes[idUnifie])
                {
                    mapIdBddVersUnifie[raw.Id] = idUnifie;
                }
            }

            // 3. Ajout des arêtes (liens) suivant les références Précédent/Suivant
            foreach (var raw in listeBrute)
            {
                if (!mapIdBddVersUnifie.TryGetValue(raw.Id, out int u))
                    continue;

                // Poids en minutes
                int poidsMinutes = raw.TempsChangement;

                // Station précédente ➔ station courante
                if (raw.Precedent.HasValue
                    && mapIdBddVersUnifie.TryGetValue(raw.Precedent.Value, out int vPrev)
                    && u != vPrev)
                {
                    graphe.AjouterLien(vPrev, u, poidsMinutes);
                }

                // Station courante ➔ station suivante
                if (raw.Suivant.HasValue
                    && mapIdBddVersUnifie.TryGetValue(raw.Suivant.Value, out int vNext)
                    && u != vNext)
                {
                    graphe.AjouterLien(u, vNext, poidsMinutes);
                }
            }

            return graphe;
        }
    }
}