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
            
        };
        public static Dictionary<string, BuffOnCast> onCastFunc = new Dictionary<string, BuffOnCast>(){
            {"ReloadAmmo", ReloadAmmo},
            {"FireTeleportBullet", FireTeleportBullet}
        };
        public static Dictionary<string, BuffOnHit> onHitFunc = new Dictionary<string, BuffOnHit>(){
            
        };
        public static Dictionary<string, BuffOnBeHurt> beHurtFunc = new Dictionary<string, BuffOnBeHurt>(){
            
        };
        public static Dictionary<string, BuffOnKill> onKillFunc = new Dictionary<string, BuffOnKill>(){
            
        };
        public static Dictionary<string, BuffOnBeKilled> beKilledFunc = new Dictionary<string, BuffOnBeKilled>(){
            
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
                //if (cs) cs.AddBuff(new AddBuffInfo(buff.model, buff.caster, buff.carrier, -9999, 0, true, false));
                buff.buffParam["firedBullet"] = null;
                return timeline;
            }else{
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
            Debug.Log("Telepor to" + bullet.transform.position + " from " + buff.carrier.transform.position);
            buff.carrier.transform.position = new Vector3(bullet.transform.position.x, 0, bullet.transform.position.z);
            Debug.Log("Telepor at" + cs.controlState.canMove + "|" + cs.controlState.canRotate + "|" + cs.controlState.canUseSkill);
            SceneVariants.RemoveBullet(bullet);
        }
    }
}