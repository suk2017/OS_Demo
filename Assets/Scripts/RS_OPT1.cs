using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RS_OPT1 : RS_Base
{
    public RS_Manager rs;

    private List<int> pageList;
    private List<int> steps;
    private int[] Memory;
    private int used;
    private int step;

    public RS_OPT1(RS_Manager lm)
    {
        rs = lm;
        RS_Name = "OPT算法";
    }

    /// <summary>
    /// 预计算序列
    /// </summary>
    public List<int> PreCalculate()
    {
        pageList = ListManager.pages;
        steps = new List<int>();
        Memory = new int[rs.memory.Length];
        for (int i = 0; i < Memory.Length; ++i)
        {
            Memory[i] = -1;
        }
        used = 0;
        step = 0;
        for (int i = 0; i < pageList.Count; ++i)
        {
            step = i;
            int pos;
            if (Exist(Memory, pageList[i], out pos))
            {
                steps.Add(pos);
            }
            else
            {
                if (used < Memory.Length)
                {
                    Memory[used] = pageList[i];
                    steps.Add(used);
                    ++used;
                }
                else
                {
                    pos = Pos();
                    Memory[pos] = pageList[i];
                    steps.Add(pos);
                }
            }
        }
        return steps;
    }

    /// <summary>
    /// 检测是否已经存在于整型数组
    /// </summary>
    /// <param name="list"></param>
    /// <param name="page"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool Exist(int[] list, int page, out int pos)
    {
        //长度应该使用used 但是这样也可以 因为初始时为-1
        for (int i = 0; i < list.Length; ++i)
        {
            if (page == list[i])
            {
                pos = i;
                return true;
            }
        }
        pos = -1;
        return false;
    }

    /// <summary>
    /// 向之后的序列寻找最后出现的元素在内存中的位置
    /// </summary>
    /// <returns></returns>
    private int Pos()
    {
        int[] tempMemory = new int[Memory.Length];
        for (int i = 0; i < Memory.Length; ++i)
        {
            tempMemory[i] = Memory[i];
        }
        int left = tempMemory.Length;
        int defaultPos = 0;
        for (int i = step; i < pageList.Count; ++i)
        {
            for (int j = 0; j < tempMemory.Length; ++j)
            {
                if (tempMemory[j] == pageList[i])
                {
                    if (left == 1)
                    {
                        return j;
                    }
                    else
                    {
                        tempMemory[j] = -1;
                        --left;
                    }
                }
                else
                {
                    if (tempMemory[j] != -1)
                    {
                        defaultPos = j;
                    }
                }

            }
        }
        return defaultPos;
    }

    /// <summary>
    /// 显示运算细节
    /// </summary>
    public override string ShowInfo()
    {
        //if (rs.steps == null || rs.steps.Count == 0 || rs.step < 0 || rs.step > rs.steps.Count - 2)
        
        if (rs.steps == null || rs.steps.Count == 0 || rs.step + 1 >= RS_Manager.pages.Count)
        {
            return "根据未调度的序列，未来使用频率最低的是:\n\n替换时使用第一个数字代表的位置";
        }
        else
        {
            return "根据未调度的序列，未来使用频率最低的是:\n" + rs.steps[rs.step] + "\n替换时使用第一个数字代表的位置";
        }
    }

    /// <summary>
    /// 进入时调用一次
    /// </summary>
    public override void IN()
    {

        ++PageLostCount;
    }
}
