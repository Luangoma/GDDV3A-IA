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
            Node target = new Node(targetNode);
            openList.Add(current);

            while (openList.Count > 0)
            {
                current = openList.First();
                openList.RemoveAt(0);
                closeList.Add(current);
                if (current.info.x == target.info.x && current.info.y == target.info.y)
                {
                    return current;
                }
                else
                {
                    Node[] sucesors = Expand(current, target);
                    foreach (Node sucesor in sucesors)
                    {
                        if (!visited(sucesor))
                        {
                            openList.Add(sucesor);
                        }
                    }
                    openList.Sort();
                }
            }
            return null;
        }
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
        public Node[] Expand(Node padre, Node targetNode)
        {
            List<Node> nodes = new List<Node>();
            CellInfo[] cells = {
                _world[padre.info.x, padre.info.y+1],
                _world[padre.info.x+1, padre.info.y],
                _world[padre.info.x, padre.info.y-1],
                _world[padre.info.x-1, padre.info.y]
            };
            if (cells[0].Walkable)
            {
                nodes.Add(new Node(cells[0], padre.G_Coste, padre));
            }
            if (cells[1].Walkable)
            {
                nodes.Add(new Node(cells[1], padre.G_Coste, padre));
            }
            if (cells[2].Walkable)
            {
                nodes.Add(new Node(cells[2], padre.G_Coste, padre));
            }
            if (cells[3].Walkable)
            {
                nodes.Add(new Node(cells[3], padre.G_Coste, padre));
            }
            nodes.ForEach(node => { node.calculateHeuristic(targetNode); });
            return nodes.ToArray();
        }
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