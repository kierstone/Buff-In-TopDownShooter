using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///角色的可操作状态，这个是根据游戏玩法来细节设计的，目前就用这个demo需要的
///</summary>
public struct ChaControlState{
    ///<summary>
    ///是否可以移动坐标
    ///</summary>
    public bool canMove;

    ///<summary>
    ///是否可以转身
    ///</summary>
    public bool canRotate;

    ///<summary>
    ///是否可以使用技能，这里的是“使用技能”特指整个技能流程是否可以开启
    ///如果是类似中了沉默，则应该走buff的onCast，尤其是类似wow里面沉默了不能施法但是还能放致死打击（部分技能被分类为法术，会被沉默，而不是法术的不会）
    ///</summary>
    public bool canUseSkill;

    public ChaControlState(bool canMove = true, bool canRotate = true, bool canUseSkill = true){
        this.canMove = canMove;
        this.canRotate = canRotate;
        this.canUseSkill = canUseSkill;
    }

    public void Origin(){
        this.canMove = true;
        this.canRotate = true;
        this.canUseSkill = true;
    }

    public static ChaControlState origin = new ChaControlState(true, true, true);

    ///<summary>
    ///昏迷效果
    ///</summary>
    public static ChaControlState stun = new ChaControlState(false, false, false);

    public static ChaControlState operator +(ChaControlState cs1, ChaControlState cs2){
        return new ChaControlState(
            cs1.canMove & cs2.canMove,
            cs1.canRotate & cs2.canRotate,
            cs1.canUseSkill & cs2.canUseSkill
        );
    }
}