using System;
using System.Collections.Generic;
using Navigation.Interfaces;
using Navigation.World;
using UnityEngine;

namespace Navigation.Algorithms.AStar
{ 
    public class AStar : INavigationAlgorithm
    {
        public enum Directions
        {
            None,
            Up,
            Right,
            Down,
            Left
        }

        private WorldInfo _world;
        private Directions _currentDirection = Directions.None;
        private int stepCount = 0;


        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm.AllowedMovements allowedMovements)
        {
            _world = worldInfo;
        }

        public CellInfo[] GetPath(CellInfo startNode, CellInfo targetNode)
        {
            throw new NotImplementedException();
        }
    }
}