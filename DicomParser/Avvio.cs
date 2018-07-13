using DicomParser;
using rzdcxLib;
using System;

namespace Avvio
{
    class Avvio
    {
        static Association MC;
        static Association PM;
        static Selector teodora;
        static Selector jane;
        static Selector provaGet;

        static void initAssociationsAndSelectors()
        {
            // Medical Connections Ltd.
            MC = new Association();
            MC.TargetIp = "www.dicomserver.co.uk";
            MC.TargetPort = 104;
            MC.TargetAET = "ANYAE";
            MC.myAET = "DANIELEAE";

            // spiridopoulou theodora of PixelMed
            jane = new Selector();
            jane.QueryRetrieveLevel = "PATIENT";
            jane.patientName = "Doe^Jane";


            // PixelMed Publishing Tm
            PM = new Association();
            PM.TargetIp = "184.73.255.26";
            PM.TargetPort = 11112;
            PM.TargetAET = "AWSPIXELMEDPUB";
            PM.myAET = "DANIELEAE";

            // spiridopoulou theodora of PixelMed
            teodora = new Selector();
            teodora.QueryRetrieveLevel = "IMAGE";
            teodora.patientName = "SPIRIDOPOULOU THEODORA";
            // teodora.sopClassUid = "1.2.840.10008.5.1.4.1.1.1.1";
            // teodora.sopInstanceUID = "1.3.6.1.4.1.5962.99.1.1533435149.813488060.1530541825293.354.0";

            provaGet = new Selector();
            provaGet.QueryRetrieveLevel = "PATIENT";
            //provaGet.sopClassUid = "1.2.840.10008.5.1.4.1.1.1.1";

        }


        static void Main(String[] args)
        {
            initAssociationsAndSelectors();
            Console.WriteLine("ciao");

            QueryRoniza.find(MC, provaGet);
            //  QueryRoniza.find(PM, teodora);
            //  QueryRoniza.find(MC, jane);
            //  System.Threading.Thread.Sleep(4000);
            // QueryRoniza.moveAndStore(PM, teodora);

        }

    }

    class Association
    {
        public String TargetIp;
        public ushort TargetPort;
        public String TargetAET;
        public String myAET;
    }


    class Selector
    {
        public String QueryRetrieveLevel;

        public String patientName = ""; // tutti i livelli

        public String sopClassUid = "";  // livello IMAGE
        public String sopInstanceUID = "";   // livello IMAGE

        public String StudyDescription = ""; // livello SERIES , STUDY, IMAGE
        public String SeriesDescription = ""; // livello SERIES, IMAGE

        public String studyInstanceUID = ""; // livello STUDY
        public String seriesInstanceUID = "";    // livello SERIES

    }

}
