using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ListManager : MonoBehaviour
{
    public GameObject page_Prefag;
    //public int[] Pages;//初始序列
    //public Texture2D[] num;//数字材质
    public Material mat;//材质
    public float length;
    public GameObject setting;
    public InputField PagesList;
    public InputField PagePosCount;
    public Dropdown Dropdown_RS;
    public Text serial;
    public Text newNum;
    public Text info;


    [Range(0, 2)] private int RS;
    private int count;//页框个数 用于初始化
    private bool isOPT = false;//是否是最佳算法
    private GameObject[] lastObject;//变动页框移除的页
    private GameObject[] currentObject;//变动页框移入的页
    private bool waiting = false;
    private EventSystem eventSystem;
    private string nextNum;
    private string defaultSerial = "6 0 1 2 0 3 0 4 2 3 0 3 2 1 2 0 1 6 0 1";//默认序列
    private string defaultCount = "3";//默认页框数
    private Transform[] position;//页框位置


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
        serial.text = GetSteps();
        info.text = currentRS.ShowInfo();
        while (!(currentRS is RS_OPT) || step <= pages.Count - 1)
        {
            yield return new WaitUntil(() => avaliable && pages.Count > step + 1);//等到用户按下按钮或者自动下一步

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

            serial.text = GetSteps();

            avaliable = false;
            yield return new WaitForSeconds(1.0f);//至少等待一秒再继续下一步
            info.text = currentRS.ShowInfo();
        }
        print("调度结束");
    }

    private void NextPage_OPT()
    {
        ++step;
        if (step <= 0 || pages[step] != memory[steps[step]])
        {
            StartCoroutine(MoveModel());//载入内存中
        }
        ChangeColor();//更新模型
        memory[steps[step]] = pages[step];
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
        //newPage.GetComponent<MeshRenderer>().material.mainTexture = num[pages[step]];
        newPage.transform.GetChild(0).GetComponent<TextMesh>().text = pages[step] + "";
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

    /// <summary>
    /// 从字符串中获取列表
    /// </summary>
    private List<int> StringToList(string value)
    {
        List<int> result = new List<int>();
        value += " ";
        string num = "";
        for (int i = 0; i < value.Length; ++i)
        {
            if (value[i] < '0' || value[i] > '9')
            {
                int result_Int;
                if (int.TryParse(num, out result_Int))
                {
                    result.Add(result_Int);
                    num = "";
                }
            }
            else
            {
                num += value[i];
            }
        }
        return result;
    }

    /// <summary>
    /// 恢复默认
    /// </summary>
    public void _Reset()
    {
        PagesList.text = defaultSerial;
        PagePosCount.text = defaultCount;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Initial()
    {
        pages = new List<int>();

        string list = "";
        int firstTime = PlayerPrefs.GetInt("FirstTime", 1);
        if (firstTime == 1)
        {
            list = "6 0 1 2 0 3 0 4 2 3 0 3 2 1 2 0 1 6 0 1";
            pages = StringToList(list);
            count = 3;
            RS = 0;

            PlayerPrefs.SetString("pagesList", list);
            PlayerPrefs.SetInt("pagePosCount", 3);
            PlayerPrefs.SetInt("dropDown_RS", 0);


            PlayerPrefs.SetInt("FirstTime", 0);
        }
        else
        {
            list = PlayerPrefs.GetString("pagesList");
            pages = StringToList(list);

            count = PlayerPrefs.GetInt("pagePosCount");
            RS = PlayerPrefs.GetInt("dropDown_RS");

            if (count < 1) { count = 1; }

        }

        memory = new int[count];
        for (int i = 0; i < memory.Length; ++i) { memory[i] = -1; }
        memoryPageModel = new GameObject[count];
        used = 0;
        step = -1;

        PagesList.text = list;
        PagePosCount.text = count + "";
        Dropdown_RS.value = RS;
        switch (RS)
        {
            case 0: currentRS = new RS_OPT(); break;
            case 1: currentRS = new RS_FIFO(count); break;
            case 2: currentRS = new RS_LRU(count); break;
        }
    }

    /// <summary>
    /// 打开关闭设置面板
    /// </summary>
    public void _Setting()
    {
        if (setting.activeInHierarchy)
        {
            setting.SetActive(false);
        }
        else
        {
            setting.SetActive(true);
        }
    }

    /// <summary>
    /// 重启
    /// </summary>
    public void _Restart()
    {
        string pageList = PagesList.text;
        string pagePosCount = PagePosCount.text;
        int dropdown_RS = Dropdown_RS.value;
        List<int> pageList_Int = new List<int>();

        PlayerPrefs.SetString("pagesList", pageList);
        PlayerPrefs.SetInt("pagePosCount", int.Parse(pagePosCount));
        PlayerPrefs.SetInt("dropDown_RS", dropdown_RS);


        SceneManager.LoadScene("RS");
    }

    /// <summary>
    /// steps转化为string 倒序
    /// </summary>
    /// <returns></returns>
    public string GetSteps()
    {
        string result = "";
        if (pages == null || step == pages.Count - 1)
        {
            if (currentRS is RS_OPT)
            {
                result = "调度结束（使用OPT算法 因此无法动态增加）";
            }
            else
            {
                result = "序列为空";
            }
        }
        else
        {
            for (int i = pages.Count - 1; i > step + 1; --i)
            {
                result += pages[i] + " ";
            }
            result += "<b>" + pages[step + 1] + "</b>";
        }

        return result;
    }
    ///////////////////////////////////

    public GameObject PageFrame_Prefab;
    public Transform rootOfPageFrame;
    private void Start()
    {
        Initial();//可重复初始化的地方

        steps = new List<int>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        position = new Transform[count];
        position[0] = Instantiate(PageFrame_Prefab, rootOfPageFrame.position + new Vector3(2, 0, 0), Quaternion.identity, rootOfPageFrame.transform).transform;
        position[0].GetChild(0).GetComponent<TextMesh>().text = "0";
        for (int i = 1; i < count; ++i)
        {
            position[i] = Instantiate(PageFrame_Prefab, position[i - 1].position + new Vector3(2, 0, 0), Quaternion.identity, rootOfPageFrame.transform).transform;
            position[i].GetChild(0).GetComponent<TextMesh>().text = i + "";
        }
        rootOfPageFrame.position -= new Vector3((count - 1) , 0, 0);

        if (currentRS is RS_OPT)
        {
            isOPT = true;
            RS_OPT opt = currentRS as RS_OPT;
            steps = opt.PreCalculate();
            Initial();
            //pages.Reverse();
            //currentRS = new RS_LRU(count);
            //for (int i = 0; i < pages.Count; ++i)
            //{

            //    if (!NextPage())//如果未存在于内存中
            //    {
            //        GetPos();
            //    }
            //    KeepRS();//维护算法状态
            //}
            //steps.Reverse();
            //Initial();
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
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            avaliable = true;
        }

        if (!isOPT)
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                nextNum += 0;
            }
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                nextNum += 1;
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                nextNum += 2;
            }
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                nextNum += 3;
            }
            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                nextNum += 4;
            }
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                nextNum += 5;
            }
            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                nextNum += 6;
            }
            if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                nextNum += 7;
            }
            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                nextNum += 8;
            }
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                nextNum += 9;
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (nextNum.Length > 0)
                {
                    nextNum = nextNum.Substring(0, nextNum.Length - 1);
                }
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                int num = 0;
                if (int.TryParse(nextNum, out num))
                {
                    pages.Add(num);
                    nextNum = "";
                    serial.text = GetSteps();
                }
            }
        }


        newNum.text = nextNum;
    }

    private IEnumerator AutoDestroy(GameObject target, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(target);
    }






}
