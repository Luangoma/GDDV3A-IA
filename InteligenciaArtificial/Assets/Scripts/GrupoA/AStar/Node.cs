using Navigation.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GrupoA.AStar
{
    public class Node
    {
        public CellInfo info;
        public Node padre;
        public int steps;

        public Node(CellInfo info, int steps = 0, Node padre = null)
        {
            this.info = info;
            this.padre = padre;
            this.steps = steps + 1;
        }
    }
}
