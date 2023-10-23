using System;
using System.Collections.Generic;
using System.Linq;
using Navigation.Interfaces;
using Navigation.World;
using UnityEditor.Experimental.GraphView;
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
        private List<Node> openList;
        private Node[] sucesors;

        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm.AllowedMovements allowedMovements)
        {
            _world = worldInfo;
        }

        public CellInfo[] GetPath(CellInfo startNode, CellInfo targetNode)
        {
            throw new NotImplementedException();
        }

        public Node SpaceStateSearch(Node initialState)
        {
            Node current = new Node();
            openList.Add(initialState);
            do
            {
                current = openList.First();
                if (current.GetCellInfo().Type = CellInfo.CellType.Exit)
                {
                    return current;
                }
                else
                {
                    sucesors = expand(current);
                    foreach (CellInfo sucesor in sucesors)
                    {
                        openList.Add(sucesor);
                    }
                }
            } while ((openList != null));
            return null;
        }
        public Node[] expand(Node nodeToExpand)
        {
            Node[] nodes = null;
            int i = 0;
            foreach (Node newNode in nodeToExpand)
            {
                nodes[i] = newNode;
                i++;
            }
            return nodes;
        }
    }
}