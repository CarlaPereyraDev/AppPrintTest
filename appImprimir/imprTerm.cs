using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;

namespace appImprimir
{
    public class imprTerm
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess, uint dwShateMode, IntPtr lpSecurityAttributes, FileMode dwCreateOption, IntPtr lpTemplateFile);
        public void mains(string jsonxd, List<MDatos> print)
        {
            string nombreImpresora = "Imp Ticket";
            string textoParaImprimir = ""; // = jsonxd; //"¡Hola impresora desde C#!";
            string codigoDeBarras = "";
            string hostname = System.Environment.MachineName;

            // Format the printer path
            string ubicacionCompletaImpresora = string.Format(@"\\{0}\{1}", hostname, nombreImpresora);

            Console.WriteLine("\n\n");

            // Create a file handle for the printer
            SafeFileHandle fh = CreateFile(ubicacionCompletaImpresora, FileAccess.Write, 0, IntPtr.Zero, FileMode.OpenOrCreate, IntPtr.Zero);

            // Check if the file handle is valid
            if (fh.IsInvalid)
            {
                Console.WriteLine("Error abrindo impresora");
                return;
            }

            ////////imprimimos los datos por consola.
            ////if (print != null)
            ////{
            ////    foreach (var item in print)
            ////    {
            ////        Console.WriteLine("\n");
            ////        if(item.Titulo == "impricodbarra") codigoDeBarras += item.Valor + "\n";

            ////        if (item.Valor.Length > 0) textoParaImprimir += item.Titulo + "\n" + item.Valor + "\n";                    
            ////        else textoParaImprimir += item.Titulo + "\n";
            ////    }
            ////}

            ////Console.WriteLine(textoParaImprimir);

            if (print != null)
            {
                foreach (var item in print)
                {
                    Console.WriteLine("\n");
                    if (item.Titulo == "impricodbarra") codigoDeBarras += item.Valor + "\n";

                    if (item.Valor.Length > 0) textoParaImprimir += item.Titulo + "\n" + item.Valor + "\n";
                    else textoParaImprimir += item.Titulo + "\n";
                }
            }

            using (var impresoraComoArchivo = new FileStream(fh, FileAccess.ReadWrite))
            {
                // Write the ESC character (hex 1B) - often used as a control character
                impresoraComoArchivo.WriteByte(0x1B);

                // Write another control command, possibly for formatting
                impresoraComoArchivo.WriteByte(0x40); // ESC @ (initialization command)

                // Write the actual text to print
                impresoraComoArchivo.Write(Encoding.ASCII.GetBytes(textoParaImprimir), 0, textoParaImprimir.Length);

                // Comando para imprimir un código de barras (GS k m d n)
                impresoraComoArchivo.WriteByte(0x1D); // GS
                impresoraComoArchivo.WriteByte(0x6B); // k
                impresoraComoArchivo.WriteByte(73);   // m = 73 (tipo Code 128)
                impresoraComoArchivo.WriteByte((byte)codigoDeBarras.Length); // n = longitud del código de barras
                impresoraComoArchivo.Write(Encoding.ASCII.GetBytes(codigoDeBarras), 0, codigoDeBarras.Length); // d = datos

                // Send the feed command
                impresoraComoArchivo.WriteByte(0x1B); // ESC
                impresoraComoArchivo.WriteByte(0x64); // Feed n lines
                impresoraComoArchivo.WriteByte(Convert.ToByte(1)); // Feed 1 line

                // Optionally, flush or close the stream
                impresoraComoArchivo.Dispose(); // Ensures the handle is closed and data is written
            }
        }
    }
}
