using System;
using System.Collections.Generic;
using System.Linq;
using BattleShipLibrary;

namespace BattleShip_server
{
    public class IA
    {
        private BattleShip playerBoard;
        private Queue<string> targetQueue = new(); // FIFO pour les cibles autour
        private List<string> availableMoves; // cases encore dispo
        private Random rnd = new Random();

        public IA(BattleShip board)
        {
            playerBoard = board;
            availableMoves = playerBoard.MaGrille.Positions.Keys.ToList();
        }

        public string NextMove()
        {
            string coord;

            // 1. S'il reste des cibles adjacentes à tester → priorité
            if (targetQueue.Count > 0)
            {
                coord = targetQueue.Dequeue();
                availableMoves.Remove(coord);
            }
            else
            {
                // 2. Sinon → random sur une case dispo
                int index = rnd.Next(availableMoves.Count);
                coord = availableMoves[index];
                availableMoves.RemoveAt(index);
            }

            // 3. Vérifie le tir
            bool touche = playerBoard.DeserializeBoolData(playerBoard.IsTouched(coord));

            // 4. Si touché → on ajoute les voisins comme cibles prioritaires
            if (touche)
                AddAdjacentTargets(coord);

            return coord;
        }

        private void AddAdjacentTargets(string coord)
        {
            var (col, row) = playerBoard.Positions[coord];

            var candidates = new List<(int c, int r)>
            {
                (col + 1, row),
                (col - 1, row),
                (col, row + 1),
                (col, row - 1)
            };

            foreach (var (c, r) in candidates)
            {
                var match = playerBoard.MaGrille.Positions
                    .FirstOrDefault(p => p.Value.Item1 == c && p.Value.Item2 == r);

                if (!string.IsNullOrEmpty(match.Key) && availableMoves.Contains(match.Key))
                {
                    targetQueue.Enqueue(match.Key); // met les voisins en file d’attente
                }
            }
        }
    }
}
