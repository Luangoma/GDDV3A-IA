#region Copyright
// MIT License
// 
// Copyright (c) 2023 David María Arribas
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Navigation.World
{
    public class WorldInfo
    {
        private float _resolution;
        
        private GameObject[] _walls;
        private GameObject[] _objectives;
        private GameObject[] _enemies;
        private GameObject[] _floor;
        private GameObject _exit;
        
        private GameObject _worldBase;
        private Vector2Int _worldSize;

        public CellInfo Exit => GetExit();
        public CellInfo[] Targets => GetObjectives();
        public CellInfo[] Enemies => GetEnemies();
        public CellInfo this[int x, int y] => getCellAt(x, y);
        public Vector2Int WorldSize => _worldSize;

        public WorldInfo(GameObject worldBase, float resolution = 1.0f)
        {
            _worldBase = worldBase;
            _resolution = resolution;
            InitWorld();
        }
        
        public CellInfo FromVector3(Vector3 vector3)
        {
            float x = vector3.x;
            float y = vector3.z;

            x = Mathf.Floor(x) + 0.5f;
            y = Mathf.Floor(y) + 0.5f;
        
            return getCellAt((int) (x / _resolution), (int) (y / _resolution));
        }

        public Vector3 ToWorldPosition(CellInfo cellInfo)
        {
            float x = cellInfo.x;
            float z = cellInfo.y;

            x = Mathf.Floor(x) + 0.5f;
            z = Mathf.Floor(z) + 0.5f;
            
            return new Vector3(x * _resolution, 0f, z * _resolution);
        }

        public void ResetObjects()
        {
            _walls = _worldBase.FindWithTag("Wall");
            _objectives = _worldBase.FindWithTag("Treasure");
            _enemies = _worldBase.FindWithTag("Enemy");
            _exit = _worldBase.FindWithTag("Exit")[0];
            Debug.Log("World reset");
        }
        
        #region Private methods
        private void InitWorld()
        {
            _floor = _worldBase.FindWithTag("Floor");
            
            Bounds currentFloorBounds = _floor[0].GetComponent<Renderer>().bounds;
            _worldSize = new Vector2Int((int)(currentFloorBounds.size.x / _resolution), 
                (int)(currentFloorBounds.size.z / _resolution));
            
            ResetObjects();
        }

        private CellInfo[] GetObjectives()
        {
            List<CellInfo> objectives = new List<CellInfo>();
            
            foreach (GameObject objective in _objectives)
            {
                Vector3 position = objective.transform.position;
                CellInfo cellInfo = FromVector3(position);
                objectives.Add(cellInfo);
            }
            
            return objectives.ToArray();
        }
        
        private CellInfo[] GetEnemies()
        {
            List<CellInfo> objectives = new List<CellInfo>();
            
            foreach (GameObject objective in _enemies)
            {
                Vector3 position = objective.transform.position;
                CellInfo cellInfo = FromVector3(position);
                objectives.Add(cellInfo);
            }
            
            return objectives.ToArray();
        }

        private CellInfo GetExit()
        {
            Vector3 position = _exit.transform.position;
            CellInfo cellInfo = FromVector3(position);
            cellInfo.Type = CellInfo.CellType.Exit;
            return cellInfo;
        }
        
        private CellInfo getCellAt(int x, int y)
        {
            CellInfo cellInfo = new CellInfo(x, y);
            cellInfo.Type = getCellType(cellInfo, out GameObject gameObject);
            cellInfo.GameObject = gameObject;
            return cellInfo;
        }

        private CellInfo.CellType getCellType(CellInfo cellInfo, out GameObject gameObject)
        {
            return getCellType(cellInfo.x, cellInfo.y, out gameObject);
        }

        private CellInfo.CellType getCellType(int x, int y, out GameObject gameObject)
        {
            if (IsLimit(x, y))
            {
                gameObject = null;
                return CellInfo.CellType.Limit;
            } else if (IsWall(x, y, out gameObject))
            {
                return CellInfo.CellType.Wall;
            } else if (IsObjective(x, y, out gameObject))
            {
                return CellInfo.CellType.Treasure;
            } else if(IsExit(x, y, out gameObject))
            {
                return CellInfo.CellType.Exit;
            } else if (IsEnemy(x, y, out gameObject))
            {
                return CellInfo.CellType.Enemy;
            }

            gameObject = null;
            return CellInfo.CellType.Floor;
        }

        private bool IsExit(int x, int y, out GameObject exitGameObject)
        {
            Bounds exitBounds = _exit.GetComponent<Collider>().bounds;
            exitBounds.Encapsulate(new Vector3(Mathf.Floor(exitBounds.min.x), 
                Mathf.Floor(exitBounds.max.y), Mathf.Floor(exitBounds.min.z)));
            exitBounds.Encapsulate(new Vector3(Mathf.Floor(exitBounds.max.x+1), 
                Mathf.Floor(exitBounds.max.y+1), Mathf.Floor(exitBounds.max.z+1)));
            
            exitGameObject = _exit;
            return exitBounds.Contains(new Vector3((x + 0.5f) * _resolution, 0.5f, (y + 0.5f) * _resolution));
        }

        private bool IsLimit(int x, int y)
        {
            return x < 0 || x >= _worldSize.x || y < 0 || y >= _worldSize.y;
        }

        private bool IsWall(int x, int y, out GameObject wallGameObject)
        {
            foreach (var wall in _walls)
            {
                Bounds wallBounds = wall.GetComponent<Collider>().bounds;
                wallBounds.Encapsulate(new Vector3(Mathf.Floor(wallBounds.min.x), 
                    Mathf.Floor(wallBounds.max.y), Mathf.Floor(wallBounds.min.z)));
                
                wallBounds.Encapsulate(new Vector3(Mathf.Floor(wallBounds.max.x+1), 
                    Mathf.Floor(wallBounds.max.y+1), Mathf.Floor(wallBounds.max.z+1)));
                
                if (wallBounds.Contains(new Vector3((x + 0.5f) * _resolution, 0.5f, (y + 0.5f) * _resolution)))
                {
                    wallGameObject = wall;
                    return true;
                }
            }

            wallGameObject = null;
            return false;
        }

        private bool IsObjective(int x, int y, out GameObject objectiveGameObject)
        {
            foreach (GameObject objective in _objectives)
            {
                if(objective.activeSelf == false) continue;
                
                Bounds objectiveBounds = objective.GetComponent<Collider>().bounds;
                objectiveBounds.Encapsulate(new Vector3(Mathf.Floor(objectiveBounds.min.x), 
                    Mathf.Floor(objectiveBounds.max.y), Mathf.Floor(objectiveBounds.min.z)));
                
                objectiveBounds.Encapsulate(new Vector3(Mathf.Floor(objectiveBounds.max.x+1), 
                    Mathf.Floor(objectiveBounds.max.y+1), Mathf.Floor(objectiveBounds.max.z+1)));
                
                if (objectiveBounds.Contains(new Vector3(x + 0.5f, 0.5f, y + 0.5f)))
                {
                    objectiveGameObject = objective;
                    return true;
                }
            }

            objectiveGameObject = null;
            return false;
        }
        
        private bool IsEnemy(int x, int y, out GameObject enemyGameObject)
        {
            foreach (GameObject enemy in _enemies)
            {
                if(enemy.activeSelf == false) continue;
                
                Bounds enemyBounds = enemy.GetComponent<Collider>().bounds;
                enemyBounds.Encapsulate(new Vector3(Mathf.Floor(enemyBounds.min.x), 
                    Mathf.Floor(enemyBounds.max.y), Mathf.Floor(enemyBounds.min.z)));
                
                enemyBounds.Encapsulate(new Vector3(Mathf.Floor(enemyBounds.max.x+1), 
                    Mathf.Floor(enemyBounds.max.y+1), Mathf.Floor(enemyBounds.max.z+1)));
                
                if (enemyBounds.Contains(new Vector3(x + 0.5f, 0.5f, y + 0.5f)))
                {
                    enemyGameObject = enemy;
                    return true;
                }
            }

            enemyGameObject = null;
            return false;
        }
        #endregion
    }
}