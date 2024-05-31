using NavigationDJIA.World;
using QMind.Interfaces;
using UnityEngine;

public class QAgent : IQMind
{
    public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
    {
        Debug.Log("QMindAgent: GetNextStep");
        return null;
    }

    public void Initialize(WorldInfo worldInfo)
    {
        Debug.Log("QMindAgent: initialized");
    }
}
