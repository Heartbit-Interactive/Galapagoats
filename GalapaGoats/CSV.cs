using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    public class CSV
    {
        public string[] rawdata;
        public string[][] data;
        public int sizex;
        public int sizey;

        public CSV(System.IO.BinaryReader br)
        {
            this.binary_read(br);
        }
        public CSV(System.IO.StreamReader tr)
        {
            var tempData = new List<string[]>();
            var tempRawData = new List<string>();
            sizex = 0;
            sizey = 0;
            string line;
            while (!tr.EndOfStream) //scorre e controlla tutto il file passato
            {
                line = tr.ReadLine();
                sizey++;
                var array = line.Split(',');
                tempRawData.Add(line);
                tempData.Add(array);
                if (sizex < array.Length)
                    sizex = array.Length;
            }
            rawdata = tempRawData.ToArray();
            data = tempData.ToArray();
        }
        public void binary_write(System.IO.BinaryWriter bw)
        {
            bw.Write(sizex);
            bw.Write(sizey);
            for (int i = 0; i < sizex; i++)
                for (int j = 0; j < sizey; j++)
                    bw.Write(data[i][j]??"");
        }

        public void binary_read(System.IO.BinaryReader br)
        {
            sizex = br.ReadInt32(); 
            sizey = br.ReadInt32();
            data = new string[sizex][];
            for (int i = 0; i < sizex; i++)
            {
                data[i] = new string[sizey];
                for (int j = 0; j < sizey; j++)
                    data[i][j] = br.ReadString();
            }
        }
    }

