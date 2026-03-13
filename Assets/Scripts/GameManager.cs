using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 50; //Ancho
    public int height = 30; //Alto
    public float updateTime = 0.1f; //Tiempo
    public GameObject cellPrefab; //Objeto prefabricado que se puede duplicar con las mismas propiedades en el unity
    public float cellNoSpawnChance = 0.95f; //Que no aparezcan celulas

    private bool[,] grid; //es un array de 2 direcciones, inicio y generacion si sesta en posicion
    private bool[,] nextGrid; //array de regilla
    private GameObject[,] cellObjects;
    private float timer;
    private bool isPaused = false;
   // public TMP_Text text;


    void Start()
    {
        grid = new bool[width, height]; //Se cea el grid
        nextGrid = new bool[width, height]; //se cra la regilla
        cellObjects = new GameObject[width, height]; //Se crea la celula

        //Asignacion de botones
        InputManager.Instance.OnPause += TogglePause;
        InputManager.Instance.OnRestart += RestartSimulation;
        InputManager.Instance.OnClear += ClearSimulation;
        InputManager.Instance.OnToggleCell += ToggleCellInput;
        InputManager.Instance.OnNext += OnStep;

        GenerateGrid();
        RandomizeGrid();
    }

    
    void OnStep()
    {
        Step();
        UpdateVisuals();
        timer = 0f;
    }
   
    void Update()
    {
        if (isPaused) return;

        
        timer += Time.deltaTime; 
        if (timer >= updateTime) //si el tiempo se mayor al asignado que ejecute los siguiente
        {
           Step(); 
           UpdateVisuals();
            timer = 0f; //reinicia el timer para que vuelva a contar desde 0 (bucle)
        }
        
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Debug.Log(isPaused ? "Simulación pausada" : "Simulación reanudada"); //es como un if
    }

    void ToggleCellInput()
    {
        // Si hay mouse disponible (PC), usar clic real
        if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            HandleMouseClick();
            return;
        }

        // Si no hay mouse, usar el centro de la cámara
        Vector3 camPos = Camera.main.transform.position;
        int x = Mathf.RoundToInt(camPos.x);
        int y = Mathf.RoundToInt(camPos.y);

        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        grid[x, y] = !grid[x, y];
        UpdateVisuals();
    }


    void ClearSimulation()
    {
        Debug.Log("Limpiando simulación...");
        ClearGrid();
        timer = 0f;
    }

    void RestartSimulation()
    {
        Debug.Log("Reiniciando simulación...");
        RandomizeGrid();
        timer = 0f;
    }

    void GenerateGrid()
    {

        for (int x = 0; x < width; x++) //itera en x
        {
            for (int y = 0; y < height; y++) //itera en y
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(x, y, 0), Quaternion.identity); //no se bloquea la rotacion
                cell.transform.parent = transform; 
                cellObjects[x, y] = cell; //se va rellenando el array visual con las celulas
            }
        }
    }

    public void ClearGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = false;
            }
        }
        UpdateVisuals();
    }

    void RandomizeGrid() //cada celula tiene un 5% de aparecer al incio
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = Random.value > cellNoSpawnChance; //Probabilidad de que no aparezcan celulas
            }
        }
        UpdateVisuals();
    }

    /*
    void Step()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
               
                int aliveNeighbors = CountAliveNeighbors(x, y); //vecinos vivos y pasa la posicion de la celula a la funcion
                bool alive = grid[x, y]; //toma la posicion en la que esta

                if (alive && (aliveNeighbors < 2 || aliveNeighbors > 3))
                    nextGrid[x, y] = false; // Muere
                else if (!alive && aliveNeighbors == 3)
                    nextGrid[x, y] = true;  // Nace
                else
                    nextGrid[x, y] = alive; // Se mantiene

            }
        }

        // Swap grids
        var temp = grid; //guarda la grilla actual de forma temporal
        grid = nextGrid;
        nextGrid = temp; 
    }
    */

    void Step()
    {

        // Limpiar nextGrid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                nextGrid[x, y] = false;
            }
        }

        // Recorrer desde abajo hacia arriba
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!grid[x, y]) continue; // si no hay arena, saltar

                int newX = x;
                int newY = y;

                // Caída vertical
                if (y > 0 && !grid[x, y - 1])
                {
                    newY = y - 1;
                    Debug.Log("Cae Normal");
                }
                else
                {
                    bool izquierdaVacio = (x > 0 && y > 0 && !grid[x - 1, y - 1]);
                    bool derechaVacio = (x < width - 1 && y > 0 && !grid[x + 1, y - 1]);

                    // Movimiento diagonal
                    if (izquierdaVacio && derechaVacio)
                    {
                        if (Random.value < 0.5f)
                        {
                            newX = x - 1;
                            newY = y - 1;
                            Debug.Log("Cae diagonal izquierda");
                        }
                        else
                        {
                            newX = x + 1;
                            newY = y - 1;
                            Debug.Log("Cae diagonal derecha");
                        }
                    }
                    else if (izquierdaVacio)
                    {
                        newX = x - 1;
                        newY = y - 1;
                        Debug.Log("Cae diagonal izquierda");
                    }
                    else if (derechaVacio)
                    {
                        newX = x + 1;
                        newY = y - 1;
                        Debug.Log("Cae diagonal derecha");
                    }
                }

                nextGrid[newX, newY] = true;
            }
        }

        // Intercambiar grids
        var temp = grid;
        grid = nextGrid;
        nextGrid = temp;

    }

    int CountAliveNeighbors(int x, int y) //limita a los vecinos de una celula si existen mas al lado de esta
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < width && ny >= 0 && ny < height) //ver si esta reviando una celula de la esquina, lo limita
                {
                    if (grid[nx, ny]) count++;
                }
            }
        }

        return count;
    }

    void HandleMouseClick()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        grid[x, y] = !grid[x, y];
        UpdateVisuals();
    }



    void UpdateVisuals() //actualiza como se a a mostrar la celula
    {
        //itera toda la grilla
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var rend = cellObjects[x, y].GetComponent<SpriteRenderer>(); 
                rend.color = grid[x, y] ? Color.black : Color.white; //cambia de color a negro si esta viva y blanco si esta muerta
            }
        }
       // text.text = "Generación: " + generation; 
    }
}
