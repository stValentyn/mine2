using static System.Console;
using static Saper_new.CellInfo;

namespace Saper_new
{
    internal class Field
    {
        readonly int row, col, mine; //кол-во строк, столбцов, мин
        readonly Cell[,] _cell;// двумерный массив ячеек поля

        // убрать магические цифры
        readonly int ShiftRow = 3; //смещение начала строк поля для размещения шапки
        readonly int ShiftCol = 2; //смещение начала столбцов поля 
        readonly string CellWindow = " [ ] ";//видимое окно ячейки

        public Field(int rows, int cols, int mines)
        {
            row = rows;
            col = cols;
            mine = mines;
            _cell = new Cell[row, col];// создаем массив для записи ячеек поля
            SetCursorPosition(0, 3);
            var placed = 0;  //счетчик заложенных бомб
            for (int r = 0; r < row ; r++)
            {
                for (int c = 0; c < col ; c++)//перебираем массив и 
                {
                    _cell[r, c] = new Cell();                     //создаем объект ячейки
                    Write(CellWindow);//обозначаем ячейку и
                }
                WriteLine();//отступаем строки
               // WriteLine();
            }

            var rnd = new System.Random();
            while (placed < mines)  //распологаем бомбы
            {
                var _row = rnd.Next() % row ;
                var _col = rnd.Next() % col ;
                if (_cell[_row, _col].Bomb) continue;  //если признак бомбы true - пропускаем
                _cell[_row, _col] = new Cell(true);//создаем ячейку с бомбой
                placed++;  //отмечаем бомбу в счетчике
            }
        }
        //ставим сетку флага
        public void FlagCell(int _row, int _col)
        {
            if (_cell[_row, _col].Info == Open)//проверяем признак ячейки
            {
                SetCursorPosition(0, 1);
                Write("Placed open ");
                return;
            }
            SetCursorPosition(_col * CellWindow.Length + ShiftCol, _row + ShiftRow);
            _cell[_row, _col].SetFlag();//вызываем метод смены признака на флаг
        }
        //открывает ячейку
        public void OpenCell (int _row, int _col)
        {
            var cell = _cell[_row, _col];
            if (cell.Info == Flag )      //если стоит знак флага
            {
                SetCursorPosition(0, 1); //пишем во второй строке предупреждение
                Write("Place Flag        ");
                return;                  //и выходим
            }
            if (cell.Bomb)                                     //если заложена бомба
            {
                Game.EndGame();                              // over=false
                for (int r = 0; r < row ; r++)            //перебираем все ячейки
                {
                    for (int c = 0; c < col; c++)
                    {
                        if (_cell[r, c].Bomb)                    //если бомба -
                        {
                            ForegroundColor = System.ConsoleColor.Red;
                            SetCursorPosition(c * CellWindow.Length, r + ShiftRow);
                            Write("  X  ");               //рисуем все бомбы
                        }
                    }
                }
                ResetColor();
                return;
            }
            //else if (ни флаг, ни бомба - считаем сосед.бомбы и открываем)
            var count = NumberOfSurroundingBombs(_row, _col);
            SetCursorPosition((_col * CellWindow.Length) + 1 , _row + ShiftRow);//смещаем курсор вправо на шаг
            _cell[_row, _col].ShowCell(count);
            if (count == 0)   //если рядом нет бомб -
                OpenEmpty(_row, _col);            //- запускаем рекурсию на пустые 
        }
        //проверка на размещении в пределах игрового поля
        private bool IsInsideField(int r, int c) => r >= 0 && c >= 0 && r < row && c < col;
        //возвращает кол-во бомб рядом с ячейкой
        private int NumberOfSurroundingBombs(int _row, int _col)
        {
            var count = 0;
            //перебираем соседние ячейки
            for (int r = _row - 1; r <= _row + 1; r++)
                for (int c = _col - 1; c <= _col + 1; c++)
                    if (!IsInsideField(r, c)) { }//если соседняя внутри поля
                    else if (_cell[r, c].Bomb)//если внутри бомба - складываем
                        count++;
            return count;
        }
        //рекурсия - открываем пустые (нулевые) ячейки
        private void OpenEmpty(int _row, int _col)
        {
            for (int r = _row - 1; r <= _row + 1; r++)
                for (int c = _col - 1; c <= _col + 1; c++)//выбираем соседнюю ячейку
                    if (!IsInsideField(r, c)) { }                                //если в пределах поля
                    else if (_cell[r, c].Info == Flag || _cell[r, c].Info  == Open) { }//если флаг или уже открыта - пропускаем
                    else
                    {
                        var count = NumberOfSurroundingBombs(r, c);//считаем соседние бомбы
                        SetCursorPosition(c * CellWindow.Length + 1, _row + ShiftRow);//курсор вправо +1
                        _cell[r, c].ShowCell(count);     //показываем ячейку
                        if (count == 0) OpenEmpty(r, c); //и если пустая - запускаем рекурсию
                    }
        }
        //признак победы = true, если площадь поля минус кол-во бомб равно кол-ву открытых ячеек
        public bool IsWin()
        {
            var count = 0;
            for (int r = 0; r < row; r++)
                for (int c = 0; c < col; c++)
                    if (_cell[r, c].Info == Open) count++;
            return count == row * col - mine;
        }
    }
}