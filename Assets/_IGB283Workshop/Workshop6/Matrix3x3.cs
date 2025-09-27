using System.Collections.Generic;
using UnityEngine;

namespace workshop6.old
{
    public class Matrix3x3
    {
        private const int matrixOrder = 3;

        // Static Variables

        /// <summary>
        /// The identiy matrix
        /// </summary>
        // NOTE: uses property rather then function as Identiy matrixes are seen as values? Ask leature about the structure of this n the choice behind it?
        public static Matrix3x3 identity
        {
            get
            {
                // Create a new matrix
                Matrix3x3 i = new Matrix3x3();
                // Set the rows of the matrix
                i.SetRow(0, new Vector3(1.0f, 0.0f, 0.0f));
                i.SetRow(1, new Vector3(0.0f, 1.0f, 0.0f));
                i.SetRow(2, new Vector3(0.0f, 0.0f, 1.0f));
                return i;
            }
        }


        // The zero matrix
        private static Vector3 zeroVector = new Vector3(0.0f, 0.0f, 0.0f);
        public static Matrix3x3 zero
        {
            get
            {
                return new Matrix3x3(
                    zeroVector,
                    zeroVector,
                    zeroVector
                );
            }
        }

        // Variables
        // The determinant of the matrix
        public float determinant
        {
            get
            {
                return
                    m[0][0] * (m[1][1] * m[2][2] - m[1][2] * m[2][1])
                    - m[0][1] * (m[1][0] * m[2][2] - m[1][2] * m[2][0])
                    + m[0][2] * (m[1][0] * m[2][1] - m[1][1] * m[2][0]);
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }


        // The inverse of the matrix
        public Matrix3x3 inverse
        {
            get
            {
                return (1 / determinant) * (new Matrix3x3(
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

        // Is the matrix an identity matrix
        public bool isIdentity
        {
            get
            {
                return this.Equals(identity);
            }
        }

        // The element at x, y
        public float this[int row, int column]
        {
            get
            {
                return m[row][column];
            }
            set
            {
                Vector3 v = m[row];
                v[column] = value;
                m[row] = v;
            }
        }

        /// <summary>
        /// The transpose of the matrix
        /// </summary>
        public Matrix3x3 transpose
        {
            get
            {
                Matrix3x3 newMatrix = new Matrix3x3();
                for (int i = 0; i < matrixOrder; i++)
                {
                    newMatrix.SetColumn(i,
                    this.GetRow(i));
                }
                return newMatrix;
            }
        }

        // Constructor
        // Arraylist to contain the vector data
        private List<Vector3> m = new List<Vector3>();

        // Create a matrix 3x3 with specified values
        private Matrix3x3(Vector3 r1, Vector3 r2, Vector3 r3)
        {
            m.Add(r1);
            m.Add(r2);
            m.Add(r3);
        }

        // Create a matrix 3x3 initialised with zeros
        public Matrix3x3()
        {
            m.Add(zeroVector);
            m.Add(zeroVector);
            m.Add(zeroVector);
        }

        // Public Functions
        // get a column of the matrix
        public Vector3 GetColumn(int column)
        {
            return new Vector3(m[0][column], m[1][column], m[2][column]);
        }

        // get a row of the matrix
        public Vector3 GetRow(int row)
        {
            return m[row];
        }

        // Transform a point by this matrix
        public Vector3 MultiplyPoint(Vector3 p)
        {
            p.z = 1.0f;
            Vector3 v = new Vector3(
                m[0][0] * p[0] + m[0][1] * p[1] + m[0][2] * p[2],
                m[1][0] * p[0] + m[1][1] * p[1] + m[1][2] * p[2],
                m[2][0] * p[0] + m[2][1] * p[1] + m[2][2] * p[2]
            );
            return v;
        }

        // Transform a direction by this matrix
        public Vector3 MultiplyPoint3x4(Vector3 p)
        {
            throw new System.NotImplementedException();
        }

        // Set a column of the matrix
        public Vector3 MultiplyVector(Vector3 v)
        {
            throw new System.NotImplementedException();
        }

        // Set a column of the matrix
        public void SetColumn(int index, Vector3 column)
        {
            // Get the three row vectors of the matrix
            Vector3 r1 = m[0];
            Vector3 r2 = m[1];
            Vector3 r3 = m[2];

            // Set the index'th value to be the value from the column vector
            r1[index] = column[0];
            r2[index] = column[1];
            r3[index] = column[2];

            // Reassign the rows to he matrix
            m[0] = r1;
            m[1] = r2;
            m[2] = r3;
        }

        // Set a row of the matrix
        public void SetRow(int index, Vector3 row)
        {
            m[index] = row;
        }

        // Sets this matrix to a translation, rotation and scaling matrix
        public void SetTRS(Vector3 pos, Quaternion q, Vector3 s)
        {
            throw new System.NotImplementedException("If you want to use this method you will need to implement it yourself");
        }
        // Return a string of the matrix
        public override string ToString()
        {
            string s = "";
            s = string.Format(
                 "{0,-12:0.00000}{1,-12:0.00000}{2,-12:0.00000}\r\n" +
                 "{3,-12:0.00000}{4,-12:0.00000}{5,-12:0.00000}\r\n" +
                 "{6,-12:0.00000}{7,-12:0.00000}{8,-12:0.00000}\r\n",
                m[0].x, m[0].y, m[0].z,
                m[1].x, m[1].y, m[1].z,
                m[2].x, m[2].y, m[2].z);

            return s;
        }

        // Operators

        /// <summary>
        /// Multiply two matrices together
        /// </summary>
        public static Matrix3x3 operator *(Matrix3x3 b, Matrix3x3 c)
        {
            // New matrix3x3 to store the values
            Matrix3x3 newMatrix = new Matrix3x3();
            // For each row in b, multiply by c and find new row
            for (int i = 0; i < matrixOrder; i++)
            {
                Vector3 r = new Vector3(
                b[i, 0] * c[0, 0] + b[i, 1] * c[1, 0]
                + b[i, 2] * c[2, 0],
                b[i, 0] * c[0, 1] + b[i, 1] * c[1, 1]
                + b[i, 2] * c[2, 1],
                b[i, 0] * c[0, 2] + b[i, 1] * c[1, 2]
                + b[i, 2] * c[2, 2]
                );
                newMatrix.SetRow(i, r);
            }
            return newMatrix;
        }

        /// <summary>
        /// Multiply a matrix by a scalar
        /// </summary>
        public static Matrix3x3 operator *(float b, Matrix3x3 c)
        {
            // New matrix3x3 to store the values
            Matrix3x3 newMatrix = new Matrix3x3();
            // multiply each value in c by b
            for (int i = 0; i < matrixOrder; i++)
            {
                for (int j = 0; j < matrixOrder; j++)
                {
                    newMatrix[i, j] = c[i, j] * b;
                }
            }
            return newMatrix;
        }

        /// <summary>
        /// Test the equality of this matrix and another
        /// </summary>
        public bool Equals(Matrix3x3 m2)
        {

            // For each row
            for (int i = 0; i < matrixOrder; i++)
            {
                // For each column
                for (int j = 0; j < matrixOrder; j++)
                {
                    if (this[i, j] != m2[i, j])
                    {
                        return false;
                    }
                }
            }
            // Everything was equal
            return true;
        }


        /// <summary>
        /// 3x3 translation matrix for 2D transformations
        /// </summary>
        /// <param name="x"> translation on x</param>
        /// <param name="y"> translation on y</param>
        /// <returns> 3x3 matrix representing the translation </returns>
        public static Matrix3x3 Translate(float x, float y)
        {
            Matrix3x3 matrix = new Matrix3x3();

            matrix.SetRow(0, new Vector3(1.0f, 0.0f, x));
            matrix.SetRow(1, new Vector3(0.0f, 1.0f, y));
            matrix.SetRow(2, new Vector3(0.0f, 0.0f, 1.0f));

            return matrix;
        }

        /// <summary>
        /// 3x3 scaling matrix for 2D transformations
        /// </summary>
        /// <param name="x"> scaling on x</param>
        /// <param name="y"> scaling on y</param>
        /// <returns> 3x3 matrix representing the scaling </returns>
        public static Matrix3x3 Scale(float sx, float sy)
        {
            Matrix3x3 matrix = new Matrix3x3();

            matrix.SetRow(0, new Vector3(sx, 0.0f, 0.0f));
            matrix.SetRow(1, new Vector3(0.0f, sy, 0.0f));
            matrix.SetRow(2, new Vector3(0.0f, 0.0f, 1.0f));

            return matrix;
        }

        /// <summary>
        /// 3x3 rotation matrix for 2D transformations
        /// </summary>
        /// <param name="angle">In Radians</param>
        /// <returns> 3x3 matrix representing the rotation </returns>
        public static Matrix3x3 Rotate(float angle)
        {
            // Create a new matrix
            Matrix3x3 matrix = new Matrix3x3();

            // Calculate sin and cos of the angle once
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            // Set the rows of the matrix
            matrix.SetRow(0, new Vector3(cos, -sin, 0.0f));
            matrix.SetRow(1, new Vector3(sin, cos, 0.0f));
            matrix.SetRow(2, new Vector3(0.0f, 0.0f, 1.0f));

            // Return the matrix
            return matrix;
        }




        /// <summary>
        /// 3x3 rotation matrix for 2D transformations
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="useDegrees"></param>
        public static Matrix3x3 Rotate(float angle, bool useDegrees = false)
        {
            if (useDegrees)
            {
                // Convert to radians
                angle = angle * Mathf.Deg2Rad;
            }

            // Create a new matrix
            Matrix3x3 matrix = new Matrix3x3();

            // Calculate sin and cos of the angle once
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            // Set the rows of the matrix
            matrix.SetRow(0, new Vector3(cos, -sin, 0.0f));
            matrix.SetRow(1, new Vector3(sin, cos, 0.0f));
            matrix.SetRow(2, new Vector3(0.0f, 0.0f, 1.0f));

            // Return the matrix
            return matrix;
        }

    }
}





