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
using Navigation.Interfaces;
using Navigation.World;
using UnityEngine;

namespace Navigation.Agent
{
    public class BaseSearchAgent : INavigationAgent
    {
        public CellInfo CurrentObjective { get; private set; }
        public Vector3 CurrentDestination { get; private set; }
        public int NumberOfDestinations { get; private set; }

        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm;

        private CellInfo _origin;
        private CellInfo[] _objectives;
        private Queue<CellInfo> _path;
        
        
        public BaseSearchAgent(WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm, CellInfo startPosition)
        {
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _origin = startPosition;
            
            _navigationAlgorithm.Initialize(_worldInfo, INavigationAlgorithm.AllowedMovements.FourDirections);
        }
        
        public Vector3? GetNextDestination()
        {
            if (_objectives == null)
            {
                _objectives = GetDestinations();
                CurrentObjective = _objectives[_objectives.Length - 1];
                NumberOfDestinations = _objectives.Length;
            }
            
            if (_path == null || _path.Count==0)
            {
                CellInfo[] path = _navigationAlgorithm.GetPath(_origin, CurrentObjective);
                _path = new Queue<CellInfo>(path); 
            }

            if (_path.Count > 0)
            {
                CellInfo destination = _path.Dequeue();
                _origin = destination;
                CurrentDestination = _worldInfo.ToWorldPosition(destination);
            }
            
            return CurrentDestination;
        }

        private CellInfo[] GetDestinations()
        {
            List<CellInfo> targets = new List<CellInfo>();
            targets.Add(_worldInfo.Exit);
            return targets.ToArray();
        }
    }
}