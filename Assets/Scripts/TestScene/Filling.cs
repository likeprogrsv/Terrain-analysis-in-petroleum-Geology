using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filling //: MeshGeneratorAbstr
{

    //Переменные, передаваемые в программу
    float[,] Z;                 //Исходная модель
    //float[,] Zout;            //Результат заполнения
    int Nx, Ny;                 //Размерность матрицы
    float Zmax, StepX, StepY;   //Минимальное и максимальное значение высоты на исходной модели, Шаг сетки
    //float Zmin;     
    float NODATA;                //Значение «нет данных»


    public Filling(float[,] Z, ref float[,] Zout, int Nx, int Ny, float Zmin, float Zmax, float StepX, float StepY, float NODATA)
    {
        this.Z = Z;
        this.Nx = Nx;
        this.Ny = Ny;
        this.Zmax = Zmax;
        this.StepX = StepX;
        this.StepY = StepY;
        this.NODATA = NODATA;

        SetArrays(Nx, Ny);
        Wang_Liu2 wangLiu2 = new Wang_Liu2(Z, ref Z_flat, ref depr, ref outlets_list, ref numberOfOutlets, Zmax, Nx, Ny, NODATA);

        Debug.Log("Found " + numberOfOutlets + " outlet points");

        //Из подпрограммы переданы: «плоская» модель, матрица понижений (целочисленная), список ячеек-выходов

        //3. Подготовка результирующей матрицы (высоты ячеек понижений заменяются на значение «нет данных»
        PreparingFinalMatrix(ref Zout);

        //4. Собственно, заполнение понижений. Последовательно, по одной паре «понижение — точка выхода»        
        FillPits(ref Zout);
    }

    //Внутренние переменные подпрограммы
    float[,] Zout_temp;         //Промежуточная матрица для одного результата заполнения
    float[,] Z_flat;         //Для работы программы заполнения нужна модель, заполненная «под плоскость»
    int[,] depr;                //Матрица, ячейки которой содержат информацию о понижениях, границах и точках выхода
    int[] outlets_list;         //Список точек выхода
    int numberOfOutlets = 0;
    int percent_0, percent_complete;
    int c1, r1;                 //Итераторы для циклов
    int numberOfPits = 1;           //Сколько понижений заполнено



    protected void FillPits(ref float[,] Zout)
    {
        percent_0 = 0;
        for(int q = 0; q < numberOfOutlets; q++)
        {
            if (outlets_list[q] == 0) break;                //Выходим, когда добрались до пустой части списка
            One_to_Two(ref c1, ref r1, outlets_list[q]);    //Получаем индексы очередной точки выхода
            if (depr[c1, r1] != -2) continue;               //Прокручиваем точку выхода, если она уже не точка выхода
            Filling_ASTAR filling_ASTAR = new Filling_ASTAR(Z, depr, c1, r1, Z[c1, r1], ref Zout_temp, Nx, Ny, Zmax, StepX, StepY, NODATA);
            //Debug.Log("fill pits : " + numberOfPits);
            //numberOfPits++;

            for (int i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                {
                    //Если высота в «результирующей» модели больше, чем во «временной», переносим значение из временной матрицы на постоянную
                    //Мы заранее заготовили матрицу, где высоты локальных понижений устранены (NODATA)
                    //Поэтому, если в матрице нет значения высоты, оно просто переносится из подпрограммы
                    switch (depr[i, j])
                    {
                        case 0:         //Точка, не относящаяся к понижению, границе или выходу
                            continue;

                        case -1:        //Граница
                            if (Zout[i, j] < Zout_temp[i, j] || Zout[i, j] == NODATA) Zout[i, j] = Zout_temp[i, j];
                            break;

                        case -2:        //Выход
                            if (Zout[i, j] > Zout_temp[i, j] || Zout[i, j] == NODATA) Zout[i, j] = Zout_temp[i, j];
                            break;

                        default:        //Понижение
                            if (Zout[i, j] > Zout_temp[i, j] || Zout[i, j] == NODATA) Zout[i, j] = Zout_temp[i, j];
                            break;
                    }     
                }
            }

            //Отображение процента выполнения
            percent_complete = 100 * q / numberOfOutlets;
            if (percent_complete > percent_0)
            {
                Debug.Log("Filling: " + percent_complete + "% completed");
                percent_0 = percent_complete;
            }
        }

        //На выходе имеем матрицу Zout, содержащую заполненную ЦМР. Все промежуточные матрицы удаляем
        if (Z_flat != null) Z_flat = null;
        if (depr != null) depr = null;
        if (Zout_temp != null) Zout_temp = null;
        if (outlets_list != null) outlets_list = null;
    }

    
    protected void SetArrays(int Nx, int Ny)
    {
        Z_flat = new float[Nx, Ny];
        Zout_temp = new float[Nx, Ny];
        depr = new int[Nx, Ny];
    	outlets_list = SetZerosOutlets(Nx, Ny);
    }

    public int[] SetZerosOutlets(int Nx, int Ny)
    {

        int[] outletsList = new int[Nx * Ny];
        for (int i = 0; i < outletsList.Length; i++)
        {
            outletsList[i] = 0;
        }
        return outletsList;
    }


    protected void PreparingFinalMatrix(ref float[,] Zout)              //Подготовка результирующей матрицы (высоты ячеек понижений заменяются на значение «нет данных»
    {
        for (int i = 0; i < Nx; i++)
        {
            for (int j = 0; j < Ny; j++)
            {
                if (depr[i, j] == 0)                // ИСПРАВИТЬ depr???????
                {
                    Zout[i, j] = Z[i, j];
                }
                else Zout[i, j] = NODATA;
            }
        }
    }


    protected void One_to_Two(ref int c1, ref int r1, int indx)
    {
        r1 = indx % Ny;
        c1 = indx / Ny;
        //в оригинальном файле на fortran'е было немного по-другому, я переделал с учётом идекса [0]
    }




    /*
    protected override void CreateMap()
    {
        throw new System.NotSupportedException();
    }
    */



}
