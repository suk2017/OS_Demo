using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RS_LRU1 : RS_Base
{
    public RS_Manager rs;

    private List<int> freq;

    public RS_LRU1(int count, RS_Manager lm)
    {
        rs = lm;
        freq = new List<int>();
        RS_Name = "LRU算法";
    }


    /// <summary>
    /// 根据当前状态返回下一个换入页的位置
    /// </summary>
    /// <returns></returns>
    public override int GetPos()
    {
        int pos = freq[0];
        freq.RemoveAt(0);
        return pos;
    }

    /// <summary>
    /// 使用时调用一次
    /// </summary>
    public override void Use()
    {
        if (rs.step < 0)
        {
            return;
        }
        int value = rs.steps[rs.step];
        if (freq.Count == 0 || freq[freq.Count - 1] != value)
        {
            freq.Add(value);
        }
        for (int i = 0; i < freq.Count - 1; ++i)
        {
            if (freq[i] == value)
            {
                freq.RemoveAt(i);
            }
        }

        string tempOrderString = "";
        for (int i = 0; i < freq.Count; ++i)
        {
            tempOrderString += freq[i] + " ";
        }
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

        ++PageLostCount;
    }

    /// <summary>
    /// 显示运算细节
    /// </summary>
    public override string ShowInfo()
    {
        string result = "";
        for (int i = 0; i < freq.Count; ++i)
        {
            result += freq[i] + " ";
        }
        return "根据已调度的序列，按使用顺序先后排序:\n" + result + "\n替换时使用第一个数字代表的位置";
    }

    //public override bool Detect(int page)
    //{
    //    int[] Pos = ListManager.memory;
    //    for (int i = 0; i < Pos.Length; ++i)
    //    {
    //        if(Pos[i] == page)
    //        {
    //            ++freq[i];
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //public override void Load(int page)
    //{
    //    int[] Pos = ListManager.memory;
    //    if (count < Pos.Length)
    //    {
    //        Pos[count] = page;
    //        latest = count;
    //        freq[count] = 0;
    //        ++count;
    //    }
    //    else
    //    {
    //        int index = FindMin(freq);
    //        Pos[index] = page;
    //        freq[index] = 0;
    //    }
    //}

    //private int FindMin(int[] list)
    //{
    //    if (list == null || list.Length < 1)
    //    {
    //        return -1;
    //    }
    //    if (list.Length == 1)
    //    {
    //        return 0;
    //    }
    //    int index = 0;
    //    int min = list[0];
    //    for (int i = 0; i < list.Length; ++i)
    //    {
    //        if (min > list[i])
    //        {
    //            min = list[i];
    //            index = i;
    //        }
    //    }
    //    return index;
    //}
}
