using System.Collections;
using TMPro;
using UnityEngine;

public class BoBAutomata : MonoBehaviour
{
    public int[,] currentState;
    public int[,] nextState;
    public int[,] startingState;
    public int width;
    public int height;
    public GameObject cell;
    public GameObject[,] cells;
    public float cycleTime = 1;
    public bool active;
    public bool step;
    public bool run;
    Coroutine lifeCoroutine;
    [SerializeField] State state;
    public TMP_InputField widthField;
    public TMP_InputField heightField;
    public bool generated;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !run && !active)
        {
            OnMouseClicked();
        }
    }

    public IEnumerator LifeCycle()
    {
        while (active)
        {
            Vector2Int[] offsets = GetNeighbourOffsets();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int alive = currentState[x, y];
                    int sum = 0;                    

                    foreach (Vector2Int offset in offsets)
                    {
                        int neighborX = (x + offset.x) % width;
                        if (neighborX < 0) neighborX += width;

                        int neighborY = (y + offset.y) % height;
                        if (neighborY < 0) neighborY += height;

                        sum += currentState[neighborX, neighborY];
                    }

                    nextState[x, y] = CheckBirthSurvival(alive, sum);
                }
            }

            int[,] temporaryHolder = currentState;
            currentState = nextState;
            nextState = temporaryHolder;
            UpdateRenderer();
            yield return new WaitForSeconds(cycleTime);

            if (step)
            {
                step = false;
                active = false;
            }
        }
        lifeCoroutine = null;
    }

    void UpdateRenderer()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpriteRenderer sr = cells[x, y].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (currentState[x, y] == 1)
                    {
                        sr.color = Color.black;
                    }
                    else if (currentState[x, y] == 0)
                    {
                        sr.color = Color.white;
                    }
                }

            }
        }
    }

    int CheckBirthSurvival(int alive, int sum)
    {
        int nextState = 0;

        switch (state)
        {
            case State.Plus:
                if (alive == 0)
                {
                    if (sum == 2)
                    {
                        nextState = 1;
                    }
                    else
                    {
                        nextState = 0;
                    }
                }
                else
                {
                    if (sum == 1 || sum == 2)
                    {
                        nextState = 1;
                    }
                    else
                    {
                        nextState = 0;
                    }
                }
                return nextState;
            case State.Cross:
                if (alive == 0)
                {
                    if (sum == 1)
                    {
                        nextState = 1;
                    }
                    else
                    {
                        nextState = 0;
                    }
                }
                else
                {
                    if (sum == 1 || sum == 2)
                    {
                        nextState = 1;
                    }
                    else
                    {
                        nextState = 0;
                    }
                }
                return nextState;
            case State.Up2:
            case State.Right2:
            case State.Down2:
            case State.Left2:
                if (alive == 0)
                {
                    if (sum == 2 || sum == 3)
                    {
                        nextState = 1;
                    }
                    else
                    {
                        nextState = 0;
                    }
                }
                else
                {
                    if (sum >= 2 && sum <= 4)
                    {
                        nextState = 1;
                    }
                    else
                    {
                        nextState = 0;
                    }
                }
                return nextState;
            case State.Standard:
            default:
                if (alive == 0)
                {
                    if (sum >= 3 && sum <= 6)
                    {
                        nextState = 1;
                    }
                    else
                    {
                        nextState = 0;
                    }
                }
                else
                {
                    if (sum % 2 == 1)
                    {
                        nextState = 1;
                    }
                    else
                    {
                        nextState = 0;
                    }
                }
                return nextState;
        }
    }

    Vector2Int[] GetNeighbourOffsets()
    {
        switch (state)
        {
            case State.Plus: // up/down/left/right
                return new Vector2Int[]
                {
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                };

            case State.Cross: // diagonals only
                return new Vector2Int[]
                {
                new Vector2Int(-1, 1),
                new Vector2Int(1, 1),
                new Vector2Int(-1, -1),
                new Vector2Int(1, -1),
                };

            case State.Up2: // 2x3 blocks above
                return new Vector2Int[]
                {
                new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1),
                new Vector2Int(-1, 2), new Vector2Int(0, 2), new Vector2Int(1, 2),
                };

            case State.Down2: // 2x3 blocks below
                return new Vector2Int[]
                {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                new Vector2Int(-1, -2), new Vector2Int(0, -2), new Vector2Int(1, -2),
                };

            case State.Right2: // 2x3 blocks right
                return new Vector2Int[]
                {
                new Vector2Int(1, -1), new Vector2Int(1, 0), new Vector2Int(1, 1),
                new Vector2Int(2, -1), new Vector2Int(2, 0), new Vector2Int(2, 1),
                };

            case State.Left2: // 2x3 blocks left
                return new Vector2Int[]
                {
                new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1),
                new Vector2Int(-2, -1), new Vector2Int(-2, 0), new Vector2Int(-2, 1),
                };

            case State.Standard: // classic 
            default:
                return new Vector2Int[]
                {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                new Vector2Int(-1,  0),                      new Vector2Int(1,  0),
                new Vector2Int(-1,  1), new Vector2Int(0,  1), new Vector2Int(1,  1),
                };
        }
    }

    void OnMouseClicked()
    {
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

            currentState[x, y] = 1 - currentState[x, y];
            UpdateRenderer();
        }
    }

    public void OnGenerateButtonPressed()
    {
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

            currentState = new int[width, height];
            nextState = new int[width, height];

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
    }

    public void OnStepButtonPressed()
    {
        if (lifeCoroutine != null)
        {
            return;
        }

        step = true;
        run = false;
        active = true;
        lifeCoroutine = StartCoroutine(LifeCycle());
    }

    public void OnRunButtonPressed()
    {
        if (lifeCoroutine != null)
        {
            return;
        }

        startingState = CopyGrid(currentState);

        run = true;
        step = false;
        active = true;
        lifeCoroutine = StartCoroutine(LifeCycle());
    }

    public void OnStopButtonPressed()
    {
        step = false;
        run = false;
        active = false;

        if (lifeCoroutine != null)
        {
            StopCoroutine(lifeCoroutine);
            lifeCoroutine = null;
        }
    }

    public void OnSaveButtonPressed()
    {
        if (!run && !active)
        {
            startingState = CopyGrid(currentState);
        }
    }

    public void OnResetButtonPressed()
    {
        if (!run && !active)
        {
            if (startingState != null)
            {
                currentState = CopyGrid(startingState);
                UpdateRenderer();
            }
        }
        else
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SpriteRenderer sr = cells[x, y].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        if (currentState[x, y] == 1)
                        {
                            currentState[x, y] = 0;
                            sr.color = Color.white;
                        }
                    }

                }
            }
        }
    }

    public void OnClearButtonPressed()
    {
        if (!run && !active)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SpriteRenderer sr = cells[x, y].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        if (currentState[x, y] == 1)
                        {
                            currentState[x, y] = 0;
                            sr.color = Color.white;
                        }
                    }

                }
            }
        }
    }

    int[,] CopyGrid(int[,] source)
    {
        int[,] copy = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                copy[x, y] = source[x, y];
            }
        }

        return copy;
    }
}