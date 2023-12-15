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

using System;
using Navigation.Algorithms.RandomMovement;
using Navigation.Interfaces;
using Navigation.World;
using UnityEngine;

namespace Components
{
    public class EnemyMovement : MonoBehaviour
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        
        public float speed = 1.0f;
        public int minimumSteps = 3;
        public int maximumSteps = 8;
        
        private Animator _animator;

        private RandomMovement.Directions _currentDirection = RandomMovement.Directions.None;
        private Vector3 _currentDestination;
        
        private bool _destinationReached = false;
        private int steps;

        private RandomMovement _navigationAlgorithm;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.SetFloat(MoveSpeed, 0f);
        }

        private void Start()
        {
            _navigationAlgorithm = new RandomMovement();
            _navigationAlgorithm.Initialize(WorldManager.Instance.WorldInfo, INavigationAlgorithm.AllowedMovements.FourDirections);
        }

        private void Update()
        {
            Vector3 currentPosition = transform.position;

            if (_currentDirection == RandomMovement.Directions.None || steps==0)
            {
                steps = UnityEngine.Random.Range(minimumSteps, maximumSteps + 1);
                _currentDirection =  _navigationAlgorithm.GetRandomDirection();
                _destinationReached = true;
                // Debug.Log($"{gameObject.name}: Enemy changed direction to {_currentDirection}");
            }
            
            if(_destinationReached)
            {
                CellInfo currentCell = WorldManager.Instance.WorldInfo.FromVector3(currentPosition);
                CellInfo targetCell = _navigationAlgorithm.GetNeighbour(currentCell, _currentDirection);
                _currentDestination = WorldManager.Instance.WorldInfo.ToWorldPosition(targetCell);
                
                if(!targetCell.Walkable)
                {
                    // Debug.Log($"{gameObject.name}:Enemy destination reached, but cell {targetCell} is not walkable");
                    _currentDirection = RandomMovement.Directions.None;
                    return;
                }

                // Debug.Log($"{gameObject.name}: Destination reached, moving to {targetCell}");
                _destinationReached = false;
            }
            
            Vector3 direction = (_currentDestination - transform.position).normalized;
            Vector3 move = direction * (speed * Time.deltaTime);
            Vector3 lookDirection = new Vector3(direction.x, 0, direction.z) + transform.position;
            
            transform.LookAt(lookDirection);
            transform.position += move;
            
            _animator.SetFloat(MoveSpeed, speed);
            
            if(Vector3.Distance(_currentDestination, transform.position) < 0.2f)
            {
                steps--;
                _destinationReached = true;
                WorldManager.Instance.DebugWorld();
            }
        }
    }
}