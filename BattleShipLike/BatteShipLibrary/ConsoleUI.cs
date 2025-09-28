using System;
using System.Linq;
using BattleShipLibrary;

namespace BattleShipLibrary
{
    public static class ConsoleUI
    {
        public static void WriteColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void WriteWarning(string text) => WriteColored(text, ConsoleColor.Red);
        public static void WriteSuccessful(string text) => WriteColored(text, ConsoleColor.Green);
        public static void WriteWaiting(string text) => WriteColored(text, ConsoleColor.Yellow);

        public static int SaisieTaille(string prompt)
        {
            int n;
            int min = 6, max = 12;
            bool valide;
            do
            {
                Console.WriteLine(prompt);
                valide = int.TryParse(Console.ReadLine(), out n) && n >= min && n <= max;
                if (!valide) WriteWarning($"Nombre invalide. Choisissez entre {min} et {max}.");
            } while (!valide);
            return n;
        }

        public static ConsoleColor ChoisirCouleur(string nomJoueur)
        {
            Console.WriteLine($"Choisissez une couleur pour {nomJoueur} :");
            var couleurs = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().ToArray();
            for (int i = 0; i < couleurs.Length; i++)
                Console.WriteLine($"{i}: {couleurs[i]}");

            Console.Write("Votre choix (numéro) : ");
            if (int.TryParse(Console.ReadLine(), out int choix) && choix >= 0 && choix < couleurs.Length)
                return couleurs[choix];

            WriteWarning("Choix invalide. Couleur par défaut utilisée (Gray).");
            return ConsoleColor.Gray;
        }
    }
}
