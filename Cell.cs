using static System.Console;

namespace Saper_new
{
    public enum CellInfo { Close, Open, Flag }      

    internal class Cell
    {    
        public CellInfo Info  { get; private set; }//признак состояния
        public bool Bomb { get;  }             //признак наличия бомбы в ячейке
        public int Bombs { get; private set; }//кол-во бомб рядом с ячейкой

        public Cell(bool bomb = false)
        {                      //ячейка создается закрытой и, по умолчанию, без бомбы
            Info = CellInfo.Close;
            Bomb = bomb;
        }
        //показывает кол-во бомб и меняет признак
        public void ShowCell(int bombs)//показывает кол-во бомб и меняет признак
        {
            ForegroundColor = System.ConsoleColor.Green;
            Write($" {bombs} ");  //пишет кол-во бомб из параметра
            ResetColor();
            Info = CellInfo.Open; //меняет признак ячейки на открыто
            Bombs = bombs;        //записывает в поле ячейки кол-во бомб
        }

        public void SetFlag()
        {   //если не было флага - ставим
            Info = Info != CellInfo.Flag ? CellInfo.Flag : CellInfo.Close;
            if (Info == CellInfo.Close)                   //если был - убираем
            {
                Write(' ');

                return;
            }
            ForegroundColor = System.ConsoleColor.Red;
            Write('F');       //рисуем в ячейке F
            ResetColor();

        }


    }
}