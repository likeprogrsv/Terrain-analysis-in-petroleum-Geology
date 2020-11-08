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

        //Подготовительный этап
        SetVariables(ref output_model, Nx, Ny);
        //Процедура поиска кратчайших путей по алгоритму AT
        SearchingShortPath(ref output_model);
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

    bool noCells = false;


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
        q1 = 0; q2 = 0; q3 = 0;                     //в оригинале было q1 = 1; q2 = 1; q3 = 0
        list_cells[0] = Two_to_One(c1, r1);
        list_Z[0] = input_model[c1, r1];
        flag_status[c1, r1] = -1;

        //Переменная Z1 хранит вторую высоту, используемую в интерполяции\
        Z1 = Zmax;          //Конкретное значение высоты будет найдено в процессе работы программы
        waylength[c1, r1] = 0.0f;

        //Подготовительный этап на этом закончен
    }


    //Процедура поиска кратчайших путей по алгоритму AT
    protected void SearchingShortPath(ref float[,] output_model)
    {
        while (noCells == false)
        {
            if (q2 > 0 && list_cells[q2] == 0)  //Завершаем поиск кратчайших путей, когда в очереди не остаётся ячеек
            {                                   //То есть на очередном месте в списке-очереди — пустая запись    
                noCells = true;                 
                break;
            }
            One_to_Two(ref c1, ref r1, list_cells[q2]);     //Выбираем ячейку для просмотра на текущем шаге
            if (depr[c1, r1] == 0 || depr[c1, r1] == -1) continue;      //Пропускаем ячейку из очереди, если она не относится к понижению

            for (int k = 0; k < kx.Length; k++)     //Идём в гости к соседям
            {
                c2 = c1 + kx[k]; r2 = r1 + ky[k];
                if (c2 < 1 || c2 >= Nx || r2 < 1 || r2 >= Ny) continue;
                if (depr[c2, r2] == 0) continue;        //Если сосед не относится к понижению, прокручиваем его
                if (flag_status[c2, r2] == 2 || flag_status[c2, r2] == 3) continue;     //Если сосед уже был обработан ранее, тоже прокручиваем его

                //Вычисляем расстояние для соседа
                waylength[c2, r2] = waylength[c1, r1] + Mathf.Sqrt(Mathf.Pow(((c2 - c1) * StepX), 2) + Mathf.Pow(((r2 - r1) * StepY), 2));
                //Отмечаем путь возврата для соседа
                way_prev[c2, r2] = Two_to_One(c1, r1);
                //Отмечаем соседа как требующего обработки
                flag_status[c2, r2] = 3;
                //Добавляем новонайденного соседа в список
                switch (depr[c2, r2])
                {
                    case -2:    //Сосед — точка выхода
                //Обработка встреченных точек выхода будет зависеть от параметра connect_depressions
                //Если TRUE, система понижений обрабатывается как единое целое
                //Если FALSE, каждое понижение будет обрабатываться отдельно
                        switch (connect_depressions)
                        {
                            case true:
                                if (input_model[c2, r2] > Zo)
                                {
                                    output_model[c2, r2] = nodata;
                                    depr[c2, r2] = 1;
                                    flag_status[c2, r2] = 3;
                                    AddCell_1(c2, r2, ref q1, q2);            ///////////////////////////////////////
                                }
                                break;
                            case false:
                                flag_status[c2, r2] = 2;
                                if (Z1 > input_model[c2, r2] && input_model[c2, r2] > input_model[c_out, r_out])
                                {
                                    Z1 = input_model[c2, r2];
                                    c3 = c2; r3 = r2;
                                }
                                break;
                        }

                        break;
                    case -1:        //Сосед — граница
                        //Вставить инструкцию записи в список
                        flag_status[c2, r2] = 2;                        
                        list_borders[q3] = Two_to_One(c2, r2);
                        q3 = q3 + 1;                                    //Эта строка в оригинале была строкой выше
                        //Инструкция поиска минимальной высоты среди граничных ячеек.По моему скромному мнению, она не нужна
                        if (Z1 > input_model[c2, r2] && input_model[c2, r2] > input_model[c_out, r_out])
                        {
                            Z1 = input_model[c2, r2];
                            c3 = c2; r3 = r2;
                        }
                        break;
                    case 0:     //Сосед — просто сосед

                    default:    //Сосед — просто сосед
                        flag_status[c2, r2] = 3;
                        AddCell_1(c2, r2, ref q1, q2);
                        break;
                }
            }
            q2 = q2 + 1;
        }
    }


    protected void AddCell_1(int c, int r, ref int q1, int q2)
    {
        //c,r,q1,q2  Столбец и строка ячейки, добавляемой в список, а также номер текущего элемента в списке
        int index;
        int new_element;        //Индекс, под которым будет записан новый элемент. По умолчанию — в конец списка
        int first, last;

        index = Two_to_One(c, r);
        q1 = q1 + 1;
        new_element = q1;       //Собственно, вот оно, "по умолчанию"

        if(q1 > 1)              //Последующие операции проводятся только тогда, когда в списке больше одного элемента
        {                       //в оригинале было q > 1
            if(input_model[c, r] < list_Z[q1 - 1])
            {
                //Если новый элемент меньше последнего, запускаем процедуру сдвига.
                //Если новый элемент больше последнего, сразу ставим его в конец)
                first = q2; last = q1;      //в оригинале было first = q2 + 1; last = q1 - 1;

                //Ищем место элемента в списке. Сортировка кучей (точнее, её подобие)
                if (last != first)
                {
                    while (last != first)
                    {
                        if (input_model[c, r] < list_Z[((last - first)/2) + first])
                        {
                            last = ((last - first) / 2) + first;
                        }
                        else
                        {
                            first = ((last - first) / 2) + first + 1;        //////////Возможно еденицу в конце стоит убрать (не прибавлять её) --->  добавлять нужно, иначе цикл будет бесконечным
                        }
                    }
                }
                new_element = first;
                MoveCellsInQueue_1(new_element, q1);
            }
        }

        //Записываем новый элемент в оба списка в ту позицию, которая была определена (q, она же new_element)
        list_cells[new_element] = index;
        list_Z[new_element] = input_model[c, r];
    }


    protected void MoveCellsInQueue_1(int first_cell, int last_cell)
    {        
        for (int i = last_cell; i > last_cell - (first_cell + 1); i--)       //в оригинале do number = last_cell, first_cell+1, -1
        {
            list_cells[i] = list_cells[i - 1];
            list_Z[i] = list_Z[i - 1];
        }
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

    protected void One_to_Two(ref int column, ref int row, int indx)
    {
        row = indx % Ny;
        column = indx / Ny;
        //в оригинальном файле на fortran'е было немного по-другому, я переделал с учётом идекса [0]
    }
}
