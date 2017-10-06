using System;

//reference - https://youtu.be/3Dw5d7PlcTM

//uses a template system with an interface
public class Heap<T> where T : IHeapItem<T>
{

    T[] items;
    int currentItemCount;

    public Heap(int maxHeapSize)
    {
        //set the size of the array
        items = new T[maxHeapSize];
    }
    //add an item to the heap-
    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    public T RemoveFirst()
    {
        //save the first item
        T firstItem = items[0];
        //then de-increment the item count
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    public bool Contains(T item)
    {
        //check to see if the item at the heap index of the item being passed in, is the item we passed in, if true it will return true, else false
        return Equals(items[item.HeapIndex], item);
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }
    void SortDown(T item)
    {
        while (true)
        {
            //the child index of the item on the left is always (2n +1)
            int childIndexLeft = item.HeapIndex * 2 + 1;
            //the index of the item on the right is always (2n + 2)
            int childIndexRight = item.HeapIndex * 2 + 2;
            int SwapIndex = 0;

            //we check to see if there is a child on the left
            if (childIndexLeft < currentItemCount)
            {
                //then we make that the swapIndex
                SwapIndex = childIndexLeft;
                //but then we need to check to see if there is a child on the right
                if (childIndexRight < currentItemCount)
                {
                    //if this child has a higher priority than the one on the left
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                    {
                        //we make that the swapIndex
                        SwapIndex = childIndexRight;
                    }
                }
                //if the parent item has a lower priority than its chosen child
                if (item.CompareTo(items[SwapIndex]) < 0)
                {
                    //swap them
                    Swap(item, items[SwapIndex]);
                }
                //otherwise, the parent is in the correct position
                else
                {
                    //so we can just return out of the loop
                    return;
                }
            }
            //or if the parent has no children
            else
            {
                //it is also in the correct position
                return;
            }
        }
    }

    void SortUp(T item)
    {
        //the index of an item's parent is always (n-1)/2
        int parentIndex = (item.HeapIndex - 1) / 2;
        while (true)
        {
            T parentItem = items[parentIndex];
            //if its the same priority, it returns 0, if it has a higher priority it returns 1 and if it has a lower it returns -1
            if (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }
    void Swap(T itemA, T itemB)
    {
        //set the positions in the array to be the index of the other
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        //then create a temporary int to store the index of item a before we change it
        int itemAIndex = itemA.HeapIndex;
        //then set item a to be item b's index
        itemA.HeapIndex = itemB.HeapIndex;
        //and set item b to the index stored in the temporary int
        itemB.HeapIndex = itemAIndex;
    }

}
public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}
