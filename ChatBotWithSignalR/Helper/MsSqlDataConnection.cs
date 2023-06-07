using FastReport;
using FastReport.Data;
using System.ComponentModel;

namespace ChatBotWithSignalR.Helper
{
    //public class MsSqlDataConnection : FastReport.Data.DataConnectionBase
    //public class MsSqlDataConnection : DataConnectionBase, IComponent, IDisposable, IFRSerializable, IParent
    //{
    //    public override string QuoteIdentifier(string value, System.Data.Common.DbConnection connection)
    //    {
    //        return "\"" + value + "\"";
    //    }

    //    public override System.Type GetConnectionType()

    //    {
    //        return typeof(System.Data.SqlClient.SqlConnection);
    //    }

    //    public override System.Type GetParameterType()
    //    {
    //        return typeof(System.Data.SqlDbType);
    //    }

    //    public override System.Data.Common.DbDataAdapter GetAdapter(string selectCommand, System.Data.Common.DbConnection connection, FastReport.Data.CommandParameterCollection parameters)
    //    {
    //        System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(selectCommand, connection as System.Data.SqlClient.SqlConnection);
    //        foreach (FastReport.Data.CommandParameter p in parameters)
    //        {
    //            System.Data.SqlClient.SqlParameter parameter = adapter.SelectCommand.Parameters.Add(p.Name, (System.Data.SqlDbType)p.DataType, p.Size);
    //            parameter.Value = p.Value;
    //        }
    //        return adapter;
    //    }
    //}
}
   

