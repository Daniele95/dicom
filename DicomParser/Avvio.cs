using DicomParser;
using System;

namespace Avvio
{
    class Avvio
    {
        static Association MC;
        static Association PM;
        static Selector teodora;

        static void initAssociationsAndSelectors()
        {
            // Medical Connections Ltd.
            MC = new Association();
            MC.TargetIp = "www.dicomserver.co.uk";
            MC.TargetPort = 104;
            MC.TargetAET = "ANYAE";
            MC.myAET = "DANIELEAE";

            // PixelMed Publishing Tm
            PM = new Association();
            PM.TargetIp = "184.73.255.26";
            PM.TargetPort = 11112;
            PM.TargetAET = "AWSPIXELMEDPUB";
            PM.myAET = "DANIELEAE";

            teodora = new Selector();
            teodora.QueryRetrieveLevel = "IMAGE";
            teodora.patientName = "SPIRIDOPOULOU THEODORA";
            teodora.patientID = @"1140731";
            teodora.StudyDescription = "Foot L";
            teodora.SeriesDescription = "Foot L";
        }


        static void Main(String[] args)
        {
            initAssociationsAndSelectors();
            Console.WriteLine("ciao");
            QueryRoniza.find(PM, teodora);
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

        public String patientName; // tutti i livelli
        public String patientID;

        public String sopClassUid;  // livello IMAGE
        public String sopInstanceUid;   // livello IMAGE

        public String StudyDescription; // livello SERIES , STUDY, IMAGE
        public String SeriesDescription; // livello SERIES, IMAGE,

        public String studyInstanceUID; // livello STUDY
        public String seriesInstanceUID;    // livello SERIES

    }

}
