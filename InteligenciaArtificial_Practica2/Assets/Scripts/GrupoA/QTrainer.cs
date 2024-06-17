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
    private QTable qTable;
    private INavigationAlgorithm _navigationAlgorithm;
    private int _episodesBetweenSaves;
    // REVISAR
    public void DoStep(bool train)
    {
        if (!_initialized)
        {
            qTable.Init();
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
            // 1 - Identificar estado actual
            QTable.QState estadoInicial = qTable.GetState(AgentPosition, OtherPosition);
            int oldDistance = (int)AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
            // 2 - Elegir accion del agente
            int accion = qTable.GetAction(estadoInicial);
            // 3 - Mover al agente
            AgentPosition = qTable.GetAgentMovement(accion, AgentPosition);
            // 4 - Mover al jugador ((other)el jugador que es una ia, no el zombi)
            CellInfo[] ruta = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 50);
            if (ruta != null && ruta.Length > 0)
                OtherPosition = ruta[0];
            // 5 - Identificar nuevo estado
            QTable.QState estadoNuevo = qTable.GetState(AgentPosition, OtherPosition);
            int newDistance = (int)AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
            // 6 - Elegir recompensa
            bool atrapado = AgentPosition.Equals(OtherPosition);
            bool ilegal = !AgentPosition.Walkable;
            float recompensa = qTable.GetReward(oldDistance, newDistance, ilegal, atrapado);
            // 7 - Actualizar tablaQ
            qTable.UpdateWithReward(estadoInicial, estadoNuevo, accion, recompensa);
            // 8 - Comprobar si hay que terminar el episodio
            if (atrapado || ilegal)
            {
                OnEpisodeFinished?.Invoke(this, null);
                _initialized = false;
                Debug.Log("Es terminal: " + AgentPosition + ", " + OtherPosition);
                CurrentEpisode++;
                CurrentStep = 0;
                if (CurrentEpisode % _episodesBetweenSaves == 0) qTable.Save();
            }
            CurrentStep++;
        }
        Debug.Log("QMindTrainer: DoStep");
    }

    public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
    {
        // Asignamos las variables de inicializacion
        _world = worldInfo;
        qTable = new QTable(qMindTrainerParams, worldInfo, navigationAlgorithm);
        _episodesBetweenSaves = qMindTrainerParams.episodesBetweenSaves;
        _navigationAlgorithm = navigationAlgorithm;
        CurrentEpisode = 0;
        Debug.Log("QMindTrainer: initialized");
    }

    private bool Terminal(CellInfo cellA, CellInfo cellB)
    {
        return cellA == cellB || !AgentPosition.Walkable;
    }
}

