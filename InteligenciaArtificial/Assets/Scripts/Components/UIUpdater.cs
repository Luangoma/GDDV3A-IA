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

using TMPro;
using UnityEngine;

namespace Components
{
    public class UIUpdater : MonoBehaviour
    {
        private NavigationMovement _navigationMovement;
        private TextMeshProUGUI _objective;
        private TextMeshProUGUI _destination;

        private void Awake()
        {
            _navigationMovement = FindObjectOfType<NavigationMovement>();

            if (_navigationMovement != null)
            {
                _navigationMovement.OnGameStatsChanged += OnGameStatsChanged;    
            }
            
            _objective = transform.Find("targets").GetComponent<TextMeshProUGUI>();
            _destination = transform.Find("destination").GetComponent<TextMeshProUGUI>();
        }

        private void OnGameStatsChanged(object sender, NavigationMovement.GameStats stats)
        {
            if(_objective != null)
            {
                _objective.text = GetTargetsString(stats);
            }
            
            if(_destination != null)
            {
                _destination.text = GetDestinationString(stats);
            }
        }
        
        private string GetTargetsString(NavigationMovement.GameStats gameStats)
        {
            return $"Objectivo actual: {gameStats.currentTarget} ({gameStats.currentTarget.GameObject?.name ?? ""}) " +
                   $"Remaining targets: [{gameStats.remainingTargets}]";
        }

        private string GetDestinationString(NavigationMovement.GameStats gameStats)
        {
            return $"Destino actual: {gameStats.currentDestination}";
        }
    }
}