using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class QTable
{
    /// <summary>
    /// Almacen de estado durante el trancurso y el uso de la tabla.
    /// </summary>
    public class QState
    {
        //Movimientos disponibles, distancia y orientacion respecto el enemigo
        public bool up, right, down, left;
        public int distance, orientation;
        /// <summary>
        /// Constructor base del estado.
        /// </summary>
        /// <param name="up"></param>
        /// <param name="right"></param>
        /// <param name="down"></param>
        /// <param name="left"></param>
        /// <param name="distance"></param>
        /// <param name="orientation"></param>
        public QState(bool up = false, bool right = false, bool down = false, bool left = false, int distance = 0, int orientation = 0)
        {
            this.up = up;
            this.right = right;
            this.down = down;
            this.left = left;
            this.distance = distance;
            this.orientation = orientation;
        }
        #region Comparadores
        /// <summary>
        /// Metodo de comparacion por los valores guardados.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True si ambos objetos son iguales.</returns>
        public override bool Equals(object obj)
        {
            QState other = obj as QState;
            if (other.up == up && other.down == down && other.right == right && other.left == left &&
                other.distance == distance && other.orientation == orientation) return true;
            return false;
        }
        /// <summary>
        /// Metodo de comparacion mediante codigos hash.
        /// </summary>
        /// <returns>Número entero que representa el identificador hash.</returns>
        public override int GetHashCode()
        {
            // Valores enteros al azar
            int hash1 = 17, hash2 = 31;
            int hash = hash1;
            hash = hash * hash2 + up.GetHashCode();
            hash = hash * hash2 + down.GetHashCode();
            hash = hash * hash2 + right.GetHashCode();
            hash = hash * hash2 + left.GetHashCode();
            hash = hash * hash2 + distance.GetHashCode();
            hash = hash * hash2 + orientation.GetHashCode();
            return hash;
        }
        #endregion
    }

    #region Variables
    // Parametros y recurrentes
    private readonly QMindTrainerParams _params;
    private readonly INavigationAlgorithm _navigationAlgorithm;
    private readonly System.Random _random = new System.Random();
    private readonly WorldInfo _worldInfo;
    private readonly string path = Application.dataPath + "/scripts/GrupoA/";
    private Action<CellInfo> updateAgent;
    private Action<CellInfo> updateOther;

    // Tabla de valores
    private Dictionary<QState, float[]> qTable = new Dictionary<QState, float[]>();
    // Estado parcial
    private QState _previousState;
    private CellInfo north, east, south, west;
    private int _previousDistance = 0;
    private int _previousAction = 0;
    // Segmentacion del mundo
    private float _distances;
    private const int _distanceSegments = 3;
    private const int _angleSegments = 8;
    // Recompensas
    private const int _negativeReward = -100;   // Pierde
    private const int _lowReward = 1;           // Sigue vivo pero se acerca
    private const int _hightReward = 5;         // Mantiene distancia
    private const int _positiveReward = 30;     // Se aleja
    #endregion

    /// <summary>
    /// Constructor del la tabla de valores y del resto de elementos base
    /// </summary>
    /// <param name="qMindTrainerParams"></param>
    /// <param name="worldSize"></param>
    /// <param name="navigationAlgorithm"></param>
    public QTable(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm, Action<CellInfo> updateAgent, Action<CellInfo> updateOther)
    {
        // Parametros de ejecucion
        _params = qMindTrainerParams;
        // Conocimiento del mundo para consultas
        _worldInfo = worldInfo;
        // Navegacion por el mundo entre posiciones
        _navigationAlgorithm = navigationAlgorithm;
        // Casillas promedio del mundo y su porcion entre 4
        _distances = (worldInfo.WorldSize[0] + worldInfo.WorldSize[1]) / 2 / 4;
        // Metodos de actualizacion de la partida
        this.updateAgent = updateAgent;
        this.updateOther = updateOther;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="agentPosition"></param>
    /// <param name="otherPosition"></param>
    internal void DoStep(CellInfo agentPosition, CellInfo otherPosition)
    {
        CellInfo[] ruta = _navigationAlgorithm.GetPath(otherPosition, agentPosition, 10);
        updateOther(ruta[0]);


        updateAgent(new CellInfo(0,0));
    }

    #region Gestion de valores

    /// <summary>
    /// Actualiza el valor de la tabla teniendo en cuenta la decision tomada y el estado.
    /// </summary>
    /// <param name="agentPosition"></param>
    /// <param name="otherPosition"></param>
    internal void UpdateWithReward(CellInfo agentPosition, CellInfo otherPosition)
    {
        // Calculamos la distancia manhattan respecto el agente y el enemigo
        float newDistance = agentPosition.Distance(otherPosition, CellInfo.DistanceType.Manhattan);
        // Calculamos el cuadrante el cual pertenece el enemigo respecto el agente
        float angle = (float)(Math.Atan2((agentPosition.x - otherPosition.x), (agentPosition.y - otherPosition.y)) * (180.0 / Math.PI));
        // Discretizamos la distancia y el angulo
        int discreteDistance = DiscretizeDistance(newDistance);
        int discreteAngle = DiscretizeAngle(angle);
        // Creamos el nuevo estado dado la situacion actual
        north = _worldInfo[agentPosition.x, agentPosition.y + 1];
        east = _worldInfo[agentPosition.x + 1, agentPosition.y];
        south = _worldInfo[agentPosition.x, agentPosition.y - 1];
        west = _worldInfo[agentPosition.x - 1, agentPosition.y];
        QState newState = new QState(north.Walkable, east.Walkable, south.Walkable, west.Walkable, discreteDistance, discreteAngle);


        //int[] algo = {29, 0, 1, 2, 3, 4, 5, 6, 7, 9, 8, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28 };
        //int max = algo.Max();
        //int posmax = algo.ToList().IndexOf(algo.Max());

        // Recompensa que se le da el jugador si la jugada es correcta*
        float reward = GetReward(agentPosition, otherPosition);

        //int newAction = GetActionIndex(otherPosition, agentPosition); // reviasr que el agente sea el enemigo o el jugador
        int newAction = GetActionIndex(agentPosition, otherPosition);

        // Actualizamos el valor de la tabla de valores sigueindo la regla de aprendizaje
        qTable[_previousState][_previousAction] = (1 - _params.alpha) * qTable[_previousState][_previousAction]
            + _params.alpha * (reward + _params.gamma * qTable[newState][newAction]);

        /**
        qTable[(state, action, angle, distance)] = (1 - _learningRate) * qTable[(state, action, angle, distance)] + _learningRate * (reward + _discountFactor * NextStateValue(state, nextState, angle, distance));
        qTable[newState][idxNewStateValue] = (1 - _learningRate) * qTable[_previousState][idxNewStateValue] + _learningRate * (reward + _discountFactor * nextAction(newState));
        //*/
    }
    /// <summary>
    /// Devuelve el angulo discretizado en uno de las distancias segmentadas (_distanceSegments).
    /// Con el valor estandar de _distanceSegments (3), cada corresponde a: Distancias cercanas = 0; Distancias medias = 1; Distancias lejanas = 2;
    /// </summary>
    /// <param name="realDistance"></param>
    /// <returns></returns>
    private int DiscretizeDistance(float realDistance)
    {
        if (realDistance < _distances)
        {
            return 0;
        }
        else if (realDistance < _distances * 3)
        {
            return 1;
        }
        else { return 2; }
    }
    /// <summary>
    /// Devuelve el angulo discretizado en uno de los cuadrantes segmentados (_angleSegments).
    /// Con el valor estandar de _angleSegments (8) cada cuadrante del 0 al 7 corresponde con cada porcion de 45º de un area circular completo (360º).
    /// </summary>
    /// <param name="realAngle"></param>
    /// <returns></returns>
    private int DiscretizeAngle(float realAngle)
    {
        return (int)(realAngle / (360 / _angleSegments));
    }
    /// <summary>
    /// Devuelve el valor con la maxima recompensa de un estado. Puede no ser la maxima dependiendo de la entropia (Epsilon).
    /// GetReward o GetRewardIndex? puede que tenga conflicto de existencia con GetActionIndex
    /// Calcula la recompensa respecto la distancia entre los agentes (agent y other)
    /// </summary>
    /// <returns></returns>
    private float GetReward(CellInfo agentPosition, CellInfo otherPosition)
    {
        float currentDistance = agentPosition.Distance(otherPosition, CellInfo.DistanceType.Euclidean);
        if (agentPosition == otherPosition)
        {
            return _negativeReward;
        }

        if (currentDistance < _previousDistance)
        {
            return _lowReward;
        }
        else if (currentDistance == _previousDistance)
        {
            return _hightReward;

        }
        else //if (currentDistance > _previousDistance)
        {
            return _positiveReward;
        }
    }
    /// <summary>
    /// Devuelve la siguiente accion a tomar en el estado actual. Up = 0; Right = 1; Down = 2; Left = 3;
    /// </summary>
    /// <param name="agentPosition"></param>
    /// <param name="otherPosition"></param>
    /// <returns></returns>
    private int GetActionIndex(CellInfo agentPosition, CellInfo otherPosition)
    {
        // Determine the action taken based on the change in agent position
        // Assuming the previous agent position is stored in _previousState
        if (otherPosition.y > agentPosition.y) return 0; // Up
        if (otherPosition.x > agentPosition.x) return 1; // Right
        if (otherPosition.y < agentPosition.y) return 2; // Down
        if (otherPosition.x < agentPosition.x) return 3; // Left
        return -1; // No valid action found
    }

    #endregion

    #region Input-Output

    /// <summary>
    /// Inicializa la tabla de valores. Si existe el archivo, se carga el archivo. En caso contrario, se genera una tabla vacia.
    /// </summary>
    /// <param name="fileName"></param>
    internal void Load(string fileName = "QTable")
    {
        if (File.Exists(path + fileName + ".csv")) Read(); else Init();
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
    /// <summary>
    /// Guarda la tabla de valores en un fichero CSV.
    /// </summary>
    /// <param name="fileName"></param>
    internal void Save(string fileName = "QTable")
    {
        StreamWriter file = File.CreateText(path + fileName + ".csv");
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
        Debug.Log($"QTable saved in '{Directory.GetCurrentDirectory()}'");
    }
    /// <summary>
    /// Lee el fichero CSV y lo carga en la tabla de valores.
    /// </summary>
    /// <param name="fileName"></param>
    internal void Read(string fileName = "QTable")
    {
        if (File.Exists(path + fileName + ".csv"))
        {
            qTable = new Dictionary<QState, float[]>();
            StreamReader file = new StreamReader(fileName + ".csv");
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
                         float.Parse(data[data.Length-4], culture),
                         float.Parse(data[data.Length-3], culture),
                         float.Parse(data[data.Length-2], culture),
                         float.Parse(data[data.Length-1], culture)
                    };
                qTable[temporalState] = dataCollection.ToArray();
            } while (!file.EndOfStream);
            file.Close(); // Al cerrar el fichero nos aseguramos que no queda ning�n dato por guardar

            Debug.Log("QTable loaded");
        }
        else
        {
            Debug.Log($"Error en la lectura de la tabla. El archivo {fileName}.csv no existe.");
        }
    }

    #endregion

}
