using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace BattleShipLibrary
{
    public class BattleShip
    {
        private int Colonnes { get; set; }
        private int Lignes { get; set; }

        private string[,] MaGrille;
        public string[,] EnemyGrille;

        private string FirstShipCase;
        private string SecondShipCase;

        public Dictionary<string, (int, int)> Positions = new Dictionary<string, (int, int)>();

        public void StartGame(int c, int l)
        {
            Lignes = l;
            Colonnes = c;
            bool isSecondCaseValid = false;

            MaGrille = new string[Lignes, Colonnes];
            EnemyGrille = new string[Lignes, Colonnes];

            RemplirGrilles();

            AfficherMaGrille();

            FirstShipCase = ChoisirCase("Première case du bateau ?");

            do
            {
                SecondShipCase = ChoisirCase("Deuxième case du bateau ? ");

                var (col1, row1) = Positions[FirstShipCase];
                var (col2, row2) = Positions[SecondShipCase];

                int diffCol = Math.Abs(col1 - col2);
                int diffRow = Math.Abs(row1 - row2);

                // Adjacent si différence de 1 dans une direction et 0 dans l'autre
                if ((diffCol == 1 && diffRow == 0) || (diffCol == 0 && diffRow == 1))
                {
                    isSecondCaseValid = true;
                    MaGrille[col1, row1] = "B";
                    MaGrille[col2, row2] = "B";
                }
            } while (!isSecondCaseValid);

            AfficherMaGrille();
            AfficherEnemyGrille();
        }

        public void AfficherMaGrille()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nMA GRILLE");
            Console.ResetColor();

            AfficherEnteteColonnes();

            for (int row = 0; row < Lignes; row++)
            {
                Console.Write(row + 1 < 10 ? $" {row + 1} " : $"{row + 1} ");

                for (int col = 0; col < Colonnes; col++)
                {
                    Console.Write($"{MaGrille[col, row]}   ");
                }

                Console.WriteLine();
            }
        }

        public void AfficherEnemyGrille()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nGRILLE ENNEMIE");
            Console.ResetColor();

            AfficherEnteteColonnes();

            for (int row = 0; row < Lignes; row++)
            {
                Console.Write(row + 1 < 10 ? $" {row + 1} " : $"{row + 1} ");

                for (int col = 0; col < Colonnes; col++)
                {
                    Console.Write($"{EnemyGrille[col, row]}   ");
                }

                Console.WriteLine();
            }
        }

        private void AfficherEnteteColonnes()
        {
            for (int col = 0; col < Colonnes; col++)
            {
                char lettreColonne = (char)('A' + col);
                Console.Write($"   {lettreColonne}");
            }
            Console.WriteLine();
        }

        public string ChoisirCase(string p)
        {
            string c;
            do
            {
                Console.WriteLine(p);
                c = Console.ReadLine().ToUpper();
            } while (!Positions.ContainsKey(c));

            return c;
        }

        private int SaisieTaille(string p)
        {
            bool isValid;
            int n;

            do
            {
                Console.WriteLine(p);
                isValid = int.TryParse(Console.ReadLine(), out n);

                if (!isValid || n > 26)
                    Console.WriteLine("Ce nombre n'est pas correct !");
            }
            while (!isValid || n > 26);

            return n;
        }

        public string IsTouched(string pos)
        {
            var (col, row) = Positions[pos];
            return SerializeData(MaGrille[col, row] == "B");
        }

        public void RemplirGrilles()
        {
            for (int col = 0; col < Colonnes; col++)
            {
                char lettre = (char)('A' + col);

                for (int row = 0; row < Lignes; row++)
                {
                    Positions.Add($"{lettre}{row + 1}", (col, row));
                    MaGrille[col, row] = "~";
                    EnemyGrille[col, row] = "~";
                }
            }
        }

        public string SerializeData(bool data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(data, options);
        }

        public bool DeserializeBoolData(string jsonFile)
        {
            return JsonSerializer.Deserialize<bool>(jsonFile);
        }

        public string SerializeData(string data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(data, options);
        }

        public string DeserializeStringData(string jsonFile)
        {
            return JsonSerializer.Deserialize<string>(jsonFile);
        }
    }
}
