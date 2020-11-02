using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wang_Liu2
{   
	
    public Wang_Liu2(float[,] Z, float[,] Z_flat, int[,] depr, int[] outlets_list, int q_out, float Zmax, int Nx, int Ny, float NODATA)
    {
        //Начало работы
        priorityQueue = new PriorityQueue();
        this.NODATA = NODATA;
        this.Z = Z;

        SetVariableValues(Z, Z_flat, Nx, Ny, outlets_list, q_out);
        SearchLocalDepr();


    }
	
	//Переменные, передаваемые в программу
    float[,] Z;             //Исходная модель
    float[,] Z_flat;        //Модель для заполнения "под плоскость"
    int[,] depr;            //Идентификатор плоскостей на модели. 1 - плоскость, 0 - не плоскость.
    int[] out_list;         //Список точек выхода (одномерный, сортировка по возрастанию высоты)
    int q_out;              //Итератор для списка точек выхода
    float NODATA;           //Значение «нет данных»

    /*
    float[,] Zout;          //Результат заполнения
    int Nx, Ny;             //Размерность матрицы
    float Zmin, Zmax, stepX, stepY;     //Минимальное и максимальное значение высоты на исходной модели, Шаг сетки
    
	*/
    PriorityQueue priorityQueue;
    Queue q = new Queue();      //Очередь с приоритетом
	Node x = new Node();        //Элементарный узел очереди с приоритетом
    
	int[,] mask;                //Массив, в котором будет отмечаться просмотренность/непросмотренность ячейки. 
								//0 - не просмотрена, 1 - в очереди, 2 - просмотрена

	int q2, q_max;				//Общее количество точек для обработки.
								//В начале работы программы равно количеству ячеек в матрице, затем снижается (за счёт значений "нет данных")

	float Z1, Z2;
    int percent_complete, percent_0;

    //Скользящее окно 3х3(сначала просмотр прямых соседей, потом соседей по диагонали)
    //int k;
    int[] kx = { 1, 0, -1, 0, 1, -1, -1, 1 };
	int[] ky = { 0, 1, 0, -1, 1, 1, -1, -1 };


    //Начало работы	
    protected void SetVariableValues(float[,] Z, float[,] Z_fl, int Nx, int Ny, int[] out_l, int qOut)
	{
		q_max = Nx * Ny;
		Z_flat = Z;							//Модель для заполнения "под плоскость"
		depr = SetZeros2dIntArr(Nx, Ny);	//Матрица меток понижений (границ, точек выхода)	
		mask = SetZeros2dIntArr(Nx, Ny);    //Метки процессинга
        out_list = out_l;
        q_out = qOut;
        //Определение первоначального списка точек
        q2 = 0;			
		InitializeSet(Nx, Ny);
	}


	protected int[,] SetZeros2dIntArr(int Nx, int Ny)
	{
		int[,] deprArr = new int[Nx, Ny];
		for (int y = 0; y < Ny; y++)
		{
			for (int x = 0; x < Nx; x++)
			{
				deprArr[x, y] = 0;
			}
		}
		return deprArr;
	}

	
	protected void InitializeSet(int Nx, int Ny)
	{
		//Идея такая:
		//Просматриваются все ячейки ЦМР. По умолчанию считается, что ячейку не следует добавлять в первоначальный список (decision = .false.)
		//Однако, если ячейка находится на границе, или у неё сосед "нет данных" — решение меняется на положительное (decision = .true.)
		//Ячейки со значениями "нет данных" в список не заносятся, а сразу убираются из рассмотрения
		//В конце главного цикла, если решение положительное, ячейка добавляется в первоначальный список
		
		bool decision;		
		
		//Просматриваем по очереди все ячейки матрицы
		
		for (int c1 = 0; c1 < Nx; c1++)			//c1 это похоже column, что соответствует "x"
		{
			for (int r1 = 0; r1 < Ny; r1++)		//r1 это похоже row, что соответствует "y"
			{
				decision = false;                
				
				//Если в ячейке нет данных, исключаем её из рассмотрения насовсем
				if (Z[c1, r1] == NODATA)
				{
					mask[c1, r1] = 2;
					q_max = q_max - 1;
					continue;
				}
				
				//Если в ячейке есть данные, присматриваемся к ней повнимательнее
				for (int k = 0; k < kx.Length; k++)
				{
					//Идем к соседу
					int c2 = c1 + kx[k];
					int r2 = r1 + ky[k];
                    //Debug.Log(c2 + "     " + r2);
                    //Debug.Log(Z[c2,r2]);
                    //Debug.Log("k iteration: " + k + " c2 : " + c2 + "    c1: " + c1 + "    Kx[]k: " + kx[k]);
                    //Debug.Log("k iteration: " + k + " r2 : " + r2 + "    r1: " + r1 + "    Kx[]k: " + kx[k]);

                    if (c2 < 1 || c2 >= Nx || r2 < 1 || r2 >= Ny)
					{
						decision = true;
                        //Debug.Log("k iteration: " + k + "BREAK");

                        break;
					} 
					if (Z[c2, r2] == NODATA)		//Если сосед существует, но в нём "нет данных", принимаем решение занести ячейку в список
					{
                        //Debug.Log("k iteration: " + k + "BREAK");
                        decision = true;
						break;
					}	
				}
				
				//Добавление ячейки в список
				if (decision == true)
				{
					mask[c1, r1] = 1;
					q2 = q2 + 1;
					
					//Уточнить позже по этой операции ниже-----> Вроде в целом понятнее стало, но алгоритм интуитивно пока не совсем ясен
					priorityQueue.Enqueue(ref q, Z[c1, r1], q2, c1, r1);
                    //Debug.Log("n in Wang_Lui2: " + q.n);
                }
			}
		}
	}


    //Поиск локальных понижений
    protected void SearchLocalDepr()
    {
        q_out = 0;
        percent_0 = 0;

        while(q.n > 0)
        {
            x = priorityQueue.Top(q);               //Извлечение первого элемента из очереди
            Debug.Log(x.priority);
            q.n--;
        }
    }
}
