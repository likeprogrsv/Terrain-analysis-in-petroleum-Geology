using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filling
{
    //Переменные, передаваемые в программу
    float[,] Z;             //Исходная модель
    float[,] Zout;          //Результат заполнения
    int Nx, Ny;             //Размерность матрицы
    float Zmin, Zmax, stepX, stepY;     //Минимальное и максимальное значение высоты на исходной модели, Шаг сетки
    float NODATA;           //Значение «нет данных»

    public Filling(float[,] Z, float[,] Zout, int Nx, int Ny, float Zmin, float Zmax, float StepX, float StepY, float NODATA)
    {
        outlets_list = CreateOutletsList(Nx, Ny);
    }

    //Внутренние переменные подпрограммы
    float[,] Zout_temp;         //Промежуточная матрица высот
    float[,] Zout_flat;         //Для работы программы заполнения нужна модель, заполненная «под плоскость»
    int[,] depr;                //Матрица, ячейки которой содержат информацию о понижениях, границах и точках выхода
    int[] outlets_list;         //Список точек выхода
    int numberOfOutlets;

    protected int[] CreateOutletsList(int Nx, int Ny)
    {
        int[] outletsList = new int[Nx * Ny];
        for (int i = 0; i < outletsList.Length; i++)
        {
            outletsList[i] = 0;
        }
        return outletsList;
    }


    /*
    protected override void CreateMap()
    {
        throw new System.NotSupportedException();
    }
    */


}
