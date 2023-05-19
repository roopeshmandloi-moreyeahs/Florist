using System.Data;

namespace Metadata
{
    public static class Def_Values
    {        
        public static string auth_code { get; set; }
        public static string client_Secret { get; set; }
        public static string sharepoint_Secret { get; set; }
        public static string client_id { get; set; }
        public static bool _createLog { get; set; }
        public static List<Auth_List> authList = new List<Auth_List> { };
        public static List<Token_List> tokenList = new List<Token_List> { };
        public static IEnumerable<DataRow> AsEnumerable(this DataTable table)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                yield return table.Rows[i];
            }
        }

    }
}
