using System.Collections.Generic;
using UnityEngine;
using System;

namespace ScottBarley.IGB283.Assessment2
{
    /// <summary>
    /// A 3x3 matrix implementation for 2D transformations (translation, rotation, scale).
    /// Uses IGB283Vector instead of UnityEngine.Vector3.
    /// 
    /// Written for, Task:
    /// Write C# code that implements your own version of the typical vector and transform 
    /// functions.Typical vector functions include vector constructor, addition, subtraction,
    /// negation, dot product and cross product etc. Typical transformation functions include
    /// translation, rotation, and scaling using matrices. Name your own Vector class as IGB283Vector 
    /// and your own transformation class as IGB283Transform.
    /// 
    /// Based on Matrix3x3 class provided in IGB283 Workshop Material
    /// </summary>
    public class IGB283Transform
    {
        private const int MATRIXORDER = 3;
        private readonly List<IGB283Vector> m = new List<IGB283Vector>();
        private static readonly IGB283Vector zeroVector = IGB283Vector.Zero;

        #region Constructors

        /// <summary>
        /// Creates a 3x3 zero matrix.
        /// </summary>
        public IGB283Transform()
        {
            m.Add(zeroVector);
            m.Add(zeroVector);
            m.Add(zeroVector);
        }

        /// <summary>
        /// Creates a 3x3 matrix from 3 row vectors.
        /// </summary>
        private IGB283Transform(IGB283Vector r1, IGB283Vector r2, IGB283Vector r3)
        {
            m.Add(r1);
            m.Add(r2);
            m.Add(r3);
        }

        #endregion

        #region Static Matrices
        /// <summary>
        /// Identity matrix.
        /// </summary>
        public static IGB283Transform Identity
        {
            get
            {
                IGB283Transform i = new IGB283Transform();
                i.SetRow(0, new IGB283Vector(1f, 0f, 0f));
                i.SetRow(1, new IGB283Vector(0f, 1f, 0f));
                i.SetRow(2, new IGB283Vector(0f, 0f, 1f));
                return i;
            }
        }

        /// <summary>
        /// Zero matrix.
        /// </summary>
        public static IGB283Transform Zero => new IGB283Transform(zeroVector, zeroVector, zeroVector);

        #endregion

        #region Properties
        /// <summary>
        /// Determinant of the matrix.
        /// </summary>
        public float Determinant =>
            m[0].x * (m[1].y * m[2].z - m[1].z * m[2].y)
            - m[0].y * (m[1].x * m[2].z - m[1].z * m[2].x)
            + m[0].z * (m[1].x * m[2].y - m[1].y * m[2].x);
        
        /// <summary>
        /// Inverse of the matrix.
        /// </summary>
        public IGB283Transform Inverse
        {
            get
            {
                float det = Determinant;
                if (Mathf.Abs(det) < 1e-6f)
                    throw new System.InvalidOperationException("Matrix is non-invertible.");

                return (1 / det) * (new IGB283Transform(
                    new IGB283Vector(
                        (m[1].y * m[2].z - m[1].z * m[2].y),
                        -(m[1].x * m[2].z - m[1].z * m[2].x),
                        (m[1].x * m[2].y - m[1].y * m[2].x)
                    ),
                    new IGB283Vector(
                        -(m[0].y * m[2].z - m[0].z * m[2].y),
                        (m[0].x * m[2].z - m[0].z * m[2].x),
                        -(m[0].x * m[2].y - m[0].y * m[2].x)
                    ),
                    new IGB283Vector(
                        (m[0].y * m[1].z - m[0].z * m[1].y),
                        -(m[0].x * m[1].z - m[0].z * m[1].x),
                        (m[0].x * m[1].y - m[0].y * m[1].x)
                    )
                ).Transpose);
            }
        }

        /// <summary>
        /// True if the matrix is the identity matrix.
        /// </summary>
        public bool isIdentity => this.Equals(Identity);

        /// <summary>
        /// Transpose of the matrix.
        /// </summary>
        public IGB283Transform Transpose
        {
            get
            {
                IGB283Transform newMatrix = new IGB283Transform();
                for (int i = 0; i < MATRIXORDER; i++)
                    newMatrix.SetColumn(i, this.GetRow(i));
                return newMatrix;
            }
        }

        /// <summary>
        /// Access element at row, column. W/ Out Of Range
        /// </summary>
        public float this[int row, int column]
        {
            get
            {
                return column switch
                {
                    0 => m[row].x,
                    1 => m[row].y,
                    2 => m[row].z,
                    _ => throw new IndexOutOfRangeException()
                };
            }
            set
            {
                switch (column)
                {
                    case 0: m[row].x = value; break;
                    case 1: m[row].y = value; break;
                    case 2: m[row].z = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        #endregion

        #region Matrix Setup
        /// <summary>
        /// Matrix Setup - Set Row Values
        /// </summary>
        public void SetRow(int index, IGB283Vector row) => m[index] = row;

        /// <summary>
        /// Matrix Setup - Get Row Values
        /// </summary>
        public IGB283Vector GetRow(int row) => m[row];

        /// <summary>
        /// Matrix Setup - Set Column Values
        /// </summary>
        public void SetColumn(int index, IGB283Vector column)
        {
            IGB283Vector r1 = m[0];
            IGB283Vector r2 = m[1];
            IGB283Vector r3 = m[2];

            r1[index] = column[0];
            r2[index] = column[1];
            r3[index] = column[2];

            m[0] = r1;
            m[1] = r2;
            m[2] = r3;
        }

        /// <summary>
        /// Matrix Setup - Get Column Values
        /// </summary>
        public IGB283Vector GetColumn(int column) =>
            new IGB283Vector(m[0][column], m[1][column], m[2][column]);

        #endregion

        #region Transformation Methods
        /// <summary>
        /// Transform a point by this matrix (homogeneous coordinates).
        /// </summary>
        public IGB283Vector MultiplyPoint_Z1(IGB283Vector p)
        {
            p.z = 1f;
            return new IGB283Vector(
                m[0].x * p.x + m[0].y * p.y + m[0].z * p.z,
                m[1].x * p.x + m[1].y * p.y + m[1].z * p.z,
                m[2].x * p.x + m[2].y * p.y + m[2].z * p.z
            );
        }

        // NOTE: The above Version results in a 1z offset for 2D

        /// <summary>
        /// Transform a point by this matrix (homogeneous coordinates).
        /// </summary>
        public IGB283Vector MultiplyPoint(IGB283Vector p)
        {
            float homogeneousZ = 1f;

            return new IGB283Vector(
                m[0].x * p.x + m[0].y * p.y + m[0].z * homogeneousZ,
                m[1].x * p.x + m[1].y * p.y + m[1].z * homogeneousZ,
                p.z  // Preserve original Z coordinate
            );
        }


        /// <summary>
        /// Transform a direction (ignores translation).
        /// </summary>
        public IGB283Vector MultiplyPoint3x4(IGB283Vector p) =>
            new IGB283Vector(
                m[0].x * p.x + m[0].y * p.y,
                m[1].x * p.x + m[1].y * p.y,
                m[2].x * p.x + m[2].y * p.y
            );

        /// <summary>
        /// Transform a vector by this matrix.
        /// </summary>
        public IGB283Vector MultiplyVector(IGB283Vector v) =>
            new IGB283Vector(
                m[0].x * v.x + m[0].y * v.y + m[0].z * v.z,
                m[1].x * v.x + m[1].y * v.y + m[1].z * v.z,
                m[2].x * v.x + m[2].y * v.y + m[2].z * v.z
            );

        #endregion

        #region Static Transfromation Generators

        /// <summary>
        /// Translateation Matrix
        /// </summary>
        public static IGB283Transform Translate(float x, float y)
        {
            IGB283Transform matrix = Identity;
            matrix.SetRow(0, new IGB283Vector(1f, 0f, x));
            matrix.SetRow(1, new IGB283Vector(0f, 1f, y));
            return matrix;
        }
        /// <summary>
        /// Translateation Matrix
        /// </summary>
        public static IGB283Transform Translate(IGB283Vector v)
        {
            IGB283Transform matrix = Identity;
            matrix.SetRow(0, new IGB283Vector(1f, 0f, v.x));
            matrix.SetRow(1, new IGB283Vector(0f, 1f, v.y));
            return matrix;
        }

        /// <summary>
        /// Scalling Matrix
        /// </summary>
        public static IGB283Transform Scale(float sx, float sy)
        {
            IGB283Transform matrix = Identity;
            matrix.SetRow(0, new IGB283Vector(sx, 0f, 0f));
            matrix.SetRow(1, new IGB283Vector(0f, sy, 0f));
            return matrix;
        }

        /// <summary>
        /// Rotation Matrix
        /// </summary>
        public static IGB283Transform Rotate(float angle, bool useDegrees = false)
        {
            if (useDegrees) angle *= (float)Math.PI / 180f;
            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);

            IGB283Transform matrix = Identity;
            matrix.SetRow(0, new IGB283Vector(cos, -sin, 0f));
            matrix.SetRow(1, new IGB283Vector(sin, cos, 0f));
            return matrix;
        }

        #endregion

        #region Operators

        public static IGB283Transform operator *(IGB283Transform b, IGB283Transform c)
        {
            IGB283Transform newMatrix = new IGB283Transform();
            for (int i = 0; i < MATRIXORDER; i++)
            {
                IGB283Vector r = new IGB283Vector(
                    b[i, 0] * c[0, 0] + b[i, 1] * c[1, 0] + b[i, 2] * c[2, 0],
                    b[i, 0] * c[0, 1] + b[i, 1] * c[1, 1] + b[i, 2] * c[2, 1],
                    b[i, 0] * c[0, 2] + b[i, 1] * c[1, 2] + b[i, 2] * c[2, 2]
                );
                newMatrix.SetRow(i, r);
            }
            return newMatrix;
        }

        public static IGB283Transform operator *(float scalar, IGB283Transform c)
        {
            IGB283Transform newMatrix = new IGB283Transform();
            for (int i = 0; i < MATRIXORDER; i++)
                for (int j = 0; j < MATRIXORDER; j++)
                    newMatrix[i, j] = c[i, j] * scalar;
            return newMatrix;
        }

        #endregion

        #region Equality & Utility

        public bool Equals(IGB283Transform m2)
        {
            for (int i = 0; i < MATRIXORDER; i++)
                for (int j = 0; j < MATRIXORDER; j++)
                    if (this[i, j] != m2[i, j]) return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format(
                "{0,-12:0.00000}{1,-12:0.00000}{2,-12:0.00000}\r\n" +
                "{3,-12:0.00000}{4,-12:0.00000}{5,-12:0.00000}\r\n" +
                "{6,-12:0.00000}{7,-12:0.00000}{8,-12:0.00000}\r\n",
                m[0].x, m[0].y, m[0].z,
                m[1].x, m[1].y, m[1].z,
                m[2].x, m[2].y, m[2].z
            );
        }

        #endregion
    }
}