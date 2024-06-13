using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class QTable
{
    // Estructuras simples, almacen de estado
    struct QState
    {
        //Movimientos disponibles, distancia y orientacion respecto el enemigo
        public bool up, right, down, left;
        public int distance, orientation;

        public QState(bool up = false, bool right = false, bool down = false, bool left = false, int distance = 0, int orientation = 0)
        {
            this.up = up;
            this.right = right;
            this.down = down;
            this.left = left;
            this.distance = distance;
            this.orientation = orientation;
        }
    }

    #region Variables
    // Parametros y recurrentes
    private readonly QMindTrainerParams _params;
    private readonly INavigationAlgorithm _navigationAlgorithm;
    private readonly System.Random _random = new System.Random();
    // Tabla de valores
    private Dictionary<QState, float[]> qTable = new Dictionary<QState, float[]>();
    private QState _previousState;
    // Segmentacion del mundo
    private float _distances;
    private const int _distanceSegments = 3;
    private const int _angleSegments = 8;
    // Recompensas
    private const int _negativeReward = -100;
    private const int _positiveReward = 5;
    #endregion

    // Constructor
    public QTable(QMindTrainerParams qMindTrainerParams, Vector2Int worldSize, INavigationAlgorithm navigationAlgorithm)
    {
        // Parametros de ejecucion
        _params = qMindTrainerParams;
        // Casillas promedio del mundo y su porcion entre 4
        _distances = (worldSize[0] + worldSize[1]) / 2 / 4;
        // Navegacion por el mundo entre posiciones
        _navigationAlgorithm = navigationAlgorithm;
    }

    internal void DoStep(CellInfo agentPosition, CellInfo otherPosition)
    {

    }
    /// <summary>
    /// Actualiza el valor de la tabla teniendo en cuenta la decision tomada y el estado.
    /// </summary>
    /// <param name="agentPosition"></param>
    /// <param name="otherPosition"></param>
    internal void UpdateWithReward(CellInfo agentPosition, CellInfo otherPosition)
    {

    }

    /// <summary>
    /// Inicializa la tabla de valores.
    /// Si ya hay un archivo, se carga el archivo y se lee.
    /// En caso contrario, se genera una tabla nueva.
    /// </summary>
    internal void Load(string name = "QTable")
    {
        if (File.Exists(name + ".csv")) Read(); else Init();
    }
    /// <summary>
    /// Se inicializa una tabla con los estados inicializados y los array de valores vacios, es decir, 0.0f.
    /// </summary>
    internal void Init()
    {
        const int filas = 16, columnas = 4;
        bool[,] states = new bool[filas, columnas];
        // Inicializacion de los estados
        for (byte i = 0; i < filas; i++)
        {
            for (byte j = 0; j < columnas; j++)
            {
                states[i, j] = (i & (1 << j)) != 0;
            }
        }
        for (int option = 0; option < filas; option++)
        {
            for (int distance = 0; distance < _distanceSegments; distance++)
            {
                for (int orientation = 0; orientation < _angleSegments; orientation++)
                {
                    // Inicializamos los valores Q a un valor por defecto 0.0f.
                    qTable[new QState(states[option, 0], states[option, 1], states[option, 2], states[option, 3], distance, orientation)] = new float[4];
                }
            }
        }
        Debug.Log("QTable initialized.");
    }
    /// Guarda la tabla de valores en un fichero CSV.
    /// </summary>
    internal void Save(string name = "QTable")
    {
        StreamWriter file = File.CreateText(name + ".csv");
        string initialText = "";
        file.WriteLine(initialText);

        string keyValue;
        foreach (var keys in qTable.Keys)
        {
            // Claves
            keyValue = keys.up + ";" + keys.right + ";" + keys.down + ";" + keys.left + ";" + keys.distance + ";" + keys.orientation;
            // Valores
            float[] dataCollection = qTable[keys];
            for (var data = 0; data < dataCollection.Length; data++)
            {
                keyValue += ";" + dataCollection[data].ToString();
            }
            file.WriteLine(keyValue); // Lo mismo que cuando escribimos por consola
        }
        file.Close(); // Al cerrar el fichero nos aseguramos que no queda ningun dato por guardar

        //Debug.Log($"QTable saved. DataPath: {Application.dataPath}/Scripts/GrupoA/");
        Debug.Log($"QTable saved. Directory: '{Directory.GetCurrentDirectory()}'");
    }
    /// <summary>
    /// Lee el fichero CSV y lo carga en la tabla de valores.
    /// </summary>
    internal void Read(string name = "QTable")
    {

        ///

        if (File.Exists(name + ".csv"))
        {
            StreamReader file = File.OpenText(name + ".csv");
            file.ReadLine();
            string keyValue;
            string[] data;
            CultureInfo culture = new CultureInfo("es-ES");

            do
            {
                keyValue = file.ReadLine();
                data = keyValue.Split(';');
                QState temporalState = new QState(bool.Parse(data[0]), bool.Parse(data[1]), bool.Parse(data[2]), bool.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]));
                List<float> dataCollection = new List<float>
            {
                float.Parse(data[6], culture),
                float.Parse(data[7], culture),
                float.Parse(data[8], culture),
                float.Parse(data[9], culture)
            };
                qTable[temporalState] = dataCollection.ToArray();
            } while (data != null);
            file.Close(); // Al cerrar el fichero nos aseguramos que no queda ning�n dato por guardar

            Debug.Log("QTable loaded");
        }
        else
        {
            Debug.Log("Error en la lectura de la tabla");
        }

        ///

    }

}
