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


    public void SiftDown(Queue q, int a)
    {
        Queue queue = q;
        //Node temp = new Node();
        int parent, child;

        List<Node> x = queue.buf;
        parent = a;

        while(parent * 2 <= queue.n)
        {
            
            child = (parent * 2) - 1;
            parent--;
            //ѕопробовать использовать условие » ѕќћ≈Ќя“№ »Ќƒ≈ —џ ¬ ќ—“јЋ№Ќџ’ ћ≈—“ј’
            if (child == 0) child = 1;

            if(child + 1 < queue.n)                     //было   if(child + 1 <= queue.n)
            {
                if(x[child + 1].priority < x[child].priority)
                {
                    child = child + 1;
                }
                else if (x[child + 1].priority == x[child].priority)
                {
                    if (x[child + 1].priority2 < x[child].priority2)
                    {
                        child = child + 1;
                    }
                }
            }

            if (x[parent].priority > x[child].priority)
            {
                Swap<Node>(x, child, parent);
                parent = child;
            }
            else if (x[parent].priority == x[child].priority)
            {
                if (x[parent].priority2 > x[child].priority2)
                {
                    Swap<Node>(x, child, parent);
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
        q.n = q.n + 1;                  // ¬ќзможно эту операцию нужно прописать в конце метода
        //Debug.Log("n: " + queue.n);                                        //потому что в фортране индексаци€ элементов в массиве
                                                                      //начинаетс€ с еденицы "1", а в шарпе с нул€ "0"
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
            SiftDown(q, i);
        }        
    }

    /*
    public void Enqueue(ref Queue q, float priority, int priority2, int c, int r)
    {
        Queue queue = q;
        Node x = new Node();
        //Node[] tmp;
        int i;

        x.priority = priority;
        x.priority2 = priority2;
        x.c = c;
        x.r = r;
        queue.n = queue.n + 1;                  // ¬ќзможно эту операцию нужно прописать в конце метода
                                                //Debug.Log("n: " + queue.n);                                        //потому что в фортране индексаци€ элементов в массиве
                                                //начинаетс€ с еденицы "1", а в шарпе с нул€ "0"
                                                //поэтому в оригинальном коде переменную n сначала делают
                                                //равной "1".




        if (queue.buf == null)
        {
            queue.buf = new List<Node>();              //??            
        }


        if (queue.buf.Count < queue.n)
        {
            queue.buf.Add(x);

            ////////////////////////////////////////
			//tmp = new Node[2 * q.buf.Length];
			//for(int i = 0; i < q.n; i++)
			//{
			//	tmp[i] = q.buf;
			//}
			////////////////////////
        }

        i = queue.n;

        while (i != 0)
        {
            i = i / 2;
            if (i == 0) break;
            SiftDown(queue, i);
        }
    }
    */


    public Node Top(Queue q)
    {
        Queue queue = q;
        Node result = new Node();
        result = q.buf[0];
        q.buf[0] = q.buf[q.n];
        q.n = q.n - 1;
        SiftDown(q, 0);        
        return result;
    }

    public static void Swap<T>(IList<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }
}
