using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///管理游戏中所有的timeline
///</summary>
public class TimelineManager : MonoBehaviour{
    private List<TimelineObj> timelines = new List<TimelineObj>();

    private void FixedUpdate() {
        if (this.timelines.Count <= 0) return;

        int idx = 0;
        while (idx < this.timelines.Count){
            float wasTimeElapsed = timelines[idx].timeElapsed;
            timelines[idx].timeElapsed += Time.fixedDeltaTime * timelines[idx].timeScale;

            //判断有没有返回点
            if (
                timelines[idx].model.chargeGoBack.atDuration < timelines[idx].timeElapsed &&
                timelines[idx].model.chargeGoBack.atDuration >= wasTimeElapsed
            ){
                if (timelines[idx].caster){
                    ChaState cs = timelines[idx].caster.GetComponent<ChaState>();
                    if (cs.charging == true){
                        timelines[idx].timeElapsed = timelines[idx].model.chargeGoBack.gotoDuration;
                        continue;
                    }
                }
            }
            //执行时间点内的事情
            for (int i = 0; i < timelines[idx].model.nodes.Length; i++){
                if (
                    timelines[idx].model.nodes[i].timeElapsed < timelines[idx].timeElapsed &&
                    timelines[idx].model.nodes[i].timeElapsed >= wasTimeElapsed
                ){
                    timelines[idx].model.nodes[i].doEvent(
                        timelines[idx], 
                        timelines[idx].model.nodes[i].eveParams
                    );
                }
            }

            //判断timeline是否终结
            if (timelines[idx].model.duration <= timelines[idx].timeElapsed){
                timelines.RemoveAt(idx);
            }else{
                idx++;
            }
        }
    }

    ///<summary>
    ///添加一个timeline
    ///<param name="timelineModel">要添加的timeline的model</param>
    ///<param name="caster">timeline的负责人</param>
    ///<param name="source">添加的源数据，比如技能就是skillObj</param>
    ///</summary>
    public void AddTimeline(TimelineModel timelineModel, GameObject caster, object source){
        if (CasterHasTimeline(caster) == true) return;
        this.timelines.Add(new TimelineObj(timelineModel, caster, source));
    }

    ///<summary>
    ///添加一个timeline
    ///<param name="timelineModel">要添加的timeline</param>
    ///</summary>
    public void AddTimeline(TimelineObj timeline){
        if (timeline.caster != null && CasterHasTimeline(timeline.caster) == true) return;
        this.timelines.Add(timeline);
    }

    public bool CasterHasTimeline(GameObject caster){
        for (var i = 0; i < timelines.Count; i++){
            if (timelines[i].caster == caster) return true;
        }
        return false;
    }
}