using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace zlib
{
    public partial class Form1 : Form
    {
        #region Conversão de Little pra Big Endian

        uint bigendian16(uint le)
        {
            //Converte um valor de 16Bits de Big endian para Little endian

            uint be = (uint)((byte)(le >> 8) | ((byte)le << 8));
            return be;
        }

        uint bigendian32(uint le)
        {
            //Converte um valor de 32Bits de Big endian para Little endian

            uint pr = le >> 24;
            uint se = le >> 8 & 0x00FF00;
            uint te = le << 24;
            uint qu = le << 8 & 0x00FF0000;
            return pr | se | te | qu;
        }

        ulong bigendian64(ulong le)
        {
            //Converte um valor de 64Bits de Big endian para Little endian

            ulong primeiroByte = (le >> 0) & 0xFF;
            ulong segundoByte = (le >> 8) & 0xFF;
            ulong terceiroByte = (le >> 16) & 0xFF;
            ulong quartoByte = (le >> 24) & 0xFF;
            ulong quintoByte = (le >> 32) & 0xFF;
            ulong sextoByte = (le >> 40) & 0xFF;
            ulong setimoByte = (le >> 48) & 0xFF;
            ulong oitavoByte = (le >> 56) & 0xFF;
            return oitavoByte | setimoByte | sextoByte | quintoByte | quartoByte | terceiroByte | segundoByte | primeiroByte;
        }

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Comprimir arquivo

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Todos os arquivos (*.*)|*.*";
            openFileDialog1.Title = "Escolha um arquivo para comprimir com ZLIB...";
            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    string nomeArquivocomprimido = Path.ChangeExtension(file, "z");

                    byte[] dados_arquivo_descomprimido = File.ReadAllBytes(file);

                    int tamanhodoarquivo = dados_arquivo_descomprimido.Length;

                    const int mod = 65521;
                    uint a = 1, b = 0;
                    foreach (byte c in dados_arquivo_descomprimido)
                    {
                        a = (a + c) % mod;
                        b = (b + a) % mod;
                    }
                    uint adler32 = (b << 16) | a;

                    MemoryStream dados_descomprimidos = new MemoryStream(dados_arquivo_descomprimido);
                    MemoryStream dados_comprimidos = new MemoryStream();
                    DeflateStream deflateStream = new DeflateStream(dados_comprimidos, CompressionMode.Compress);
                    dados_descomprimidos.CopyTo(deflateStream);
                    deflateStream.Close();
                    byte[] resultadodacompressão = dados_comprimidos.ToArray();

                    int tamanho_comprimido = Buffer.ByteLength(resultadodacompressão);

                    using (BinaryWriter bw = new BinaryWriter(new FileStream(nomeArquivocomprimido, FileMode.Create)))
                    {
                        bw.Write((ushort)0x9C78);
                        bw.Write(resultadodacompressão);
                        bw.Write(bigendian32(adler32));
                        bw.Write(tamanhodoarquivo);
                    }
                }
                MessageBox.Show("Terminado!", "AVISO!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Extrair arquivo

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Todos os arquivos (*.z)|*.z";
            openFileDialog1.Title = "Escolha um arquivo para descomprimir...";
            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    string arquivoDescomprimido = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));                    

                    byte[] dadosComprimidos = File.ReadAllBytes(file);

                    using (MemoryStream streamComprimido = new MemoryStream(dadosComprimidos, 2, dadosComprimidos.Length - 2))
                    {
                        using (DeflateStream deflateStream = new DeflateStream(streamComprimido, CompressionMode.Decompress))
                        {
                            using (FileStream arquivoStream = File.Create(arquivoDescomprimido))
                            {
                                deflateStream.CopyTo(arquivoStream);
                            }
                        }
                    }
                }
                MessageBox.Show("Terminado", "AVISO!");
            }
        }
    }
}
