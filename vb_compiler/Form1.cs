using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
 
        string fn;
        static string[] m_TR = { "dim", "as", "integer", "byte" , "long" , "exit" , "do" , "while" , "loop" , "or" , "and" , "exit"}; //терминалы
        static char[] limiters = { '=', '(', ')', '/', '+', '-', '*', '<', '>', '#' }; //разделители           
        string s = "";
        // iserror в работе лр таблицы, analizer на этапе просмотра по букве
        bool iserror,analizer, errorsim;
        int type;
        // массив значений из таблицы ТСС
        string[,] sTSC = new string[300, 2];
        // переменные для работы логического анализатора
        int versh, pos_vhod, pos_sost, sost;
        int[] sost_stek = new int [300];
        string[] stek = new string[300];
        // переменные для работы логической функции          
        string[] lstek = new string[300];
        string opn = "";
        int lversh;
        string tip;
        bool logerror = false;
        // переменные для работы матричной функции
        string[] mstek = new string[300];
        int mversh;
        // массив в который собираем ОПН
        string[] mtrmass = new string[300];
        // strm увеличивает значения M, indstr индекс массива для ОПН
        int strm, indstr;
        string strmtr;

        void clear_proc()
        {        
            // функция для обнуления переменнных 
            //и очищения таблиц, для запуска программы сначала
            dataGridView1.Rows.Clear();
            dGV1term.Rows.Clear();
            dGV2lim.Rows.Clear();
            dGV3lit.Rows.Clear();
            dGV4ide.Rows.Clear();
            dGV5tss.Rows.Clear();
            // очистка текстовых сообщений
            errorbox.Text = "";
            opnbox.Text = "";
            matrixbox.Text = "";
            analizer = false;
            logerror = false;
            errorsim = false;
        }
        void Analys(char c)                 
        {
            // анализируем каждую букву в строке
            // меняем символ переноса строки на символ # с которым работает грамматика
            if (c == '\n')
                c = '#';
            else if (char.IsLetterOrDigit(c))
            {
                s += char.ToString(c);
                return;
            }
            if (s != null)
                {
                    if (Regex.IsMatch(s, "^[а-яА-ЯёЁ]+$"))                          //проверка на наличие русских букв 
                    {                                                               // иначе ошибка
                        errorbox.AppendText("Недопустимый символ!");
                        analizer = true;
                    }
                    int number;
                    bool result = Int32.TryParse(s, out number); //если строка содержит только цифры
                    //также возвращает false, когда буфер s содержит число большеcase 2case 147case 483 647
                    if (result)
                    {
                        type = 4;                                                   //нахождение литерала
                        vivod(s);
                        finlim(c);
                        return;
                    }

                 
                    //проверяем на принадледность к массиву терминалов
                    if (m_TR.Contains(s)) 
                    {
                        type = 1;                                                   //терминала
                        vivod(s);
                        finlim(c);
                        return;
                    }
                    else 
                    {
                        if (s.Length > 8)                                           // если длина строки преывышает 8 симв 
                        {                                                           // выводим сообщение об ошибке 
                            errorbox.AppendText("Превышена длина символов для идентификатора: " + s); 
                            analizer = true;
                            s = null;
                            return; 
                        }
                        type = 2;                                                   // идентификатор
                        vivod(s);
                        finlim(c);
                        return;
                    }
                }
            finlim(c);                                                              //разделитель
        }
        void finlim(char ci)                                                       
        {
            // процедура нахождения разделителя
            foreach (char ch in limiters)
            {
                if (ci == ch)
                {
                    s = Convert.ToString(ci);
                    type = 3;
                    vivod(s);
                    s = null;
                    return;
                }
            }
            s = null;// иначе ошибка
            if (ci != ' ' && ci != '\r' && ci != '\n')
            { errorbox.AppendText("Недопустимый символ!" + "\r\n"); errorsim = true; return; }
            
        }
        void vivod(string s)
        {
            //1 - терминал,case 2 - идентификатор,case 3 - разделитель,case 4 - литерал
            switch (type)
            {
                case 1:                                                             // заполнение таблицы ТСС согласно 
                    {                                                               // переменной type
                        dataGridView1.Rows.Add(Convert.ToString(dataGridView1.RowCount + 1));
                        dataGridView1[1, dataGridView1.RowCount - 1].Value = s;
                        dataGridView1[2, dataGridView1.RowCount - 1].Value = "терминал";
                    } break;
                case 2:
                    {
                        dataGridView1.Rows.Add(Convert.ToString(dataGridView1.RowCount + 1));
                        dataGridView1[1, dataGridView1.RowCount - 1].Value = s;
                        dataGridView1[2, dataGridView1.RowCount - 1].Value = "идентификатор";
                    } break;
                case 3:
                    {
                        dataGridView1.Rows.Add(Convert.ToString(dataGridView1.RowCount + 1));
                        dataGridView1[1, dataGridView1.RowCount - 1].Value = s;
                        dataGridView1[2, dataGridView1.RowCount - 1].Value = "разделитель";
                    } break;
                case 4:
                    {
                        dataGridView1.Rows.Add(Convert.ToString(dataGridView1.RowCount + 1));
                        dataGridView1[1, dataGridView1.RowCount - 1].Value = s;
                        dataGridView1[2, dataGridView1.RowCount - 1].Value = "литерал";
                    } break;
            }
        }
        void intables(DataGridView dgv)                                             // процедура работы с таблицей значений
        {
            // очищаем масив sTSC
            Array.Clear(sTSC, 0, 600);
            if (analizer || errorsim)
                return;
            int adrow1 = 0; int adrow2 = 0;
            int addtss = 0;
            string strid;
            for (int j = 0; j < m_TR.Length; j++)                                   //заполнение таблицы из массива терминалов
            {
                dGV1term.Rows.Add(j + 1, m_TR[j]);
            }
            for (int j = 0; j < limiters.Length; j++)                               // разделителей
            {
                dGV2lim.Rows.Add(j + 1, limiters[j]);
            }
                for (int i = 0; i < dgv.RowCount; i++)
                {
                    bool povtlit = false; bool povtlim = false; bool povtide = false;
                    strid = Convert.ToString(dgv[1, i].Value);
                    int number;
                    bool result = Int32.TryParse(strid, out number);//если строка содержит только цифры
                    //  также возвращает false, когда буфер s содержит число большеcase 2case 147case 483 647
                    if (result)
                    {
                        //если уже содержится - выход
                        for (int j = 0; j < dGV3lit.RowCount; j++)                  
                        {
                            if (Convert.ToString( dGV3lit[1, j].Value) == strid)
                                povtlit = true;
                        }
                        if (povtlit == false)
                        {                                                           // если число то в таблицу литералов
                             dGV3lit.Rows.Add(Convert.ToString( dGV3lit.RowCount));
                             dGV3lit[1, adrow2].Value = strid;
                            adrow2++;
                        }
                    }
                    for (int j = 0; j < limiters.Length; j++)
                    {
                        if (Convert.ToString(limiters[j]) == strid)
                        {
                            povtlim = true;
                        }
                    }
                    if (m_TR.Contains(strid) == false && result == false && povtlim == false)
                    {
                        //если уже содержится - выход
                        for (int j = 0; j <  dGV4ide.RowCount; j++)
                        {
                            if (Convert.ToString( dGV4ide[1, j].Value) == strid)
                                povtide = true;
                        }
                        if (povtide == false)
                        {                                                           // если идентификатор 
                             dGV4ide.Rows.Add(Convert.ToString( dGV4ide.RowCount)); //то в таблицу идентификаторов
                             dGV4ide[1, adrow1].Value = strid;
                            adrow1++;
                        }
                    }
                }
            for (int i = 0; i < dgv.RowCount; i++)                                  // заполнение таблицы тсс
            {
                string caseSwitch = Convert.ToString(dgv[2,i].Value);
                switch (caseSwitch)
                {
                    case "терминал":
                         dGV5tss.Rows.Add(Convert.ToString( dGV5tss.RowCount));
                         dGV5tss[0, addtss].Value = 1;
                        for (int j = 0; j <  dGV1term.RowCount - 1; j++)
                        {
                            if (Convert.ToString( dGV1term[1, j].Value) == Convert.ToString(dgv[1, i].Value))
                            {
                                 dGV5tss[1, addtss].Value = j+1;
                                addtss++;
                                break;
                            }
                        }
                        break;
                    case "разделитель":
                         dGV5tss.Rows.Add(Convert.ToString( dGV5tss.RowCount));
                         dGV5tss[0, addtss].Value = 2;
                        for (int j = 0; j <  dGV2lim.RowCount - 1; j++)
                        {
                            if (Convert.ToString( dGV2lim[1, j].Value) == Convert.ToString(dgv[1, i].Value))
                            {
                                 dGV5tss[1, addtss].Value = j+1;
                                addtss++;
                                break;
                            }
                        }
                        break;
                    case "литерал":
                         dGV5tss.Rows.Add(Convert.ToString( dGV5tss.RowCount));
                         dGV5tss[0, addtss].Value = 3;
                        for (int j = 0; j <  dGV3lit.RowCount - 1; j++)
                        {
                            if (Convert.ToString( dGV3lit[1, j].Value) == Convert.ToString(dgv[1, i].Value))
                            {
                                 dGV5tss[1, addtss].Value = j+1;
                                addtss++;
                                break;
                            }
                        }
                        break;
                    case "идентификатор":
                         dGV5tss.Rows.Add(Convert.ToString( dGV5tss.RowCount));
                         dGV5tss[0, addtss].Value = 4;
                        for (int j = 0; j <  dGV4ide.RowCount - 1; j++)
                        {
                            if (Convert.ToString( dGV4ide[1, j].Value) == Convert.ToString(dgv[1, i].Value))
                            {
                                 dGV5tss[1, addtss].Value = j+1;
                                addtss++;
                                break;
                            }
                        }
                        break;
                }
            }
            for (int i = 0; i <  dGV5tss.RowCount - 1; i++)
            {
                for (int k = 0; k < 2; k++)   //заполнение массива данными из
                {                                      //таблици TSC
                    sTSC[i,k] = Convert.ToString( dGV5tss[k,i].Value);
                }  
            }
        }
        string OutTSC(int pos_vhod)                                                 // функция которая возвращает
        {                                                                           // символ из тСС
            switch(Convert.ToInt32(sTSC[pos_vhod, 0]))
            {
                case 1: 
                    {
                        return m_TR[Convert.ToInt32(sTSC[pos_vhod, 1])-1];
                    } 
                case 2:
                    {
                        return Convert.ToString(limiters[Convert.ToInt32(sTSC[pos_vhod, 1])-1]);
                    }
                case 3:
                    {
                        return "lit";
                    } 
                case 4:
                    {
                        return "id";
                    } 
            }
            return "$";
        }
        void SDVIG()                                                                //процедура для работы  
        {                                                                           // метода Дейкстры
            versh++;
            stek[versh] = OutTSC(pos_vhod);
            pos_vhod++;
        }
        string follow1()                                                            // берем следующий символ
        {
            return OutTSC(pos_vhod);
        }
        void GOSTATE(int st)
        {
            pos_sost++;
            sost_stek[pos_sost] = st;
            sost = st;
        }
        void SVERTKA(int num,string neterm)                                         // процедура свертка
        {
            versh = versh + num;
            versh++;
            for (int i = versh; i < versh + num + 1; i++)
            {
                stek[i] = "";
                sost_stek[i] = 0;
            }
            stek[versh] = neterm;
            pos_sost = pos_sost + num; 
            sost = sost_stek[pos_sost];
        }
        void proverka()                                                             // процедура LR проверки 
        {
            if (analizer || errorsim)
                return;
            pos_vhod = 0;
            pos_sost = 0;
            iserror = false;
            versh = 0;
            sost = 0;
            Array.Clear(stek, 0, 300);
            Array.Clear(sost_stek, 0, 300);
            while (true)
            {
                switch (sost)
                {
                    case 0:
                        {
                            if (stek[versh] == "<S>") { iserror = false; errorbox.AppendText("Трансляция выполнена успешно!"); return; }
                            else if (stek[versh] == null) { SDVIG(); }
                            else if (stek[versh] == "<прог>") { GOSTATE(1); }
                            else if (stek[versh] == "<спис_опис>") { GOSTATE(2); }
                            else if (stek[versh] == "<опис>") { GOSTATE(3); }
                            else if (stek[versh] == "dim") { GOSTATE(4); } else { iserror = true; break; } 
                        } break;
                    case 1:
                        {
                            if (stek[versh] == "<прог>") { SVERTKA(-1, "<S>"); }
                            else { iserror = true; break; }
                        } break;
                    case 2:
                        {
                            if (stek[versh] == "<спис_опис>") { SDVIG(); }
                            else if (stek[versh] == "#") { GOSTATE(5); } else { iserror = true; break; }
                        } break;
                    case 3:
                        {
                            if (stek[versh] == "<опис>") { SVERTKA(-1, "<спис_опис>"); } else { iserror = true; break; }
                        } break;
                    case 4:
                        {
                            if (stek[versh] == "dim") { SDVIG(); }
                            else if (stek[versh] == "<спис_перем>") { GOSTATE(6); }
                            else if (stek[versh] == "<перем>") { GOSTATE(7); }
                            else if (stek[versh] == "id") { GOSTATE(8); }
                            else if (stek[versh] == "lit") { GOSTATE(9); } else { iserror = true; break; }
                        } break;
                    case 5:
                        {
                            if (stek[versh] == "#") { SDVIG(); } 
                            else if (stek[versh] == "<спис_опер>") { GOSTATE(10); } 
                            else if (stek[versh] == "<опис>") { GOSTATE(11); } 
                            else if (stek[versh] == "<опер>") { GOSTATE(12); } 
                            else if (stek[versh] == "<присв>") { GOSTATE(13); } 
                            else if (stek[versh] == "id") { GOSTATE(16); } 
                            else if (stek[versh] == "dim") { GOSTATE(4); } else { iserror = true; break; } 
                        } break;
                    case 6:
                        {
                            if (stek[versh] == "<спис_перем>") { SDVIG(); }
                            else if (stek[versh] == "as") { GOSTATE(18); }
                            else if (stek[versh] == ",") { GOSTATE(19); } else { iserror = true; break; }
                        } break;
                    case 7:
                        {
                            if (stek[versh] == "<перем>") { SVERTKA(-1, "<спис_перем>"); } else { iserror = true; break; }
                        } break;
                    case 8:
                        {
                            if (stek[versh] == "id") { SVERTKA(-1, "<перем>"); } else { iserror = true; break; }
                        } break;
                    case 9:
                        {
                            if (stek[versh] == "lit") { SVERTKA(-1, "<перем>"); } else { iserror = true; break; }
                        } break;
                    case 10:
                        {
                            if (stek[versh] == "<спис_опер>") { SDVIG(); }
                            else if (stek[versh] == "#") { GOSTATE(20); } else { iserror = true; break; }
                        } break;
                    case 11:
                        {
                            if (stek[versh] == "<опис>") { SVERTKA(-3, "<спис_опис>"); } else { iserror = true; break; }
                        } break;
                    case 12:
                        {
                            if (stek[versh] == "<опер>") { SVERTKA(-1, "<спис_опер>"); } else { iserror = true; break; }
                        } break;
                    case 13:
                        {
                            if (stek[versh] == "<присв>") { SVERTKA(-1, "<опер>"); } else { iserror = true; break; }
                        } break;
                    case 14:
                        {
                            if (stek[versh] == "<цикл>") { SVERTKA(-1, "<опер>"); } else { iserror = true; break; }
                        } break;
                    case 15:
                        {
                            if (stek[versh] == "<перем>") { SDVIG(); }
                            else if (stek[versh] == "=") { GOSTATE(22); } else { iserror = true; break; }
                        } break;
                    case 16:
                        {
                            if (stek[versh] == "id") { SDVIG(); }
                            else if (stek[versh] == "=") { GOSTATE(22); } else { iserror = true; break; }
                        } break;
                    case 17:
                        {
                            if ((stek[versh] == "do")  && (follow1() == "while")) { SDVIG(); }
                            else if (stek[versh] == "while") { GOSTATE(23); } else { iserror = true; break; }
                        } break;
                    case 18:
                        {
                            if (stek[versh] == "as") { SDVIG(); }
                            else if (stek[versh] == "<тип>") { GOSTATE(24); }
                            else if (stek[versh] == "integer") { GOSTATE(25); }
                            else if (stek[versh] == "byte") { GOSTATE(26); }
                            else if (stek[versh] == "long") { GOSTATE(27); } else { iserror = true; break; }
                        } break;
                    case 19:
                        {
                            if (stek[versh] == ",") { SDVIG(); }
                            else if (stek[versh] == "<перем>") { GOSTATE(28); }
                            else if (stek[versh] == "id") { GOSTATE(8); }
                            else if (stek[versh] == "lit") { GOSTATE(9); } else { iserror = true; break; }
                        } break;
                    case 20:
                        {
                            if ((stek[versh] == "#") && (follow1() == "$")) { SVERTKA(-4, "<прог>"); } 
                            else if ((stek[versh] == "#") && (follow1() == "id")) { SDVIG(); } 
                            else if ((stek[versh] == "#") && (follow1() == "do")) { SDVIG(); } 
                            else if (stek[versh] == "<опер>") { GOSTATE(29); } 
                            else if (stek[versh] == "<присв>") { GOSTATE(13); } 
                            else if (stek[versh] == "<цикл>") { GOSTATE(14); } 
                            else if (stek[versh] == "id") { GOSTATE(16); } 
                            else if (stek[versh] == "do") { GOSTATE(17); } else { iserror = true; break; }
                        } break;
                    case 22:
                        {
                            if (stek[versh] == "=") { SDVIG(); }
                            else if (stek[versh] == "<альт>") { GOSTATE(30); }
                            else if (stek[versh] == "<перем>") { GOSTATE(31); }
                            else if (stek[versh] == "<оп>") { GOSTATE(32); }
                            else if (stek[versh] == "id") { GOSTATE(8); }
                            else if (stek[versh] == "lit") { GOSTATE(9); } else { iserror = true; break; }
                        } break;
                    case 23:
                        {
                            if (stek[versh] == "while") { versh++; logic(); }  
                            else if (stek[versh] == "<logic>") { GOSTATE(33); } else { iserror = true; break; }
                        } break;
                    case 24:
                        {
                            if (stek[versh] == "<тип>") { SVERTKA(-4, "<опис>"); } else { iserror = true; break; }
                        } break;
                    case 25:
                        {
                            if (stek[versh] == "integer") { SVERTKA(-1, "<тип>"); } else { iserror = true; break; }
                        } break;
                    case 26:
                        {
                            if (stek[versh] == "byte") { SVERTKA(-1, "<тип>"); } else { iserror = true; break; }
                        } break;
                    case 27:
                        {
                            if (stek[versh] == "long") { SVERTKA(-1, "<тип>"); } else { iserror = true; break; }
                        } break;
                    case 28:
                        {
                            if (stek[versh] == "<перем>") { SVERTKA(-3, "<спис_перем>"); } else { iserror = true; break; }
                        } break;
                    case 29:
                        {
                            if (stek[versh] == "<опер>") { SVERTKA(-3, "<спис_опер>"); } else { iserror = true; break; }
                        } break;
                    case 30:
                        {
                            if (stek[versh] == "<альт>") { SVERTKA(-3, "<присв>"); } else { iserror = true; break; }
                        } break;
                    case 31:
                        {
                            if ((stek[versh] == "<перем>") && (follow1() == "#")) { SVERTKA(-1, "<альт>"); }
                            else if ((stek[versh] == "<перем>") && (follow1() == "+")) { SDVIG(); }
                            else if ((stek[versh] == "<перем>") && (follow1() == "-")) { SDVIG(); }
                            else if ((stek[versh] == "<перем>") && (follow1() == "*")) { SDVIG(); }
                            else if ((stek[versh] == "<перем>") && (follow1() == "/")) { SDVIG(); }
                            else if (stek[versh] == "<сим>") { GOSTATE(34); }
                            else if (stek[versh] == "+") { GOSTATE(35); }
                            else if (stek[versh] == "-") { GOSTATE(36); }
                            else if (stek[versh] == "*") { GOSTATE(37); }
                            else if (stek[versh] == "/") { GOSTATE(38); } else { iserror = true; break; }
                        } break;
                    case 32:
                        {
                            if (stek[versh] == "<оп>") { SVERTKA(-1, "<альт>"); } else { iserror = true; break; }
                        } break;
                    case 33:
                        {
                            if (stek[versh] == "<logic>") { SDVIG(); }
                            else if (stek[versh] == "#") { GOSTATE(39); } else { iserror = true; break; }
                        } break;
                    case 34:
                        {
                            if (stek[versh] == "<сим>") { SDVIG(); }
                            else if (stek[versh] == "<перем>") { GOSTATE(40); }
                            else if (stek[versh] == "lit") { GOSTATE(9); }
                            else if (stek[versh] == "id") { GOSTATE(8); } else { iserror = true; break; }
                        } break;
                    case 35:
                        {
                            if (stek[versh] == "+") { SVERTKA(-1, "<сим>"); } else { iserror = true; break; }
                        } break;
                    case 36:
                        {
                            if (stek[versh] == "-") { SVERTKA(-1, "<сим>"); } else { iserror = true; break; }
                        } break;
                    case 37:
                        {
                            if (stek[versh] == "*") { SVERTKA(-1, "<сим>"); } else { iserror = true; break; }
                        } break;
                    case 38:
                        {
                            if (stek[versh] == "/") { SVERTKA(-1, "<сим>"); } else { iserror = true; break; }
                        } break;
                    case 39:
                        {
                            if (stek[versh] == "#") { SDVIG(); } 
                            else if (stek[versh] == "<спис_опер>") { GOSTATE(41); } 
                            else if (stek[versh] == "<опер>") { GOSTATE(12); } 
                            else if (stek[versh] == "<присв>") { GOSTATE(13); } 
                            else if (stek[versh] == "<перем>") { GOSTATE(15); }
                            else if (stek[versh] == "id") { GOSTATE(8); } 
                            else if (stek[versh] == "exit") { GOSTATE(44); } else { iserror = true; break; }
                        } break;
                    case 40:
                        {
                            if (stek[versh] == "<перем>") { SVERTKA(-3, "<оп>"); } else { iserror = true; break; }
                        } break;
                    case 41:
                        {
                            if (stek[versh] == "<спис_опер>") { SDVIG(); }
                            else if (stek[versh] == "loop") { GOSTATE(47); }
                            else if (stek[versh] == "#") { GOSTATE(42); } else { iserror = true; break; }
                        } break;
                    case 42:
                        {
                            if (stek[versh] == "#") { SDVIG(); } 
                            else if (stek[versh] == "<присв>") { GOSTATE(13); } 
                            else if (stek[versh] == "id") { GOSTATE(16); } 
                            else if (stek[versh] == "exit") { GOSTATE(39); } 
                            else if (stek[versh] == "loop") { GOSTATE(43); } 
                            else if (stek[versh] == "do") { GOSTATE(20); } 
                            else if (stek[versh] == "<опер>") { GOSTATE(29); } 
                            else if (stek[versh] == "<вых_цикл>") { GOSTATE(46); } else { iserror = true; break; } 
                        } break;
                    case 43:
                        {
                             if (stek[versh] == "loop") { SVERTKA(-7, "<цикл>"); } else { iserror = true; break; }
                        } break;
                    case 44:
                       {
                           if (stek[versh] == "exit") { SDVIG(); }
                           else if (stek[versh] == "do") { GOSTATE(45); } else { iserror = true; break; }
                        } break;
                    case 45:
                        {
                            if (stek[versh] == "do") { SDVIG(); }
                            else if (stek[versh] == "#") { SVERTKA(-3, "<вых_цикл>"); } else { iserror = true; break; }
                        } break;
                    case 46:
                    {
                            if (stek[versh] == "<вых_цикл>") { SVERTKA(-1, "<опер>"); } else { iserror = true; break; }
                    } break;
                    case 47:
                        {
                            if (stek[versh] == "loop") { SVERTKA(-6, "<цикл>"); } else { iserror = true; break; }
                        } break;
                } if (iserror)
                {
                    errorbox.AppendText(ERROR(sost));
                    break;
                }
                else { continue; }
            } 
        }
        void pop()
        {
            mtrmass[indstr] = lstek[lversh];
            indstr++;
            opn = opn + lstek[lversh--];
        }
        void push(string stp)
        {
            lstek[lversh++] = stp;
        }
        void logic()                                                                        // проверка методом Дейкстры
        {
            lversh = 0;
            indstr = 0;
            string novsim = "";
            lstek[lversh] = "";
            Array.Clear(lstek, 0, 300);
            Array.Clear(mtrmass, 0, 300);
            opn = "";
            bool poperror = true;
            logerror = false;
            novsim = OutTSClog(pos_vhod);
            if (tip == "term") { return; }
            while (true)
            {
                novsim = OutTSClog(pos_vhod);
                if (tip == "op" || tip == "term")
                {
                    if (novsim == "(") { push(novsim); pos_vhod++; }
                    else if (novsim == ")")
                    {
                        if (lversh < 0) { logerror = true; return; }
                        lversh--;
                        while (lstek[lversh] != "(")
                        {
                            pop(); //записываем в массив для работы в ОПН
                            if (lversh < 0) { logerror = true; return; }
                        }
                        lstek[lversh] = null;
                        pos_vhod++;
                    }
                    else if (novsim == "<" || novsim == ">" || novsim == "or" || novsim == "and")
                    {
                        pos_vhod++;
                        if (novsim == "<" && follow1() == "=" || novsim == "<" && follow1() == ">" || novsim == ">" && follow1() == "=")
                        {
                            novsim = novsim + follow1();
                        }
                        else { pos_vhod--;}
                        if (lversh != 0)
                            lversh--; 
                        if (GetPriority(novsim) == 0 || GetPriority(novsim) > GetPriority(lstek[lversh]) || lstek[lversh] == null)
                        {
                            if (lstek[lversh] != null)
                                lversh++;
                            push(novsim);
                            pos_vhod++;
                        }
                        else
                        {
                            while (GetPriority(novsim) <= GetPriority(lstek[lversh]))
                            {
                                if (GetPriority(novsim) <= GetPriority(lstek[lversh]))
                                {
                                    pop();
                                    if (lversh < 0) { lversh = 0; poperror = false; break; }
                                }
                                else { push(novsim); break; }
                            }
                            if (poperror)
                                lversh++;
                            push(novsim);
                            pos_vhod++;
                        }
                    }
                }
                if (tip == "id")
                {
                    mtrmass[indstr] = novsim;
                    indstr++;
                    opn = opn + novsim; //записываем в массив для работы в ОПН
                    pos_vhod++;
                    if (follow1() != "<" && follow1() != ">" && follow1() == "=") { break; }
                    OutTSClog(pos_vhod);
                    if (tip == "id") { break; }
                }
                else if (novsim == "#")
                {
                    if (lstek[lversh] == null || lstek[lversh] != null)
                        lversh--;
                    while (lversh >= 0)
                    {
                        if (lstek[lversh] == "(") { logerror = true; return; }
                        pop();
                        if (lversh < 0) { lversh = 0; break; }
                    }
                    stek[versh] = "<logic>";
                    if (iserror == false)
                        opnbox.AppendText(opn + "\r\n" + "--------" + "\r\n");
                    break;
                }
            }
            perevodopn();
        }
        string OutTSClog(int pos_vhod)
        {
            switch (Convert.ToInt32(sTSC[pos_vhod, 0]))
            {
                case 1:
                    {
                        tip = "term";
                        return m_TR[Convert.ToInt32(sTSC[pos_vhod, 1]) - 1];
                    }
                case 2:
                    {
                        tip = "op";
                        return Convert.ToString(limiters[Convert.ToInt32(sTSC[pos_vhod, 1]) - 1]);
                    }
                case 3:
                    {
                        tip = "id";
                        return Convert.ToString( dGV3lit[1, Convert.ToInt32(sTSC[pos_vhod, 1]) - 1].Value);   
                    }
                case 4:
                    {
                        tip = "id";
                        return Convert.ToString( dGV4ide[1, Convert.ToInt32(sTSC[pos_vhod, 1]) - 1].Value);
                    }
            }
            return "";
        }
        byte GetPriority(string s)                                                                      // функция для получения приоритета
        {                                                                                               // операции
            switch (s)
            {
                case "(":
                    return 0;
                case ")":
                    return 1;
                case "or":
                    return 2;
                case "and":
                    return 3;
                case "<":
                case ">":
                case "=":
                case ">=":
                case "<=":
                case "<>":
                    return 4;
                default:
                    return 0;
            }
        }
        void perevodopn()                                                                                   // Перевод ОПН в матричную форму
        {
            if (logerror)
                return;
            strmtr = "";
            mversh = 0;
            strm = 0;
            strm++;
            for (int i = 0; i < indstr; i++)
            {
                if (mtrmass[i] == ">" || mtrmass[i] == "<" || mtrmass[i] == ">=" || mtrmass[i] == "<=" || mtrmass[i] == "<>" || mtrmass[i] == "or" || mtrmass[i] == "and")
                {
                    for (int j = mversh - 1; j >= mversh - 2; j--)
                    {
                        if (j < 0) { iserror = true; return; }
                        strmtr = mstek[j] + strmtr;
                    }
                    strmtr = mtrmass[i] + strmtr;
                    mstek[mversh] = "M" + strm++;
                    strmtr = mstek[mversh] +": " + strmtr;
                    matrixbox.AppendText(strmtr + "\r\n");
                    strmtr = "";
                    mstek[mversh - 2] = mstek[mversh];
                    mversh--;
                }
                else if (mtrmass[i] != null)
                {
                    mstek[mversh] = mtrmass[i];
                    mversh++;
                }
            }
            matrixbox.AppendText("--------" + "\r\n");
        }
        string ERROR(int osh)                                                                           // функция нахождения ошибки в состоянии
        {
            switch(osh)
            {
                case 0:
                    {
                        return "Error: Состояние №0: Ожидалось ключевое слово 'Dim', а встретилось :"  + stek[versh];
                    }
                case 2:
                    {
                        return "Error: Состояние №2: Ожидался разделитель окончания строки, а встретилось : " + stek[versh];
                    }
                case 4:
                    {
                        return "Error: Состояние №4: Ожидался идентификатор или литерал, а встретилось : " + stek[versh];
                    }
                case 5:
                    {
                        return "Error: Состояние №5: Ожидался ключевое слово 'Dim' или идентификатор, а встретилось : " + stek[versh];
                    }
                case 6:
                    {
                        return "Error: Состояние №6: Ожидался идетификатор или разделитель 'as', а встретилось : " + stek[versh];
                    }
                case 10:
                    {
                        return "Error: Состояние №10: Ожидался разделитель окончания строки, а встретилось : " + stek[versh];
                    }
                case 16:
                    {
                        return "Error: Состояние №16: Ожидался разделитель или ключевое слово 'do', а встретилось : " + stek[versh];
                    }
                case 17:
                    {
                        return "Error: Состояние №17: Ожидался разделитель или ключевое слово цикла 'while', а встретилось : " + stek[versh];
                    }
                case 18:
                    {
                        return "Error: Состояние №18: Ожидался тип 'integer','byte' или 'long', а встретилось : " + stek[versh];
                    }
                case 19:
                    {
                        return "Error: Состояние №19: Ожидался идентификатор или литерал, а встретилось : " + stek[versh];
                    }
                case 20:
                    {
                        return "Error: Состояние №20: Ожидалось ключевое слово 'do', а встретилось : " + stek[versh];
                    }
                case 22:
                    {
                        return "Error: Состояние №22: Ожидался идентификатор или литерал, а встретилось : " + stek[versh];
                    }
                case 23:
                    {
                        return "Error: Состояние №23: Ошибка в логическом выражении!";
                    }
                case 31:
                    {
                        return "Error: Состояние №31: Ожидался символ '+', '*', '-', '/', а встретилось : " + stek[versh];
                    }
                case 33:
                    {
                        return "Error: Состояние №33: Ожидался разделитель окончания строки, а встретилось : " + stek[versh-1];
                    }
                case 34:
                    {
                        return "Error: Состояние №34: Ожидался идентификатор или литерал, а встретилось : " + stek[versh];
                    }
                case 39:
                    {
                        return "Error: Состояние №39: Ожидался идентификатор, литерал или ключевое слово 'exit', а встретилось : " + stek[versh];
                    }
                case 41:
                    {
                        return "Error: Состояние №41: Ожидалось ключевое слово 'loop' или 'exit', а встретилось : " + stek[versh];
                    }
                case 42:
                    {
                        return "Error: Состояние №42: Ожидалось ключевое слово 'loop', 'exit' или идентификатор, а встретилось : " + stek[versh];
                    }
                case 43:
                    {
                        return "Error: Состояние №43: Ожидался разделитель окончания строки, а встретилось : " + stek[versh];
                    }
                case 44:
                    {
                        return "Error: Состояние №44: Ожидалось ключевое слово 'do', а встретилось : " + stek[versh];
                    }
            } return "";
        }
        public void button1_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            //открываем и читаем из файла
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fn = openFileDialog1.FileName;

                StreamReader sr = new StreamReader(fn);
                textBox1.Text = sr.ReadToEnd();
                textBox1.SelectionStart = textBox1.TextLength;

                sr.Close();
            }
  
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Add("Column1", "№");
            dataGridView1.Columns.Add("Column2", "Лексема");
            dataGridView1.Columns.Add("Column3", "Предварительный тип");
            dGV1term.Columns.Add("Column1", "№");
            dGV1term.Columns.Add("Column2", "Тип");
            dGV2lim.Columns.Add("Column1", "№");
            dGV2lim.Columns.Add("Column2", "Тип");
            dGV3lit.Columns.Add("Column1", "№");
            dGV3lit.Columns.Add("Column2", "Тип");
            dGV4ide.Columns.Add("Column1", "№");
            dGV4ide.Columns.Add("Column2", "Тип");
            dGV5tss.Columns.Add("Column1", "Таблица");
            dGV5tss.Columns.Add("Column2", "Строка");
        }
        private void button3_Click(object sender, EventArgs e)
        {
            clear_proc();
                //добавляем пробел в конец текста, т.к он является одним из граничащих символов 
                char last_symbol = textBox1.Text[textBox1.TextLength-1];
            // условие на добавление 1 пробела или переноса на строку
                if (last_symbol != '\n')
                    textBox1.AppendText('\n' + "");
                for (int i = 0; i <= textBox1.Text.Length - 1; i++)
                {
                    if (textBox1.Text[i] == '\'')
                    {
                        while (textBox1.Text[i] != '\n')
                            i++;
                    }
                    Analys(textBox1.Text[i]);//анализируем каждый символ текста
                    if (analizer)
                        break;
                }
            intables(dataGridView1);
            proverka();
        }
    }
}