using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignerScripts
{
    ///<summary>
    ///buff的效果
    ///</summary>
    public class Buff{
        public static Dictionary<string, BuffOnOccur> onOccurFunc = new Dictionary<string, BuffOnOccur>(){
            
        };
        public static Dictionary<string, BuffOnRemoved> onRemovedFunc = new Dictionary<string, BuffOnRemoved>(){
            {"TeleportCarrier", TeleportCarrier}
        };
        public static Dictionary<string, BuffOnTick> onTickFunc = new Dictionary<string, BuffOnTick>(){
            {"BarrelDurationLose", BarrelDurationLose}
        };
        public static Dictionary<string, BuffOnCast> onCastFunc = new Dictionary<string, BuffOnCast>(){
            {"ReloadAmmo", ReloadAmmo},
            {"FireTeleportBullet", FireTeleportBullet}
        };
        public static Dictionary<string, BuffOnHit> onHitFunc = new Dictionary<string, BuffOnHit>(){
            
        };
        public static Dictionary<string, BuffOnBeHurt> beHurtFunc = new Dictionary<string, BuffOnBeHurt>(){
            {"OnlyTakeOneDirectDamage", OnlyTakeOneDirectDamage}
        };
        public static Dictionary<string, BuffOnKill> onKillFunc = new Dictionary<string, BuffOnKill>(){
            
        };
        public static Dictionary<string, BuffOnBeKilled> beKilledFunc = new Dictionary<string, BuffOnBeKilled>(){
            {"BarrelExplosed", BarrelExplosed}
        };


        ///<summary>
        ///onCast
        ///如果子弹不够放技能，就会填装子弹
        ///no params
        ///</summary>
        private static TimelineObj ReloadAmmo(BuffObj buff, SkillObj skill, TimelineObj timeline){
            ChaState cs = buff.carrier.GetComponent<ChaState>();
            return (cs.resource.Enough(skill.model.cost) == true) ? timeline : 
                new TimelineObj(DesingerTables.Timeline.data["skill_reload"], buff.carrier, new object[0]);
        }

        ///<summary>
        ///onCast
        ///判断自己的param的"firedBullet"，如果firedBullet已经不存在了，或者压根不存在，就发射子弹，否则，就传送过去，参数：
        ///["firedBullet"]GameObject：firedBullet，理论上也可以是别的东西
        ///</summary>
        private static TimelineObj FireTeleportBullet(BuffObj buff, SkillObj skill, TimelineObj timeline){
            if (skill.model.id != "teleportBullet") return timeline;
            GameObject firedBullet = buff.buffParam.ContainsKey("firedBullet") ? (GameObject)buff.buffParam["firedBullet"] : null;
            ChaState cs = buff.carrier.GetComponent<ChaState>();
            
            if (firedBullet == null){
                buff.buffParam["firedBullet"] = null;
                return timeline;
            }else{
                if (cs == null || SceneVariants.map.CanUnitPlacedHere(firedBullet.transform.position, cs.property.bodyRadius, cs.property.moveType) == false){
                    SceneVariants.PopUpStringOnCharacter(buff.carrier, "<color=red>无法传送</color>");
                    return null;    //如果没有角色了，或者说飞弹的位置不能传送，那么就返回一个空，也就是不让放技能
                }
                return new TimelineObj(DesingerTables.Timeline.data["skill_teleportBullet_tele"], timeline.caster, null);
            }
        }

        ///<summary>
        ///onRemoved
        ///把buff的carrier传送到记录的子弹的世界坐标（非常危险，因为那个坐标未必能站立），并且移除掉那个子弹
        ///</summary>
        private static void TeleportCarrier(BuffObj buff){
            ChaState cs = buff.carrier.GetComponent<ChaState>();
            if (cs.dead) return;
            List<BuffObj> fireRec = cs.GetBuffById("TeleportBulletPassive", new List<GameObject>(){buff.caster});
            if (fireRec.Count <= 0) return;
            GameObject bullet = fireRec[0].buffParam.ContainsKey("firedBullet") ? (GameObject)fireRec[0].buffParam["firedBullet"] : null;
            if (bullet == null) return;
            buff.carrier.transform.position = new Vector3(bullet.transform.position.x, 0, bullet.transform.position.z);
            SceneVariants.RemoveBullet(bullet);
        }

        ///<summary>
        ///beHurt
        ///buff的Carrier只能受到1点直接伤害，免疫其他一切，桶子就是这样的
        ///</summary>
        private static void OnlyTakeOneDirectDamage(BuffObj buff, ref DamageInfo damageInfo, GameObject attacker){
            bool isDirectDamage = false;
            for (int i = 0; i < damageInfo.tags.Length; i++){
                if (damageInfo.tags[i] == DamageInfoTag.directDamage){
                    isDirectDamage = true;
                    break;
                }
            }
            if (isDirectDamage == true && damageInfo.DamageValue(false) > 0){
                int finalDV = 1;
                if (attacker != null){
                    ChaState cs = attacker.GetComponent<ChaState>();
                    //来自另外一个桶子（不包含自己）的伤害为9999，其他的都是1
                    if (cs != null && cs.HasTag("Barrel") == true && attacker.Equals(buff.carrier) == false){
                        finalDV = 9999;
                    }
                }
                damageInfo.damage = new Damage(0, finalDV);
            }else{
                damageInfo.damage = new Damage(0);
            }
        }
        ///<summary>
        ///onTick
        ///桶子每5秒对自己伤害，其实可以写一个公用的dot，不过这里表达的是，不公用也没问题
        ///</summary>
        private static void BarrelDurationLose(BuffObj buff){
            SceneVariants.CreateDamage(buff.carrier, buff.carrier, new Damage(0,1), 0, 0, new DamageInfoTag[]{DamageInfoTag.directDamage});
        }
        ///<summary>
        ///beKilled
        ///死亡后爆炸，对敌人造成伤害，其他桶子也是其他敌人，所以不必特殊处理，beHurt已经特殊处理了，当然还要立即清除掉这个桶子。
        ///</summary>
        private static void BarrelExplosed(BuffObj buff, DamageInfo damageInfo, GameObject attacker){
            GameObject aoeCaster = buff.caster != null ? buff.caster : buff.carrier;
            //AoeModel是可以动态生成的
            SceneVariants.CreateAoE(new AoeLauncher(
                new AoeModel(
                    "BoomExplosive", "", new string[0], 0, false,
                    "CreateSightEffect", new object[]{"Effect/Explosion_A"},
                    "BarrelExplosed", new object[0], 
                    "", new object[0],  //tick
                    "", new object[0],  //chaEnter
                    "", new object[0],  //chaLeave
                    "", new object[0],  //bulletEnter
                    "", new object[0]   //bulletLeave
                ), 
                aoeCaster, buff.carrier.transform.position, 2.2f, 0.5f, 0,
                null, null, new Dictionary<string, object>(){
                    {"Barrel", buff.carrier}
                }
            ));
            //隐藏自己，反正后面会被Remover移走
            buff.carrier.transform.localScale = Vector3.zero;
        }
    }
}