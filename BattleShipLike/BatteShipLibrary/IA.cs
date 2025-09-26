using System;
using System.Collections.Generic;
using BattleShipLibrary;

namespace BattleShip_server
{
    public class IA
    {
        private BattleShip playerBoard;
        private List<string> targetQueue = new List<string>(); // cases autour d'un hit
        private Random rnd = new Random();

        public IA(BattleShip board)
        {
            playerBoard = board;
        }

        // Retourne la prochaine coordonnée où l'IA va tirer
        public string NextMove()
        {
            string coord;

            // Si on a des cases autour d'un hit, on les teste en priorité
            if (targetQueue.Count > 0)
            {
                coord = targetQueue[0];
                targetQueue.RemoveAt(0);
            }
            else
            {
                // Tir aléatoire parmi les cases non touchées
                do
                {
                    int col = rnd.Next(0, playerBoard.MaGrille.Colonnes);
                    int row = rnd.Next(0, playerBoard.MaGrille.Lignes);
                    coord = playerBoard.MaGrille.GetCoordString(col, row);
                } while (playerBoard.CasesTouchees.Contains(coord));
            }

            playerBoard.CasesTouchees.Add(coord);

            // Vérifie si le tir touche un bateau
            bool touche = playerBoard.DeserializeBoolData(playerBoard.IsTouched(coord));

            if (touche)
                AddAdjacentTargets(coord);

            return coord;
        }

        // Ajoute les cases autour d'un hit à la liste des cibles
        private void AddAdjacentTargets(string coord)
        {
            var (col, row) = playerBoard.Positions[coord];

            var candidates = new List<string>
            {
                playerBoard.MaGrille.GetCoordString(col + 1, row),
                playerBoard.MaGrille.GetCoordString(col - 1, row),
                playerBoard.MaGrille.GetCoordString(col, row + 1),
                playerBoard.MaGrille.GetCoordString(col, row - 1)
            };

            foreach (var c in candidates)
            {
                if (playerBoard.Positions.ContainsKey(c) && !playerBoard.CasesTouchees.Contains(c))
                    targetQueue.Add(c);
            }
        }
    }
}
