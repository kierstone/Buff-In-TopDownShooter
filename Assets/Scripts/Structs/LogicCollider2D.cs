using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CollisionResult{
    ///<summary>
    ///是否有碰撞到
    ///</summary>
    public bool hit;

    ///<summary>
    ///如果碰撞到，那么给被挤开的一个坐标
    ///</summary>
    public Vector2 pushTo;

    public CollisionResult(bool hit, Vector2 pushTo){
        this.hit = hit;
        this.pushTo = pushTo;
    }
}