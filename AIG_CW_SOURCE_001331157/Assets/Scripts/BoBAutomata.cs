using System.Collections;
using System.Collections.Generic;
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

    // Tables: inputBits -> outputBits (packed int)
    private Dictionary<int, int> plusTable;
    private Dictionary<int, int> crossTable;

    private Dictionary<int, int> up2Table;
    private Dictionary<int, int> right2Table;
    private Dictionary<int, int> down2Table;
    private Dictionary<int, int> left2Table;

    private void Awake()
    {
        BuildAllTables();
    }

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
            // Clear next state
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    nextState[x, y] = 0;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Standard stays as Conway
                    if (state == State.Standard)
                    {
                        ApplyConwayAt(x, y);
                        continue;
                    }

                    // BoB pattern rules
                    Vector2Int[] orderOffsets = GetBitOrderOffsets(state);
                    int key = ReadPatternKey(x, y, orderOffsets);

                    Dictionary<int, int> table = GetTableForState(state);
                    if (table == null)
                        continue;

                    if (!table.TryGetValue(key, out int outputBits))
                    {
                        // Unspecified pattern: treat as all 0 output
                        continue;
                    }

                    WritePatternOutput(x, y, orderOffsets, outputBits);
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

    // Conway
    private void ApplyConwayAt(int x, int y)
    {
        int alive = currentState[x, y];
        int sum = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                sum += ReadCellWrapped(x + dx, y + dy);
            }
        }

        int next;
        if (alive == 0)
        {
            if (sum == 3)
            {
                next = 1;
            }
            else
            {
                next = 0;
            }
        }
        else
        {
            if (sum == 2 || sum == 3)
            {
                next = 1;
            }
            else
            {
                next = 0;
            }
        }

        nextState[x, y] = Mathf.Max(nextState[x, y], next);
    }

    // Pattern
    private int ReadPatternKey(int x, int y, Vector2Int[] offsets)
    {
        int bits = 0;
        for (int i = 0; i < offsets.Length; i++)
        {
            int b = ReadCellWrapped(x + offsets[i].x, y + offsets[i].y);
            bits = (bits << 1) | (b & 1);
        }
        return bits;
    }

    private void WritePatternOutput(int x, int y, Vector2Int[] offsets, int outputBits)
    {
        int n = offsets.Length;

        for (int i = 0; i < n; i++)
        {
            int shift = (n - 1 - i);
            int bit = (outputBits >> shift) & 1;

            if (bit == 0)
            {
                continue;
            }

            int wx = WrapX(x + offsets[i].x);
            int wy = WrapY(y + offsets[i].y);

            nextState[wx, wy] = 1;
        }
    }

    private int ReadCellWrapped(int x, int y)
    {
        return currentState[WrapX(x), WrapY(y)];
    }

    private int WrapX(int x)
    {
        x %= width;
        if (x < 0) x += width;
        return x;
    }

    private int WrapY(int y)
    {
        y %= height;
        if (y < 0) y += height;
        return y;
    }

    //Offsets
    private Vector2Int[] GetBitOrderOffsets(State s)
    {
        switch (s)
        {
            case State.Plus:
                // Up, Left, BoB, Right, Down
                return new Vector2Int[]
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(-1, 0),
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, -1),
                };

            case State.Cross:
                // TopLeft, TopRight, BoB, BottomLeft, BottomRight
                return new Vector2Int[]
                {
                    new Vector2Int(-1, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(0, 0),
                    new Vector2Int(-1, -1),
                    new Vector2Int(1, -1),
                };

            case State.Up2:
                // Top2L, Top2C, Top2R, TopL, TopC, TopR, BoB
                return new Vector2Int[]
                {
                    new Vector2Int(-1, 2),
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 2),
                    new Vector2Int(-1, 1),
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(0, 0),
                };

            case State.Right2:
                // Right2Top, Right2Center, Right2Bottom, RightTop, RightCenter, RightBottom, BoB
                return new Vector2Int[]
                {
                    new Vector2Int(2, 1),
                    new Vector2Int(2, 0),
                    new Vector2Int(2, -1),
                    new Vector2Int(1, 1),
                    new Vector2Int(1, 0),
                    new Vector2Int(1, -1),
                    new Vector2Int(0, 0),
                };

            case State.Down2:
                // Bottom2L, Bottom2C, Bottom2R, BottomL, BottomC, BottomR, BoB
                return new Vector2Int[]
                {
                    new Vector2Int(-1, -2),
                    new Vector2Int(0, -2),
                    new Vector2Int(1, -2),
                    new Vector2Int(-1, -1),
                    new Vector2Int(0, -1),
                    new Vector2Int(1, -1),
                    new Vector2Int(0, 0),
                };

            case State.Left2:
                // Left2Top, Left2Center, Left2Bottom, LeftTop, LeftCenter, LeftBottom, BoB
                return new Vector2Int[]
                {
                    new Vector2Int(-2, 1),
                    new Vector2Int(-2, 0),
                    new Vector2Int(-2, -1),
                    new Vector2Int(-1, 1),
                    new Vector2Int(-1, 0),
                    new Vector2Int(-1, -1),
                    new Vector2Int(0, 0),
                };

            default:
                return new Vector2Int[] { new Vector2Int(0, 0) };
        }
    }

    private Dictionary<int, int> GetTableForState(State s)
    {
        switch (s)
        {
            case State.Plus:
                return plusTable;
            case State.Cross: 
                return crossTable;
            case State.Up2: 
                return up2Table;
            case State.Right2: 
                return right2Table;
            case State.Down2: 
                return down2Table;
            case State.Left2: 
                return left2Table;
            default: 
                return null;
        }
    }

    //Build Tables
    private void BuildAllTables()
    {
        // PLUS (5-bit)
        plusTable = BuildTable(
            bitCount: 5,
            inputs: new string[]
            {
                "00000","10000","01000","00100","00010","00001","11000","10100","10010","10001","01100","01010","01001","00110","00101","00011",
                "11100","11010","11001","10110","10101","10011","01110","01101","01011","00111","11110","11101","11011","10111","01111","11111"
            },
            outputs: new string[]
            {
                "00000","10100","01100","00000","00110","00001","11100","10100","10110","10101","01100","01110","01101","00110","00101","00111",
                "11100","11010","11001","10110","11011","10011","11011","01101","01011","00111","11011","11011","00100","11011","11011","00000"
            }
        );

        // CROSS (5-bit)
        crossTable = BuildTable(
            bitCount: 5,
            inputs: new string[]
            {
                "00000","10000","01000","00100","00010","00001","11000","10100","10010","10001","01100","01010","01001","00110","00101","00011",
                "11100","11010","11001","10110","10101","10011","01110","01101","01011","00111","11110","11101","11011","10111","01111","11111"
            },
            outputs: new string[]
            {
                "00100","10101","01110","11011","01110","10101","00100","10101","00100","10101","01110","01110","00100","01110","10101","00100",
                "11111","11011","11011","11111","00000","11111","00000","00000","00100","11111","00000","00000","00100","00000","00000","00100"
            }
        );

        PasteTables();
    }

    // --- YOU PASTE YOUR 128-LINE OUTPUT ARRAYS HERE (keeps your data intact) ---
    private void PasteTables()
    {
        // UP2
        up2Table = BuildTable(7, BuildAllBinaryInputs(7), UP2_OUTPUTS_128);

        // RIGHT2
        right2Table = BuildTable(7, BuildAllBinaryInputs(7), RIGHT2_OUTPUTS_128);

        // DOWN2
        down2Table = BuildTable(7, BuildAllBinaryInputs(7), DOWN2_OUTPUTS_128);

        // LEFT2
        left2Table = BuildTable(7, BuildAllBinaryInputs(7), LEFT2_OUTPUTS_128);
    }

    private static readonly string[] UP2_OUTPUTS_128 = new string[]
    {
        "0000000","1101011","0010110","0100010","1010100","0010011","0110111","1011111",
        "0100000","0100101","0101011","1010011","0010100","0101101","1011110","1011111",
        "0010000","0100011","0110011","1010011","0011100","1010111","1011110","1011111",
        "0011000","0110011","1011011","1011111","1011100","1011111","1011111","1011111",
        "0100000","0100101","0010110","1010011","1010100","1010111","1011110","1011111",
        "0101000","0100101","1011011","1011111","0011100","1011111","1011111","1011111",
        "0110000","1010011","1011011","1011111","1011100","1011111","1011111","1011111",
        "1011000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "1000000","0100001","0010010","1010011","0010100","1010111","1011110","1011111",
        "0101000","1010011","1011011","1011111","1011100","1011111","1011111","1011111",
        "0110000","1010011","1011011","1011111","1011100","1011111","1011111","1011111",
        "1011000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "1100000","1010011","1011011","1011111","1011100","1011111","1011111","1011111",
        "1011000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "1110000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "1011111","1011111","1011111","1011111","1011111","1011111","1011111","1011111"
    };

    private static readonly string[] RIGHT2_OUTPUTS_128 = new string[]
    {
        "0000000","0010000","0100100","0101001","0010100","1010011","1010110","1011111",
        "0100000","0010011","1010011","1011011","0011100","1011111","1011111","1011111",
        "0010000","1010011","1011011","1011111","1011100","1011111","1011111","1011111",
        "1011000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "0100000","0010011","1010011","1011011","0011100","1011111","1011111","1011111",
        "0101000","1010011","1011111","1011111","1011111","1011111","1011111","1011111",
        "0110000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "1011000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "1000000","0010011","1010011","1011011","0011100","1011111","1011111","1011111",
        "0101000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "0110000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "1011000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "1100000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "1011111","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "1110000","1110001","1110010","1110011","1110100","1110101","1110110","1110111",
        "1111000","1111001","1111010","1111011","1111100","1111101","1111110","1111111"
    };


    private static readonly string[] DOWN2_OUTPUTS_128 = new string[]
    {
        "0000000","0000000","0000010","0000011","0000100","0000101","0010110","0010111",
        "0001000","0001001","0010010","0010011","0011100","0011101","0011110","0011111",
        "0010000","0010001","0010010","0010011","0010100","0010101","0010110","0010111",
        "0011000","0011001","0011010","0011011","0011100","0011101","0011110","0011111",
        "0100000","0100001","0100010","0100011","0100100","0100101","0100110","0100111",
        "0101000","0101001","0101010","0101011","0101100","0101101","0101110","0101111",
        "0110000","0110001","0110010","0110011","0110100","0110101","0110110","0110111",
        "0111000","0111001","0111010","0111011","0111100","0111101","0111110","0111111",
        "1000000","1000001","1000010","1000011","1000100","1000101","1000110","1000111",
        "1001000","1001001","1001010","1001011","1001100","1001101","1001110","1001111",
        "1010000","1010001","1010010","1010011","1010100","1010101","1010110","1010111",
        "1011000","1011001","1011010","1011011","1011100","1011101","1011110","1011111",
        "1100000","1100001","1100010","1100011","1100100","1100101","1100110","1100111",
        "1101000","1101001","1101010","1101011","1101100","1101101","1101110","1101111",
        "1110000","1110001","1110010","1110011","1110100","1110101","1110110","1110111",
        "1111000","1111001","1111010","1111011","1111100","1111101","1111110","1111111"
    };


    private static readonly string[] LEFT2_OUTPUTS_128 = new string[]
    {
        "0100100","1101011","0010110","1110011","1010100","1110011","0110111","1111111",
        "0100000","1100101","1111011","1111111","0011100","1111111","1111111","1111111",
        "0010000","1110011","1111011","1111111","1011100","1111111","1111111","1111111",
        "1011000","1111111","1111111","1111111","1111111","1111111","1111111","1111111",
        "0100000","1100101","1110011","1111011","1011100","1111111","1111111","1111111",
        "0101000","1110011","1111111","1111111","1111111","1111111","1111111","1111111",
        "0110000","1111111","1111111","1111111","1111111","1111111","1111111","1111111",
        "1011000","1111111","1111111","1111111","1111111","1111111","1111111","1111111",
        "1000000","1100101","1110011","1111011","1011100","1111111","1111111","1111111",
        "0101000","1111111","1111111","1111111","1111111","1111111","1111111","1111111",
        "0110000","1111111","1111111","1111111","1111111","1111111","1111111","1111111",
        "1011000","1111111","1111111","1111111","1111111","1111111","1111111","1111111",
        "1100000","1111111","1111111","1111111","1111111","1111111","1111111","1111111",
        "1111111","1111111","1111111","1111111","1111111","1111111","1111111","1111111",
        "1110000","1011111","1011111","1011111","1011111","1011111","1011111","1011111",
        "1011111","1011111","1011111","1011111","1011111","1011111","1011111","1011111"
    };

    private Dictionary<int, int> BuildTable(int bitCount, string[] inputs, string[] outputs)
    {
        Dictionary<int, int> table = new Dictionary<int, int>(inputs.Length);

        int n = Mathf.Min(inputs.Length, outputs.Length);
        
        for (int i = 0; i < n; i++)
        {
            int key = System.Convert.ToInt32(inputs[i], 2);
            int val = System.Convert.ToInt32(outputs[i], 2);
            table[key] = val;
        }

        return table;
    }

    private string[] BuildAllBinaryInputs(int bitCount)
    {
        int count = 1 << bitCount;
        string[] arr = new string[count];
        
        for (int i = 0; i < count; i++)
        {
            arr[i] = System.Convert.ToString(i, 2).PadLeft(bitCount, '0');
        }
        
        return arr;
    }

    //Rendering
    void UpdateRenderer()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpriteRenderer sr = cells[x, y].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = (currentState[x, y] == 1) ? Color.red : Color.white;
                }
            }
        }
    }

    //Input
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

    //UI / Buttons
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
            if (generated && cells != null)
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
        UpdateRenderer();
    }

    public void OnStepButtonPressed()
    {
        if (currentState == null || nextState == null || cells == null)
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
        if (currentState == null || nextState == null || cells == null)
        {
            return;
        }

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
                    currentState[x, y] = 0;
                }
            }

            UpdateRenderer();
        }
    }

    public void OnClearButtonPressed()
    {
        if (!run && !active)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    currentState[x, y] = 0;

            UpdateRenderer();
        }
    }

    int[,] CopyGrid(int[,] source)
    {
        int[,] copy = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                copy[x, y] = source[x, y];
        return copy;
    }
}