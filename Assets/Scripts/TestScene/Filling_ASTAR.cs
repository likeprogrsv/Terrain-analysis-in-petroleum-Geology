using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filling_ASTAR 
{
    //Переменные, передаваемые в подпрограму
    float[,] input_model;       //Исходная матрица высот
    int[,] depr;                //Матрица понижений, границ и точек выхода
    int Nx, Ny;                 //Размерность матриц
    float StepX, StepY;         //Шаг сетки (в предположении, что шаги по X и Y могут различаться)
    float Zmax;                 //Максимальная высота в пределах исходной модели. Должна быть прописана в свойствах файла
    int c_out, r_out;           //Колонка и столбец ячейки выхода
    float Zo;                   //Высота ячейки-выхода
    float nodata;
    bool connect_depressions;

    public Filling_ASTAR(float[,] input_model, int[,] depr, int c_out, int r_out, float Zo, ref float[,] output_model, int Nx, int Ny, float Zmax, float StepX, float StepY, float nodata)
    {
        this.input_model = input_model;
        this.depr = depr;
        this.c_out = c_out;
        this.r_out = r_out;
        this.Zo = Zo;
        this.Nx = Nx;
        this.Ny = Ny;
        this.Zmax = Zmax;
        this.StepX = StepX;
        this.StepY = StepY;
        this.nodata = nodata;

        SetVariables(ref output_model, Nx, Ny);
    }

    //Переменные, работающие в программе
    float[,] waylength;         //Массив, в который записывается длина пути. Это не то же самое, что стоимость пути
    int[,] way_prev;            //В этот массив мы пишем, из какой точки мы возвращаемся
    int[,] flag_status;         //Массив, отражающий «состояние» ячейки: 
    //0 — нет сведений, 
    //1 — относится к понижению, обработана, 
    //2 — относится к границе, поставлена в очередь на «обратный ход»,
    //3 — относится к понижению, поставлен в очередь на обработку
    //-1 — выход из понижения
    int[] list_cells;           //Общий список ячеек
    float[] list_Z;             //Список высот ячеек. ! Параллелен списку list_cells. Необходим для удобного сравнения высоты (не надо каждый раз обращаться к исходной матрице)
    int[] list_borders;         //Общий список граничных ячеек. Нужен, чтобы не итерировать каждый раз всю матрицу при построении обратных путей

    int c1, c2, c3, r1, r2, r3;
    float length;
    float dZ;                   //Разность высот (используется при расчёте новой высоты точки

    //Первая переменная соответствует индексу, вторая маркирует "просмотренность" (1 - в очереди, 0 - просмотрено)
    int q1, q2, q3;             //Счётчик общего количества точек и индекс текущей точки в списке
    int s, s2;                  //Переменные для конвертации строки-столбца в одно целое число (и последующей записи этого числа в список)
    //int i, j, k;              //Итераторы для циклов
    //int i1, j1;

    //Переменные для работы с алгоритмом A*
    Queue queue_points, queue_borders;
    Node x;

    float Z0, Z1, Z2, Z3, Z_temp;       //Высота точки истечения, временная новая высота заполняемой точки

    //Переменные для присвоения высоты тем ячейкам, через которые не проходит кратчайший путь
    float dist1, dist2;

    //Скользящее окно 3х3(сначала просмотр прямых соседей, потом соседей по диагонали)
    int[] kx = { 1, 0, -1, 0, 1, -1, -1, 1 };
    int[] ky = { 0, 1, 0, -1, 1, 1, -1, -1 };


    protected void SetVariables(ref float[,] output_model, int Nx, int Ny)
    {
        queue_points = new Queue();
        queue_borders = new Queue();
        x = new Node();
        waylength = new float[Nx, Ny];       
        way_prev = new int[Nx, Ny];
        flag_status = new int[Nx, Ny];
        list_cells = new int[Nx * Ny];
        list_Z = new float[Nx * Ny];
        list_borders = new int[Nx * Ny];
        connect_depressions = true;

        //Заполняем выходной массив
        for (int i = 0; i < Ny; i++)
        {
            for (int j = 0; j < Nx; j++)
            {
                if (depr[i, j] > 0)
                {
                    output_model[i, j] = nodata;
                }
                else
                {
                    output_model[i, j] = input_model[i, j];
                }
            }
        }

        //Записываем индексы ячейки-выхода в переменные, которые будут использованы в цикле
        c1 = c_out;
        r1 = r_out;

        //Подготовительные операции
        list_cells = SetZeros1dIntArr();            //Общий список ячеек
        list_borders = SetZeros1dIntArr();          //Общий список точек выхода
        list_Z = SetNODATA1dFloatArr();             //Общий список высот ячеек
        waylength = SetNODATA2dFloatArr();

        //Ставим первую точку в очередь
        flag_status = SetZeros2dIntArr();
        q1 = 1; q2 = 1; q3 = 0;
        list_cells[0] = Two_to_One(c1, r1);
        list_Z[0] = input_model[c1, r1];
        flag_status[c1, r1] = -1;

        //Переменная Z1 хранит вторую высоту, используемую в интерполяции\
        Z1 = Zmax;          //Конкретное значение высоты будет найдено в процессе работы программы
        waylength[c1, r1] = 0.0f;

        //Подготовительный этап на этом закончен
    }




    protected int[] SetZeros1dIntArr()
    {
        int[] outArr = new int[Nx * Ny];
        for (int i = 0; i < outArr.Length; i++)
        {
            outArr[i] = 0;
        }
        return outArr;
    }

    protected float[] SetNODATA1dFloatArr()
    {
        float[] outArr = new float[Nx * Ny];
        for (int i = 0; i < outArr.Length; i++)
        {
            outArr[i] = nodata;
        }
        return outArr;
    }

    protected float[,] SetNODATA2dFloatArr()
    {
        float[,] outArr = new float[Nx, Ny];
        for (int i = 0; i < Ny; i++)
        {
            for (int j = 0; j < Nx; j++)
            {
                outArr[i, j] = nodata;
            }
        }
        return outArr;
    }

    protected int[,] SetZeros2dIntArr()
    {
        int[,] outArr = new int[Nx, Ny];
        for (int x = 0; x < Ny; x++)
        {
            for (int y = 0; y < Nx; y++)
            {
                outArr[x, y] = 0;
            }
        }
        return outArr;
    }

    protected int Two_to_One(int c1, int r1)
    {
        return c1 * Ny + r1;
    }
}
