using System;
using System.Collections.Generic;
using System.Linq;
using Navigation.Interfaces;
using Navigation.World;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace Assets.Scripts.GrupoA.AStar
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
            Node current = new Node(startNode);
            openList.Add(current);
            while ((openList != null)) { 
                current = openList.First();
                openList.RemoveAt(0);
                if (startNode.Type == targetNode.Type)
                {
                    CellInfo[] a = { startNode };
                    return a;
                }
                else
                {
                    sucesors = Expand(current);
                    foreach (Node sucesor in sucesors)
                    {
                        openList.Add(sucesor);
                    }
                }
            }
            return null;
        }

        public Node[] Expand(Node nodeToExpand)
        {
            List<Node> nodes = new List<Node>();
            #region PosicionesAExpandir
            CellInfo[] cells = { _world[nodeToExpand.info.x+1, nodeToExpand.info.y],
                _world[nodeToExpand.info.x, nodeToExpand.info.y+1],
                _world[nodeToExpand.info.x, nodeToExpand.info.y-1],
                _world[nodeToExpand.info.x-1, nodeToExpand.info.y]
            };
            if (cells[0].Walkable)
            {
                nodes.Add(new Node(cells[0], 1, nodeToExpand));
            }
            if (cells[1].Walkable)
            {
                nodes.Add(new Node(cells[1], 1, nodeToExpand));
            }
            if (cells[2].Walkable)
            {
                nodes.Add(new Node(cells[2], 1, nodeToExpand));
            }
            if (cells[3].Walkable)
            {
                nodes.Add(new Node(cells[3], 1, nodeToExpand));
            }
            #endregion
            return nodes.ToArray();
        }
    }
}