using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShipLibrary
{
    public class Bateau
    {
        // Liste des positions relatives depuis la case d'ancrage (0,0)
        public List<(int x, int y)> Forme { get; private set; }

        // Positions absolues dans la grille (après placement)
        public List<(int x, int y)> Positions { get; private set; }

        public Bateau(List<(int x, int y)> forme)
        {
            Forme = forme;
            Positions = new List<(int x, int y)>();
        }

        // Méthode pour définir la position (ancrage + translation)
        public bool Placer(int startX, int startY, int colonnes, int lignes)
        {
            var nouvellesPositions = new List<(int x, int y)>();
            foreach (var (dx, dy) in Forme)
            {
                int x = startX + dx;
                int y = startY + dy;

                // Vérifie si la position est dans la grille
                if (x < 0 || y < 0 || x >= colonnes || y >= lignes)
                    return false;

                nouvellesPositions.Add((x, y));
            }

            Positions = nouvellesPositions;
            return true;
        }
    }

}
