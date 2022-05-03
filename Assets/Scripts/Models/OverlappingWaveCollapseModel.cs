using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Models
{
    public class OverlappingWaveCollapseModel
    {
        private const int InvalidTileColorId = -1;

        private List<Color> _tileColors;
        private List<TilePattern> _patterns;
        private Tile[] _canvasTiles;
        private int _sampleTileWidth;
        private int _sampleTileHeight;
        private int _canvasWidth;
        private int _canvasHeight;
        private Texture2D _canvasTexture;

        public OverlappingWaveCollapseModel()
        {

        }

        public List<TilePattern> Patterns
        {
            get { return _patterns; }
        }

        public List<Color> TileColors => _tileColors;

        public void Initialize(int width, int height, Color[] samplePixels)
        {
            _sampleTileWidth = width;
            _sampleTileHeight = height;
            _tileColors = new List<Color>();
            _patterns = new List<TilePattern>();
            List<string> alreadyAddedPattern = new List<string>();
            int currentIdShift = 0;

            for (int i = 0; i < samplePixels.Length; i++)
            {
                if (!_tileColors.Contains(samplePixels[i]))
                {
                    Debug.Log($"Adding id {_tileColors.Count} for color {samplePixels[i]}");
                    _tileColors.Add(samplePixels[i]);
                }
            }

            for (int y = 1; y < _sampleTileHeight - 1; y++)
            {
                for (int x = 1; x < _sampleTileWidth - 1; x++)
                {
                    int index = y * _sampleTileWidth + x;
                    //if (_tileColors.Contains(samplePixels[index]))
                    //{
                    GetSurrounding(samplePixels, index, _sampleTileWidth, out int[] surrounding);
                    var tilePattern = new TilePattern(_tileColors.IndexOf(samplePixels[index]), surrounding);
                    string patternIdentity = tilePattern.GetIdentity();
                    if (!alreadyAddedPattern.Contains(patternIdentity))
                    {
                        tilePattern.ID = _patterns.Count;
                        _patterns.Add(tilePattern);

                        alreadyAddedPattern.Add(patternIdentity);
                    }
                    else
                    {
                        //Debug.Log($"SIMILAR : {tilePattern.ToString()} and {existingPattern.ToString()}");
                    }
                    //}
                }
            }

        }

        public void SetOutputSize(int width, int height)
        {
            _canvasWidth = width;
            _canvasHeight = height;

            _canvasTiles = new Tile[width * height];

            int[] allTileIds = new int[Patterns.Count];

            for (int i = 0; i < Patterns.Count; i++)
            {
                allTileIds[i] = Patterns[i].ID;
            }


            for (int y = 0; y < _canvasHeight; y++)
            {
                for (int x = 0; x < _canvasWidth; x++)
                {
                    int index = x + y * _canvasWidth;
                    _canvasTiles[index] = new Tile(index, x, y, allTileIds);
                }
            }

            GenerateTexture();
        }

        private void GenerateTexture()
        {
            if (_canvasTexture == null)
            {
                _canvasTexture = new Texture2D(_canvasWidth, _canvasHeight, DefaultFormat.HDR, TextureCreationFlags.None);
                _canvasTexture.filterMode = FilterMode.Point;
            }

            Color[] pixels = _canvasTexture.GetPixels(0);

            for (var index = 0; index < _canvasTiles.Length; index++)
            {
                var canvasTile = _canvasTiles[index];

                pixels[index] = canvasTile.GetPossibilityColor(Patterns, _tileColors);
            }

            _canvasTexture.SetPixels(pixels);
            _canvasTexture.Apply();
        }

        private void GetSurrounding(Color[] samplePixel, int i, int width, out int[] surroundings)
        {
            surroundings = new int[9];

            //Debug.Log($"Generating tile for {i} x : {i % _sampleTileWidth} | y : {i / _sampleTileWidth} : Color : {samplePixel[i]}");

            surroundings[0] = GetPixelColorId(samplePixel, i - width - 1);
            surroundings[1] = GetPixelColorId(samplePixel, i - width);
            surroundings[2] = GetPixelColorId(samplePixel, i - width + 1);
            surroundings[3] = GetPixelColorId(samplePixel, i - 1);
            surroundings[4] = GetPixelColorId(samplePixel, i);
            surroundings[5] = GetPixelColorId(samplePixel, i + 1);
            surroundings[6] = GetPixelColorId(samplePixel, i + width - 1);
            surroundings[7] = GetPixelColorId(samplePixel, i + width);
            surroundings[8] = GetPixelColorId(samplePixel, i + width + 1);
        }

        private int GetPixelColorId(Color[] samplePixel, int index)
        {
            if (index < 0 || index > samplePixel.Length)
                return InvalidTileColorId;
            return _tileColors.IndexOf(samplePixel[index]);
        }



        private void GetSurroundingTiles(int i, out Tile[] surroundings)
        {
            surroundings = new Tile[9];

            //Debug.Log($"Generating tile for {i} x : {i % _sampleTileWidth} | y : {i / _sampleTileWidth} : Color : {samplePixel[i]}");
            surroundings[0] = GetTile(_canvasTiles, i - _canvasWidth - 1);
            surroundings[1] = GetTile(_canvasTiles, i - _canvasWidth);
            surroundings[2] = GetTile(_canvasTiles, i - _canvasWidth + 1);
            surroundings[3] = GetTile(_canvasTiles, i - 1);
            surroundings[4] = null;
            surroundings[5] = GetTile(_canvasTiles, i + 1);
            surroundings[6] = GetTile(_canvasTiles, i + _canvasWidth - 1);
            surroundings[7] = GetTile(_canvasTiles, i + _canvasWidth);
            surroundings[8] = GetTile(_canvasTiles, i + _canvasWidth + 1);
        }

        private Tile GetTile(Tile[] tiles, int index)
        {
            if (index < 0 || index >= tiles.Length)
                return null;
            return tiles[index];
        }


        public void RunIteration(out Texture2D texture, Action<int, int> setEntropyFunction)
        {

            int collapsedIndex = CollapseBestCandidate();

            if (collapsedIndex != -1)
            {
                UpdateEntropy(collapsedIndex);
            }
            else
            {
                Debug.Log($"<color=green>Collapsing finished</color>");
            }
            for (var index = 0; index < _canvasTiles.Length; index++)
            {
                setEntropyFunction.Invoke(index, _canvasTiles[index].RemainingPossiblePatternsIds.Count);
            }
            GenerateTexture();
            texture = _canvasTexture;
        }

        private void GetTile(int x, int y, out Tile tile)
        {
            int index = x + y * _canvasWidth;
            tile = _canvasTiles[index];
        }

        private void GetCoordFromIndex(int index, out int x, out int y)
        {
            x = index % _canvasWidth;
            y = index / _canvasWidth;
        }



        private int CollapseBestCandidate()
        {
            int minEntropyIndex = -1;
            int minEntropyFound = int.MaxValue;

            for (var index = 0; index < _canvasTiles.Length; index++)
            {
                Tile canvasTile = _canvasTiles[index];
                if (canvasTile.RemainingPossiblePatternsIds.Count != 1 &&
                    minEntropyFound >= canvasTile.RemainingPossiblePatternsIds.Count)
                {
                    if (minEntropyFound == canvasTile.RemainingPossiblePatternsIds.Count && Random.Range(0, 2) == 0)
                    {
                        continue;
                    }
                    // TODO :  // if it is the same, theres 50% change to take the new one
                    minEntropyFound = canvasTile.RemainingPossiblePatternsIds.Count;
                    minEntropyIndex = index;
                }
            }

            if (minEntropyIndex != -1)
            {
                GetCoordFromIndex(minEntropyIndex, out int x, out int y);

                _canvasTiles[minEntropyIndex].CollapseRandomly();
                Debug.Log($"COLLAPSING Tile with entropy {minEntropyFound}| index {minEntropyIndex}| {_canvasTiles[minEntropyIndex]}");
            }

            return minEntropyIndex;
        }

        private void UpdateEntropy(int collapsedIndex)
        {
            GetCoordFromIndex(collapsedIndex, out int tileX, out int tileY);
            //tileX -= 1;
            //int index = tileX + tileY * _canvasWidth;
            //Debug.Log($"DEBUG ENTROPY AT POS  | coord [{tileX},{tileY}]");
            //GetTile(tileX, tileY, out Tile tile);

            //GetSurroundingTiles(index, out Tile[] surrounding);
            //tile.UpdateEntropy(_patterns, _tileColors, surrounding);

            Debug.Log($"Updating Entropy from {collapsedIndex}");
            int waveLevel = 1;
            bool somethingChanged = UpdateEntropyLevel(collapsedIndex, waveLevel, tileX, tileY);
            Debug.Log($"{nameof(UpdateEntropy)}(index:{collapsedIndex}) -> somethingChanged {somethingChanged}");

           // UpdateEntropyLevel(collapsedIndex, 2, tileX, tileY);
        }

        private bool UpdateEntropyLevel(int collapsedIndex, int waveLevel, int collapsedTileX, int collapsedTileY)
        {
            Debug.Log($"Starting wave check {waveLevel}");
            bool wasChanged = false;
            for (int offsetY = -waveLevel; offsetY <= waveLevel; offsetY++)
            {
                int y = offsetY + collapsedTileY;
                if (y < 0 || y >= _canvasHeight)
                {
                    Debug.Log($"Y: Skipped Y:{y} UpdatingEntropy (wave {waveLevel}) for {y} ");
                    continue;
                }
                for (int offsetX = -waveLevel; offsetX <= waveLevel; offsetX++)
                {
                    int x = offsetX + collapsedTileX;
                    int index = x + y * _canvasWidth;
                    
                    if ((Mathf.Abs(offsetX) != waveLevel && Mathf.Abs(offsetY) != waveLevel) || x < 0 || x >= _canvasWidth)
                    {
                      //  Debug.Log($"Skipped UpdatingEntropy (wave {waveLevel}) for {x},{y} i:{index}");
                        continue;
                    }
                    //Debug.Log($"Checking position {x},{y} i:{index}");


                    GetTile(x, y, out Tile tile);

                    GetSurroundingTiles(index, out Tile[] surrounding);
                    wasChanged |= tile.UpdateEntropy(_patterns, _tileColors, surrounding);

                }
            }

            return wasChanged;
        }
    }
}
