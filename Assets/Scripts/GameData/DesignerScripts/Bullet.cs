using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignerScripts
{
    ///<summary>
    ///子弹的效果
    ///</summary>
    public class Bullet{

        public static Dictionary<string, BulletOnHit> onHitFunc = new Dictionary<string, BulletOnHit>(){
            {"CommonBulletHit", Bullet.CommonBulletHit}
        };
        public static Dictionary<string, BulletOnRemoved> onRemovedFunc = new Dictionary<string, BulletOnRemoved>(){
            {"CommonBulletRemoved", CommonBulletRemoved}
        };
        public static Dictionary<string, BulletTween> bulletTween = new Dictionary<string, BulletTween>(){
            {"FollowingTarget", FollowingTarget},
        };
        public static Dictionary<string, BulletTargettingFunction> targettingFunc = new Dictionary<string, BulletTargettingFunction>(){
            {"GetNearestEnemy", GetNearestEnemy}
        };

        ///<summary>
        ///onHit
        ///普通子弹命中效果，参数：
        ///[0]伤害倍数
        ///[1]基础暴击率
        ///[2]命中视觉特效
        ///[3]播放特效位于目标的绑点，默认Body
        ///</summary>
        private static void CommonBulletHit(GameObject bullet, GameObject target){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return;
            object[] onHitParam = bulletState.model.onHitParams;
            float damageTimes = onHitParam.Length > 0 ? (float)onHitParam[0] : 1.00f;
            float critRate = onHitParam.Length > 1 ? (float) onHitParam[1] : 0.00f;
            string sightEffect = onHitParam.Length > 2 ? (string) onHitParam[2] : "";
            string bpName = onHitParam.Length > 3 ? (string) onHitParam[3] : "Body";
            if (sightEffect != ""){
                UnitBindManager ubm = target.GetComponent<UnitBindManager>();
                if (ubm){
                    ubm.AddBindGameObject(bpName, "Prefabs/Effect/"+sightEffect, "", false);
                }
            }
            SceneVariants.CreateDamage(
                bulletState.caster, 
                target,
                new Damage(Mathf.CeilToInt(damageTimes * bulletState.propWhileCast.attack)), 
                critRate,
                new DamageInfoTag[]{DamageInfoTag.directDamage, }
            );
        }

        ///<summary>
        ///onRemoved
        ///普通子结束，参数：
        ///[0]命中视觉特效
        ///</summary>
        private static void CommonBulletRemoved(GameObject bullet){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return;
            object[] onRemovedParams = bulletState.model.onRemovedParams;
            string sightEffect = onRemovedParams.Length > 0 ? (string)onRemovedParams[0] : "";
            if (sightEffect != ""){
                SceneVariants.CreateSightEffect(
                    sightEffect, 
                    bullet.transform.position, 
                    bullet.transform.rotation.eulerAngles.y
                );      
            }
        }

        ///<summary>
        ///targetting
        ///选择最近的敌人作为目标
        ///</summary>
        private static GameObject GetNearestEnemy(GameObject bullet, GameObject[] targets){
            BulletState bs = bullet.GetComponent<BulletState>();
            int side = -1;
            if (bs.caster){
                ChaState ccs = bs.caster.GetComponent<ChaState>();
                if (ccs) side = ccs.side;
            }

            GameObject bestTarget = null;
            float bestDis = float.MaxValue;
            for (int i = 0; i < targets.Length; i++){
                ChaState tcs = targets[i].GetComponent<ChaState>();
                if (!tcs || tcs.side == side || tcs.dead == true) continue;
                float dis2 = (
                    Mathf.Pow(bullet.transform.position.x - targets[i].transform.position.x, 2) +
                    Mathf.Pow(bullet.transform.position.z - targets[i].transform.position.z, 2)
                );
                if (bestDis > dis2 || bestTarget == null){
                    bestTarget = targets[i];
                    bestDis = dis2;
                }
            }

            return bestTarget;
        }
        ///<summary>
        ///tween
        ///跟踪目标
        ///</summary>
        private static Vector3 FollowingTarget(float t, GameObject bullet, GameObject target){
            Vector3 res = Vector3.forward;
            if (target!=null){
                Vector3 tarDir = target.transform.position - bullet.transform.position;
                float flyingRad = (Mathf.Atan2(tarDir.x,  tarDir.z) * 180 / Mathf.PI - bullet.transform.eulerAngles.y) * Mathf.PI / 180;

                res.x = Mathf.Sin(flyingRad);
                res.z = Mathf.Cos(flyingRad);
                
            }
            return res;
        }
    }
}