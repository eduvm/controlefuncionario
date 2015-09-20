#region Header

// Criado por eduvm
// Data: 19/09/2015
// Solução: Controle de Etiquetas
// Projeto: Controle de Etiquetas
// Arquivo: MainWindow.xaml.cs
// =========================================
// Última alteração: 20/09/2015

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

using Controle_de_Etiquetas.Helpers;

#endregion

namespace Controle_de_Etiquetas {

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        #region Construtores

        public MainWindow() {
            // Inicializa os componentes da janela
            InitializeComponent();

            // Chama método que faz o carregamento do cadastro de funcionários
            CarregaFuncionarios();

            // Chama método que faz o carregamento do cadsatro de destinos
            CarregaDestinos();

            // Chama método que faz o carregamendo do cadastro de controle
            CarregaControle();

            // Instacia Thread passando a ThreadStart
            MinhaThread = new Thread(EscutarConexao);

            // Seto thread como background
            MinhaThread.IsBackground = true;

            // Inicia a Thread
            MinhaThread.Start();
        }

        #endregion Construtores

        #region Botões

        private void btnCadDestino_Click(object sender, RoutedEventArgs e) {
            // Chama método que para fazer inclusão de destinos
            IncluirDestino();
        }

        private void btnExcluirDestino_Click(object sender, RoutedEventArgs e) {
            // Chama método que exlcuir o destino
            ExcluirDestino();
        }

        

        #endregion Botões

        

        #region Variaveis

        // Defino objeto Thread
        private Thread MinhaThread;

        // Define porta
        private int Porta = 5000;

        // Define IP
        private IPAddress Server = IPAddress.Parse("192.168.25.108");

        // Defino objeto TcpListene
        private TcpListener tcpServidor;

        // Defino variável de controle da Thread
        public bool NeedStop;

        #endregion Variaveis

        #region Métodos

        /// <summary>
        ///     Método responsável por carregar os funcionários cadastrados no banco de dados
        /// </summary>
        private void CarregaFuncionarios() {
            // Limpa registros do dataGrid
            dgFuncionarios.ItemsSource = null;

            // Cria objeto de acesso ao banco de dados
            var objCarregaFuncionario = new DatabaseHelper();

            // Comando SQL
            var SQL = "SELECT id, c_funcionario FROM dados.funcionario WHERE b_deletado = false ORDER BY id";

            // Pega DataTable com resultado do SQL
            var result = objCarregaFuncionario.GetDataTable(SQL);

            // Seta item source do DataGrid
            dgFuncionarios.ItemsSource = result.DefaultView;
        }

        /// <summary>
        ///     Método responsável por carregar os funcionários cadastrados no banco de dados
        /// </summary>
        private void CarregaDestinos() {
            // Limpa registros do dataGrid
            dgDestino.ItemsSource = null;

            // Cria objeto de acesso ao banco de dados
            var objCarregaDestino = new DatabaseHelper();

            // Comando SQL
            var SQL = "SELECT id, c_nome FROM dados.destino WHERE b_deletado = false ORDER BY id";

            // Pega DataTable com resultado do SQL
            var result = objCarregaDestino.GetDataTable(SQL);

            // Seta item source do DataGrid
            dgDestino.ItemsSource = result.DefaultView;
        }

        private void CarregaControle() {
            // Limpa registros do dataGrid
            dgControle.ItemsSource = null;

            // Cria objeto de acesso ao banco de dados
            var objCarregaControle = new DatabaseHelper();

            // Comando SQL
            var SQL = "SELECT id, n_id_funcionario, d_data, t_hora, n_id_destino  FROM dados.entradas WHERE b_deletado = false ORDER BY id";

            // Pega DataTable com resultado do SQL
            var result = objCarregaControle.GetDataTable(SQL);

            // Seta item source do DataGrid
            dgControle.ItemsSource = result.DefaultView;
        }

        private void EscutarConexao() {
            // Seta controle da Thread como false
            NeedStop = false;

            try {
                // Instâncio TcpListener
                tcpServidor = new TcpListener(Server, Porta);

                // Inicio o TcpListener
                tcpServidor.Start();

                // Defino Buffer para recebimento de mensagem
                var bytes = new byte[256];

                // Defino variavel que vai receber o texto da mensamge
                string Mensagem = null;

                // Faço laço para ficar verificando mensagem
                while (true) {
                    // Verifico se deve parar o while
                    if (NeedStop) {
                        // Try to stop Thread
                        MinhaThread.Abort();

                        // Get out off the while
                        break;
                    }

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    var client = tcpServidor.AcceptTcpClient();

                    // Limpa mensagem
                    Mensagem = null;

                    // Cria objeto stream para ler e escrever no socket
                    var SocketStream = client.GetStream();

                    // Variáve de controle
                    int i;

                    // Faz loop para receber o texto da mensagem enviado pelo cliente.
                    while ((i = SocketStream.Read(bytes, 0, bytes.Length)) != 0) {
                        // Traduz os bytes para uma string
                        Mensagem = Encoding.ASCII.GetString(bytes, 0, i);

                        // Cria objeto de acesso ao banco de dados
                        var objControle = new DatabaseHelper();

                        // Cria dicionário com chave/valor a ser inserido no banco de dados
                        var dctDados = new Dictionary<string, string>();

                        // Pega data do dia
                        var dData = DateTime.Now.ToString("dd/MM/yyyy");

                        // Pega hora atual
                        var dHora = DateTime.Now.ToString("hh:mm:ss");

                        // TODO - Pegar na base de dados o nome do funcionario atraves do codigo de barras recebido
                        dctDados.Add("n_id_funcionario", Mensagem.Substring(0, 3));

                        // TODO - Pegar na base de dados o nome do destino através do código de barras recebido
                        dctDados.Add("n_id_destino", Mensagem.Substring(3, 5));

                        // Adiciono a data no Dicionario de dados
                        dctDados.Add("d_data", dData);

                        // Adiciono a hora no Dicionario de dados
                        dctDados.Add("t_hora", dHora);

                        // Insere os dados no banco
                        if (objControle.Insert("dados.entradas", dctDados)) {
                            // Chama o método de carregametno de dados através do Invoke porque está rodando em uma Thread diferente
                            Dispatcher.Invoke(() => {
                                // Chama método que atualiza o grid de controle
                                CarregaControle();
                            });
                        }
                        else {
                            // Informa que ocorreu erro ao tentar adicionar o controle no banco de dados
                            MessageBox.Show("Ocorreu um erro ao tentar incluir o controle no banco de dados\nPor favor, informe ao seu administrador");
                        }

                        // Define mensagem de resposta transformando de string para bytes
                        var msg = Encoding.ASCII.GetBytes("OK");

                        // Envia a resposta
                        SocketStream.Write(msg, 0, msg.Length);
                    }

                    // Finaliza a conexao
                    SocketStream.Close();
                }
            }

                // Em caso de erro , apresenta a mensagem na tela
            catch (SocketException e) {
                MessageBox.Show("Ocorreu um erro\n" + e);
            }

            finally {
                // Finaliza a escuta
                tcpServidor.Stop();
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            // Seta valor do controle para parar a thread
            NeedStop = true;
        }

        private void IncluirDestino() {
            // Cria nova instância da janela de Cadastro de Destinos
            var CadWin = new CadDestino();

            // Chama (mostra na tela) a janela de Cadastro de Destinos
            CadWin.ShowDialog();

            // Chama método de carregar os destinos
            CarregaDestinos();
        }

        private void ExcluirDestino() {
            // Verifica se existe registro selecionado
            if (dgDestino.SelectedItem == null) {
                // Apresenta mensagem de erro
                MessageBox.Show("Você precisa escolher um destino para excluí-lo");
            }

            // Se existir registro selecionado
            else {
                // Pega id do registro selecionado
                var rowview = dgDestino.SelectedItem as DataRowView;

                // Defino valor da coluna id
                var strId = rowview.Row["id"].ToString();

                // Cria objeto de acesso ao banco de dados
                var objDB = new DatabaseHelper();

                // Crio dicionário com chave e valor a ser alterado no banco de dados
                var dctDados = new Dictionary<string, string>();

                // Adiciono campo e valor no dicionario
                dctDados.Add("b_deletado", "true");

                // Chama método que faz alteração no banco de dados
                if (objDB.Update("dados.destino", dctDados, "id = " + strId)) {
                    // Informa que o registro foi alterado com sucesso
                    MessageBox.Show("O Destino foi deletado");

                    // Chama método que atualiza o grid de destinos
                    CarregaDestinos();
                }

                else {
                    // Informo que ocorreu erro
                    MessageBox.Show("Ocorreu um erro ao tentar excluir o destino");
                }
            }
        }

        #endregion Métodos
    }

}