using System;
using System.Text;

namespace atp_tp_tetris
{
    internal abstract class UI
    {
        // https://en.wikipedia.org/wiki/ANSI_escape_code#Colors
        public enum Cores
        {
            PRETO,
            VERMELHO,
            VERDE,
            AMARELO,
            AZUL,
            ROXO,
            CIANO,
            BRANCO,
            PADRAO = 9,
            IGNORAR = -1
        }

        public class EscapeKeys
        {
            public const string EspaceCode = "\x1b";
            public const string ResetAttributes = "\x1b[0m";
            public const string MoveCursorToHome = "\x1b[H";
            public const string EraseEntireScreen = "\x1b[2J";
            public const string EraseEntireLine = "\x1b[2K";
            public static string MoveCursorToColumn(int column)
            {
                return $"\x1b[{column}G";
            }
            public static string MoveCursorTo(int line, int column)
            {
                return $"\x1b[{line};{column}H";
            }
            public static string ForegroundColor(Cores cor)
            {
                string texto;
                switch (cor)
                {
                    case Cores.PRETO:
                        texto = $"{EspaceCode}[30m";
                        break;
                    case Cores.VERMELHO:
                        texto = $"{EspaceCode}[31m";
                        break;
                    case Cores.VERDE:
                        texto = $"{EspaceCode}[32m";
                        break;
                    case Cores.AMARELO:
                        texto = $"{EspaceCode}[33m";
                        break;
                    case Cores.AZUL:
                        texto = $"{EspaceCode}[34m";
                        break;
                    case Cores.ROXO:
                        texto = $"{EspaceCode}[35m";
                        break;
                    case Cores.CIANO:
                        texto = $"{EspaceCode}[36m";
                        break;
                    case Cores.BRANCO:
                        texto = $"{EspaceCode}[37m";
                        break;
                    case Cores.PADRAO:
                        texto = $"{EspaceCode}[39m";
                        break;
                    default:
                        texto = "";
                        break;
                }
                return texto;
            }
            public static string BackgroundColor(Cores cor)
            {
                string texto;
                switch (cor)
                {
                    case Cores.PRETO:
                        texto = $"{EspaceCode}[40m";
                        break;
                    case Cores.VERMELHO:
                        texto = $"{EspaceCode}[41m";
                        break;
                    case Cores.VERDE:
                        texto = $"{EspaceCode}[42m";
                        break;
                    case Cores.AMARELO:
                        texto = $"{EspaceCode}[43m";
                        break;
                    case Cores.AZUL:
                        texto = $"{EspaceCode}[44m";
                        break;
                    case Cores.ROXO:
                        texto = $"{EspaceCode}[45m";
                        break;
                    case Cores.CIANO:
                        texto = $"{EspaceCode}[46m";
                        break;
                    case Cores.BRANCO:
                        texto = $"{EspaceCode}[47m";
                        break;
                    case Cores.PADRAO:
                        texto = $"{EspaceCode}[49m";
                        break;
                    default:
                        texto = "";
                        break;
                }
                return texto;
            }
        }

        public static void AlterarCorTexto(Cores texto = Cores.IGNORAR, Cores fundo = Cores.IGNORAR)
        {
            if (texto == Cores.IGNORAR && fundo == Cores.IGNORAR) return;
            Console.WriteLine(EscapeKeys.ForegroundColor(texto) + EscapeKeys.BackgroundColor(fundo));
        }

        public static void ResetarCorTexto()
        {
            Console.Write(EscapeKeys.ResetAttributes);
        }

        public static string TextColorido(string texto, Cores corTexto = Cores.IGNORAR, Cores corFundo = Cores.IGNORAR)
        {
            var str = new StringBuilder();
            str.Append(EscapeKeys.ForegroundColor(corTexto));
            str.Append(EscapeKeys.BackgroundColor(corFundo));
            for (int i = 0; i < texto.Length; i++)
            {
                str.Append(texto[i]);
            }
            str.Append(EscapeKeys.ForegroundColor(Cores.PADRAO));
            str.Append(EscapeKeys.BackgroundColor(Cores.PADRAO));
            return str.ToString();
        }

        public static void WriteColorido(string texto, Cores corTexto = Cores.IGNORAR, Cores corFundo = Cores.IGNORAR)
        {
            Console.Write(TextColorido(texto, corTexto, corFundo));
        }

        public static void WriteLineColorido(string texto, Cores corTexto = Cores.IGNORAR, Cores corFundo = Cores.IGNORAR)
        {
            Console.WriteLine(TextColorido(texto, corTexto, corFundo));
        }

        public static void LimparTela()
        {
            Console.Write(EscapeKeys.MoveCursorToHome + EscapeKeys.EraseEntireScreen);
        }

        public static void RetornarInicioLinha()
        {
            Console.Write(EscapeKeys.EraseEntireLine + EscapeKeys.MoveCursorToColumn(1));
        }

        public static void AlterarCoordenadasTela(int linha, int coluna)
        {
            Console.Write(EscapeKeys.MoveCursorTo(linha, coluna));
        }

        public static int Selecao(string pergunta, string[] opcoes)
        {
            if (opcoes.Length <= 0)
            {
                return -1;
            }
            int selecionado = 0;
            int numSelecoes = opcoes.Length;
            ConsoleKey key;
            do
            {
                Console.WriteLine(pergunta);
                for (int i = 0; i < numSelecoes; i++)
                {
                    if (i == selecionado)
                    {
                        WriteLineColorido(opcoes[i], Cores.PADRAO, Cores.AZUL);
                    }
                    else
                    {
                        Console.WriteLine(opcoes[i]);
                    }
                }
                key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.Tab:
                        selecionado++;
                        break;
                    case ConsoleKey.LeftArrow:
                        selecionado--;
                        break;
                    case ConsoleKey.RightArrow:
                        selecionado++;
                        break;
                    case ConsoleKey.UpArrow:
                        selecionado--;
                        break;
                    case ConsoleKey.DownArrow:
                        selecionado++;
                        break;
                }
                if (selecionado >= numSelecoes)
                    selecionado = 0;
                else if (selecionado < 0)
                    selecionado = numSelecoes - 1;
                // Remover
                LimparTela();
            } while (key != ConsoleKey.Enter);
            return selecionado;
        }

        public static void AperteTeclaEnter(string mensagem = "Pressione enter para continuar!")
        {
            Console.WriteLine(mensagem);
            ConsoleKey key;
            do
            {
                RetornarInicioLinha();
                key = Console.ReadKey().Key;
            } while (key != ConsoleKey.Enter);
        }

        public static void AperteTeclaQualquer(string mensagem = "Pressione qualquer tecla para iniciar!")
        {
            Console.WriteLine(mensagem);
            Console.ReadKey();
        }

        public static string LerInformacao(string mensagem)
        {
            string ret;
            do
            {
                Console.WriteLine(mensagem);
                ret = Console.ReadLine();
                if (ret.Length <= 0)
                {
                    LimparTela();
                    Console.WriteLine("Valor inválido!");
                }
            } while (ret.Length <= 0);
            return ret;
        }
    }
}