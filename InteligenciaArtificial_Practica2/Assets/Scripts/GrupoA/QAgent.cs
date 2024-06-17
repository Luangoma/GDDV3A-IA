using NavigationDJIA.World;
using QMind.Interfaces;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class QAgent : IQMind
{
    private WorldInfo _world;
    private QTable _qTable;
    public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
    {
        // 1 - Identificar estado actual
        QTable.QState estadoInicial = _qTable.GetState(currentPosition, otherPosition);
        int oldDistance = (int)currentPosition.Distance(otherPosition, CellInfo.DistanceType.Manhattan);
        // 2 - Elegir accion del agente
        int accion = _qTable.GetAction(estadoInicial);
        // 3 - Mover al agente
        return _qTable.GetAgentMovement(accion, currentPosition);
    }

    public void Initialize(WorldInfo worldInfo)
    {
        _world = worldInfo;
        _qTable.Load();
        Debug.Log("QMindAgent: initialized");
    }
}
