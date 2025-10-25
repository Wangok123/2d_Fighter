# LATLinkedList

> 自定义链表，性能高一些

## 节点缓存机制

该类通过 AcquireNode 和 ReleaseNode 方法实现了节点缓存机制。缓存机制减少了频繁的内存分配和回收操作，从而提高了性能。

```C#
private LinkedListNode<T> AcquireNode(T value)
{
    LinkedListNode<T> node = null;
    if (m_CachedNodes.Count > 0)
    {
        node = m_CachedNodes.Dequeue();
        node.Value = value;
    }
    else
    {
        node = new LinkedListNode<T>(value);
    }
    return node;
}

private void ReleaseNode(LinkedListNode<T> node)
{
    node.Value = default(T);
    m_CachedNodes.Enqueue(node);
}
```

## 灵活的链表操作

提供了多种方法来操作链表，如 AddAfter、AddBefore、AddFirst、AddLast、Remove、RemoveFirst 和 RemoveLast 等。这些方法使得链表操作更加灵活和方便。

```C#
public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
{
    LinkedListNode<T> newNode = AcquireNode(value);
    m_LinkedList.AddAfter(node, newNode);
    return newNode;
}

public void Remove(LinkedListNode<T> node)
{
    m_LinkedList.Remove(node);
    ReleaseNode(node);
}
```

## 线程安全

通过 SyncRoot 和 IsSynchronized 属性提供了线程安全相关的信息，确保在多线程环境下的安全操作。

```C#
public object SyncRoot
{
    get
    {
        return ((ICollection)m_LinkedList).SyncRoot;
    }
}

public bool IsSynchronized
{
    get
    {
        return ((ICollection)m_LinkedList).IsSynchronized;
    }
}
```

## 枚举器支持

实现了 IEnumerable<T> 接口，并提供了内部的 Enumerator 结构，用于循环访问链表中的元素。

```C#
public struct Enumerator : IEnumerator<T>, IEnumerator
{
    private LinkedList<T>.Enumerator m_Enumerator;

    internal Enumerator(LinkedList<T> linkedList)
    {
        if (linkedList == null)
        {
            throw new GameFrameworkException("Linked list is invalid.");
        }
        m_Enumerator = linkedList.GetEnumerator();
    }

    public T Current
    {
        get
        {
            return m_Enumerator.Current;
        }
    }

    object IEnumerator.Current
    {
        get
        {
            return m_Enumerator.Current;
        }
    }
}
```