using static System.Console;
using static System.ConsoleKey;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Saper_new
{
    internal class Game
    {
        Field field;
        int row, col;  //размер поля, класс Game
        int colMark, rowMark;  //координаты маркера на поле

        // убрать магические цифры
        readonly int ShiftRow = 3; //смещение начала строк поля для размещения шапки
        readonly int ShiftCol = 2; //смещение начала столбцов поля 
        readonly string CellWindow = " [ ] ";//видимое окно ячейки

        private static bool over;
        //признак конца игры и метод
        public static void EndGame() => over = false;

        public Game()
        {
            Clear();
            StartGame(); // запуск стартового меню, создание поля с минами
            Mutex consoleMutex = new Mutex();
            int timerCoordX = 0; // координаты таймера
            int timerCoordY = 1; //(вторая строка)

            ForegroundColor = System.ConsoleColor.Red;
            int timeOfGame = 0;  //таймер 
            ResetColor();

            Thread thread = new Thread(() => // создаем второй поток, ему передаем лямбда-выражение (метод), который необходимо выполнять

            {
                while (true)
                {

                    consoleMutex.WaitOne(); // этот метод приостанавливает текущий поток до тех пор,
                                            // пока в другом потоке не вызовется метод ReleaseMutex.
                                            // после того как второй поток отпустит mutex, мы его занимаем

                   CursorTop = timerCoordY; // устанавливаем курсор в координаты таймера
                   CursorLeft = timerCoordX;
                   timeOfGame++;            // отсчет таймера
                   WriteLine();
                    ForegroundColor = System.ConsoleColor.Green;
                    WriteLine("Timer:  " + timeOfGame);
                    ResetColor();
                    SetCursorPosition(colMark * CellWindow.Length + ShiftCol, rowMark + ShiftRow);
                    consoleMutex.ReleaseMutex(); // освобождаем mutex
                    Thread.Sleep(1000); // останавливаем текущий поток на секунду
                }
            });
            thread.Start();

            while (true)
            {
                consoleMutex.WaitOne(); // перед работой с консолью ждем освобождения mutex'a, и занимаем его
                                        // CursorTop = startCoordY; // устанавливаем курсор 
                                        // CursorLeft = startCoordX;

                consoleMutex.ReleaseMutex(); // освобождаем mutex

                while (over)
                {
                    var enter = ReadKey(true).Key;//отслеживаем действия игрока
                    switch (enter)
                    {
                        case Z:
                            if (colMark - 1 == -1) break;
                            colMark--;//перемещение влево
                            break;

                        case X:
                            if (rowMark + 1 == row) break;
                            rowMark++;//перемещение вниз
                            break;

                        case C:
                            if (colMark + 1 == col) break;
                            colMark++;//перемещение вправо
                            break;

                        case S:
                            if (rowMark - 1 == -1) break;
                            rowMark--;//перемещение вверх
                            break;

                        case F://ставим метку флага
                            field.FlagCell(rowMark, colMark);
                            break;

                        case Enter://открываем ячейку
                            field.OpenCell(rowMark, colMark);
                            break;

                        default://предупреждаем о неправильном вводе
                            SetCursorPosition(0, 1); //во второй строке
                            ForegroundColor = System.ConsoleColor.Red;
                            Write("Wrong input   ");
                            ResetColor();
                            break;
                    }
                    if (field.IsWin()) break;//если все ячейки открыты (победа) - 
                    if (!over) continue;
                    if (timeOfGame == 0)
                    {
                        over = false;

                        break;
                    }
                    SetCursorPosition(colMark * CellWindow.Length + ShiftCol, rowMark + ShiftRow);
                }
                SetCursorPosition(0, row + ShiftRow);
                ForegroundColor = ConsoleColor.Red;
                WriteLine('\n' + (over ? "YOU won!" : "You Luse!)))") + "\nPush 'Y' if you want play once");
                while (ReadKey(true).Key != Y) { }
                ResetColor();
                
                Thread.Sleep(1000); //останавка потока на секунду
            }

        }
        // запуск стартового меню, создание поля с минами
        private void StartGame()
        {          
            WriteLine("Game MineSwepper! You choose levelyourself. If you want to play, PUSH quantity of rows,columns,bombs ");
            WriteLine(" Numbers must be not less 3 and no more 20. For Example 3,3,3");
            ForegroundColor = System.ConsoleColor.Green;
            ResetColor();
            over = true;

            while (true)
            {
                WriteLine("Enter numbers of rows, columns, bombs, please  ");
                var str = ReadLine().Trim().Split(", ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);//вносим размер поля и кол-во мин
                if (str.Length < 3) continue;    //проверяем, что внесено и распознано не менее 3-х значений
                if (int.TryParse(str[0], out var rowResult) && int.TryParse(str[1], out var colResult) &&
                    int.TryParse(str[2], out var bombResult))  //проверяем удачно ли преобразуем введеное в массив
                {
                    if (rowResult < 3 || rowResult > 20) // проверяем, не вышли ли за заданные рамки
                        WriteLine("Rows can't be less then 3 and more then 20.");
                    else if (colResult < 3 || colResult > 20) // проверяем, не вышли ли за заданные рамки
                        WriteLine("Cols can't be less then 3 and more then 20.");
                    else if (bombResult < 1 || bombResult > rowResult * colResult)
                        WriteLine("Many bombs for our small placed");
                          
                    else
                    {
                        row  = rowResult;
                        col  = colResult;
                        Clear();
                        Write("Use: Enter - to Open,  Z, X, C, S - for moving, F - for flag");
                        field = new Field(rowResult, colResult, bombResult);  //создаем поле
                        colMark = col / 2;    //устанавливаем маркер в середину поля
                        rowMark = row  / 2;
                        SetCursorPosition(colMark * CellWindow.Length + ShiftCol, rowMark + ShiftRow);
                        return;
                    }
                    continue;
                }
                ForegroundColor = System.ConsoleColor.Red;
                WriteLine("Error, Try once more");  //если есть ошибки ввода параметров - перезапуск ввода
                ResetColor();
            }
        }
    }
}