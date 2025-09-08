using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace atp_tp_tetris
{
    internal class Program
    {
        static void Main()
        {
            Tetris jogo = new Tetris();
            jogo.Iniciar();

            UI.AperteTeclaEnter("Programa finalizado! Pressione enter para sair!");
        }
    }
}
