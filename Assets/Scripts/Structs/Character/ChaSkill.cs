using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///技能是角色拥有的东西，因为角色有技能，玩家或者ai才能操作角色释放技能
///</summary>
public class SkillObj{
    ///<summary>
    ///技能的模板，创建于skillModel，但运行中还是会允许改变
    ///</summary>
    public SkillModel model;

    ///<summary>
    ///技能等级
    ///</summary>
    public int level;

    ///<summary>
    ///冷却时间，单位秒。尽管游戏设计里面是没有冷却时间的，但是我们依然需要这个数据
    ///因为作为一个ARPG子分类，和ARPG游戏有一样的问题：一次按键（时间够久）会发生连续多次使用技能，所以得有一个GCD来避免问题
    ///当然和wow的gcd不同，这个“GCD”就只会让当前使用的技能进入0.1秒的冷却
    ///</summary>
    public float cooldown;

    public SkillObj(SkillModel model, int level = 1){
        this.model = model;
        this.level = level;
        this.cooldown = 0;
    }
}

///<summary>
///策划填表的技能
///</summary>
public struct SkillModel{
    ///<summary>
    ///技能的id
    ///</summary>
    public string id;

    ///<summary>
    ///技能使用的条件，这个游戏中只有资源需求，比如hp、ammo之类的
    ///</summary>
    public ChaResource condition;

    ///<summary>
    ///技能的消耗，成功之后会扣除这些资源
    ///</summary>
    public ChaResource cost;

    ///<summary>
    ///技能的效果，必然是一个timeline
    ///</summary>
    public TimelineModel effect;

    ///<summary>
    ///学会技能的时候，同时获得的buff
    ///</summary>
    public AddBuffInfo[] buff;

    public SkillModel(string id, ChaResource cost, ChaResource condition, string effectTimeline, AddBuffInfo[] buff){
        this.id = id;
        this.cost = cost;
        this.condition = condition;
        this.effect = DesingerTables.Timeline.data[effectTimeline]; //SceneVariants.desingerTables.timeline.data[effectTimeline];
        this.buff = buff;
    }
}