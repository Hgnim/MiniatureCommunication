using System;
using System.IO;
using System.Text;


//类代码版本：V1.0.0.20230421
public class _2ndGenerationEncryptionMethodsFromHgnim
{
    public static int progress;//用来表示当前进度
    public readonly static int progressMax_Encrypt = 484, progressMax_Encrypt_binaryFile = 554, progressMax_Decrypt = 494;
    public string[] EncryptInoutPut(string[] inputStr, string inputPwStr,string inputType)
    {
        progress = 0;
        if (inputPwStr.IndexOf("|") != -1)
        {
            return new string[1] { "PwError" };//密码字符串不能包含|字符
        }
        switch (inputType)
        {
            case "text":
                    if (inputStr[0] == "")
                    {
                        return new string[1] { "StrNull" };//被加密的文本为空
                    }
                break;
            case "binaryFile":
                if (inputStr[0] == "" || inputStr[1] == "")
                {
                    return new string[1] { "PathNull" };//被加密的文件路径为空
                }
                else if (File.Exists(inputStr[0]) == false || Directory.Exists(inputStr[1].Substring(0,
                   inputStr[1].Length- inputStr[1].Split(char.Parse("\\"))[inputStr[1].Split(char.Parse("\\")).Length-1].Length)) == false)
                {
                    return new string[1] { "PathError" };//被加密的文件路径错误
                }
                break;
        }

        StringBuilder ES3str1;
        StringBuilder ES3str2;
        StringBuilder ES7str1;
        StringBuilder ES7str2;
        string ES11str1;
        string ES11str2;
        StringBuilder ES12str1;
        StringBuilder ES12str2;

       char[] tempChar1;
       char[] tempChar2;
       char[] tempChar3;
       char[] tempChar4;
       char[] tempChar5;
        char[] tempChar6;

        string encryptStr;//即将被加密的字符串；第一阶段（当前为原文）
        string encryptStr2;//第二阶段（当前为二进制形式）
        StringBuilder encryptStr3;//第三阶段（由第二阶段将列数为奇数和偶数的字符分别取出并按顺序排列，然后再将偶数列的字符与奇数列的字符拼接（偶在前，奇在后））
        StringBuilder encryptStr4;//第四阶段（由第三阶段每四个字符调换位置后组成的字符串）
        StringBuilder encryptStr5;//第五阶段（由第四阶段的二进制转为十六进制）
        StringBuilder encryptStr6;//第六阶段（由第五阶段每六个字符调换位置后组成的字符串635241）
        StringBuilder encryptStr7;//第七阶段（由第六阶段将列数为奇数和偶数的字符分别取出并按顺序排列，然后再将偶数列的字符与奇数列的字符拼接（偶在前，奇在后））
        StringBuilder encryptStr8;//第八阶段（由第七阶段压缩部分字符组成不规律形式）
        string encryptStr8p = null;//加密后的密钥输出
        string encryptStr8s = null;//加密后的文本输出
        string encryptStr9;//第九阶段（简易加密后的密码长度，加密后的密钥，加密后的文本综合在一起）
        StringBuilder encryptStr10;//第十阶段（由第九阶段每六个字符调换位置后组成的字符串524631）
        string encryptStr11;//第十一阶段（由第十阶段将字符串分成两半并颠倒顺序拼接，如果字符串长度为奇数，则多出的那个字符在第二半里）
        StringBuilder encryptStr12;//第十二阶段（由第十一阶段将列数为奇数和偶数的字符分别取出并按顺序排列，然后再将偶数列的字符与奇数列的字符拼接（偶在前，奇在后））

        for (int cs = 0; cs <= 1; cs++)
        {
            double progressComputeDoubCs=-1;//在加密文本和密码时进度值分开赋值，因为密钥计算快一点，所以给密钥计算的进度条空位就小一点
            switch (cs)
            {
                case 0:
                    progressComputeDoubCs = 0.02;
                    break;
                case 1:
                    progressComputeDoubCs = 0.25;
                    break;
            }

            progress ++;
            encryptStr = null;
            if (inputPwStr != "")
            {
                switch (cs)//密码和原文分开加密
                {
                    case 0:
                        switch (inputType)
                        {
                            case "text":
                                encryptStr = "t|" + inputStr[0];
                                break;
                            case "binaryFile":
                                encryptStr = inputStr[0].Split(char.Parse("\\"))[inputStr[0].Split(char.Parse("\\")).Length - 1] + 
                                    "|t|";//因为读取文件直接是以二进制的形式读取的，所以应在转二进制后添加
                                break;
                        }                       
                        break;
                    case 1:
                        encryptStr = inputPwStr + "|y";
                        break;
                }

            }
            else
            {
                switch (cs)
                {
                    case 0:
                        switch (inputType)
                        {
                            case "text":
                                encryptStr = "f|" + inputStr[0];
                                break;
                            case "binaryFile":
                                encryptStr = inputStr[0].Split(char.Parse("\\"))[inputStr[0].Split(char.Parse("\\")).Length - 1] + 
                                    "|f|";
                                break;
                        }
                        break;
                    case 1:
                        encryptStr = inputPwStr + "|n";
                        break;
                }

            }

            progress ++;
            StringBuilder byteStr = new StringBuilder();
            //encryptStr2 = null;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(encryptStr.ToString());
            for (int i = 0; i < bytes.Length; i++)
            {
                byteStr.Append(Convert.ToString(bytes[i], 2).PadLeft(8, char.Parse("0")));

                #region 进度计算
                int ints = 0, ints2 = bytes.Length;//此段语句代码时用来计算当前for语句的进度
                switch (cs)
                {
                    case 0:
                        ints = 61;
                        break;
                    case 1:
                        ints = 26;
                        break;
                }
                for (int i2 = 1; i2 < ints; i2++)
                {
                    if (ints2 < ints)
                    {
                        if (i + 1 == ints2)
                        {
                            progress += (ints - ints2);
                        }
                        else
                        {
                            progress++;
                        }
                        break;
                    }
                    else if (ints2 / ints * i2 == i)
                    {
                        progress++;
                        break;
                    }
                }
                #endregion
            }
            if (cs==0 && inputType == "binaryFile")
            {
                try
                {
                    FileStream fileStream = new FileStream(inputStr[0], FileMode.Open, FileAccess.Read);
                    BinaryReader binaryReader = new BinaryReader(fileStream);
                    byte[] bytes2 = new byte[fileStream.Length];
                    binaryReader.Read(bytes2, 0, bytes2.Length);//以二进制的形式读取文件
                    StringBuilder str =new StringBuilder();
                    for (int i = 0; i < bytes2.Length; i++)//将二进制形式转换为二进制文本
                    {
                        str .Append(Convert.ToString(bytes2[i], 2).PadLeft(8, char.Parse("0")));

                        #region 进度计算
                        int ints = 71, ints2 = bytes2.Length;//此段语句代码时用来计算当前for语句的进度
                        for (int i2 = 1; i2 < ints; i2++)
                        {
                            if (ints2 < ints)
                            {
                                if (i + 1 == ints2)
                                {
                                    progress += (ints - ints2);
                                }
                                else
                                {
                                    progress++;
                                }
                                break;
                            }
                            else if (ints2 / ints * i2 == i)
                            {
                                progress++;
                                break;
                            }
                        }
                        #endregion
                    }

                    fileStream.Close();
                    binaryReader.Close();
                    encryptStr2 = byteStr.ToString() + str.ToString();
                }
                catch { return new string[1] { "FileError" }; }               
            }
            else
            {
                encryptStr2 = byteStr.ToString();
            }

            progress ++;
            encryptStr3 = new StringBuilder();
            ES3str1 = new StringBuilder();
            ES3str2 = new StringBuilder();
            tempChar1 = new char[1];tempChar2 = new char[1];
            for (int i = 0; i + 1 < encryptStr2.ToString().Length; i += 2)
            {
                encryptStr2.CopyTo(i, tempChar1, 0, 1);
                encryptStr2.CopyTo(i + 1, tempChar2, 0, 1);
                //ES3str1.Append(encryptStr2.ToString().Substring(i, 1));
                //ES3str2.Append(encryptStr2.ToString().Substring(i + 1, 1));
                ES3str1.Append(tempChar1[0]);
                ES3str2.Append(tempChar2[0]);
                if (i + 3 == encryptStr2.ToString().Length)
                {
                    ES3str1.Append(encryptStr2.ToString().Substring(i + 2, 1));                  
                }


                ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                progressCompute.Compute(i, encryptStr2.ToString().Length - 2, progressComputeDoubCs);
                if((i+2+1< encryptStr2.ToString().Length) == false)
                {
                    progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                    progressCompute.Rectify(progressComputeDoubCs);
                }
            }
            encryptStr3.Append(ES3str1.ToString()).Append(ES3str2.ToString());

            progress++;
            encryptStr4 = new StringBuilder();
              tempChar1 = new char[1]; tempChar2 = new char[1]; tempChar3 = new char[1]; tempChar4 = new char[1];
            for (int i = 0; i < encryptStr3.ToString().Length; i += 4)
            {
                if (i + 3 < encryptStr3.ToString().Length)
                {
                    //string str1, str2, str3, str4;
                    //str1 = encryptStr3.ToString().Substring(i, 1);
                    //str2 = encryptStr3.ToString().Substring(i + 1, 1);
                    //str3 = encryptStr3.ToString().Substring(i + 2, 1);
                    //str4 = encryptStr3.ToString().Substring(i + 3, 1);
                    //encryptStr4.Append(str3).Append(str1).Append(str4).Append(str2);

                    //tempStr1.Remove(0, tempStr1.ToString().Length);
                    //tempStr2.Remove(0, tempStr2.ToString().Length);
                    //tempStr3.Remove(0, tempStr3.ToString().Length);
                    //tempStr4.Remove(0, tempStr4.ToString().Length);
                    //tempStr1.Append( encryptStr3.ToString().Substring(i, 1));
                    //tempStr2.Append( encryptStr3.ToString().Substring(i + 1, 1));
                    //tempStr3.Append( encryptStr3.ToString().Substring(i + 2, 1));
                    //tempStr4.Append( encryptStr3.ToString().Substring(i + 3, 1));
                    //encryptStr4.Append(tempStr3.ToString()).Append(tempStr1.ToString()).Append(tempStr4.ToString()).Append(tempStr2.ToString());

                    encryptStr3.CopyTo(i, tempChar1, 0, 1);
                    encryptStr3.CopyTo(i+1, tempChar2, 0, 1);
                    encryptStr3.CopyTo(i+2, tempChar3, 0, 1);
                    encryptStr3.CopyTo(i+3, tempChar4, 0, 1);
                    encryptStr4.Append(tempChar3[0]).Append(tempChar1[0]).Append(tempChar4[0]).Append(tempChar2[0]);
                }
                else
                {
                    encryptStr4.Append(encryptStr3.ToString().Substring(i, encryptStr3.ToString().Length - i));
                }

                ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                progressCompute.Compute(i, encryptStr3.ToString().Length-1, progressComputeDoubCs);
                if ((i +4 < encryptStr3.ToString().Length) == false)
                {
                    progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                    progressCompute.Rectify(progressComputeDoubCs);
                }
            }

            progress++;
            encryptStr5 = new StringBuilder();
            for (int i = 0; i < (encryptStr4.ToString().Length / 8); i++)
            {
                encryptStr5.Append(Convert.ToString((byte)Convert.ToInt32(encryptStr4.ToString().Substring(i * 8, 8).ToString(), 2), 16).PadLeft(2, char.Parse("0")));//.PadLeft(8, char.Parse("0"));

                #region 进度计算
                int ints = 0,ints2= (encryptStr4.ToString().Length / 8);//此段语句代码时用来计算当前for语句的进度
                switch (cs)
                {
                    case 0:
                        ints = 61;
                        break;
                    case 1:
                        ints = 26;
                        break;
                }
                for (int i2 = 1; i2 < ints; i2++)
                {
                    if (ints2 < ints)
                    {
                        if (i + 1 == ints2)
                        {
                            progress += (ints - ints2);
                        }
                        else
                        {
                            progress++;
                        }
                        break;
                    }
                    else if (ints2 / ints * i2 == i)
                    {
                        progress++;
                        break;
                    }
                }
                #endregion
            }

            progress ++;
            encryptStr6 = new StringBuilder();
              tempChar1 = new char[1]; tempChar2 = new char[1]; tempChar3 = new char[1]; tempChar4 = new char[1]; tempChar5 = new char[1]; tempChar6 = new char[1];
            for (int i = 0; i < encryptStr5.ToString().Length; i += 6)
            {
                if (i + 5 < encryptStr5.ToString().Length)
                {
                    //string str1, str2, str3, str4, str5, str6;
                    //str1 = encryptStr5.ToString().Substring(i, 1);
                    //str2 = encryptStr5.ToString().Substring(i + 1, 1);
                    //str3 = encryptStr5.ToString().Substring(i + 2, 1);
                    //str4 = encryptStr5.ToString().Substring(i + 3, 1);
                    //str5 = encryptStr5.ToString().Substring(i + 4, 1);
                    //str6 = encryptStr5.ToString().Substring(i + 5, 1);
                    //encryptStr6.Append(str6).Append(str3).Append(str5).Append(str2).Append(str4).Append(str1);

                    //    tempStr1.Remove(0, tempStr1.ToString().Length);
                    //    tempStr2.Remove(0, tempStr2.ToString().Length);
                    //    tempStr3.Remove(0, tempStr3.ToString().Length);
                    //    tempStr4.Remove(0, tempStr4.ToString().Length);
                    //    tempStr5.Remove(0, tempStr5.ToString().Length);
                    //    tempStr6.Remove(0, tempStr6.ToString().Length);
                    //    tempStr1.Append( encryptStr5.ToString().Substring(i, 1));
                    //    tempStr2.Append( encryptStr5.ToString().Substring(i + 1, 1));
                    //    tempStr3.Append( encryptStr5.ToString().Substring(i + 2, 1));
                    //    tempStr4.Append( encryptStr5.ToString().Substring(i + 3, 1));
                    //    tempStr5.Append( encryptStr5.ToString().Substring(i + 4, 1));
                    //    tempStr6.Append( encryptStr5.ToString().Substring(i + 5, 1));
                    //    encryptStr6.Append(tempStr6.ToString()).Append(tempStr3.ToString()).Append(tempStr5.ToString()).Append(tempStr2.ToString()).Append(tempStr4.ToString()).Append(tempStr1.ToString());
                      encryptStr5.CopyTo(i, tempChar1, 0, 1);
                      encryptStr5.CopyTo(i+1, tempChar2, 0, 1);
                      encryptStr5.CopyTo(i+2, tempChar3, 0, 1);
                      encryptStr5.CopyTo(i + 3, tempChar4, 0, 1);
                    encryptStr5.CopyTo(i + 4, tempChar5, 0, 1);
                    encryptStr5.CopyTo(i + 5, tempChar6, 0, 1);
                    encryptStr6.Append(tempChar6[0]).Append(tempChar3[0]).Append(tempChar5[0]).Append(tempChar2[0]).Append(tempChar4[0]).Append(tempChar1[0]);
                }
                else
                {
                    encryptStr6.Append(encryptStr5.ToString().Substring(i, encryptStr5.ToString().Length - i));
                }

                ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                progressCompute.Compute(i, encryptStr5.ToString().Length - 1, progressComputeDoubCs);
                if ((i + 6 < encryptStr5.ToString().Length) == false)
                {
                    progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                    progressCompute.Rectify(progressComputeDoubCs);
                }
            }

            progress++;
            encryptStr7 = new StringBuilder();
            ES7str1 = new StringBuilder();
            ES7str2 = new StringBuilder();
            tempChar1 = new char[1];tempChar2 = new char[1];
            for (int i = 0; i + 1 < encryptStr6.ToString().Length; i += 2)
            {
                encryptStr6.CopyTo(i, tempChar1, 0, 1);
                encryptStr6.CopyTo(i + 1, tempChar2, 0, 1);
                //ES7str1.Append(encryptStr6.ToString().Substring(i, 1));
                //ES7str2.Append(encryptStr6.ToString().Substring(i + 1, 1));
                ES7str1.Append(tempChar1[0]);
                ES7str2.Append(tempChar2[0]);
                if (i + 3 == encryptStr6.ToString().Length)
                {
                    ES7str1.Append(encryptStr6.ToString().Substring(i + 2, 1));
                }

                ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                progressCompute.Compute(i, encryptStr6.ToString().Length - 1-1, progressComputeDoubCs);
                if ((i + 2+1< encryptStr6.ToString().Length) == false)
                {
                    progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                    progressCompute.Rectify(progressComputeDoubCs);
                }
            }
            encryptStr7.Append(ES7str1.ToString()).Append(ES7str2.ToString());

            progress++;
            encryptStr8 = new StringBuilder();
            tempChar1 = new char[2];
            for (int i = 0; i < encryptStr7.ToString().Length; i += 2)
            {
                if (i + 2 <= encryptStr7.ToString().Length)
                {
                    encryptStr7.CopyTo(i, tempChar1, 0, 2);
                    switch (tempChar1[0].ToString() + tempChar1[1].ToString())//(encryptStr7.ToString().Substring(i, 2))
                    {
                        case "aa":
                            encryptStr8.Append("g");
                            break;
                        case "bb":
                            encryptStr8.Append("h");
                            break;
                        case "cc":
                            encryptStr8.Append("i");
                            break;
                        case "dd":
                            encryptStr8.Append("j");
                            break;
                        case "ee":
                            encryptStr8.Append("k");
                            break;
                        case "ff":
                            encryptStr8.Append("l");
                            break;
                        case "00":
                            encryptStr8.Append("m");
                            break;
                        case "11":
                            encryptStr8.Append("n");
                            break;
                        case "22":
                            encryptStr8.Append("o");
                            break;
                        case "33":
                            encryptStr8.Append("p");
                            break;
                        case "44":
                            encryptStr8.Append("q");
                            break;
                        case "55":
                            encryptStr8.Append("r");
                            break;
                        case "66":
                            encryptStr8.Append("s");
                            break;
                        case "77":
                            encryptStr8.Append("t");
                            break;
                        case "88":
                            encryptStr8.Append("u");
                            break;
                        case "99":
                            encryptStr8.Append("v");
                            break;
                        case "12":
                            encryptStr8.Append("w");
                            break;
                        case "23":
                            encryptStr8.Append("x");
                            break;
                        case "34":
                            encryptStr8.Append("y");
                            break;
                        case "45":
                            encryptStr8.Append("z");
                            break;
                        case "56":
                            encryptStr8.Append("/");
                            break;
                        case "67":
                            encryptStr8.Append("*");
                            break;
                        case "78":
                            encryptStr8.Append("-");
                            break;
                        case "89":
                            encryptStr8.Append("+");
                            break;
                        case "90":
                            encryptStr8.Append(".");
                            break;
                        case "01":
                            encryptStr8.Append("!");
                            break;
                        case "ab":
                            encryptStr8.Append(";");
                            break;
                        case "bc":
                            encryptStr8.Append("&");
                            break;
                        case "cd":
                            encryptStr8.Append("\\");
                            break;
                        case "de":
                            encryptStr8.Append("#");
                            break;
                        case "ef":
                            encryptStr8.Append("%");
                            break;
                        case "fa":
                            encryptStr8.Append("@");
                            break;
                        default:
                            encryptStr8.Append(tempChar1[0].ToString() + tempChar1[1].ToString());//(encryptStr7.ToString().Substring(i, 2));
                            break;
                    }
                }
                else
                {
                    encryptStr8.Append(encryptStr7.ToString().Substring(i, 1));
                }

                ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                progressCompute.Compute(i, encryptStr7.ToString().Length - 1 , progressComputeDoubCs);
                if ((i + 2 < encryptStr7.ToString().Length) == false)
                {
                    progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                    progressCompute.Rectify(progressComputeDoubCs);
                }
            }

            progress ++;
            switch (cs)//密码和原文分开输出
            {
                case 0:
                    encryptStr8s = encryptStr8.ToString();//加密后的文本
                    break;
                case 1:
                    encryptStr8p = encryptStr8.ToString();//加密后的密钥文本
                    break;
            }

            {//调试输出：
                //Console.WriteLine("ES3str1-" + cs + "：" + ES3str1);
                //Console.WriteLine("ES3str2-" + cs + "：" + ES3str2);
                //Console.WriteLine("ES7str1-" + cs + "：" + ES7str1);
                //Console.WriteLine("ES7str2-" + cs + "：" + ES7str2);
                //Console.WriteLine("encryptStr-" + cs + "：" + encryptStr);
                //Console.WriteLine("encryptStr2-" + cs + "：" + encryptStr2);
                //Console.WriteLine("encryptStr3-" + cs + "：" + encryptStr3);
                //Console.WriteLine("encryptStr4-" + cs + "：" + encryptStr4);
                //Console.WriteLine("encryptStr5-" + cs + "：" + encryptStr5);
                //Console.WriteLine("encryptStr6-" + cs + "：" + encryptStr6);
                //Console.WriteLine("encryptStr7-" + cs + "：" + encryptStr7);
                //Console.WriteLine("encryptStr8-" + cs + "：" + encryptStr8);
                //Console.WriteLine("encryptStr8p：" + encryptStr8p);
                //Console.WriteLine("encryptStr8s：" + encryptStr8s);
            }
        }

        progress++;
        StringBuilder pwLengthStr = new StringBuilder();//简单加密后的"密码长度"值
        for (int i = 0; i < encryptStr8p.Length.ToString().Length; i++)
        {
            switch (encryptStr8p.Length.ToString().Substring(i, 1))
            {
                case "0":
                    pwLengthStr.Append("z");
                    break;
                case "1":
                    pwLengthStr.Append("y");
                    break;
                case "2":
                    pwLengthStr.Append("x");
                    break;
                case "3":
                    pwLengthStr.Append("w");
                    break;
                case "4":
                    pwLengthStr.Append("v");
                    break;
                case "5":
                    pwLengthStr.Append("u");
                    break;
                case "6":
                    pwLengthStr.Append("t");
                    break;
                case "7":
                    pwLengthStr.Append("s");
                    break;
                case "8":
                    pwLengthStr.Append("r");
                    break;
                case "9":
                    pwLengthStr.Append("q");
                    break;
            }
        }
        pwLengthStr.Append("4");//用来做开头密码长度的分隔符

        progress ++;
        encryptStr9 = pwLengthStr.ToString() + encryptStr8p + encryptStr8s;

        progress ++;
        encryptStr10 = new StringBuilder();
           tempChar1 = new char[1]; tempChar2 = new char[1]; tempChar3 = new char[1]; tempChar4 = new char[1]; tempChar5 = new char[1]; tempChar6 = new char[1];
        for (int i = 0; i < encryptStr9.ToString().Length; i += 6)
        {
            if (i + 5 < encryptStr9.ToString().Length)
            {
                //string str1, str2, str3, str4, str5, str6;
                //str1 = encryptStr9.ToString().Substring(i, 1);
                //str2 = encryptStr9.ToString().Substring(i + 1, 1);
                //str3 = encryptStr9.ToString().Substring(i + 2, 1);
                //str4 = encryptStr9.ToString().Substring(i + 3, 1);
                //str5 = encryptStr9.ToString().Substring(i + 4, 1);
                //str6 = encryptStr9.ToString().Substring(i + 5, 1);
                //encryptStr10.Append(str2).Append(str5).Append(str4).Append(str6).Append(str3).Append(str1);

                //tempStr1.Remove(0, tempStr1.ToString().Length);
                //tempStr2.Remove(0, tempStr2.ToString().Length);
                //tempStr3.Remove(0, tempStr3.ToString().Length);
                //tempStr4.Remove(0, tempStr4.ToString().Length);
                //tempStr5.Remove(0, tempStr5.ToString().Length);
                //tempStr6.Remove(0, tempStr6.ToString().Length);
                //tempStr1.Append(encryptStr9.ToString().Substring(i, 1));
                //tempStr2.Append(encryptStr9.ToString().Substring(i + 1, 1));
                //tempStr3.Append(encryptStr9.ToString().Substring(i + 2, 1));
                //tempStr4.Append(encryptStr9.ToString().Substring(i + 3, 1));
                //tempStr5.Append(encryptStr9.ToString().Substring(i + 4, 1));
                //tempStr6.Append(encryptStr9.ToString().Substring(i + 5, 1));
                //encryptStr10.Append(tempStr2.ToString()).Append(tempStr5.ToString()).Append(tempStr4.ToString()).Append(tempStr6.ToString()).Append(tempStr3.ToString()).Append(tempStr1.ToString());

                encryptStr9.CopyTo(i, tempChar1, 0, 1);
                encryptStr9.CopyTo(i + 1, tempChar2, 0, 1);
                encryptStr9.CopyTo(i + 2, tempChar3, 0, 1);
                encryptStr9.CopyTo(i + 3, tempChar4, 0, 1);
                encryptStr9.CopyTo(i + 4, tempChar5, 0, 1);
                encryptStr9.CopyTo(i + 5, tempChar6, 0, 1);
                encryptStr10.Append(tempChar2[0]).Append(tempChar5[0]).Append(tempChar4[0]).Append(tempChar6[0]).Append(tempChar3[0]).Append(tempChar1[0]);
            }
            else
            {
                encryptStr10.Append(encryptStr9.ToString().Substring(i, encryptStr9.ToString().Length - i));

            }

            ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
            progressCompute.Compute(i, encryptStr9.ToString().Length - 1, 0.1);
            if ((i + 6 < encryptStr9.ToString().Length) == false)
            {
                progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                progressCompute.Rectify(0.1);
            }
        }

        progress++;
        //encryptStr11 =null;
        ES11str1 = encryptStr10.ToString().Substring(0, encryptStr10.ToString().Length / 2);
        ES11str2 = encryptStr10.ToString().Substring(ES11str1.Length, encryptStr10.ToString().Length - ES11str1.Length);
        encryptStr11 = ES11str2 + ES11str1;

        progress ++;
        encryptStr12 = new StringBuilder();
        ES12str1 = new StringBuilder();
        ES12str2 = new StringBuilder();
        tempChar1 = new char[1];tempChar2 = new char[1];
        for (int i = 0; i + 1 < encryptStr11.Length; i += 2)
        {
            encryptStr11.CopyTo(i, tempChar1, 0, 1);
            encryptStr11.CopyTo(i + 1, tempChar2, 0, 1);

            //ES12str1.Append(encryptStr11.Substring(i, 1));
            //ES12str2.Append(encryptStr11.Substring(i + 1, 1));
            ES12str1.Append(tempChar1[0]);
            ES12str2.Append(tempChar2[0]);
            if (i + 3 == encryptStr11.Length)
            {
                ES12str1.Append(encryptStr11.Substring(i + 2, 1));
            }

            ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
            progressCompute.Compute(i, encryptStr11.Length - 1-1, 0.1);
            if ((i + 2+1 < encryptStr11.Length) == false)
            {
                progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                progressCompute.Rectify(0.1);
            }
        }
        encryptStr12.Append(ES12str1.ToString()).Append(ES12str2.ToString());

        {  //调试输出：
            //Console.WriteLine("ES11str1：" + ES11str1);
            //Console.WriteLine("ES11str2：" + ES11str2);
            //Console.WriteLine("ES12str1：" + ES12str1);
            //Console.WriteLine("ES12str2：" + ES12str2);
            //Console.WriteLine("encryptStr9：" + encryptStr9);
            //Console.WriteLine("encryptStr10：" + encryptStr10);
            //Console.WriteLine("encryptStr11：" + encryptStr11);
            //Console.WriteLine("encryptStr12：" + encryptStr12);
        }
        progress++;
       //Console.WriteLine(progress);//最后得值为484，当inputType == "binaryFile"时，最后得值为554
        switch (inputType)
        {
            case "text":
                return new string[2] { "output", encryptStr12.ToString() };//输出加密后的字符串      
            case "binaryFile":
                try
                {
                    StreamWriter writer = new StreamWriter(inputStr[1], false, System.Text.UTF8Encoding.Default);
                    writer.Write(encryptStr12.ToString());
                    writer.Close();
                    return new string[4] { "outputFile", inputStr[0], inputStr[1],inputType };                  
                }
                catch
                {
                    return new string[1] { "FileError"};
                }
               
        }
        return new string[1] { "error" };
    }

    public string[] DecryptInoutPut(string[] inputStr, string inputPwStr, string inputType)
    {
        try
        {
            progress=0;
            if (inputPwStr.IndexOf("|") != -1)
            {
                return new string[1] { "PwError" };//密码字符串不能包含|字符
            }
            switch (inputType)
            {
                case "text":
                    if (inputStr[0] == "")
                    {
                        return new string[1] { "StrNull" };//被加密的文本为空
                    }
                    break;
                case "binaryFile":
                    if (inputStr[0] == "" || inputStr[1] == "")
                    {
                        return new string[1] { "PathNull" };//被加密的文件路径为空
                    }
                    else if (File.Exists(inputStr[0]) == false || Directory.Exists(inputStr[1]) == false)
                    {
                        return new string[1] { "PathError" };//被加密的文件路径错误
                    }
                    break;
            }


            bool passwordPass = false;
            StringBuilder ES3str1;
            StringBuilder ES3str2;
            StringBuilder ES7str1;
            StringBuilder ES7str2;
            string ES11str1;
            string ES11str2;
            StringBuilder ES12str1;
            StringBuilder ES12str2;

            char[] tempChar1;
            char[] tempChar2;
            char[] tempChar3;
            char[] tempChar4;
            char[] tempChar5;
            char[] tempChar6;

            string encryptStr;
            StringBuilder encryptStr2;
            StringBuilder encryptStr3;
            StringBuilder encryptStr4;
            StringBuilder encryptStr5;
            StringBuilder encryptStr6;
            StringBuilder encryptStr7;
            string encryptStr8;
            string encryptStr8p = null;
            string encryptStr8s = null;
            StringBuilder encryptStr9;
            string encryptStr10;
            StringBuilder encryptStr11;
            string encryptStr12;


            progress++;
            ES12str1 = null;
            ES12str2 = null;
            encryptStr12 = null;
            switch (inputType)
            {
                case "text":
                    encryptStr12 = inputStr[0];
                    break;
                case "binaryFile":
                    StreamReader reader = new StreamReader(inputStr[0], UTF8Encoding.Default);
                    encryptStr12= reader.ReadToEnd();
                    reader.Close();
                    break;
            }

            progress++;
            int i1 = (int)(encryptStr12.Length / 2);
            if (encryptStr12.Length - i1 > i1) { i1 = encryptStr12.Length - i1; }
            ES12str1 = new StringBuilder();ES12str2 = new StringBuilder();
            ES12str1.Append(encryptStr12.Substring(0, i1));
            ES12str2 .Append( encryptStr12.Substring(i1, (int)(encryptStr12.Length / 2)));
            tempChar1 = new char[1];tempChar2 = new char[1];
            encryptStr11 = new StringBuilder();
            for (int i = 0; i < ES12str1.Length || i < ES12str2.Length; i++)
            {

                if (i < ES12str1.Length) {
                    ES12str1.CopyTo(i, tempChar1, 0, 1);
                    encryptStr11.Append(tempChar1[0]);//(ES12str1.Substring(i, 1));
                }
                if (i < ES12str2.Length) {
                    ES12str2.CopyTo(i, tempChar2, 0, 1);
                    encryptStr11.Append(tempChar2[0]);//(ES12str2.Substring(i, 1));
                }


                ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                progressCompute.Compute(i, encryptStr12.Length - (encryptStr12.Length / 2) - 1, 0.1);
                if ((i +1< ES12str1.Length) == false || (i +1< ES12str2.Length) == false)
                {
                    progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                    progressCompute.Rectify(0.1);
                }
            }

            progress++;
            encryptStr10 = null;
            ES11str1 = encryptStr11.ToString().Substring(0, encryptStr11.ToString().Length - (encryptStr11.ToString().Length / 2));
            ES11str2 = encryptStr11.ToString().Substring(ES11str1.Length, encryptStr11.ToString().Length / 2);
            encryptStr10 = ES11str2 + ES11str1;

            progress++;
            encryptStr9 = new StringBuilder();
            tempChar1 = new char[1]; tempChar2 = new char[1]; tempChar3 = new char[1]; tempChar4 = new char[1]; tempChar5 = new char[1]; tempChar6 = new char[1];
            for (int i = 0; i < encryptStr10.ToString().Length; i += 6)
            {
                if (i + 5 < encryptStr10.ToString().Length)
                {
                    //string str1, str2, str3, str4, str5, str6;
                    //str1 = encryptStr10.ToString().Substring(i, 1);
                    //str2 = encryptStr10.ToString().Substring(i + 1, 1);
                    //str3 = encryptStr10.ToString().Substring(i + 2, 1);
                    //str4 = encryptStr10.ToString().Substring(i + 3, 1);
                    //str5 = encryptStr10.ToString().Substring(i + 4, 1);
                    //str6 = encryptStr10.ToString().Substring(i + 5, 1);
                    //encryptStr9.Append(str6).Append(str1).Append(str5).Append(str3).Append(str2).Append(str4);
                   
                     encryptStr10.CopyTo(i, tempChar1, 0, 1);
                     encryptStr10.CopyTo(i + 1, tempChar2, 0, 1);
                     encryptStr10.CopyTo(i + 2, tempChar3, 0, 1);
                     encryptStr10.CopyTo(i + 3, tempChar4, 0, 1);
                     encryptStr10.CopyTo(i + 4, tempChar5, 0, 1);
                     encryptStr10.CopyTo(i + 5, tempChar6, 0, 1);
                     encryptStr9.Append(tempChar6[0]).Append(tempChar1[0]).Append(tempChar5[0]).Append(tempChar3[0]).Append(tempChar2[0]).Append(tempChar4[0]);
                }
                else
                {
                    encryptStr9.Append(encryptStr10.ToString().Substring(i, encryptStr10.ToString().Length - i));
                }

                ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                progressCompute.Compute(i, encryptStr10.ToString().Length - 1, 0.1);
                if ((i + 1 < encryptStr10.ToString().Length) == false)
                {
                    progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                    progressCompute.Rectify(0.1);
                }
            }

            progress++;
            StringBuilder pwLengthStr = new StringBuilder();
            string pwLengthStr2 = encryptStr9.ToString().Split(char.Parse("4"))[0];
            for(int i = 0; i < pwLengthStr2.Length; i++)
            {
                switch (pwLengthStr2.Substring(i, 1))
            {
                    case "z":
                        pwLengthStr.Append("0");
                        break;
                    case "y":
                        pwLengthStr.Append("1");
                        break;
                    case "x":
                        pwLengthStr.Append("2");
                        break;
                    case "w":
                        pwLengthStr.Append("3");
                        break;
                    case "v":
                        pwLengthStr.Append("4");
                        break;
                    case "u":
                        pwLengthStr.Append("5");
                        break;
                    case "t":
                        pwLengthStr.Append("6");
                        break;
                    case "s":
                        pwLengthStr.Append("7");
                        break;
                    case "r":
                        pwLengthStr.Append("8");
                        break;
                    case "q":
                        pwLengthStr.Append("9");
                        break;
                }
            }

            progress++;
            if (encryptStr9.ToString().Split(char.Parse("4")).Length > 2)
            {
                StringBuilder splitStr = new StringBuilder();
                for (int i = 1; i < encryptStr9.ToString().Split(char.Parse("4")).Length; i++)
                {
                    splitStr.Append(encryptStr9.ToString().Split(char.Parse("4"))[i]);
                    if (i + 1 != encryptStr9.ToString().Split(char.Parse("4")).Length)
                    {
                        splitStr.Append("4");
                    }
                }
                encryptStr8p = splitStr.ToString().Substring(0, int.Parse(pwLengthStr.ToString()));
                encryptStr8s = splitStr.ToString().Substring(encryptStr8p.Length, splitStr.ToString().Length - encryptStr8p.Length);              
            }
            else
            {
                encryptStr8p = encryptStr9.ToString().Split(char.Parse("4"))[1].Substring(0, int.Parse(pwLengthStr.ToString()));
                encryptStr8s = encryptStr9.ToString().Split(char.Parse("4"))[1].Substring(encryptStr8p.Length, encryptStr9.ToString().Split(char.Parse("4"))[1].Length - encryptStr8p.Length);
            }

            {  //调试输出：
                //Console.WriteLine("ES11str1：" + ES11str1);
                //Console.WriteLine("ES11str2：" + ES11str2);
                //Console.WriteLine("ES12str1：" + ES12str1);
                //Console.WriteLine("ES12str2：" + ES12str2);
                //Console.WriteLine("encryptStr8p：" + encryptStr8p);
                //Console.WriteLine("encryptStr8s：" + encryptStr8s);
                //Console.WriteLine("encryptStr9：" + encryptStr9);
                //Console.WriteLine("encryptStr10：" + encryptStr10);
                //Console.WriteLine("encryptStr11：" + encryptStr11);
                //Console.WriteLine("encryptStr12：" + encryptStr12);
            }
            for (int cs = 0; cs <= 1; cs++)
            {
                double progressComputeDoubCs = -1;//在加密文本和密码时进度值分开赋值，因为密钥计算快一点，所以给密钥计算的进度条空位就小一点
                switch (cs)
                {
                    case 1:
                        progressComputeDoubCs = 0.02;
                        break;
                    case 0:
                        progressComputeDoubCs = 0.25;
                        break;
                }

                progress++;
                encryptStr8 = null;
                switch (cs)
                {
                    case 0://先解密密码
                        encryptStr8 = encryptStr8p;
                        break;
                    case 1:
                        encryptStr8 = encryptStr8s;
                        break;
                }

                progress++;
                encryptStr7 = new StringBuilder();
                tempChar1 = new char[1];
                for (int i = 0; i < encryptStr8.Length; i += 1)
                {
                    encryptStr8.CopyTo(i, tempChar1, 0, 1);
                        switch (tempChar1[0].ToString())
                        {
                            case "g":
                                encryptStr7.Append("aa");
                                break;
                            case "h":
                                encryptStr7.Append("bb");
                                break;
                            case "i":
                                encryptStr7.Append("cc");
                                break;
                            case "j":
                                encryptStr7.Append("dd");
                                break;
                            case "k":
                                encryptStr7.Append("ee");
                                break;
                            case "l":
                                encryptStr7.Append("ff");
                                break;
                            case "m":
                                encryptStr7.Append("00");
                                break;
                            case "n":
                                encryptStr7.Append("11");
                                break;
                            case "o":
                                encryptStr7.Append("22");
                                break;
                            case "p":
                                encryptStr7.Append("33");
                                break;
                            case "q":
                                encryptStr7.Append("44");
                                break;
                            case "r":
                                encryptStr7.Append("55");
                                break;
                            case "s":
                                encryptStr7.Append("66");
                                break;
                            case "t":
                                encryptStr7.Append("77");
                                break;
                            case "u":
                                encryptStr7.Append("88");
                                break;
                            case "v":
                                encryptStr7.Append("99");
                                break;
                            case "w":
                                encryptStr7.Append("12");
                                break;
                            case "x":
                                encryptStr7.Append("23");
                                break;
                            case "y":
                                encryptStr7.Append("34");
                                break;
                            case "z":
                                encryptStr7.Append("45");
                                break;
                            case "/":
                                encryptStr7.Append("56");
                                break;
                            case "*":
                                encryptStr7.Append("67");
                                break;
                            case "-":
                                encryptStr7.Append("78");
                                break;
                            case "+":
                                encryptStr7.Append("89");
                                break;
                            case ".":
                                encryptStr7.Append("90");
                                break;
                            case "!":
                                encryptStr7.Append("01");
                                break;
                            case ";":
                                encryptStr7.Append("ab");
                                break;
                            case "&":
                                encryptStr7.Append("bc");
                                break;
                            case "\\":
                                encryptStr7.Append("cd");
                                break;
                            case "#":
                                encryptStr7.Append("de");
                                break;
                            case "%":
                                encryptStr7.Append("ef");
                                break;
                            case "@":
                                encryptStr7.Append("fa");
                                break;
                            default:
                                encryptStr7.Append(tempChar1[0].ToString());
                                break;
                        }

                    ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                    progressCompute.Compute(i, encryptStr8.Length - 1, progressComputeDoubCs);
                    if ((i + 1< encryptStr8.Length) == false)
                    {
                        progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                        progressCompute.Rectify(progressComputeDoubCs);
                    }
                }

                progress++;
                ES7str1 = new StringBuilder();
                ES7str2 = new StringBuilder();
                ES7str1.Append(encryptStr7.ToString().Substring(0, encryptStr7.Length - (encryptStr7.Length / 2)));
                ES7str2.Append(encryptStr7.ToString().Substring(ES7str1.Length, (encryptStr7.Length / 2)));
                tempChar1 = new char[1];tempChar2 = new char[1];
                encryptStr6 = new StringBuilder();
                for (int i = 0; i < ES7str1.ToString().Length || i < ES7str2.ToString().Length; i++)
                {
                    if (i < ES7str1.ToString().Length) {
                        ES7str1.CopyTo(i, tempChar1, 0, 1);
                        encryptStr6.Append(tempChar1[0]); }
                    if (i < ES7str2.ToString().Length) {
                        ES7str2.CopyTo(i, tempChar2, 0, 1);
                        encryptStr6.Append(tempChar2[0]); }

                    ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                    progressCompute.Compute(i, encryptStr7.Length - (encryptStr7.Length / 2) - 1, progressComputeDoubCs);
                    if ((i+1 < ES7str1.ToString().Length)==false || (i +1< ES7str2.ToString().Length)==false)
                    {
                        progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                        progressCompute.Rectify(progressComputeDoubCs);
                    }
                }

                progress++;
                encryptStr5 = new StringBuilder();
                tempChar1 = new char[1]; tempChar2 = new char[1]; tempChar3 = new char[1]; tempChar4 = new char[1]; tempChar5 = new char[1]; tempChar6 = new char[1];
                for (int i = 0; i < encryptStr6.ToString().Length; i += 6)
                {
                    if (i + 5 < encryptStr6.ToString().Length)
                    {
                        //string str1, str2, str3, str4, str5, str6;
                        //str1 = encryptStr6.ToString().Substring(i, 1);
                        //str2 = encryptStr6.ToString().Substring(i + 1, 1);
                        //str3 = encryptStr6.ToString().Substring(i + 2, 1);
                        //str4 = encryptStr6.ToString().Substring(i + 3, 1);
                        //str5 = encryptStr6.ToString().Substring(i + 4, 1);
                        //str6 = encryptStr6.ToString().Substring(i + 5, 1);
                        //encryptStr5.Append(str6).Append(str4).Append(str2).Append(str5).Append(str3).Append(str1);

                        encryptStr6.CopyTo(i, tempChar1, 0, 1);
                        encryptStr6.CopyTo(i + 1, tempChar2, 0, 1);
                        encryptStr6.CopyTo(i + 2, tempChar3, 0, 1);
                        encryptStr6.CopyTo(i + 3, tempChar4, 0, 1);
                        encryptStr6.CopyTo(i + 4, tempChar5, 0, 1);
                        encryptStr6.CopyTo(i + 5, tempChar6, 0, 1);
                        encryptStr5.Append(tempChar6[0]).Append(tempChar4[0]).Append(tempChar2[0]).Append(tempChar5[0]).Append(tempChar3[0]).Append(tempChar1[0]);
                    }
                    else
                    {
                        encryptStr5.Append(encryptStr6.ToString().Substring(i, encryptStr6.ToString().Length - i));
                    }

                    ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                    progressCompute.Compute(i, encryptStr6.ToString().Length - 1, progressComputeDoubCs);
                    if ((i + 6 < encryptStr6.ToString().Length) == false)
                    {
                        progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                        progressCompute.Rectify(progressComputeDoubCs);
                    }
                }

                progress++;
                encryptStr4 = new StringBuilder();
                for (int i = 0; i < (encryptStr5.ToString().Length / 2); i++)
                {
                    encryptStr4.Append(Convert.ToString((byte)Convert.ToInt32(encryptStr5.ToString().Substring(i * 2, 2).ToString(), 16), 2).PadLeft(8, char.Parse("0")));//.PadLeft(8, char.Parse("0"));

                    #region 进度计算
                    int ints = 0, ints2 = (encryptStr5.ToString().Length / 2);//此段语句代码时用来计算当前for语句的进度
                    switch (cs)
                    {
                        case 1:
                            ints = 61;
                            break;
                        case 0:
                            ints = 26;
                            break;
                    }
                    for (int i2 = 1; i2 < ints; i2++)
                    {
                        if (ints2 < ints)
                        {
                            if (i + 1 == ints2)
                            {
                                progress += (ints - ints2);
                            }
                            else
                            {
                                progress++;
                            }
                            break;
                        }
                        else if (ints2 / ints * i2 == i)
                        {
                            progress++;
                            break;
                        }
                    }
                    #endregion
                }

                progress++;
                encryptStr3 = new StringBuilder();
                tempChar1 = new char[1]; tempChar2 = new char[1]; tempChar3 = new char[1]; tempChar4 = new char[1];
                for (int i = 0; i < encryptStr4.ToString().Length; i += 4)
                {
                    if (i + 3 < encryptStr4.ToString().Length)
                    {
                        //string str1, str2, str3, str4;
                        //str1 = encryptStr4.ToString().Substring(i, 1);
                        //str2 = encryptStr4.ToString().Substring(i + 1, 1);
                        //str3 = encryptStr4.ToString().Substring(i + 2, 1);
                        //str4 = encryptStr4.ToString().Substring(i + 3, 1);
                        //encryptStr3.Append(str2).Append(str4).Append(str1).Append(str3);

                        encryptStr4.CopyTo(i, tempChar1, 0, 1);
                        encryptStr4.CopyTo(i + 1, tempChar2, 0, 1);
                        encryptStr4.CopyTo(i + 2, tempChar3, 0, 1);
                        encryptStr4.CopyTo(i + 3, tempChar4, 0, 1);
                        encryptStr3.Append(tempChar2[0]).Append(tempChar4[0]).Append(tempChar1[0]).Append(tempChar3[0]);
                    }
                    else
                    {
                        encryptStr3.Append(encryptStr4.ToString().Substring(i, encryptStr4.ToString().Length - i));
                    }

                    ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                    progressCompute.Compute(i, encryptStr4.ToString().Length - 1, progressComputeDoubCs);
                    if ((i + 4 < encryptStr4.ToString().Length) == false)
                    {
                        progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                        progressCompute.Rectify(progressComputeDoubCs);
                    }
                }

                progress++;
                ES3str1 = new StringBuilder();
                ES3str2 = new StringBuilder();
                ES3str1.Append(encryptStr3.ToString().Substring(0, encryptStr3.Length - (encryptStr3.Length / 2)));
                ES3str2.Append(encryptStr3.ToString().Substring(ES3str1.Length, (encryptStr3.Length / 2)));
                tempChar1 = new char[1];tempChar2 = new char[1];
                encryptStr2 = new StringBuilder();
                for (int i = 0; i < ES3str1.ToString().Length || i < ES3str2.ToString().Length; i++)
                {
                    if (i < ES3str1.ToString().Length) {
                        ES3str1.CopyTo(i, tempChar1, 0, 1);
                        encryptStr2.Append(tempChar1[0]); }
                    if (i < ES3str2.ToString().Length) { 
                        ES3str2.CopyTo(i, tempChar2, 0, 1);
                        encryptStr2.Append(tempChar2[0]); }


                    ProgressCompute progressCompute = new ProgressCompute();//进度计算代码
                    progressCompute.Compute(i, encryptStr3.Length - (encryptStr3.Length / 2)-1, progressComputeDoubCs);
                    if ((i+1 < ES3str1.ToString().Length) == false || (i+1 < ES3str2.ToString().Length)==false)
                    {
                        progressCompute = new ProgressCompute();//如果最后的值不等，则强制纠正值
                        progressCompute.Rectify(progressComputeDoubCs);
                    }
                }

                progress++;
                byte[] bytes = new byte[(encryptStr2.ToString().Length / 8)];
                for (int i = 0; i < (encryptStr2.ToString().Length / 8); i++)
                {
                    bytes[i] = (byte)Convert.ToInt32(encryptStr2.ToString().Substring(i * 8, 8).ToString(), 2);

                    #region 进度计算
                    int ints = 0, ints2 = (encryptStr2.ToString().Length / 8);//此段语句代码时用来计算当前for语句的进度
                    switch (cs)
                    {
                        case 1:
                            ints = 61;
                            break;
                        case 0:
                            ints = 26;
                            break;
                    }
                    for (int i2 = 1; i2 < ints; i2++)
                    {
                        if (ints2 < ints)
                        {
                            if (i + 1 == ints2)
                            {
                                progress += (ints - ints2);
                            }
                            else
                            {
                                progress++;
                            }
                            break;
                        }
                        else if (ints2 / ints * i2 == i)
                        {
                            progress++;
                            break;
                        }
                    }
                    #endregion
                }
                encryptStr = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);


                {//调试输出：
                    //Console.WriteLine("ES3str1-" + cs + "：" + ES3str1);
                    //Console.WriteLine("ES3str2-" + cs + "：" + ES3str2);
                    //Console.WriteLine("ES7str1-" + cs + "：" + ES7str1);
                    //Console.WriteLine("ES7str2-" + cs + "：" + ES7str2);
                    //Console.WriteLine("encryptStr-" + cs + "：" + encryptStr);
                    //Console.WriteLine("encryptStr2-" + cs + "：" + encryptStr2);
                    //Console.WriteLine("encryptStr3-" + cs + "：" + encryptStr3);
                    //Console.WriteLine("encryptStr4-" + cs + "：" + encryptStr4);
                    //Console.WriteLine("encryptStr5-" + cs + "：" + encryptStr5);
                    //Console.WriteLine("encryptStr6-" + cs + "：" + encryptStr6);
                    //Console.WriteLine("encryptStr7-" + cs + "：" + encryptStr7);
                    //Console.WriteLine("encryptStr8-" + cs + "：" + encryptStr8);
                }            
                switch (cs)
                {
                    case 0:
                        if (encryptStr.Split(char.Parse("|"))[1] == "y")
                        {
                            if (encryptStr.Split(char.Parse("|"))[0] == inputPwStr)//密码正确则继续解密文本，错误直接返回值
                            {
                                passwordPass = true;
                            }
                            else
                            {
                                passwordPass = false;
                                return new string[1] { "PwWrong" };//密码错误返回
                            }
                        }
                        else if (encryptStr.Split(char.Parse("|"))[1] == "n")
                        {
                            passwordPass = false;
                        }
                        else { return new string[1] { "StrError" }; }

                        progress++;
                        break;
                    case 1:
                        switch (inputType)
                        {
                            case "text":
                                if (encryptStr.Split(char.Parse("|"))[0] == "t")
                                {
                                    switch (passwordPass)
                                    {
                                        case true:
                                            goto output;
                                        case false://二次判断密码是否通过，如果密码未通过，则表示加密文本损坏，因为之前如果密码未通过将直接返回，不会运行到此处
                                            return new string[1] { "StrError" };
                                    }
                                }
                                else if (encryptStr.Split(char.Parse("|"))[0] == "f")
                                {
                                    switch (passwordPass)
                                    {
                                        case true://二次判断，没有密码的情况下，"passwordPass"布尔值不可能为true，否则可能加密文本损坏
                                            return new string[1] { "StrError" };
                                        case false:
                                            goto output;                                          
                                    }
                                }
                                else { return new string[1] { "StrError" }; }
                                return new string[1] { "StrError" };
output:;
                                progress++;
                                if (encryptStr.Split(char.Parse("|")).Length > 2)//如果文本中包含"|"字符，则运行以下代码来保证所有文本正常输出
                                {
                                    StringBuilder output = new StringBuilder();
                                    for (int i = 1; i < encryptStr.Split(char.Parse("|")).Length; i++)
                                    {
                                        output.Append(encryptStr.Split(char.Parse("|"))[i]);
                                        if (i + 1 != encryptStr.Split(char.Parse("|")).Length)
                                        {
                                            output.Append("|");
                                        }
                                    }
                                    progress++;

                               // Console.WriteLine(progress);
                                    return new string[2] { "output", output.ToString() };
                                }
                                else
                                {
                                    progress++;

                                // Console.WriteLine(progress);
                                    return new string[2] { "output", encryptStr.Split(char.Parse("|"))[1] };//输出解密后的字符串
                                }
                            case "binaryFile":
                                if (encryptStr.Split(char.Parse("|"))[1] == "t")
                                {
                                    switch (passwordPass)
                                    {
                                        case true:
                                            goto output2;
                                        case false://二次判断密码是否通过，如果密码未通过，则表示加密文本损坏，因为之前如果密码未通过将直接返回，不会运行到此处
                                            return new string[1] { "StrError" };
                                    }
                                }
                                else if (encryptStr.Split(char.Parse("|"))[1] == "f")
                                {
                                    switch (passwordPass)
                                    {
                                        case true://二次判断，没有密码的情况下，"passwordPass"布尔值不可能为true，否则可能加密文本损坏
                                            return new string[1] { "StrError" };
                                        case false:
                                            goto output2;
                                    }
                                }
                                else { return new string[1] { "StrError" }; }
                                return new string[1] { "StrError" };
output2:;
                                progress++;
                                string combinOutput;
                                if (encryptStr.Split(char.Parse("|")).Length > 3)//如果文本中包含"|"字符，则运行以下代码来保证所有文本正常输出
                                {
                                    combinOutput =encryptStr.Substring((encryptStr.Split(char.Parse("|"))[0] + encryptStr.Split(char.Parse("|"))[1]  + "||").Length);
                                    //StringBuilder output = new StringBuilder();
                                    //for (int i = 2; i < encryptStr.Split(char.Parse("|")).Length; i++)
                                    //{
                                    //    output.Append(encryptStr.Split(char.Parse("|"))[i]);
                                    //    if (i + 1 != encryptStr.Split(char.Parse("|")).Length)
                                    //    {
                                    //        output.Append("|");
                                    //    }
                                    //}
                                    //combinOutput = output.ToString();                                  
                                }
                                else
                                {combinOutput = encryptStr.Split(char.Parse("|"))[2];}
                                switch (inputType)
                                {
                                    case "binaryFile":
                                        try
                                        {
                                            StringBuilder byteStr = new StringBuilder();
                                            byte[] bytes2 = System.Text.Encoding.UTF8.GetBytes(encryptStr.Split(char.Parse("|"))[0]+"|"+ encryptStr.Split(char.Parse("|"))[1]+"|");
                                            for (int i = 0; i < bytes2.Length; i++)//将前面的几个命令文本转换为二进制文本
                                            {
                                                byteStr.Append(Convert.ToString(bytes2[i], 2).PadLeft(8, char.Parse("0")));
                                            }

                                           string str = encryptStr2.ToString().Substring(byteStr.ToString().Length);//再把前面转换的二进制文本中的二进制文本形式的命令文本去掉，则得到文件数据
                                            bytes2 = new byte[(str.Length / 8)];
                                            for (int i = 0; i < (str.Length / 8); i++)
                                            {
                                                bytes2[i] = (byte)Convert.ToInt32(str.Substring(i * 8, 8), 2);
                                            }//转换为二进制
                                            //因为这个二进制数据特殊，转成字符串就不能转回去了，所以用之前转成二进制的文本（前面的二进制文本没有转成字符串过）再减去前面的命令文本就得到了文件数据
                                            System.IO.File.Create(inputStr[1] + encryptStr.Split(char.Parse("|"))[0]).Close();
                                            FileStream fileStream = new FileStream(inputStr[1]+ encryptStr.Split(char.Parse("|"))[0], FileMode.Create, FileAccess.ReadWrite);//文件名加后缀储存在加密文档里，直接获取
                                            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                                            binaryWriter.Write(bytes2,0,bytes2.Length);
                                            binaryWriter.Close();
                                            fileStream.Close();
                                        }
                                        catch { return new string[1] { "FileError" }; }
                                        break;
                                }

                                progress++;

                              // Console.WriteLine(progress);//输出值为494
                                return new string[4] { "outputFile", inputStr[0], inputStr[1]+ encryptStr.Split(char.Parse("|"))[0] ,inputType};
                        }
                      
                        break;
                }
            }
        }
        catch //(Exception ex)
        {
           // Console.WriteLine("try语句：在解密文本时发生错误！" +ex);
            return new string[1] { "StrError" };
        }
        return new string[1] { "StrError" };
    }
    
    class ProgressCompute
    {
        //readonly double doub = 0.02;
        static int addProgress = 0;
       static double lastProgDoub=0;
        double ProgDoub=0;
       internal void Compute(int intI,int intLength, double doub)
        {
            if (intI == 0) { lastProgDoub = 0;ProgDoub = 0; addProgress = 0; }

            ProgDoub = (double)intI / (double)intLength;
            if (ProgDoub - lastProgDoub > doub)
            {
               // progress++;
               int add= 1 * (int)Math.Floor((ProgDoub - lastProgDoub) / doub);
                progress += add;
                addProgress += add;
                lastProgDoub = (double)( (int)(Math.Floor(ProgDoub / doub)) * doub);
            }        
        }
        internal void Rectify(double doub)
        {
            if (addProgress < 1/doub )
            {
                progress += (int)(1/doub) - addProgress;
            }
            addProgress = 0;
        }
    }
}

///说明：
///
///加密块(EncryptInoutPut)：
///输入值(string[] inputStr, string inputPwStr,string inputType)
///inputStr[]:inputStr[0]为被加密的文本；如果inputType值为file,则inputStr[0]为输入文件路径,inputStr[1]为输出文件路径
///inputPwStr:设置加密文本的密钥
///inputType:被加密的类型(binaryFile:加密二进制文件；text:加密文本)
///返回值：
///output:输出信息，输出的信息在数组的第二索引
///PwError:返回密钥值不能包含"|"字符
///StrNull:返回被加密的文本不能为空
///PathNull:返回被加密的文件路径不能为空
///PathError:返回被加密的文件路径错误
///FileError:返回文件操作时发送错误
///outputFile:返回文件加密成功，并返回加密文件路径和输出文件路径（输出文件的后缀名要为".emfx2g"）
///error:加密错误
///
///解密块(DecryptInoutPut)：
///输入值(string inputStr, string inputPwStr)
///inputStr:被解密的文本
///inputPwStr:被解密文本的密钥
///返回值：
///output:输出信息，输出的信息在数组的第二索引
///PwError:返回密钥值不能包含"|"字符
///StrNull:返回被解密的文本不能为空
///PwWrong:返回密钥与设置的密钥不相符（密钥错误）
///StrError:返回被解密的文本有误
///PathNull:返回被加密的文件路径不能为空
///PathError:返回被加密的文件路径错误
///FileError:返回文件操作时发送错误
///outputFile:返回文件加密成功，并返回加密文件路径和输出文件路径（输出文件的后缀名要为".emfx2g"）