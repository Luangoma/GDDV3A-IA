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
using Navigation.Agent;
using Navigation.Interfaces;
using Navigation.World;
using UnityEngine;

namespace Components
{
    public class NavigationMovement : MonoBehaviour
    {
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        
        public float speed = 0.05f;
        public string navigationAgent;
        public string navigationClass;
        
        private INavigationAlgorithm _navigator;
        private INavigationAgent _searchAgent;
        
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.SetFloat(MoveSpeed, 0f);
            _animator.SetBool(Grounded, true);
        }
        
        private void Start()
        {
            _navigator = (INavigationAlgorithm) Activator.CreateInstance(Type.GetType(navigationClass) 
                ?? throw new InvalidOperationException());
            
            // _searchAgent = new BaseSearchAgent(WorldManager.Instance.WorldInfo, _navigator, 
            // WorldManager.Instance.WorldInfo.FromVector3(transform.position));
            _searchAgent = (INavigationAgent) Activator.CreateInstance(Type.GetType(navigationAgent)
                           ?? throw new InvalidOperationException(), new object[]
            {
                WorldManager.Instance.WorldInfo, _navigator, 
                WorldManager.Instance.WorldInfo.FromVector3(transform.position)
            });
        }
        
        private Vector3? _currentDestination = null;
        
        private void Update()
        {
            if (_searchAgent == null)
            {
                return;
            }

            if (_currentDestination == null)
            {
                _currentDestination = _searchAgent.GetNextDestination();
                UpdateStats();
                
                if(_currentDestination == null)
                {
                    return;
                }
            }
            
            Vector3 destination = _currentDestination.Value;
            Vector3 direction = (destination - transform.position).normalized;
            Vector3 move = direction * (speed * Time.deltaTime);
            Vector3 lookDirection = new Vector3(direction.x, 0, direction.z) + transform.position;
            
            transform.LookAt(lookDirection);
            transform.position += move;
            _animator.SetFloat(MoveSpeed, speed);
            _animator.SetBool(Grounded, true);
            
            if(Vector3.Distance(destination, transform.position) < 0.2f)
            {
                _currentDestination = null;
            }
        }

        #region Statistics update
        public EventHandler<GameStats> OnGameStatsChanged;
        
        public struct GameStats
        {
            public int remainingTargets;
            public CellInfo currentTarget;
            public Vector3 currentDestination;
        }

        private void UpdateStats()
        {
            GameStats stats = new GameStats();
            stats.currentTarget = _searchAgent.CurrentObjective;
            stats.remainingTargets = _searchAgent.NumberOfDestinations;
            stats.currentDestination = _searchAgent.CurrentDestination;
            OnGameStatsChanged?.Invoke(this, stats);
        }
        #endregion
    }
}