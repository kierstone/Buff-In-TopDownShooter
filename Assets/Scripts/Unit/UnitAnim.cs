using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///动画播放的管理器，是典型的ARPG之类即时回合制使用的，用来管理当前应该播放那个动画
///不仅仅是角色，包括aoe、bullet等，只要需要管理播放什么动画（带有animator的gameobject）就应该用这个
///</summary>
public class UnitAnim : MonoBehaviour{
    private Animator animator;

    ///<summary>
    ///播放的倍速，作用于每个信息的duration减少速度
    ///</summary>
    public float timeScale = 1;

    ///<summary>
    ///动画的逻辑信息
    ///key其实就是要播放的动画的key，比如“attack”等。
    ///value则是一个animInfo，取其RandomKey()的值就可以得到要播放的动画在animator中的名称（play()的参数）
    ///</summary>
    public Dictionary<string, AnimInfo> animInfo;

    //当前正在播放的动画的权重，只有权重>=这个值才会切换动画
    private AnimInfo playingAnim = null;

    //当前权重持续时间（单位秒），归0后，currentPriority归0
    private float priorityDuration = 0;

    private int currentAnimPriority{
        get{
            return playingAnim == null ? 0 : 
                (priorityDuration <= 0 ? 0 : playingAnim.priority);
        }
    }

    ///<summary>
    ///正在播放的动画
    ///</summary>

    void Start() {
        animator = this.gameObject.GetComponent<Animator>();
    }

    void FixedUpdate() {
        if (!animator) animator = this.gameObject.GetComponentInChildren<Animator>();   //尝试从子级GameObject找到一个Animator

        if (!animator || animInfo == null || animInfo.Count <= 0) return;   

        if (priorityDuration > 0) priorityDuration -= Time.fixedDeltaTime * timeScale;
    }

    ///<summary>
    ///申请播放某个动画，不是你申请就鸟你了，要看有什么正在播放的
    ///<param name="animName">动画的名称，对应animInfo的key</param>
    ///</summary>
    public void Play(string animName){
        if (animInfo.ContainsKey(animName) == false || animator == null) return;
        if (playingAnim != null && playingAnim.key == animName) return;  //已经在播放了
        AnimInfo toPlay = animInfo[animName];
        if (currentAnimPriority > toPlay.priority) return;   //优先级不够不放
        SingleAnimInfo playOne = toPlay.RandomKey();
        animator.Play(playOne.animName);
        playingAnim = toPlay;
        priorityDuration = playOne.duration;
    }

    ///<summary>
    ///设置Animator为对象
    ///<param name="animator">要被这个unitAnim所管理的animator</param>
    ///</summary>
    public void SetAnimator(Animator animator){
        this.animator = animator;
    }
}