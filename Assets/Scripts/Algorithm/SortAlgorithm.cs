
public class SortAlgorithm
{
    public static void QuickSort(int[] arr, int begin = 0, int end = -1)
    {
        if (end == -1) end = arr.Length - 1;
        if (begin < end)
        {
            int index = Partition(arr, begin, end);
            QuickSort(arr, begin, index - 1);
            QuickSort(arr, index + 1, end);
        }
    }

    static int Partition(int[] arr, int left, int right)
    {
        int pivot = left;
        left ++;
        while (left < right)
        {
            while (left < right && arr[left] <= arr[pivot])
            {
                left ++;
            }
            while (left < right && arr[pivot] < arr[right])
            {
                right --;
            }
            if (left < right)
            {
                Swap(arr, left, right);
            }
        }
        if (arr[pivot] <= arr[left])
        {
            Swap(arr, pivot, left - 1);
            return left - 1;
        }
        else
        {
            Swap(arr, pivot, left);
            return left;
        }
    }

    static void Swap(int[] arr, int left, int right)
    {
        var temp = arr[left];
        arr[left] = arr[right];
        arr[right] = temp;
    }

    public static void BubbleSort(int[] arr, int begin = 0, int end = -1)
    {
        if (end == -1) end = arr.Length - 1;
        for (var i = 0; i < arr.Length - 1; i++)
        {
            for (var j = i; j < arr.Length - 1; j++)
            {
                if (arr[i] > arr[j])
                {
                    Swap(arr, i, j);
                }
            }
        }
    }

    public static void InsertionSort(int[] arr, int begin = 0, int end = -1)
    {
        if (end == -1) end = arr.Length - 1;
        for (var i = 1; i < arr.Length - 1; i++)
        {
            int pivot = i;
            while (arr[pivot] < arr[pivot - 1])
            {
                Swap(arr, pivot - 1, pivot);
                pivot --;
                if (pivot == 0) break;
            }
        }
        return;
    }
}
