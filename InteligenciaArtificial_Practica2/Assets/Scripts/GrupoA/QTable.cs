using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTable
{
    // Estructuras simples, almacen de estado
    struct QState
    {

        //Accion, distancia y orientacion respecto el enemigo
        char action;
        int distance;
        int orientation;

        public QState(char action, int distance, int orientation)
        {
            this.action = action;
            this.distance = distance;
            this.orientation = orientation;
        }
    }

    #region Variables
    // Variables Base
    private Dictionary<QState, float[]> qTable = new Dictionary<QState, float[]>();
    private QState _previousState;
    private QMindTrainerParams _params;
    private INavigationAlgorithm _navigationAlgorithm;
    private System.Random _random = new System.Random();
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
    internal void Init()
    {

    }
    /// <summary>
    /// Guarda la tabla de valores en un fichero CSV.
    /// </summary>
    internal void Save()
    {

    }
    /// <summary>
    /// Lee el fichero CSV y lo carga en la tabla de valores.
    /// </summary>
    internal void Read()
    {

    }
}
