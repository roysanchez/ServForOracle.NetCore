
namespace ServForOracle.NetCore.Parameters
{
    public class ParamHandler
    {
        public PreparedParameter PrepareParameterForQuery<T>(string name, ParamObject<T> parameter, int startNumber)
        {
            var (constructor, lastNumber) = parameter.Metadata.BuildQueryConstructorString(parameter.Value, name, startNumber);
            var oracleParameters = parameter.Metadata.GetOracleParameters(parameter.Value, startNumber);

            return new PreparedParameter(constructor, startNumber, lastNumber, oracleParameters);
        }

        public PreparedOutputParameter PrepareOutputParameter<T>(string name, ParamObject<T> parameter, int startNumber)
        {
            string query = string.Empty;
            if (typeof(T).IsArray)
            {
                query = parameter.Metadata.GetRefCursorCollectionQuery(startNumber, name);
            }
            else
            {
                query = parameter.Metadata.GetRefCursorQuery(startNumber, name);
            }

            var oracleParameter = parameter.Metadata.GetOracleParameterForRefCursor(startNumber);

            return new PreparedOutputParameter(parameter, oracleParameter, query);
        }
    }
}
