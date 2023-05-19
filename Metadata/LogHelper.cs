namespace Metadata
{
    public class LogHelper
    {        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static void WriteLog(string type, string methodName, string msg)
        {
            try
            {
                string log_msg = DateTime.Now.ToString() + " | " + methodName + " | " + msg;
                if (type.Equals("INFO"))
                {
                    log.Info(log_msg);
                }
                else if (type.Equals("ERROR"))
                {
                    log.Error(log_msg);
                }
                
            }
            catch //(Exception)
            {

            }
        }
    }
}
