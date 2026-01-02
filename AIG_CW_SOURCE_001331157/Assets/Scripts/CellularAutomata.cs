using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Neighbourhood { Moore, Plus, Cross, Up2, Down2, Right2, Left2 }
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

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !run && !active)
        {
            if (grid != null && grid.conwayCurrent != null && grid.bobCurrent != null && grid.cells != null)
            {
                bool paintConway = (conwayRule != null && conwayRule.isOn);
                bool paintBoB = (bobRule != null && bobRule.isOn);

                if (!paintConway && !paintBoB)
                {
                    paintConway = true;
                }

                grid.OnMouseClicked(paintConway, paintBoB);
            }
        }
    }

    public IEnumerator LifeCycle()
    {
        while (active)
        {
            Vector2Int[] offsets = GetNeighbourOffsets();

            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    GetNeighbourSums(x, y, offsets, out int conwaySum, out int bobSum);

                    int conwayAlive = grid.conwayCurrent[x, y];
                    int bobAlive = grid.bobCurrent[x, y];

                    int conwayCandidate;
                    int bobCandidate;

                    bool runConway = (conwayRule != null && conwayRule.isOn);
                    bool runBoB = (bobRule != null && bobRule.isOn);
                    if (!runConway && !runBoB)
                    {
                        runConway = true;
                    }

                    if (runConway)
                    {
                        conwayCandidate = ConwayRule(conwayAlive, conwaySum);
                    }
                    else
                    {
                        conwayCandidate = conwayAlive;
                    }

                    if (runBoB)
                    {
                        bobCandidate = BoBRule(bobAlive, bobSum);
                    }
                    else
                    {
                        bobCandidate = bobAlive;
                    }

                    if (conwayCandidate == 1 && bobCandidate == 0)
                    {
                        grid.conwayNext[x, y] = 1;
                        grid.bobNext[x, y] = 0;
                    }
                    else if (conwayCandidate == 0 && bobCandidate == 1)
                    {
                        grid.conwayNext[x, y] = 0;
                        grid.bobNext[x, y] = 1;
                    }
                    else if (conwayCandidate == 1 && bobCandidate == 1)
                    {
                        if (conwaySum > bobSum)
                        {
                            grid.conwayNext[x, y] = 1;
                            grid.bobNext[x, y] = 0;
                        }
                        else if (bobSum > conwaySum)
                        {
                            grid.conwayNext[x, y] = 0;
                            grid.bobNext[x, y] = 1;
                        }
                        else
                        {
                            if (bobAlive == 1)
                            {
                                grid.conwayNext[x, y] = 0;
                                grid.bobNext[x, y] = 1;
                            }
                            else if (conwayAlive == 1)
                            {
                                grid.conwayNext[x, y] = 1;
                                grid.bobNext[x, y] = 0;
                            }
                            else
                            {
                                grid.conwayNext[x, y] = 0;
                                grid.bobNext[x, y] = 0;
                            }
                        }
                    }
                    else
                    {
                        grid.conwayNext[x, y] = 0;
                        grid.bobNext[x, y] = 0;
                    }
                }
            }

            int[,] tempConway = grid.conwayCurrent;
            grid.conwayCurrent = grid.conwayNext;
            grid.conwayNext = tempConway;

            int[,] tempBoB = grid.bobCurrent;
            grid.bobCurrent = grid.bobNext;
            grid.bobNext = tempBoB;

            grid.UpdateRenderer();
            yield return new WaitForSeconds(cycleTime);

            if(step)
            {
                step = false;
                active = false;
                grid.running = false;
            }
        }
        lifeCoroutine = null;
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
            case Neighbourhood.Down2:
            case Neighbourhood.Right2:
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
                    if (sum % 2 != 0)
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
                    new Vector2Int(-1, 0),         /*Cell*/       new Vector2Int(1, 0),
                                           new Vector2Int(0, -1),
                };

            case Neighbourhood.Cross: // Cross Neighborhood
                return new Vector2Int[]
                {
                    new Vector2Int(-1, 1),                    new Vector2Int(1, 1),
                                                 /*Cell*/
                    new Vector2Int(-1, -1),                    new Vector2Int(1, -1),
                };

            case Neighbourhood.Up2: // 2x3 Blocks Above Neighborhood
                return new Vector2Int[]
                {
                    new Vector2Int(-1, 2), new Vector2Int(0, 2), new Vector2Int(1, 2),
                    new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1),
                                                   /*Cell*/
                };

            case Neighbourhood.Down2: // 2x3 Blocks Below Neighborhood
                return new Vector2Int[]
                {
                                                   /*Cell*/
                    new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                    new Vector2Int(-1, -2), new Vector2Int(0, -2), new Vector2Int(1, -2),
                };

            case Neighbourhood.Right2: // 2x3 Blocks Right Neighborhood
                return new Vector2Int[]
                {
                            new Vector2Int(1, 1), new Vector2Int(2, 1),
                    /*Cell*/new Vector2Int(1, 0), new Vector2Int(2, 0),
                            new Vector2Int(1, -1), new Vector2Int(2, -1)
                };

            case Neighbourhood.Left2: // 2x3 Blocks Left Neighborhood
                return new Vector2Int[]
                {
                      new Vector2Int(-2, 1), new Vector2Int(-1, 1),
                      new Vector2Int(-2, 0), new Vector2Int(-1, 0),/*Cell*/
                    new Vector2Int(-2, -1), new Vector2Int(-1, -1),
                };

            case Neighbourhood.Moore: // Moore Neighborhood
            default:
                return new Vector2Int[]
                {
                    new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                    new Vector2Int(-1,  0),        /*Cell*/        new Vector2Int(1,  0),
                    new Vector2Int(-1,  1), new Vector2Int(0,  1), new Vector2Int(1,  1),
                };
        }
    }

    void GetNeighbourSums(int x, int y, Vector2Int[] offsets, out int conwaySum, out int bobSum)
    {
        conwaySum = 0;
        bobSum = 0;

        foreach (Vector2Int offset in offsets)
        {
            int nx = (x + offset.x) % grid.width;
            if (nx < 0) nx += grid.width;

            int ny = (y + offset.y) % grid.height;
            if (ny < 0) ny += grid.height;

            conwaySum += grid.conwayCurrent[nx, ny];
            bobSum += grid.bobCurrent[nx, ny];
        }
    }

    public void OnStepButtonPressed()
    {
        if (grid == null || grid.conwayCurrent == null || grid.bobCurrent == null || grid.cells == null)
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
        grid.running = true;
        lifeCoroutine = StartCoroutine(LifeCycle());
    }

    public void OnRunButtonPressed()
    {
        if (grid == null || grid.conwayCurrent == null || grid.bobCurrent == null || grid.cells == null)
        {
            return;
        }

        if (lifeCoroutine != null)
        {
            return;
        }

        grid.conwayStarting = CopyGrid(grid.conwayCurrent);
        grid.bobStarting = CopyGrid(grid.bobCurrent);

        run = true;
        step = false;
        active = true;
        grid.running = true;
        lifeCoroutine = StartCoroutine(LifeCycle());
    }

    public void OnStopButtonPressed()
    {
        step = false;
        run = false;
        active = false;
        grid.running = false;

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
            grid.conwayStarting = CopyGrid(grid.conwayCurrent);
            grid.bobStarting = CopyGrid(grid.bobCurrent);
        }
    }

    public void OnResetButtonPressed()
    {
        if (!run && !active)
        {
            if (grid.conwayStarting != null && grid.bobStarting != null)
            {
                grid.conwayCurrent = CopyGrid(grid.conwayStarting);
                grid.bobCurrent = CopyGrid(grid.bobStarting);
                grid.UpdateRenderer();
            }
        }
        else
        {
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    grid.conwayCurrent[x, y] = 0;
                    grid.bobCurrent[x, y] = 0;
                }
            }
            grid.UpdateRenderer();
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
                    grid.conwayCurrent[x, y] = 0;
                    grid.bobCurrent[x, y] = 0;
                }
            }
            grid.UpdateRenderer();
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
}