using System;
using System.Threading;

namespace atp_tp_tetris
{
    internal class Tetris
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
            Console.Write("\u001b[H");
            for (int i = 0; i < display.GetLength(0); i++)
            {
                Console.Write(DIV);
                for (int j = 0; j < display.GetLength(1); j++)
                {
                    if (display[i, j] > 0)
                    {
                        switch(display[i, j])
                        {
                            case Tetrominos.CYAN:
                                Console.Write("\u001b[46;36mX\u001b[0m");
                                break;
                            case Tetrominos.BLUE:
                                Console.Write("\u001b[44;34mX\u001b[0m");
                                break;
                            case Tetrominos.ORANGE:
                                Console.Write("\u001b[47;37mX\u001b[0m");
                                break;
                            case Tetrominos.YELLOW:
                                Console.Write("\u001b[43;33mX\u001b[0m");
                                break;
                            case Tetrominos.GREEN:
                                Console.Write("\u001b[42;32mX\u001b[0m");
                                break;
                            case Tetrominos.PURPLE:
                                Console.Write("\u001b[45;35mX\u001b[0m");
                                break;
                            case Tetrominos.RED:
                                Console.Write("\u001b[41;31mX\u001b[0m");
                                break;
                            default:
                                Console.Write("\u001b[0mX");
                                break;

                        }
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
                        if (tabuleiro[k, pos_peca_col] > 0 && peca.Peca[j, i] > 0)
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
            int linhaFinal = tabuleiro.GetLength(0) - 1;
            for (int i = linhaFinal; i >= 0; i--)
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
                    i = linhaFinal;
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
                            _tabuleiro[k, pos_peca_col] = peca.Peca[j, i];
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
            if (resposta == "S")
            {
                jogador.SalvarPontuacao();
            }
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
                            pecaCaindo = false;
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
}
