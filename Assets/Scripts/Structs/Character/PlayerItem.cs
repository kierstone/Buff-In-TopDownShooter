using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///游戏中的道具，其实道具实只有玩家有的，而不是某个角色有的，所以道具和角色本质上没有什么直接关系
///包括装备也是如此，我们通常以为装备是挂在角色身上的，但实际上只有玩这个游戏的主玩家（最多一起热座的几个）才有装备的概念
///而通常因为我们看到了一个角色的外貌变化、数值变化，也猜得到他肯定穿了装备（其他玩家角色），我们就认为装备和角色是直接相关的
///这都是幻术，实际上我们只是看到了一个“属性被hack”了的角色而已。
///</summary>
public struct ItemObj{
    ///<summary>
    ///道具的model，当然model并不是运行中不可变化的
    ///比如你强化了某个装备，致使起名字变化了，比如铁锹变成钢锹
    ///他不一定是非得直接删除一个itemObj再添加一个的，因为其他属性可能还需要保留
    ///就比如作为当前耐久度来使用的count
    ///</summary>
    public ItemModel model;

    ///<summary>
    ///持有的个数，因为有堆叠规则，所以才有这个
    ///</summary>
    public int count;

    ///<summary>
    ///冷却时间，单位：秒，顾名思义>0的时候道具就没法使用
    ///我们并没有在道具model看到冷却时间，那么这个数字怎么来的？
    ///其1，是游戏规则所致，比如wow里面使用道具都有1.5秒gcd，这时候使用一个道具会导致所有道具都进入1.5秒的cooldown
    ///其2，是道具的使用效果所致，比如使用了某个道具，他的效果就是导致角色身上含有某tag的道具进入5秒冷却
    ///这些规则都可以没有，那么cooldown就不需要了吗？还是留着吧，他是规则，只是我们需不需要用这个规则而已
    ///</summary>
    public float cooldown;

    public ItemObj(ItemModel model, int count = 0, float cooldown = 0){
        this.model = model;
        this.cooldown = cooldown;
        this.count = count;
    }
}


///<summary>
///道具的模板属性，这些属性都是策划填表来的，在初始化一个道具Obj的时候会派上用场
///但是道具被初始化之后，很多属性在运行中也会发生变化，所以我们不能用道具模板的地址，由此直接作为struct更合算
///因为这个demo的ui并不打算精心制作，所以包括icon这样的属性也就省略了
///</summary>
public struct ItemModel{
    ///<summary>
    ///道具id
    ///</summary>
    public string id;

    ///<summary>
    ///道具的icon
    ///</summary>
    public string icon;

    ///<summary>
    ///道具名称
    ///</summary>
    public string name;

    ///<summary>
    ///道具Tag
    ///</summary>
    public string[] tags;

    ///<summary>
    ///最大堆叠数，不是所有的游戏道具堆叠的规则都一样的
    ///我这个demo里面，可能存在药水之类的，他们的model几乎不会在运行中被改变（游戏规则如此，而非正常逻辑），所以才能堆叠
    ///我们在ui上看到一个道具图标带一个数字，未必她就真的是带有“堆叠数”这个属性的，很可能是统计了有多少个id一样的道具，按照显示规则显示成这样了罢了
    ///</summary>
    public int maxStack;

    ///<summary>
    ///对于道具而言，这是最核心的部分，就是使用效果，使用效果被抽象为一个timeline
    ///</summary>
    public TimelineModel useEffect;

    

    public ItemModel(
        string id, string icon, string name, string[] tags, 
        TimelineModel useEffect,
        int maxStack = 1
    ){
        this.id = id;
        this.name = name;
        this.icon = icon;
        this.tags = tags;
        this.maxStack = maxStack;
        this.useEffect = useEffect;
    }
}

