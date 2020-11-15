using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Node
{
    public float priority;     //Elevation of current node
    public int priority2;      //Global addition order
    public int c;              //column index of current node
    public int r;              //row index of current node
}

public struct Queue
{
    public List<Node> buf;
    public int n;
}



public class PriorityQueue
{

    public PriorityQueue()
    {

    }


    public void SiftDown(ref Queue q, int a)
    {
        //Queue queue = q;
        //Node temp = new Node();
        int parent, child;

        //List<Node> x = q.buf;
        parent = a;

        while(parent * 2 <= q.n)
        {
            if (parent != 0)
            {
                child = (parent * 2) - 1;
                parent--;
            } 
            //Попробовать использовать условие И ПОМЕНЯТЬ ИНДЕКСЫ В ОСТАЛЬНЫХ МЕСТАХ
            else child = 1;

            if(child + 1 < q.n)                     //было   if(child + 1 <= queue.n)
            {
                if(q.buf[child + 1].priority < q.buf[child].priority)
                {
                    child = child + 1;
                }
                else if (q.buf[child + 1].priority == q.buf[child].priority)
                {
                    if (q.buf[child + 1].priority2 < q.buf[child].priority2)
                    {
                        child = child + 1;
                    }
                }
            }

            if (q.buf[parent].priority > q.buf[child].priority)
            {
                Swap<Node>(ref q.buf, child, parent);
                parent = child;
            }
            else if (q.buf[parent].priority == q.buf[child].priority)
            {
                if (q.buf[parent].priority2 > q.buf[child].priority2)
                {
                    Swap<Node>(ref q.buf, child, parent);
                    parent = child;
                }
                else break;
            }
            else break;
        }
    }


    public void Enqueue(ref Queue q, float priority, int priority2, int c, int r)
    {
        //Queue queue = q;
        Node x = new Node();
        //Node[] tmp;
        int i;

        x.priority = priority;
        x.priority2 = priority2;
        x.c = c;
        x.r = r;
        q.n = q.n + 1;                  // ВОзможно эту операцию нужно прописать в конце метода
        //Debug.Log("n: " + queue.n);                                        //потому что в фортране индексация элементов в массиве
                                                                      //начинается с еденицы "1", а в шарпе с нуля "0"
                                                                      //поэтому в оригинальном коде переменную n сначала делают
                                                                      //равной "1".




        if (q.buf == null)
        {
            q.buf = new List<Node>();              //??            
        }

        
        if (q.buf.Count < q.n)
        {
            /*
            if (q.n == 1)
            {
                q.buf.Add(new Node());
            }
            */
            q.buf.Add(x);

            /*
			tmp = new Node[2 * q.buf.Length];
			for(int i = 0; i < q.n; i++)
			{
				tmp[i] = q.buf;
			}
			*/
        }

        i = q.n;
        
        while (i != 0)
        {
            i = i / 2;
            if (i == 0) break;
            SiftDown(ref q, i);
        }        
    }     


    public Node Top(ref Queue q)
    {
        //Queue queue = q;
        Node result = new Node();
        result = q.buf[0];
        q.buf[0] = q.buf[q.n - 1];
        q.n = q.n - 1;
        SiftDown(ref q, 0);        
        return result;
    }

    public static void Swap<T>(ref List<Node> list, int indexA, int indexB)
    {
        Node tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }
}
