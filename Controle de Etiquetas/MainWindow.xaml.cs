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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Printing;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using BarcodeLib;

using Controle_de_Etiquetas.Clientes;
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
            CarregaClientes();

            // Chama método que faz o carregamendo do cadastro de controle
            CarregaControle();

            // Chama função que carrega os combos de funcionario e de destino
            //CarregaCombos();

            // Instacia Thread passando a ThreadStart
            //MinhaThread = new Thread(EscutarConexao);

            // Seto thread como background
            //MinhaThread.IsBackground = true;

            // Inicia a Thread
            //MinhaThread.Start();
        }

        #endregion Construtores

        private void btnCodImp_Click(object sender, RoutedEventArgs e) {

            // Chama método que imprime a etiqueta
            ImprimirEtq(impCod);

        }

        private void btnPreImp_Click(object sender, RoutedEventArgs e) {
            // Chama método que imprime a etiqueta
            ImprimirEtq(impPre);
        }

        private void tbCliPesq_TextChanged(object sender, TextChangedEventArgs e) {
            var t = (TextBox) sender;
            var filter = t.Text;
            var cv = CollectionViewSource.GetDefaultView(dgClientes.ItemsSource);

            if (filter == "") {
                cv.Filter = null;
            }
            else {
                cv.Filter = o => {
                    var p = o as Cliente;
                    return (p.NomeFantasia.ToUpper().StartsWith(filter.ToUpper()));
                };
            }
        }

        #region Botões

        private void btnCadDestino_Click(object sender, RoutedEventArgs e) {
            // Chama método que para fazer inclusão de destinos
            IncluirDestino();
        }

        private void btnExcluirDestino_Click(object sender, RoutedEventArgs e) {
            // Chama método que exlcuir o destino
            ExcluirDestino();
        }

        private void btnAlterarDestino_Click(object sender, RoutedEventArgs e) {
            // Chama rotina de alteração
            AlterarDestino();
        }

        private void bntIncluirFunc_Click(object sender, RoutedEventArgs e) {
            // Chama método para fazer a inclusão de funcionário
            IncluirFuncionario();
        }

        private void btnExcluirFunc_Click(object sender, RoutedEventArgs e) {
            // Chama método para excluir funcionário
            ExcluirFuncionario();
        }

        private void btnAlterarFunc_Click(object sender, RoutedEventArgs e) {
            // Chama método para alterar funcionário
            AlterarFuncionario();
        }

        private void btnPreAtu_Click(object sender, RoutedEventArgs e) {
            CarregaCombos();
        }

        private void tbCodCod_TextChanged(object sender, TextChangedEventArgs e) {

            // Verifica se texto não está em branco
            if (!string.IsNullOrEmpty(tbCodCod.Text)) {

                // Envia texto para o método que vai gerar o código de barras
                GeraBarCode(tbCodCod.Text, 1);

            }

        }

        private void cbFunc_DropDownClosed(object sender, EventArgs e) {

            // Gero novo objeto de acesso ao banco de dados
            var objDB = new DatabaseHelper();

            // Gero novo comando SQL
            var SQL = string.Format("SELECT id FROM dados.funcionario WHERE c_funcionario = '{0}'", cbFunc.Text);

            // Executo SQL salvando resultado na variável
            CodFunc = objDB.ExecuteScalar(SQL).PadLeft(4, '0');

            // Faço junção dos textos
            var CodigoBarras = CodFunc + CodDest;

            // Se a string não for nula
            if (!string.IsNullOrEmpty(CodigoBarras)) {

                // Chamo rotina de gerar o código de barras
                GeraBarCode(CodigoBarras, 2);

            }
        }

        private void cbDestino_DropDownClosed(object sender, EventArgs e) {

            // Gero novo objeto de acesso ao banco de dados
            var objDB = new DatabaseHelper();

            // REdefinno comando SQL
            var SQL = string.Format("SELECT id FROM dados.destino WHERE c_nome = '{0}'", cbDestino.Text);

            // Executo SQL salvando resultado na variável
            CodDest = objDB.ExecuteScalar(SQL).PadLeft(5, '0');

            // Faço junção dos textos
            var CodigoBarras = CodFunc + CodDest;

            // Se a string não for nula
            if (!string.IsNullOrEmpty(CodigoBarras)) {

                // Chamo rotina de gerar o código de barras
                GeraBarCode(CodigoBarras, 2);

            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            // Seta valor do controle para parar a thread
            NeedStop = true;
        }

        #endregion Botões

        #region Variaveis

        // Defino objeto Thread
        private Thread MinhaThread;

        // Define porta
        private readonly int Porta = 5000;

        // Define IP
        private readonly IPAddress Server = IPAddress.Parse("10.14.1.56");

        // Defino objeto TcpListene
        private TcpListener tcpServidor;

        // Defino variável de controle da Thread
        public bool NeedStop;

        private string CodFunc;

        private string CodDest;

        #endregion Variaveis

        #region Métodos

        /// <summary>
        ///     Método responsável por carregar os funcionários cadastrados no banco de dados
        /// </summary>
        private void CarregaFuncionarios() {

            // Limpa registros do dataGrid
            dgFuncionarios.ItemsSource = null;

            try {

                // Cria objeto de acesso ao banco de dados
                var objCarregaFuncionario = new DatabaseHelper();

                // Comando SQL
                var SQL = "SELECT id, c_funcionario FROM dados.funcionario WHERE b_deletado = false ORDER BY id";

                // Pega DataTable com resultado do SQL
                var result = objCarregaFuncionario.GetDataTable(SQL);

                // Seta item source do DataGrid
                dgFuncionarios.ItemsSource = result.DefaultView;
            }
            catch (Exception fail) {
                // Seta mensagem de erro
                var error = "O seguinte erro ocorreu:\n\n";

                // Anexa mensagem de erro na mensagem
                error += fail.Message + "\n\n";

                // Apresenta mensagem na tela
                MessageBox.Show(error);

                // Fecha o formulário
                Close();
            }
        }

        /// <summary>
        ///     Método responsável por carregar os funcionários cadastrados no banco de dados
        /// </summary>
        private void CarregaClientes() {

            // Limpa dataGrid
            dgClientes.ItemsSource = null;

            try {
                // Gera novo objeto de conexao ao banco de dados
                var objCliente = new DatabaseHelper("etiquetas");

                // Define SQL Query
                var query = "SELECT id , c_nomefantas, i_cdcliente FROM dados.cliente WHERE b_deletado = false ORDER BY id";

                // Executa a query
                var dt = objCliente.GetDataTable(query);

                // Gera nova lista de clientes
                var lClientes = new ListaClientes();

                // Faz for para preencher a lista de pessoas
                foreach (DataRow row in dt.Rows) {
                    lClientes.Add(new Cliente {
                        Id = row["id"].ToString(),
                        NomeFantasia = row["c_nomefantas"].ToString(),
                        Cpd = row["i_cdcliente"].ToString(),
                        BarCode = row["i_cdcliente"].ToString()
                    });
                }

                // Faz bind da lista de pessoas no Grid
                dgClientes.ItemsSource = lClientes;
            }

                // Trata excessão
            catch (Exception fail) {
                // Seta mensagem de erro
                var error = "O seguinte erro ocorreu:\n\n";

                // Anexa mensagem de erro na mensagem
                error += fail.Message + "\n\n";

                // Apresenta mensagem na tela
                MessageBox.Show(error);

                // Fecha o formulário
                Close();
            }

        }

        private void CarregaControle() {
            // Limpa registros do dataGrid
            dgControle.ItemsSource = null;

            try {
                // Cria objeto de acesso ao banco de dados
                var objCarregaControle = new DatabaseHelper();

                // Comando SQL
                var SQL = "SELECT id, n_id_funcionario, d_data, t_hora, n_id_destino  FROM dados.entradas WHERE b_deletado = false ORDER BY id";

                // Pega DataTable com resultado do SQL
                var result = objCarregaControle.GetDataTable(SQL);

                // Seta item source do DataGrid
                dgControle.ItemsSource = result.DefaultView;
            }

            catch (Exception fail) {
                // Seta mensagem de erro
                var error = "O seguinte erro ocorreu:\n\n";

                // Anexa mensagem de erro na mensagem
                error += fail.Message + "\n\n";

                // Apresenta mensagem na tela
                MessageBox.Show(error);

            }
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

        private void IncluirDestino() {
            // Cria nova instância da janela de Cadastro de Destinos
            var CadWin = new CadDestino("incluir", "");

            // Chama (mostra na tela) a janela de Cadastro de Destinos
            CadWin.ShowDialog();

            // Chama método de carregar os destinos
            CarregaClientes();
        }

        private void ExcluirDestino() {
            // Verifica se existe registro selecionado
            if (dgClientes.SelectedItem == null) {
                // Apresenta mensagem de erro
                MessageBox.Show("Você precisa escolher um destino para excluí-lo");
            }

            // Se existir registro selecionado
            else {
                // Pega id do registro selecionado
                var rowview = dgClientes.SelectedItem as DataRowView;

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
                    CarregaClientes();
                }

                else {
                    // Informo que ocorreu erro
                    MessageBox.Show("Ocorreu um erro ao tentar excluir o destino");
                }
            }
        }

        private void AlterarDestino() {
            // Verifico se existe registro selecionado
            if (dgClientes.SelectedItem == null) {
                // Informo que deve selecionar um registro
                MessageBox.Show("Você deve selecionar um registro para alterá-lo");
            }

            // Se existe registro selecionado
            else {
                // Pega id do registro selecionado
                var rowview = dgClientes.SelectedItem as DataRowView;

                // Defino valor da coluna id
                var strId = rowview.Row["id"].ToString();

                // Cria nova instância da janela de Cadastro de Destinos
                var CadWin = new CadDestino("alterar", strId);

                // Chama (mostra na tela) a janela de Cadastro de Destinos
                CadWin.ShowDialog();

                // Chama método de carregar os destinos
                CarregaClientes();
            }
        }

        private void IncluirFuncionario() {
            // Cria nova instância da janela de Cadastro de Destinos
            var CadWin = new CadFuncionario("incluir", "");

            // Chama (mostra na tela) a janela de Cadastro de Destinos
            CadWin.ShowDialog();

            // Chama método de carregar os destinos
            CarregaFuncionarios();
        }

        private void ExcluirFuncionario() {
            // Verifica se existe registro selecionado
            if (dgFuncionarios.SelectedItem == null) {
                // Apresenta mensagem de erro
                MessageBox.Show("Você precisa escolher um funcionário para excluí-lo");
            }

            // Se existir registro selecionado
            else {
                // Pega id do registro selecionado
                var rowview = dgFuncionarios.SelectedItem as DataRowView;

                // Defino valor da coluna id
                var strId = rowview.Row["id"].ToString();

                // Cria objeto de acesso ao banco de dados
                var objDB = new DatabaseHelper();

                // Crio dicionário com chave e valor a ser alterado no banco de dados
                var dctDados = new Dictionary<string, string>();

                // Adiciono campo e valor no dicionario
                dctDados.Add("b_deletado", "true");

                // Chama método que faz alteração no banco de dados
                if (objDB.Update("dados.funcionario", dctDados, "id = " + strId)) {
                    // Informa que o registro foi alterado com sucesso
                    MessageBox.Show("O Funcionário foi deletado");

                    // Chama método que atualiza o grid de destinos
                    CarregaFuncionarios();
                }

                else {
                    // Informo que ocorreu erro
                    MessageBox.Show("Ocorreu um erro ao tentar excluir o funcionário");
                }
            }
        }

        private void AlterarFuncionario() {
            // Verifico se existe registro selecionado
            if (dgFuncionarios.SelectedItem == null) {
                // Informo que deve selecionar um registro
                MessageBox.Show("Você deve selecionar um funcionário para alterá-lo");
            }

            // Se existe registro selecionado
            else {
                // Pega id do registro selecionado
                var rowview = dgFuncionarios.SelectedItem as DataRowView;

                // Defino valor da coluna id
                var strId = rowview.Row["id"].ToString();

                // Cria nova instância da janela de Cadastro de Destinos
                var CadWin = new CadFuncionario("alterar", strId);

                // Chama (mostra na tela) a janela de Cadastro de Destinos
                CadWin.ShowDialog();

                // Chama método de carregar os destinos
                CarregaFuncionarios();
            }
        }

        private void GeraBarCode(string texto, int tipo) {

            var barcode = new Barcode {
                IncludeLabel = true,
                Alignment = AlignmentPositions.CENTER,
                Width = 300,
                Height = 100,
                RotateFlipType = RotateFlipType.RotateNoneFlipNone
            };

            var img = barcode.Encode(TYPE.CODE128, texto);

            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            switch (tipo) {

                case 1:
                    imgCodCod.Source = bi;
                    break;
                case 2:
                    imgPre.Source = bi;
                    break;
            }

        }

        /// <summary>
        ///     Método responsáel por carregar os combos de funcionários e destino com os respectivos valores
        /// </summary>
        private void CarregaCombos() {

            // Limpa valores atuais
            cbFunc.Items.Clear();
            cbDestino.Items.Clear();

            // Cria novo objeto de acesso ao banco de dados
            var objDB = new DatabaseHelper();

            // Cria comando SQL
            var SQL = "SELECT c_funcionario FROM dados.funcionario WHERE b_deletado = false";

            // Executo SQL salvando resultado na variável result
            var result = objDB.GetDataTable(SQL);

            // Para cada linha do DataTable
            foreach (DataRow row in result.Rows) {

                // Adiciona o nome do funcionário na lista
                cbFunc.Items.Add(row["c_funcionario"].ToString());

            }

            // Redefino o SQL para popular o destino
            SQL = "SELECT c_nomefantas FROM dados.cliente WHERE b_deletado = false";

            // Executo SQL salvando resultado na variável result
            result = objDB.GetDataTable(SQL);

            // Para cada linha do DataTable
            foreach (DataRow row in result.Rows) {

                // Adiciona o nome do funcionário na lista
                cbDestino.Items.Add(row["c_nomefantas"].ToString());

            }

        }

        private void ImprimirEtq(Grid objeto) {

            // Gera novo dialog print
            var printDialog = new PrintDialog();

            // Define queue da dialog
            var pq = LocalPrintServer.GetDefaultPrintQueue();

            // Define novo Print Ticket
            var pt = pq.DefaultPrintTicket;

            // Seta tamanho do papel
            pt.PageMediaSize = new PageMediaSize(396, 130);

            // Seta Print Ticket na dialog
            printDialog.PrintTicket = pt;

            // Define margem
            var xMargin = 38;
            var yMargin = 4;

            // Define tamanho da área a ser imprimida
            var printableWidth = pt.PageMediaSize.Width.Value;
            var printableHeight = pt.PageMediaSize.Height.Value;

            // Define a escala do objeto a ser imprimido para se encaixar no tamanho do papel
            var xScale = (printableWidth - xMargin*2)/printableWidth;
            var yScale = (printableHeight - yMargin*2)/printableHeight;

            // Transforma o tamanho do objeto
            objeto.LayoutTransform = new MatrixTransform(xScale, 0, 0, yScale, xMargin, yMargin);

            // Verifica se deve mesmo imprimir
            if (printDialog.ShowDialog() == true) {

                // Envia impressão para a impressora
                printDialog.PrintVisual(objeto, "Etiqueta");
            }

        }

        #endregion Métodos
    }

}