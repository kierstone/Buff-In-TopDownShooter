using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///<summary>
///虽然名字看起来很牛逼，其实也就只拿了角色的属性的hp而已
///</summary>
public class PlayerStateListener : MonoBehaviour{
    [Tooltip("玩家的角色（核心的那个）的GameObject")]
    public GameObject playerGameObject;

    Text text;
    private ChaState playerState;

    private void Start() {
        text = this.gameObject.GetComponent<Text>();
    }
    
    //因为UI必然是渲染的，所以走Update
    private void Update() {
        if (playerGameObject == null || text == null) return;
        if (playerState == null) playerState = playerGameObject.GetComponent<ChaState>();
        if (playerState == null) return;    //还是没拿到就再见了
        int curHp = playerState.resource.hp;
        int maxHp = playerState.property.hp;
        string c = (curHp * 1.000f / (maxHp * 1.000f)) > 0.300f ? "green" : "red";
        text.text = "<color=" + c + ">" + curHp.ToString() + " / " + maxHp.ToString() + "</color>";
    }
}