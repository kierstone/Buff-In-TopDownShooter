using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
///用于添加一条buff的信息
///</summary>
public struct AddBuffInfo{
    ///<summary>
    ///buff的负责人是谁，可以是null
    ///</summary>
    public GameObject caster;

    ///<summary>
    ///buff要添加给谁，这个必须有
    ///</summary>
    public GameObject target;

    ///<summary>
    ///buff的model，这里当然可以从数据里拿，也可以是逻辑脚本现生成的
    ///</summary>
    public BuffModel buffModel;

    ///<summary>
    ///要添加的层数，负数则为减少
    ///</summary>
    public int addStack;

    ///<summary>
    ///关于时间，是改变还是设置为, true代表设置为，false代表改变
    ///</summary>
    public bool durationSetTo;

    ///<summary>
    ///是否是一个永久的buff，即便=true，时间设置也是有意义的，因为时间如果被减少到0以下，即使是永久的也会被删除
    ///</summary>
    public bool permanent;

    ///<summary>
    ///时间值，设置为这个值，或者加上这个值，单位：秒
    ///</summary>
    public float duration;

    ///<summary>
    ///buff的一些参数，这些参数是逻辑使用的，比如wow中牧师的盾还能吸收多少伤害，就可以记录在buffParam里面
    ///</summary>
    public Dictionary<string, object> buffParam;

    public AddBuffInfo(
        BuffModel model, GameObject caster, GameObject target,
        int stack, float duration, bool durationSetTo = true,
        bool permanent = false,
        Dictionary<string, object> buffParam = null
    ){
        this.buffModel = model;
        this.caster = caster;
        this.target = target;
        this.addStack = stack;
        this.duration = duration;
        this.durationSetTo = durationSetTo;
        this.buffParam = buffParam;
        this.permanent = permanent;
    }
}


///<summary>
///游戏中运行的、角色身上存在的buff
///</summary>
public class BuffObj{
    ///<summary>
    ///这是个什么buff
    ///</summary>
    public BuffModel model;

    ///<summary>
    ///剩余多久，单位：秒
    ///</summary>
    public float duration;

    ///<summary>
    ///是否是一个永久的buff，永久的duration不会减少，但是timeElapsed还会增加
    ///</summary>
    public bool permanent;

    ///<summary>
    ///当前层数
    ///</summary>
    public int stack;

    ///<summary>
    ///buff的施法者是谁，当然可以是空的
    ///</summary>
    public GameObject caster;

    ///<summary>
    ///buff的携带者，实际上是作为参数传递给脚本用，具体是谁，可定是所在控件的this.gameObject了
    ///</summary>
    public GameObject carrier;

    ///<summary>
    ///buff已经存在了多少时间了，单位：秒
    ///</summary>
    public float timeElapsed = 0.00f;

    ///<summary>
    ///buff执行了多少次onTick了，如果不会执行onTick，那将永远是0
    ///</summary>
    public int ticked = 0;

    ///<summary>
    ///buff的一些参数，这些参数是逻辑使用的，比如wow中牧师的盾还能吸收多少伤害，就可以记录在buffParam里面
    ///</summary>
    public Dictionary<string, object> buffParam = new Dictionary<string, object>();

    public BuffObj(
        BuffModel model, GameObject caster, GameObject carrier,  float duration, int stack, bool permanent = false,
        Dictionary<string, object> buffParam = null
    ){
        this.model = model;
        this.caster = caster;
        this.carrier = carrier;
        this.duration = duration;
        this.stack = stack;
        this.permanent = permanent;
        if (buffParam != null) {
            foreach(KeyValuePair<string, object> kv in buffParam){
                this.buffParam.Add(kv.Key, kv.Value);
            }
        }
    }
}

///<summary>
///策划填表的内容
///</summary>
public struct BuffModel{
    ///<summary>
    ///buff的id
    ///</summary>
    public string id;

    ///<summary>
    ///buff的名称
    ///</summary>
    public string name;

    ///<summary>
    ///buff的优先级，优先级越低的buff越后面执行，这是一个非常重要的属性
    ///比如经典的“吸收50点伤害”和“受到的伤害100%反弹给攻击者”应该反弹多少，取决于这两个buff的priority谁更高
    ///</summary>
    public int priority;

    ///<summary>
    ///buff堆叠的规则中需要的层数，在这个游戏里只要id和caster相同的buffObj就可以堆叠
    ///激战2里就不同，尽管图标显示堆叠，其实只是统计了有多少个相同id的buffObj作为层数显示了
    ///</summary>
    public int maxStack;

    ///<summary>
    ///buff的tag
    ///</summary>
    public string[] tags;

    ///<summary>
    ///buff的工作周期，单位：秒。
    ///每多少秒执行工作一次，如果<=0则代表不会周期性工作，只要>0，则最小值为Time.FixedDeltaTime。
    ///</summary>
    public float tickTime;

    ///<summary>
    ///buff会给角色添加的属性，这些属性根据这个游戏设计只有2种，plus和times，所以这个数组实际上只有2维
    ///</summary>
    public ChaProperty[] propMod;

    ///<summary>
    ///buff对于角色的ChaControlState的影响
    ///</summary>
    public ChaControlState stateMod;

    ///<summary>
    ///buff在被添加、改变层数时候触发的事件
    ///<param name="buff">会传递给脚本buffObj作为参数</param>
    ///<param name="modifyStack">会传递本次改变的层数</param>
    ///</summary>
    public BuffOnOccur onOccur;
    public object[] onOccurParams;

    ///<summary>
    ///buff在每个工作周期会执行的函数，如果这个函数为空，或者tickTime<=0，都不会发生周期性工作
    ///<param name="buff">会传递给脚本buffObj作为参数</param>
    ///</summary>
    public BuffOnTick onTick;
    public object[] onTickParams;

    ///<summary>
    ///在这个buffObj被移除之前要做的事情，如果运行之后buffObj又不足以被删除了就会被保留
    ///<param name="buff">会传递给脚本buffObj作为参数</param>
    ///</summary>
    public BuffOnRemoved onRemoved;
    public object[] onRemovedParams;

    ///<summary>
    ///在释放技能的时候运行的buff，执行这个buff获得最终技能要产生的Timeline
    ///<param name="buff">会传递给脚本的buffObj</param>
    ///<param name="skill">即将释放的技能skillObj</param>
    ///<param name="timeline">释放出来的技能，也就是一个timeline，这里的本质就是让你通过buff还能对timeline进行hack以达到修改技能效果的目的</return>
    ///</summary>
    public BuffOnCast onCast;
    public object[] onCastParams;

    ///<summary>
    ///在伤害流程中，持有这个buff的人作为攻击者会发生的事情
    ///<param name="buff">会传递给脚本buffObj作为参数</param>
    ///<param name="damageInfo">这次的伤害信息</param>
    ///<param name="target">挨打的角色对象</param>
    ///</summary>
    public BuffOnHit onHit;
    public object[] onHitParams;

    ///<summary>
    ///在伤害流程中，持有这个buff的人作为挨打者会发生的事情
    ///<param name="buff">会传递给脚本buffObj作为参数</param>
    ///<param name="damageInfo">这次的伤害信息</param>
    ///<param name="attacker">打我的角色，当然可以是空的</param>
    ///</summary>
    public BuffOnBeHurt onBeHurt;
    public object[] onBeHurtParams;

    ///<summary>
    ///在伤害流程中，如果击杀目标，则会触发的啥事情
    ///<param name="buff">会传递给脚本buffObj作为参数</param>
    ///<param name="damageInfo">这次的伤害信息</param>
    ///<param name="target">挨打的角色对象</param>
    ///</summary>
    public BuffOnKill onKill;
    public object[] onKillParams;

    ///<summary>
    ///在伤害流程中，持有这个buff的人被杀死了，会触发的事情
    ///<param name="buff">会传递给脚本buffObj作为参数</param>
    ///<param name="damageInfo">这次的伤害信息</param>
    ///<param name="attacker">发起攻击造成击杀的角色对象</param>
    ///</summary>
    public BuffOnBeKilled onBeKilled;
    public object[] onBeKilledParams;
    
    public BuffModel(
        string id, string name, string[] tags, int priority, int maxStack, float tickTime,
        string onOccur, object[] occurParam,
        string onRemoved, object[] removedParam,
        string onTick, object[] tickParam,
        string onCast, object[] castParam,
        string onHit, object[] hitParam,
        string beHurt, object[] hurtParam,
        string onKill, object[] killParam,
        string beKilled, object[] beKilledParam,
        ChaControlState stateMod, ChaProperty[] propMod = null
    ){
        this.id = id;
        this.name = name;
        this.tags = tags;
        this.priority = priority;
        this.maxStack = maxStack;
        this.stateMod = stateMod;
        this.tickTime = tickTime;
        
        this.propMod = new ChaProperty[2]{
            ChaProperty.zero,
            ChaProperty.zero
        };
        if (propMod != null){
            for (int i = 0; i < Mathf.Min(2, propMod.Length); i++){
                this.propMod[i] = propMod[i];
            }
        }

        this.onOccur = (onOccur == "") ? null : DesignerScripts.Buff.onOccurFunc[onOccur];
        this.onOccurParams = occurParam;
        this.onRemoved = (onRemoved == "") ? null : DesignerScripts.Buff.onRemovedFunc[onRemoved];
        this.onRemovedParams = removedParam;
        this.onTick = (onTick == "") ? null : DesignerScripts.Buff.onTickFunc[onTick];
        this.onTickParams = tickParam;
        this.onCast = (onCast == "") ? null : DesignerScripts.Buff.onCastFunc[onCast];
        this.onCastParams = castParam;
        this.onHit = (onHit == "") ? null : DesignerScripts.Buff.onHitFunc[onHit];
        this.onHitParams = hitParam;
        this.onBeHurt = (beHurt == "") ? null: DesignerScripts.Buff.beHurtFunc[beHurt];
        this.onBeHurtParams = hurtParam;        
        this.onKill = (onKill == "") ? null : DesignerScripts.Buff.onKillFunc[onKill];
        this.onKillParams = killParam;
        this.onBeKilled = (beKilled == "") ? null : DesignerScripts.Buff.beKilledFunc[beKilled];
        this.onBeKilledParams = beKilledParam;
    }
}

public delegate void BuffOnOccur(BuffObj buff, int modifyStack);
public delegate void BuffOnRemoved(BuffObj buff);
public delegate void BuffOnTick(BuffObj buff);
public delegate void BuffOnHit(BuffObj buff, ref DamageInfo damageInfo, GameObject target);
public delegate void BuffOnBeHurt(BuffObj buff, ref DamageInfo damageInfo, GameObject attacker);
public delegate void BuffOnKill(BuffObj buff, DamageInfo damageInfo, GameObject target);
public delegate void BuffOnBeKilled(BuffObj buff, DamageInfo damageInfo, GameObject attacker);
public delegate TimelineObj BuffOnCast(BuffObj buff, SkillObj skill, TimelineObj timeline);