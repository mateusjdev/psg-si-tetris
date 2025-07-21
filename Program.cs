using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace atp_tp_tetris
{
    class Tetrominos
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

    class Jogador
    {
        private string nome;
        private int pontuacao;

        public string Nome
        {
            get { return nome; }
            set { nome = value; }
        }
        public int Pontuacao
        {
            get { return pontuacao; }
            set { pontuacao = value; }
        }

        public Jogador(string nome, int pontuacao)
        {
            this.nome = nome;
            this.pontuacao = pontuacao;
        }

        public void SalvarPontuacao(string caminhoArquivo)
        {
            try
            {
                StreamWriter sw = new StreamWriter(caminhoArquivo, true, Encoding.UTF8);
                sw.WriteLine($"{nome};{pontuacao}");
                sw.Close();
                Console.WriteLine("Pontuação salva!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar no arquivo: {ex.Message} ");
            }
        }
    }

    class Tetris
    {
        private const int FRAMES_PER_SECOND = 10;
        private const int MILLI_SECONDS = 100;
        private const int FRAME_TIME = MILLI_SECONDS / FRAMES_PER_SECOND;

        private const int LINES = 20;
        private const int COLUMNS = 10;

        private const int SEM_COLISAO = 0;
        private const int COLISAO_VERTICAL = 1;
        private const int COLISAO_HORIZONTAL = 2;

        private const int LINHA_INICIAL = -1;
        private const char DIV = '|';

        private int[,] tabuleiro = new int[LINES, COLUMNS];
        private int[,] display = new int[LINES, COLUMNS];

        private Jogador jogador;

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

        public void MostrarTabuleiro()
        {
            Console.WriteLine("\x1b[H");
            for (int i = 0; i < display.GetLength(0); i++)
            {
                Console.Write(DIV);
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
                        Console.Write(DIV);
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine($"Pontuação: {jogador.Pontuacao}");
        }

        private int VerificarColisao(int pos_peca_lin, int pos_peca_col, Tetrominos peca)
        {
            int hitbox_top = peca.HitboxVerticalInicio();
            int hitbox_bottom = peca.HitboxVerticalFim();
            int hitbox_left = peca.HitboxHorizontalInicio();
            int hitbox_right = peca.HitboxHorizontalFim();

            if (pos_peca_lin < -1 || pos_peca_lin + hitbox_bottom >= LINES)
            {
                return COLISAO_VERTICAL;
            }

            if (pos_peca_col + hitbox_left < 0 || pos_peca_col + hitbox_right >= COLUMNS)
            {
                return COLISAO_HORIZONTAL;
            }

            pos_peca_col += hitbox_left;
            for (int i = hitbox_left; i <= hitbox_right; i++, pos_peca_col++)
            {
                int k = pos_peca_lin;
                for (int j = 0; j <= hitbox_bottom; j++, k++)
                {
                    if (k >= 0)
                    {
                        if (tabuleiro[k, pos_peca_col] + peca.Peca[j, i] > 1)
                        {
                            return COLISAO_VERTICAL;
                        }
                    }
                }
            }

            return SEM_COLISAO;
        }

        private void RemoverLinha(int linha)
        {
            if (linha < 0 || linha > tabuleiro.GetLength(0) - 1) return;
            for (int j = linha; j > 0; j--)
            {
                for (int i = 0; i < tabuleiro.GetLength(1); i++)
                {
                    tabuleiro[j, i] = tabuleiro[j - 1, i];
                }
            }
            for (int i = 0; i < tabuleiro.GetLength(1); i++)
            {
                tabuleiro[0, i] = 0;
            }
        }

        private void VerificarAndRemoverLinhas()
        {
            int linhasRemovidas = 0;
            for (int i = tabuleiro.GetLength(0) - 1; i >= 0; i--)
            {
                bool completa = true;
                for (int j = 0; j < tabuleiro.GetLength(1) && completa; j++)
                {
                    if (tabuleiro[i, j] <= 0)
                    {
                        completa = false;
                    }
                }
                if (completa)
                {
                    linhasRemovidas++;
                    RemoverLinha(i);
                    i = 0;
                }
            }

            if (linhasRemovidas >= 1)
            {
                jogador.Pontuacao += 300;
                linhasRemovidas--;
                jogador.Pontuacao += linhasRemovidas * 100;
            }
        }

        private void InserirPeca(int pos_peca_lin, int pos_peca_col, int[,] _tabuleiro, Tetrominos peca)
        {
            pos_peca_col += peca.HitboxHorizontalInicio();
            for (int i = peca.HitboxHorizontalInicio(); i <= peca.HitboxHorizontalFim(); i++, pos_peca_col++)
            {
                int k = pos_peca_lin + peca.HitboxVerticalInicio();
                for (int j = peca.HitboxVerticalInicio(); j <= peca.HitboxVerticalFim(); j++, k++)
                {
                    if (k >= 0)
                    {
                        if (peca.Peca[j, i] > 0)
                            _tabuleiro[k, pos_peca_col] = 1;
                    }
                }
            }
        }



        private void ResetarMatrizDisplay()
        {
            for (int i = 0; i < tabuleiro.GetLength(0); i++)
            {
                for (int j = 0; j < tabuleiro.GetLength(1); j++)
                {
                    display[i, j] = tabuleiro[i, j];
                }
            }
        }
        private void FimDeJogo()
        {
            Console.WriteLine("Fim de jogo!");
            string resposta = "";
            do
            {
                Console.WriteLine("Deseja salvar a pontuação? (S/N)");
                resposta = Console.ReadLine();
                if (resposta != "S" && resposta != "N")
                {
                    Console.WriteLine("Resposta Inválida! Digite 'S' ou 'N'");
                }
            } while (resposta != "S" && resposta != "N");
            jogador.SalvarPontuacao("scores.txt");
        }

        private void ImprimirControles()
        {
            Console.WriteLine("Controles:");
            Console.WriteLine("Seta para Esquerda -> Virar peça no sentido Anti-Horário");
            Console.WriteLine("Seta para Direita -> Virar peça no sentido Horário");
            Console.WriteLine("Letra A -> Mover peça para esquerda");
            Console.WriteLine("Letra D -> Mover peça para direita");
            Console.WriteLine("Espaço -> Colocar peça na posição final (Cair até o final)");
            Console.WriteLine("Esc -> Pause");
        }

        public void Iniciar()
        {
            string nome = "";
            do
            {
                Console.WriteLine("Informe o nome do jogador: ");
                nome = Console.ReadLine();
            } while (nome.Length <= 0);

            ImprimirControles();
            Console.WriteLine("Pressione qualquer tecla para iniciar!");
            Console.ReadKey();

            ZerarTabuleiro();
            jogador = new Jogador(nome, 0);

            IniciarLogica();
        }

        private void IniciarLogica()
        {
            bool jogando = true;
            while (jogando)
            {
                Tetrominos nova_peca = new Tetrominos(Tetrominos.EscolherFormatoAleatorio());
                int posLinha = LINHA_INICIAL;
                int posColuna = (tabuleiro.GetLength(1) - nova_peca.Peca.GetLength(0)) / 2;
                bool pecaCaindo = true;
                for (int i = 0; pecaCaindo; i++)
                {
                    VerificarAndRemoverLinhas();
                    MostrarTabuleiro();
                    if (VerificarColisao(posLinha, posColuna, nova_peca) == COLISAO_VERTICAL)
                    {
                        if (posLinha == LINHA_INICIAL)
                        {
                            jogando = false;
                            MostrarTabuleiro();
                            FimDeJogo();
                        }
                        else
                        {
                            InserirPeca(posLinha - 1, posColuna, tabuleiro, nova_peca);
                            pecaCaindo = false;
                        }
                    }
                    else
                    {
                        ResetarMatrizDisplay();
                        InserirPeca(posLinha, posColuna, display, nova_peca);
                        posLinha++;

                        MostrarTabuleiro();
                        for (int j = 0; j < FRAME_TIME; j++)
                        {
                            Thread.Sleep(FRAME_TIME);
                            if (Console.KeyAvailable)
                            {
                                var key = Console.ReadKey();
                                switch (key.Key)
                                {
                                    case ConsoleKey.LeftArrow:
                                        nova_peca.Rotacionar90AntiHorario();
                                        if (VerificarColisao(posLinha, posColuna, nova_peca) != SEM_COLISAO)
                                        {
                                            nova_peca.Rotacionar90Horario();
                                        }
                                        break;
                                    case ConsoleKey.RightArrow:
                                        nova_peca.Rotacionar90Horario();
                                        if (VerificarColisao(posLinha, posColuna, nova_peca) != SEM_COLISAO)
                                        {
                                            nova_peca.Rotacionar90AntiHorario();
                                        }
                                        break;
                                    case ConsoleKey.A:
                                        if (VerificarColisao(posLinha, posColuna - 1, nova_peca) != COLISAO_HORIZONTAL)
                                        {
                                            posColuna--;
                                        }
                                        break;
                                    case ConsoleKey.D:
                                        if (VerificarColisao(posLinha, posColuna + 1, nova_peca) != COLISAO_HORIZONTAL)
                                        {
                                            posColuna++;
                                        }
                                        break;
                                    case ConsoleKey.DownArrow:
                                        if (VerificarColisao(posLinha + 1, posColuna, nova_peca) != SEM_COLISAO)
                                        {
                                            posLinha++;
                                            j = FRAMES_PER_SECOND;
                                        }
                                        break;
                                    case ConsoleKey.Spacebar:
                                        bool colidiu = false;
                                        for (int a = 0; !colidiu; a++)
                                        {
                                            if (VerificarColisao(posLinha + a, posColuna, nova_peca) != SEM_COLISAO)
                                            {
                                                colidiu = true;
                                                InserirPeca(posLinha + a - 1, posColuna, tabuleiro, nova_peca);
                                                pecaCaindo = false;
                                                j = FRAMES_PER_SECOND;
                                            }
                                        }
                                        break;
                                    case ConsoleKey.Escape:
                                        Console.WriteLine("Jogo Pausado!");
                                        Console.WriteLine("Pressione enter para continuar!");
                                        Console.ReadLine();
                                        break;
                                }
                                ResetarMatrizDisplay();
                                InserirPeca(posLinha, posColuna, display, nova_peca);
                            }                            
                            MostrarTabuleiro();
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

            Console.WriteLine("Pressione qualquer tecla para continuar!");
            Console.ReadLine();
        }
    }
}
