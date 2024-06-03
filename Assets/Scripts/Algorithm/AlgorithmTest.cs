using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgorithmTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var arr = new int[100];
        for (var i = 0; i < arr.Length - 1; i++)
        {
            arr[i] = Random.Range(0, 100);
        }
        LogIntArr(arr, "乱序：");
        // SortAlgorithm.QuickSort(arr);
        // SortAlgorithm.BubbleSort(arr);
        SortAlgorithm.InsertionSort(arr);
        LogIntArr(arr, "排序：");

        Test_Event += (int a) =>
        {
            return 0;
        };
        Test_Delegate += (int a) =>
        {
            return 1;
        };
    }

    void LogIntArr(int[] arr, string prefix = "", string subfix = "")
    {
        var result = "";
        for (var i = 0; i < arr.Length - 1; i++)
        {
            result += arr[i];
            if (i == arr.Length - 1) break;
            result += ", ";
        }
        Debug.Log(result);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator IE_Test()
    {
        yield return null;
        Debug.Log("12");
    }

    public delegate int Delegate_Test(int param);
    public event Delegate_Test Test_Event;
    public Delegate_Test Test_Delegate;


}
