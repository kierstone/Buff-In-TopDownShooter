using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///挂上这个的特效就会小球弹跳了
///</summary>
public class BouncingBallY : MonoBehaviour{
    private float timeElapsed = 0;
    
    [Tooltip("弹跳的最高点，单位：米")]
    public float highestPoint = 4.0f;

    [Tooltip("落点反弹的时间点，因为落地和时间有关，单位：秒")]
    public float[] hitGroundAt = new float[0];

    private int partIndex = 0;

    private void Update() {
        if (this.hitGroundAt.Length <= 0) return;

        float timePassed = Time.deltaTime;

        while (partIndex < hitGroundAt.Length && timeElapsed >= hitGroundAt[partIndex] ){
            partIndex += 1;
        }
        
        if (partIndex >= hitGroundAt.Length){
            this.transform.position = new Vector3(
                this.transform.position.x,
                0,
                this.transform.position.z
            );
            this.hitGroundAt = new float[0];
            return;
        }

        float partTime = Mathf.Max(0.001f, hitGroundAt[partIndex] - (partIndex <= 0 ? 0 : hitGroundAt[partIndex - 1]));
        float cpTime = timeElapsed - (partIndex <= 0 ? 0 : hitGroundAt[partIndex - 1]);
        float tPerc = Mathf.Min(cpTime / partTime, 1.000f);
        bool upper = tPerc < 0.5f;
        float cHighest = highestPoint / Mathf.Pow(2, partIndex);
        float cY = Mathf.Sin(tPerc * Mathf.PI) * cHighest;
        this.transform.position = new Vector3(
            this.transform.position.x,
            upper ? Mathf.Max(this.transform.position.y, cY) : Mathf.Min(this.transform.position.y, cY),
            this.transform.position.z
        );

        this.timeElapsed += timePassed;
    }

    public void ResetTo(float highest, float[] hitGroundTime){
        this.hitGroundAt = hitGroundTime;
        this.highestPoint = highest;
        this.partIndex = 0;
        this.timeElapsed = 0;
    }
}