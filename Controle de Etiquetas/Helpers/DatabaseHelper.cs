#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

using Controle_de_Etiquetas.Properties;

using Npgsql;

#endregion

namespace Controle_de_Etiquetas.Helpers {

    internal class DatabaseHelper {

        #region Construtores

        /// <summary>
        ///     Construtor padão
        /// </summary>
        public DatabaseHelper() {

            CarregaParametros();

            dbParameters = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};", serverName, port, userName, password, databaseName);

        }

        /// <summary>
        ///     Overload de construtor onde informo qual database utilizar
        /// </summary>
        /// <param name="database">Nome da base de dados a ser utilizada</param>
        public DatabaseHelper(string database) {

            CarregaParametros();

            // Neste, preciso redefinir a váriavel databaseName pois ela será recebida por parametro
            databaseName = database;

            dbParameters = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};", serverName, port, userName, password, databaseName);

        }

        /// <summary>
        ///     Construtor especificando parametros avançados
        /// </summary>
        /// <param name="connectionOpts">
        ///     Um dicionário contendo todos os parametros e seus devidos valores
        /// </param>
        public DatabaseHelper(Dictionary<string, string> connectionOpts) {
            // Define variavel que recebera os parametros
            var str = "";

            // Para cada parametro
            foreach (var row in connectionOpts) {
                // Adiciono o parametro (key) e o valor (value)
                str += string.Format("{0}={1}; ", row.Key, row.Value);
            }

            // Remove possiveis espaços em branco
            str = str.Trim().Substring(0, str.Length - 1);

            // Seto string de conexao com os parametros
            dbParameters = str;
        }

        #endregion Construtores

        #region Métodos

        private void CarregaParametros() {

            serverName = Settings.Default.Host;
            port = Settings.Default.Port;
            userName = Settings.Default.User;
            password = Settings.Default.Password;
            databaseName = Settings.Default.Database;

        }

        /// <summary>
        ///     Executa uma query no banco
        /// </summary>
        /// <param name="sql">SQL a ser rodado</param>
        /// <returns>Retorna uma DataTable com o resultado da query.</returns>
        public DataTable GetDataTable(string sql) {
            // Define novo DataTable que vai receber os dados da consulta SQL
            var dt = new DataTable();

            try {
                var cnn = new NpgsqlConnection(dbParameters);

                cnn.Open();

                var mycommand = new NpgsqlCommand(sql, cnn);

                var reader = mycommand.ExecuteReader();

                dt.Load(reader);

                reader.Close();

                cnn.Close();
            }

            catch (Exception e) {
                throw new Exception(e.Message);
            }

            return dt;
        }

        /// <summary>
        ///     Executa outros comandos SQL que não sejam de uma query (SELECT).
        /// </summary>
        /// <param name="sql">Comando SQL a ser executado.</param>
        /// <returns>Um inteiro com a quantidade de linhas alteradas.</returns>
        public int ExecuteNonQuery(string sql) {
            var cnn = new NpgsqlConnection(dbParameters);

            cnn.Open();

            var mycommand = new NpgsqlCommand(sql, cnn);

            mycommand.CommandText = sql;

            var rowsUpdated = mycommand.ExecuteNonQuery();

            cnn.Close();

            return rowsUpdated;
        }

        /// <summary>
        ///     Possibilita recuperar apenas um item da base (uma linha de query).
        /// </summary>
        /// <param name="sql">SQL a ser rodado.</param>
        /// <returns>Uma string</returns>
        public string ExecuteScalar(string sql) {
            var cnn = new NpgsqlConnection(dbParameters);

            cnn.Open();

            var mycommand = new NpgsqlCommand(sql, cnn);

            var value = mycommand.ExecuteScalar();

            cnn.Close();

            if (value != null) {
                return value.ToString();
            }

            return "";
        }

        /// <summary>
        ///     Permite fazer update de linhas na base.
        /// </summary>
        /// <param name="tableName">A tabela a ser atualizada.</param>
        /// <param name="data">Dicionário contendo nomes das colunas e seus valores.</param>
        /// <param name="where">Especifica o WHERE do SQL.</param>
        /// <returns>Retorna um booleano com true para sucesso e false para erro</returns>
        public bool Update(string tableName, Dictionary<string, string> data, string where) {
            var vals = "";

            var returnCode = true;

            if (data.Count >= 1) {
                foreach (var val in data) {
                    vals += string.Format(" {0} = '{1}',", val.Key, val.Value);
                }

                vals = vals.Substring(0, vals.Length - 1);
            }

            try {
                ExecuteNonQuery(string.Format("update {0} set {1} where {2};", tableName, vals, where));
            }

            catch {
                returnCode = false;
            }

            return returnCode;
        }

        /// <summary>
        ///     Permite deletar linhas da base de dados.
        /// </summary>
        /// <param name="tableName">Nome da tabela</param>
        /// <param name="where">Clausula WHERE.</param>
        /// <returns>Retorna booleano para sucesso ou erro.</returns>
        public bool Delete(string tableName, string where) {
            var returnCode = true;

            try {
                ExecuteNonQuery(string.Format("delete from {0} where {1};", tableName, where));
            }

            catch (Exception fail) {
                MessageBox.Show(fail.Message);

                returnCode = false;
            }

            return returnCode;
        }

        /// <summary>
        ///     Permite inserir linhas na base de dados.
        /// </summary>
        /// <param name="tableName">Tabela ser ser inserida a linha</param>
        /// <param name="data">Dicionario contendo nomes das colunas e seu devido valor.</param>
        /// <returns>Retorna booleano para sucesso ou erro.</returns>
        public bool Insert(string tableName, Dictionary<string, string> data) {
            var columns = "";

            var values = "";

            var returnCode = true;

            foreach (var val in data) {
                columns += string.Format(" {0},", val.Key);

                values += string.Format(" '{0}',", val.Value);
            }

            columns = columns.Substring(0, columns.Length - 1);

            values = values.Substring(0, values.Length - 1);

            try {
                ExecuteNonQuery(string.Format("insert into {0}({1}) values({2});", tableName, columns, values));
            }

            catch (Exception fail) {
                MessageBox.Show(fail.Message);

                returnCode = false;
            }

            return returnCode;
        }

        /// <summary>
        ///     Permite limpar todos os dados de uma determinada tabela.
        /// </summary>
        /// <param name="table">Nome da tabela a ser limpa.</param>
        /// <returns>Retorna um booleano para sucesso ou erro.</returns>
        public bool ClearTable(string table) {
            try {
                ExecuteNonQuery(string.Format("delete from {0};", table));

                return true;
            }

            catch {
                return false;
            }
        }

        /// <summary>
        ///     Permite deletar todos os dados do Database.
        /// </summary>
        /// <returns>Retorna um booleano para sucesso ou erro.</returns>
        public bool ClearDB() {
            DataTable tables;

            try {
                tables = GetDataTable("select NAME from SQLITE_MASTER where type='table' order by NAME;");

                foreach (DataRow table in tables.Rows) {
                    ClearTable(table["NAME"].ToString());
                }

                return true;
            }

            catch {
                return false;
            }
        }

        //Pega todos os registros
        public DataTable GetTodosRegistros() {
            // Define novo DataTable que vai receber os dados da consulta SQL
            var dt = new DataTable();

            // Tenta fazer a consulta
            try {
                using (pgsqlConnection = new NpgsqlConnection(dbParameters)) {
                    // abre a conexão com o PgSQL e define a instrução SQL
                    pgsqlConnection.Open();
                    var cmdSeleciona = "Select * from funcionarios order by id";

                    using (var Adpt = new NpgsqlDataAdapter(cmdSeleciona, pgsqlConnection)) {
                        Adpt.Fill(dt);
                    }
                }
            }
            catch (NpgsqlException ex) {
                throw ex;
            }
            catch (Exception ex) {
                throw ex;
            }
            finally {
                pgsqlConnection.Close();
            }

            return dt;
        }

#endregion Métodos

        #region Definição de variáveis

        // Defino variaveis
        private static string serverName; // Host

        private static string port; // porta default

        private static string userName; // nome do administrador

        private static string password; // senha do administrador

        private static string databaseName; // nome do banco de dados

        private NpgsqlConnection pgsqlConnection;

        private string dbParameters;

        #endregion Definição de variáveis

    }

}