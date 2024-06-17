using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System;
using UnityEngine;

public class QTrainer : IQMindTrainer
{
    public int CurrentEpisode { get; private set; }
    public int CurrentStep { get; private set; }
    public CellInfo AgentPosition { get; private set; }
    public CellInfo OtherPosition { get; private set; }
    public float Return { get; }
    public float ReturnAveraged { get; }
    public event EventHandler OnEpisodeStarted;
    public event EventHandler OnEpisodeFinished;
    public bool _initialized = false;
    private WorldInfo _world;
    private QTable QTable;

    // REVISAR
    public void DoStep(bool train)
    {
        if (!_initialized)
        {
            QTable.Init();
            do
            {
                AgentPosition = _world.RandomCell();
                OtherPosition = _world.RandomCell();
            } while (!(AgentPosition.Walkable && OtherPosition.Walkable && (AgentPosition != OtherPosition)));
            _initialized = true;
            OnEpisodeStarted?.Invoke(this, null);
        }
        else
        {
            if (Terminal(AgentPosition, OtherPosition))
            {
                OnEpisodeFinished?.Invoke(this, null);
                _initialized = false;
                Debug.Log("Es terminal: " + AgentPosition + ", " + OtherPosition);
                CurrentEpisode++;
                CurrentStep = 0;
                QTable.Save();
            }
            else
            {
                QTable.DoStep(AgentPosition, OtherPosition);
                QTable.UpdateWithReward(AgentPosition,OtherPosition);
                CurrentStep++;
            }
        }
        Debug.Log("QMindTrainer: DoStep");
    }

    public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
    {
        // Asignamos las variables de inicializacion
        _world = worldInfo;
        QTable = new QTable(qMindTrainerParams, worldInfo, navigationAlgorithm,SetAgent,SetOther);
        CurrentEpisode = 0;
        Debug.Log("QMindTrainer: initialized");
    }

    private bool Terminal(CellInfo cellA, CellInfo cellB)
    {
        return cellA == cellB;
    }
    private void SetAgent(CellInfo newData)
    {
        AgentPosition = newData;
    }
    private void SetOther(CellInfo newData)
    {
        OtherPosition = newData;
    }
}

