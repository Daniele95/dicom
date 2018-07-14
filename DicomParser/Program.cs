using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DicomQuery;

namespace VolumeRendering.Raycast
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool CSHARPGL = false;

            String studyUID = "";
            if (!CSHARPGL) { 
                AvvioQuery myQuery = new AvvioQuery();
                studyUID = myQuery.avviaQuery();
                // "1.3.6.1.4.1.5962.99.1.1533435149.813488060.1530541825293.6.0";
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(studyUID, CSHARPGL));
            
        }
    }
}
