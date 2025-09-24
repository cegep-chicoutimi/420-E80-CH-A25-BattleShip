using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipLibrary
{
    public class Grille
    {
        public string[,] Cells { get; private set; }
        public Dictionary<string, (int, int)> Positions { get; private set; }

        public int Colonnes { get; private set; }
        public int Lignes { get; private set; }

        public Grille(int colonnes, int lignes)
        {
            Colonnes = colonnes;
            Lignes = lignes;
            Cells = new string[Colonnes, Lignes];
            Positions = new Dictionary<string, (int, int)>();
            Initialize();
        }

        private void Initialize()
        {
            for (int c = 0; c < Colonnes; c++)
            {
                char lettre = (char)('A' + c);
                for (int r = 0; r < Lignes; r++)
                {
                    Cells[c, r] = "~";
                    Positions[$"{lettre}{r + 1}"] = (c, r);
                }
            }
        }

        // Affichage classique en couleur directement dans la console
        public void Afficher(ConsoleColor couleur)
        {
            Console.ForegroundColor = couleur;

            // En-tête des colonnes
            for (int col = 0; col < Colonnes; col++)
                Console.Write($"   {(char)('A' + col)}");
            Console.WriteLine();

            // Lignes de la grille
            for (int row = 0; row < Lignes; row++)
            {
                Console.Write(row + 1 < 10 ? $" {row + 1} " : $"{row + 1} ");
                for (int col = 0; col < Colonnes; col++)
                    Console.Write($"{Cells[col, row]}   ");
                Console.WriteLine();
            }

            Console.ResetColor();
        }
    }
}
