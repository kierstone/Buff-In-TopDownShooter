using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///负责aoe的移动、生命周期等
///还负责aoe和角色、子弹的碰撞，需要加aoe碰撞也在这里。值得注意的是：aoe是主体
///</summary>
public class AoeManager : MonoBehaviour {
    private void FixedUpdate() {
        GameObject[] aoe = GameObject.FindGameObjectsWithTag("AoE");
        if (aoe.Length <= 0) return;
        GameObject[] cha = GameObject.FindGameObjectsWithTag("Character");
        GameObject[] bullet = GameObject.FindGameObjectsWithTag("Bullet");

        float timePassed = Time.fixedDeltaTime;

        for (int i = 0; i < aoe.Length; i++){
            AoeState aoeState = aoe[i].GetComponent<AoeState>();
            if (!aoeState) continue;

            //首先是aoe的移动
            if (aoeState.tween != null){
                AoeMoveInfo aoeMoveInfo = aoeState.tween(aoe[i], aoeState.tweenRunnedTime);
                aoeState.tweenRunnedTime += timePassed;
                aoeState.SetMoveAndRotate(aoeMoveInfo);
            }
            
            if (aoeState.justCreated == true){
                //刚创建的，走onCreate
                aoeState.justCreated = false;
                if (aoeState.model.onCreate != null){
                    aoeState.model.onCreate(aoe[i]);
                }
                
            }else{
                //已经创建完成的
                //先抓角色离开事件
                List<GameObject> leaveCha = new List<GameObject>();
                for (int m = 0; m < aoeState.characterInRange.Count; m++){
                    if (
                        aoeState.characterInRange[m] &&
                        Utils.InRange(
                            aoe[i].transform.position.x, aoe[i].transform.position.z, 
                            aoeState.characterInRange[m].gameObject.transform.position.x, aoeState.characterInRange[m].gameObject.transform.position.z,
                            aoeState.radius
                        ) == false
                    ){
                        leaveCha.Add(aoeState.characterInRange[m]);
                    }
                }
                for (int m = 0; m < leaveCha.Count; m++){
                    aoeState.characterInRange.Remove(leaveCha[m]);
                }
                if (aoeState.model.onChaLeave != null){
                    aoeState.model.onChaLeave(aoe[i], leaveCha);
                }

                //再看进入的角色
                List<GameObject> enterCha = new List<GameObject>();
                for (int m = 0; m < cha.Length; m++){
                    if (
                        cha[m] &&
                        aoeState.characterInRange.IndexOf(cha[m]) < 0 &&
                        Utils.InRange(
                            aoe[i].transform.position.x, aoe[i].transform.position.z, 
                            cha[m].transform.position.x, cha[m].transform.position.z,
                            aoeState.radius
                        ) == true
                    ){
                        enterCha.Add(cha[m]);
                    }
                }
                if (aoeState.model.onChaEnter != null){
                    aoeState.model.onChaEnter(aoe[i], enterCha);
                }
                for (int m = 0; m < enterCha.Count; m++){
                    if (enterCha[m] != null && enterCha[m].GetComponent<ChaState>() && enterCha[m].GetComponent<ChaState>().dead == false){
                        aoeState.characterInRange.Add(enterCha[m]);
                    }
                }

                //子弹离开
                List<GameObject> leaveBullet = new List<GameObject>();
                for (int m = 0; m < aoeState.bulletInRange.Count; m++){
                    if (
                        aoeState.bulletInRange[m] &&
                        Utils.InRange(
                            aoe[i].transform.position.x, aoe[i].transform.position.z, 
                            aoeState.bulletInRange[m].gameObject.transform.position.x, aoeState.bulletInRange[m].gameObject.transform.position.z,
                            aoeState.radius
                        ) == false
                    ){
                        leaveBullet.Add(aoeState.bulletInRange[m]);
                    }
                }
                for (int m = 0; m < leaveBullet.Count; m++){
                    aoeState.bulletInRange.Remove(leaveBullet[m]);
                }
                if (aoeState.model.onBulletLeave != null){
                    aoeState.model.onBulletLeave(aoe[i], leaveBullet);
                }

                //子弹进入
                List<GameObject> enterBullet = new List<GameObject>();
                for (int m = 0; m < bullet.Length; m++){
                    if (
                        bullet[m] &&
                        aoeState.bulletInRange.IndexOf(bullet[m]) < 0 &&
                        Utils.InRange(
                            aoe[i].transform.position.x, aoe[i].transform.position.z, 
                            bullet[m].transform.position.x, bullet[m].transform.position.z,
                            aoeState.radius
                        ) == true
                    ){
                        enterBullet.Add(bullet[m]);
                    }
                }
                if (aoeState.model.onBulletEnter != null){
                    aoeState.model.onBulletEnter(aoe[i], enterBullet);
                }
                for (int m = 0; m < enterBullet.Count; m++){
                    if (enterBullet[m] != null){
                        aoeState.bulletInRange.Add(enterBullet[m]);
                    }
                }
            }
            //然后是aoe的duration
            aoeState.duration -= timePassed;
            aoeState.livedTime += timePassed;
            if (aoeState.duration <= 0 || aoeState.HitObstacle() == true){
                if (aoeState.model.onRemoved != null){
                    aoeState.model.onRemoved(aoe[i]);
                }
                Destroy(aoe[i]);
                continue;
            }else{
                 //最后是onTick，remove
                if (
                    aoeState.model.tickTime > 0 && aoeState.model.onTick != null &&
                    Mathf.RoundToInt(aoeState.duration * 1000) % Mathf.RoundToInt(aoeState.model.tickTime * 1000) == 0
                ){
                    aoeState.model.onTick(aoe[i]);
                }
            }

           
        }

            
    }
}