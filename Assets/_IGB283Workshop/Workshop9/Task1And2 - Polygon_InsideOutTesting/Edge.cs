using UnityEngine;

public class Edge
{
    public Vector2 Start;
    public Vector2 End;

    public Vector2 Displacement { get { return End - Start; } }


    public Edge()
    {
        Start = Vector2.zero;
        End = Vector2.zero;
    }
    public Edge(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }


    public bool CollidesWith(Edge other)
    {
        float dCross = 0f;
        dCross = Cross2D(other.Displacement, this.Displacement);
        if(dCross == 0f)
            return false;

        float t = Cross2D((this.Start - other.Start), this.Displacement) / dCross;
        float u = Cross2D((this.Start - other.Start), other.Displacement) / dCross;

        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            // the two edges intersect
            return true;
        }

        return false;
    }



    private static float Cross2D(Vector2 v1, Vector2 v2)
    {
        return v1.x * v2.y - v1.y * v2.x;
    }

    //Operators
    public static bool operator ==(Edge e1, Edge e2)
    {
        return (e1.Start == e2.Start && e1.End == e2.End)
        || (e1.Start == e2.End && e1.End == e2.Start); // Reversed
    }
    public static bool operator !=(Edge e1, Edge e2)
    {
        return !(e1 == e2);
    }
    // Override methods
    public override bool Equals(object obj)
    {
        Edge other = obj as Edge;
        if (other == null)
        {
            return false;
        }
        else
        {
            return this == other;
        }
    }
    public override int GetHashCode()
    {
        return Start.GetHashCode() & End.GetHashCode();
    }
    public override string ToString()
    {
        return string.Format("Line from {0} to {1}\nDisplacement={2}",
        Start.ToString(), End.ToString(), Displacement.ToString());
    }

}
