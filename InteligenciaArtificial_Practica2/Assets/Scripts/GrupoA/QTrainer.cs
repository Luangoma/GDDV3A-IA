using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System;
using UnityEngine;
using Assets.Scripts.GrupoA;

public class QTrainer : IQMindTrainer
{
    public int CurrentEpisode { get; private set; }
    public int CurrentStep { get; private set; }
    public CellInfo AgentPosition { get; private set; }
    public CellInfo OtherPosition { get; private set; }
    public float Return { get; }
    public float ReturnAveraged { get; }
    public event EventHandler OnEpisodeStarted; // ?
    public event EventHandler OnEpisodeFinished; // ?
    public bool _initialized = false;
    private WorldInfo _world;
    private QTable QTable;

    // HACER
    public void DoStep(bool train)
    {

        if (!_initialized)
        {
            QTable.InitializeEpisode();
            do
            {
                AgentPosition = _world.RandomCell();
                OtherPosition = _world.RandomCell();
            } while (!(AgentPosition.Walkable && OtherPosition.Walkable && (AgentPosition != OtherPosition)));
            _initialized = true;
            CurrentEpisode = 0;
        }
        else
        {
            /**/
            if (Terminal(AgentPosition, OtherPosition))
            {
                _initialized = false;
                Debug.Log("Es terminal: " + AgentPosition + ", " + OtherPosition);
                CurrentEpisode++;
                CurrentStep = 0;
            }
            else
            {
                QTable.DoStep(AgentPosition, OtherPosition);
                CurrentStep++;
            }
            //*/
        }
        Debug.Log("QMindTrainer: DoStep");
    }
    // REVISAR
    public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
    {
        _world = worldInfo;
        QTable = new QTable(qMindTrainerParams, worldInfo, navigationAlgorithm, true, false);
        OnEpisodeStarted?.Invoke(this, EventArgs.Empty);

        Debug.Log("QMindTrainer: initialized");
    }
    //// HECHO
    private bool Terminal(CellInfo cellA, CellInfo cellB)
    {
        return cellA == cellB;

    }
}

