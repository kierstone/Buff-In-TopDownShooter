using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignerScripts
{
    ///<summary>
    ///aoe的效果
    ///</summary>
    public class AoE{
        public static Dictionary<string, AoeOnCreate> onCreateFunc = new Dictionary<string, AoeOnCreate>(){
            
        };
        public static Dictionary<string, AoeOnRemoved> onRemovedFunc = new Dictionary<string, AoeOnRemoved>(){
            {"DoDamageOnRemoved", DoDamageOnRemoved}
        };
        public static Dictionary<string, AoeOnTick> onTickFunc = new Dictionary<string, AoeOnTick>(){
            
        };
        public static Dictionary<string, AoeOnCharacterEnter> onChaEnterFunc = new Dictionary<string, AoeOnCharacterEnter>(){
            {"DoDamageToEnterCha", DoDamageToEnterCha}
        };
        public static Dictionary<string, AoeOnCharacterLeave> onChaLeaveFunc = new Dictionary<string, AoeOnCharacterLeave>(){
            
        };
        public static Dictionary<string, AoeOnBulletEnter> onBulletEnterFunc = new Dictionary<string, AoeOnBulletEnter>(){
            {"BlockBullets", BlockBullets},
            {"SpaceMonkeyBallHit", SpaceMonkeyBallHit}
        };
        public static Dictionary<string, AoeOnBulletLeave> onBulletLeaveFunc = new Dictionary<string, AoeOnBulletLeave>(){
            
        };
        public static Dictionary<string, AoeTween> aoeTweenFunc = new Dictionary<string, AoeTween>(){
            {"AroundCaster", AroundCaster},
            {"SpaceMonkeyBallRolling", SpaceMonkeyBallRolling}
        };


        ///<summary>
        ///aoeTween
        ///环绕施法者旋转，参数：
        ///[0]float：距离caster的距离（单位米）
        ///[1]float：移动速度（度/秒），正负的效果是方向
        ///</summary>
        private static AoeMoveInfo AroundCaster(GameObject aoe, float t){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (aoeState == null || aoeState.caster == null) return null;
            Vector3 b = aoeState.caster.transform.position;
            
            float dis = aoeState.tweenParam.Length > 0 ? (float)aoeState.tweenParam[0] : 0;
            float degPlus = aoeState.tweenParam.Length > 1 ? (float)aoeState.tweenParam[1] : 0;
            float cDeg = degPlus * t;
            float dr = cDeg * Mathf.PI / 180; 
            
            Vector3 targetP = new Vector3(
                b.x + Mathf.Sin(dr) * dis - aoe.transform.position.x, 
                0,
                b.z + Mathf.Cos(dr) * dis - aoe.transform.position.z
            );

            //Debug.Log("Around Caster " + aoeState.GetHashCode() + " // " + b);

            return new AoeMoveInfo(MoveType.fly, targetP, cDeg % 360);
        }

        ///<summary>
        ///onBulletEnter
        ///消灭所有进入的敌人的子弹，参数：
        ///[0]bool：是否有抵挡次数限制
        ///来自AoeState.aoeParam的参数：
        ///["times"]int：抵消多少次
        ///</summary>
        private static void BlockBullets(GameObject aoe, List<GameObject> bullets){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;
            AoeModel am = aoeState.model;
            bool countLimited = am.onBulletEnterParams.Length > 0 ? (bool)am.onBulletEnterParams[0] : false;
            int times = aoeState.param.ContainsKey("times") ? (int)aoeState.param["times"] : 1;

            int side = -1;
            if (aoeState.caster){
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                side = ccs.side;
            }

            for (int i = 0; i < bullets.Count; i++){
                BulletState bs = bullets[i].GetComponent<BulletState>();
                int bSide = -1;
                if (bs && bs.caster){
                    ChaState bcs = bs.caster.GetComponent<ChaState>();
                    if (bcs) bSide = bcs.side;
                }
                if (side != bSide){
                    SceneVariants.RemoveBullet(bullets[i], false);
                    SceneVariants.CreateSightEffect("HitEffect_B", aoe.transform.position, aoe.transform.eulerAngles.y);
                }
            }

            times -= 1;
        }

        ///<summary>
        ///aoeTween
        ///小猴设计的滚球，往前滚动，受到攻击会略微转向。参数：
        ///[0]Vector3：原始的力量
        ///来自AoeState.aoeParam的参数：
        ///["forces"]List<Vector3>：被子弹施加的力
        ///</summary>
        private static AoeMoveInfo SpaceMonkeyBallRolling(GameObject aoe, float t){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return null;

            Vector3 velocity = aoeState.tweenParam.Length > 0 ? (Vector3)aoeState.tweenParam[0] : Vector3.zero;
            velocity *= Time.fixedDeltaTime; //算的是一个tick的，所以得在这里乘一下，回头再读取的地方除一下，这是因为设计者在设计这个函数时候思考环境不同所产生的必须要的“牺牲”
            List<Vector3> forces = aoeState.param.ContainsKey("forces") ? (List<Vector3>)aoeState.param["forces"] : null;
            if (forces != null){
                for (int i = 0; i < forces.Count; i++){
                    velocity += forces[i] * Time.fixedDeltaTime;
                }
            }

            float dis = Mathf.Sqrt(Mathf.Pow(velocity.x, 2) + Mathf.Pow(velocity.z, 2));
            float rr = Mathf.Atan2(velocity.x, velocity.z);
            float rotateTo = rr * 180 / Mathf.PI;

            return new AoeMoveInfo(MoveType.fly, new Vector3(Mathf.Sin(rr) * dis, 0, Mathf.Cos(rr) * dis), rotateTo);
        }

        ///<summary>
        ///onBulletEnter
        ///小猴设计的滚球，挨打后会吃到来自子弹的力，参数：
        ///[0]float：力的大小，米/秒
        ///</summary>
        private static void SpaceMonkeyBallHit(GameObject aoe, List<GameObject> bullets){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            float baseForce = aoeState.model.onBulletEnterParams.Length > 0 ? (float)aoeState.model.onBulletEnterParams[0] : 0;
            if (baseForce == 0) return;

            int side = -1;
            if (aoeState.caster){
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                side = ccs.side;
            }

            if (aoeState.param.ContainsKey("forces") == false){
                aoeState.param["forces"] = new List<Vector3>();   
            }
            for (int i = 0; i < bullets.Count; i++){
                BulletState bs = bullets[i].GetComponent<BulletState>();
                int bSide = -1;
                
                if (bs){
                    if (bs.caster){
                        ChaState bcs = bs.caster.GetComponent<ChaState>();
                        if (bcs) bSide = bcs.side;
                    }
                    if (bSide == side){
                        Vector3 bMove = bs.velocity * baseForce;    //算了，就直接乘把，凑合凑合
                        ((List<Vector3>)aoeState.param["forces"]).Add(bMove);
                        SceneVariants.RemoveBullet(bullets[i]);
                    }
                }
            }

            float scaleTo = 1 + ((List<Vector3>)aoeState.param["forces"]).Count * 0.05f;
            aoeState.radius = 0.25f * scaleTo;
            aoeState.SetViewScale(scaleTo);
            aoeState.ModViewY(aoeState.radius);
        }

        ///<summary>
        ///onChaEnter
        ///对于范围内的人造成伤害（治疗得另写一个，这是严肃的），参数：
        ///[0]Damage：基础伤害
        ///[1]float：施法者攻击倍率
        ///[2]bool：对敌人有效
        ///[3]bool：对盟军有效
        ///[4]bool：挨打的人是否受伤动作
        ///[5]string：挨打者身上特效
        ///[6]string：挨打者特效绑点，默认"Body"
        ///</summary>
        private static void DoDamageToEnterCha(GameObject aoe, List<GameObject> characters){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            object[] p = aoeState.model.onChaEnterParams;
            Damage baseDamage = p.Length > 0 ? (Damage)p[0] : new Damage(0);
            float damageTimes = p.Length > 1 ? (float)p[1] : 0;
            bool toFoe = p.Length > 2 ? (bool)p[2] : true;
            bool toAlly = p.Length > 3 ? (bool)p[3] : false;
            bool hurtAction = p.Length > 4 ? (bool)p[4] : false;
            string effect = p.Length > 5 ? (string)p[5] : "";
            string bp = p.Length > 6 ? (string)p[6] : "Body";

            Damage damage = baseDamage * (aoeState.propWhileCreate.attack * damageTimes);

            int side = -1;
            if (aoeState.caster){
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                if (ccs) side = ccs.side;
            }

            for (int i = 0; i < characters.Count; i++){
                ChaState cs = characters[i].GetComponent<ChaState>();
                if (cs && cs.dead == false && ((toFoe == true && side != cs.side) || (toAlly == true && side == cs.side))){
                    SceneVariants.CreateDamage(aoeState.caster, characters[i], damage, 0.05f, new DamageInfoTag[]{DamageInfoTag.directDamage});
                    if (hurtAction == true) cs.Play("Hurt");
                    if (effect != "") cs.PlaySightEffect(bp, effect);
                }
            }
        }

        ///<summary>
        ///onRemoved
        ///对于范围内的人造成伤害（治疗得另写一个，这是严肃的），参数：
        ///[0]Damage：基础伤害
        ///[1]float：施法者攻击倍率
        ///[2]bool：对敌人有效
        ///[3]bool：对盟军有效
        ///[4]bool：挨打的人是否受伤动作
        ///[5]string：挨打者身上特效
        ///[6]string：挨打者特效绑点，默认"Body"
        ///</summary>
        private static void DoDamageOnRemoved(GameObject aoe){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            object[] p = aoeState.model.onRemovedParams;
            Damage baseDamage = p.Length > 0 ? (Damage)p[0] : new Damage(0);
            float damageTimes = p.Length > 1 ? (float)p[1] : 0;
            bool toFoe = p.Length > 2 ? (bool)p[2] : true;
            bool toAlly = p.Length > 3 ? (bool)p[3] : false;
            bool hurtAction = p.Length > 4 ? (bool)p[4] : false;
            string effect = p.Length > 5 ? (string)p[5] : "";
            string bp = p.Length > 6 ? (string)p[6] : "Body";

            Damage damage = baseDamage * (aoeState.propWhileCreate.attack * damageTimes);

            int side = -1;
            if (aoeState.caster){
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                if (ccs) side = ccs.side;
            }

            for (int i = 0; i < aoeState.characterInRange.Count; i++){
                ChaState cs = aoeState.characterInRange[i].GetComponent<ChaState>();
                if (cs && cs.dead == false && ((toFoe == true && side != cs.side) || (toAlly == true && side == cs.side))){
                    SceneVariants.CreateDamage(aoeState.caster, aoeState.characterInRange[i], damage, 0.05f, new DamageInfoTag[]{DamageInfoTag.directDamage});
                    if (hurtAction == true) cs.Play("Hurt");
                    if (effect != "") cs.PlaySightEffect(bp, effect);
                }
            }
        }
    }

    
}