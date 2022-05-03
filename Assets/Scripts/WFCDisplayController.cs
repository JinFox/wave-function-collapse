using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class WFCDisplayController : MonoBehaviour
    {
        [SerializeField] private Image _sampleImage;
        [SerializeField] private Image _resultImage;
        [SerializeField] private RectTransform _resultImageContainer;
        [SerializeField] private GridLayoutGroup _tilesDisplayGroup;
        [SerializeField] private GameObject _tilePrefab;
        private List<Image> _tileImages;

        // EntropyDisplay
        [SerializeField] private GridLayoutGroup _entropyAmountGrid;
        private List<TextMeshProUGUI> _entropyTexts;
        [SerializeField] private GameObject _canvasEntropyPrefab;
        public Image SampleImage
        {
            get { return _sampleImage; }
            set { _sampleImage = value; }
        }

        public Image ResultImage
        {
            get { return _resultImage; }
            set { _resultImage = value; }
        }

        // Start is called before the first frame update
        public void SetSampleImage(Texture2D texture)
        {
            SampleImage.material.mainTexture = texture;
        }
        public void SetResultImage(Texture2D texture)
        {
            ResultImage.preserveAspect = true;
            ResultImage.material.mainTexture = texture;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void SetResultImageSize(int textureWidth, int textureHeight)
        {
           
            float newWidth = _resultImageContainer.sizeDelta.x;
            float newHeight = _resultImageContainer.sizeDelta.y;
            if (textureWidth > textureHeight)
            {
                newWidth = newHeight * textureWidth / textureHeight;
            }
            else
            {
                newHeight = newWidth * textureHeight / textureWidth;
            }

            _resultImageContainer.sizeDelta = new Vector2(newWidth, newHeight);
            _entropyAmountGrid.cellSize = new Vector2(newWidth / textureWidth, newHeight / textureHeight);

            GenerateEntropyGrid(textureWidth, textureHeight);
        }

        private void GenerateEntropyGrid(int textureWidth, int textureHeight)
        {
            
            if (_entropyTexts != null)
            {
                foreach (TextMeshProUGUI textMeshPro in _entropyTexts)
                {
                    Destroy(textMeshPro.gameObject);
                }
            }
            
            _entropyTexts = new List<TextMeshProUGUI>(textureWidth * textureHeight);
            
            for (int i = 0; i < textureWidth*textureHeight; i++)
            {
                GameObject entropyGo = Instantiate(_canvasEntropyPrefab);
                TextMeshProUGUI txtMeshPro = entropyGo.GetComponent<TextMeshProUGUI>();
                _entropyTexts.Add(txtMeshPro);
                txtMeshPro.text = i.ToString();
                txtMeshPro.transform.SetParent(_entropyAmountGrid.transform);
            }
        }

        public void AddTile(Texture2D texture, string text = "")
        {
            if (_tileImages == null)
            {
                _tileImages = new List<Image>();
            }
            GameObject tile = Instantiate(_tilePrefab);
            Image img = tile.GetComponent<Image>();
            TextMeshProUGUI txt = tile.GetComponentInChildren<TextMeshProUGUI>();
            _tileImages.Add(img);
            txt.text = text;
            tile.transform.SetParent(_tilesDisplayGroup.transform);
            img.material = new Material(img.material);
            img.material.SetTexture("_MainTex", texture);
        }

        public void ClearTiles()
        {
            if (_tileImages != null)
            {
                foreach (var image in _tileImages)
                {
                    Destroy(image.gameObject);
                }
                _tileImages.Clear();
            }
            
        }

        public void SetEntropyAtIndex(int index, int entropy)
        {
            if (index < _entropyTexts.Count)
            {
                _entropyTexts[index].text = entropy.ToString();
            }
        }
    }
}
