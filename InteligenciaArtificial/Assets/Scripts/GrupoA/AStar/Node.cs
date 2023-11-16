using Navigation.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GrupoA.AStar
{
    public class Node : IComparable<Node>, IEquatable<Node>
    {
        public CellInfo info;
        public Node padre;
        public int G_Coste;
        public int H_Heuristica;
        public int Funcion_Heuristica;
        public Node(CellInfo info, int G_Coste = 0, Node padre = null)
        {
            this.info = info;
            this.padre = padre;
            this.G_Coste = G_Coste + 1;
            this.H_Heuristica = 0;
            this.Funcion_Heuristica = 0;
        }

        public Node[] Expand(WorldInfo mundo)
        {
            List<Node> nodes = new List<Node>();
            CellInfo[] cells = { mundo[this.info.x+1, this.info.y],
                mundo[this.info.x, this.info.y+1],
                mundo[this.info.x, this.info.y-1],
                mundo[this.info.x-1, this.info.y]
            };

            if (cells[0].Walkable)
            {
                nodes.Add(new Node(cells[0], this.G_Coste, this));
            }
            if (cells[1].Walkable)
            {
                nodes.Add(new Node(cells[1], this.G_Coste, this));
            }
            if (cells[2].Walkable)
            {
                nodes.Add(new Node(cells[2], this.G_Coste, this));
            }
            if (cells[3].Walkable)
            {
                nodes.Add(new Node(cells[3], this.G_Coste, this));
            }
            return nodes.ToArray();
        }
        public int D_Manhattan(Node targetNode)
        {
            return (Math.Abs(targetNode.info.x - this.info.x) + Math.Abs(targetNode.info.y - this.info.y));
        }
        public int calculateHeuristic(Node targetNode)
        {
            H_Heuristica = D_Manhattan(targetNode);
            Funcion_Heuristica = G_Coste + H_Heuristica;
            return Funcion_Heuristica;
        }

        public int CompareTo(Node other)
        {
            if (other == null) return 1;
            return this.Funcion_Heuristica.CompareTo(other.Funcion_Heuristica);
        }

        public bool Equals(Node other)
        {
            if (other == null) return false;
            return this.Funcion_Heuristica == other.Funcion_Heuristica;
        }
    }
}
