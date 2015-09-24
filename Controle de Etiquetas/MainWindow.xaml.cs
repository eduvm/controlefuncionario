// Criado por eduardo
// Data: 21/09/2015
// Solução: Controle de Etiquetas
// Projeto:Controle de Etiquetas
// Arquivo: MainWindow.xaml.cs
// =========================================
// Última alteração: 24/09/2015

#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using BarcodeLib;

using Controle_de_Etiquetas.Clientes;
using Controle_de_Etiquetas.Controles;
using Controle_de_Etiquetas.Funcionarios;
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

            // Cria nova instancia da janela de atualização de cliente
            var WinImport = new ImportarClientes();

            // Mostra janela como Dialog
            WinImport.ShowDialog();

            // Recebe todos os argumentos recebidos
            var args = Environment.GetCommandLineArgs();

            // Verificase deve habilitar os botões de inclusão
            if (!string.IsNullOrEmpty(args[0]) && args[0] == "dev") {

                // Cadastro de clientes
                btnCadDestino.IsEnabled = true;
                btnAlterarDestino.IsEnabled = true;
                btnExcluirDestino.IsEnabled = true;

                // Cadastro de funcionários
                btnIncluirFunc.IsEnabled = true;
                btnAlterarFunc.IsEnabled = true;
                btnExcluirFunc.IsEnabled = true;

            }

            // Chama método que faz o carregamento do cadastro de funcionários
            CarregaFuncionarios();

            // Chama método que faz o carregamento do cadsatro de destinos
            CarregaClientes();

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

        #region Eventos

        private void btnCadDestino_Click(object sender, RoutedEventArgs e) {
            // Chama método que para fazer inclusão de destinos
            IncluirCliente();
        }

        private void btnExcluirDestino_Click(object sender, RoutedEventArgs e) {
            // Chama método que exlcuir o destino
            ExcluirCliente();
        }

        private void btnAlterarDestino_Click(object sender, RoutedEventArgs e) {
            // Chama rotina de alteração
            AlterarCliente();
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

        private void tbCodCod_TextChanged(object sender, TextChangedEventArgs e) {

            // Verifica se texto não está em branco
            if (!string.IsNullOrEmpty(tbCodCod.Text)) {

                // Envia texto para o método que vai gerar o código de barras
                GeraBarCode(tbCodCod.Text);

            }

        }

        private void Window_Closed(object sender, EventArgs e) {
            // Seta valor do controle para parar a thread
            NeedStop = true;
        }

        private void btnCodImp_Click(object sender, RoutedEventArgs e) {

            // Chama método que imprime a etiqueta
            ImprimirEtq(impCod);

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

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            KListener.KeyDown += KListener_KeyDown;
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            KListener.Dispose();
        }

        private void KListener_KeyDown(object sender, RawKeyEventArgs args) {

            //Console.WriteLine(args.Key.ToString());

            if (args.Key.ToString() == "Return") {

                // Seja não estiver editando
                if (editando == false) {

                    tbLeitor.Focus();

                    editando = true;

                    //Console.WriteLine("Editando");
                }

                else {

                    //Console.WriteLine("Devo inicializar");

                    editando = false;
                    IncluirLeitor(tbLeitor.Text);
                    tbLeitor.Text = "";

                }

            }

        }

        private void tbPesqControle_TextChanged(object sender, TextChangedEventArgs e) {

            // Define objeto Text Box
            var t = (TextBox) sender;

            // Define campo de filtro
            var filter = t.Text;

            // Define coleção
            var cv = CollectionViewSource.GetDefaultView(dgControle.ItemsSource);

            // Se o filtro (campos texto) estiver vazio
            if (filter == "") {

                // Seta filtro como nulo
                cv.Filter = null;

            }

            // Se não estiver vazio
            else {

                // Seta filtro
                cv.Filter = o => {

                    // Define objeto controle
                    var p = o as Controle;

                    // Verifica que tipo de filtro deve ser aplicado
                    // Se for funcionário
                    if (cbTipFilterControle.Text == "Funcionário") {

                        // Retorna filtro no nome do funcionário
                        return (p.NomeFuncionario.ToUpper().StartsWith(filter.ToUpper()));

                    }

                    // Se for cliente

                    // Retorna filtro no nome do cliente
                    return (p.NomeCliente.ToUpper().StartsWith(filter.ToUpper()));

                };
            }

        }

        #endregion Eventos

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

        // Defino novo objeto que vai receber os dados do código de barras
        public BarCodeControle EscutaControle = new BarCodeControle();

        private const string CodIni = "INICIALIZA";

        private const string CodFim = "FINALIZA";

        private const string CodRetorno = "RETORNO";

        private bool editando;

        private KeyboardListener KListener = new KeyboardListener();

        #endregion Variaveis

        #region Métodos

        /// <summary>
        ///     Método responsável por carregar os funcionários cadastrados no banco de dados
        /// </summary>
        private void CarregaFuncionarios() {

            // Limpa dataGrid
            dgFuncionarios.ItemsSource = null;

            try {
                // Gera novo objeto de conexao ao banco de dados
                var objFunc = new DatabaseHelper();

                // Define SQL Query
                var query = "SELECT id , c_nome FROM dados.funcionario WHERE b_deletado = false ORDER BY id";

                // Executa a query
                var dt = objFunc.GetDataTable(query);

                // Gera nova lista de clientes
                var lFuncionarios = new ListaFuncionarios();

                // Faz for para preencher a lista de pessoas
                foreach (DataRow row in dt.Rows) {
                    lFuncionarios.Add(new Funcionario {
                        Id = row["id"].ToString(),
                        Nome = row["c_nome"].ToString(),
                        BarCode = row["id"].ToString()
                    });
                }

                // Faz bind da lista de pessoas no Grid
                dgFuncionarios.ItemsSource = lFuncionarios;
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
                var SQL = "SELECT id, c_nomefuncionario, i_funcionario_id, c_nomecliente, i_cliente_id, to_char(d_data_saida, 'dd/MM/yyyy') AS d_data_saida, t_hora_saida, to_char(d_data_chegada, 'dd/MM/yyyy') AS d_data_chegada, t_hora_chegada, b_fechado FROM dados.entradas WHERE b_deletado = false ORDER BY d_data_saida, t_hora_saida DESC";

                // Pega DataTable com resultado do SQL
                var dtResult = objCarregaControle.GetDataTable(SQL);

                // Gera nova lista de clientes
                var lControle = new ListaControles();

                // Cria laço para preencher a lista de controles
                foreach (DataRow row in dtResult.Rows) {
                    lControle.Add(new Controle {
                        Id = row["id"].ToString(),
                        NomeFuncionario = row["c_nomefuncionario"].ToString(),
                        IdFuncionario = row["i_funcionario_id"].ToString(),
                        NomeCliente = row["c_nomecliente"].ToString(),
                        IdCliente = row["i_cliente_id"].ToString(),
                        DataSaida = row["d_data_saida"].ToString(),
                        HoraSaida = row["t_hora_saida"].ToString(),
                        DataChegada = row["d_data_chegada"].ToString(),
                        HoraChegada = row["t_hora_chegada"].ToString(),
                        FlagFechado = Convert.ToBoolean(row["b_fechado"])

                    });

                }

                // Define o item source do grid de controle
                dgControle.ItemsSource = lControle;

            }

            catch (Exception fail) {
                // Seta mensagem de erro
                var error = "Erro ao carregar controle:\n\n";

                // Anexa mensagem de erro na mensagem
                error += fail.Message + "\n\n";

                // Apresenta mensagem na tela
                MessageBox.Show(error);

            }
        }

        private void IncluirLeitor(string Mensagem) {

            // Verifica se é a mensagem de iniciar o objeto
            if (string.IsNullOrEmpty(EscutaControle.CodigoInicial)) {

                if (Mensagem != CodIni) {

                    // Apresenta mensagem de erro
                    MessageBox.Show("Você deve primeiro escanear o código de inicialização");

                }
                else {

                    // Devo marcar no objeto como iniciado
                    EscutaControle.CodigoInicial = Mensagem;

                    Dispatcher.Invoke(() => {

                        // Apresenta mensagem de informação
                        tbInformação.Text = "Iniciada saída:";

                        // Limpa Grid
                        dgControle.ItemsSource = null;

                        // Define objeto EscutaControle como novo ItemsSource
                        dgControle.ItemsSource = EscutaControle;

                    });

                }

            }

            // Se o objeto já foi inicializado
            else {

                // Verifico se o código do funcionário está vazrio
                if (string.IsNullOrEmpty(EscutaControle.CodFuncionario)) {

                    // Verifica se a mensagem retornada é um código númerico
                    if (Regex.IsMatch(Mensagem, @"^\d+$")) {

                        var dbResult = RetornaFuncionario(Mensagem);

                        // Verifico se existe o funcionario com o código da mensagem
                        if (string.IsNullOrEmpty(dbResult)) {

                            // Apresento mensagem de que não reconheceu o funcionário
                            MessageBox.Show("Funcionário não reconhecido.\nCódigo do funcionário nãoencontrado: " + Mensagem);

                        }

                        // Se conseguiu encontrar o funcionário
                        else {

                            // Grava código do funcionário no objeto
                            EscutaControle.CodFuncionario = Mensagem;

                            Dispatcher.Invoke(() => {

                                // Apresenta mensagem de informação
                                tbInformação.Text = "Funcionário: " + dbResult;

                            });
                            ;

                        }

                    }

                    // Se ela não for um código numérico
                    else {

                        // Apresento mensagem de que não reconheceu o funcionário
                        MessageBox.Show("Funcionário não reconhecido.\nCódigo do funcionário nãoencontrado: " + Mensagem);

                    }

                }

                // Se o objeto ja foi inicializado, e o funcionário preenchido
                // Neste caso, a Mensagem é o codigo do cliente
                else {

                    switch (Mensagem) {

                        case CodFim: {

                            // Verificose existem registros a serem gravados no banco
                            if (EscutaControle.Count > 0) {

                                // Gero laço para percorrer a lista de registros

                                foreach (var iControle in EscutaControle) {

                                    // Crio objeto de acesso ao banco de dados
                                    var objDB = new DatabaseHelper();

                                    // Crio Dicionario que vai conter os campos  e valores
                                    var dctDados = new Dictionary<string, string>();

                                    // Adiciono dados no dicionario
                                    dctDados.Add("c_nomefuncionario", iControle.NomeFuncionario);
                                    dctDados.Add("i_funcionario_id", iControle.IdFuncionario);
                                    dctDados.Add("c_nomecliente", iControle.NomeCliente);
                                    dctDados.Add("i_cliente_id", iControle.IdCliente);
                                    dctDados.Add("d_data_saida", iControle.DataSaida);
                                    dctDados.Add("t_hora_saida", iControle.HoraSaida);
                                    dctDados.Add("b_fechado", iControle.FlagFechado.ToString());

                                    // Verifico se não consseguir incluir os dados no banco
                                    if (!objDB.Insert("dados.entradas", dctDados)) {

                                        // Apresenta mensagem de erro
                                        MessageBox.Show("Ocorreu um erro ao tentar adicionar o registro");

                                    }

                                }

                            }

                            LimpaObjeto();

                            break;
                        }

                        case CodRetorno: {

                            // Gero novo objeto de acesso ao banco de dados
                            var objDB = new DatabaseHelper();

                            // Gero novo comando SQL
                            var SQL = string.Format("SELECT id FROM dados.entradas WHERE i_funcionario_id = '{0}' AND b_fechado = false", EscutaControle.CodFuncionario);

                            // Salvo resultado do SQL em um datatable
                            var dtResult = objDB.GetDataTable(SQL);

                            // Defino data de saída
                            var dtDataRetorno = DateTime.Now.ToString("dd/MM/yyyy");

                            // Defino hora de saída
                            var dtHoraRetorno = DateTime.Now.ToString("hh:mm:ss");

                            // Crio laço para rodar em cada resultado
                            foreach (DataRow row in dtResult.Rows) {

                                // Defino novo dicionario com campo/valor
                                var dctDados = new Dictionary<string, string>();

                                // Adiciono campos no dicionario
                                dctDados.Add("d_data_chegada", dtDataRetorno);
                                dctDados.Add("t_hora_chegada", dtHoraRetorno);
                                dctDados.Add("b_fechado", "true");

                                // Tenta atualizar os registros
                                if (!objDB.Update("entradas", dctDados, "id = " + row["id"])) {

                                    MessageBox.Show("Ocorreu um erro ao tentar fechar as saídas");

                                }

                            }

                            // Executa rotina para limpar o objeto
                            LimpaObjeto();

                            // Devo limpar e recarregar o Grid de Controle
                            Dispatcher.Invoke(() => {

                                // Apresenta mensagem de informação
                                tbInformação.Text = "Gravado retorno";

                            });

                            break;
                        }

                        default: {

                            // Carrego nome do cliente
                            var dbResult = RetornaCliente(Mensagem);

                            // Verifico se o cliente existe
                            if (string.IsNullOrEmpty(dbResult)) {

                                // Apresento mensagem de que não existe o cliente
                                MessageBox.Show("Não foi possível localizar o cliente com o código :" + Mensagem);

                            }

                            // Se conseguir encontrar o cliente
                            else {

                                // Crio novo objeto controle
                                var ctrTemp = new Controle();

                                // Defino ID
                                ctrTemp.Id = "*";

                                // Defino funcionario com o funcionário atual
                                ctrTemp.IdFuncionario = EscutaControle.CodFuncionario;

                                // Defino nome do funcionario
                                ctrTemp.NomeFuncionario = RetornaFuncionario(EscutaControle.CodFuncionario);

                                // Defino código do cliente
                                ctrTemp.IdCliente = Mensagem;

                                // Defino nome do cliente
                                ctrTemp.NomeCliente = RetornaCliente(Mensagem);

                                // Defino data de saída
                                ctrTemp.DataSaida = DateTime.Now.ToString("dd/MM/yyyy");

                                // Defino hora de saída
                                ctrTemp.HoraSaida = DateTime.Now.ToString("hh:mm:ss");

                                // Defino flag como aberto
                                ctrTemp.FlagFechado = false;

                                Dispatcher.Invoke(() => {

                                    // Adiciono o objeto na lista de objetos
                                    EscutaControle.Add(ctrTemp);

                                });

                            }

                            break;

                        }

                    }

                    // Verifico se devo fechar o registro
                    if (Mensagem == CodFim) {

                    }

                }

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

                        // Verifica se é a mensagem de iniciar o objeto
                        if (string.IsNullOrEmpty(EscutaControle.CodigoInicial)) {

                            if (Mensagem != CodIni) {

                                // Apresenta mensagem de erro
                                MessageBox.Show("Você deve primeiro escanear o código de inicialização");

                            }
                            else {

                                // Devo marcar no objeto como iniciado
                                EscutaControle.CodigoInicial = Mensagem;

                                Dispatcher.Invoke(() => {

                                    // Apresenta mensagem de informação
                                    tbInformação.Text = "Iniciada saída:";

                                    // Limpa Grid
                                    dgControle.ItemsSource = null;

                                    // Define objeto EscutaControle como novo ItemsSource
                                    dgControle.ItemsSource = EscutaControle;

                                });

                            }

                        }

                        // Se o objeto já foi inicializado
                        else {

                            // Verifico se o código do funcionário está vazrio
                            if (string.IsNullOrEmpty(EscutaControle.CodFuncionario)) {

                                // Verifica se a mensagem retornada é um código númerico
                                if (Regex.IsMatch(Mensagem, @"^\d+$")) {

                                    var dbResult = RetornaFuncionario(Mensagem);

                                    // Verifico se existe o funcionario com o código da mensagem
                                    if (string.IsNullOrEmpty(dbResult)) {

                                        // Apresento mensagem de que não reconheceu o funcionário
                                        MessageBox.Show("Funcionário não reconhecido.\nCódigo do funcionário nãoencontrado: " + Mensagem);

                                    }

                                    // Se conseguiu encontrar o funcionário
                                    else {

                                        // Grava código do funcionário no objeto
                                        EscutaControle.CodFuncionario = Mensagem;

                                        Dispatcher.Invoke(() => {

                                            // Apresenta mensagem de informação
                                            tbInformação.Text = "Funcionário: " + dbResult;

                                        });
                                        ;

                                    }

                                }

                                // Se ela não for um código numérico
                                else {

                                    // Apresento mensagem de que não reconheceu o funcionário
                                    MessageBox.Show("Funcionário não reconhecido.\nCódigo do funcionário nãoencontrado: " + Mensagem);

                                }

                            }

                            // Se o objeto ja foi inicializado, e o funcionário preenchido
                            // Neste caso, a Mensagem é o codigo do cliente
                            else {

                                switch (Mensagem) {

                                    case CodFim: {

                                        // Verificose existem registros a serem gravados no banco
                                        if (EscutaControle.Count > 0) {

                                            // Gero laço para percorrer a lista de registros

                                            foreach (var iControle in EscutaControle) {

                                                // Crio objeto de acesso ao banco de dados
                                                var objDB = new DatabaseHelper();

                                                // Crio Dicionario que vai conter os campos  e valores
                                                var dctDados = new Dictionary<string, string>();

                                                // Adiciono dados no dicionario
                                                dctDados.Add("c_nomefuncionario", iControle.NomeFuncionario);
                                                dctDados.Add("i_funcionario_id", iControle.IdFuncionario);
                                                dctDados.Add("c_nomecliente", iControle.NomeCliente);
                                                dctDados.Add("i_cliente_id", iControle.IdCliente);
                                                dctDados.Add("d_data_saida", iControle.DataSaida);
                                                dctDados.Add("t_hora_saida", iControle.HoraSaida);
                                                dctDados.Add("b_fechado", iControle.FlagFechado.ToString());

                                                // Verifico se não consseguir incluir os dados no banco
                                                if (!objDB.Insert("dados.entradas", dctDados)) {

                                                    // Apresenta mensagem de erro
                                                    MessageBox.Show("Ocorreu um erro ao tentar adicionar o registro");

                                                }

                                            }

                                        }

                                        LimpaObjeto();

                                        break;
                                    }

                                    case CodRetorno: {

                                        // Gero novo objeto de acesso ao banco de dados
                                        var objDB = new DatabaseHelper();

                                        // Gero novo comando SQL
                                        var SQL = string.Format("SELECT id FROM dados.entradas WHERE i_funcionario_id = '{0}' AND b_fechado = false", EscutaControle.CodFuncionario);

                                        // Salvo resultado do SQL em um datatable
                                        var dtResult = objDB.GetDataTable(SQL);

                                        // Defino data de saída
                                        var dtDataRetorno = DateTime.Now.ToString("dd/MM/yyyy");

                                        // Defino hora de saída
                                        var dtHoraRetorno = DateTime.Now.ToString("hh:mm:ss");

                                        // Crio laço para rodar em cada resultado
                                        foreach (DataRow row in dtResult.Rows) {

                                            // Defino novo dicionario com campo/valor
                                            var dctDados = new Dictionary<string, string>();

                                            // Adiciono campos no dicionario
                                            dctDados.Add("d_data_chegada", dtDataRetorno);
                                            dctDados.Add("t_hora_chegada", dtHoraRetorno);
                                            dctDados.Add("b_fechado", "true");

                                            // Tenta atualizar os registros
                                            if (!objDB.Update("entradas", dctDados, "id = " + row["id"])) {

                                                MessageBox.Show("Ocorreu um erro ao tentar fechar as saídas");

                                            }

                                        }

                                        // Executa rotina para limpar o objeto
                                        LimpaObjeto();

                                        // Devo limpar e recarregar o Grid de Controle
                                        Dispatcher.Invoke(() => {

                                            // Apresenta mensagem de informação
                                            tbInformação.Text = "Gravado retorno";

                                        });

                                        break;
                                    }

                                    default: {

                                        // Carrego nome do cliente
                                        var dbResult = RetornaCliente(Mensagem);

                                        // Verifico se o cliente existe
                                        if (string.IsNullOrEmpty(dbResult)) {

                                            // Apresento mensagem de que não existe o cliente
                                            MessageBox.Show("Não foi possível localizar o cliente com o código :" + Mensagem);

                                        }

                                        // Se conseguir encontrar o cliente
                                        else {

                                            // Crio novo objeto controle
                                            var ctrTemp = new Controle();

                                            // Defino ID
                                            ctrTemp.Id = "*";

                                            // Defino funcionario com o funcionário atual
                                            ctrTemp.IdFuncionario = EscutaControle.CodFuncionario;

                                            // Defino nome do funcionario
                                            ctrTemp.NomeFuncionario = RetornaFuncionario(EscutaControle.CodFuncionario);

                                            // Defino código do cliente
                                            ctrTemp.IdCliente = Mensagem;

                                            // Defino nome do cliente
                                            ctrTemp.NomeCliente = RetornaCliente(Mensagem);

                                            // Defino data de saída
                                            ctrTemp.DataSaida = DateTime.Now.ToString("dd/MM/yyyy");

                                            // Defino hora de saída
                                            ctrTemp.HoraSaida = DateTime.Now.ToString("hh:mm:ss");

                                            // Defino flag como aberto
                                            ctrTemp.FlagFechado = false;

                                            Dispatcher.Invoke(() => {

                                                // Adiciono o objeto na lista de objetos
                                                EscutaControle.Add(ctrTemp);

                                            });

                                        }

                                        break;

                                    }

                                }

                                // Verifico se devo fechar o registro
                                if (Mensagem == CodFim) {

                                }

                            }

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

        private void LimpaObjeto() {

            // Devo limpar o objeto
            Dispatcher.Invoke(() => {

                // Limpa o objeto para ser usado novamente
                EscutaControle.CodFinal = null;
                EscutaControle.CodFuncionario = null;
                EscutaControle.CodigoInicial = null;
                EscutaControle.Clear();

                // Define objeto EscutaControle como novo ItemsSource
                CarregaControle();

                // Apresenta mensagem de informação
                tbInformação.Text = "Cadastro finalizado";

            });
        }

        private static string RetornaFuncionario(string CodFunc) {

            // Crio novo objeto de acesso ao banco de dados
            var objDB = new DatabaseHelper();

            // Defino SQL
            var SQL = string.Format("SELECT c_nome FROM dados.funcionario WHERE id = '{0}'", CodFunc);

            // Executo query e salvo na variável result
            var dbResult = objDB.ExecuteScalar(SQL);

            return dbResult;

        }

        private static string RetornaCliente(string CodCli) {

            // Crio novo objeto de acesso ao banco de dados
            var objDB = new DatabaseHelper();

            // Defino SQL
            var SQL = string.Format("SELECT c_nomefantas FROM dados.cliente WHERE i_cdcliente = '{0}'", CodCli);

            // Executo query e salvo na variável result
            var dbResult = objDB.ExecuteScalar(SQL);

            return dbResult;

        }

        private void IncluirCliente() {
            // Cria nova instância da janela de Cadastro de Clientes
            var CadWin = new CadCliente("incluir", "");

            // Chama (mostra na tela) a janela de Cadastro de Clientes
            CadWin.ShowDialog();

            // Chama método de carregar os clientes
            CarregaClientes();
        }

        private void ExcluirCliente() {
            // Verifica se existe registro selecionado
            if (dgClientes.SelectedItem == null) {
                // Apresenta mensagem de erro
                MessageBox.Show("Você precisa escolher um destino para excluí-lo");
            }

            // Se existir registro selecionado
            else {

                // Gero objeto cliente com os dados do cliente selecionado
                var objCLiente = (Cliente) dgClientes.SelectedItem;

                // Defino valor da coluna id
                var strId = objCLiente.Id;

                // Cria objeto de acesso ao banco de dados
                var objDB = new DatabaseHelper();

                // Crio dicionário com chave e valor a ser alterado no banco de dados
                var dctDados = new Dictionary<string, string>();

                // Adiciono campo e valor no dicionario
                dctDados.Add("b_deletado", "true");

                // Chama método que faz alteração no banco de dados
                if (objDB.Update("dados.cliente", dctDados, "id = " + strId)) {
                    // Informa que o registro foi alterado com sucesso
                    MessageBox.Show("O cliente foi deletado");

                    // Chama método que atualiza o grid de destinos
                    CarregaClientes();
                }

                else {
                    // Informo que ocorreu erro
                    MessageBox.Show("Ocorreu um erro ao tentar excluir o cliente");
                }
            }
        }

        private void AlterarCliente() {
            // Verifico se existe registro selecionado
            if (dgClientes.SelectedItem == null) {
                // Informo que deve selecionar um registro
                MessageBox.Show("Você deve selecionar um registro para alterá-lo");
            }

            // Se existe registro selecionado
            else {

                // Defino novo objeto Funcionario com o registro selecionado
                var selecionado = (Cliente) dgClientes.SelectedItem;

                // Gravo na variável o Id do usuário selecionado
                var strId = selecionado.Id;

                // Cria nova instância da janela de Cadastro de Destinos
                var CadWin = new CadCliente("alterar", strId);

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

        private void GeraBarCode(string texto) {

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

            imgCodCod.Source = bi;

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