using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Assets.Scripts.Models
{
    public class TilePattern
    {
        public enum Side
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        private readonly int _size = 3;
        private readonly int _colorId;
        private readonly int[] _surroundings;
        public int ID { get; set; }

        public int ColorId
        {
            get { return _colorId; }
        }
        /// <summary>
        /// 6   7   8
        /// 3   4   5
        /// 0   1   2
        /// </summary>
        public TilePattern(int ownColorId, int[] surroundingColorIds)
        {

            _colorId = ownColorId;
            _surroundings = surroundingColorIds;

        }

     

        //public Color[] GetSide(Side side)
        //{
        //    Color[] colors = new Color[3];
        //    switch (side)   
        //    {
        //        case Side.Left:
        //            colors[0] = _surroundings[0];
        //            colors[1] = _surroundings[3];
        //            colors[2] = _surroundings[6];
        //            break;
        //        case Side.Top:
        //            colors[0] = _surroundings[0];
        //            colors[1] = _surroundings[1];
        //            colors[2] = _surroundings[2];
        //            break;
        //        case Side.Right:
        //            colors[0] = _surroundings[2];
        //            colors[1] = _surroundings[5];
        //            colors[2] = _surroundings[8];
        //            break;
        //        case Side.Bottom:
        //            colors[0] = _surroundings[6];
        //            colors[1] = _surroundings[7];
        //            colors[2] = _surroundings[8];
        //            break;
        //    }
        //    return colors;
        //}

        public Texture2D GetTexture(List<Color> tileColors)
        {
            var generatedTexture = new Texture2D(3, 3, DefaultFormat.HDR, TextureCreationFlags.None);
            generatedTexture.filterMode = FilterMode.Point;

            Color[] pixels = generatedTexture.GetPixels(0);

            for (int i = 0; i < pixels.Length; i++)
            {
                if (i == 4) // center
                {
                    pixels[i] = tileColors[_surroundings[i]];
                }
                else
                {
                    pixels[i] = tileColors[_surroundings[i]] * 0.8f;
                }
            }

            generatedTexture.SetPixels(pixels);
            generatedTexture.Apply();
            return generatedTexture;
        }

        public override int GetHashCode()
        {
            int hashCode = _surroundings[0].GetHashCode();
            for (int i = 0; i < _surroundings.Length; i++)
            {
                hashCode ^= _surroundings[i].GetHashCode() << i;
            }
            return hashCode;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[{nameof(TilePattern)}|id:{ID}|c:{ColorId}|");
            foreach (int surrounding in _surroundings)
            {
                sb.Append($"{surrounding},");
            }
            return sb.Append("]").ToString();
        }

        public static int Compare(TilePattern a, TilePattern b)
        {
            int res = a.ColorId.CompareTo(b.ColorId);
            if (res != 0)
                return res;
            for (int i = 0; i < a._surroundings.Length; i++)
            {
                res += a._surroundings[i].CompareTo(b._surroundings[i]);
            }

            return res;
        }

        public bool CheckCompatibleWith(List<TilePattern> patterns, Tile[] surroundings)
        {
         //   Debug.Log($"{nameof(CheckCompatibleWith)}");
            for (var index = 0; index < surroundings.Length; index++)
            {
                Tile tile = surroundings[index];

                if (tile != null)
                {
                    //Debug.Log($"{nameof(CheckCompatibleWith)}");
                    tile.GetPossibleColorList(patterns, out HashSet<int> colorIdList);
                    StringBuilder sb = new StringBuilder();
                    
                    foreach (int colorId in colorIdList)
                    {
                        sb.Append($"{colorId},");
                    }
                    int surroundingColor = _surroundings[index];
                    //Debug.Log($"Checking on Pattern {ID} [at:{index}]| is {surroundingColor} is in {tile} [{sb}]");
                    if (!colorIdList.Contains(surroundingColor))
                    {
                        Debug.Log($"\t\tNOT COMPATIBLE: with Checking if Pattern {ID}  compatible {tile} | {surroundingColor} not in Possible Colors : [{sb}] ");
                        return false;
                    }
                        
                }

              
            }

            return true;
        }

        public string GetIdentity()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{ColorId}");
            for (int i = 0; i < _surroundings.Length; i++)
            {
                sb.Append(_surroundings[i]);
            }

            return sb.ToString();
        }
    }
}