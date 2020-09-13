using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace Cubo.Dados.Conexao
{
    public class ConnectionFactory
    {
        public async Task<MySqlConnection> getConnection()
        {
            //senha edari :h8w4f9w2
            return await Task.Run(() => {
                //String conexao = "Server=FATURAMENTO; Database=dbcalbrasil; Uid=root; Pwd=h8w4f9w2;";
                String conexao = "Server=localhost; Database=dbcubo; Uid=root; Pwd=mbi123;";
                MySqlConnection con = new MySqlConnection(conexao);
                return con;
            });
        }
    }
}
