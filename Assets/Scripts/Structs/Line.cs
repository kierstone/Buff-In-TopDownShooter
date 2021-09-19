using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line{
    public Vector2[] point;

    public Line(Vector2[] point = null){
        this.point = new Vector2[2]{Vector2.zero, Vector2.zero};
        if (point != null){
            if (point.Length > 0) this.point[0] = point[0];
            if (point.Length > 1) this.point[1] = point[1];
        }
    }

    ///<summary>
    ///左侧的点
    ///</summary>
    public Vector2 LeftPoint(){
        return this.point[0].x <= this.point[1].x ? this.point[0] : this.point[1];
    }

    ///<summary>
    ///右侧的点
    ///</summary>
    public Vector2 RightPoint(){
        return this.point[0].x > this.point[1].x ? this.point[0] : this.point[1];
    }

    ///<summary>
    ///Y值更小的点，但是unity和正常游戏是反的，所以起名难，只能这么来了，因为思维里还是y小在上
    ///</summary>
    public Vector2 TopPoint(){
        return this.point[0].y <= this.point[1].y ? this.point[0] : this.point[1];
    }

    ///<summary>
    ///Y值更大的点，但是unity和正常游戏是反的，所以起名难，只能这么来了，因为思维里还是y大在下
    ///</summary>
    public Vector2 BottomPoint(){
        return this.point[0].y > this.point[1].y ? this.point[0] : this.point[1];
    }

    ///<summary>
    ///是否与另外一条线段（Line）相交
    ///</summary>
    public bool Cross(Line other){
        return (
            Mathf.Min(this.point[0].x, this.point[1].x) <= Mathf.Max(other.point[0].x, other.point[1].x) &&
            Mathf.Max(this.point[0].x, this.point[1].x) >= Mathf.Min(other.point[0].x, other.point[1].x) &&
            Mathf.Min(this.point[0].y, this.point[1].y) <= Mathf.Max(other.point[0].y, other.point[1].y) &&
            Mathf.Max(this.point[0].y, this.point[1].y) >= Mathf.Min(other.point[0].y, other.point[1].y) &&
            ((Line.Mul(other.point[0], this.point[0], other.point[1]))*(Line.Mul(other.point[0], other.point[1], this.point[1]))) >= 0 &&
            ((Line.Mul(this.point[0], other.point[0], this.point[1]))*(Line.Mul(this.point[0], this.point[1], other.point[1]))) >= 0
        );
    }

    private static float Mul(Vector2 a , Vector2 b, Vector2 c){
        return (b.x - a.x)*(c.y - a.y) - (c.x - a.x) * (b.y - a.y);
    }
		
}