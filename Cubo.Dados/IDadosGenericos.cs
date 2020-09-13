using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Cubo.Dados
{
    public enum TIPOSQL { NOT_DEFINE, INTERVAL_DATA, LIKE }
    public interface IDadosGenericos
    {
        Task<Entidade> ObterObjeto<Entidade>(string ID);
        Task<IList<Entidade>> CarregarListaObjetos<Entidade>(string[] astrcampo = null, string[] astrvalor = null, TIPOSQL DATA_REL = TIPOSQL.NOT_DEFINE);
        Task DeletarObjeto(object aobjDeletar);
        Task InserirObjeto(object aobInsert);
        Task AtualizarObjeto(object aobAtualiza);
        Task<DataSet> ConsultaPersonalizada(string sql);
    }
}
