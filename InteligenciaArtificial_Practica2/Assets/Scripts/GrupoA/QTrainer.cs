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
    public float Return { get; private set; }
    public float ReturnAveraged { get; private set; }
    public event EventHandler OnEpisodeStarted;
    public event EventHandler OnEpisodeFinished;
    public bool _initialized = false;
    private WorldInfo _world;
    private QTable _qTable;
    private INavigationAlgorithm _navigationAlgorithm;
    private QMindTrainerParams _params;
    private bool autoEpsilon = false;
    private float _rewards;
    private int _rewardsCount;

    public void DoStep(bool train)
    {
        if (!_initialized)
        {
            do
            {
                AgentPosition = _world.RandomCell();
                OtherPosition = _world.RandomCell();
            } while (AgentPosition == OtherPosition);
            Return = 0; ReturnAveraged = 0;
            _initialized = true;
            if (autoEpsilon) { _params.epsilon = Mathf.Exp(-CurrentEpisode / (float)_params.episodes); } // Epsilon calculado - entrenamiento automatico
            OnEpisodeStarted?.Invoke(this, null);
        }
        else
        {
            // 1 - Identificar estado actual
            QTable.QState estadoInicial = _qTable.GetState(AgentPosition, OtherPosition);
            int oldDistance = (int)AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
            // 2 - Elegir accion del agente
            int accion = _qTable.GetTrainingAction(estadoInicial);
            // 3 - Mover al agente
            AgentPosition = _qTable.GetAgentMovement(accion, AgentPosition);
            // 4 - Identificar nuevo estado
            QTable.QState estadoNuevo = _qTable.GetState(AgentPosition, OtherPosition);
            int newDistance = (int)AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
            // 5 - Elegir recompensa
            //bool atrapado = AgentPosition.Equals(OtherPosition);
            bool atrapado = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan) <= 1;
            bool ilegal = !AgentPosition.Walkable;
            float recompensa = _qTable.GetReward(oldDistance, newDistance, ilegal, atrapado);
            _rewards += recompensa;
            _rewardsCount++;
            ReturnAveraged = _rewards / _rewardsCount;
            Return = recompensa;
            // 6 - Actualizar tablaQ
            if (train)
            {
                _qTable.UpdateWithReward(estadoInicial, estadoNuevo, accion, recompensa);
            }
            // 7 - Comprobar si hay que terminar el episodio
            if (atrapado || ilegal)
            {
                OnEpisodeFinished?.Invoke(this, null);
                _initialized = false;
                //Debug.Log("Es terminal: " + AgentPosition + ", " + OtherPosition);
                CurrentEpisode++;
                CurrentStep = 0;
                if (CurrentEpisode % _params.episodesBetweenSaves == 0 && train) _qTable.Save(CurrentEpisode);
            }
            // 8 - Mover al jugador (Player aka Other)
            CellInfo[] ruta = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 50);
            if (ruta != null && ruta.Length > 0) OtherPosition = ruta[0];
            CurrentStep++;
        }
        //Debug.Log("QMindTrainer: DoStep");
    }

    public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
    {
        // Asignamos las variables
        _params = qMindTrainerParams;
        _world = worldInfo;
        _navigationAlgorithm = navigationAlgorithm;
        // Inicializacion
        _qTable = new QTable(_params, _world);
        _qTable.Load();
        _navigationAlgorithm.Initialize(_world);
        Debug.Log("QMindTrainer: initialized");
    }
}

