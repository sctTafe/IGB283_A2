using System.Collections.Generic;
using UnityEngine;
namespace ScottBarley.IGB283.Assessment2.WS7
{

    /// <summary>
    /// A 3x3 matrix implementation for 2D transformations (translation, rotation, scale).
    /// </summary>
    public class Matrix3x3
    {
        private const int matrixOrder = 3;
        private readonly List<Vector3> m = new List<Vector3>();

        private static readonly Vector3 zeroVector = new Vector3(0f, 0f, 0f);

        #region Constructors

        /// <summary>
        /// Creates a 3x3 zero matrix.
        /// </summary>
        public Matrix3x3()
        {
            m.Add(zeroVector);
            m.Add(zeroVector);
            m.Add(zeroVector);
        }

        /// <summary>
        /// Creates a 3x3 matrix from 3 row vectors.
        /// </summary>
        private Matrix3x3(Vector3 r1, Vector3 r2, Vector3 r3)
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
        public static Matrix3x3 Identity
        {
            get
            {
                Matrix3x3 i = new Matrix3x3();
                i.SetRow(0, new Vector3(1f, 0f, 0f));
                i.SetRow(1, new Vector3(0f, 1f, 0f));
                i.SetRow(2, new Vector3(0f, 0f, 1f));
                return i;
            }
        }

        /// <summary>
        /// Zero matrix.
        /// </summary>
        public static Matrix3x3 Zero => new Matrix3x3(zeroVector, zeroVector, zeroVector);

        #endregion

        #region Properties

        /// <summary>
        /// Determinant of the matrix.
        /// </summary>
        public float determinant =>
            m[0][0] * (m[1][1] * m[2][2] - m[1][2] * m[2][1])
            - m[0][1] * (m[1][0] * m[2][2] - m[1][2] * m[2][0])
            + m[0][2] * (m[1][0] * m[2][1] - m[1][1] * m[2][0]);

        /// <summary>
        /// Inverse of the matrix.
        /// </summary>
        public Matrix3x3 inverse
        {
            get
            {
                float det = determinant;
                if (Mathf.Abs(det) < 1e-6f)
                    throw new System.InvalidOperationException("Matrix is non-invertible.");

                return (1 / det) * (new Matrix3x3(
                    new Vector3(
                        (m[1][1] * m[2][2] - m[1][2] * m[2][1]),
                        -(m[1][0] * m[2][2] - m[1][2] * m[2][0]),
                        (m[1][0] * m[2][1] - m[1][1] * m[2][0])
                    ),
                    new Vector3(
                        -(m[0][1] * m[2][2] - m[0][2] * m[2][1]),
                        (m[0][0] * m[2][2] - m[0][2] * m[2][0]),
                        -(m[0][0] * m[2][1] - m[0][1] * m[2][0])
                    ),
                    new Vector3(
                        (m[0][1] * m[1][2] - m[0][2] * m[1][1]),
                        -(m[0][0] * m[1][2] - m[0][2] * m[1][0]),
                        (m[0][0] * m[1][1] - m[0][1] * m[1][0])
                    )
                ).transpose);
            }
        }

        /// <summary>
        /// True if the matrix is the identity matrix.
        /// </summary>
        public bool isIdentity => this.Equals(Identity);

        /// <summary>
        /// Transpose of the matrix.
        /// </summary>
        public Matrix3x3 transpose
        {
            get
            {
                Matrix3x3 newMatrix = new Matrix3x3();
                for (int i = 0; i < matrixOrder; i++)
                    newMatrix.SetColumn(i, this.GetRow(i));
                return newMatrix;
            }
        }

        /// <summary>
        /// Access element at row, column.
        /// </summary>
        public float this[int row, int column]
        {
            get => m[row][column];
            set
            {
                Vector3 v = m[row];
                v[column] = value;
                m[row] = v;
            }
        }

        #endregion

        #region Matrix Setup

        public void SetRow(int index, Vector3 row) => m[index] = row;

        public Vector3 GetRow(int row) => m[row];

        public void SetColumn(int index, Vector3 column)
        {
            Vector3 r1 = m[0];
            Vector3 r2 = m[1];
            Vector3 r3 = m[2];

            r1[index] = column[0];
            r2[index] = column[1];
            r3[index] = column[2];

            m[0] = r1;
            m[1] = r2;
            m[2] = r3;
        }

        public Vector3 GetColumn(int column) =>
            new Vector3(m[0][column], m[1][column], m[2][column]);

        #endregion

        #region Transformation Methods

        /// <summary>
        /// Transform a point by this matrix (homogeneous coordinates).
        /// </summary>
        public Vector3 MultiplyPoint(Vector3 p)
        {
            p.z = 1f;
            return new Vector3(
                m[0][0] * p[0] + m[0][1] * p[1] + m[0][2] * p[2],
                m[1][0] * p[0] + m[1][1] * p[1] + m[1][2] * p[2],
                m[2][0] * p[0] + m[2][1] * p[1] + m[2][2] * p[2]
            );
        }

        /// <summary>
        /// Transform a direction (ignores translation).
        /// </summary>
        public Vector3 MultiplyPoint3x4(Vector3 p) =>
            new Vector3(
                m[0][0] * p[0] + m[0][1] * p[1],
                m[1][0] * p[0] + m[1][1] * p[1],
                m[2][0] * p[0] + m[2][1] * p[1]
            );

        /// <summary>
        /// Transform a vector by this matrix.
        /// </summary>
        public Vector3 MultiplyVector(Vector3 v) =>
            new Vector3(
                m[0][0] * v.x + m[0][1] * v.y + m[0][2] * v.z,
                m[1][0] * v.x + m[1][1] * v.y + m[1][2] * v.z,
                m[2][0] * v.x + m[2][1] * v.y + m[2][2] * v.z
            );

        #endregion

        #region Static Generators

        public static Matrix3x3 Translate(float x, float y)
        {
            Matrix3x3 matrix = Identity;
            matrix.SetRow(0, new Vector3(1f, 0f, x));
            matrix.SetRow(1, new Vector3(0f, 1f, y));
            return matrix;
        }

        public static Matrix3x3 Scale(float sx, float sy)
        {
            Matrix3x3 matrix = Identity;
            matrix.SetRow(0, new Vector3(sx, 0f, 0f));
            matrix.SetRow(1, new Vector3(0f, sy, 0f));
            return matrix;
        }

        public static Matrix3x3 Rotate(float angle, bool useDegrees = false)
        {
            if (useDegrees) angle *= Mathf.Deg2Rad;
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            Matrix3x3 matrix = Identity;
            matrix.SetRow(0, new Vector3(cos, -sin, 0f));
            matrix.SetRow(1, new Vector3(sin, cos, 0f));
            return matrix;
        }

        #endregion

        #region Operators

        public static Matrix3x3 operator *(Matrix3x3 b, Matrix3x3 c)
        {
            Matrix3x3 newMatrix = new Matrix3x3();
            for (int i = 0; i < matrixOrder; i++)
            {
                Vector3 r = new Vector3(
                    b[i, 0] * c[0, 0] + b[i, 1] * c[1, 0] + b[i, 2] * c[2, 0],
                    b[i, 0] * c[0, 1] + b[i, 1] * c[1, 1] + b[i, 2] * c[2, 1],
                    b[i, 0] * c[0, 2] + b[i, 1] * c[1, 2] + b[i, 2] * c[2, 2]
                );
                newMatrix.SetRow(i, r);
            }
            return newMatrix;
        }

        public static Matrix3x3 operator *(float scalar, Matrix3x3 c)
        {
            Matrix3x3 newMatrix = new Matrix3x3();
            for (int i = 0; i < matrixOrder; i++)
                for (int j = 0; j < matrixOrder; j++)
                    newMatrix[i, j] = c[i, j] * scalar;
            return newMatrix;
        }

        #endregion

        #region Equality & Utility

        public bool Equals(Matrix3x3 m2)
        {
            for (int i = 0; i < matrixOrder; i++)
                for (int j = 0; j < matrixOrder; j++)
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