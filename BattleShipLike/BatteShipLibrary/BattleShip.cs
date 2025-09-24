using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BattleShipLibrary
{
    public class BattleShip
    {
        private int Colonnes;
        private int Lignes;

        public Grille MaGrille;
        public Grille EnemyGrille;

        public List<Bateau> MesBateaux = new();

        public ConsoleColor CouleurJoueur { get; set; } = ConsoleColor.Green;
        public ConsoleColor CouleurServeur { get; set; } = ConsoleColor.Yellow;

        public Dictionary<string, (int, int)> Positions => MaGrille.Positions;
        public HashSet<string> CasesTouchees { get; private set; } = new HashSet<string>();

        public void StartGame(int c, int l)
        {
            Colonnes = c;
            Lignes = l;
            MaGrille = new Grille(c, l);
            EnemyGrille = new Grille(c, l);

            MesBateaux = new List<Bateau>
            {
                new Bateau(new List<(int,int)> { (0,0), (1,0), (0,1) }), // L
                new Bateau(new List<(int,int)> { (0,0), (-1,1), (0,1), (1,1) }), // T
                new Bateau(new List<(int,int)> { (0,0), (1,0), (0,1), (1,1) }) // carré
            };

            AfficherMaGrille();

            foreach (var bateau in MesBateaux)
            {
                bool placé = false;
                do
                {
                    string pos = ChoisirCase($"Case d'ancrage bateau taille {bateau.Forme.Count}:");
                    var (col, row) = Positions[pos];

                    if (!bateau.Placer(col, row, Colonnes, Lignes))
                    {
                        Console.WriteLine("Bateau ne rentre pas, choisissez autre position.");
                        continue;
                    }

                    if (MesBateaux.Any(b => b != bateau && b.Positions.Any(p => bateau.Positions.Contains(p))))
                    {
                        Console.WriteLine("Chevauchement détecté, choisissez autre position.");
                        continue;
                    }

                    foreach (var (x, y) in bateau.Positions)
                        MaGrille.Cells[x, y] = "B";

                    placé = true;
                    AfficherMaGrille();
                } while (!placé);
            }
        }

        public string ChoisirCase(string prompt)
        {
            string c;
            do
            {
                Console.WriteLine(prompt);
                c = Console.ReadLine()?.ToUpper();

                if (string.IsNullOrWhiteSpace(c) || !Positions.ContainsKey(c))
                {
                    ConsoleUI.WriteWarning("Coordonnée invalide !");
                    c = null;
                }
                else if (CasesTouchees.Contains(c))
                {
                    ConsoleUI.WriteWarning("Cette case a déjà été touchée ! Considérée comme manquée.");
                    CasesTouchees.Add(c); // On l'ajoute pour qu'elle reste "touchée"
                    return c; // On retourne la coordonnée pour qu'elle soit traitée comme un tir manqué
                }
            } while (c == null);

            CasesTouchees.Add(c);
            return c;
        }


        public string IsTouched(string pos)
        {
            var (col, row) = Positions[pos];
            bool touché = MesBateaux.Any(b => b.Positions.Contains((col, row)));
            return SerializeData(touché);
        }

        public void AfficherMaGrille() => MaGrille.Afficher(CouleurJoueur);
        public void AfficherEnemyGrille() => EnemyGrille.Afficher(CouleurServeur);


        #region Sérialisation
        public string SerializeData(bool data) => JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        public string SerializeData(string data) => JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        public bool DeserializeBoolData(string jsonFile) => JsonSerializer.Deserialize<bool>(jsonFile);
        public string DeserializeStringData(string jsonFile) => JsonSerializer.Deserialize<string>(jsonFile);
        #endregion
    }
}
