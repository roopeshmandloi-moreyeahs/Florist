using System.IO;

namespace WebApplication1.Models
{
    public class ByteToFile
    {
        public string fileUrl1 { get; set; }
        public string fileName1 { get; set; }
        public string fileUrl2 { get; set; }
        public string fileName2 { get; set; }
        public string custType { get; set; }
        public string access_token { get; set; }
        public void SaveByteArrayToFileWithBinaryWriter(byte[] data, string filePath)
        {
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(filePath)))
            {
                writer.Write(data);
            }            
        }
    }
}
