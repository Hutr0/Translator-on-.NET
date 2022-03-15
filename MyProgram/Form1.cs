using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Globalization;
using System.IO;

namespace MyProgram
{
    public partial class Form1 : Form
    {

        //Синхронизация скроллинга двух richtextbox
        public enum ScrollBarType : uint
        {
            SbHorz = 0, SbVert = 1, SbCtl = 2, SbBoth = 3
        }
        public enum Message : uint
        { WM_VSCROLL = 0x0115 }
        public enum ScrollBarCommands : uint
        { SB_THUMBPOSITION = 4 }
        [DllImport("User32.dll")]
        public extern static int GetScrollPos(IntPtr hWnd, int nBar);
        [DllImport("User32.dll")]
        public extern static int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        //Конец блока для синхронизации

        private void textIn_VScroll(object sender, EventArgs e)
        {
            int nPos = GetScrollPos(textIn.Handle, (int)ScrollBarType.SbVert);
            nPos <<= 16; uint wParam = (uint)ScrollBarCommands.SB_THUMBPOSITION | (uint)nPos;
            SendMessage(numeric_textBox.Handle, (int)Message.WM_VSCROLL, new IntPtr(wParam), new IntPtr(0));
        }

        private void textIn_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            int linesCount = textIn.Lines.Count();
            numeric_textBox.Text = string.Empty;
            if (linesCount == 0) return;
            string text = string.Empty;
            for (int i = 1; i < linesCount; i++)
            { text = text + i.ToString() + Environment.NewLine; }
            text = text + linesCount.ToString();
            numeric_textBox.Text = text;
            textIn_VScroll(sender, e);
        }

        public List<string> reservedWords = new List<string> { "Begin", "Real", "Integer", "End" };
        List<List<string>> wordsTable;
        Dictionary<string, string> variables;
        bool clearSelection = false;
        int currentRow = 0;
        int currentWord = 0;

        bool obv = false;
        bool opred = false;

        public enum VariableErrorType : int
        {
            Correct = 0,
            FirstDigit = 1,
            FirstUnknownChar = 2,
            UnknownChar = 3,
            ReserverWord = 4
        }

        public enum NumericErrorType : int
        {
            Correct = 0,
            NotNumeric = 1,
            Overflow = 2,
            DoubleNumer = 3//THIS
        }

        public Form1()
        {
            InitializeComponent();
            BNF__textBox.Text = File.ReadAllText("TextFile1.txt");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int linesCount = textIn.Lines.Count();
            textOut.Text = string.Empty;
            textOut.ForeColor = Color.Black;
            if (linesCount == 0)
            {
                MessageBox.Show("Ошибка! На вход подан пустой текст!"); return;
            }
            else
            { CollectAndOrganizeData(); }
        }

        private void CollectAndOrganizeData()
        {
            if (wordsTable != null) wordsTable.Clear();
            wordsTable = new List<List<string>>();
            clearSelection = false;
            currentRow = 0;
            currentWord = 0;
            obv = opred = false;
            if (variables != null) variables.Clear();
            variables = new Dictionary<string, string>();
            foreach (var line in textIn.Lines)
            {
                string code = line.Replace(";", " ; ").Replace(":", " : ").Replace("=", " = ").Replace("\r\n", " ").Replace("*", " * ").Replace("/", " / ").Replace("-", " - ").Replace("+", " + ").Replace("(", " ( ").Replace(")", " ) ").Replace("^", " ^ ");
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                code = regex.Replace(code, " ");
                code = code.Trim();
                wordsTable.Add(code.Split(' ').ToList());
            }
            AnalizeLanguage();
        }

        private bool CheckFirstWord(string wordToFind)
        {
            foreach (List<string> row in wordsTable.Skip(currentRow))
            {
                foreach (string word in row.Skip(currentWord))
                {
                    currentWord++;
                    if (word != string.Empty)
                    {
                        return word.Equals(wordToFind);
                    }
                }
                currentRow++;
                currentWord = 0;
            }
            return false;
        }


        void changeLine(RichTextBox RTB, int line, string text)
        {
            int s1 = RTB.GetFirstCharIndexFromLine(line);
            int s2 = line < RTB.Lines.Count() - 1 ? RTB.GetFirstCharIndexFromLine(line + 1) - 1 : RTB.Text.Length;
            RTB.Select(s1, s2 - s1); RTB.SelectedText = text;
        }

        private void OutputError(string errorText)
        {
            textOut.Text = errorText + ", строка " + (currentRow + (currentWord > 0 ? 1 : 0));
            textOut.ForeColor = Color.Red; currentRow = currentRow - (currentWord > 0 ? 0 : 1);
            int selectionStartIndex = textIn.GetFirstCharIndexFromLine(currentRow);
            int selectionLength = 1;
            if (currentWord == 0)
            { currentWord = wordsTable[currentRow].Count - 1; }
            else { currentWord--; }
            clearSelection = true;
            if (wordsTable[currentRow][0] == string.Empty)
            {
                changeLine(textIn, currentRow, " ");
                selectionStartIndex = textIn.Find(" ", selectionStartIndex, RichTextBoxFinds.MatchCase);
            }
            else
            {
                for (int i = 0; i <= currentWord; i++)
                {
                    selectionStartIndex = textIn.Find(wordsTable[currentRow][i], selectionStartIndex + (i == 0 ? 0 : 1), RichTextBoxFinds.MatchCase);
                }
                selectionLength = wordsTable[currentRow][currentWord].Length;
            }
            textIn.Select(selectionStartIndex, selectionLength);
            textIn.SelectionColor = System.Drawing.Color.Black;
            textIn.SelectionBackColor = System.Drawing.Color.Red;
        }

        private void PrintData()
        {
            textOut.Text = "Полученные результаты вычислений";
            foreach (var key in variables.Keys)
            {
                textOut.Text += Environment.NewLine + key + " = " + variables[key];
            }
            textOut.ForeColor = Color.Black;
        }

        //+++
        private bool AnalizeAnnounces()
        {
            if (CheckFirstWord("Real"))
            {
                foreach (List<string> row in wordsTable.Skip(currentRow))
                {
                    foreach (string word in row.Skip(currentWord))
                    {
                        currentWord++;
                        if (word != string.Empty)
                        {
                            VariableErrorType varError = IsVariable(word);
                            if (varError == VariableErrorType.FirstDigit)
                            {
                                OutputError("Ошибка, имена переменных должны начинаться с буквы, дана цифра");
                                return false;
                            }
                            else if (varError == VariableErrorType.FirstUnknownChar)
                            {
                                OutputError("Ошибка, недопустимый символ при перечеслении переменных");
                                return false;
                            }
                            else if (varError == VariableErrorType.UnknownChar)
                            {
                                OutputError("Ошибка, недопустимый символ в имени переменной");
                                return false;
                            }
                            else if (varError == VariableErrorType.ReserverWord)
                            {
                                OutputError("Ошибка, переменные не могут быть заданы зарезервированными словами:\r\n\"Begin\"" + "\r\n\"Real\"\r\n\"Integer\"\r\n\"End\"");
                                return false;
                            }
                        }
                        else
                        {
                            OutputError("Ошибка, после \"Real\" ожидалась перем или набор перем");
                            return false;
                        }
                    }

                    currentRow++;
                    currentWord = 0;
                    obv = true;
                    return true;
                }
                return true;
            }
            else //"Integer"
            {
                foreach (List<string> row in wordsTable.Skip(currentRow))
                {
                    foreach (string word in row.Skip(currentWord))
                    {
                        currentWord++;
                        if (word != string.Empty)
                        {
                            NumericErrorType numericError = IsDouble(word);
                            if (numericError == NumericErrorType.DoubleNumer)
                            {
                                OutputError("Ошибка, могут быть заданы только целые числа, использовано вещественное число");
                                return false;
                            }
                            else if (numericError == NumericErrorType.NotNumeric)
                            {
                                OutputError("Ошибка, после \"Метки\" ожидалось целое или набор целых");
                                return false;
                            }
                            else if (numericError == NumericErrorType.Overflow)
                            {
                                OutputError("Возникла ошибка в процессе вычислений. Полученные вычисления превысели Int64");
                                return false;
                            }
                        }
                        else
                        {
                            OutputError("Ошибка, после \"Integer\" ожидалось целое или набор целых");
                            return false;
                        }
                    }

                    currentRow++;
                    currentWord = 0;
                    obv = true;
                    return true;
                }
                return true;
            }
        }

        //
        private bool AnalizeAnnounceOperator()
        {
            foreach (List<string> row in wordsTable.Skip(currentRow))
            {
                while (wordsTable[currentRow][currentWord] == string.Empty)
                {
                    currentRow++;
                }
                if (!obv)
                {
                    if (!(wordsTable[currentRow][currentWord] == "Real") && !(wordsTable[currentRow][currentWord] == "Integer"))
                    {
                        currentWord++;
                        OutputError("Ошибка, после \"Begin\" дожны быть \"Real\" или \"Integer\" ");
                        return false;
                    }
                    else
                    {
                        if (!AnalizeAnnounces()) { return false; }
                    }
                }
                if (!opred)
                {
                    while (wordsTable[currentRow][currentWord] == string.Empty)
                    {
                        currentRow++;
                    }
                    if ((wordsTable[currentRow][currentWord] == "Real") || (wordsTable[currentRow][currentWord] == "Integer"))
                    {
                        obv = false;
                        if (!AnalizeAnnounceOperator()) { return false; }
                    }
                    else
                    {
                        if (!AnalizeOperators()) { return false; }
                        else
                        {
                            return true;
                        }
                    }
                    return true;
                }
                if (!obv || !opred)
                {
                    AnalizeAnnounceOperator();
                }
                return true;
            }
            return true;
        }

        //+++
        private bool AnalizeOperators()
        {
            bool isEndNotFinded = false;
            while (isEndNotFinded = !CheckFirstWord("End"))
            {
                if (currentWord > 0) { currentWord--; }
                if (!AnalizeOperator()) { return false; }
                opred = true;
                if (currentRow == wordsTable.Count && (currentWord == wordsTable[currentRow - 1].Count || currentWord == 0)) { break; }
            }
            if (!isEndNotFinded && currentWord > 0) { currentWord--; }
            return true;
        }

        private bool AnalizeOperator()
        {
            bool firstWord = false;
            bool doubleDotsSeen = false; //двойные точки
            bool readRightPart = false;
            bool isEmptyAfterDoubleDots = true;
            foreach (List<string> row in wordsTable.Skip(currentRow))
            {
                foreach (string word in row.Skip(currentWord))
                {
                    if (word != string.Empty)
                    {
                        if (!firstWord)
                        {
                            if (IsDouble(word) == NumericErrorType.Correct)
                            {
                                firstWord = true;
                                currentWord++;
                            }
                            else if (word == ":")
                            {
                                currentWord++;
                                OutputError("Ошибка, пропущена метка перед знаком \":\"");
                                return false;
                            }
                            else if (IsVariable(word) == VariableErrorType.Correct)
                            {
                                readRightPart = true;
                                break;
                            }
                        }
                        else if (word == ":")
                        {
                            if (!doubleDotsSeen)
                            {
                                doubleDotsSeen = true;
                                readRightPart = true;
                                currentWord++;
                                break;
                            }
                            else
                            {
                                OutputError("Ошибка, два знака \":\" подряд");
                                return false;
                            }
                        }
                        else if (IsDouble(word) == NumericErrorType.Correct)
                        {
                            currentWord++;
                            OutputError("Ошибка, встречена вторая метка");
                            return false;
                        }
                        else if (word == "=")
                        {
                            currentWord = 0;
                            firstWord = false;
                            readRightPart = true;
                            break;
                        }
                        else if (IsVariable(word) == VariableErrorType.Correct)
                        {
                            readRightPart = true;
                            if (firstWord)
                            break;
                        }
                    }
                }
                if (readRightPart) { break; }
                currentRow++;
                currentWord = 0;
            }
            if (firstWord && !doubleDotsSeen)
            {
                OutputError("Ошибка, \":\" после меток");
                return false;
            }

            foreach (List<string> row in wordsTable.Skip(currentRow))
            {
                foreach (string word in row.Skip(currentWord))
                {
                    currentWord++;
                    if (word != string.Empty)
                    {
                        isEmptyAfterDoubleDots = false;
                        VariableErrorType errorType = IsVariable(word);
                        if (errorType == VariableErrorType.Correct)
                        {
                            if (!CheckFirstWord("="))
                            {
                                OutputError("Ошибка, после переменной пропущен знак \"=\"");
                                return false;
                            }
                            string mathResult = CheckAndExecuteMath();
                            if (mathResult != "err")
                            {
                                double value = double.Parse(mathResult);
                                if (variables.ContainsKey(word))
                                {
                                    variables[word] = Convert.ToInt64(value).ToString();
                                }
                                else
                                {
                                    variables.Add(word, Convert.ToInt64(value).ToString());
                                }
                                return true;
                            }
                            else { return false; }
                        }
                        else if (errorType == VariableErrorType.ReserverWord)
                        {
                            OutputError("Ошибка, переменные не могут быть заданы зарезервированными словами:\r\n\"Begin\"" + "\r\n\"Real\"\r\n\"Integer\"\r\n\"End\"");
                            return false;
                        }
                        else if (errorType == VariableErrorType.UnknownChar)
                        {
                            OutputError("Ошибка, недопустимый символ в имени переменной");
                            return false;
                        }
                        else if (IsDouble(word) == NumericErrorType.Correct)
                        {
                            OutputError("Ошибка, только переменным могут быть присвоены значения, дано число");
                            return false;
                        }
                        else if (errorType == VariableErrorType.FirstDigit)
                        {
                            OutputError("Ошибка, имена переменных должны начинаться с буквы, дана цифра");
                            return false;
                        }
                        else if (errorType == VariableErrorType.FirstUnknownChar)
                        {
                            OutputError("Ошибка, математическое выражение содержит недопустимый символ");
                            return false;
                        }
                    }
                }
                currentRow++;
                currentWord = 0;
            }
            if (isEmptyAfterDoubleDots)
            {
                OutputError("Ошибка, после знака ожидалось математическое выражение");
                return false;
            }
            return false;
        }

        //+++
        private void AnalizeLanguage()
        {
            if (!CheckFirstWord("Begin"))
            {
                OutputError("Ошибка, программа должна начинаться со слова \"Begin\"");
                return;
            }
            else
            {
                currentRow = currentRow + 1;
                currentWord = 0;
                while (wordsTable[currentRow][currentWord] == string.Empty)
                {
                    currentRow++;
                }
                if (!AnalizeAnnounceOperator()) { return; }

            }
            if (CheckFirstWord("End"))
            {
                foreach (List<string> row in wordsTable.Skip(currentRow))
                {
                    foreach (string word in row.Skip(currentWord))
                    {
                        currentWord++;
                        if (word != string.Empty)
                        {
                            OutputError("Ошибка, после слова \"End\" есть текст");
                            return;
                        }
                    }
                    currentRow++;
                    currentWord = 0;
                }
                PrintData();
                return;
            }
            else
            {
                OutputError("Ошибка, программа должна завершаться словом \"End\"");
                return;
            }
        }

        //+++
        private NumericErrorType IsDouble(string value)
        {
            if (int.TryParse(value, NumberStyles.Any, new CultureInfo("en-US"), out _))
            {
                return NumericErrorType.Correct;
            }
            else
            {
                try
                {
                    if (double.TryParse(value, NumberStyles.Any, new CultureInfo("en-US"), out _))
                    {
                        return NumericErrorType.DoubleNumer;
                    }
                    double val = double.Parse(value, new CultureInfo("en-US"));
                }
                catch (System.OverflowException)
                {
                    return NumericErrorType.Overflow;
                }
                catch (System.FormatException)
                {
                    return NumericErrorType.NotNumeric;
                }
                return NumericErrorType.NotNumeric;
            }
        }

        //+++
        private VariableErrorType IsVariable(string value)
        {
            char a = value.ToUpper()[0];
            if (a >= '0' && a <= '9')
            {
                return VariableErrorType.FirstDigit;
            }
            else if (!((a >= 'A' && a <= 'Z') || (a >= '0' && a <= '9') ))
            { return VariableErrorType.FirstUnknownChar; }
            foreach (char c in value.ToUpper())
            {
                if (!((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') ))
                { return VariableErrorType.UnknownChar; }
            }
            if (reservedWords.Contains(value))
            { return VariableErrorType.ReserverWord; }
            return VariableErrorType.Correct;
        }

        //+++
        private void textIn_MouseDown(object sender, MouseEventArgs e)
        {
            if (clearSelection)
            {
                textIn.SelectAll();
                textIn.SelectionColor = System.Drawing.Color.Black;
                textIn.SelectionBackColor = System.Drawing.Color.White;
                textIn.DeselectAll();
                clearSelection = false;
            }
        }

        //+++
        private string CheckAndExecuteMath()
        {
            List<string> words = new List<string>();
            int openBrackets = 0; //открыть скобки
            bool canCompute = false;  //может вычислить
            bool prevPlus = false;//
            bool prevMin = false;//
            bool prevMult = false;// *!/
            bool prevPov = false;// степенная функция
            bool prevDig = false;// число
            bool prevPerem = false;//переменная
            bool prevCloseBracket = false;//предыдущая закрывающая скобка
            bool prevOpenBracket = false;//предыдущая Открытая скобка
            foreach (List<string> row in wordsTable.Skip(currentRow))
            {
                foreach (string word in row.Skip(currentWord))
                {
                    currentWord++;
                    if (word != string.Empty)
                    {
                        if (word == "End")
                        {
                            currentWord--;
                            canCompute = true;
                            break;
                        }
                        else if (word == "-")
                        {
                            if (prevMin || prevPlus || prevMult || prevPov)
                            {
                                OutputError("Ошибка, два знака математического действия подряд");
                                return "err";
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = true;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = false;
                                prevDig = false;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                            }
                        }
                        else if (word == "+")
                        {
                            if (!prevDig && !prevCloseBracket)
                            {
                                if (prevOpenBracket)
                                {
                                    OutputError("Ошибка, знак математического действия \"+\" после открывающейся скобки");
                                    return "err";
                                }
                                else if (prevMin || prevPlus || prevMult || prevPov)
                                {
                                    OutputError("Ошибка, два знака математического действия подряд");
                                    return "err";
                                }
                                else if (!prevDig)
                                {
                                    OutputError("Ошибка, знак математического действия \"+\" не может быть в начале выражения");
                                    return "err";
                                }
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = true;
                                prevMult = false;
                                prevPov = false;
                                prevDig = false;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                            }
                        }
                        else if (word == "*" || word == "/")
                        {
                            if (!prevDig && !prevCloseBracket)
                            {
                                if (prevOpenBracket)
                                {
                                    OutputError("Ошибка, знак математического действия \"" + word + "\" после открывающейся скобки");
                                    return "err";
                                }
                                else if (prevMin || prevPlus || prevMult || prevPov)
                                {
                                    OutputError("Ошибка, два знака математического действия подряд");
                                    return "err";
                                }
                                else if (!prevDig)
                                {
                                    OutputError("Ошибка, знак математического действия \"" + word + "\" в начале выражения правой части");
                                    return "err";
                                }
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = true;
                                prevPov = false;
                                prevDig = false;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                            }
                        }
                        else if (word == "^")
                        {
                            if (!prevDig && !prevCloseBracket)
                            {
                                if (prevOpenBracket)
                                {
                                    OutputError("Ошибка, знак математического действия \"" + word + "\" после открывающейся скобки");
                                    return "err";
                                }
                                else if (prevMin || prevPlus || prevMult || prevPov)
                                {
                                    OutputError("Ошибка, два знака математического действия подряд");
                                    return "err";
                                }
                                else if (!prevDig )
                                {
                                    OutputError("Ошибка, знак математического действия \"" + word + "\" в начале выражения правой части");
                                    return "err";
                                }
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = true;
                                prevDig = false;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                            }
                        }
                        else if (word == "(")
                        {
                            openBrackets++;
                            if (prevCloseBracket)
                            {
                                OutputError("Ошибка, между скобками пропущен знак действия");
                                return "err";
                            }
                            else if (prevDig)
                            {
                                OutputError("Ошибка, между скобкой и числом пропущен знак действия");
                                return "err";
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = false;
                                prevDig = false;
                                prevCloseBracket = false;
                                prevOpenBracket = true;
                            }
                        }
                        else if (word == ")")
                        {
                            openBrackets--;
                            if (openBrackets < 0)
                            {
                                OutputError("Ошибка, лишняя закрывающая скобка");
                                return "err";
                            }
                            if (!prevCloseBracket && !prevDig)
                            {
                                if (prevOpenBracket)
                                {
                                    OutputError("Ошибка, пустые скобки");
                                }
                                else
                                {
                                    OutputError("Ошибка, после математического действия пропущено число");
                                }
                                return "err";
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = false;
                                prevDig = false;
                                prevCloseBracket = true;
                                prevOpenBracket = false;
                            }
                        }
                        else if (IsDouble(word) == NumericErrorType.Correct)
                        {
                            if (prevDig || prevCloseBracket)
                            {
                                currentWord--;
                                canCompute = true;
                                prevDig = true;
                                break;
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = false;
                                prevDig = true;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                            }
                        }
                        else if (IsVariable(word) == VariableErrorType.Correct)
                        {
                            if (prevDig )
                            {
                                OutputError("Ошибка, число и переменная подряд");
                                return "err";
                            }
                            else if (prevCloseBracket)
                            {
                                OutputError("Ошибка, пропущено действие между переменной и скобкой");
                                return "err";
                            }
                            else if (prevPerem)
                            {
                                OutputError("Ошибка, двe переменных подряд");
                                return "err";
                            }
                            else if (variables.ContainsKey(word))
                            {
                                words.Add(variables[word]);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = false;
                                prevDig = true;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                            }
                            else
                            {
                                OutputError("Ошибка, обращение к неинициализированной переменной");
                                return "err";
                            }
                        }
                        else if (IsVariable(word) == VariableErrorType.FirstDigit)
                        {
                            OutputError("Ошибка, имена переменных должны начинаться с буквы, дана цифра");
                            return "err";
                        }
                        else if (IsVariable(word) == VariableErrorType.UnknownChar)
                        {
                            OutputError("Ошибка, недопустимый символ в имени переменной");
                            return "err";
                        }
                        else if (IsVariable(word) == VariableErrorType.ReserverWord)
                        {
                            if (prevDig)
                            { OutputError("Ошибка, два числа подряд"); }
                            else
                            {
                                OutputError("Ошибка, переменные не могут быть заданы зарезервированными словами:\r\n\"Begin\"" + "\r\n\"Real\"\r\n\"Integer\"\r\n\"End\"");
                            }
                            return "err";
                        }
                        else
                        {
                            OutputError("Ошибка, выражение содержит недопустимый символ");
                            return "err";
                        }
                    }
                }
                if (canCompute) { break; }
                else { currentRow++; currentWord = 0; }
            }
            if (openBrackets > 0 && prevDig)
            {
                if (prevCloseBracket)
                {
                    OutputError("Ошибка, между числом и скобкой пропущен знак действия");
                }
                else { OutputError("Ошибка, два числа подряд"); }
                return "err";
            }
            else if (openBrackets > 0)
            {
                OutputError("Ошибка, не все скобки закрыты");
                return "err";
            }
            else if (prevMin || prevPlus || prevPov || prevMult)
            {
                OutputError("Ошибка, после знака действия должны идти скобка \"(\", переменная или целое");
                return "err";
            }
            else if (words.Count < 1)
            {
                OutputError("Ошибка, после знака \"=\" ожидалось выражение");
                return "err";
            }
            return ComputeMath(ComputeBrackets(string.Join(" ", words.ToArray())));
        }

        private string ComputeMath(string math)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            math = regex.Replace(math, " ").Replace(",", ".");
            math = Regex.Replace(math, @"\d+(\.\d+)?", m => { var x = m.ToString(); return x.Contains(".") ? x : string.Format("{0}.0", x); });
            math = ComputePov(math).Replace(",", ".");
            string result = "err";
            try
            {
                result = String.Format("{0:F20}", Convert.ToDouble(new DataTable().Compute(math, "")));
            }
            catch (System.OverflowException)
            {
                OutputError("Возникла ошибка в процессе вычислений. Полученные вычисления превысели Int64");
            }
            catch (System.DivideByZeroException)
            {
                OutputError("Возникла ошибка в процессе вычислений. Деление на ноль");
            }
            catch (System.Data.EvaluateException)
            {
                OutputError("Возникла ошибка в процессе вычислений. Полученные вычисления превысели Int64");
            }
            return result;
        }

        private string ComputeBrackets(string math)
        {
            while (math.Contains("("))
            {
                string beforeOpen = math.Substring(0, math.IndexOf("("));
                string afterOpen = math.Substring(math.IndexOf("(") + 1);
                if (afterOpen.IndexOf("(") < afterOpen.IndexOf(")"))
                {
                    afterOpen = ComputeBrackets(afterOpen);
                    string inBrackets = afterOpen.Substring(0, afterOpen.IndexOf(")"));
                    afterOpen = afterOpen.Substring(afterOpen.IndexOf(")") + 1);
                    inBrackets = ComputeMath(inBrackets);
                    math = beforeOpen + inBrackets + afterOpen;
                }
                else
                {
                    string inBrackets = afterOpen.Substring(0, afterOpen.IndexOf(")"));
                    afterOpen = afterOpen.Substring(afterOpen.IndexOf(")") + 1);
                    inBrackets = ComputeMath(inBrackets);
                    math = beforeOpen + inBrackets + afterOpen;
                }
            }
            return math;
        }

        private string ComputePov(string math)
        {
            if (math.Contains("^"))
            {
                string[] wordsForPov = math.Split(' ');
                for (int j = wordsForPov.Length - 1; j > 0; j--)
                {
                    wordsForPov[j] = wordsForPov[j].Trim();
                    if (wordsForPov[j] == "^")
                    {
                        if (j == 0 || (j + 1) == wordsForPov.Length)
                        { continue; }
                        else if (IsDouble(wordsForPov[j - 1]) == NumericErrorType.Correct && IsDouble(wordsForPov[j + 1]) == NumericErrorType.Correct)
                        {
                            var answer = Math.Pow(double.Parse(wordsForPov[j - 1], new CultureInfo("en-US")), double.Parse(wordsForPov[j + 1], new CultureInfo("en-US")));
                            wordsForPov[j - 1] = String.Format("{0:F20}", answer).Replace(",", ".");
                            wordsForPov[j + 1] = wordsForPov[j] = "";
                        }
                    }
                }
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                math = regex.Replace(string.Join(" ", wordsForPov), " ");
            }
            return math;
        }

    }
}
