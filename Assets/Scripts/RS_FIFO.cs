﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RS_FIFO : RS_Base
{

    private List<int> order;

    public RS_FIFO(int count) : base(count)
    {
        order = new List<int>();
        RS_Name = "FIFO算法";
    }



    ////////////////////////////////////////////////////

    /// <summary>
    /// 根据当前状态返回下一个换入页的位置
    /// </summary>
    /// <returns></returns>
    public override int GetPos()
    {
        int pos = order[0];
        order.RemoveAt(0);
        return pos;
    }

    /// <summary>
    /// 使用时调用一次
    /// </summary>
    public override void Use()
    {

    }

    /// <summary>
    /// 修改时调用一次
    /// </summary>
    public override void Edit()
    {

    }

    /// <summary>
    /// 进入时调用一次
    /// </summary>
    public override void IN()
    {
        //if (order.Count >= ListManager.memory.Length)
        //{
        //    order.RemoveAt(0);
        //}
        order.Add(ListManager.steps[ListManager.step]);

        string tempOrderString = "";
        for (int i = 0; i < order.Count; ++i)
        {
            tempOrderString += order[i] + " ";
        }
        Debug.Log(tempOrderString);
    }

}
