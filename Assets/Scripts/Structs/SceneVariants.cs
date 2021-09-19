using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneVariants{
    public static MapInfo map;

    //随机创建一个地图
    public static void RandomMap(int mapWidth, int mapHeight, float waterline = 6.00f){
        GridInfo grass = new GridInfo("Terrain/Grass");
        GridInfo water = new GridInfo("Terrain/Water", false);
        GridInfo[,] mGrids = new GridInfo[mapWidth, mapHeight];
        for (var i = 0; i < mapWidth; i++){
            for (var j = 0; j < mapHeight; j++){
                float pValue = Mathf.PerlinNoise(i / mapWidth, j / mapHeight) * Random.Range(10.00f, 20.00f);
                mGrids[i, j] = (pValue <= waterline) ? water : grass;
            }
        }
        map = new MapInfo(mGrids, Vector2.one);
    }


    ///<summary>
    ///获得当前的主角gameObject
    ///</summary>
    public static GameObject MainActor(){
        return GameObject.Find("GameManager").GetComponent<GameManager>().mainActor;
    }

    ///<summary>
    ///创建一个子弹到场景
    ///</summary>
    public static void CreateBullet(BulletLauncher bulletLauncher){
        GameObject.Find("GameManager").GetComponent<GameManager>().CreateBullet(bulletLauncher);
    }

    ///<summary>
    ///删除一个存在的子弹Object
    ///<param name="aoe">子弹的GameObject</param>
    ///<param name="immediately">是否当场清除，如果false，就是把时间变成0</param>
    ///</summary>
    public static void RemoveBullet(GameObject bullet, bool immediately = false){
        GameObject.Find("GameManager").GetComponent<GameManager>().RemoveBullet(bullet, immediately);
    }

    ///<summary>
    ///创建一个aoe对象在场景上
    ///<param name="aoeLauncher">aoe的创建信息</param>
    ///</summary>
    public static void CreateAoE(AoeLauncher aoeLauncher){
        GameObject.Find("GameManager").GetComponent<GameManager>().CreateAoE(aoeLauncher);
    }

    ///<summary>
    ///删除一个存在的aoeObject
    ///<param name="aoe">aoe的GameObject</param>
    ///<param name="immediately">是否当场清除，如果false，就是把时间变成0</param>
    ///</summary>
    public static void RemoveAoE(GameObject aoe, bool immediately = false){
        GameObject.Find("GameManager").GetComponent<GameManager>().RemoveAoE(aoe, immediately);
    }

    ///<summary>
    ///添加一个timeline
    ///<param name="timelineModel">要添加的timeline的model</param>
    ///<param name="caster">timeline的负责人</param>
    ///<param name="source">添加的源数据，比如技能就是skillObj</param>
    ///</summary>
    public static void CreateTimeline(TimelineModel timelineModel, GameObject caster, object source){
        GameObject.Find("GameManager").GetComponent<TimelineManager>().AddTimeline(timelineModel, caster, source);
    }

    ///<summary>
    ///添加一个timeline
    ///<param name="timelineObj">要添加的timeline</param>
    ///</summary>
    public static void CreateTimeline(TimelineObj timeline){
        GameObject.Find("GameManager").GetComponent<TimelineManager>().AddTimeline(timeline);
    }

    ///<summary>
    ///创建一个视觉特效在场景上
    ///<param name="prefab">特效的prefab文件夹，约定就在Prefabs/下，所以路径不应该加这段</param>
    ///<param name="pos">创建的位置</param>
    ///<param name="degree">角度</param>
    ///<param name="key">特效的key，如果重复则无法创建，删除的时候也有用，空字符串的话不加入管理</param>
    ///<param name="loop">是否循环，循环的得手动remove</param>
    ///</summary>
    public static void CreateSightEffect(string prefab, Vector3 pos, float degree, string key = "", bool loop = false){
        GameObject.Find("GameManager").GetComponent<GameManager>().CreateSightEffect(prefab, pos, degree, key, loop);
    }

    ///<summary>
    ///删除一个视觉特效在场景上
    ///<param name="key">特效的key</param>
    ///</summary>
    public static void RemoveSightEffect(string key){
        GameObject.Find("GameManager").GetComponent<GameManager>().RemoveSightEffect(key);
    }

    ///<summary>
    ///添加一个damageInfo
    ///<param name="attacker">攻击者，可以为null</param>
    ///<param name="target">挨打对象</param>
    ///<param name="damage">基础伤害值</param>
    ///<param name="damageDegree">伤害的角度</param>
    ///<param name="criticalRate">暴击率，0-1</param>
    ///<param name="tags">伤害信息类型</param>
    ///</summary>
    public static void CreateDamage(GameObject attacker, GameObject target, Damage damage, float damageDegree, float criticalRate, DamageInfoTag[] tags){
        GameObject.Find("GameManager").GetComponent<DamageManager>().DoDamage(attacker, target, damage, damageDegree, criticalRate, tags);
    }

    ///<summary>
    ///创建一个角色到场上
    ///<param name="prefab">特效的prefab文件夹，约定就在Prefabs/Character/下，所以路径不应该加这段</param>
    ///<param name="unitAnimInfo">角色的动画信息</param>
    ///<param name="side">所属阵营</param>
    ///<param name="pos">创建的位置</param>
    ///<param name="degree">角度</param>
    ///<param name="baseProp">初期的基础属性</param>
    ///<param name="tags">角色的标签，分类角色用的</param>
    ///</summary>
    public static GameObject CreateCharacter(string prefab, int side, Vector3 pos, ChaProperty baseProp, float degree, string unitAnimInfo = "Default_Gunner", string[] tags = null){
        return GameObject.Find("GameManager").GetComponent<GameManager>().CreateCharacter(prefab, side, pos, baseProp, degree, unitAnimInfo, tags);
    }

    ///<summary>
    ///在指定角色身上跳一个伤害或者治疗的数字，要跳别的走别的函数
    ///<param name="cha">目标角色</param>
    ///<param name="value">伤害数字，或者治疗数字</param>
    ///<param name="asHeal">是否是治疗数字，如果是就用绿字，前面带+；如果不是，用红色前面-</param>
    ///<param name="asCritical">是否暴击，暴击数字会变大，并且加个感叹号</param>
    ///</summary>
    public static void PopUpNumberOnCharacter(GameObject cha, int value, bool asHeal = false, bool asCritical = false){
        GameObject.Find("Canvas").GetComponent<PopTextManager>().PopUpNumberOnCharacter(cha, value, asHeal, asCritical);
    }

    ///<summary>
    ///在指定角色身上跳一个文字
    ///<param name="cha">目标角色</param>
    ///<param name="text">要跳出来的文字，格式什么都靠这个了</param>
    ///<param name="size">字体大小</param>
    ///</summary>
    public static void PopUpStringOnCharacter(GameObject cha, string text, int size = 30){
        GameObject.Find("Canvas").GetComponent<PopTextManager>().PopUpStringOnCharacter(cha, text, size);
    }
}