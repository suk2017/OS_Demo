using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public InputField PagesList;
    public InputField PagePosCount;
    public Dropdown Dropdown_RS;
    public Text serial;
    public Text newNum;
    public Text info;
    public Text[] pageLost;
    public GameObject helpPanel;
    public GameObject helpButton;
    public GameObject setting;
    public GameObject settingButton;
    public RS_Manager[] rs;
    public TextAsset[] ta;//用于验证的数据


    private EventSystem eventSystem;
    private string defaultSerial = "6 0 1 2 0 3 0 4 2 3 0 3 2 1 2 0 1 6 0 1";//默认序列
    private string defaultCount = "3";//默认页框数
    private string nextNum = "";
    private List<int> pages = ListManager.pages;
    private int ta_step = 0;//当前数据组


    /// <summary>
    /// 换一组数据
    /// </summary>
    public void _ChangeString()
    {
        ++ta_step;
        ta_step %= ta.Length;
        PagesList.text = ta[ta_step].text;
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
    /// 帮助面板
    /// </summary>
    public void _HelpInfo()
    {
        if (helpPanel.activeInHierarchy)
        {
            helpPanel.SetActive(false);
        }
        else
        {
            helpPanel.SetActive(true);
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


        SceneManager.LoadScene("RS2");
    }


    ///////////////////////////////////

    private void Start()
    {
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        PagesList.text = PlayerPrefs.GetString("pagesList");
        PagePosCount.text = "" + PlayerPrefs.GetInt("pagePosCount");
    }

    private void Update()
    {
        //防止UI焦点影响回车输入
        if (eventSystem.currentSelectedGameObject == helpButton || eventSystem.currentSelectedGameObject == settingButton)
        {
            eventSystem.SetSelectedGameObject(null);
        }

        if (Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            foreach (var i in rs)
            {
                i._Avaliable();
            }
        }

        serial.text = RS_Manager.Pages;

        for (int i = 0; i < pageLost.Length; ++i)
        {
            pageLost[i].text = "缺页 " + rs[i]._GetPageLostCount() + " 次";
        }

        //输入相关序列
        //if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
        //{
        //    nextNum += 0;
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    nextNum += 1;
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    nextNum += 2;
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    nextNum += 3;
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    nextNum += 4;
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    nextNum += 5;
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
        //{
        //    nextNum += 6;
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7))
        //{
        //    nextNum += 7;
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
        //{
        //    nextNum += 8;
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
        //{
        //    nextNum += 9;
        //}
        //if (Input.GetKeyDown(KeyCode.Backspace))
        //{
        //    if (nextNum.Length > 0)
        //    {
        //        nextNum = nextNum.Substring(0, nextNum.Length - 1);
        //    }
        //}
        //if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        //{
        //    int num = 0;
        //    if (int.TryParse(nextNum, out num))
        //    {
        //        pages.Add(num);
        //        nextNum = "";
        //        serial.text = StepsToString();
        //    }
        //}
        //newNum.text = nextNum;
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
    /// 将列表转化为字符串
    /// </summary>
    private string ListToString(List<int> value)
    {
        string result = "";
        for (int i = 0; i < value.Count; ++i)
        {
            result += value[i] + " ";
        }
        return result;
    }


}
