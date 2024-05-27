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
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;
using Assets.Scripts.GrupoA;

public class QMindTrainer : IQMindTrainer
{
    public int CurrentEpisode { get; }
    public int CurrentStep { get; }
    public CellInfo AgentPosition { get; private set; }
    public CellInfo OtherPosition { get; private set; }
    public float Return { get; }
    public float ReturnAveraged { get; }
    public event EventHandler OnEpisodeStarted;
    public event EventHandler OnEpisodeFinished;

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
        QTable data = new QTable(qMindTrainerParams, worldInfo, navigationAlgorithm);
        OnEpisodeStarted?.Invoke(this, EventArgs.Empty);

        Debug.Log("QMindTrainer: initialized");
    }
}

