using System.Collections;
using System.Collections.Generic;

public class PriorityQueue<T>
{
	List<T> queue;
	IComparer<T> defaultComparer;

	static readonly int DEFAULT_CAPACITY = 11;

	public int Count {get{return queue.Count;}}

	public PriorityQueue() : this(DEFAULT_CAPACITY)
	{
	}

	public PriorityQueue(int initCapacity)
	{
		queue = new List<T>(initCapacity);
		defaultComparer = Comparer<T>.Default;
	}

	public PriorityQueue(IEnumerable<T> enumerable)
	{
		queue = new List<T>(enumerable);
		defaultComparer = Comparer<T>.Default;
	}

	public PriorityQueue(IComparer comparer)
	{
		queue = new List<T>(DEFAULT_CAPACITY);
		defaultComparer = Comparer<T>.Default;
	}

	public void Add(T e)
	{
		if (e == null)
		{
			throw new System.ArgumentNullException();
		}
		int i = Count;
		queue.Add(e);
		SiftUp(i, e);
	}

	void SiftUp(int k, T e)
	{
		while (k > 0)
		{
			int parent = (k - 1) >> 1;
			T o = queue[parent];
			if (defaultComparer.Compare(e, o) >= 0)
			{
				break;
			}
			queue[k] = o;
			k = parent;
		}
		queue[k] = e;
	}

	public T Peek()
	{
		return queue[0];
	}

	public T Poll()
	{
		if (Count == 0)
		{
			throw new System.ArgumentOutOfRangeException();
		}
		T e = queue[0];
		int s = Count - 1;
		T x = queue[s];
		queue.RemoveAt(s);
		if (s != 0)
		{
			SiftDown(0, x);
		}
		return e;
	}

	void SiftDown(int k, T e)
	{
		int half = Count >> 1;
		while (k < half)
		{
			int child = (k << 1) + 1;
			T o = queue[child];
			int right = child + 1;
			if ((right < Count) && defaultComparer.Compare(o, queue[right]) > 0)
			{
				o = queue[child = right];
			}
			if (defaultComparer.Compare(e, o) <= 0)
			{
				break;
			}
			queue[k] = o;
			k = child;
		}
		queue[k] = e;
	}

	public bool Remove(T e)
	{
		int i = queue.IndexOf(e);
		if (i == -1)
		{
			return false;
		}
		else
		{
			RemoveAt(i);
			return true;
		}
	}

	T RemoveAt(int i)
	{
		int s = Count - 1;
		if (s == i)
		{
			T e = queue[i];
			queue.RemoveAt(i);
			return e;
		}
		else
		{
			T moved = queue[s];
			queue.RemoveAt(s);
			SiftDown(i, moved);
			if (queue[i].Equals(moved))
			{
				SiftUp(i, moved);
				if (!queue[i].Equals(moved))
				{
					return moved;
				}
			}
		}
		return default(T);
	}
	
}
