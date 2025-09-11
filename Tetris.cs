using System;
using System.Diagnostics;
using System.Threading;

namespace atp_tp_tetris
{
    internal class Tetris
    {
        private const double VelocidadeDoJogo = 1.0;

        private const double Milissegundos = 1000.0;

        private const double QuadrosPorSegundo = 10.0;
        private const double TempoAtualizacaoLogicaSegundos = 1 / VelocidadeDoJogo / QuadrosPorSegundo;
        // FRAME_TIME
        private const long TempoAtualizacaoLogicaMillisegundos = (long)(TempoAtualizacaoLogicaSegundos * Milissegundos);

        private const int LINHAS = 20;
        private const int COLUNAS = 10;

        // Posicao no vetor em que novas pecas se iniciam:
        // A Comparacao se inicia antes do inicio do vetor do tabuleiro
        private const int POS_VERTICAL_INICIAL = -1;
        private const char DIV = '|';

        private int[,] tabuleiro = new int[LINHAS, COLUNAS];
        private int[,] display = new int[LINHAS, COLUNAS];

        private Jogador jogador;

        public Status estadoLogica;

        private Tetrominos peca = null;

        private const int TAMANHO_FILA_PECA = 5;
        private TetrominosQueue filePecas;

        int pecaPosVertical = -1;
        int pecaPosHorizontal = -1;
        bool pecaEstaCaindo = false;

        private int cooldown = -1;
        private int sleepTime = 0;

        private bool necessitaRenderTabuleiro = true;
        private bool necessitaRenderFilaDePeca = true;
        private long lastRender = 0;
        private long nextRender = 0;
        private Stopwatch watch;
        private long renderDelta = 0;

        private int DEFAULT_COOLDOWN = (int)Math.Round(1.0 / TempoAtualizacaoLogicaSegundos, MidpointRounding.ToPositiveInfinity);

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

        // TODO: Remover função
        private void ImprimirPeca(int value)
        {
            if (value > 0)
            {
                switch (value)
                {
                    case Tetrominos.CYAN:
                        UI.WriteColorido("X", UI.Cores.CIANO, UI.Cores.CIANO);
                        break;
                    case Tetrominos.BLUE:
                        UI.WriteColorido("X", UI.Cores.AZUL, UI.Cores.AZUL);
                        break;
                    case Tetrominos.ORANGE:
                        UI.WriteColorido("X", UI.Cores.BRANCO, UI.Cores.BRANCO);
                        break;
                    case Tetrominos.YELLOW:
                        UI.WriteColorido("X", UI.Cores.AMARELO, UI.Cores.AMARELO);
                        break;
                    case Tetrominos.GREEN:
                        UI.WriteColorido("X", UI.Cores.VERDE, UI.Cores.VERDE);
                        break;
                    case Tetrominos.PURPLE:
                        UI.WriteColorido("X", UI.Cores.ROXO, UI.Cores.ROXO);
                        break;
                    case Tetrominos.RED:
                        UI.WriteColorido("X", UI.Cores.VERMELHO, UI.Cores.VERMELHO);
                        break;
                    default:
                        UI.WriteColorido("X", UI.Cores.PADRAO, UI.Cores.PADRAO);
                        break;
                }
            }
            else
            {
                Console.Write(" ");
            }
        }

        private void RenderizarTabuleiro(bool forceRender = false)
        {
            if (!necessitaRenderTabuleiro && !forceRender) return;

            Console.Write(UI.EscapeKeys.MoveCursorToHome);
            for (int i = 0; i < display.GetLength(0); i++)
            {
                for (int j = 0; j < display.GetLength(1); j++)
                {
                    Console.Write(DIV);
                    ImprimirPeca(display[i, j]);
                }
                Console.WriteLine(DIV);
            }
            Console.WriteLine($"Pontuação: {jogador.Pontuacao}");
            necessitaRenderTabuleiro = false;
        }

        private void RenderizarFilaDePeca(bool forceRender = false)
        {
            if (!necessitaRenderFilaDePeca && !forceRender) return;

            int line = 1;
            const int column = 25;
            UI.AlterarCoordenadasTela(line, column);

            int j = -1, l = -1, m = -1;
            for (int i = 0; i < filePecas.Size; i++)
            {
                Tetrominos tmp = filePecas.Get(i);
                j = tmp.HitboxVerticalInicio();
                l = tmp.HitboxHorizontalInicio();
                for (int k = j; k <= tmp.HitboxVerticalFim(); k++)
                {
                    for (m = l; m <= tmp.HitboxHorizontalFim(); m++)
                    {
                        Console.Write(DIV);
                        ImprimirPeca(tmp.Peca[k, m]);
                    }
                    Console.Write(DIV);
                    // TODO: Clean screen from (x, 25) to (0, end)
                    Console.Write(new string(' ', 10 - m));
                    UI.AlterarCoordenadasTela(++line, column);
                }
                Console.Write("          ");
                UI.AlterarCoordenadasTela(++line, column);
            }
            necessitaRenderFilaDePeca = false;
        }

        private bool EstaEmColisao(int posPecaLinha, int posPecaColuna, Tetrominos peca)
        {
            int hitboxTop = peca.HitboxVerticalInicio();
            int hitboxBottom = peca.HitboxVerticalFim();
            int hitboxLeft = peca.HitboxHorizontalInicio();
            int hitboxRight = peca.HitboxHorizontalFim();

            if (posPecaLinha < -1 || posPecaLinha + hitboxBottom >= LINHAS)
            {
                return true;
            }

            if (posPecaColuna + hitboxLeft < 0 || posPecaColuna + hitboxRight >= COLUNAS)
            {
                return true;
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
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void RemoverLinha(int linha)
        {
            necessitaRenderTabuleiro = true;
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
            if (peca == null) return;

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
            int salvarPontuacao = UI.Selecao($"Pontuação: {jogador.Pontuacao}\nDeseja salvar a pontuação?", ["Sim", "Não"]);
            if (salvarPontuacao == 0)
            {
                jogador.SalvarPontuacao();
            }
        }

        private void ImprimirControles()
        {
            UI.LimparTela();
            Console.WriteLine("Controles:");
            Console.WriteLine("Seta para Esquerda -> Virar peça no sentido Anti-Horário");
            Console.WriteLine("Seta para Direita -> Virar peça no sentido Horário");
            Console.WriteLine("Letra A -> Mover peça para esquerda");
            Console.WriteLine("Letra D -> Mover peça para direita");
            Console.WriteLine("Espaço -> Colocar peça na posição final (Cair até o final)");
            Console.WriteLine("Esc -> Pause");
            UI.AperteTeclaQualquer();
        }

        private void Pause()
        {
            UI.LimparTela();
            necessitaRenderTabuleiro = true;
            necessitaRenderFilaDePeca = true;

            string pergunta = "Jogo Pausado!";
            string[] opcoes = ["Continuar", "Reiniciar", "Sair"];
            int resposta = UI.Selecao(pergunta, opcoes);
            switch (resposta)
            {
                // case 0: ignore
                case 1:
                    ReiniciarEstadoJogo();
                    ImprimirControles();
                    break;
                case 2:
                    estadoLogica = Status.Interrompido;
                    UI.LimparTela();
                    break;
            }
            UI.LimparTela();
        }

        private void TentarRotacionarPeca(SentidoRotacao sentido)
        {
            switch (sentido)
            {
                case SentidoRotacao.HORARIO_90:
                    peca.Rotacionar90Horario();
                    if (EstaEmColisao(pecaPosVertical, pecaPosHorizontal, peca))
                    {
                        peca.Rotacionar90AntiHorario();
                        return;
                    }
                    break;
                case SentidoRotacao.ANTI_HORARIO_90:
                    peca.Rotacionar90AntiHorario();
                    if (EstaEmColisao(pecaPosVertical, pecaPosHorizontal, peca))
                    {
                        peca.Rotacionar90Horario();
                        return;
                    }
                    break;
            }
            necessitaRenderTabuleiro = true;
        }

        private void TentarMoverPeca(DirecaoMovimentacao direcao)
        {
            switch (direcao)
            {
                case DirecaoMovimentacao.ESQUERDA:
                    if (!EstaEmColisao(pecaPosVertical, pecaPosHorizontal - 1, peca))
                    {
                        pecaPosHorizontal--;
                        necessitaRenderTabuleiro = true;
                    }
                    break;
                case DirecaoMovimentacao.DIREITA:
                    if (!EstaEmColisao(pecaPosVertical, pecaPosHorizontal + 1, peca))
                    {
                        pecaPosHorizontal++;
                        necessitaRenderTabuleiro = true;
                    }
                    break;
                case DirecaoMovimentacao.BAIXO:
                    if (!EstaEmColisao(pecaPosVertical + 1, pecaPosHorizontal, peca))
                    {
                        pecaPosVertical++;
                        cooldown = DEFAULT_COOLDOWN;
                        necessitaRenderTabuleiro = true;
                    }
                    break;
                case DirecaoMovimentacao.FUNDO:
                    bool colidiu = false;
                    for (int a = 0; !colidiu; a++)
                    {
                        if (EstaEmColisao(pecaPosVertical + a, pecaPosHorizontal, peca))
                        {
                            colidiu = true;
                            InserirPeca(pecaPosVertical + a - 1, pecaPosHorizontal, tabuleiro, peca);
                            pecaEstaCaindo = false;
                        }
                    }
                    necessitaRenderTabuleiro = true;
                    break;
            }
        }

        private void LerInformacoesJogador()
        {
            UI.LimparTela();
            string nomeJogador = UI.LerInformacao("Informe o nome do jogador:");
            jogador = new Jogador(nomeJogador);
        }

        public void Iniciar()
        {
            ReiniciarEstadoJogo();
            ImprimirControles();
            IniciarLogica();
        }

        private static int CalcularPosHorizontalInicial(int tamanhoTabuleiro, int tamanhoPeca)
        {
            return (tamanhoTabuleiro - tamanhoPeca) / 2;
        }

        private void ReiniciarEstadoJogo()
        {
            UI.LimparTela();
            ZerarTabuleiro();
            LerInformacoesJogador();

            estadoLogica = Status.Executando;
            cooldown = -1;
            watch = Stopwatch.StartNew();
            nextRender = 0;
            lastRender = 0;
            sleepTime = 0;
            peca = null;
            filePecas = new TetrominosQueue(TAMANHO_FILA_PECA);
            pecaPosHorizontal = -1;
            pecaPosVertical = -1;
            pecaEstaCaindo = false;

            necessitaRenderTabuleiro = true;
            necessitaRenderFilaDePeca = true;

            // RoubarQuadro() -> LimparTela + necessitaRender(2x)

            // SolicitarRender(Tela.Tabuleiro);
            // SolicitarRender(Tela.FilaDePeca);
            // SolicitarRender(Tela.PecaGuardada);
            // SolicitarRenderTabuleiro();
            // SolicitarRenderFileDePeca(); 
        }

        private void LerControlesDoUsuario()
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
                        TentarMoverPeca(DirecaoMovimentacao.BAIXO);
                        break;
                    case ConsoleKey.Spacebar:
                        TentarMoverPeca(DirecaoMovimentacao.FUNDO);
                        break;
                    case ConsoleKey.Escape:
                        Pause();
                        break;
                }
            }
        }

        private void IniciarLogica()
        {
            UI.LimparTela();
            while (estadoLogica == Status.Executando)
            {
                VerificarAndRemoverLinhas();

                if (!pecaEstaCaindo)
                {
                    peca = filePecas.Pop();
                    RenderizarFilaDePeca(true);

                    pecaPosVertical = POS_VERTICAL_INICIAL;
                    pecaPosHorizontal = CalcularPosHorizontalInicial(tabuleiro.GetLength(1), peca.Peca.GetLength(0));
                    pecaEstaCaindo = true;
                    cooldown = DEFAULT_COOLDOWN + 1;
                    necessitaRenderTabuleiro = true;

                    if (EstaEmColisao(pecaPosVertical, pecaPosHorizontal, peca))
                    {
                        estadoLogica = Status.Finalizada;
                    }
                }
                else
                {
                    if (cooldown <= 0)
                    {
                        if (EstaEmColisao(pecaPosVertical + 1, pecaPosHorizontal, peca))
                        {
                            InserirPeca(pecaPosVertical, pecaPosHorizontal, tabuleiro, peca);
                            pecaEstaCaindo = false;
                        }
                        else
                        {
                            pecaPosVertical++;
                            cooldown = DEFAULT_COOLDOWN + 1;
                        }
                        necessitaRenderTabuleiro = true;
                    }
                    else
                    {
                        cooldown--;
                    }

                    LerControlesDoUsuario();

                    // Render
                    CopiarTabuleiro(tabuleiro, display);
                    InserirPeca(pecaPosVertical, pecaPosHorizontal, display, peca);

                    RenderizarTabuleiro();
                    RenderizarFilaDePeca();
                    lastRender = watch.ElapsedMilliseconds;

                    sleepTime = (int)(nextRender - lastRender);
                    nextRender = lastRender + TempoAtualizacaoLogicaMillisegundos;

                    if (sleepTime > 0)
                        Thread.Sleep(sleepTime);
                    else
                        renderDelta = sleepTime;
                }
            }

            if (estadoLogica == Status.Finalizada)
            {
                RenderizarTabuleiro(true);
                FimDeJogo();
            }
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

    public enum Status
    {
        Pendente,
        Executando,
        Finalizada,
        Interrompido
    }
}
