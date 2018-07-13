using DicomParser;
using rzdcxLib;
using System;
using VolumeRayCasting;

namespace Avvio
{
    class Avvio
    {
        static Association MC;
        static Association PM;
        static Selector teodora;
        static Selector john;

        static void initAssociationsAndSelectors()
        {
            // Medical Connections Ltd.
            MC = new Association();
            MC.TargetIp = "www.dicomserver.co.uk";
            MC.TargetPort = 104;
            MC.TargetAET = "ANYAE";
            MC.myAET = "DANIELEAE";

            // doe john of medical connections
            john = new Selector();
            john.QueryRetrieveLevel = "PATIENT";
            // john.patientName = "Doe^John";
            // john.sopInstanceUID = "1.3.6.1.4.1.5962.1.1.0.0.0.1184731235.1361.0.13";
            john.sopClassUid = "1.2.840.10008.5.1.4.1.1.4";

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
            
        }

        static void Main(String[] args)
        {
            using (VolumetricRayCasting game = new VolumetricRayCasting())
            {
                game.Run();
            }


            //   initAssociationsAndSelectors();

            // QueryRoniza.find(MC, john);
            //   QueryRoniza.find(PM, teodora);

            // System.Threading.Thread.Sleep(4000);
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
