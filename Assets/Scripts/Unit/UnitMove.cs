using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///单位移动控件，所有需要移动的单位都应该添加这个来控制它的移动，不论是角色，aoe还是子弹，但是地形不能用这个，因为地形是terrain不是unit
///这里负责的是每一帧往一个方向移动，至于往什么方向移动，这应该是其他控件的事情，比如角色是由操作来决定的，子弹则是轨迹决定的
///在这游戏里，角色只有x,z方向移动，依赖于地形的y移动如果有，也不归这个逻辑来管理，而是由视觉元素（比如小土坡）自身决定的
///</summary>
public class UnitMove : MonoBehaviour
{
    //是否有权移动
    private bool canMove = true;

    [Tooltip("单位的移动类型，根据游戏设计不同，这个值也可以不同")]
    public MoveType moveType = MoveType.ground;

    [Tooltip("单位的移动体型碰撞圆形的半径，单位：米")]
    public float bodyRadius = 0.25f;

    [Tooltip(
        "当单位移动被地图阻挡的时候，是选择一个更好的落脚点（true）还是直接停止移动（false），如果直接停止移动，那么停下的时候访问hitObstacle的时候就是true，否则hitObstacle永远是false"
    )]
    public bool smoothMove = true;

    public bool hitObstacle{get{
        return _hitObstacle;
    }}
    private bool _hitObstacle = false;
    
    //要移动的方向的力，单位：米/秒。
    private Vector3 velocity = Vector3.zero;

    void FixedUpdate() {
        if (canMove == false || velocity == Vector3.zero) return;   
        
        Vector3 targetPos = new Vector3(
            velocity.x * Time.fixedDeltaTime + transform.position.x,
            velocity.y * Time.fixedDeltaTime + transform.position.y, 
            velocity.z * Time.fixedDeltaTime + transform.position.z
        );

        MapTargetPosInfo mapTargetPosInfo = SceneVariants.map.FixTargetPosition(transform.position, bodyRadius, targetPos, moveType);
        if (smoothMove == false && mapTargetPosInfo.obstacle == true){
            _hitObstacle = true;
            canMove = false;
        }
        transform.position = mapTargetPosInfo.suggestPos;

        velocity = Vector3.zero;
    }

    

    private void StopMoving(){
        velocity = Vector3.zero;
    }


    ///<summary>
    ///当前的移动方向
    ///</summary>
    public Vector3 GetMoveDirection(){
        return velocity;
    }

    ///<summary>
    ///移动向某个方向，距离也决定了速度，距离单位是米，1秒内移动的量
    ///<param name="moveForce">移动方向和力，单位：米/秒</param>
    ///</summary>
    public void MoveBy(Vector3 moveForce){
        if (canMove == false) return;

        this.velocity = moveForce;
    }

    ///<summary>
    ///禁止角色可以移动能力，会停止当前的移动
    ///终止当前移动看起来是一个side-effect，但是依照游戏规则设计来说，他只是“配套功能”所以严格的说并不是side-effect
    ///</summary>
    public void DisableMove(){
        StopMoving();
        canMove = false;
    }

    ///<summary>
    ///开启角色可以移动的能力
    ///</summary>
    public void EnableRotate(){
        canMove = true;
    }
}   


