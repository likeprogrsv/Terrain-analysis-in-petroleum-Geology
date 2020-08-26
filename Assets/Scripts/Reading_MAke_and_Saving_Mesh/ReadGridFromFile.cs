using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using UnityEngine;


public class ReadGridFromFile           //was ": MonoBehaviour"
{
    public static string filePath = @"E:\Unity3D\__Unity3D_Projects__\Imilor Grid in Unity3D\Imilor Grid in Unity\GridSFile\Imilor_Ach3_2.is-txt";    //Imilor_Ach3_2.is-txt
    static string dataList;

    public static float[] dataArrayValues;

    public static int countX;
    public static int countY;
    public static float xMin;
    public static float yMin;
    public static float xMax;
    public static float yMax;
    public static float lengthX;
    public static float lengthY;
    public static float stepX;
    public static float stepY;




    public void GetGridParameters(string path)
    {
        string txtFile = File.ReadAllText(path);
        string parametersList;        

        Match match = Regex.Match(txtFile, @"\[Param\](.*)\[Data\](.*)\[Faults Lines\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        parametersList = match.Groups[1].Value;
        dataList = match.Groups[2].Value;

        Match matchParam = Regex.Match(parametersList, @"CountX\s*(.*)\nCountY\s*(.*)\nXMin\s*(.*)\nYMin\s*(.*)\nXMax\s*(.*)\nYMax\s(.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);  //RegexOptions.IgnoreCase | RegexOptions.Singleline      CountX\s*(.+)\nCountY

        countX = Convert.ToInt32(matchParam.Groups[1].Value);
        countY = Convert.ToInt32(matchParam.Groups[2].Value);
        xMin = float.Parse(matchParam.Groups[3].Value, CultureInfo.InvariantCulture.NumberFormat);
        yMin = float.Parse(matchParam.Groups[4].Value, CultureInfo.InvariantCulture.NumberFormat);
        xMax = float.Parse(matchParam.Groups[5].Value, CultureInfo.InvariantCulture.NumberFormat);
        yMax = float.Parse(matchParam.Groups[6].Value, CultureInfo.InvariantCulture.NumberFormat);
        lengthX = xMax - xMin;
        lengthY = yMax - yMin;
        stepX = lengthX / countX;
        stepY = lengthY / countY;

    }

    public void GetDataValues()
    {
        string[] dataStringArrayValue = dataList.Replace(".", ",").Split(new[] { "\r\n", "\r", "\n", "\t" }, StringSplitOptions.None);   //Replacing dots into a commas and Creating a new array //string[] dataStringArrayValue = dataList.Split(new[] { "\r\n", "\r", "\n", "\t" }, StringSplitOptions.None);
        dataStringArrayValue = dataStringArrayValue.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();                //
     
        dataArrayValues = dataStringArrayValue.Select(float.Parse).ToArray();
        
    }
    

    public float TakeXmin()
    {
        return xMin;
    }

    public float TakeYmin()
    {
        return yMin;
    }

    /*
    void Start()
    {
        GetGridParameters(filePath);
        GetDataValues();
    }
    */      
}
