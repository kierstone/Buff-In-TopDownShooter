using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
///子弹的“状态”，用来管理当前应该怎么移动、应该怎么旋转、应该怎么播放动画的。
///是一个角色的总的“调控中心”。
///</summary>
public class BulletState:MonoBehaviour{
    ///<summary>
    ///这是一颗怎样的子弹
    ///</summary>
    public BulletModel model;
    
    ///<summary>
    ///要发射子弹的这个人的gameObject，这里就认角色（拥有ChaState的）
    ///当然可以是null发射的，但是写效果逻辑的时候得小心caster是null的情况
    ///</summary>
    public GameObject caster;

    ///<summary>
    ///子弹发射时候，caster的属性，如果caster不存在，就会是一个ChaProperty.zero
    ///在一些设计中，比如wow的技能中，技能效果是跟发出时候的角色状态有关的，之后即使获得或者取消了buff，更换了装备，数值一样不会受到影响，所以得记录这个释放当时的值
    ///</summary>
    public ChaProperty propWhileCast = ChaProperty.zero;

    ///<summary>
    ///发射的角度，单位：角度，如果useFireDegreeForever == true，那就得用这个角度来获得当前飞行路线了
    ///</summary>
    public float fireDegree;

    ///<summary>
    ///子弹的初速度，单位：米/秒，跟Tween结合获得Tween得到当前移动速度
    ///</summary>
    public float speed;

    ///<summary>
    ///子弹的生命周期，单位：秒
    ///</summary>
    public float duration;

    ///<summary>
    ///子弹已经存在了多久了，单位：秒
    ///毕竟duration是可以被重设的，比如经过一个aoe，生命周期减半了
    ///</summary>
    public float worked = 0;

    ///<summary>
    ///子弹的轨迹函数
    ///<param name="t">子弹飞行了多久的时间点，单位秒。</param>
    ///<return>返回这一时间点上的速度和偏移，Vector3就是正常速度正常前进</return>
    ///</summary>
    public BulletTween tween = null;

    ///<summary>
    ///本帧的移动
    ///</summary>
    private Vector3 moveForce = new Vector3();

    ///<summary>
    ///本帧的移动信息
    ///</summary>
    public Vector3 velocity{
        get{return moveForce;}
    }

    ///<summary>
    ///子弹的移动轨迹是否严格遵循发射出来的角度
    ///</summary>
    public bool useFireDegreeForever = false;

    ///<summary>
    ///子弹命中纪录
    ///</summary>
    public List<BulletHitRecord> hitRecords = new List<BulletHitRecord>();

    ///<summary>
    ///子弹创建后多久是没有碰撞的，这样比如子母弹之类的，不会在创建后立即命中目标，但绝大多子弹还应该是0的
    ///单位：秒
    ///</summary>
    public float canHitAfterCreated = 0;

    ///<summary>
    ///子弹正在追踪的目标，不太建议使用这个，最好保持null
    ///</summary>
    public GameObject followingTarget = null;


    ///<summary>
    ///还能命中几次
    ///</summary>
    public int hp = 1;

    private MoveType moveType;
    private bool smoothMove;

    private UnitRotate unitRotate;
    private UnitMove unitMove;
    private GameObject viewContainer;
    
    private void Start() {
        // unitRotate = gameObject.GetComponent<UnitRotate>();
        // unitMove = gameObject.GetComponent<UnitMove>();
        synchronizedUnits();
    }

    ///<summary>
    ///子弹是否碰到了碰撞
    ///</summary>
    public bool HitObstacle(){
        return unitMove == null ? false : unitMove.hitObstacle;
    }

    ///<summary>
    ///控制子弹移动，这应该是由bulletSystem来调用的
    ///</summary>
    public void SetMoveForce(Vector3 mf){
        this.moveForce = mf;

        float moveDeg = (
            useFireDegreeForever == true ||
            worked <= 0     //还是那个问题，unity的动画走的是update，所以慢了，旋转没转到预设角度，所以我得在第一帧走firedegree
            ) ? fireDegree : transform.rotation.eulerAngles.y; //欧拉获得的是角度

        moveForce.y = 0;
        moveForce *= speed;
        moveDeg += Mathf.Atan2(moveForce.x, moveForce.z) * 180 / Mathf.PI;
        
        float mR = moveDeg * Mathf.PI / 180;
        float mLen = Mathf.Sqrt(Mathf.Pow(moveForce.x, 2) + Mathf.Pow(moveForce.z, 2));

        moveForce.x = Mathf.Sin(mR) * mLen;
        moveForce.z = Mathf.Cos(mR) * mLen;

        unitMove.MoveBy(moveForce);
        unitRotate.RotateTo(moveDeg);
    }

    ///<summary>
    ///根据BulletLauncher初始化这个数据
    ///<param name="bullet">bulletLauncher</param>
    ///<param name="targets">子弹允许跟踪的全部目标，在这里根据脚本筛选</param>
    ///</summary>
    public void InitByBulletLauncher(BulletLauncher bullet, GameObject[] targets){
        this.model = bullet.model;
        this.caster = bullet.caster;
        if (this.caster && caster.GetComponent<ChaState>()){
            this.propWhileCast = caster.GetComponent<ChaState>().property;
        }
        this.fireDegree = bullet.fireDegree;
        this.speed = bullet.speed;
        this.duration = bullet.duration;
        this.worked = 0;
        this.tween = bullet.tween;
        this.useFireDegreeForever = bullet.useFireDegreeForever;
        this.canHitAfterCreated = bullet.canHitAfterCreated;
        this.smoothMove = !bullet.model.removeOnObstacle;
        this.moveType = bullet.model.moveType;
        this.hp = bullet.model.hitTimes;

        synchronizedUnits();

        //把视觉特效补充给bulletObj
        GameObject bulletEffect = Instantiate<GameObject>(
            Resources.Load<GameObject>("Prefabs/Bullet/" + bullet.model.prefab),
            new Vector3(),
            Quaternion.identity,
            this.transform
        );
        bulletEffect.transform.localPosition = new Vector3();
        bulletEffect.transform.localRotation = Quaternion.identity;
        
        this.followingTarget = bullet.targetFunc == null ? null :
            bullet.targetFunc(this.gameObject, targets);
    }

    //同步一下unitMove和自己的一些状态
    private void synchronizedUnits(){
        if (!unitRotate) unitRotate = gameObject.GetComponent<UnitRotate>();
        if (!unitMove)  unitMove = gameObject.GetComponent<UnitMove>();
        if (!viewContainer) viewContainer = gameObject.GetComponentInChildren<ViewContainer>().gameObject;
        
        unitMove.smoothMove = this.smoothMove;
        unitMove.moveType = this.moveType;
        unitMove.bodyRadius = this.model.radius;
    }

    ///<summary>
    ///判断子弹是否还能击中某个GameObject
    ///<param name="target">目标gameObject</param>
    ///</summary>
    public bool CanHit(GameObject target){
        if (canHitAfterCreated > 0) return false;
        for (int i = 0; i < this.hitRecords.Count; i++){
            if (hitRecords[i].target == target){
                return false;
            }
        }
        
        ChaState cs = target.GetComponent<ChaState>();
        if (cs && cs.immuneTime > 0) return false;

        return true;
    }

    ///<summary>
    ///添加命中纪录
    ///<param name="target">目标GameObject</param>
    ///</summary>
    public void AddHitRecord(GameObject target){
        hitRecords.Add(new BulletHitRecord(
            target,
            this.model.sameTargetDelay
        ));
    }

    ///<summary>
    ///改变子弹半径
    ///<param name="radius">子弹要改变为的半径，单位米</param>
    ///</summary>
    public void SetRadius(float radius){
        this.model.radius = radius;
        synchronizedUnits();
    }
}