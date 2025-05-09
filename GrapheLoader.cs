// Projet_PSI/GrapheLoader.cs
using Projet_PSI.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Projet_PSI
{
    public static class GrapheLoader
    {
        private const int TEMPS_PAR_SEGMENT_MIN = 2; // Temps par défaut entre deux stations sur une même ligne

        private class DonneeStationBrute
        {
            public int IdBdd { get; set; } // Renommé pour clarté
            public string Nom { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string NomCommune { get; set; }
            public int CodeCommune { get; set; }
            public int? PrecedentIdBdd { get; set; } // Renommé
            public int? SuivantIdBdd { get; set; }   // Renommé
            public int TempsChangement { get; set; }
            public bool EstOrienteLigne { get; set; } // Si la ligne/segment est orientée
            public string NomLigne { get; set; }
        }

        public static Graphe<Station> ChargerDepuisBDD()
        {
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
                        IdBdd = lecteur.GetInt32("IDStation"),
                        Nom = lecteur.GetString("LibelleStation"),
                        Latitude = lecteur.GetDouble("Latitude"),
                        Longitude = lecteur.GetDouble("Longitude"),
                        NomCommune = lecteur.GetString("CommuneNom"),
                        CodeCommune = lecteur.GetInt32("CommuneCode"),
                        PrecedentIdBdd = lecteur.IsDBNull("Precedent") ? (int?)null : lecteur.GetInt32("Precedent"),
                        SuivantIdBdd = lecteur.IsDBNull("Suivant") ? (int?)null : lecteur.GetInt32("Suivant"),
                        TempsChangement = lecteur.GetInt32("TempsChangement"), // Temps de changement à cette station physique sur cette ligne
                        EstOrienteLigne = lecteur.GetBoolean("EstOriente"),
                        NomLigne = lecteur.GetString("LibelLigne")
                    });
                }
                lecteur.Close();
            }

            if (!listeBrute.Any()) return new Graphe<Station>(0);

            // Unification des stations physiques par libellé (ID unifié de 0 à N-1)
            var groupesParNom = listeBrute.GroupBy(s => s.Nom).ToList();
            int maxIdUnifie = groupesParNom.Count - 1;
            var graphe = new Graphe<Station>(maxIdUnifie + 1); // Taille max pour la matrice

            var mapNomStationVersIdUnifie = new Dictionary<string, int>();
            var mapIdBddVersIdUnifie = new Dictionary<int, int>();

            for (int idUnifie = 0; idUnifie <= maxIdUnifie; idUnifie++)
            {
                var groupeCourant = groupesParNom[idUnifie];
                var premiereEntreeDuGroupe = groupeCourant.First();

                mapNomStationVersIdUnifie[premiereEntreeDuGroupe.Nom] = idUnifie;

                var stationUnifiee = new Station
                {
                    ID = idUnifie,
                    Libelle = premiereEntreeDuGroupe.Nom,
                    Latitude = premiereEntreeDuGroupe.Latitude, // Prend les coords de la 1ère
                    Longitude = premiereEntreeDuGroupe.Longitude,
                    CommuneNom = premiereEntreeDuGroupe.NomCommune,
                    CommuneCode = premiereEntreeDuGroupe.CodeCommune,
                    // LibelLigne = premiereEntreeDuGroupe.NomLigne, // Moins pertinent après unification
                    TempsMoyenChangement = (int)groupeCourant.Average(s => s.TempsChangement) // Moyenne des temps de changements
                };

                foreach (var rawInGroup in groupeCourant)
                {
                    mapIdBddVersIdUnifie[rawInGroup.IdBdd] = idUnifie;
                    stationUnifiee.LignesDesservies.Add(rawInGroup.NomLigne); // Collecte toutes les lignes
                }
                graphe.AjouterNoeud(idUnifie, stationUnifiee);
            }

            // Ajout des arêtes orientées (liens)
            foreach (var rawStation in listeBrute)
            {
                if (!mapIdBddVersIdUnifie.TryGetValue(rawStation.IdBdd, out int idUnifieCourant)) continue;

                // Lien vers la station SUIVANTE
                if (rawStation.SuivantIdBdd.HasValue && mapIdBddVersIdUnifie.TryGetValue(rawStation.SuivantIdBdd.Value, out int idUnifieSuivant))
                {
                    if (idUnifieCourant != idUnifieSuivant) // Eviter boucle sur soi-même si données étranges
                    {
                        // Poids = temps de segment + temps de changement EN ARRIVANT à la station suivante si c'est une correspondance
                        // Pour simplifier, on prend le temps de segment ici. Dijkstra devra gérer le chgmt.
                        // Ou bien: poids = TEMPS_PAR_SEGMENT_MIN + graphe.Noeuds[idUnifieSuivant].Data.TempsMoyenChangement (approximation)
                        int poids = TEMPS_PAR_SEGMENT_MIN;
                        graphe.AjouterLienOriente(idUnifieCourant, idUnifieSuivant, poids);
                    }
                }

                // Lien depuis la station PRECEDENTE (si la ligne N'EST PAS orientée dans un seul sens)
                if (rawStation.PrecedentIdBdd.HasValue && !rawStation.EstOrienteLigne) // EstOrienteLigne = false signifie bidirectionnel
                {
                    if (mapIdBddVersIdUnifie.TryGetValue(rawStation.PrecedentIdBdd.Value, out int idUnifiePrecedent))
                    {
                        if (idUnifieCourant != idUnifiePrecedent)
                        {
                            int poids = TEMPS_PAR_SEGMENT_MIN;
                            graphe.AjouterLienOriente(idUnifieCourant, idUnifiePrecedent, poids);
                        }
                    }
                }
            }
            return graphe;
        }
    }
}