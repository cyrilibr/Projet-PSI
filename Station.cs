namespace Projet_PSI
{
    /// <summary>
    /// Classe représentant une station de métro.
    /// </summary>
    public class Station
    {
        public int ID { get; set; }
        public string Libelle { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string CommuneNom { get; set; }
        public int CommuneCode { get; set; }
        public int Precedent { get; set; }
        public int Suivant { get; set; }
        public int TempsChangement { get; set; }
        public bool EstOriente { get; set; }

        public override string ToString()
        {
            return Libelle;
        }
    }
}