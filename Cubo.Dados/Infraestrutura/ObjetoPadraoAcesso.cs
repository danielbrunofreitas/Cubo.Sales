using System;
using System.Collections.Generic;
using System.Text;

namespace Cubo.Dados.Infraestrutura
{
    public class ObjetoPadraoAcesso
    {
        #region Membro Privado
        protected FonteDados gobjFonte;
        #endregion
        #region Construtor
        public ObjetoPadraoAcesso(FonteDados aobjFonte)
        {
            this.gobjFonte = aobjFonte;
        }
        #endregion
    }
}
