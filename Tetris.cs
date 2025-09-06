using System;
using System.Diagnostics;
using System.Threading;

namespace atp_tp_tetris
{
    internal class Tetris
    {
        private const double GAME_SPEED = 1;

        private const double MS = 1000;

        private const double FRAMES_PER_SECOND = 10;
        private const double ENGINE_UPDATE_S = 1 / GAME_SPEED / FRAMES_PER_SECOND;
        // FRAME_TIME
        private const long ENGINE_UPDATE_MS = (long)(ENGINE_UPDATE_S * MS);

        private const int LINES = 20;
        private const int COLUMNS = 10;

        // Posicao no vetor em que novas pecas se iniciam:
        // A Comparacao se inicia antes do inicio do vetor do tabuleiro
        private const int POS_VERTICAL_INICIAL = -1;
        private const char DIV = '|';

        private int[,] tabuleiro = new int[LINES, COLUMNS];
        private int[,] display = new int[LINES, COLUMNS];

        private Jogador jogador;

        private bool jogando = false;

        Tetrominos peca = null;
        int pecaPosVertical = -1;
        int pecaPosHorizontal = -1;
        bool pecaEstaCaindo = false;

        private bool needRender = true;
        private long lastRender = 0;
        private long nextRender = 0;
        private Stopwatch watch;
        private long renderDelta = 0;

        private int DEFAULT_COOLDOWN = (int)Math.Round(1.0 / ENGINE_UPDATE_S, MidpointRounding.ToPositiveInfinity);

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

        public void MostrarTabuleiro(bool forceRender = false)
        {
            if (!needRender && !forceRender) return;

            Console.Write("\u001b[H");
            for (int i = 0; i < display.GetLength(0); i++)
            {
                Console.Write(DIV);
                for (int j = 0; j < display.GetLength(1); j++)
                {
                    if (display[i, j] > 0)
                    {
                        switch (display[i, j])
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
            needRender = false;
        }

        private Colisao VerificarColisao(int posPecaLinha, int posPecaColuna, Tetrominos peca)
        {
            int hitboxTop = peca.HitboxVerticalInicio();
            int hitboxBottom = peca.HitboxVerticalFim();
            int hitboxLeft = peca.HitboxHorizontalInicio();
            int hitboxRight = peca.HitboxHorizontalFim();

            if (posPecaLinha < -1 || posPecaLinha + hitboxBottom >= LINES)
            {
                return Colisao.VERTICAL;
            }

            if (posPecaColuna + hitboxLeft < 0 || posPecaColuna + hitboxRight >= COLUMNS)
            {
                return Colisao.HORIZONTAL;
            }

            posPecaColuna += hitboxLeft;
            for (int i = hitboxLeft; i <= hitboxRight; i++, posPecaColuna++)
            {
                int k = posPecaLinha;
                for (int j = 0; j <= hitboxBottom; j++, k++)
                {
                    if (k >= 0)
                    {
                        if (tabuleiro[k, posPecaColuna] > 0 && peca.Peca[j, i] > 0)
                        {
                            return Colisao.VERTICAL;
                        }
                    }
                }
            }

            return Colisao.NULA;
        }

        private void RemoverLinha(int linha)
        {
            needRender = true;
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

        private void InserirPeca(int posPecaLinha, int posPecaColuna, int[,] _tabuleiro, Tetrominos peca)
        {
            if (posPecaLinha + peca.HitboxVerticalInicio() < -1 || posPecaLinha + peca.HitboxVerticalFim() >= _tabuleiro.GetLength(0))
                throw new Exception("posPecaLinha out of bounds!");

            if (posPecaColuna + peca.HitboxHorizontalInicio() < 0 || posPecaColuna + peca.HitboxHorizontalFim() >= _tabuleiro.GetLength(1))
                throw new Exception("posPecaColuna out of bounds!");

            int yPeca = 0;
            int yEnd = posPecaLinha + peca.HitboxVerticalFim();
            if (posPecaLinha < 0)
            {
                yPeca = 0 - posPecaLinha;
                posPecaLinha += yPeca;
            }
            int xPecaInicial = 0;
            int xEnd = posPecaColuna + peca.HitboxHorizontalFim();
            if (posPecaColuna < 0)
            {
                xPecaInicial = 0 - posPecaColuna;
                posPecaColuna += xPecaInicial;
            }            
            int xPeca, xVetor;
            for (int yVetor = posPecaLinha; yVetor <= yEnd; yVetor++, yPeca++)
            {
                xPeca = xPecaInicial;
                for (xVetor = posPecaColuna; xVetor <= xEnd; xVetor++, xPeca++)
                {
                    if (peca.Peca[yPeca, xPeca] > 0)
                        _tabuleiro[yVetor, xVetor] = peca.Peca[yPeca, xPeca];
                }
            }
        }

        private static void CopiarTabuleiro(int[,] source, int[,] destination)
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

        private static void Pause()
        {
            // TODO: Criar menu
            // C -> Continuar()
            // R -> Reiniciar()
            // S/Q -> Sair()
            Console.Write("\x1b[H\x1b[2J");
            Console.WriteLine("Jogo Pausado!");
            Console.WriteLine("Pressione enter para continuar!");
            Console.ReadLine();
        }

        private void TentarRotacionarPeca(SentidoRotacao sentido)
        {
            switch (sentido)
            {
                case SentidoRotacao.HORARIO_90:
                    peca.Rotacionar90Horario();
                    if (VerificarColisao(pecaPosVertical, pecaPosHorizontal, peca) != Colisao.NULA)
                    {
                        peca.Rotacionar90AntiHorario();
                        return;
                    }
                    break;
                case SentidoRotacao.ANTI_HORARIO_90:
                    peca.Rotacionar90AntiHorario();
                    if (VerificarColisao(pecaPosVertical, pecaPosHorizontal, peca) != Colisao.NULA)
                    {
                        peca.Rotacionar90Horario();
                        return;
                    }
                    break;
            }
            needRender = true;
        }

        private void TentarMoverPeca(DirecaoMovimentacao direcao)
        {
            switch (direcao)
            {
                case DirecaoMovimentacao.ESQUERDA:
                    if (VerificarColisao(pecaPosVertical, pecaPosHorizontal - 1, peca) != Colisao.HORIZONTAL)
                    {
                        pecaPosHorizontal--;
                        needRender = true;
                    }
                    break;
                case DirecaoMovimentacao.DIREITA:
                    if (VerificarColisao(pecaPosVertical, pecaPosHorizontal + 1, peca) != Colisao.HORIZONTAL)
                    {
                        pecaPosHorizontal++;
                        needRender = true;
                    }
                    break;
            }
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

            jogando = true;
            IniciarLogica();
        }

        private static int CalcularPosHorizontalInicial(int tamanhoTabuleiro, int tamanhoPeca)
        {
            return (tamanhoTabuleiro - tamanhoPeca) / 2;
        }

        private void IniciarLogica()
        {
            int cooldown = -1;

            watch = Stopwatch.StartNew();
            int sleepTime = 0;
            while (jogando)
            {
                VerificarAndRemoverLinhas();

                if (!pecaEstaCaindo)
                {
                    peca = new Tetrominos();
                    pecaPosVertical = POS_VERTICAL_INICIAL;
                    pecaPosHorizontal = CalcularPosHorizontalInicial(tabuleiro.GetLength(1), peca.Peca.GetLength(0));
                    pecaEstaCaindo = true;
                    cooldown = DEFAULT_COOLDOWN + 1;
                    needRender = true;
                }

                if (VerificarColisao(pecaPosVertical, pecaPosHorizontal, peca) == Colisao.VERTICAL)
                {
                    if (pecaPosVertical == POS_VERTICAL_INICIAL)
                    {
                        jogando = false;
                    }
                    else
                    {
                        InserirPeca(pecaPosVertical - 1, pecaPosHorizontal, tabuleiro, peca);
                        pecaEstaCaindo = false;
                        // CONFIGURAR COMO TRUE DEPOIS
                        needRender = false;
                    }
                }
                else
                {
                    if (cooldown <= 0)
                    {
                        pecaPosVertical++;
                        cooldown = DEFAULT_COOLDOWN + 1;
                        needRender = true;
                    }
                    else
                    {
                        cooldown--;
                    }

                    // Boilerplate bracket to reduce git diff surface, will be removed in the next commit
                    {
                        {
                            if (Console.KeyAvailable)
                            {
                                switch (Console.ReadKey().Key)
                                {
                                    case ConsoleKey.LeftArrow:
                                        TentarRotacionarPeca(SentidoRotacao.ANTI_HORARIO_90);
                                        break;
                                    case ConsoleKey.RightArrow:
                                        TentarRotacionarPeca(SentidoRotacao.HORARIO_90);
                                        break;
                                    case ConsoleKey.A:
                                        TentarMoverPeca(DirecaoMovimentacao.ESQUERDA);
                                        break;
                                    case ConsoleKey.D:
                                        TentarMoverPeca(DirecaoMovimentacao.DIREITA);
                                        break;
                                    case ConsoleKey.DownArrow:
                                        // TODO: Reset frame
                                        // TentarMoverPeca(DirecaoMovimentacao.BAIXO);
                                        if (VerificarColisao(pecaPosVertical + 1, pecaPosHorizontal, peca) == Colisao.NULA)
                                        {
                                            pecaPosVertical++;
                                            cooldown = DEFAULT_COOLDOWN;
                                            needRender = true;
                                        }
                                        break;
                                    case ConsoleKey.Spacebar:
                                        // TODO: Reset frame
                                        // TentarMoverPeca(DirecaoMovimentacao.FUNDO);
                                        bool colidiu = false;
                                        for (int a = 0; !colidiu; a++)
                                        {
                                            if (VerificarColisao(pecaPosVertical + a, pecaPosHorizontal, peca) != Colisao.NULA)
                                            {
                                                colidiu = true;
                                                InserirPeca(pecaPosVertical + a - 1, pecaPosHorizontal, tabuleiro, peca);
                                                pecaEstaCaindo = false;
                                            }
                                        }
                                        needRender = true;
                                        break;
                                    case ConsoleKey.Escape:
                                        Pause();
                                        needRender = true;
                                        cooldown = DEFAULT_COOLDOWN;
                                        break;
                                }
                            }
                        }
                    }

                    // Render
                    CopiarTabuleiro(tabuleiro, display);
                    InserirPeca(pecaPosVertical, pecaPosHorizontal, display, peca);

                    MostrarTabuleiro();
                    lastRender = watch.ElapsedMilliseconds;

                    sleepTime = (int)(nextRender - lastRender);
                    nextRender = lastRender + ENGINE_UPDATE_MS;

                    if (sleepTime > 0)
                        Thread.Sleep(sleepTime);
                    else
                        renderDelta = sleepTime;
                }
            }

            MostrarTabuleiro(true);
            FimDeJogo();
        }

        private enum Colisao
        {
            NULA, HORIZONTAL, VERTICAL
        }

        private enum SentidoRotacao
        {
            HORARIO_90, ANTI_HORARIO_90
        }

        private enum DirecaoMovimentacao
        {
            ESQUERDA, DIREITA, BAIXO, FUNDO
        }
    }
}
