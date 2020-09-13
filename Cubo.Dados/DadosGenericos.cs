using Cubo.Dados.Infraestrutura;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cubo.Dados
{
    public class DadosGenericos : ObjetoPadraoAcesso
    {
        private static DadosGenericos gobjDadosgenericos = null;
        public DadosGenericos(FonteDados aobSource) : base(aobSource) { }
        public async Task<Entidade> ObterObjeto<Entidade>(string ID)
        {
            List<Entidade> Objeto;
            String dataSerializable = "";
            CultureInfo cultureInfo = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            try
            {
                Dictionary<string, string> keys;
                TableAttribute tableName = typeof(Entidade).GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;
                var dataset = await gobjFonte.Consultar($" SELECT * FROM {tableName.Name} WHERE ID = {ID} ");
                keys = new Dictionary<string, string>();
                if (dataset.Tables.Count > 0)
                {
                    if (dataset.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataColumn colum in dataset.Tables[0].Columns)
                        {
                            if (colum.ToString().Contains("_Id".ToUpper()))
                            {
                                var namecolumn = colum.ToString();
                                var value = dataset.Tables[0].Rows[0][colum?.ToString()].ToString();
                                if (value != "")
                                    keys.Add(namecolumn, value);
                            }

                        }
                        dataSerializable = DataTableParaString(dataset.Tables[0]);
                    }
                }
                if (dataSerializable != "")
                {
                    // JArray json = JArray.Parse(dataSerializable) as JArray;
                    JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                    jsonSerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    jsonSerializerSettings.Culture = CultureInfo.CurrentCulture;
                    Objeto = JsonConvert.DeserializeObject<List<Entidade>>(dataSerializable, jsonSerializerSettings);
                    dataSerializable = "";
                    List<object> list = new List<object>();
                    foreach (var item in keys)
                    {
                        var data = await gobjFonte.Consultar($" SELECT * FROM { Regex.Replace(item.Key.Replace("_Id".ToUpper(), ""), @"[\d-]", string.Empty)} WHERE ID = {item.Value} ");
                        if (data.Tables.Count > 0)
                        {
                            if (data.Tables[0].Rows.Count > 0)
                            {
                                dataSerializable = DataTableParaString(data.Tables[0]);
                                Type tipo = Objeto.FirstOrDefault().GetType();
                                IList<PropertyInfo> props = new List<PropertyInfo>(tipo.GetProperties()).ToList();
                                var obj = props.Last();
                                foreach (PropertyInfo prop in props)
                                {
                                    var dd = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                                    if (dd != null)
                                    {
                                        if (dd.Name == item.Key?.Trim())
                                        {
                                            var forenObject = JsonConvert.DeserializeObject<List<object>>(dataSerializable, jsonSerializerSettings).FirstOrDefault();
                                            var objetokey = JsonConvert.DeserializeObject(forenObject.ToString(), prop.PropertyType, jsonSerializerSettings);
                                            var propriedades = tipo.GetProperties();
                                            SetValue(Objeto.FirstOrDefault(), prop.Name, objetokey);
                                            break;

                                        }
                                    }
                                }
                            }
                        }
                    }
                    return Objeto.FirstOrDefault();
                }
            }
            catch (Exception lobjSoftiumException)
            {
                throw lobjSoftiumException;
            }
            return default(Entidade);
        }
        /// <param name="DATA_REL">Esse parametro define qual tipo de instrução SQL vai ser usada</param>
        public async Task<IList<Entidade>> CarregarListaObjetos<Entidade>(string[] astrcampo = null, string[] astrvalor = null, TIPOSQL DATA_REL = TIPOSQL.NOT_DEFINE)
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            IList<Entidade> Objetos;
            StringBuilder sb = new StringBuilder();
            String dataSerializable = "";
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            jsonSerializerSettings.Culture = CultureInfo.CurrentCulture;
            bool verylist = false;
            try
            {
                return await Task.Run(async () =>
                {
                    if (astrcampo == null & astrvalor == null)
                    {
                        TableAttribute tableName = typeof(Entidade).GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;
                        sb.Append($" SELECT * FROM {tableName.Name} ");
                    }
                    else
                    {
                        TableAttribute tableName = typeof(Entidade).GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;
                        sb.Append($" SELECT * FROM {tableName.Name} ");
                        sb.Append(" where 1=1 ");
                        for (int i = 0; i < astrcampo.Length; i++)
                        {
                            switch (DATA_REL)
                            {
                                case TIPOSQL.NOT_DEFINE:
                                    sb.Append(" AND " + astrcampo[i] + " = " + astrvalor[i]);
                                    break;
                                case TIPOSQL.INTERVAL_DATA:
                                    sb.Append($" AND {astrcampo[0]}  between  '{astrvalor[0]}'  AND  '{astrvalor[1]}' ");
                                    break;

                            }
                        }
                    }
                    DataSet dataset = await gobjFonte.Consultar(sb.ToString());
                    IList<Entidade> objectsList = new List<Entidade>();
                    if (dataset.Tables.Count > 0)
                    {
                        bool index = false;
                        if (dataset.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow item in dataset.Tables[0].Rows)
                            {
                                if (index == true) { };
                                Dictionary<string, string> keys = new Dictionary<string, string>();
                                string auxdataSerializable = "";
                                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                                Dictionary<string, object> row;
                                row = new Dictionary<string, object>();
                                for (int i = 0; i < item.Table.Columns.Count; i++)
                                {
                                    row.Add(item.Table.Columns[i].ColumnName, item.Table.Rows[item.Table.Rows.IndexOf(item)][item.Table.Columns[i]]);
                                    if (item.Table.Columns[i].ToString().Contains("_Id".ToUpper()))
                                    {
                                        var namecolumn = item.Table.Columns[i].ToString();
                                        var objitem = item.Table.Rows[item.Table.Rows.IndexOf(item)][item.Table.Columns[i].ToString()].ToString();
                                        keys.Add(namecolumn, objitem);
                                        verylist = true;
                                    }
                                }
                                rows.Add(row);
                                auxdataSerializable = JsonConvert.SerializeObject(rows);
                                var array = JArray.Parse(auxdataSerializable);
                                index = true;
                                foreach (var obj in array)
                                {
                                    try
                                    {
                                        var objdata = JsonConvert.DeserializeObject<Entidade>(obj.ToString(), jsonSerializerSettings);
                                        Type tipo = objdata.GetType();
                                        foreach (var key in keys)
                                        {
                                            if (key.Value != "")
                                            {
                                                var data = await gobjFonte.Consultar($" SELECT * FROM { Regex.Replace(key.Key.Replace("_Id".ToUpper(), ""), @"[\d-]", string.Empty)} WHERE ID = {key.Value} ");
                                                dataSerializable = DataTableParaString(data.Tables[0]);
                                            }
                                            IList<PropertyInfo> props = new List<PropertyInfo>(tipo.GetProperties()).ToList();
                                            foreach (PropertyInfo prop in props)
                                            {
                                                var dd = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                                                if (dd != null)
                                                {
                                                    if (dd.Name == key.Key?.Trim())
                                                    {
                                                        if (dataSerializable != "")
                                                        {
                                                            var forenObject = JsonConvert.DeserializeObject<List<object>>(dataSerializable, jsonSerializerSettings).FirstOrDefault();
                                                            var objetokey = JsonConvert.DeserializeObject(forenObject.ToString(), prop.PropertyType, jsonSerializerSettings);
                                                            var propriedades = tipo.GetProperties();
                                                            SetValue(objdata, prop.Name, objetokey);
                                                            dataSerializable = "";
                                                        }

                                                    }
                                                }
                                            }

                                        }
                                        objectsList.Add(objdata);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                }
                            }
                        }
                    }
                    dataSerializable = DataTableParaString(dataset.Tables[0]);
                    if (dataSerializable != "")
                    {
                        if (verylist == false)
                        {
                            Objetos = JsonConvert.DeserializeObject<List<Entidade>>(dataSerializable, jsonSerializerSettings);
                            return Objetos;
                        }
                        else { return objectsList; }
                    }
                    return default(List<Entidade>);
                });
            }
            catch (Exception lobjSoftiumException)
            {
                throw lobjSoftiumException;
            }
        }
        public async Task DeletarObjeto(object aobjDeletar)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                Type myType = aobjDeletar.GetType();
                IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties()).Where(x => x.Name.ToUpper() == "Id".ToUpper()).ToList();
                var obj = props.Last();
                object ID = obj.GetValue(aobjDeletar, null);
                TableAttribute tableName = aobjDeletar.GetType().GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;
                sb.Append($" DELETE FROM { tableName.Name } WHERE ID = {ID} ");
                await gobjFonte.Excluir(sb.ToString());
            }
            catch (Exception lobjSoftiumException)
            {
                throw lobjSoftiumException;
            }
        }
        public async Task InserirObjeto(object aobInsert)
        {
            try
            {
                CultureInfo cultureInfo = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                StringBuilder sb = new StringBuilder();
                Type myType = aobInsert.GetType();
                IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties()).Where(x => x.Name.ToUpper() != "ID".ToUpper()).ToList();
                TableAttribute tableName = myType.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;
                sb.Append($" INSERT INTO {tableName.Name} SET ");
                var obj = props.Last();
                var Id = props.Where(x => x.Name == "ID").FirstOrDefault();
                props = props.Where(x => x.Name != "ID").ToList();
                foreach (PropertyInfo prop in props)
                {
                    object propValue = prop.GetValue(aobInsert, null);
                    if (prop.Equals(obj))
                    {
                        if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Int32) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.DateTime) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Decimal) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Double) || prop.PropertyType == typeof(System.String) || prop.PropertyType == typeof(System.DateTime) || prop.PropertyType == typeof(System.Double) || prop.PropertyType == typeof(System.Decimal))
                            if (prop.PropertyType == typeof(System.DateTime)) { sb.Append($" {prop.Name} = '{String.Format("{0:yyyy-MM-dd HH:mm:ss}", propValue)}' "); }
                            else
                            {
                                if (prop.PropertyType == typeof(System.String))
                                {
                                    if (propValue != null)
                                        sb.Append($" {prop.Name} = '{propValue}'");
                                }
                                else if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.DateTime))
                                {
                                    if (propValue != null)
                                        sb.Append($" {prop.Name} = '{String.Format("{0:yyyy-MM-dd HH:mm:ss}", propValue)}' ");
                                }
                                else
                                {
                                    if (propValue != null)
                                        sb.Append($" {prop.Name} =  {propValue}");
                                }
                            }
                        else
                        {
                            if (prop.GetType().IsClass)
                            {
                                if (propValue != null)
                                {
                                    var colunaId = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                                    var IdObjeto = propValue.GetType().GetProperty("ID".ToUpper()).GetValue(propValue, null);
                                    sb.Append($"{colunaId.Name} = { IdObjeto } ");
                                }
                                else
                                {
                                    var colunaId = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                                    sb.Append($"{ colunaId.Name } = null   ");
                                }
                            }
                            else
                            {
                                if (propValue != null)
                                    sb.Append($" {prop.Name} = {propValue} ");
                            }
                        }
                    }
                    else
                    {
                        //Type answer = Nullable.GetUnderlyingType(prop.PropertyType);
                        if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Int32) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.DateTime) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Decimal) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Double) || prop.PropertyType == typeof(System.String) || prop.PropertyType == typeof(System.DateTime) || prop.PropertyType == typeof(System.Double) || prop.PropertyType == typeof(System.Decimal))
                        {
                            if (prop.PropertyType == typeof(System.DateTime)) { sb.Append($" {prop.Name} = '{String.Format("{0:yyyy-MM-dd HH:mm:ss}", propValue)}', "); }
                            else
                            {
                                if (prop.PropertyType == typeof(System.String))
                                {
                                    if (propValue != null)
                                        sb.Append($" {prop.Name} = '{propValue}',");
                                }
                                else if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.DateTime))
                                {
                                    if (propValue != null)
                                        sb.Append($" {prop.Name} = '{String.Format("{0:yyyy-MM-dd HH:mm:ss}", propValue)}', ");
                                }
                                else
                                {
                                    if (propValue != null)
                                        sb.Append($" {prop.Name} =  {propValue},");
                                }
                            }
                        }
                        else
                        {
                            if (prop.GetType().IsClass)
                            {
                                if (propValue != null)
                                {
                                    var colunaId = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                                    var IdObjeto = propValue.GetType().GetProperty("ID".ToUpper()).GetValue(propValue, null);
                                    sb.Append($"{colunaId.Name} = { IdObjeto }, ");
                                }
                                else
                                {
                                    var colunaId = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                                    sb.Append($"{ colunaId.Name } = null ,  ");
                                }

                            }
                            else
                            {
                                if (propValue != null)
                                    sb.Append($" {prop.Name} = {propValue}, ");
                            }
                        }
                    }
                }
                await gobjFonte.Insert(sb.ToString());
            }
            catch (Exception lobjSoftiumException)
            {
                throw lobjSoftiumException;
            }
        }
        public async Task AtualizarObjeto(object aobAtualiza)
        {
            try
            {

                CultureInfo cultureInfo = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                StringBuilder sb = new StringBuilder();
                Type myType = aobAtualiza.GetType();
                IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties()).ToList();
                TableAttribute tableName = myType.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;
                sb.Append($" UPDATE {tableName.Name} SET ");
                var obj = props.Last();
                var Id = props.Where(x => x.Name.ToUpper() == "Id".ToUpper()).FirstOrDefault();
                props = props.Where(x => x.Name.ToUpper() != "Id".ToUpper()).ToList();
                foreach (PropertyInfo prop in props)
                {
                    object propValue = prop.GetValue(aobAtualiza, null);
                    if (prop.Equals(obj))
                    {
                        if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Int32) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.DateTime) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Decimal) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Double) || prop.PropertyType == typeof(System.String) || prop.PropertyType == typeof(System.DateTime) || prop.PropertyType == typeof(System.Double) || prop.PropertyType == typeof(System.Decimal))
                            if (prop.PropertyType == typeof(System.DateTime)) { sb.Append($" {prop.Name} = '{String.Format("{0:yyyy-MM-dd HH:mm:ss}", propValue)}' "); }
                            else
                            {
                                if (prop.PropertyType == typeof(System.String))
                                {
                                    if (propValue != null)
                                    {
                                        sb.Append($" {prop.Name} = '{propValue}' ");
                                    }

                                }
                                else if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.DateTime))
                                {
                                    if (propValue != null)
                                        sb.Append($" {prop.Name} = '{String.Format("{0:yyyy-MM-dd HH:mm:ss}", propValue)}' ");
                                }
                                else
                                {
                                    if (propValue != null)
                                        sb.Append($" {prop.Name} =  {propValue} ");
                                }
                            }
                        else
                        {
                            if (prop.GetType().IsClass)
                            {
                                if (propValue != null)
                                {
                                    var colunaId = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                                    var IdObjeto = propValue.GetType().GetProperty("ID".ToUpper()).GetValue(propValue, null);
                                    sb.Append($"{colunaId.Name} = { IdObjeto } ");
                                }
                                else
                                {
                                    var colunaId = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                                    sb.Append($"{ colunaId.Name } = null   ");
                                }
                            }
                            else
                            {
                                if (propValue != null)
                                    sb.Append($" {prop.Name} = {propValue} ");
                            }
                        }
                    }
                    else
                    {
                        //Type answer = Nullable.GetUnderlyingType(prop.PropertyType);
                        if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Int32) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.DateTime) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Decimal) || Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.Double) || prop.PropertyType == typeof(System.String) || prop.PropertyType == typeof(System.DateTime) || prop.PropertyType == typeof(System.Double) || prop.PropertyType == typeof(System.Decimal))
                        {
                            if (prop.PropertyType == typeof(System.DateTime)) { sb.Append($" {prop.Name} = '{String.Format("{0:yyyy-MM-dd HH:mm:ss}", propValue)}', "); }
                            else
                            {
                                if (prop.PropertyType == typeof(System.String))
                                {
                                    if (propValue != null)
                                        sb.Append($" {prop.Name} = '{propValue}',");
                                }
                                else if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(System.DateTime))
                                {
                                    if (propValue != null)
                                        sb.Append($" {prop.Name} = '{String.Format("{0:yyyy-MM-dd HH:mm:ss}", propValue)}', ");
                                }
                                else
                                {
                                    if (propValue != null)
                                        sb.Append($" {prop.Name} =  {propValue},");
                                }
                            }
                        }
                        else
                        {
                            if (prop.GetType().IsClass)
                            {
                                if (propValue != null)
                                {
                                    var colunaId = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                                    var IdObjeto = propValue.GetType().GetProperty("ID".ToUpper()).GetValue(propValue, null);
                                    sb.Append($" { colunaId.Name } = { IdObjeto }, ");
                                }
                                else
                                {
                                    var colunaId = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                                    sb.Append($"{ colunaId.Name } = null, ");
                                }
                            }
                            else
                            {
                                if (propValue != null)
                                    sb.Append($" {prop.Name} = {propValue}, ");
                            }
                        }
                    }
                    // Do something with propValue
                }
                object valueId = Id.GetValue(aobAtualiza, null);
                sb.Append($" WHERE ID = {valueId} ");
                await gobjFonte.Alterar(sb.ToString());
            }
            catch (Exception lobjSoftiumException)
            {
                throw lobjSoftiumException;
            }
        }
        public async Task<DataSet> ConsultaPersonalizada(string sql)
        {
            try
            {
                return await gobjFonte.Consultar(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private String DataTableParaString(DataTable table)
        {
            StringBuilder JSONString = new StringBuilder();
            if (table.Rows.Count > 0)
            {
                JSONString.Append("[");
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    JSONString.Append("{");
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (j < table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + (table.Rows[i][j].GetType().ToString() != "System.Double" ? table.Rows[i][j].ToString().Replace("\"", "") : table.Rows[i][j].ToString().Replace(",", ".").Replace("\"", "")) + "\",");
                        }
                        else if (j == table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + (table.Rows[i][j].GetType().ToString() != "System.Double" ? table.Rows[i][j].ToString().Replace("\"", "") : table.Rows[i][j].ToString().Replace(",", ".").Replace("\"", "")) + "\"");
                        }
                    }
                    if (i == table.Rows.Count - 1)
                    {
                        JSONString.Append("}");
                    }
                    else
                    {
                        JSONString.Append("},");
                    }
                }
                JSONString.Append("]");
            }
            return JSONString.ToString();
        }
        public static DadosGenericos ObterInstancia
        {
            get
            {
                try
                {
                    if (DadosGenericos.gobjDadosgenericos == null)
                        DadosGenericos.gobjDadosgenericos = new DadosGenericos(FonteDados.ObterInstancia);

                    return DadosGenericos.gobjDadosgenericos;
                }
                catch (Exception lobjSoftiumException)
                {
                    throw lobjSoftiumException;
                }
            }
        }
        private void SetValue(object inputObject, string propertyName, object propertyVal)
        {
            //find out the type
            Type type = inputObject.GetType();
            //get the property information based on the type
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            //find the property type
            Type propertyType = propertyInfo.PropertyType;
            //Convert.ChangeType does not handle conversion to nullable types
            //if the property type is nullable, we need to get the underlying type of the property
            var targetType = IsNullableType(propertyInfo.PropertyType) ? Nullable.GetUnderlyingType(propertyInfo.PropertyType) : propertyInfo.PropertyType;
            //Returns an System.Object with the specified System.Type and whose value is
            //equivalent to the specified object.
            propertyVal = Convert.ChangeType(propertyVal, targetType);
            //Set the value of the property
            propertyInfo.SetValue(inputObject, propertyVal, null);

        }
        private bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
        private static String ConverterDatasetJson(DataSet dsEntrada)
        {
            if (dsEntrada != null)
            {
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row;
                DataTable dt = dsEntrada.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }
                return "";
            }
            else
            {
                return "";
            }

        }

    }
}
