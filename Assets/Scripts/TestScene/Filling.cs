using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filling //: MeshGeneratorAbstr
{

    //Переменные, передаваемые в программу
    float[,] Z;             //Исходная модель
    //float[,] Zout;          //Результат заполнения
    int Nx, Ny;             //Размерность матрицы
    //float Zmin, Zmax, stepX, stepY;     //Минимальное и максимальное значение высоты на исходной модели, Шаг сетки
    float NODATA;           //Значение «нет данных»


    public Filling(float[,] Z, ref float[,] Zout, int Nx, int Ny, float Zmin, float Zmax, float StepX, float StepY, float NODATA)
    {
        this.Z = Z;
        this.Nx = Nx;
        this.Ny = Ny;
        this.NODATA = NODATA;

        SetArrays(Nx, Ny);
        Wang_Liu2 wangLiu2 = new Wang_Liu2(Z, ref Z_flat, ref depr, ref outlets_list, ref numberOfOutlets, Zmax, Nx, Ny, NODATA);

        Debug.Log("Found " + numberOfOutlets + " outlet points");

        //Из подпрограммы переданы: «плоская» модель, матрица понижений (целочисленная), список ячеек-выходов

        //3. Подготовка результирующей матрицы (высоты ячеек понижений заменяются на значение «нет данных»
        PreparingFinalMatrix(ref Zout);

        //4. Собственно, заполнение понижений. Последовательно, по одной паре «понижение — точка выхода»
        FillPits();
    }

    //Внутренние переменные подпрограммы
    float[,] Zout_temp;         //Промежуточная матрица для одного результата заполнения
    float[,] Z_flat;         //Для работы программы заполнения нужна модель, заполненная «под плоскость»
    int[,] depr;                //Матрица, ячейки которой содержат информацию о понижениях, границах и точках выхода
    int[] outlets_list;         //Список точек выхода
    int numberOfOutlets = 0;
    int percent_0, percent_complete;
    int c1, r1;                 //Итераторы для циклов



    protected void FillPits()
    {
        percent_0 = 0;
        for(int q = 0; q < numberOfOutlets; q++)
        {
            if (outlets_list[q] == 0) break;                //Выходим, когда добрались до пустой части списка
            One_to_Two(ref c1, ref r1, outlets_list[q]);    //Получаем индексы очередной точки выхода
            if (depr[c1, r1] != -2) continue;               //Прокручиваем точку выхода, если она уже не точка выхода


        }
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
        for (int i = 0; i < Ny; i++)
        {
            for (int j = 0; j < Nx; j++)
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
