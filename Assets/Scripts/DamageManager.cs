using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///负责处理游戏中所有的DamageInfo
///</summary>
public class DamageManager : MonoBehaviour{
    private List<DamageInfo> damageInfos = new List<DamageInfo>();

    private void FixedUpdate() {
        int i = 0;
        while( i < damageInfos.Count ){
            DealWithDamage(damageInfos[i]);
            damageInfos.RemoveAt(0);
        }
    }

    ///<summary>
    ///处理DamageInfo的流程，也就是整个游戏的伤害流程
    ///<param name="dInfo">要处理的damageInfo</param>
    ///<retrun>处理完之后返回出一个damageInfo，依照这个，给对应角色扣血处理</return>
    ///</summary>
    private void DealWithDamage(DamageInfo dInfo){
        //如果目标已经挂了，就直接return了
        if (!dInfo.defender) return;
        ChaState defenderChaState = dInfo.defender.GetComponent<ChaState>();
        if (!defenderChaState) return;
        ChaState attackerChaState = null;
        if (defenderChaState.dead == true) 
            return;
        //先走一遍所有攻击者的onHit
        if (dInfo.attacker){
            attackerChaState = dInfo.attacker.GetComponent<ChaState>();
            for (int i = 0; i < attackerChaState.buffs.Count; i++){
                if (attackerChaState.buffs[i].model.onHit != null){
                    attackerChaState.buffs[i].model.onHit(attackerChaState.buffs[i], ref dInfo, dInfo.defender);
                }
            }
        }
        //然后走一遍挨打者的beHurt
        for (int i = 0; i < defenderChaState.buffs.Count; i++){
            if (defenderChaState.buffs[i].model.onBeHurt != null){
               defenderChaState.buffs[i].model.onBeHurt(defenderChaState.buffs[i], ref dInfo, dInfo.attacker);
            }
        }
        if (defenderChaState.CanBeKilledByDamageInfo(dInfo) == true){
            //如果角色可能被杀死，就会走OnKill和OnBeKilled，这个游戏里面没有免死金牌之类的技能，所以只要判断一次就好
            if (attackerChaState != null){
                for (int i = 0; i < attackerChaState.buffs.Count; i++){
                    if (attackerChaState.buffs[i].model.onKill != null){
                        attackerChaState.buffs[i].model.onKill(attackerChaState.buffs[i], ref dInfo, dInfo.defender);
                    }
                }
            }
            for (int i = 0; i < defenderChaState.buffs.Count; i++){
                if (defenderChaState.buffs[i].model.onBeKilled != null){
                    defenderChaState.buffs[i].model.onBeKilled(defenderChaState.buffs[i], ref dInfo, dInfo.attacker);
                }
            }
        }
        //最后根据结果处理：如果是治疗或者角色非无敌，才会对血量进行调整。
        bool isHeal = dInfo.isHeal();
        int dVal = dInfo.DamageValue(isHeal);
        if (isHeal == true || defenderChaState.immuneTime <= 0){
            if (dInfo.requireDoHurt() == true && defenderChaState.CanBeKilledByDamageInfo(dInfo) == false){
                UnitAnim ua = defenderChaState.GetComponent<UnitAnim>();
                if (ua) ua.Play("Hurt");
            }
            defenderChaState.ModResource(new ChaResource(
                -dVal
            ));
        }
        
    }

    ///<summary>
    ///添加一个damageInfo
    ///<param name="attacker">攻击者，可以为null</param>
    ///<param name="target">挨打对象</param>
    ///<param name="damage">基础伤害值
    ///<param name="criticalRate">暴击率，0-1</param>
    ///<param name="tags">伤害信息类型</param>
    ///</summary>
    public void DoDamage(GameObject attacker, GameObject target, Damage damage, float criticalRate, DamageInfoTag[] tags){
        this.damageInfos.Add(new DamageInfo(
            attacker, target, damage, criticalRate, tags
        ));
    }
}
