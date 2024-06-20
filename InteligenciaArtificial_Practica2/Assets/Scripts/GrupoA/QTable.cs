﻿using NavigationDJIA.World;
using QMind;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
    private readonly WorldInfo _worldInfo;
    private readonly string path = Application.dataPath + "/scripts/GrupoA/";

    // Tabla de valores
    private Dictionary<QState, float[]> _qTable = new Dictionary<QState, float[]>();

    // Estado parcial
    private bool north, east, south, west;

    // Segmentacion del mundo
    private float _distances;
    private int _distancesValues;
    private const int _distanceSegments = 3;
    private int _angleSegments = 8;

    // Recompensas
    private const int _negativeReward = -100;   // Pierde o terminal
    private const int _lowReward = -10;          // Sigue vivo pero se acerca
    private const int _hightReward = 0;         // Mantiene distancia
    private const int _positiveReward = 100;      // Se aleja

    // Version del fichero
    private int _episodesFile;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor de la tabla de valores y del resto de elementos base para entrenar.
    /// </summary>
    /// <param name="qMindTrainerParams"></param>
    /// <param name="worldSize"></param>
    /// <param name="navigationAlgorithm"></param>
    public QTable(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo)
    {
        // Parametros de ejecucion
        _params = qMindTrainerParams;
        // Conocimiento del mundo para consultas
        _worldInfo = worldInfo;
        // Casillas promedio del mundo y su porcion entre 4
        _distances = (worldInfo.WorldSize[0] + worldInfo.WorldSize[1]) / 2 / 4;
        _distancesValues = _distanceSegments;// + (int)_distances;
    }
    /// <summary>
    /// Constructor de la tabla de valores para solamente entreno.
    /// </summary>
    /// <param name="worldInfo"></param>
    public QTable(WorldInfo worldInfo)
    {
        _worldInfo = worldInfo;
        // Casillas promedio del mundo y su porcion entre 4
        _distances = (worldInfo.WorldSize[0] + worldInfo.WorldSize[1]) / 2 / 4;
    }

    #endregion

    #region Gestion de valores

    /// <summary>
    /// Actualiza el valor de la tabla teniendo en cuenta la decision tomada y el estado.
    /// </summary>
    /// <param name="oldState"></param>
    /// <param name="newState"></param>
    /// <param name="action"></param>
    /// <param name="reward"></param>
    internal void UpdateWithReward(QState oldState, QState newState, int action, float reward)
    {
        if (!_qTable.ContainsKey(oldState)) { _qTable.Add(oldState, new float[4]); }
        if (!_qTable.ContainsKey(newState)) { _qTable.Add(newState, new float[4]); }
        /**
         * q'(s,a) =  (1-alpha)*q(s,a) + alpha(r + gamma*max(q(s',a'))
         */
        float newValue = (1 - _params.alpha) * _qTable[oldState][action];
        float maxFutureValue = _qTable[newState].Max();
        newValue += _params.alpha * (reward + _params.gamma * maxFutureValue);
        _qTable[oldState][action] = newValue;
    }
    /// <summary>
    /// Devuelve el angulo discretizado en uno de las distancias segmentadas (_distanceSegments).
    /// </summary>
    /// <param name="realDistance"></param>
    /// <returns>Distancias cercanas = 0; Distancias medias = 1; Distancias lejanas = 2;</returns>
    private int DiscretizeDistance(float realDistance)
    {
        if (realDistance < _distances)
        {
            return 0;
            //return (int)realDistance;
        }
        else if (realDistance < _distances * 3)
        {
            return 1;
            //return (int)_distances;
        }
        else
        {
            return 2;
            //return (int)(1 + _distances);
        }
    }
    /// <summary>
    /// Devuelve el angulo discretizado en uno de los cuadrantes segmentados (_angleSegments).
    /// Con el valor estandar de _angleSegments (8) 
    /// </summary>
    /// <param name="realAngle"></param>
    /// <returns>Indice del cuadrante del 0 al 7 corresponde, cada porcion de 45º corresponde con un cuadrante.</returns>
    private int DiscretizeAngle(float realAngle)
    {
        return (int)(realAngle / (360 / _angleSegments));
    }
    /// <summary>
    /// Funcion que normaliza los episodios a la funcion de e^(-x). (Funcion decreciente).
    /// </summary>
    /// <param name="value"></param>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <returns>Valor normalizado entre los dos parametros.</returns>
    private float Normalize(float value, float minValue = 0, float maxValue = 1)
    {
        if (minValue == maxValue)
        {
            return value;
        }
        return (value - minValue) / (maxValue - minValue);
    }
    /// <summary>
    /// Devuelve el valor con la maxima recompensa de un estado. Puede no ser la maxima dependiendo de la entropia (Epsilon).
    /// </summary>
    /// <param name="oldDist"></param>
    /// <param name="newDist"></param>
    /// <param name="illegal"></param>
    /// <param name="player"></param>
    /// <returns>Recompensa respecto las distancias.</returns>
    public float GetReward(int oldDist, int newDist, bool illegal, bool player)
    {
        //float currentDistance = agentPosition.Distance(otherPosition, CellInfo.DistanceType.Manhattan);
        if (player || illegal)
        {
            return _negativeReward;
        }

        if (newDist < oldDist)
        {
            //recompensa negativa
            return _lowReward;
        }
        else if (newDist == oldDist)
        {
            return _hightReward;
        }
        else //if (newDist > oldDist)
        {
            return _positiveReward;
        }
    }
    /// <summary>
    /// Crea un estado dado las posiciones del agente y del enemigo.
    /// </summary>
    /// <param name="agentPosition"></param>
    /// <param name="otherPosition"></param>
    /// <returns>Nuevo estado respecto las posiciones.</returns>
    public QState GetState(CellInfo agentPosition, CellInfo otherPosition)
    {
        // Calculamos la distancia manhattan respecto el agente y el enemigo
        float distance = agentPosition.Distance(otherPosition, CellInfo.DistanceType.Manhattan);
        // Calculamos el cuadrante el cual pertenece el enemigo respecto el agente
        Vector2 miPosicion = new Vector2(agentPosition.x, agentPosition.y);
        Vector2 suPosicion = new Vector2(otherPosition.x, otherPosition.y);
        Vector2 direction = suPosicion - miPosicion;
        float angle = (float)(Mathf.Atan2(direction.y, direction.x) * (180.0 / Mathf.PI));
        angle -= 360 / _angleSegments / 2;
        angle = angle >= 0 ? angle : 360 + angle;
        // Discretizamos la distancia y el angulo
        int discreteDistance = DiscretizeDistance(distance);
        int discreteAngle = DiscretizeAngle(angle);
        //Debug.Log($"Angulo real: {angle}. angulo discreto: {discreteAngle}.");
        // Creamos el nuevo estado dado la situacion actual
        north = _worldInfo.NextCell(agentPosition,Directions.Up).Walkable;
        east = _worldInfo.NextCell(agentPosition,Directions.Right).Walkable;
        south = _worldInfo.NextCell(agentPosition,Directions.Down).Walkable;
        west = _worldInfo.NextCell(agentPosition,Directions.Left).Walkable;
        return new QState(north, east, south, west, discreteDistance, discreteAngle);
    }
    /// <summary>
    /// Devuelve una accion dependiendo del epsilon. Usado solo para entrenar.
    /// </summary>
    /// <param name="estado"></param>
    /// <returns>Indice de una accion a entrenar.</returns>
    public int GetTrainingAction(QState estado, int CurrentEpisode = 1)
    {
        // 1 - Elegir nº random entre 0 y 1
        float value = UnityEngine.Random.Range(0f, 1f);
        // 2 - Si epsilon es mayor al numero, accion aleatoria, si no, la opcion valor amas alto
        if (_params.epsilon > value) // Usando el epsilon proporcionado - entrenamiento manual
        //Debug.Log(Mathf.Exp(-Normalize(CurrentEpisode, 0, _params.episodes)));
        //if (Mathf.Exp(-Normalize(CurrentEpisode, 0, _params.episodes)) > value) // Epsilon calculado - entrenamiento automatico
        {
            return UnityEngine.Random.Range(0, 4);
        }
        else
        {
            return GetAction(estado);
        }
    }
    /// <summary>
    /// Devuelve la mejor accion a tomar dada la situacion.
    /// </summary>
    /// <param name="estado"></param>
    /// <returns>El indice de la accion tomada.</returns>
    public int GetAction(QState estado)
    {
        return _qTable[estado].ToList().IndexOf(_qTable[estado].Max());
    }
    /// <summary>
    /// Calcula la celda a donde va a moverse el agente dado la accion y la posicion del agente.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="agentPosition"></param>
    /// <returns>Nueva posicion del agente.</returns>
    public CellInfo GetAgentMovement(int action, CellInfo agentPosition)
    {
        switch (action)
        {
            case 0:
                return _worldInfo.NextCell(agentPosition, Directions.Up);
            case 1:
                return _worldInfo.NextCell(agentPosition, Directions.Right);
            case 2:
                return _worldInfo.NextCell(agentPosition, Directions.Down);
            case 3:
                return _worldInfo.NextCell(agentPosition, Directions.Left);
        }
        return null;
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
            for (int distance = 0; distance < _distancesValues; distance++)
            {
                for (int orientation = 0; orientation < _angleSegments; orientation++)
                {
                    // Inicializamos los valores Q a un valor por defecto 0.0f.
                    _qTable[new QState(states[option, 0], states[option, 1], states[option, 2], states[option, 3], distance, orientation)] = new float[4]
                    /**/
                    {
                        states[option, 0] ? 0f : _negativeReward,
                        states[option, 1] ? 0f : _negativeReward,
                        states[option, 2] ? 0f : _negativeReward,
                        states[option, 3] ? 0f : _negativeReward
                    }//*/
                    ;
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
        string initialText = "Episodios del archivo;" + _episodesFile.ToString() + ";Distancias;" + _distancesValues + ";Segmentos de angulos;" + _angleSegments;
        file.WriteLine(initialText);

        string keyValue;
        foreach (var keys in _qTable.Keys)
        {
            // Claves
            keyValue = keys.up + ";" + keys.right + ";" + keys.down + ";" + keys.left + ";" + keys.distance + ";" + keys.orientation;
            // Valores
            float[] dataCollection = _qTable[keys];
            for (var data = 0; data < dataCollection.Length; data++)
            {
                keyValue += ";" + dataCollection[data].ToString();
            }
            file.WriteLine(keyValue); // Lo mismo que cuando escribimos por consola
        }
        file.Close(); // Al cerrar el fichero nos aseguramos que no queda ningun dato por guardar

        //Debug.Log($"QTable saved. DataPath: {Application.dataPath}/Scripts/GrupoA/");
        Debug.Log($"QTable saved in '{path}'");
    }
    /// <summary>
    /// Lee el fichero CSV y lo carga en la tabla de valores.
    /// </summary>
    /// <param name="fileName"></param>
    internal void Read(string fileName = "QTable")
    {
        if (File.Exists(path + fileName + ".csv"))
        {
            _qTable = new Dictionary<QState, float[]>();
            StreamReader file = new StreamReader(path + fileName + ".csv");
            string keyValue = file.ReadLine();
            string[] data;
            data = keyValue.Split(';');
            _episodesFile = int.Parse(data[1]);
            /**
            _distances = int.Parse(data[3]);
            _angleSegments = int.Parse(data[5]);
            //*/
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
                _qTable[temporalState] = dataCollection.ToArray();
            } while (!file.EndOfStream);
            file.Close(); // Al cerrar el fichero nos aseguramos que no queda ning�n dato por guardar

            Debug.Log("QTable read");
        }
        else
        {
            Debug.Log($"Error en la lectura de la tabla. El archivo {fileName}.csv no existe.");
        }
    }

    #endregion

}
