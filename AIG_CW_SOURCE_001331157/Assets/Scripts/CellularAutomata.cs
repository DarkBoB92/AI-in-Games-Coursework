using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CellularAutomata : MonoBehaviour
{
    public int[,] currentState;
    public int[,] nextState;
    public int width;
    public int height;
    public GameObject cell;
    public GameObject[,] cells;
    public float cycleTime = 1;
    public bool active;
    public bool step;
    public bool run;
    Coroutine lifeCoroutine;

    private void Start()
    {
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

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseClicked();
        }
    }

    public virtual IEnumerator LifeCycle()
    {
        while (active)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int state = currentState[x, y];
                    int sum = 0;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (i == 0 && j == 0)
                            {
                                continue;
                            }

                            int neighborX = (x + i) % width;
                            if (neighborX < 0)
                            {
                                neighborX += width;
                            }

                            int neighborY = (y + j) % height;
                            if (neighborY < 0)
                            {
                                neighborY += height;
                            }

                            sum += currentState[neighborX, neighborY];
                        }
                    }

                    if (state == 0)
                    {
                        if (sum == 3)
                        {
                            nextState[x, y] = 1;
                        }
                        else
                        {
                            nextState[x, y] = 0;
                        }
                    }
                    else
                    {
                        if (sum == 2 || sum == 3)
                        {
                            nextState[x, y] = 1;
                        }
                        else
                        {
                            nextState[x, y] = 0;
                        }
                    }
                }
            }

            int[,] temporaryHolder = currentState;
            currentState = nextState;
            nextState = temporaryHolder;
            UpdateRenderer();
            yield return new WaitForSeconds(cycleTime);

            if(step)
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

    private void OnMouseClicked()
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
}