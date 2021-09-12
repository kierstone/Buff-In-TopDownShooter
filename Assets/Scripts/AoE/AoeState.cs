using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
///AoE的状态控制器
///</summary>
public class AoeState : MonoBehaviour{
    ///<summary>
    ///要释放的aoe
    ///</summary>
    public AoeModel model;

    ///<summary>
    ///是否被视作刚创建
    ///</summary>
    public bool justCreated = true;

    ///<summary>
    ///aoe的半径，单位：米
    ///目前这游戏的设计中，aoe只有圆形，所以只有一个半径，也不存在角度一说，如果需要可以扩展
    ///</summary>
    public float radius;

    ///<summary>
    ///aoe的施法者
    ///</summary>
    public GameObject caster;

    ///<summary>
    ///aoe存在的时间，单位：秒
    ///</summary>
    public float duration;

    ///<summary>
    ///aoe已经存在过的时间，单位：秒
    ///</summary>
    public float timeElapsed = 0;

    ///<summary>
    ///aoe移动轨迹函数
    ///</summary>
    public AoeTween tween;

    ///<summary>
    ///aoe的轨迹运行了多少时间了，单位：秒
    ///<summary>
    public float tweenRunnedTime = 0;

    ///<summary>
    ///创建时角色的属性
    ///</summary>
    public ChaProperty propWhileCreate;

    ///<summary>
    ///aoe的传入参数，比如可以吸收次数之类的
    ///</summary>
    public Dictionary<string, object> param = new Dictionary<string, object>();

    ///<summary>
    ///现在aoe范围内的所有角色的gameobject
    ///</summary>
    public List<GameObject> characterInRange = new List<GameObject>();

    ///<summary>
    ///现在aoe范围内的所有子弹的gameobject
    ///</summary>
    public List<GameObject> bulletInRange = new List<GameObject>();

    ///<summary>
    ///Tween函数的参数
    ///</summary>
    public object[] tweenParam;

    ///<summary>
    ///移动信息
    ///</summary>
    public Vector3 velocity{
        get{ return this._velo;}
    }
    private Vector3 _velo = new Vector3();

    private UnitMove unitMove;
    private UnitRotate unitRotate;
    private GameObject viewContainer;

    private void Start() {
        // this.unitMove = this.gameObject.GetComponent<UnitMove>();
        // this.unitRotate = this.gameObject.GetComponent<UnitRotate>();
        synchronizedUnits();
    }

    ///<summary>
    ///设置移动和旋转的信息，用于执行
    ///</summary>
    public void SetMoveAndRotate(AoeMoveInfo aoeMoveInfo){
        if (aoeMoveInfo != null){
            if (unitMove){
                unitMove.moveType = aoeMoveInfo.moveType;
                unitMove.bodyRadius = this.radius;
                _velo = aoeMoveInfo.velocity / Time.fixedDeltaTime;
                unitMove.MoveBy(_velo);
            }
            if (unitRotate){
                unitRotate.RotateTo(aoeMoveInfo.rotateToDegree);
            }
        }
    }

    public bool HitObstacle(){
        return unitMove == null ? false : unitMove.hitObstacle;
    }

    private void synchronizedUnits(){
        if (!unitMove) unitMove = this.gameObject.GetComponent<UnitMove>();
        if (!unitRotate) unitRotate = this.gameObject.GetComponent<UnitRotate>();
        if (!viewContainer) viewContainer = this.gameObject.GetComponentInChildren<ViewContainer>().gameObject;
        unitMove.bodyRadius = this.radius;
        unitMove.smoothMove = !model.removeOnObstacle;
    }

    public void InitByAoeLauncher(AoeLauncher aoe){
        this.model = aoe.model;
        this.radius = aoe.radius;
        this.duration = aoe.duration;
        this.timeElapsed = 0;
        this.tween = aoe.tween;
        this.tweenParam = aoe.tweenParam;
        this.tweenRunnedTime = 0;
        this.param = new Dictionary<string, object>();
        foreach (KeyValuePair<string, object> kv in aoe.param){
            this.param[kv.Key] = kv.Value;
        }//aoe.param;
        this.caster = aoe.caster;
        this.propWhileCreate = aoe.caster ? aoe.caster.GetComponent<ChaState>().property : ChaProperty.zero;
        
        this.transform.position = aoe.position;
        this.transform.eulerAngles.Set(0, aoe.degree, 0);

        synchronizedUnits();

        //把视觉特效给aoe
        if (aoe.model.prefab != ""){
            GameObject aoeEffect = Instantiate<GameObject>(
                Resources.Load<GameObject>("Prefabs/" + aoe.model.prefab),
                new Vector3(),
                Quaternion.identity,
                viewContainer.transform
            );
            aoeEffect.transform.localPosition = new Vector3();
            aoeEffect.transform.localRotation = Quaternion.identity;
        }
    }

    ///<summary>
    ///改变aoe视觉的尺寸
    ///</summary>
    public void SetViewScale(float scaleX = 1, float scaleY = 1, float scaleZ = 1){
        synchronizedUnits();
        viewContainer.transform.localScale.Set(scaleX, scaleY, scaleZ);
    }

    ///<summary>
    ///改变图形的y高度
    ///</summary>
    public void ModViewY(float toY){
        this.viewContainer.transform.position = new Vector3(
            viewContainer.transform.position.x,
            toY,
            viewContainer.transform.position.z
        );
    }
}