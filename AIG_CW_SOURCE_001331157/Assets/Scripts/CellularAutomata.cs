using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Neighbourhood { Moore, Plus, Cross, Up2, Right2, Left2, Down2 }
public enum RuleSet { Conway, BoB }

public class CellularAutomata : MonoBehaviour
{
    public GridManager grid;

    public float cycleTime = 1;
 
    public bool active;
    public bool step;
    public bool run;
    
    Coroutine lifeCoroutine;
    
    public Toggle conwayRule;
    public Toggle bobRule;
    public TMP_Dropdown neighbourhood;
    [SerializeField] Neighbourhood neighbour
    {
        get
        {
            if (neighbourhood == null)
            {
                return Neighbourhood.Moore;
            }

            return (Neighbourhood)neighbourhood.value;
        }
    }
    [SerializeField] RuleSet ruleSet
    {
        get
        {
            if (bobRule != null && bobRule.isOn)
            { 
                return RuleSet.BoB;
            }

            return RuleSet.Conway;
        }
    }

    private void Awake()
    {
        grid.renderColour = SetRenderColour(ruleSet);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !run && !active)
        {
            if (grid != null && grid.currentState != null && grid.nextState != null && grid.cells != null)
            {
                grid.OnMouseClicked();
            }
        }
    }

    public IEnumerator LifeCycle()
    {
        grid.renderColour = SetRenderColour(ruleSet);

        while (active)
        {
            Vector2Int[] offsets = GetNeighbourOffsets();

            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    int alive = grid.currentState[x, y];
                    int sum = 0;

                    foreach (Vector2Int offset in offsets)
                    {
                        int neighborX = (x + offset.x) % grid.width;
                        if (neighborX < 0) neighborX += grid.width;

                        int neighborY = (y + offset.y) % grid.height;
                        if (neighborY < 0) neighborY += grid.height;

                        sum += grid.currentState[neighborX, neighborY];
                    }

                    NextGeneration(x, y, alive, sum);
                }
            }

            int[,] temporaryHolder = grid.currentState;
            grid.currentState = grid.nextState;
            grid.nextState = temporaryHolder;
            grid.UpdateRenderer();
            yield return new WaitForSeconds(cycleTime);

            if(step)
            {
                step = false;
                active = false;
            }
        }
        lifeCoroutine = null;
    }

    public void NextGeneration(int x, int y, int alive, int sum)
    {
        switch (ruleSet)
        {
            case RuleSet.Conway:
                grid.nextState[x, y] = ConwayRule(alive, sum);
                break;
            case RuleSet.BoB:
                grid.nextState[x, y] = BoBRule(alive, sum);
                break;
        }
    }

    int ConwayRule(int alive, int sum)
    {
        if (alive == 0)
        {
            if (sum == 3)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            if (sum == 2 || sum == 3)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    int BoBRule(int alive, int sum)
    {
        switch (neighbour)
        {
            case Neighbourhood.Plus:
                if (alive == 0)
                {
                    if (sum == 2)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    if (sum == 1 || sum == 2)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            case Neighbourhood.Cross:
                if (alive == 0)
                {
                    if (sum == 1)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    if (sum == 1 || sum == 2)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            case Neighbourhood.Up2:
            case Neighbourhood.Right2:
            case Neighbourhood.Down2:
            case Neighbourhood.Left2:
                if (alive == 0)
                {
                    if (sum == 2 || sum == 3)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    if (sum >= 2 && sum <= 4)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            case Neighbourhood.Moore:
            default:
                if (alive == 0)
                {
                    if (sum >= 3 && sum <= 6)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    if (sum % 2 == 1)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
        }
    }

    Vector2Int[] GetNeighbourOffsets()
    {
        switch (neighbour)
        {
            case Neighbourhood.Plus: // Plus Neighborhood 
                return new Vector2Int[]
                {
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                };

            case Neighbourhood.Cross: // Cross Neighborhood
                return new Vector2Int[]
                {
                new Vector2Int(-1, 1),
                new Vector2Int(1, 1),
                new Vector2Int(-1, -1),
                new Vector2Int(1, -1),
                };

            case Neighbourhood.Up2: // 2x3 Blocks Above Neighborhood
                return new Vector2Int[]
                {
                new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1),
                new Vector2Int(-1, 2), new Vector2Int(0, 2), new Vector2Int(1, 2),
                };

            case Neighbourhood.Down2: // 2x3 Blocks Below Neighborhood
                return new Vector2Int[]
                {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                new Vector2Int(-1, -2), new Vector2Int(0, -2), new Vector2Int(1, -2),
                };

            case Neighbourhood.Right2: // 2x3 Blocks Right Neighborhood
                return new Vector2Int[]
                {
                new Vector2Int(1, -1), new Vector2Int(1, 0), new Vector2Int(1, 1),
                new Vector2Int(2, -1), new Vector2Int(2, 0), new Vector2Int(2, 1),
                };

            case Neighbourhood.Left2: // 2x3 Blocks Left Neighborhood
                return new Vector2Int[]
                {
                new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1),
                new Vector2Int(-2, -1), new Vector2Int(-2, 0), new Vector2Int(-2, 1),
                };

            case Neighbourhood.Moore: // Moore Neighborhood
            default:
                return new Vector2Int[]
                {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                new Vector2Int(-1,  0),                      new Vector2Int(1,  0),
                new Vector2Int(-1,  1), new Vector2Int(0,  1), new Vector2Int(1,  1),
                };
        }
    }

    public void OnStepButtonPressed()
    {
        if (grid == null || grid.currentState == null || grid.nextState == null || grid.cells == null)
        {
            return;
        }

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
        if (grid == null || grid.currentState == null || grid.nextState == null || grid.cells == null)
        {
            return;
        }

        if (lifeCoroutine != null)
        {
            return;
        }

        grid.startingState = CopyGrid(grid.currentState);

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
            grid.startingState = CopyGrid(grid.currentState);
        }
    }

    public void OnResetButtonPressed()
    {
        if (!run && !active)
        {
            if (grid.startingState != null)
            {
                grid.currentState = CopyGrid(grid.startingState);
                grid.UpdateRenderer();
            }
        }
        else
        {
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    SpriteRenderer sr = grid.cells[x, y].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        if (grid.currentState[x, y] == 1)
                        {
                            grid.currentState[x, y] = 0;
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
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    SpriteRenderer sr = grid.cells[x, y].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        if (grid.currentState[x, y] == 1)
                        {
                            grid.currentState[x, y] = 0;
                            sr.color = Color.white;
                        }
                    }

                }
            }
        }
    }

    int[,] CopyGrid(int[,] source)
    {
        int[,] copy = new int[grid.width, grid.height];

        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                copy[x, y] = source[x, y];
            }
        }

        return copy;
    }

    public static Color SetRenderColour(RuleSet ruleSet)
    {
        switch (ruleSet)
        {
            case RuleSet.Conway:
            default:
                return Color.black;
            case RuleSet.BoB:
                return Color.red;
        }
    }
}