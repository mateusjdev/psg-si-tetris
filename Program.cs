using System;
using System.Threading;

namespace atp_tp_tetris
{
    class TetrisHelper
    {
        public static void MostrarMatriz(int[,] _tabuleiro)
        {
            for (int i = 0; i < _tabuleiro.GetLength(0); i++)
            {
                for (int j = 0; j < _tabuleiro.GetLength(1); j++)
                {
                    Console.Write(_tabuleiro[i, j]);
                }
                Console.WriteLine();
            }
        }

        public static void WaitForInput(bool print = false)
        {
            if (print)
            {
                Console.WriteLine("Pressione qualquer tecla!");
            }
            Console.ReadLine();
        }

        public static void MatrizCopy(int[,] source, int[,] destination)
        {
            if (source.GetLength(0) != destination.GetLength(0)) return;
            if (source.GetLength(1) != destination.GetLength(1)) return;

            for (int i = 0; i < source.GetLength(0); i++)
            {
                for (int j = 0; j < source.GetLength(1); j++)
                {
                    destination[i, j] = source[i, j];
                }
            }
        }
    }

    class Tetris
    {
        private const int LINES = 20;
        private const int COLUMNS = 10;

        private const char LIN_DIV = '-';
        private const char COL_DIV = '|';

        private int[,] tabuleiro = new int[LINES, COLUMNS];
        private int[,] display = new int[LINES, COLUMNS];

        private Random random = new Random();

        private void ZerarTabuleiro()
        {
            for (int i = 0; i < tabuleiro.GetLength(0); i++)
            {
                for (int j = 0; j < tabuleiro.GetLength(1); j++)
                {
                    tabuleiro[i, j] = display[i, j] = 0;
                }
            }
        }

        public void MostrarTabuleiro(bool espaco)
        {
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n");
            // return;
            /*
            Console.Write(LIN_DIV);
            for (int i = 0; i < tabuleiro.GetLength(1); i++)
            {
                if (i != tabuleiro.GetLength(1))
                {
                    Console.Write($"{LIN_DIV}{LIN_DIV}");
                }
            }
            Console.WriteLine();
            */
            for (int i = 0; i < display.GetLength(0); i++)
            {
                Console.Write(COL_DIV);
                for (int j = 0; j < display.GetLength(1); j++)
                {
                    if (display[i, j] > 0)
                    {
                        Console.Write("X");
                    }
                    else
                    {
                        Console.Write(" ");
                    }

                    if (j != display.GetLength(1))
                    {
                        Console.Write(COL_DIV);
                    }
                }
                /*
                Console.WriteLine();
                for (int k = 0; k < tabuleiro.GetLength(1); k++)
                {
                    if (k != tabuleiro.GetLength(1))
                    {
                        Console.Write($"{LIN_DIV}{LIN_DIV}");
                    }
                }*/
                Console.WriteLine();
            }
        }

        private int[,] CriarPeca()
        {
            const int MIN_VALUE = 1;
            const int MAX_VALUE = 6;
            int piece = random.Next(MIN_VALUE, MAX_VALUE);
            switch (piece)
            {
                // I = XXXX
                case 1:
                    return new int[,] {
                        { 0,0,0,0 },
                        { 1,1,1,1 },
                        { 0,0,0,0 },
                        { 0,0,0,0 }
                    };
                // J =  X
                //      X
                //     XX
                case 2:
                    return new int[,] {
                        { 0,1,0 },
                        { 0,1,0 },
                        { 1,1,0 }
                    };
                // L = X
                //     X
                //     XX
                case 3:
                    return new int[,] {
                        { 1,0,0 },
                        { 1,0,0 },
                        { 1,1,0 },
                    };
                // O = XX
                //     XX
                case 4:
                    return new int[,] {
                        { 0,1,1,0 },
                        { 0,1,1,0 }
                    };
                // S =  XX
                //     XX
                case 5:
                    return new int[,] {
                        { 0,1,1 },
                        { 1,1,0 },
                        { 0,0,0 },
                    };
                // T = XXX
                //      X
                case 6:
                    return new int[,] {
                        { 1,1,1 },
                        { 0,1,0 },
                        { 0,0,0 }
                    };
                // Z = XX
                //      XX
                case 7:
                    return new int[,] {
                        { 1,1,0 },
                        { 0,1,1 },
                        { 0,0,0 }
                    };
                default:
                    return new int[,] { { 0 } };
            }
        }

        private void RotacionarParaDireita(int[,] peca)
        {
            if (peca.GetLength(0) != peca.GetLength(1)) return;

            int[,] _peca = new int[peca.GetLength(0), peca.GetLength(1)];

            for (int i = 0; i < peca.GetLength(0); i++)
            {
                for (int j = 0; j < peca.GetLength(1); j++)
                {
                    _peca[i, j] = peca[j, peca.GetLength(0) - 1 - i];
                }
            }
            TetrisHelper.MatrizCopy(_peca, peca);
        }

        private void RotacionarParaEsquerda(int[,] peca)
        {
            if (peca.GetLength(0) != peca.GetLength(1)) return;

            int[,] _peca = new int[peca.GetLength(0), peca.GetLength(1)];

            for (int i = 0; i < peca.GetLength(0); i++)
            {
                for (int j = 0; j < peca.GetLength(1); j++)
                {
                    _peca[i, j] = peca[peca.GetLength(0) - 1 - j, i];
                }
            }
            TetrisHelper.MatrizCopy(_peca, peca);
        }

        private bool VerificarColisao(int pos_peca_lin, int pos_peca_col, int[,] piece)
        {
            // Se a boarda do topo da peça esta fora da matriz retorna true
            if (pos_peca_lin < -1) return true;
            if (pos_peca_lin > LINES) return true;
            if (pos_peca_col > COLUMNS) return true;

            for (int i = 0; i < piece.GetLength(0); i++, pos_peca_lin++)
            {
                if (pos_peca_lin >= 0)
                {
                    for (int j = 0; j < piece.GetLength(1); j++)
                    {
                        bool is_peca_line_empty = true;
                        for (int k = 0; k < piece.GetLength(1) && is_peca_line_empty; k++)
                        {
                            if (piece[i, j] != 0) is_peca_line_empty = false;
                        }

                        // pos_peca_lin + piece.GetLength(0);

                        if (!is_peca_line_empty)
                        {
                            // Console.WriteLine(pos_peca_lin);
                            // Console.WriteLine(pos_peca_col);
                            if (pos_peca_lin >= tabuleiro.GetLength(0)) return true;
                            if (tabuleiro[pos_peca_lin, pos_peca_col + j] + piece[i, j] > 1)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void InserirPeca(int pos_peca_lin, int pos_peca_col, int[,] piece, int[,] _tabuleiro)
        {
            for (int i = 0; i < piece.GetLength(0); i++, pos_peca_lin++)
            {
                if (pos_peca_lin >= 0)
                {
                    for (int j = 0; j < piece.GetLength(1); j++)
                    {
                        // if(pos_peca_lin >= 20) continue;
                        // Console.WriteLine("X:" + pos_peca_lin);
                        if (piece[i, j] > 0) _tabuleiro[pos_peca_lin, pos_peca_col + j] = piece[i, j];
                    }
                }
            }
        }

        // static void RotacionarEsquerda()
        // static void RotacionarDireita()
        // static void Abaixar()
        // static void Cair()

        // VerificarLinhas -> Verifica se há uma linha com tudo 0 
        // VerificarLinhas -> Mover linhas para baixo (se tem uma linha com 1 e abaixo com 0, descer linha)

        private void ResetarDisplay()
        {
            for (int i = 0; i < tabuleiro.GetLength(0); i++)
            {
                for (int j = 0; j < tabuleiro.GetLength(1); j++)
                {
                    display[i, j] = tabuleiro[i, j];
                }
            }
        }

        public void Iniciar()
        {
            ZerarTabuleiro();
            const int INSERT_LIN = -1;
            const int INSERT_COL = 3;

            bool rodando = true;
            while (rodando)
            {
                var nova_peca = CriarPeca();
                int insert_linha = INSERT_LIN;
                int insert_col = INSERT_COL;

                bool rodando_peca = true;
                for (int i = 0; rodando_peca; i++)
                {
                    // Console.WriteLine($"{insert_linha}, {insert_col}");
                    bool colisao = VerificarColisao(insert_linha, insert_col, nova_peca);
                    if (colisao)
                    {
                        if (insert_linha == INSERT_LIN)
                        {
                            rodando = false;
                        }
                        // Console.WriteLine("Colisão");
                        InserirPeca(insert_linha - 1, insert_col, nova_peca, tabuleiro);
                        rodando_peca = false;
                    }
                    else
                    {
                        ResetarDisplay();
                        // Console.WriteLine("Pode continuar chefe!");
                        InserirPeca(insert_linha, insert_col, nova_peca, display);
                        insert_linha++;
                    }
                    MostrarTabuleiro(true);

                    // 10 FPS
                    for (int j = 0; j < 10; j++)
                    {
                        Thread.Sleep(10);
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey();
                            switch (key.Key)
                            {
                                case ConsoleKey.LeftArrow:
                                    Console.WriteLine("fn RotacionarEsquerda");
                                    RotacionarParaEsquerda(nova_peca);
                                    break;
                                case ConsoleKey.RightArrow:
                                    Console.WriteLine("fn RotacionarDireita");
                                    RotacionarParaDireita(nova_peca);
                                    break;
                                case ConsoleKey.DownArrow:
                                    Console.WriteLine("fn MoverBaixo");
                                    break;
                                case ConsoleKey.Spacebar:
                                    Console.WriteLine("fn Cair");
                                    break;
                                case ConsoleKey.C:
                                    Console.WriteLine("fn GuardarPeca");
                                    break;
                                case ConsoleKey.Escape:
                                    Console.WriteLine("Jogo Pausado!");
                                    Console.WriteLine("Pressione enter para continuar!");
                                    Console.ReadLine();
                                    break;
                            }

                            ResetarDisplay();
                            InserirPeca(insert_linha, insert_col, nova_peca, display);
                            MostrarTabuleiro(true);
                        }
                    }
                }
            }
        }
    }

    internal class Program
    {
        static void Main()
        {
            Tetris jogo = new Tetris();
            jogo.Iniciar();
            TetrisHelper.WaitForInput();
        }
    }
}
