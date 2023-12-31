using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [Header("UI Elements")]
    [SerializeField] private Transform levelSelectPanel;
    [SerializeField] private List<Texture2D> imageTextures;
    [SerializeField] private Image levelSelectPrefab;

    [Header("Game Elements")]
    [Range(2, 6)]
    [SerializeField] private int difficulty = 4;
    [SerializeField] private Transform gameHolder;
    [SerializeField] private Transform piecePrefab;

    private List<Transform> pieces;
    private Vector2Int dimensions;
    private float width;
    private float height;

    void Start()
    {
        foreach (Texture2D texture in imageTextures) {
            Image image = Instantiate(levelSelectPrefab, levelSelectPanel);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            image.GetComponent<Button>().onClick.AddListener(delegate { StartGame(texture);  } );
        }
    }

    public void StartGame(Texture2D jigsawTexture) {
        // hide UI
        levelSelectPanel.gameObject.SetActive(false);

        // list of transforms for each piece
        pieces = new List<Transform>();

        // get size of each piece based on difficulty
        dimensions = GetDimensions(jigsawTexture, difficulty);

        // Create the pieces
        CreateJigsawPieces(jigsawTexture);
        Scatter();
        UpdateBorder();

    }

    Vector2Int GetDimensions(Texture2D jigsawTexture, int difficulty) {
        Vector2Int dimensions = Vector2Int.zero;
        if (jigsawTexture.width < jigsawTexture.height) {
            dimensions.x = difficulty;
            dimensions.y = difficulty * jigsawTexture.height / jigsawTexture.width;

        } else {
            dimensions.x = difficulty * jigsawTexture.width / jigsawTexture.height;
            dimensions.y = difficulty;
        }

        return dimensions;
    }

    void CreateJigsawPieces(Texture2D jigsawTexture) {
        height = 1f / dimensions.y;
        float aspect = (float)jigsawTexture.width / jigsawTexture.height;
        width = aspect / dimensions.x;
        for (int row = 0; row < dimensions.y; row++) {
            for (int col = 0; col < dimensions.x; col++) {
                // Create the piece in the right location of the right size
                Transform piece = Instantiate(piecePrefab, gameHolder);
                piece.localPosition = new Vector3(
                   (-width * dimensions.x / 2) + (width * col) + (width / 2),
                   (-height * dimensions.x / 2) + (height * row) + (height / 2),
                   -1);
                piece.localScale = new Vector3(width, height, 1f);
                piece.name = $"Piece {(row * dimensions.x) + col}";
                pieces.Add(piece);

                // Assign correct part of texture image to piece
                // Normalize width and height
                float width1 = 1f / dimensions.x;
                float height1 = 1f / dimensions.y;

                Vector2[] uv = new Vector2[4];
                uv[0] = new Vector2(width1 * col, height1 * row);
                uv[1] = new Vector2(width1 * (1 + col), height1 * row);
                uv[2] = new Vector2(width1 * col, height1 * (1 + row));
                uv[3] = new Vector2(width1 * (1 + col), height1 * (1 + row));

                Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                mesh.uv = uv;
                piece.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", jigsawTexture);

            }
        }

    }

    private void Scatter() {
        // Calculate the visible screen size
        float orthoHeight = Camera.main.orthographicSize;
        float screenAspect = (float)Screen.width / Screen.height;
        float orthoWidth = (screenAspect * orthoHeight);

        // confine pieces to edges of screen
        float pieceWidth = width * gameHolder.localScale.x;
        float pieceHeight = height * gameHolder.localScale.y;
        orthoHeight -= pieceHeight;
        orthoWidth -= pieceWidth;

        foreach (Transform piece in pieces) {
            float x = Random.Range(-orthoWidth, orthoWidth);
            float y = Random.Range(-orthoHeight, orthoHeight);
            piece.position = new Vector3(x, y, -1);
        }
    }

    private void UpdateBorder() {
        LineRenderer lineRenderer = gameHolder.GetComponent<LineRenderer>();
        Debug.Log(width);
        Debug.Log(dimensions.x);
        float halfWidth = width * dimensions.x / 2f;
        float halfHeight = height * dimensions.y / 2f;
        float borderZ = 0f;
        Debug.Log(halfWidth);

        lineRenderer.SetPosition(0, new Vector3(-halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(1, new Vector3(halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(2, new Vector3(halfWidth, -halfHeight, borderZ));
        lineRenderer.SetPosition(3, new Vector3(-halfWidth, -halfHeight, borderZ));
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        lineRenderer.enabled = true;
    }
}
