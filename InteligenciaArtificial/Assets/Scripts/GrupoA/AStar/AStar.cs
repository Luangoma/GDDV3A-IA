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
        private List<Directions> _currentDirection;
        private List<Node> openList;

        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm.AllowedMovements allowedMovements)
        {
            _world = worldInfo;
        }

        public Node GetNodePath(CellInfo startNode, CellInfo targetNode)
        {
            Node current = new Node(startNode);
            openList.Add(current);

            while (openList.Count > 0)
            {
                current = openList.First();
                openList.RemoveAt(0);
                if (startNode.x == targetNode.x && startNode.y == targetNode.y)
                {
                    //?// COMO DEVOLVER EL ESTADO ACTUAL
                    return current;
                }
                else
                {
                    Node[] sucesors = current.Expand(_world);
                    foreach (Node sucesor in sucesors)
                    {
                        sucesor.padre = current;
                        openList.Add(sucesor);
                        //?// CONSULTAR COMO SE PUEDE ORDENAR POR UN VALOR DE UNA VARIABLE
                        openList.Sort();
                    }
                }
            }
            return null;
        }
        /**/
        public CellInfo[] GetPath(CellInfo startNode, CellInfo targetNode)
        {
            Node current = GetNodePath(startNode, targetNode);

            List<CellInfo> temporal = new List<CellInfo>();
            while (current != null)
            {
                temporal.Add(current.info);
                current = current.padre;
            }
            return temporal.ToArray();
        }
        //*/


    }
}