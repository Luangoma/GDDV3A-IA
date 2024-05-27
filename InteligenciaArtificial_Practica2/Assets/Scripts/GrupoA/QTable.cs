using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GrupoA
{
    internal class QTable
    {

        #region Variables, a lot of variables
        private bool _initialized = false;
        // Posicion,accion y distancia,orientacion del enemigo
        private Dictionary<(CellInfo, CellInfo, int, int), float> qTable = new Dictionary<(CellInfo, CellInfo, int, int), float>();
        private QMindTrainerParams _params;
        private float _learningRate { get => _params.alpha; } // Alpha
        private float _discountFactor { get => _params.gamma; } // Gamma
        private float _explorationRate { get => _params.epsilon; } // Epsilon
        private int _episodes { get => _params.episodes; }
        private int _maxSteps { get => _params.maxSteps; }
        private int _currentEpisode;
        private int _episodesBetweenSaves { get => _params.episodesBetweenSaves; }
        private INavigationAlgorithm _navigationAlgorithm;
        public WorldInfo _world;
        private float _distances { get => ((_world.WorldSize[0] + _world.WorldSize[1]) / 2) / 4; }
        private int _distanceSegments = 3;
        private int _angleSegments = 8;
        private bool _isLearning = false;
        public bool memory = true;
        #endregion

        public QTable(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm, int currentEpisode, bool isLearning)
        {
            _initialized = true;
            _params = qMindTrainerParams;
            _world = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _currentEpisode = currentEpisode;
            _isLearning = isLearning;
        }
        // HACER
        private void InitializeEpisode()
        {
            if (memory) { ReadQTable(); } else { initQTable(); }

        }
        #region Cells
        //// HECHO
        private CellInfo[] GetCellInfos()
        {
            Vector2 dimensions = _world.WorldSize;
            List<CellInfo> cells = new List<CellInfo>();
            for (int x = 0; x < dimensions[0]; x++)
            {
                for (int y = 0; y < dimensions[1]; y++)
                {
                    cells.Add(new CellInfo(x, y));
                }
            }
            return cells.ToArray();
        }
        //// HECHO
        private CellInfo[] ExpandCellInfo(CellInfo initState)
        {
            CellInfo[] cells = { // Expansion de la celda
                _world[initState.x, initState.y+1], // Arriba
                _world[initState.x+1, initState.y], // Derecha
                _world[initState.x, initState.y-1], // Abajo
                _world[initState.x-1, initState.y]  // Izquierda
            };
            return cells;
        }
        //// HECHO
        private CellInfo[] ExpandWalkablesCellInfo(CellInfo initState)
        {
            CellInfo[] cells = ExpandCellInfo(initState);
            List<CellInfo> cellList = new List<CellInfo>();
            foreach (var cell in cells)
            {
                if (cell.Walkable)
                {
                    cellList.Add(cell);
                }
            }
            return cellList.ToArray();
        }
        //// HECHO
        private bool Terminal(CellInfo cellA, CellInfo cellB)
        {
            return cellA == cellB;
        }
        //// HECHO
        private int DiscretizeAngle(float angle)
        {
            return (int)(angle / (360 / _angleSegments));
        }
        //// HECHO
        private int DiscretizeDistance(float dist)
        {
            if (dist < _distances)
            {
                return 0;
            }
            else if (dist < _distances * 3)
            {
                return 1;
            }
            else { return 2; }
        }
        //// HECHO
        private float ManhattanDistance(CellInfo player, CellInfo enemy)
        {
            return (Math.Abs(player.x - enemy.x) + Math.Abs(player.y - enemy.y));
        }
        #endregion
        #region Rewards
        // REVISAR
        private float NextStateValue(CellInfo state, CellInfo nextState, int angle, int distance)
        {
            /**
            CellInfo[] cells = { // Expansion de la celda
                    _world[state.x, state.y+1], // Arriba
                    _world[state.x+1, state.y], // Derecha
                    _world[state.x, state.y-1], // Abajo
                    _world[state.x-1, state.y]  // Izquierda
                };
            List<CellInfo> rewards = new List<CellInfo>();
            foreach (var cell in cells)
            {
                if (cell.Walkable)
                {
                    rewards.Add(cell);
                }
            }
            List<float> cost = new List<float>();
            foreach (var cell in rewards)
            {
                cost.Add(qTable[(nextState, cell)]);
            }
            List<float> rewards = new List<float>();
            foreach (var cell in cells)
            {
                if (cell.Walkable)
                {
                    rewards.Add(qTable[(nextState, cell)]);
                }
            }
            //*/
            List<float> rewards = new List<float>();
            CellInfo[] cells = ExpandCellInfo(state);
            foreach (var cell in cells)
            {
                rewards.Add(qTable[(nextState, cell, angle, distance)]);
            }
            return rewards.Max();
        }
        //// HECHO
        private void updateWithReward(CellInfo state, CellInfo action, float realAngle, float realDistance, float reward, CellInfo nextState)
        {
            /**
            qTable = Q[state, action] + _learningRate * (reward + _discountFactor * Enumerable.Range
                (0, Q.GetLength(1)).Select(i => Q[nextState, i]).Max() - Q[state, action]);
            qTable = Q[state, action] + _learningRate * (reward + _discountFactor * np.max(Q[s_,:]) - Q[s, a]);

            var key = Tuple.Create(state, action);
            qTable[key] = qTable.ContainsKey(key) ? Q[key] : 0;
            qTable[key] += _learningRate * (reward + _discountFactor * Q.Where(x => x.Key.Item1 == nextState).Max(x => x.Value) - qTable[key]);
            Si fuera un array:
            Q[s, a] = Q[s, a] + lr * (r + y * Enumerable.Range(0, Q.GetLength(1)).Select(i => Q[nextState, i]).Max() - Q[s, a]);

            Q = qtable[currentState, direction];
            r = GetReward(nextState);
            float nextQMax = nextState != null ? qtable.GetHighestQValue(nextState) : 0;
            (1 - alpha) * Q + alpha * (r + gamma * nextQMax);
            
            //*/
            int angle = DiscretizeAngle(realAngle);
            int distance = DiscretizeDistance(realDistance);
            qTable[(state, action, angle, distance)] = (1 - _learningRate) * qTable[(state, action, angle, distance)] + _learningRate * (reward + _discountFactor * NextStateValue(state, nextState, angle, distance));
        }
        #endregion
        #region Init-Read-Save
        // REVISAR
        void initQTable()
        {
            CellInfo[] cells = GetCellInfos();
            /**
            int length = cells.Count;
            for (int x = 0; x < length; x++)
            {

                for (int y = 0; y < length; y++)
                {
                    qTable[(cells[x], cells[y])] = 0;
                }
            }
            //*/
            foreach (var cell in cells)
            {
                CellInfo[] neighbors = ExpandCellInfo(cell);
                foreach (var neighbor in neighbors)
                {
                    for (var angles = 0; angles < _angleSegments; angles++)
                    {
                        for (var distance = 0; distance < _distanceSegments; distance++)
                        {
                            qTable[(cell, neighbor, angles, distance)] = 0.0f;
                        }
                    }
                }
            }
        }
        // HACER
        void SaveQTable()
        {
            String name = "Qlearning_" + _currentEpisode + "k.csv"; // Nombre del fichero
            StreamWriter file = File.CreateText(name); // Creamos un fichero
                                                       // file = File.AppendText("prueba.txt"); // Se a�ade texto al fichero existente
            foreach (var cellInfo in qTable.Values)
            {
                file.WriteLine(cellInfo); // Lo mismo que cuando escribimos por consola
            }
            file.Close(); // Al cerrar el fichero nos aseguramos que no queda ning�n dato por guardar
        }
        // HACER
        void ReadQTable()
        {
            int episode = _currentEpisode - 20000;
            String name = "Qlearning_" + episode + "k.csv"; // Nombre del fichero
            StreamReader file = File.OpenText(name); // Leemos un fichero
            if (File.Exists(name))
            {
                while (!file.EndOfStream)
                {
                    file.ReadLine();
                }
            }
        }
        #endregion
    }
}
