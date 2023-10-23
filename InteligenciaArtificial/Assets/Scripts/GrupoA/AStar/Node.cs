using Navigation.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GrupoA.AStar
{
    internal class Node
    {
        public CellInfo celdaInfo;

        public Node() {
            
        }

        public CellInfo GetCellInfo(CellInfo celdaInfo) 
        { return celdaInfo; }
    }
}
