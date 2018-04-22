using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RS_Base
{
    public string RS_Name = "算法类基类";
    
    public int count = 0;//当前已使用的数量 前期小于Pos.Length 后期等于Pos.Length
    public int latest;

    public RS_Base()
    {

    }

    public RS_Base(int MaxLength)
    {

    }

    ///// <summary>
    ///// 调用一页
    ///// </summary>
    ///// <param name="page"></param>
    ///// <returns></returns>
    //public bool Use(int page)
    //{
    //    if (!Detect(page))
    //    {
    //        Load(page);

    //    }
    //    return true;
    //}


    ///// <summary>
    ///// 获取取来的这页所存的页框序号
    ///// </summary>
    ///// <returns></returns>
    //public int GetLatest()
    //{
    //    return latest;
    //}

    ///// <summary>
    ///// 检测当前页是否在内存中
    ///// </summary>
    ///// <param name="page"></param>
    ///// <returns></returns>
    //public virtual bool Detect(int page)
    //{
    //    int[] Pos = ListManager.memory;
    //    for (int i = 0; i < Pos.Length; ++i)
    //    {
    //        if (Pos[i] == page)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    ///// <summary>
    ///// 默认只存在第一个
    ///// </summary>
    ///// <param name="page"></param>
    ///// <returns></returns>
    //public virtual void Load(int page)
    //{
    //    int[] Pos = ListManager.memory;
    //    if (count < Pos.Length)
    //    {
    //        Pos[count] = page;
    //        latest = count;
    //        ++count;
    //    }
    //    else
    //    {
    //        Pos[0] = page;
    //        latest = 0;
    //    }
    //}


    ////////////////////////////////////////////////////

    /// <summary>
    /// 根据当前状态返回下一个换入页的位置
    /// </summary>
    /// <returns></returns>
    public virtual int GetPos()
    {
        return 0;
    }

    /// <summary>
    /// 使用时调用一次
    /// </summary>
    public virtual void Use()
    {

    }

    /// <summary>
    /// 修改时调用一次
    /// </summary>
    public virtual void Edit()
    {

    }

    /// <summary>
    /// 进入时调用一次
    /// </summary>
    public virtual void IN()
    {

    }

    /// <summary>
    /// 用于显示内部运算的细节
    /// </summary>
    public virtual string ShowInfo()
    {
        return "";
    }
}
