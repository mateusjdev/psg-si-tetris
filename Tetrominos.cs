using System;

namespace atp_tp_tetris
{
    internal class Tetrominos
    {
        int[,] peca;

        public int[,] Peca
        {
            get { return peca; }
        }

        public Tetrominos(char formato)
        {
            switch (formato)
            {
                case 'I':
                    peca = new int[,] {
                        { 0,0,0,0 },
                        { 1,1,1,1 },
                        { 0,0,0,0 },
                        { 0,0,0,0 }
                    };
                    break;
                case 'J':
                    peca = new int[,] {
                        { 1,0,0 },
                        { 1,1,1 },
                        { 0,0,0 }
                    };
                    break;
                case 'L':
                    peca = new int[,] {
                        { 0,0,1 },
                        { 1,1,1 },
                        { 0,0,0 },
                    }; break;
                case 'O':
                    peca = new int[,] {
                        { 1,1 },
                        { 1,1 }
                    }; break;
                case 'S':
                    peca = new int[,] {
                        { 0,1,1 },
                        { 1,1,0 },
                        { 0,0,0 },
                    }; break;
                case 'T':
                    peca = new int[,] {
                        { 0,1,0 },
                        { 1,1,1 },
                        { 0,0,0 }
                    }; break;
                case 'Z':
                    peca = new int[,] {
                        { 1,1,0 },
                        { 0,1,1 },
                        { 0,0,0 }
                    }; break;
                default:
                    peca = new int[,] { { 0 } };
                    break;
            }
        }

        public static char EscolherFormatoAleatorio()
        {
            char[] pecas_disponiveis = { 'T', 'O', 'L', 'J', 'S', 'Z', 'I' };
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
    }
}
