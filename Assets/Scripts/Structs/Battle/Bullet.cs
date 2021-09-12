using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///子弹的发射信息，专门有个系统会处理这个发射信息，然后往地图上放置出子弹的GameObject
///所有脚本中，需要创建一个子弹，也应该传递这个结构作为产生子弹的参数
///</summary>
public class BulletLauncher{
    ///<summary>
    ///要发射的子弹
    ///</summary>
    public BulletModel model;

    ///<summary>
    ///要发射子弹的这个人的gameObject，这里就认角色（拥有ChaState的）
    ///当然可以是null发射的，但是写效果逻辑的时候得小心caster是null的情况
    ///</summary>
    public GameObject caster;

    ///<summary>
    ///发射的坐标，y轴是无效的
    ///</summary>
    public Vector3 firePosition;

    ///<summary>
    ///发射的角度，单位：角度
    ///</summary>
    public float fireDegree;

    ///<summary>
    ///子弹的初速度，单位：米/秒
    ///</summary>
    public float speed;

    ///<summary>
    ///子弹的生命周期，单位：秒
    ///子弹应该是有个生命周期的，因为如果总是不命中，也不回收总不好
    ///当然更多的还是因为有些子弹射程非常短
    ///</summary>
    public float duration;

    ///<summary>
    ///子弹在发射瞬间，可以捕捉一个GameObject作为目标，并且将这个目标传递给BulletTween，作为移动参数
    ///<param name="bullet">是当前的子弹GameObject，不建议公式中用到这个</param>
    ///<param name="targets">所有可以被选作目标的对象，这里是GameManager的逻辑决定的传递过来谁，比如这个游戏子弹只能捕捉角色作为对象，那就是只有角色的GameObject，当然如果需要，加入子弹也不麻烦</param>
    ///<return>在创建子弹的瞬间，根据这个函数获得一个GameObject作为followingTarget</return>
    ///</summary>
    public BulletTargettingFunction targetFunc;

    ///<summary>
    ///子弹的轨迹函数，传入一个时间点，返回出一个Vector3，作为这个时间点的速度和方向，这是个相对于正在飞行的方向的一个偏移（*speed的）
    ///正在飞行的方向按照z轴，来算，也就是说，当你只需要子弹匀速行动的时候，你可以让这个函数只做一件事情——return Vector3.forward。
    ///如果这个值是null，就会跟return Vector3.forward一样处理，性能还高一些。
    ///虽然是vector3，但是y坐标是无效的，只是为了统一单位
    ///比如手榴弹这种会一跳一跳的可不得y变化吗？是要变化，但是这个变化归我管，这是render的事情
    ///简单地说就是做一个跳跳的Component，update（而非fixedupdate）里面去管理跳吧
    ///<param name="t">子弹飞行了多久的时间点，单位秒。</param>
    ///<return>返回这一时间点上的速度和偏移，Vector3就是正常速度正常前进</return>
    ///</summary>
    public BulletTween tween = null;

    ///<summary>
    ///子弹的移动轨迹是否严格遵循发射出来的角度
    ///如果是true，则子弹每一帧Tween返回的角度是按照fireDegree来偏移的
    ///如果是false，则会根据子弹正在飞的角度(transform.rotation)来算下一帧的角度
    ///</summary>
    public bool useFireDegreeForever = false;

    ///<summary>
    ///子弹创建后多久是没有碰撞的，这样比如子母弹之类的，不会在创建后立即命中目标，但绝大多子弹还应该是0的
    ///单位：秒
    ///</summary>
    public float canHitAfterCreated = 0;

    ///<summary>
    ///子弹的一些特殊逻辑使用的参数，可以在创建子的时候传递给子弹
    ///</summary>
    public Dictionary<string, object> param;

    public BulletLauncher(
        BulletModel model, GameObject caster, Vector3 firePos, float degree, float speed, float duration,
        float canHitAfterCreated = 0,
        BulletTween tween = null, BulletTargettingFunction targetFunc = null, bool useFireDegree = false,
        Dictionary<string, object> param = null
    ){
        this.model = model;
        this.caster = caster;
        this.firePosition = firePos;
        this.fireDegree = degree;
        this.speed = speed;
        this.duration = duration;
        this.tween = tween;
        this.useFireDegreeForever = useFireDegree;
        this.targetFunc = targetFunc;
        this.param = param;
    }
}

///<summary>
///子弹的模板，也是策划填表的东西，当然游戏过程中所有的子弹模板，未必都得由策划填表，也可以运行的脚本逻辑产生
///值得注意的是，这些信息只是构成“一个子弹”，也就是描述了这个子弹是怎样的，因此有很多数据并不属于这个结构
///比如子弹的飞行速度、轨迹等，这些数据其实都是子弹的发射环境决定的，同一个导弹，可能被不同的人、地形、其他任何东西发射出来
///这些子弹的性质是一样的，就是被填表的这些内容，但是他们可能轨迹之类都不同。
///</summary>
public struct BulletModel{
    public string id;

    ///<summary>
    ///子弹需要用的prefab，默认是Resources/Prefabs/Bullet/下的，所以这个string需要省略前半部分
    ///比如是BlueRocket0，就会创建自Resources/Prefabs/Bullet/BlueRocket0这个prefab
    ///</summary>
    public string prefab;

    ///<summary>
    ///子弹的碰撞半径，单位：米。这个游戏里子弹在逻辑世界都是圆形的，当然是这个游戏设定如此，实际策划的需求未必只能是圆形。
    ///</summary>
    public float radius;

    ///<summary>
    ///子弹可以碰触的次数，每次碰到合理目标-1，到0的时候子弹就结束了。
    ///</summary>
    public int hitTimes;

    ///<summary>
    ///子弹碰触同一个目标的延迟，单位：秒，最小值是Time.fixedDeltaTime（每帧发生一次）
    ///</summary>
    public float sameTargetDelay;

    ///<summary>
    ///子弹被创建的事件
    ///<param name="bullet">要创建的子弹</param>
    ///</summary>
    public BulletOnCreate onCreate;

    public object[] onCreateParam;

    ///<summary>
    ///子弹命中目标时候发生的事情
    ///<param name="bullet">发生碰撞的子弹，应该是个bulletObj，但是在unity的逻辑下，他就是个GameObject，具体数据从GameObject拿了</param>
    ///<param name="target">被击中的角色</param>
    ///</summary>
    public BulletOnHit onHit;

    ///<summary>
    ///OnHit的参数
    ///</summary>
    public object[] onHitParams;

    ///<summary>
    ///子弹生命周期结束时候发生的事情
    ///<param name="bullet">发生碰撞的子弹，应该是个bulletObj，但是在unity的逻辑下，他就是个GameObject，具体数据从GameObject拿了</param>
    ///</summary>
    public BulletOnRemoved onRemoved;

    ///<summary>
    ///OnRemoved的参数
    ///</summary>
    public object[] onRemovedParams;

    ///<summary>
    ///子弹的移动方式，一般来说都是飞行的，也有部分是手雷这种属于走路的
    ///</summary>
    public MoveType moveType;

    ///<summary>
    ///子弹的是否碰到障碍物就爆炸，不会的话会沿着障碍物移动
    ///</summary>
    public bool removeOnObstacle;

    ///<summary>
    ///子弹是否会命中敌人
    ///</summary>
    public bool hitFoe;

    ///<summary>
    ///子弹是否会命中盟军
    ///</summary>
    public bool hitAlly;

    public BulletModel(
        string id, string prefab, 
        string onCreate = "",
        object[] createParams = null,
        string onHit = "",
        object[] onHitParams = null,
        string onRemoved = "",
        object[] onRemovedParams = null,
        MoveType moveType = MoveType.fly, bool removeOnObstacle = true,
        float radius = 0.1f, int hitTimes = 1, float sameTargetDelay = 0.1f,
        bool hitFoe = true, bool hitAlly = false
    ){
        this.id = id;
        this.prefab = prefab;
        this.onHit = onHit == "" ? null : DesignerScripts.Bullet.onHitFunc[onHit];
        this.onRemoved = onRemoved == "" ? null : DesignerScripts.Bullet.onRemovedFunc[onRemoved];
        this.onCreate = onCreate == "" ? null : DesignerScripts.Bullet.onCreateFunc[onCreate];
        this.onCreateParam = createParams != null ? createParams : new object[0];
        this.onHitParams = onHitParams != null ? onHitParams : new object[0];
        this.onRemovedParams = onRemovedParams != null ? onRemovedParams : new object[0];
        this.radius = radius;
        this.hitTimes = hitTimes;
        this.sameTargetDelay = sameTargetDelay;
        this.moveType = moveType;
        this.removeOnObstacle = removeOnObstacle;
        this.hitAlly = hitAlly;
        this.hitFoe = hitFoe;
    }
}

///<summary>
///子弹命中纪录
///</summary>
public class BulletHitRecord{
    ///<summary>
    ///角色的GameObject
    ///</summary>
    public GameObject target;

    ///<summary>
    ///多久之后还能再次命中，单位秒
    ///</summary>
    public float timeToCanHit;

    public BulletHitRecord(GameObject character, float timeToCanHit){
        this.target = character;
        this.timeToCanHit = timeToCanHit;
    }
}


///<summary>
///子弹被创建的事件
///</summary>
public delegate void BulletOnCreate(GameObject bullet);

///<summary>
///子弹命中目标的时候触发的事件
///<param name="bullet">发生碰撞的子弹，应该是个bulletObj，但是在unity的逻辑下，他就是个GameObject，具体数据从GameObject拿了</param>
///<param name="target">被击中的角色</param>
///<summary>
public delegate void BulletOnHit(GameObject bullet, GameObject target);

///<summary>
///子弹在生命周期消耗殆尽之后发生的事件，生命周期消耗殆尽是因为BulletState.duration<=0，或者是因为移动撞到了阻挡。
///<param name="bullet">发生碰撞的子弹，应该是个bulletObj，但是在unity的逻辑下，他就是个GameObject，具体数据从GameObject拿了</param>
///</summary>
public delegate void BulletOnRemoved(GameObject bullet);

///<summary>
///子弹的轨迹函数，传入一个时间点，返回出一个Vector3，作为这个时间点的速度和方向，这是个相对于正在飞行的方向的一个偏移（*speed的）
///正在飞行的方向按照z轴，来算，也就是说，当你只需要子弹匀速行动的时候，你可以让这个函数只做一件事情——return Vector3.forward。
///<param name="t">子弹飞行了多久的时间点，单位秒。</param>
///<param name="bullet">是当前的子弹GameObject，不建议公式中用到这个</param>
///<param name="following">是正在跟踪的对象的GameObject，除非要做“跟踪弹”不然不建议使用</param>
///<return>返回这一时间点上的速度和偏移，Vector3就是正常速度正常前进</return>
///</summary>
public delegate Vector3 BulletTween(float t, GameObject bullet, GameObject target);

///<summary>
///子弹在发射瞬间，可以捕捉一个GameObject作为目标，并且将这个目标传递给BulletTween，作为移动参数
///<param name="bullet">是当前的子弹GameObject，不建议公式中用到这个</param>
///<param name="targets">所有可以被选作目标的对象，这里是GameManager的逻辑决定的传递过来谁，比如这个游戏子弹只能捕捉角色作为对象，那就是只有角色的GameObject，当然如果需要，加入子弹也不麻烦</param>
///<return>在创建子弹的瞬间，根据这个函数获得一个GameObject作为followingTarget</return>
///</summary>
public delegate GameObject BulletTargettingFunction(GameObject bullet, GameObject[] targets);