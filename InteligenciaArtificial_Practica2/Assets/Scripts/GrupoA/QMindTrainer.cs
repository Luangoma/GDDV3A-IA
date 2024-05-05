using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class QMindTrainer : IQMindTrainer
{
    #region Variables, a lot of variables
    private bool _initialized = false;
    private Dictionary<(CellInfo, CellInfo), float> qTable = new Dictionary<(CellInfo, CellInfo), float>();
    private QMindTrainerParams _params;
    private float _learningRate { get => _params.alpha; } // Alpha
    private float _discountFactor { get => _params.gamma; } // Gamma
    private float _explorationRate { get => _params.epsilon; } // Epsilon
    private int _episodes { get => _params.episodes; }
    private int _maxSteps { get => _params.maxSteps; }
    private int _episodesBetweenSaves { get => _params.episodesBetweenSaves; }
    private INavigationAlgorithm _navigationAlgorithm;
    public WorldInfo _world;
    public bool memory = true;
    public int CurrentEpisode { get; }
    public int CurrentStep { get; }
    public CellInfo AgentPosition { get; private set; }
    public CellInfo OtherPosition { get; private set; }
    public float Return { get; }
    public float ReturnAveraged { get; }
    public event EventHandler OnEpisodeStarted;
    public event EventHandler OnEpisodeFinished;
    #endregion

    // HACER
    public void DoStep(bool train)
    {
        if (!_initialized)
        {
            InitializeEpisode();
            _initialized = true;
        }
        else
        {
            /**
            updateWithReward();
            if (Terminal())
            {
                _initialized = false;
                Debug.Log("Es terminal: " + Terminal());
            }
            //*/
        }

        Debug.Log("QMindTrainer: DoStep");
    }
    // REVISAR
    public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
    {
        _initialized = true;
        _params = qMindTrainerParams;
        _world = worldInfo;
        _navigationAlgorithm = navigationAlgorithm;
        OnEpisodeStarted?.Invoke(this, EventArgs.Empty);

        Debug.Log("QMindTrainer: initialized");
    }
    // HACER
    private void InitializeEpisode()
    {
        if (memory) { ReadQTable(); } else { initQTable(); }

    }
    #region Cells
    //// HECHO
    private CellInfo[] ExpandCellInfo(CellInfo initState)
    {
        CellInfo[] cells = { // Expansion de la celda
                _world[initState.x, initState.y+1], // Arriba
                _world[initState.x+1, initState.y], // Derecha
                _world[initState.x, initState.y-1], // Abajo
                _world[initState.x-1, initState.y]  // Izquierda
            };
        return cells;
    }
    //// HECHO
    private CellInfo[] ExpandWalkablesCellInfo(CellInfo initState)
    {
        CellInfo[] cells = ExpandCellInfo(initState);
        List<CellInfo> cellList = new List<CellInfo>();
        foreach (var cell in cells)
        {
            if (cell.Walkable)
            {
                cellList.Add(cell);
            }
        }
        return cellList.ToArray();
    }
    //// HECHO
    private bool Terminal(CellInfo cellA, CellInfo cellB)
    {
        return cellA == cellB;
    }
    #endregion
    #region Rewards
    // REVISAR
    private float NextStateValue(CellInfo state, CellInfo nextState)
    {
        /**
        CellInfo[] cells = { // Expansion de la celda
                _world[state.x, state.y+1], // Arriba
                _world[state.x+1, state.y], // Derecha
                _world[state.x, state.y-1], // Abajo
                _world[state.x-1, state.y]  // Izquierda
            };
        List<CellInfo> rewards = new List<CellInfo>();
        foreach (var cell in cells)
        {
            if (cell.Walkable)
            {
                rewards.Add(cell);
            }
        }
        List<float> cost = new List<float>();
        foreach (var cell in rewards)
        {
            cost.Add(qTable[(nextState, cell)]);
        }
        List<float> rewards = new List<float>();
        foreach (var cell in cells)
        {
            if (cell.Walkable)
            {
                rewards.Add(qTable[(nextState, cell)]);
            }
        }
        //*/
        List<float> rewards = new List<float>();
        CellInfo[] cells = ExpandWalkablesCellInfo(state);
        foreach (var cell in cells)
        {
            rewards.Add(qTable[(nextState, cell)]);
        }
        return rewards.Max();
    }
    //// HECHO
    private void updateWithReward(CellInfo state, CellInfo action, float reward, CellInfo nextState)
    {
        /**
        qTable = Q[state, action] + _learningRate * (reward + _discountFactor * Enumerable.Range
            (0, Q.GetLength(1)).Select(i => Q[nextState, i]).Max() - Q[state, action]);
        qTable = Q[state, action] + _learningRate * (reward + _discountFactor * np.max(Q[s_,:]) - Q[s, a]);

        var key = Tuple.Create(state, action);
        qTable[key] = qTable.ContainsKey(key) ? Q[key] : 0;
        qTable[key] += _learningRate * (reward + _discountFactor * Q.Where(x => x.Key.Item1 == nextState).Max(x => x.Value) - qTable[key]);
        Si fuera un array:
        Q[s, a] = Q[s, a] + lr * (r + y * Enumerable.Range(0, Q.GetLength(1)).Select(i => Q[nextState, i]).Max() - Q[s, a]);

        Q = qtable[currentState, direction];
        r = GetReward(nextState);
        float nextQMax = nextState != null ? qtable.GetHighestQValue(nextState) : 0;
        (1 - alpha) * Q + alpha * (r + gamma * nextQMax);
        
        //*/
        qTable[(state, action)] = (1 - _learningRate) * qTable[(state, action)] + _learningRate * (reward + _discountFactor * NextStateValue(state, nextState));
    }
    #endregion
    #region QTable
    // REVISAR
    void initQTable()
    {
        Vector2 dimensions = _world.WorldSize;
        List<CellInfo> cells = new List<CellInfo>();
        for (int x = 0; x < dimensions[0]; x++)
        {
            for (int y = 0; y < dimensions[1]; y++)
            {
                cells.Add(new CellInfo(x, y));
            }
        }
        /**
        int length = cells.Count;
        for (int x = 0; x < length; x++)
        {

            for (int y = 0; y < length; y++)
            {
                qTable[(cells[x], cells[y])] = 0;
            }
        }
        //*/
        foreach (var cell in cells)
        {
            CellInfo[] neighbors = ExpandWalkablesCellInfo(cell);
            foreach (var neighbor in neighbors)
            {
                qTable[(cell, neighbor)] = 0.0f;
            }
        }
    }
    // HACER
    void SaveQTable()
    {
        String name = "Qlearning_" + CurrentEpisode + "k.csv"; // Nombre del fichero
        StreamWriter file = File.CreateText(name); // Creamos un fichero
        // file = File.AppendText("prueba.txt"); // Se añade texto al fichero existente
        foreach (var cellInfo in qTable.Values)
        {
            file.WriteLine(cellInfo); // Lo mismo que cuando escribimos por consola
        }
        file.Close(); // Al cerrar el fichero nos aseguramos que no queda ningún dato por guardar
    }
    // HACER
    void ReadQTable()
    {
        int episode = CurrentEpisode - 20000;
        String name = "Qlearning_" + episode + "k.csv"; // Nombre del fichero
        StreamReader file = File.OpenText(name); // Leemos un fichero
        if (File.Exists(name))
        {
            while (!file.EndOfStream)
            {
                file.ReadLine();
            }
        }
    }
    #endregion
}

