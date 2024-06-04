using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.GrupoA
{
    internal class QTable
    {
        // Estructuras simples, almacen de estado
        struct QState
        {
            //Accion, distancia y orientacion respecto el enemigo
            char action;
            int distance;
            int orientation;

            public QState(char action, int distance, int orientation)
            {
                this.action = action;
                this.distance = distance;
                this.orientation = orientation;
            }
        }
        #region Variables, a lot of variables
        private Dictionary<QState, float[]> qTable = new Dictionary<QState, float[]>();
        private QMindTrainerParams _params;
        private float _learningRate { get => _params.alpha; } // Alpha
        private float _discountFactor { get => _params.gamma; } // Gamma
        private float _explorationRate { get => _params.epsilon; } // Epsilon
        private int _episodes { get => _params.episodes; }
        private int _episodesBetweenSaves { get => _params.episodesBetweenSaves; }
        private int _maxSteps { get => _params.maxSteps; }
        private int _currentEpisode;
        private INavigationAlgorithm _navigationAlgorithm;
        private System.Random _random = new System.Random();
        private float _distances;
        private int _distanceSegments = 3;
        private int _angleSegments = 8;
        private const int _negativeReward = -100;
        private const int _positiveReward = 5;
        private bool _isLearning = false;
        private bool _memory = true;
        private QState _previousState;
        #endregion

        public QTable(QMindTrainerParams qMindTrainerParams, Vector2Int worldSize, INavigationAlgorithm navigationAlgorithm, bool isLearning, bool memory)
        {
            _params = qMindTrainerParams;
            _distances = ((worldSize[0] + worldSize[1]) / 2) / 4;
            _navigationAlgorithm = navigationAlgorithm;
            _isLearning = isLearning;
            _memory = memory;
        }
        // INCOMPLETO
        public void DoStep(CellInfo agent, CellInfo other)
        {
            // Expandir cellinfo
            CellInfo[] posibles = ExpandCellInfo(agent);
            // Elegir uno respecto al epsilon
            CellInfo randomCell = RandomCellInfo(posibles);
            // Camino al enemigo
            CellInfo[] camino = _navigationAlgorithm.GetPath(agent, other, 2);
            CellInfo newo = camino[0];
            // Comprobar la lista
            if (camino?.Length > 0)
            {
                newo = camino[camino.Length - 1];
            }
            // Calculamos el cuadrante el cual pertenece el enemigo respecto el agente
            float angleBetweenAgents = (float)(Math.Atan2((agent.x - other.x), (agent.y - other.y)) * (180.0 / Math.PI));
            // unity implementa una funcion para saber los angulos entre dos elementos, revisar docuentacion, para saber como lo implementa correctametne
            //Vector2.SignedAngle()
            // Calculamos la distancia manhattan respecto el agente y el enemigo
            float distanceBetweenAgents = agent.Distance(other, CellInfo.DistanceType.Manhattan);
            // Recompensa que se le da el jugador si la jugada es correcta*
            float reward = (newo.Walkable && agent.x != other.x && agent.y != other.y) ? _positiveReward : _negativeReward;
            // tempInCelsius < 20.0 ? "Cold." : "Perfect!";
            updateWithReward(agent, newo, angleBetweenAgents, distanceBetweenAgents, reward);
            //guardar tabla
            SaveQTable();
        }
        //// REVISAR
        private CellInfo RandomCellInfo(CellInfo[] posibles)
        {
            CellInfo final = posibles[0];
            int i = 0;
            while (_random.NextDouble() <= _explorationRate)
            {
                i++;
                final = posibles[i % posibles.Length];
            }
            return final;
        }
        //// REVISAR
        public void InitializeEpisode()
        {
            if (_memory) { ReadQTable(); } else { initQTable(); }
        }
        #region Cells
        /* GetCellInfos *
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
        //*/
        /* ExpandCellInfo *
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
        //*/
        /* ExpandWalkablesCellInfo *
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
        //*/
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
        private int nextAction(QState state)
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
            /**
            List<float> rewards = new List<float>();
            CellInfo[] cells = ExpandCellInfo(state);
            foreach (var cell in cells)
            {
                rewards.Add(qTable[state]);
            }
            return rewards.Max();
            //*/
            // Leemos los valores (recompensas) de las posibles acciones
            float[] rewards = qTable[state];
            // Selecionamos la recompensa de mayor valor
            float maxValue = rewards.Max();
            // Buscamos el indice de la recompensa de mayor valor
            int idx = rewards.ToList().IndexOf(maxValue);
            // Devolvemos el indice con la accion de mayor valor
            return idx;
        }
        //// HECHO
        private void updateWithReward(char action, float realDistance, float realAngle, float reward)
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
            // 
            int distance = DiscretizeDistance(realDistance);
            int angle = DiscretizeAngle(realAngle);
            QState newState = new QState(action, distance, angle); // Asignar parametros, completar
            int idxNewStateValue = nextAction(newState);
            qTable[newState][idxNewStateValue] = (1 - _learningRate) * qTable[_previousState][idxNewStateValue] + _learningRate * (reward + _discountFactor * nextAction(newState));
        }
        #endregion
        #region Init-Read-Save
        // REVISAR
        private void initQTable()
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
        public void SaveQTable()
        {
            string name = "Qlearning_" + _currentEpisode + ".csv"; // Nombre del fichero
            StreamWriter file = File.CreateText(name); // Creamos un fichero
            //file = File.AppendText("prueba.txt"); // Se anade texto al fichero existente
            file.WriteLine("Tabla Q");
            foreach (var cellInfo in qTable.Values)
            {
                file.WriteLine(cellInfo); // Lo mismo que cuando escribimos por consola
            }
            file.Close(); // Al cerrar el fichero nos aseguramos que no queda ning�n dato por guardar
            Debug.Log(Directory.GetCurrentDirectory() + " : " + name);
        }
        // HACER
        private void ReadQTable()
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
