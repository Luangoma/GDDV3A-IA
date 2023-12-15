using Navigation.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Navigation.World.CellInfo;

namespace Assets.Scripts.GrupoA.AStar
{
    public class Node : IComparable<Node>, IEquatable<Node>
    {
        public CellInfo info;
        public Node padre;
        public float G_Coste;
        public float H_Heuristica;
        public float Funcion_Heuristica;

        public Node(CellInfo info, float G_Coste = 0, Node padre = null)
        {
            this.info = info;
            this.padre = padre;
            this.G_Coste = G_Coste + 1;
            this.H_Heuristica = 0;
            this.Funcion_Heuristica = 0;
        }

        public float D_Manhattan(Node targetNode)
        {
            return (Math.Abs(targetNode.info.x - this.info.x) + Math.Abs(targetNode.info.y - this.info.y));
        }
        public float calculateHeuristic(Node targetNode)
        {
            H_Heuristica = D_Manhattan(targetNode);
            //H_Heuristica = this.info.Distance(targetNode.info, DistanceType.Euclidean);
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
        public bool EqualsNode(Node other)
        {
            return this.info.x == other.info.x && this.info.y == other.info.y;
        }
    }
}
