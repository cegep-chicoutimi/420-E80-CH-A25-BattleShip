using System;
using System.Text.Json;

namespace BattleShipLibrary
{
    public class BattleShip
    {
        private int Colonnes;
        private int Lignes;

        public string[,] MaGrille;
        public string[,] EnemyGrille;

        public List<Bateau> MesBateaux = new();

        public Dictionary<string, (int, int)> Positions = new();

        public ConsoleColor CouleurJoueur { get; set; } = ConsoleColor.Green;   // Par défaut
        public ConsoleColor CouleurServeur { get; set; } = ConsoleColor.Yellow; // Par défaut

        public void StartGame(int c, int l)
        {
            Colonnes = c;
            Lignes = l;

            MaGrille = new string[Colonnes, Lignes];
            EnemyGrille = new string[Colonnes, Lignes];

            RemplirGrilles();

            // Initialiser bateaux avec formes pré-cannées
            MesBateaux = new List<Bateau>
            {
                new Bateau(new List<(int,int)> { (0,0), (1,0), (0,1) }), // L
                new Bateau(new List<(int,int)> { (0,0), (-1,1), (0,1), (1,1) }), // T
                new Bateau(new List<(int,int)> { (0,0), (1,0), (0,1), (1,1) }) // carré 2x2
            };

            // Pour chaque bateau, demander la position à l'utilisateur
            foreach (var bateau in MesBateaux)
            {
                bool placé = false;
                do
                {
                    string pos = ChoisirCase($"Choisissez la case d'ancrage pour votre bateau de taille {bateau.Forme.Count} :");
                    var (col, row) = Positions[pos];

                    if (!bateau.Placer(col, row, Colonnes, Lignes))
                    {
                        Console.WriteLine("Bateau ne rentre pas dans la grille à cette position, choisissez-en une autre.");
                        continue;
                    }

                    // Vérifier qu’il n’y a pas de chevauchement avec bateaux déjà placés
                    if (MesBateaux.Any(b => b != bateau && b.Positions.Any(p => bateau.Positions.Contains(p))))
                    {
                        Console.WriteLine("Chevauchement détecté avec un autre bateau, choisissez une autre position.");
                        continue;
                    }

                    // Placer dans la grille
                    foreach (var (x, y) in bateau.Positions)
                    {
                        MaGrille[x, y] = "B";
                    }
                    placé = true;

                    AfficherMaGrille();

                } while (!placé);
            }
        }

        public void AfficherMaGrille()
        {
            AfficherGrille(MaGrille, CouleurJoueur);
        }

        public void AfficherEnemyGrille()
        {
            AfficherGrille(EnemyGrille, CouleurServeur);
        }

        // Méthode modifiée pour prendre en compte la couleur
        private void AfficherGrille(string[,] grille, ConsoleColor couleur)
        {
            Console.ForegroundColor = couleur;

            // En-tête des colonnes
            for (int col = 0; col < Colonnes; col++)
            {
                char lettreColonne = (char)('A' + col);
                Console.Write($"   {lettreColonne}");
            }
            Console.WriteLine();

            for (int row = 0; row < Lignes; row++)
            {
                Console.Write(row + 1 < 10 ? $" {row + 1} " : $"{row + 1} ");

                for (int col = 0; col < Colonnes; col++)
                {
                    Console.Write($"{grille[col, row]}   ");
                }

                Console.WriteLine();
            }

            Console.ResetColor();
        }

        public string ChoisirCase(string prompt)
        {
            string c;
            do
            {
                Console.WriteLine(prompt);
                c = Console.ReadLine()?.ToUpper();
            } while (string.IsNullOrWhiteSpace(c) || !Positions.ContainsKey(c));

            return c;
        }

        public string IsTouched(string pos)
        {
            var (col, row) = Positions[pos];

            bool touché = MesBateaux.Any(b => b.Positions.Contains((col, row)));
            return SerializeData(touché);
        }

        public void RemplirGrilles()
        {
            Positions.Clear();

            for (int col = 0; col < Colonnes; col++)
            {
                char lettre = (char)('A' + col);

                for (int row = 0; row < Lignes; row++)
                {
                    string key = $"{lettre}{row + 1}";
                    Positions[key] = (col, row);

                    MaGrille[col, row] = "~";
                    EnemyGrille[col, row] = "~";
                }
            }
        }

        #region Sérialisation
        public string SerializeData(bool data)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(data, options);
        }

        public string SerializeData(string data)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(data, options);
        }

        public bool DeserializeBoolData(string jsonFile)
        {
            return JsonSerializer.Deserialize<bool>(jsonFile);
        }

        public string DeserializeStringData(string jsonFile)
        {
            return JsonSerializer.Deserialize<string>(jsonFile);
        }
        #endregion
    }
}
