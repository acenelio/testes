using System.Collections.Generic;
using Tabuleiro;

namespace Xadrez {
    class PartidaDeXadrez {

        public Tabuleiro.Tabuleiro Tab { get; private set; }
        public int Turno { get; private set; }
        public Cor JogadorAtual { get; private set; }
        public bool terminada { get; private set; }
        private HashSet<Peca> Pecas;
        private HashSet<Peca> PecasCapturadas;
        public bool Xeque { get; private set; }

        public PartidaDeXadrez() {
            Tab = new Tabuleiro.Tabuleiro(8, 8);
            Turno = 1;
            JogadorAtual = Cor.Branco;
            terminada = false;
            Xeque = false;
            Pecas = new HashSet<Peca>();
            PecasCapturadas = new HashSet<Peca>();
            colocarPecas();
        }

        public Peca executaMovimento(Posicao origem, Posicao destino) {
            Peca p = Tab.RetirarPeca(origem);
            p.IncrementarQteMovimentos();
            Peca pecaCapturada = Tab.RetirarPeca(destino);
            Tab.ColocarPecas(p, destino);
            if (pecaCapturada != null) {
                PecasCapturadas.Add(pecaCapturada);
            }
            return pecaCapturada;
        }

        public void desfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada) {
            Peca p = Tab.RetirarPeca(destino);
            p.DecrementarQteMovimentos();
            if (pecaCapturada != null) {
                Tab.ColocarPecas(pecaCapturada, destino);
                PecasCapturadas.Remove(pecaCapturada);
            }
            Tab.ColocarPecas(p, origem);
        }

        public void RealizarJogada(Posicao origem, Posicao destino) {
            Peca pecaCapturada = executaMovimento(origem, destino);

            if (estaEmXeque(JogadorAtual)) {
                desfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em Xeque!");
            }

            if (estaEmXeque(adversaria(JogadorAtual))) {
                Xeque = true;
            }
            else {
                Xeque = false;
            }

            if (testeXequemate(adversaria(JogadorAtual))) {
                terminada = true;
            }
            else {
                Turno++;
                MudaJogador();
            }
        }

        public void ValidarPosicaoDeOrigem(Posicao pos) {
            if (Tab.Peca(pos) == null) {
                throw new TabuleiroException("Não existe peça na posição de origem escolhida!");
            }
            if (JogadorAtual != Tab.Peca(pos).Cor) {
                throw new TabuleiroException("A peça de origem escolhida não é sua!");
            }
            if (!Tab.Peca(pos).ExisteMovimentosPossiveis()) {
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida!");
            }
        }

        public void ValidarPosicaoDeDestino(Posicao origem, Posicao destino) {
            if (!Tab.Peca(origem).MovimentoPossivel(destino)) {
                throw new TabuleiroException("Posição de destino inválida!");
            }
        }

        private void MudaJogador() {
            if (JogadorAtual == Cor.Branco) {
                JogadorAtual = Cor.Preto;
            }
            else {
                JogadorAtual = Cor.Branco;
            }
        }

        public HashSet<Peca> Capturadas(Cor Cor) {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in PecasCapturadas) {
                if (x.Cor == Cor) {
                    aux.Add(x);
                }
            }
            return aux;
        }

        public HashSet<Peca> PecasEmJogo(Cor Cor) {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in Pecas) {
                if (x.Cor == Cor) {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(Capturadas(Cor));
            return aux;
        }

        private Cor adversaria(Cor Cor) {
            if (Cor == Cor.Branco) {
                return Cor.Preto;
            }
            else {
                return Cor.Branco;
            }
        }

        private Peca rei(Cor Cor) {
            foreach (Peca x in PecasEmJogo(Cor)) {
                if (x is Rei) {
                    return x;
                }
            }
            return null;
        }

        public bool estaEmXeque(Cor Cor) {
            Peca R = rei(Cor);
            if (R == null) {
                throw new TabuleiroException("Não tem rei da Cor " + Cor + " no Tabuleiro!");
            }
            foreach (Peca x in PecasEmJogo(adversaria(Cor))) {
                bool[,] mat = x.MovimentosPossiveis();
                if (mat[R.Posicao.Linha, R.Posicao.Coluna]) {
                    return true;
                }
            }
            return false;
        }

        public bool testeXequemate(Cor Cor) {
            if (!estaEmXeque(Cor)) {
                return false;
            }
            foreach (Peca x in PecasEmJogo(Cor)) {
                bool[,] mat = x.MovimentosPossiveis();
                for (int i = 0; i < Tab.Linhas; i++) {
                    for (int j = 0; j < Tab.Colunas; j++) {
                        if (mat[i, j]) {
                            Posicao origem = x.Posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = executaMovimento(origem, destino);
                            bool testeXeque = estaEmXeque(Cor);
                            desfazMovimento(origem, destino, pecaCapturada);
                            if (!testeXeque) {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void colocarNovaPeca(char Coluna, int Linha, Peca Peca) {
            Tab.ColocarPecas(Peca, new PosicaoXadrez(Coluna, Linha).ToPosicao());
            Pecas.Add(Peca);
        }

        private void colocarPecas() {
            colocarNovaPeca('a', 1, new Torre(Tab, Cor.Branco));
            colocarNovaPeca('b', 1, new Cavalo(Tab, Cor.Branco));
            colocarNovaPeca('c', 1, new Bispo(Tab, Cor.Branco));
            colocarNovaPeca('d', 1, new Dama(Tab, Cor.Branco));
            colocarNovaPeca('e', 1, new Rei(Tab, Cor.Branco));
            colocarNovaPeca('f', 1, new Bispo(Tab, Cor.Branco));
            colocarNovaPeca('g', 1, new Cavalo(Tab, Cor.Branco));
            colocarNovaPeca('h', 1, new Torre(Tab, Cor.Branco));
            colocarNovaPeca('a', 2, new Peao(Tab, Cor.Branco));
            colocarNovaPeca('b', 2, new Peao(Tab, Cor.Branco));
            colocarNovaPeca('c', 2, new Peao(Tab, Cor.Branco));
            colocarNovaPeca('d', 2, new Peao(Tab, Cor.Branco));
            colocarNovaPeca('e', 2, new Peao(Tab, Cor.Branco));
            colocarNovaPeca('f', 2, new Peao(Tab, Cor.Branco));
            colocarNovaPeca('g', 2, new Peao(Tab, Cor.Branco));
            colocarNovaPeca('h', 2, new Peao(Tab, Cor.Branco));

            colocarNovaPeca('a', 8, new Torre(Tab, Cor.Preto));
            colocarNovaPeca('b', 8, new Cavalo(Tab, Cor.Preto));
            colocarNovaPeca('c', 8, new Bispo(Tab, Cor.Preto));
            colocarNovaPeca('d', 8, new Dama(Tab, Cor.Preto));
            colocarNovaPeca('e', 8, new Rei(Tab, Cor.Preto));
            colocarNovaPeca('f', 8, new Bispo(Tab, Cor.Preto));
            colocarNovaPeca('g', 8, new Cavalo(Tab, Cor.Preto));
            colocarNovaPeca('h', 8, new Torre(Tab, Cor.Preto));
            colocarNovaPeca('a', 7, new Peao(Tab, Cor.Preto));
            colocarNovaPeca('b', 7, new Peao(Tab, Cor.Preto));
            colocarNovaPeca('c', 7, new Peao(Tab, Cor.Preto));
            colocarNovaPeca('d', 7, new Peao(Tab, Cor.Preto));
            colocarNovaPeca('e', 7, new Peao(Tab, Cor.Preto));
            colocarNovaPeca('f', 7, new Peao(Tab, Cor.Preto));
            colocarNovaPeca('g', 7, new Peao(Tab, Cor.Preto));
            colocarNovaPeca('h', 7, new Peao(Tab, Cor.Preto));
        }
    }
}