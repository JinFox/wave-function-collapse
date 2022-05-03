using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class Tile
    {
        private readonly int _index;
        private readonly int _y;
        private readonly int _x;
        
        private bool IsCollapsed { get; set; }
        public List<int> RemainingPossiblePatternsIds { get; set; }

        public Tile(int index, int x, int y, int[] allPossibilities)
        {
            _index = index;
            _x = x;
            _y = y;
            RemainingPossiblePatternsIds = new List<int>(allPossibilities);
        }
        public void RemovePossibility(int id)
        {
            if (RemainingPossiblePatternsIds.Count == 1)
            {
                Debug.Log($"Trying to remove the last option");
                return;
            }
            RemainingPossiblePatternsIds.Remove(id);
            if (RemainingPossiblePatternsIds.Count <= 1)
            {
                IsCollapsed = true;
            }
        }
        public void CollapseRandomly()
        {
            if (RemainingPossiblePatternsIds.Count == 0)
            {
                Debug.Log($"CANNOT COLLAPSE, NO MORE POSSIBILITY");
                return;
            }
           
            RemainingPossiblePatternsIds = new List<int>
            {
                RemainingPossiblePatternsIds[Random.Range(0, RemainingPossiblePatternsIds.Count)]
            };
            IsCollapsed = true;
        }

        public Color GetPossibilityColor(List<TilePattern> patterns, List<Color> tileColors)
        {
            Color result = new Color(0, 0, 0, 0);
            foreach (var patternId in RemainingPossiblePatternsIds)
            {
                result += tileColors[patterns[patternId].ColorId];

            }
            result /= RemainingPossiblePatternsIds.Count;
            return result;
        }

        public bool UpdateEntropy(List<TilePattern> patterns, List<Color> tileColors, Tile[] surroundings)
        {
            bool wasChanged = false;
            // Shouldn't be like this, we should filter by patter that have the tile index we want
            List<TilePattern> remainingPossiblePattern =
                patterns.Where(p => RemainingPossiblePatternsIds.Contains(p.ID)).ToList();

            Debug.Log($"\t{nameof(UpdateEntropy)}():{this}");

            foreach (TilePattern tilePattern in remainingPossiblePattern)
            {
                // Check for each possible pattern if it is compatible with given surrounding
                if (_index == 6 && tilePattern.ID == 1)
                {
                    Debug.Log($"DANGER INDEX {_index}");
                }
                if (!tilePattern.CheckCompatibleWith(remainingPossiblePattern, surroundings))
                {
                    //Debug.Log($"\tRemoving pattern {tilePattern.ID}");
                    RemovePossibility(tilePattern.ID);
                    wasChanged = true;
                }
            }
            Debug.Log($"\tAfter Updating Entropy {this}: {ShowPatternList()}");
            return wasChanged;
        }

        public override string ToString()
        {
            return $"[Tile({_index}):{_x},{_y}|{RemainingPossiblePatternsIds.Count} Pattern possible]";
        }
        public string ShowPatternList()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"({RemainingPossiblePatternsIds.Count}):");
            for (int i = 0; i < RemainingPossiblePatternsIds.Count; i++)
            {
                sb.Append($"{RemainingPossiblePatternsIds[i]},");
            }

            return sb.ToString();
        }

        public void GetPossibleColorList(List<TilePattern> patternList, out HashSet<int> colorIdList)
        {
            colorIdList = new HashSet<int>(this.RemainingPossiblePatternsIds.Count);

            foreach (int remainingPossiblePatternsId in RemainingPossiblePatternsIds)
            {
                if (patternList.Count > remainingPossiblePatternsId)
                {
                    colorIdList.Add(patternList[remainingPossiblePatternsId].ColorId);
                }
                

            }

        }
    }
}