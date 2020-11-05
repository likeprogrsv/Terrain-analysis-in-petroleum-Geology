using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wang_Liu2
{   
	
    public Wang_Liu2(float[,] Z, ref float[,] Z_flat, ref int[,] depr, ref int[] outlets_list, ref int q_out, float Zmax, int Nx, int Ny, float NODATA)
    {
        //Начало работы
        priorityQueue = new PriorityQueue();
        this.NODATA = NODATA;
        this.Z = Z;
        //this.Z_flat = Z_flat;

        SetVariableValues(Z, ref Z_flat, ref depr, Nx, Ny);
        SearchLocalDepr(ref Z_flat, ref depr, Nx, Ny, ref outlets_list, ref q_out);
    }
	
	//Переменные, передаваемые в программу
    float[,] Z;             //Исходная модель    
    int[,] depr;            //Идентификатор плоскостей на модели. 1 - плоскость, 0 - не плоскость.
    float NODATA;           //Значение «нет данных»

    //float[,] Z_flat;        //Модель для заполнения "под плоскость"
    //int q_out;              //Итератор для списка точек выхода
    //int[] out_list;         //Список точек выхода (одномерный, сортировка по возрастанию высоты)


    PriorityQueue priorityQueue;
    Queue q = new Queue();      //Очередь с приоритетом
	Node x = new Node();        //Элементарный узел очереди с приоритетом
    
	int[,] mask;                //Массив, в котором будет отмечаться просмотренность/непросмотренность ячейки. 
								//0 - не просмотрена, 1 - в очереди, 2 - просмотрена

	int q2, q_max;				//Общее количество точек для обработки.
								//В начале работы программы равно количеству ячеек в матрице, затем снижается (за счёт значений "нет данных")
    
	float Z1, Z2;
    int percent_complete, percent_0;
    int c1, c2, r1, r2;

    //Скользящее окно 3х3(сначала просмотр прямых соседей, потом соседей по диагонали)
    //int k;
    int[] kx = { 1, 0, -1, 0, 1, -1, -1, 1 };
	int[] ky = { 0, 1, 0, -1, 1, 1, -1, -1 };


    //Начало работы	
    protected void SetVariableValues(float[,] Z, ref float[,] Z_flat, ref int[,] depr, int Nx, int Ny)
	{
		q_max = Nx * Ny;
		Z_flat = Z;							//Модель для заполнения "под плоскость"
		depr = SetZeros2dIntArr(Nx, Ny);	//Матрица меток понижений (границ, точек выхода)	
		mask = SetZeros2dIntArr(Nx, Ny);    //Метки процессинга
        //out_list = out_l;
        //q_out = qOut;
        //Определение первоначального списка точек
        q2 = 0;			
		InitializeSet(Nx, Ny);
	}


	protected int[,] SetZeros2dIntArr(int Nx, int Ny)
	{
		int[,] deprArr = new int[Nx, Ny];
		for (int x = 0; x < Nx; x++)
		{
			for (int y = 0; y < Ny; y++)
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
    protected void SearchLocalDepr(ref float[,] Z_flat, ref int[,] depr, int Nx, int Ny, ref int[] out_list, ref int q_out)
    {
        q_out = 0;
        percent_0 = 0;

        while(q.n > 0)
        {
            x = priorityQueue.Top(ref q);               //Извлечение первого элемента из очереди
            Z1 = x.priority;
            c1 = x.c;
            r1 = x.r;

            //Просмотр соседей извлечённого элемента
            for (int k = 0; k < kx.Length; k++)
            {
                c2 = c1 + kx[k]; r2 = r1 + ky[k];
                if (c2 < 1 || c2 >= Nx || r2 < 1 || r2 >= Ny) continue;
                if (Z[c2, r2] == NODATA) continue;

                //Проверка границы
                if(Z_flat[c2, r2] >= Z_flat[c1, r1] && depr[c1, r1] == 1 && depr[c2, r2] == 0)
                {
                    depr[c2, r2] = -1;
                    Debug.Log("Граница в ячейке Z[" + c2 + " " + r2 + "]");
                }

                //Проверка дополнительных точек выхода
                if (mask[c2, r2] == 1 && depr[c1, r1] == 1 && depr[c2, r2] == -1 && Z_flat[c2, r2] == Z_flat[c1, r1])
                {
                    depr[c2, r2] = -2;
                    Debug.Log("Точка выхода в ячейке Z[" + c2 + " " + r2 + "]");
                    q_out = q_out + 1;
                    out_list[q_out] = c1 * Ny + r1;         //"(c1 - 1) * Ny + r1" - в оригинале было так. Я же сделал
                                                            //поправку на то что в с# индексация начинается с нуля
                }

                //Пропускаются соседи, которые уже «засветились» в очереди
                if (mask[c2, r2] != 0) continue;

                //Добавление в очередь
                q2 = q2 + 1;
                priorityQueue.Enqueue(ref q, Z[c2, r2], q2, c2, r2);
                mask[c2, r2] = 1;       //Устанавливаем маску

                //Проверка понижения
                if (Z_flat[c2, r2] <= Z_flat[c1, r1])
                {
                    Z_flat[c2, r2] = Z_flat[c1, r1];
                    depr[c2, r2] = 1;

                    //Если точка, из которой мы пришли, не является понижением, маркируем её как точку выхода
                    if(depr[c1, r1] != 1)
                    {
                        depr[c1, r1] = -2;
                        q_out = q_out + 1;
                        out_list[q_out] = c1 * Ny + r1;         //"(c1 - 1) * Ny + r1" - в оригинале было так. Я же сделал
                                                                //поправку на то что в с# индексация начинается с нуля
                    }

                }
            }

            mask[c1, r1] = 2;

            //Отображение процента выполнения
            percent_complete = 100 * q2 / q_max;
            if(percent_complete > percent_0)
            {
                Debug.Log("Searching for pits: " + percent_complete + "% DEM scanned");
                percent_0 = percent_complete;
            }
        }
        if (mask != null) mask = null;
    }
}
