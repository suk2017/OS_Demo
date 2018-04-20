using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ListManager : MonoBehaviour
{
    public GameObject page_Prefag;
    public int[] Pages;//初始序列
    public Transform[] position;
    public Texture2D[] num;//数字材质
    public Material mat;//材质
    public int count;//页框个数 用于初始化

    public float length;
    [Range(0, 2)] public int RS;


    private bool isOPT = false;//是否是最佳算法
    private GameObject[] lastObject;//变动页框移除的页
    private GameObject[] currentObject;//变动页框移入的页


    public static bool avaliable = false;//是否可以进行下一步
    public static List<int> pages;//要使用的页面序列 使用链表为了动态增加
    public static List<int> steps;//操作步骤 记录
    public static int step;//当前步
    public static int[] memory;//已调入内存的序列
    public static int used;//内存已使用的位置（有未使用的页框则一定按顺序载入）
    public static RS_Base currentRS;//当前算法
    public static GameObject[] memoryPageModel;//内存中的页模型
    public static bool RS_IN;


    /// <summary>
    /// 处理页使用的主要方法 为了异步接收用户输入要使用协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator PageStream()
    {
        while (used < count)
        {
            yield return new WaitUntil(() => avaliable && step < pages.Count - 1);//等到用户按下按钮或者自动下一步

            if (isOPT)
            {
                NextPage_OPT();
            }
            else
            {
                if (!NextPage())//如果未存在于内存中
                {
                    GetPos();
                    StartCoroutine(MoveModel());//载入内存中
                }
                KeepRS();//维护算法状态

                ChangeColor();//更新模型
            }

            avaliable = false;
            yield return new WaitForSeconds(1.0f);//至少等待一秒再继续下一步
        }
        print("调度结束");
    }

    private void NextPage_OPT()
    {
        ++step;
        if (step <= 0 || steps[step] != steps[step - 1])
        {
            StartCoroutine(MoveModel());//载入内存中
        }
        ChangeColor();//更新模型
    }

    /// <summary>
    /// 使用下一页 若已存在内存则返回true 否则返回false
    /// </summary>
    private bool NextPage()
    {
        ++step;
        for (int i = 0; i < used; ++i)
        {
            if (pages[step] == memory[i])
            {
                steps.Add(i);
                RS_IN = false;
                return true;
            }
        }

        RS_IN = true;
        return false;
    }

    /// <summary>
    /// 根据当前内存剩余空闲页框计算是否需要使用算法 并调用算法 换出换入页
    /// </summary>
    /// <returns></returns>
    private bool GetPos()
    {
        //确定位置
        if (used < memory.Length)
        {

            int pos = used;
            ++used;

            steps.Add(pos);
            memory[pos] = pages[step];
            return false;
        }
        else
        {

            int pos = currentRS.GetPos();

            steps.Add(pos);
            memory[pos] = pages[step];
            return true;
        }
    }


    /// <summary>
    /// 移动页模型
    /// </summary>
    private IEnumerator MoveModel()
    {
        int pos = steps[step];
        //移动模型
        GameObject newPage = Instantiate(page_Prefag, position[pos].position + new Vector3(0, length, 0), Quaternion.Euler(new Vector3(0, 90, 0)));
        newPage.GetComponent<MeshRenderer>().material.mainTexture = num[pages[step]];
        yield return null;
        newPage.transform.DOMoveY(newPage.transform.position.y - length, 1.0f);
        if (memoryPageModel[pos])
        {
            memoryPageModel[pos].transform.DOMoveY(memoryPageModel[pos].transform.position.y - length, 1.0f);
            StartCoroutine(AutoDestroy(memoryPageModel[pos], 1.0f));
        }
        memoryPageModel[pos] = newPage;

    }

    /// <summary>
    /// 改变颜色 以示使用
    /// </summary>
    private void ChangeColor()
    {
        Sequence se = DOTween.Sequence();
        se.AppendInterval(0.3f);
        se.Append(position[steps[step]].GetComponent<MeshRenderer>().material.DOColor(Color.red, 0.3f));
        se.Append(position[steps[step]].GetComponent<MeshRenderer>().material.DOColor(Color.white, 0.3f));
    }

    private void KeepRS()
    {
        currentRS.Use();
        if (RS_IN) { currentRS.IN(); };
    }

    private void Initial()
    {
        memory = new int[count];
        memoryPageModel = new GameObject[count];
        used = 0;
        pages = new List<int>();
        pages.AddRange(Pages);
        step = -1;
        memory = new[] { -1, -1, -1 };
        //currentObject = new GameObject[count];
        //lastObject = new GameObject[count];


        switch (RS)
        {
            case 0: currentRS = new RS_OPT(); break;
            case 1: currentRS = new RS_FIFO(count); break;
            case 2: currentRS = new RS_LRU(count); break;
        }
    }
    ///////////////////////////////////

    private void Start()
    {
        Initial();
        steps = new List<int>();

        if (currentRS is RS_OPT)
        {
            isOPT = true;
            pages.Reverse();
            currentRS = new RS_LRU(count);
            for (int i = 0; i < pages.Count; ++i)
            {

                if (!NextPage())//如果未存在于内存中
                {
                    GetPos();
                }
                KeepRS();//维护算法状态
            }
            steps.Reverse();
            Initial();
        }
        else
        {
            isOPT = false;
        }

        print("当前载入：" + currentRS.RS_Name);

        StartCoroutine(PageStream());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            avaliable = true;
        }
    }

    private IEnumerator AutoDestroy(GameObject target, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(target);
    }






}
