using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignerScripts
{
    public class Timeline{
        public static Dictionary<string, TimelineEvent> functions = new Dictionary<string, TimelineEvent>(){
            {"CasterPlayAnim", CasterPlayAnim},
            {"CasterForceMove", CasterForceMove},
            {"SetCasterControlState", SetCasterControlState},
            {"PlaySightEffectOnCaster", PlaySightEffectOnCaster},
            {"StopSightEffectOnCaster", StopSightEffectOnCaster},
            {"FireBullet", FireBullet},
            {"CasterImmune", CasterImmune},
            {"CreateAoE", CreateAoE},
            {"AddBuffToCaster", AddBuffToCaster},
            {"CasterAddAmmo", CasterAddAmmo}
        };

        ///<summary>
        ///在Caster的某个绑点(Muzzle/Head/Body)上发射一个子弹出来
        ///<param name="args">总共3个参数：
        ///[0]BulletLauncher：子弹发射信息，其中caster和position是需要获得后该写的，degree则需要加上角色的转向
        ///[1]string：角色身上绑点位置，默认Muzzle
        ///</param>
        ///</summary>
        private static void FireBullet(TimelineObj tlo, params object[] args){
            if (args.Length <= 0) return;
            
            if (tlo.caster){
                UnitBindManager ubm = tlo.caster.GetComponent<UnitBindManager>();
                if (!ubm) return;

                BulletLauncher bLauncher = (BulletLauncher)args[0];
                UnitBindPoint ubp = ubm.GetBindPointByKey(args.Length > 1 ? (string)args[1] : "Muzzle");
                if (!ubp) return;

                bLauncher.caster = tlo.caster;
                bLauncher.fireDegree = tlo.caster.transform.rotation.eulerAngles.y;
                bLauncher.firePosition = ubp.gameObject.transform.position;

                SceneVariants.CreateBullet(bLauncher);
            }
        }

        ///<summary>
        ///在caster=timeline.caster的面前位置aoe
        ///<param name="args">总共3个参数：
        ///[0]AoeLauncher：aoe的发射器，caster在这里被重新赋值，position则作为增量加给现在的角色坐标
        ///[1]bool：true=面前，false=角色坐标
        ///</param>
        ///</summary>
        private static void CreateAoE(TimelineObj tlo, params object[] args){
            if (args.Length <= 0) return;
            
            if (tlo.caster){
                UnitBindManager ubm = tlo.caster.GetComponent<UnitBindManager>();
                if (!ubm) return;

                AoeLauncher aLauncher = ((AoeLauncher)args[0]).Clone(); //必须克隆出来，去掉ref属性，使之变成临时的属性
                bool inFront = args.Length > 1 ? (bool)args[1] : true;
                
                aLauncher.caster = tlo.caster;
                aLauncher.degree += tlo.caster.transform.rotation.eulerAngles.y;

                float rr = aLauncher.degree * Mathf.PI / 180;
                Vector3 pos = aLauncher.position;
                
                float dis = Mathf.Sqrt(Mathf.Pow(pos.x, 2) + Mathf.Pow(pos.z, 2));
                if (inFront == true){
                    dis += tlo.caster.GetComponent<ChaState>().property.bodyRadius + aLauncher.radius;
                } 

                aLauncher.position.x = dis * Mathf.Sin(rr) + tlo.caster.transform.position.x;
                aLauncher.position.z = dis * Mathf.Cos(rr) + tlo.caster.transform.position.z;

                aLauncher.tweenParam = new object[]{
                    new Vector3(
                        dis * Mathf.Sin(rr),
                        0,
                        dis * Mathf.Cos(rr)
                    )
                };

                SceneVariants.CreateAoE(aLauncher);
            }
        }

        ///<summary>
        ///timelien的焦点角色播放某个动作，是否是跳转到那个动作一直播放还是会回到站立，这取决于animator里面做的，我也无能为力
        ///<param name="args">总共3个参数：
        ///[0]string：是要播放的动画
        ///[1]bool：是否要取得动画的方向，如果不要就直接用预设的值了
        ///[2]bool：是否启用当前正在进行的面向和移动角度，如果false或者缺省了，就代表启用timelineObj中储存的（开始时的）
        ///</param>
        ///</summary>
        private static void CasterPlayAnim(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                string animName = args.Length >= 1 ? (string)(args[0]) : "";

                if (animName == "") return;

                bool getTail = args.Length >= 2 ? (bool)(args[1]) : false;
                bool useCurrentDeg = args.Length >= 3 ? (bool)(args[2]) : false;
                
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    float faceDeg = useCurrentDeg == true ? cs.faceDegree : (float)tlo.GetValue("faceDegree");
                    float moveDeg = useCurrentDeg == true ? cs.moveDegree : (float)tlo.GetValue("moveDegree");
                    if (getTail == true) animName += Utils.GetTailStringByDegree(faceDeg, moveDeg);
                    cs.Play(animName); 
                }
            }
        }

        ///<summary>
        ///timeline的焦点角色强制进行移动
        ///<param name="args">总共4个参数：
        ///[0]float：想要强行移动的距离，单位：米。
        ///[1]float：在多久内完成这个移动，单位：秒。这是匀速直线移动的。
        ///[2]float：基于角色移动方向或者面向（取决于[2]），获得一个基础的移动角度偏移量。
        ///[3]bool：是否要基于角色移动方向，如果不是，就是基于角色的面朝方向。
        ///[4]bool：如果启用面向，是否启用正在进行的，而非timeline创建时的，缺省或者false代表启用timeline创建时产生的
        ///</param>
        ///</summary>
        private static void CasterForceMove(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                float dis = args.Length >= 1 ? (float)args[0] : 0.00f;
                float inSec = (args.Length >= 2 ? (float)args[1] : 0.00f) / tlo.timeScale;  //移动速度可得手动设置倍速
                float degOffset = args.Length >= 3 ? (float)args[2] : 0.00f;
                bool basedOnMoveDir = args.Length >= 4 ? (bool)args[3] : true;
                bool useCurrentDeg = args.Length >= 5 ? (bool)args[4] : false;
                
                if (cs){
                    float mr = (
                        (
                            basedOnMoveDir == true ? 
                                (useCurrentDeg == true ? cs.moveDegree : (float)tlo.GetValue("moveDegree")) : 
                                (useCurrentDeg == true ? cs.faceDegree : (float)tlo.GetValue("faceDegree"))
                        ) + degOffset
                    ) * Mathf.PI / 180.00f;

                    Vector3 mdir = new Vector3(
                        Mathf.Sin(mr) * dis,
                        0,
                        Mathf.Cos(mr) * dis
                    );
                    cs.AddForceMove(new MovePreorder(mdir, inSec));
                }
            }
        }

        ///<summary>
        ///设置timeline的焦点角色的ChaControlState
        ///<param name="args">总共3个参数：
        ///[0]bool：可否移动，如果得不到参数，就保持原值。
        ///[1]bool：可否转身，如果得不到参数，就保持原值。
        ///[2]bool：可否释放技能，如果得不到参数，就保持原值。
        ///</param>
        ///</summary>
        private static void SetCasterControlState(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    if (args.Length >= 1) cs.timelineControlState.canMove = (bool)args[0];
                    if (args.Length >= 2) cs.timelineControlState.canRotate = (bool)args[1];
                    if (args.Length >= 3) cs.timelineControlState.canUseSkill = (bool)args[2];
                }
            }
        }

        ///<summary>
        ///在timeline焦点角色身上播放一个视觉特效
        ///<param name="args">总共4个参数：
        ///[0]string：要播放特效的绑点
        ///[1]string：特效的文件名，位于Prafabs/Effect/下
        ///[2]string：特效的key，用于删除的
        ///[3]bool：是否循环播放特效（循环就要手动删除）
        ///</param>
        ///</summary>
        private static void PlaySightEffectOnCaster(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    string bindPointKey = args.Length >= 1 ? (string)args[0] : "Body";
                    string effectName = args.Length >= 2 ? (string)args[1] : "";
                    string effectKey = args.Length >= 3 ? (string)args[2] : Random.value.ToString();
                    bool loop = args.Length >= 4 ? (bool)args[3] : false;
                    cs.PlaySightEffect(bindPointKey, effectName, effectKey, loop);
                }
            }
        }

        ///<summary>
        ///在timeline焦点角色身上关闭一个视觉特效
        ///<param name="args">总共2个参数：
        ///[0]string：要关闭的特效所处绑点
        ///[1]string：特效的key，创建时产生的
        ///</param>
        ///</summary>
        private static void StopSightEffectOnCaster(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    string bindPointKey = args.Length >= 1 ? (string)args[0] : "Body";
                    string effectKey = args.Length >= 2 ? (string)args[1] : "";
                    if (effectKey == "") return;
                    cs.StopSightEffect(bindPointKey, effectKey);
                }
            }
        }

        ///<summary>
        ///设置timeline的caster身上的无敌时间
        ///<param name="args">总共1个参数：
        ///[0]float：无敌的时间，单位：秒
        ///</param>
        ///</summary>
        private static void CasterImmune(TimelineObj timelineObj, params object[] args){
            if (timelineObj.caster){
                ChaState cs = timelineObj.caster.GetComponent<ChaState>();
                if (cs && args.Length > 0){
                    float immT = (float)args[0];
                    cs.SetImmuneTime(immT);
                }
            }
        }

        ///<summary>
        ///修改timeline的caster身上的子弹数量
        ///[0]int：需要添加的数量，负数就是减少了
        ///</summary>
        private static void CasterAddAmmo(TimelineObj timelineObj, params object[] args){
            if (timelineObj.caster){
                ChaState cs = timelineObj.caster.GetComponent<ChaState>();
                if (cs && args.Length > 0){
                    int modCount = (int)args[0];
                    cs.ModResource(new ChaResource(cs.resource.hp, modCount + cs.resource.ammo, cs.resource.stamina));
                }
            }
        }

        ///<summary>
        ///给timeline的caster添加一个buff
        ///[0]AddBuffInfo：如何添加一个buff，其中caster和carrier都会是timeline.caster本身
        ///</summary>
        private static void AddBuffToCaster(TimelineObj timelineObj, params object[] args){
            if (timelineObj.caster && args.Length > 0){
                AddBuffInfo abi = (AddBuffInfo)args[0];
                abi.caster = timelineObj.caster;
                abi.target = timelineObj.caster;
                ChaState cs = timelineObj.caster.GetComponent<ChaState>();
                if (cs){
                    cs.AddBuff(abi);
                }
            }
        }

    }
}