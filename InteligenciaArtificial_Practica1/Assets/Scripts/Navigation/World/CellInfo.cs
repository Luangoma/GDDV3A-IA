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

using UnityEngine;

namespace Navigation.World
{
    public class CellInfo
    {
        public enum CellType
        {
            None,
            Floor,
            Wall,
            Limit,
            Treasure,
            Enemy,
            Exit
        }

        public CellInfo(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.Type = CellType.None;
        }
        
        public GameObject GameObject;
        public CellType Type;
        public readonly int x;
        public readonly int y;
        
        public bool Walkable
        {
            get
            {
                return Type != CellType.None && Type != CellType.Wall && Type != CellType.Limit; 
            }
        }

        #region Comparison operators
        public override bool Equals(object obj)
        {
            return Equals(obj as CellInfo);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public bool Equals(CellInfo other)
        {
            return x == other.x && y == other.y;
        }

        public static bool operator ==(CellInfo a, CellInfo b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null || (object)b == null) return false;
            
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(CellInfo a, CellInfo b)
        {
            return !(a == b);
        }

        #endregion
        
        #region Distance calculation
        public enum DistanceType
        {
            Euclidean
        }
        
        public float Distance(CellInfo other, DistanceType distanceType)
        {
            switch (distanceType)
            {
                case DistanceType.Euclidean:
                    return EuclideanDistance(other);
                default:
                    return 0.0f;
            }
        }
        
        private float EuclideanDistance(CellInfo other)
        {
            return Mathf.Sqrt(Mathf.Pow(x - other.x, 2) + Mathf.Pow(y - other.y, 2));
        }
        #endregion

        public Color CellColor()
        {
            switch (Type)
            {
                case CellType.None:
                    return Color.gray;
                case CellType.Floor:
                    return Color.green;
                case CellType.Wall:
                    return Color.red;
                case CellType.Limit:
                    return new Color(0.0f, 0.8f, 0.0f);
                case CellType.Treasure:
                    return Color.blue;
                case CellType.Exit:
                    return Color.yellow;
                case CellType.Enemy:
                    return new Color(0.6078f, 0.1490f, 0.7137f);
                default:
                    return Color.black;
            }
        }

        public override string ToString()
        {
            return $"({x}, {y}) => {Type.ToString()}";
        }
    }
}