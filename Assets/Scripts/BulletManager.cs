using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///负责子弹的一切，包括移动、生命周期等
///还负责子弹和角色等的碰撞，需要加入子弹与子弹碰撞也在这里。值得注意的是：子弹是主体
///</summary>
public class BulletManager : MonoBehaviour{
    private void FixedUpdate() {
        
        GameObject[] bullet = GameObject.FindGameObjectsWithTag("Bullet");
        if (bullet.Length <= 0) return;
        GameObject[] character = GameObject.FindGameObjectsWithTag("Character");
        if (bullet.Length <= 0 || character.Length <= 0) return;

        float timePassed = Time.fixedDeltaTime;

        for (int i = 0; i < bullet.Length; i++){
            BulletState bs = bullet[i].GetComponent<BulletState>();
            if (!bs || bs.hp <= 0) continue;

            //处理子弹命中纪录信息
            int hIndex = 0;
            while (hIndex < bs.hitRecords.Count){
                bs.hitRecords[hIndex].timeToCanHit -= timePassed;
                if (bs.hitRecords[hIndex].timeToCanHit <= 0 || bs.hitRecords[hIndex].target == null){
                    //理论上应该支持可以鞭尸，所以即使target dead了也得留着……
                    bs.hitRecords.RemoveAt(hIndex);
                }else{
                    hIndex += 1;
                }
            }

            //处理子弹的移动信息
            bs.SetMoveForce(
                bs.tween == null ? Vector3.forward : bs.tween(bs.worked, bullet[i], bs.followingTarget)
            );

            //处理子弹的碰撞信息，如果子弹可以碰撞，才会执行碰撞逻辑
            if (bs.canHitAfterCreated > 0) {
                bs.canHitAfterCreated -= timePassed;  
            }else{
                float bRadius = bs.model.radius;
                int bSide = -1;
                if (bs.caster){
                    ChaState bcs = bs.caster.GetComponent<ChaState>();
                    if (bcs){
                        bSide = bcs.side;
                    }
                }

                for (int j = 0; j < character.Length; j++){
                    if (bs.CanHit(character[j]) == false) continue;

                    ChaState cs = character[j].GetComponent<ChaState>();
                    if (!cs || cs.dead == true || cs.immuneTime > 0) continue;

                    if (bSide == cs.side) continue;
                    
                    float cRadius = cs.property.hitRadius;
                    Vector3 dis = bullet[i].transform.position - character[j].transform.position;
                    
                    if (Mathf.Pow(dis.x, 2) + Mathf.Pow(dis.z, 2) <= Mathf.Pow(bRadius + cRadius, 2)){
                        //命中了
                        bs.hp -= 1;

                        UnitAnim ua = character[j].GetComponent<UnitAnim>();
                        if (ua){
                            ua.Play("Hurt");
                        }

                        if (bs.model.onHit != null){
                            bs.model.onHit(bullet[i],character[j]);
                        }
                        
                        if (bs.hp > 0){
                            bs.AddHitRecord(character[j]);
                        }else{
                            Destroy(bullet[i]);
                            continue;
                        }
                    }
                }
            }

            ///生命周期的结算
            bs.duration -= timePassed;
            bs.worked += timePassed;
            if (bs.duration <= 0 || bs.HitObstacle() == true){
                if (bs.model.onRemoved != null){
                    bs.model.onRemoved(bullet[i]);
                }
                Destroy(bullet[i]);
                continue;
            }
        }
    }
}