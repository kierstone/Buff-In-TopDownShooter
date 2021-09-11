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
            
        };
        public static Dictionary<string, BuffOnTick> onTickFunc = new Dictionary<string, BuffOnTick>(){
            
        };
        public static Dictionary<string, BuffOnCast> onCastFunc = new Dictionary<string, BuffOnCast>(){
            {"ReloadAmmo", ReloadAmmo}
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
    }
}