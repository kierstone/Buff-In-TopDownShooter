using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils{

    public static bool CircleHitRects(Vector2 circlePivot, float circleRadius, List<Rect> rects){
        if (rects.Count <= 0) return false;
        for (var i = 0; i < rects.Count; i++){
            if (Utils.CircleHitRect(circlePivot, circleRadius, rects[i]) == true){
                return true;
            }
        }
        return false;
    }
    public static bool CircleHitRects(Vector2 circlePivot, float circleRadius, Rect[] rects){
        List<Rect> rl = new List<Rect>();
        for (var i = 0; i < rects.Length; i++){
            rl.Add(rects[i]);
        }
        return CircleHitRects(circlePivot, circleRadius, rl);
    }

    public static bool CircleHitRect(Vector2 circlePivot, float circleRadius, Rect rect){
        Rect r1 = new Rect(rect.x - circleRadius, rect.y, rect.width + circleRadius * 2.00f, rect.height);
        Rect r2 = new Rect(rect.x, rect.y - circleRadius, rect.width, rect.height + circleRadius * 2.00f);
        float px = circlePivot.x;
        float py = circlePivot.y;
        float allowMiss = circleRadius * 0.05f;
        if (
            (r1.x + allowMiss <= px && r1.x + r1.width - allowMiss >= px && r1.y + allowMiss <= py && r1.y + r1.height - allowMiss >= py) ||
            (r2.x + allowMiss <= px && r2.x + r2.width - allowMiss >= px && r2.y + allowMiss <= py && r2.y + r2.height - allowMiss >= py)
        ){
            return true;
        }

        float rd2 = circleRadius * circleRadius;
        float[] rx = new float[]{
            rect.x + allowMiss, 
            rect.x + allowMiss, 
            rect.x + rect.width - allowMiss, 
            rect.x + rect.width -allowMiss
        };
        float[] ry = new float[]{
            rect.y + allowMiss, 
            rect.y + rect.height - allowMiss, 
            rect.y + allowMiss, 
            rect.y + rect.height - allowMiss
        };
        for (var i = 0; i < rx.Length; i++){
            if (Mathf.Pow(rx[i] - px, 2) + Mathf.Pow(ry[i] - py, 2) <= rd2) return true;
        }

        return false;
    }

    public static bool InRange(float x1, float y1, float x2, float y2, float range){
        return Mathf.Pow(x1 - x2, 2) + Mathf.Pow(y1 - y2, 2) <= Mathf.Pow(range,  2);
    }


    ///<summary>
    ///根据面向和移动方向得到一个资源名预订了规则的后缀名
    ///<param name="faceDegree">面向角度</param>
    ///<param name="moveDegree">移动角度</param>
    ///<return>约定好的关键字，比如"Forward","Back","Left","Right"，对应到角色动画的key</return>
    ///</summary>
    public static string GetTailStringByDegree(float faceDegree, float moveDegree){
        float fd = faceDegree;
        float md = moveDegree;
        while (fd < 180) fd += 360;
        while (md < 180) md += 360;
        fd = fd % 360;
        md = md % 360;
        float dd = md - fd;
        if (dd > 180){
            dd -= 360;
        }else if (dd < -180){
            dd += 360;
        }
        //Debug.Log("degree:"+fd + " / " + md + " / " + dd);
        if (dd >= -45 && dd <= 45){
            return "Forward";
        }else
        if (dd < -45 && dd >= -135){
            return "Left";
        }else
        if (dd > 45 && dd <= 135){
            return "Right";
        }else{
            return "Back";
        }
    }

}