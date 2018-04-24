using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RS_Manager : MonoBehaviour
{

    public GameObject page_Prefag;
    public GameObject PageFrame_Prefab;
    public Transform rootOfPageFrame;
    public Material mat;//材质
    public float length;

    public static string Pages
    {
        get
        {
            string result = "";
            for (int i = pages.Count - 1; i > Step + 1; --i)
            {
                result += pages[i] + " ";
            }
            if (Step + 1 < pages.Count)
            {
                result += "<b>" + pages[Step + 1] + "</b>";
            }
            return result;
        }
    }//初始序列
    public static int Step;
    //public Texture2D[] num;//数字材质


    [Range(0, 2)] public int RS;
    private int count;//页框个数 用于初始化
    private bool isOPT = false;//是否是最佳算法
    private GameObject[] lastObject;//变动页框移除的页
    private GameObject[] currentObject;//变动页框移入的页
    private bool waiting = false;
    private string nextNum = "";
    private string defaultSerial = "6 0 1 2 0 3 0 4 2 3 0 3 2 1 2 0 1 6 0 1";//默认序列
    private string defaultCount = "3";//默认页框数
    private Transform[] position;//页框位置


    [HideInInspector()] public bool avaliable = false;//是否可以进行下一步
    [HideInInspector()] public static List<int> pages;//要使用的页面序列 使用链表为了动态增加
    [HideInInspector()] public List<int> steps;//操作步骤 记录
    [HideInInspector()] public int step;//当前步
    [HideInInspector()] public int[] memory;//已调入内存的序列
    [HideInInspector()] public int used;//内存已使用的位置（有未使用的页框则一定按顺序载入）
    [HideInInspector()] public RS_Base currentRS;//当前算法
    [HideInInspector()] public GameObject[] memoryPageModel;//内存中的页模型
    [HideInInspector()] public bool RS_IN;

    //public GameObject helpPanel;
    //public GameObject settingButton;
    //public GameObject helpButton;
    //public GameObject setting;
    //private EventSystem eventSystem;
    //public InputField PagesList;
    //public InputField PagePosCount;
    //public Dropdown Dropdown_RS;
    //public Text serial;
    //public Text newNum;
    public Text info;


    /// <summary>
    /// 改变状态
    /// </summary>
    public void _Avaliable()
    {
        avaliable = true;
    }

    /// <summary>
    /// 获取故障缺页次数
    /// </summary>
    /// <returns></returns>
    public int _GetPageLostCount()
    {
        return currentRS.GetPageLostCount();
    }

    /// <summary>
    /// 处理页使用的主要方法 为了异步接收用户输入要使用协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator PageStream()
    {
        //serial.text = GetSteps();
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

            //serial.text = GetSteps();

            avaliable = false;
            yield return new WaitForSeconds(1.0f);//至少等待一秒再继续下一步
            info.text = currentRS.ShowInfo();
        }
        print("调度结束");
    }

    /// <summary>
    /// 使用OPT算法的逐步计算
    /// </summary>
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

    /// <summary>
    /// 维持算法状态
    /// </summary>
    private void KeepRS()
    {
        currentRS.Use();
        if (RS_IN) { currentRS.IN(); };
    }


    /// <summary>
    /// 从字符串中获取列表
    /// </summary>
    private static List<int> StringToList(string value)
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
    /// 将列表转化为字符串
    /// </summary>
    private static string ListToString(List<int> value)
    {
        string result = "";
        for (int i = 0; i < value.Count; ++i)
        {
            result += value[i] + " ";
        }
        return result;
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
            //RS = PlayerPrefs.GetInt("dropDown_RS");

            if (count < 1) { count = 1; }

        }

        memory = new int[count];
        for (int i = 0; i < memory.Length; ++i) { memory[i] = -1; }
        memoryPageModel = new GameObject[count];
        used = 0;
        step = -1;

        //PagesList.text = ListToString(pages);
        //PagePosCount.text = count + "";
        //Dropdown_RS.value = RS;

        switch (RS)
        {
            case 0: currentRS = new RS_OPT1(this); break;
            case 1: currentRS = new RS_FIFO1(count, this); break;
            case 2: currentRS = new RS_LRU1(count, this); break;
        }
    }


    /// <summary>
    /// steps转化为string 倒序
    /// </summary>
    /// <returns></returns>
    private string GetSteps()
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


    /// <summary>
    /// 自动删除
    /// </summary>
    /// <param name="target"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator AutoDestroy(GameObject target, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(target);
    }

    ///// <summary>
    ///// 打开关闭设置面板
    ///// </summary>
    //public void _Setting()
    //{
    //    if (setting.activeInHierarchy)
    //    {
    //        setting.SetActive(false);
    //    }
    //    else
    //    {
    //        setting.SetActive(true);
    //    }
    //}

    ///// <summary>
    ///// 帮助面板
    ///// </summary>
    //public void _HelpInfo()
    //{
    //    if (helpPanel.activeInHierarchy)
    //    {
    //        helpPanel.SetActive(false);
    //    }
    //    else
    //    {
    //        helpPanel.SetActive(true);
    //    }
    //}

    ///// <summary>
    ///// 重启
    ///// </summary>
    //public void _Restart()
    //{
    //    string pageList = PagesList.text;
    //    string pagePosCount = PagePosCount.text;
    //    int dropdown_RS = Dropdown_RS.value;
    //    List<int> pageList_Int = new List<int>();

    //    PlayerPrefs.SetString("pagesList", pageList);
    //    PlayerPrefs.SetInt("pagePosCount", int.Parse(pagePosCount));
    //    PlayerPrefs.SetInt("dropDown_RS", dropdown_RS);


    //    SceneManager.LoadScene("RS");
    //}

    ///////////////////////////////////

    private void Start()
    {
        Initial();//可重复初始化的地方

        Is_OPT_RS();//对于OPT算法的额外处理

        CreateModels();//创建可视化模型

        print("当前载入：" + currentRS.RS_Name);//提示

        StartCoroutine(PageStream());//开启调度
    }

    private void Update()
    {
        Step = step;
        //if (Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftControl))
        //{
        //    avaliable = true;
        //}

        //if (!isOPT)
        //{
        //    if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
        //    {
        //        nextNum += 0;
        //    }
        //    if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
        //    {
        //        nextNum += 1;
        //    }
        //    if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
        //    {
        //        nextNum += 2;
        //    }
        //    if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
        //    {
        //        nextNum += 3;
        //    }
        //    if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
        //    {
        //        nextNum += 4;
        //    }
        //    if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
        //    {
        //        nextNum += 5;
        //    }
        //    if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
        //    {
        //        nextNum += 6;
        //    }
        //    if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7))
        //    {
        //        nextNum += 7;
        //    }
        //    if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
        //    {
        //        nextNum += 8;
        //    }
        //    if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
        //    {
        //        nextNum += 9;
        //    }
        //    if (Input.GetKeyDown(KeyCode.Backspace))
        //    {
        //        if (nextNum.Length > 0)
        //        {
        //            nextNum = nextNum.Substring(0, nextNum.Length - 1);
        //        }
        //    }

        //    if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        //    {
        //        int num = 0;
        //        if (int.TryParse(nextNum, out num))
        //        {
        //            pages.Add(num);
        //            nextNum = "";
        //            //serial.text = GetSteps();
        //        }
        //    }
        //}
        //newNum.text = nextNum;

        //if (eventSystem.currentSelectedGameObject == helpButton || eventSystem.currentSelectedGameObject == settingButton)
        //{
        //    eventSystem.SetSelectedGameObject(null);
        //}

    }

    /// <summary>
    /// 对于OPT算法的额外处理
    /// </summary>
    private void Is_OPT_RS()
    {
        steps = new List<int>();
        if (currentRS is RS_OPT)
        {
            isOPT = true;
            RS_OPT opt = currentRS as RS_OPT;
            steps = opt.PreCalculate();
            Initial();
        }
        else
        {
            isOPT = false;
        }
    }

    /// <summary>
    /// 创建可视化模型
    /// </summary>
    private void CreateModels()
    {

        position = new Transform[count];
        position[0] = Instantiate(PageFrame_Prefab, rootOfPageFrame.position + new Vector3(2, 0, 0), Quaternion.identity, rootOfPageFrame.transform).transform;
        position[0].GetChild(0).GetComponent<TextMesh>().text = "0";
        for (int i = 1; i < count; ++i)
        {
            position[i] = Instantiate(PageFrame_Prefab, position[i - 1].position + new Vector3(2, 0, 0), Quaternion.identity, rootOfPageFrame.transform).transform;
            position[i].GetChild(0).GetComponent<TextMesh>().text = i + "";
        }
        rootOfPageFrame.position += new Vector3(-(count), 5, 0);
    }

}
