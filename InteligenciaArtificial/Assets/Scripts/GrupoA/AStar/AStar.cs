using System;
using System.Collections.Generic;
using System.Linq;
using Navigation.Interfaces;
using Navigation.World;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace Assets.Scripts.GrupoA.AStar
{
    public class AStar : INavigationAlgorithm
    {
        private WorldInfo _world;
        private List<Node> openList = new List<Node>();
        private List<Node> closeList = new List<Node>();

        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm.AllowedMovements allowedMovements)
        {
            _world = worldInfo;
        }

        public Node GetNodePath(CellInfo startNode, CellInfo targetNode)
        {
            openList.Clear();
            closeList.Clear();

            Node current = new Node(startNode);
            openList.Add(current);

            while (openList.Count > 0)
            {
                current = openList.First();
                openList.RemoveAt(0);
                closeList.Add(current);
                if (current.info.x == targetNode.x && current.info.y == targetNode.y)
                {
                    //?// COMO DEVOLVER EL ESTADO ACTUAL
                    return current;
                }
                else
                {
                    Node[] sucesors = current.Expand(_world);
                    foreach (Node sucesor in sucesors)
                    {
                        if (!visited(sucesor))
                        {
                            openList.Add(sucesor);
                            //?// CONSULTAR COMO SE PUEDE ORDENAR POR UN VALOR DE UNA VARIABLE
                        }
                    }
                    openList.Sort();
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
            temporal.Reverse();
            return temporal.ToArray();
        }
        //*/ 

        public bool visited(Node node)
        {
            foreach (Node lista in closeList)
            {
                if (lista.EqualsNode(node))
                {
                    return true;
                }
            }
            return false;
        }
    }
}