using System;
using UnityEngine;

namespace ScottBarley.IGB283.Assessment2
{
    /// <summary>
    /// IS: Custom 3D vector class with basic vector math operations.
    /// 
    /// NOTE: IGB283 Instructions - Write C# code that implements your own version of the typical vector and 
    /// transform functions. Typical vector functions include vector constructor, addition, subtraction, 
    /// negation, dot product and cross product etc.
    /// 
    /// Based on Scott Barley CAB201 Assignment 1 & IGB283 Workshop material 
    /// </summary>
    [Serializable]
    public class IGB283Vector : IComparable<IGB283Vector>, IEquatable<IGB283Vector>
    {
        public float x;
        public float y;
        public float z;

        #region Constructors
        /// <summary>
        /// Constructor (Vector3)
        /// </summary>
        /// <param name="x">X component</param>
        /// <param name="y">Y component</param>
        /// <param name="z">Z component</param>
        public IGB283Vector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Indexed constructor - So that values are indexed as in Vector3.
        /// </summary>
        public IGB283Vector(IGB283Vector v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        /// <summary>
        /// Empty constructor: IGB283Vector(0,0,0)
        /// </summary>
        public IGB283Vector()
        {
            x = 0f;
            y = 0f;
            z = 0f;
        }


        #endregion

        #region Implicit conversion
        // (IGB283Vector : Vector3)
        /// <summary>
        /// Implicit conversion from UnityEngine.Vector3 to IGB283Vector.
        /// </summary>
        public static implicit operator IGB283Vector(Vector3 v) =>
            new IGB283Vector(v.x, v.y, v.z);

        /// <summary>
        /// Implicit conversion from IGB283Vector to UnityEngine.Vector3.
        /// </summary>
        public static implicit operator Vector3(IGB283Vector v) =>
            new Vector3(v.x, v.y, v.z);

        // (IGB283Vector : Vector2)

        /// <summary>
        /// Implicit conversion from UnityEngine.Vector2 to IGB283Vector.
        /// </summary>
        public static implicit operator IGB283Vector(Vector2 v) =>
            new IGB283Vector(v.x, v.y, 0);

        /// <summary>
        /// Implicit conversion from IGB283Vector to UnityEngine.Vector2
        /// </summary>
        public static implicit operator Vector2(IGB283Vector v) =>
            new Vector2(v.x, v.y);
        
        #endregion

        #region Indexer
        /// <summary>
        /// Indexer to access vector components by index,
        /// added to be similar to Vector3 & to allow use with Matrix3x3 class
        /// 0 = x, 1 = y, 2 = z
        /// </summary>
        public float this[int index]
        {
            get
            {
                return index switch
                {
                    0 => x,
                    1 => y,
                    2 => z,
                    _ => throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0, 1, or 2")
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0, 1, or 2");
                }
            }
        }
        #endregion

        #region Math Operations
        /// <summary>
        /// Vector addition
        /// </summary>
        public static IGB283Vector Addition(IGB283Vector a, IGB283Vector b) => new IGB283Vector(a.x + b.x, a.y + b.y, a.z + b.z);

        /// <summary>
        /// Vector subtraction
        /// </summary>
        public static IGB283Vector Subtraction(IGB283Vector a, IGB283Vector b) => new IGB283Vector(a.x - b.x, a.y - b.y, a.z - b.z);

        /// <summary>
        /// Negation (-v)
        /// </summary>
        public static IGB283Vector Negation(IGB283Vector v) => new IGB283Vector(-v.x, -v.y, -v.z);

        /// <summary>
        /// Dot product of two vectors
        /// </summary>
        public static float Dot(IGB283Vector a, IGB283Vector b) =>
            a.x * b.x + a.y * b.y + a.z * b.z;

        /// <summary>
        /// Cross product of two vectors
        /// </summary>
        public static IGB283Vector Cross(IGB283Vector a, IGB283Vector b) =>
            new IGB283Vector(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x
            );

        /// <summary>
        /// Linear interpolation between two vectors
        /// </summary>
        /// <param name="a">Start vector</param>
        /// <param name="b">End vector</param>
        /// <param name="t">Interpolation factor (0 = a, 1 = b)</param>
        public static IGB283Vector Lerp(IGB283Vector a, IGB283Vector b, float t)
        {
            t = Mathf.Clamp01(t);
            return new IGB283Vector(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t,
                a.z + (b.z - a.z) * t
            );
        }

        /// <summary>
        /// Normalized vector
        /// </summary>
        public IGB283Vector Normalized
        {
            get
            {
                float mag = Magnitude;
                return mag > float.MinValue ? new IGB283Vector(x / mag, y / mag, z / mag) : Zero;
            }
        }

        /// <summary>
        /// Scalar Magnitude
        /// </summary>
        public float Magnitude => Mathf.Sqrt(x * x + y * y + z * z);

        /// <summary>
        /// Scalar multiplication
        /// </summary>
        public static IGB283Vector Multiply(IGB283Vector v, float scalar) => new IGB283Vector(v.x * scalar, v.y * scalar, v.z * scalar);

        /// <summary>
        /// Scalar division
        /// </summary>
        public static IGB283Vector Divide(IGB283Vector v, float scalar) => new IGB283Vector(v.x / scalar, v.y / scalar, v.z / scalar);

        /// <summary>
        /// Scalar Distance between two vectors
        /// </summary>
        public static float Distance(IGB283Vector a, IGB283Vector b) => (a - b).Magnitude;
        #endregion

        #region Overrides
        /// <summary>
        /// Vector addition
        /// </summary>
        public static IGB283Vector operator +(IGB283Vector a, IGB283Vector b) => Addition(a, b);

        /// <summary>
        /// Vector subtraction
        /// </summary>
        public static IGB283Vector operator -(IGB283Vector a, IGB283Vector b) => Subtraction(a, b);

        /// <summary>
        /// Negation (-v)
        /// </summary>
        public static IGB283Vector operator -(IGB283Vector v) => Negation(v);

        /// <summary>
        /// Scalar multiplication
        /// </summary>
        public static IGB283Vector operator *(IGB283Vector v, float scalar) => Multiply(v, scalar);

        /// <summary>
        /// Scalar division
        /// </summary>
        public static IGB283Vector operator /(IGB283Vector v, float scalar) => Divide(v, scalar);

        /// <summary>
        /// Compares IGB283Vector X, Y & Z values
        /// </summary>
        public static bool operator ==(IGB283Vector values1, IGB283Vector values2)
        {
            if (ReferenceEquals(values1, values2)) return true;
            if (values1 is null || values2 is null) return false;
            return values1.x == values2.x && values1.y == values2.y && values1.z == values2.z;
        }

        /// <summary>
        /// Compares IGB283Vector X, Y & Z values
        /// </summary>
        public static bool operator !=(IGB283Vector coordinates1, IGB283Vector coordinates2) => !(coordinates1 == coordinates2);

        /// <summary>
        /// Compares IGB283Vector X & values
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is IGB283Vector vec) return Equals(vec);
            return false;
        }

        /// <summary>
        /// Hash based on IGB283Vector X,Y & Z values
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(x, y, z);

        /// <summary>
        /// Returns IGB283Vector as a string (x, y, z)
        /// </summary>
        public override string ToString() => $"IGB283Vector({x}, {y}, {z})";
        #endregion

        #region Static IGB283Vector Values
        /// <summary>
        /// Zero vector (0,0,0)
        /// </summary>
        public static IGB283Vector Zero => new IGB283Vector(0f, 0f, 0f);
        
        /// <summary>
        /// Returns new (0,1,0)
        /// </summary>
        public static IGB283Vector Up => new IGB283Vector(0f, 1f, 0f);
        
        /// <summary>
        /// Returns new (0,-1, 0)
        /// </summary>
        public static IGB283Vector Down => new IGB283Vector(0f, -1f, 0f);
        
        /// <summary>
        /// Returns new (1, 0 ,0)
        /// </summary>
        public static IGB283Vector Right => new(1f, 0f, 0f);
        
        /// <summary>
        /// Returns new (-1,0, 0)
        /// </summary>
        public static IGB283Vector Left => new(-1f, 0f, 0f);

        /// <summary>
        /// Returns new (0, 0 ,1)
        /// </summary>
        public static IGB283Vector Foward => new(0f, 0f, 1f);

        /// <summary>
        /// Returns new (0,0, -1)
        /// </summary>
        public static IGB283Vector Backward => new(0f, 0f, -1f);

        /// <summary>
        /// Returns Reserved Coordinate
        /// </summary>
        public static IGB283Vector ErrorReservedValue => new(float.MinValue, float.MinValue, float.MinValue);
        #endregion

        #region Interface Implmentations
        // IComparable
        /// <summary>
        /// Compare this vector to another vector.
        /// </summary>
        /// <param name="other">The other vector to compare against.</param>
        /// <returns>
        /// A value less than zero if this vector is less than the other.
        /// Zero if they are equal.
        /// Greater than zero if this vector is greater.
        /// </returns>
        public int CompareTo(IGB283Vector? other)
        {
            if (other is null) return 1;

            int compareX = x.CompareTo(other.x);
            if (compareX != 0) return compareX;

            int compareY = y.CompareTo(other.y);
            if (compareY != 0) return compareY;

            return z.CompareTo(other.z);
        }

        // IEquatable
        /// <summary>
        /// Compares Coordinate X & Y values
        /// </summary>
        public bool Equals(IGB283Vector? other)
        {
            if (other is null) return false;
            return x == other.x && y == other.y && z == other.z;
        }
        #endregion


    }
}