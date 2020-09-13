using Cubo.Dados.Conexao;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Cubo.Dados.Infraestrutura
{
    public class FonteDados
    {
        private static FonteDados gobjFontedeDados = null;
        private readonly ConnectionFactory conexao = null;
        public FonteDados()
        {
            conexao = new ConnectionFactory();
        }
        public static FonteDados ObterInstancia
        {
            get
            {
                try
                {
                    if (gobjFontedeDados == null)
                        FonteDados.gobjFontedeDados = new FonteDados();

                    return FonteDados.gobjFontedeDados;

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public async Task<bool> Insert(string sql)
        {
            try
            {
                return await Task.Run(async () => {
                    MySqlConnection con = await conexao.getConnection();
                    MySqlCommand cmd = con.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    using (con)
                    {
                        MySqlTransaction tran;
                        await con.OpenAsync();
                        tran = await con.BeginTransactionAsync();
                        cmd.Transaction = tran;
                        try
                        {
                            await cmd.ExecuteNonQueryAsync();
                            tran.Commit();
                            await con.CloseAsync();
                            return true;
                        }
                        catch (Exception erro)
                        {
                            tran.Rollback();
                            await con.CloseAsync();
                            Logger.Gerarlog("Cubo System: " + DateTime.Now + " : CONEXAO : FALHA AO INSERIR DADOS." + erro.Message);
                            throw;
                        }

                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Gerarlog("Cubo System: " + DateTime.Now + " : CONEXAO : FALHA AO INSERIR DADOS." + ex.Message);
                return false;
            }
        }
        public async Task<bool> Alterar(string sql)
        {
            try
            {
                return await Task.Run(async () => {
                    MySqlConnection con = await conexao.getConnection();
                    MySqlCommand cmd = con.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    using (con)
                    {
                        MySqlTransaction tran;
                        con.Open();
                        tran = await con.BeginTransactionAsync();
                        cmd.Transaction = tran;
                        try
                        {
                            await cmd.ExecuteNonQueryAsync();
                            tran.Commit();
                            await con.CloseAsync();
                            return true;
                        }
                        catch (Exception erro)
                        {
                            tran.Rollback();
                            await con.CloseAsync();
                            Logger.Gerarlog("Cubo System: " + DateTime.Now + " : CONEXAO : FALHA AO ALTERAR DADOS." + erro.Message);
                            throw;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Gerarlog("Cubo System: " + DateTime.Now + " : CONEXAO : FALHA AO ALTERAR DADOS." + ex.Message);
                return false;
            }
        }
        public async Task<bool> Excluir(string sql)
        {
            try
            {
                return await Task.Run(async () => {
                    MySqlConnection con = await conexao.getConnection();
                    MySqlCommand cmd = con.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    using (con)
                    {
                        MySqlTransaction tran;
                        await con.OpenAsync();
                        tran = await con.BeginTransactionAsync();
                        cmd.Transaction = tran;
                        try
                        {
                            await cmd.ExecuteNonQueryAsync();
                            tran.Commit();
                            await con.CloseAsync();
                            return true;
                        }
                        catch (Exception erro)
                        {
                            tran.Rollback();
                            await con.CloseAsync();
                            Logger.Gerarlog("Cubo System: " + DateTime.Now + " : CONEXAO : FALHA AO EXCLUIR DADOS." + erro.Message);
                            throw;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Gerarlog("Cubo System: " + DateTime.Now + " : CONEXAO : FALHA AO EXCLUIR DADOS." + ex.Message);
                return false;
            }
        }
        public async Task<DataSet> Consultar(string sql)
        {
            try
            {
                return await Task.Run(async () => {
                    MySqlConnection con = await conexao.getConnection();
                    MySqlCommand cmd = con.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    using (con)
                    {
                        await con.OpenAsync();
                        cmd.CommandTimeout = 3000;
                        await cmd.ExecuteNonQueryAsync();
                        DbDataAdapter da = (DbDataAdapter)(new MySqlDataAdapter());
                        DataSet ds = new DataSet();
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                        await con.CloseAsync();
                        return ds;
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Gerarlog("Cubo System: " + DateTime.Now + " : CONEXAO : FALHA AO CONSULTAR DADOS." + ex.Message);
                throw ex;
            }
        }
    }
}
