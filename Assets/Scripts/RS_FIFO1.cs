using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RS_FIFO1 : RS_Base
{
    public RS_Manager rs;

    private List<int> order;

    public RS_FIFO1(int count, RS_Manager lm)
    {
        rs = lm;
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
        order.Add(rs.steps[rs.step]);

        string tempOrderString = "";
        for (int i = 0; i < order.Count; ++i)
        {
            tempOrderString += order[i] + " ";
        }

        ++PageLostCount;
    }

    /// <summary>
    /// 显示运算细节
    /// </summary>
    public override string ShowInfo()
    {
        string result = "";
        for (int i = 0; i < order.Count; ++i)
        {
            result += order[i] + " ";
        }
        return "根据已调度的序列，按进入顺序先后排序:\n" + result + "\n替换时使用第一个数字代表的位置";
    }
}
