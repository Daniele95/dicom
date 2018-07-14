using rzdcxLib;
using System;

namespace DicomQuery
{
    class AvvioQuery
    {
        static Association MC;
        static Association PM;
        static Selector teodora;
        static Selector john;
        static String studyUID = "";

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
            teodora.QueryRetrieveLevel = "SERIES";
            teodora.patientName = "SPIRIDOPOULOU THEODORA";
            // teodora.sopClassUid = "1.2.840.10008.5.1.4.1.1.1.1";
            // teodora.sopInstanceUID = "1.3.6.1.4.1.5962.99.1.1533435149.813488060.1530541825293.354.0";

        }
        
        public static String getSeriesFromStudy(string studyInstanceUid)
        {
            studyUID = studyInstanceUid;
            teodora.studyInstanceUID = studyInstanceUid;
            // 1.3.6.1.4.1.5962.99.1.1533435149.813488060.1530541825293.71.0
            String seriesInstanceUID = QueryRoniza.find(PM, teodora);
            return seriesInstanceUID;
        }

        public String avviaQuery()
        {
            // QueryRoniza.find(MC, john);

            initAssociationsAndSelectors();

            QueryFoDicom fellowOak = new QueryFoDicom(PM);

            String studyInstanceUID = fellowOak.CFind(teodora);

            String seriesInstanceUID = getSeriesFromStudy(studyInstanceUID);

            fellowOak.MoveAndStore(teodora, seriesInstanceUID, studyUID);

            return studyUID;
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
