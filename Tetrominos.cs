using System;

namespace atp_tp_tetris
{
    internal class Tetrominos
    {
        // Cores
        // TODO: Mover pra enum ou struct
        public const int CYAN = 1;
        public const int BLUE = 2;
        public const int ORANGE = 3;
        public const int YELLOW = 4;
        public const int GREEN = 5;
        public const int PURPLE = 6;
        public const int RED = 7;

        int[,] peca;

        public int[,] Peca
        {
            get { return peca; }
        }

        public Tetrominos(Formato formato)
        {
            InicializarMatriz(formato);
        }

        public Tetrominos()
        {
            InicializarMatriz(EscolherFormatoAleatorio());
        }

        private void InicializarMatriz(Formato formato)
        {
            switch (formato)
            {
                case Formato.I:
                    peca = new int[,] {
                        { 0,0,0,0 },
                        { CYAN,CYAN,CYAN,CYAN},
                        { 0,0,0,0 },
                        { 0,0,0,0 }
                    };
                    break;
                case Formato.J:
                    peca = new int[,] {
                        { BLUE,0,0 },
                        { BLUE,BLUE,BLUE },
                        { 0,0,0 }
                    };
                    break;
                case Formato.L:
                    peca = new int[,] {
                        { 0,0,ORANGE },
                        { ORANGE,ORANGE,ORANGE },
                        { 0,0,0 },
                    }; break;
                case Formato.O:
                    peca = new int[,] {
                        { YELLOW,YELLOW },
                        { YELLOW,YELLOW }
                    }; break;
                case Formato.S:
                    peca = new int[,] {
                        { 0,GREEN,GREEN },
                        { GREEN,GREEN,0 },
                        { 0,0,0 },
                    }; break;
                case Formato.T:
                    peca = new int[,] {
                        { 0,PURPLE,0 },
                        { PURPLE,PURPLE,PURPLE },
                        { 0,0,0 }
                    }; break;
                case Formato.Z:
                    peca = new int[,] {
                        { RED,RED,0 },
                        { 0,RED,RED },
                        { 0,0,0 }
                    }; break;
            }
        }

        public static Formato EscolherFormatoAleatorio()
        {
            Formato[] pecas_disponiveis = {
                Formato.T,
                Formato.O,
                Formato.L,
                Formato.J,
                Formato.S,
                Formato.Z,
                Formato.I };
            int index_peca = new Random().Next(0, pecas_disponiveis.Length);
            return pecas_disponiveis[index_peca];
        }

        private static void CopiarPeca(int[,] src, int[,] dest)
        {
            if (dest.GetLength(0) != src.GetLength(0)) return;
            if (dest.GetLength(1) != src.GetLength(1)) return;

            for (int i = 0; i < dest.GetLength(0); i++)
            {
                for (int j = 0; j < dest.GetLength(1); j++)
                {
                    dest[i, j] = src[i, j];
                }
            }
        }

        public void Rotacionar90AntiHorario()
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

            CopiarPeca(_peca, peca);
        }

        public void Rotacionar90Horario()
        {
            if (peca.GetLength(0) != peca.GetLength(1)) return;

            int[,] _peca = new int[peca.GetLength(0), peca.GetLength(0)];

            for (int i = 0; i < peca.GetLength(0); i++)
            {
                for (int j = 0; j < peca.GetLength(1); j++)
                {
                    _peca[i, j] = peca[peca.GetLength(0) - 1 - j, i];
                }
            }

            CopiarPeca(_peca, peca);
        }

        public int HitboxHorizontalInicio()
        {
            for (int i = 0; i < peca.GetLength(1); i++)
            {
                for (int j = 0; j < peca.GetLength(0); j++)
                {
                    if (peca[j, i] > 0)
                        return i;
                }
            }
            return 0;
        }

        public int HitboxHorizontalFim()
        {
            for (int i = peca.GetLength(1) - 1; i >= 0; i--)
            {
                for (int j = 0; j < peca.GetLength(0); j++)
                {
                    if (peca[j, i] > 0)
                        return i;
                }
            }
            return 0;
        }

        public int HitboxVerticalInicio()
        {
            for (int i = 0; i < peca.GetLength(0); i++)
            {
                for (int j = 0; j < peca.GetLength(1); j++)
                {
                    if (peca[i, j] > 0)
                        return i;
                }
            }
            return 0;
        }

        public int HitboxVerticalFim()
        {
            for (int i = peca.GetLength(0) - 1; i >= 0; i--)
            {
                for (int j = 0; j < peca.GetLength(1); j++)
                {
                    if (peca[i, j] > 0)
                        return i;
                }
            }
            return 0;
        }

        public enum Formato
        {
            T, O, L, J, S, Z, I
        }
    }
}
