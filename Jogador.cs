using System;
using System.IO;
using System.Text;

namespace atp_tp_tetris
{
    internal class Jogador
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

        public void SalvarPontuacao(string caminhoArquivo = "scores.txt")
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
}
