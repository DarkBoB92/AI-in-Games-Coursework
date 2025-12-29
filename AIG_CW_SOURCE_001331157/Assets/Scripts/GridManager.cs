using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public int width;
    public int height;
    public TMP_InputField widthField;
    public TMP_InputField heightField;
    public GameObject cell;

    public int[,] conwayCurrent;
    public int[,] conwayNext;
    public int[,] bobCurrent;
    public int[,] bobNext;
    public int[,] conwayStarting;
    public int[,] bobStarting;

    public GameObject[,] cells;
    public bool generated;

    public bool running;

    Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void OnMouseClicked(bool paintConway, bool paintBoB)
    {
        if (cells == null || conwayCurrent == null || bobCurrent == null)
        {
            return;
        }

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Cell"))
        {
            Vector3 hittedCell = hit.collider.transform.position;

            int x = Mathf.RoundToInt(hittedCell.x);
            int y = Mathf.RoundToInt(hittedCell.y);

            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            if (paintBoB)
            {
                bobCurrent[x, y] = 1 - bobCurrent[x, y];
            }

            if (paintConway)
            {
                conwayCurrent[x, y] = 1 - conwayCurrent[x, y];
            }

            UpdateRenderer();
        }
    }

    public void OnGenerateButtonPressed()
    {
        if(running)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(widthField.text.Trim()) && string.IsNullOrWhiteSpace(heightField.text.Trim()))
        {
            width = 10;
            height = 10;
        }
        else if (string.IsNullOrWhiteSpace(widthField.text.Trim()))
        {
            width = 10;
            height = int.Parse(heightField.text.Trim());
        }
        else if (string.IsNullOrWhiteSpace(heightField.text.Trim()))
        {
            width = int.Parse(widthField.text.Trim());
            height = 10;
        }
        else
        {
            width = int.Parse(widthField.text.Trim());
            height = int.Parse(heightField.text.Trim());
        }

        if (width > 0 && height > 0)
        {
            if (generated)
            {
                for (int x = 0; x < cells.GetLength(0); x++)
                {
                    for (int y = 0; y < cells.GetLength(1); y++)
                    {
                        if (cells[x, y] != null)
                        {
                            Destroy(cells[x, y]);
                        }
                    }
                }
                cells = null;
            }

            conwayCurrent = new int[width, height];
            conwayNext = new int[width, height];

            bobCurrent = new int[width, height];
            bobNext = new int[width, height];

            cells = new GameObject[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GameObject go = Instantiate(cell, new Vector3(x, y, 0), Quaternion.identity);
                    cells[x, y] = go;
                }
            }
        }
        generated = true;
        UpdateRenderer();
    }

    public void UpdateRenderer()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpriteRenderer sr = cells[x, y].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (bobCurrent[x, y] == 1)
                    {
                        sr.color = Color.red;
                    }
                    else if (conwayCurrent[x, y] == 1)
                    {
                        sr.color = Color.black;
                    }
                    else
                    {
                        sr.color = Color.white;
                    }
                }

            }
        }
    }   
}