using System;
using System.IO;
using System.Text;

namespace atp_tp_tetris
{
    internal class Jogador
    {
        const int PONTUACAO_INICIAL = 0;
        const string SALVAR_CAMINHO_PADRAO = "./scores.txt";

        public string Nome { get; set; }
        public int Pontuacao { get; set; }

        public Jogador(string nome, int pontuacao = PONTUACAO_INICIAL)
        {
            Nome = nome;
            Pontuacao = pontuacao;
        }

        public void SalvarPontuacao(string caminhoArquivo = SALVAR_CAMINHO_PADRAO)
        {
            try
            {
                StreamWriter sw = new StreamWriter(caminhoArquivo, true, Encoding.UTF8);
                sw.WriteLine($"{Nome};{Pontuacao}");
                sw.Close();
                Console.WriteLine("Pontuação salva!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar no arquivo: {ex.Message} ");
                UI.AperteTeclaEnter();
            }
        }
    }
}
