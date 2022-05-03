using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class WaveFunctionCollapseController : MonoBehaviour
    {
        [SerializeField]private int _seed;
        [SerializeField] private WFCDisplayController _displayController;
        [SerializeField] private int _nbIndexPerFrame = 1;
        [SerializeField] private float _timeBetweenFrame = 0.5f;
        [SerializeField] private Vector2Int _outputSize = new Vector2Int(32, 32);
        private int _textureWidth;
        private int _textureHeight;
        private Color[] _samplePixels;

        private int _currentIndex = 0;
        private float _timer;
        private bool _isRenderingFinished;
        private OverlappingWaveCollapseModel _model;
        

        // Start is called before the first frame update
        void Start()
        {
            Random.InitState(_seed);
            _model = new OverlappingWaveCollapseModel();
            GetImageInformation(_displayController.SampleImage, out _textureWidth, out _textureHeight,
                out Color[] samplePixels);

            _samplePixels = samplePixels;
            _model.Initialize(_textureWidth, _textureHeight, _samplePixels);

            DisplayPatterns(_model);

         
            _displayController.SetResultImageSize(_outputSize.x, _outputSize.y);
            _model.SetOutputSize(_outputSize.x, _outputSize.y);

            _model.RunIteration(out Texture2D outputTexture, _displayController.SetEntropyAtIndex);
            _displayController.SetResultImage(outputTexture);
          
            StartRendering();
        }

        private void DisplayPatterns(OverlappingWaveCollapseModel model)
        {
            _displayController.ClearTiles();
            var sortedPattern = new List<TilePattern>(model.Patterns);
            sortedPattern.Sort(TilePattern.Compare);
            foreach (TilePattern pattern in sortedPattern)
            {

                _displayController.AddTile(pattern.GetTexture(model.TileColors), pattern.ID.ToString());
            }
        }

        private void StartRendering()
        {
            _isRenderingFinished = false;
            _currentIndex = 0;
            _timer = 0f;
        }
        
        private void GetImageInformation(Image image, out int textureWidth, out int textureHeight, out Color[] colors)
        {
            textureWidth = image.mainTexture.width;
            textureHeight = image.mainTexture.height;

            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                textureWidth,
                textureHeight,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);


            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(image.mainTexture, tmp);


            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;


            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;


            // Create a new readable Texture2D to copy the pixels to it
            Texture2D myTexture2D = new Texture2D(textureWidth, textureHeight);


            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();


            // Reset the active RenderTexture
            RenderTexture.active = previous;


            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            colors = myTexture2D.GetPixels(0);
        }

        private int nbIteration = 0;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                
                Debug.Log($"Run Iteration {nbIteration++}");
                _model.RunIteration(out Texture2D outputTexture, _displayController.SetEntropyAtIndex);
                _displayController.SetResultImage(outputTexture);
            }
            //if (!_isRenderingFinished && _timer < 0f)
            //{
            //    _timer = _timeBetweenFrame;
            //    SetPixelOnTexture(_samplePixels, _currentIndex, _nbIndexPerFrame);
            //    _currentIndex += _nbIndexPerFrame;
            //}
            //_timer -= Time.deltaTime;
        }


    }
}
