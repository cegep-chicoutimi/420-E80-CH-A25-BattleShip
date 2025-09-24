using System;
using System.IO;

namespace BattleShipLibrary
{
    public static class ConfigManager
    {
        private static readonly string configPath = "couleurs_config.txt";

        // Charge les couleurs depuis le fichier ou demande au joueur de les choisir
        public static (ConsoleColor joueur, ConsoleColor serveur) LoadOrChooseColors()
        {
            if (File.Exists(configPath))
            {
                string[] lines = File.ReadAllLines(configPath);
                if (lines.Length >= 2 &&
                    Enum.TryParse(lines[0], out ConsoleColor joueur) &&
                    Enum.TryParse(lines[1], out ConsoleColor serveur))
                {
                    return (joueur, serveur);
                }
            }

            // Si le fichier n'existe pas ou est invalide, on demande les couleurs
            Console.WriteLine("Configuration non trouvée ou invalide, choisissez les couleurs :");
            ConsoleColor couleurJoueur = ChoisirCouleur("Joueur");
            ConsoleColor couleurServeur;

            do
            {
                couleurServeur = ChoisirCouleur("Serveur");
                if (couleurServeur == couleurJoueur)
                    Console.WriteLine("Deux joueurs ne peuvent pas avoir la même couleur.");
            } while (couleurServeur == couleurJoueur);

            // Sauvegarde dans le fichier
            File.WriteAllLines(configPath, new[] { couleurJoueur.ToString(), couleurServeur.ToString() });
            Console.WriteLine("Configuration enregistrée.");

            return (couleurJoueur, couleurServeur);
        }

        // Demande à l'utilisateur de choisir une couleur
        private static ConsoleColor ChoisirCouleur(string nomJoueur)
        {
            var couleurs = Enum.GetValues(typeof(ConsoleColor));
            Console.WriteLine($"Choisissez une couleur pour {nomJoueur} :");

            for (int i = 0; i < couleurs.Length; i++)
            {
                Console.WriteLine($"{i}: {couleurs.GetValue(i)}");
            }

            Console.Write("Votre choix (numéro) : ");
            string saisie = Console.ReadLine();

            if (int.TryParse(saisie, out int choix) && choix >= 0 && choix < couleurs.Length)
                return (ConsoleColor)couleurs.GetValue(choix);

            Console.WriteLine("Choix invalide. Couleur par défaut utilisée (Gray).");
            return ConsoleColor.Gray;
        }
    }
}
