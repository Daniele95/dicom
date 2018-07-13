using Avvio;
using rzdcxLib;
using System;
using System.IO;
using System.Windows;

namespace DicomParser
{
    class QueryRoniza
    {
        static String level = "";

        static DCXOBJ fillData (Selector sel)
        {
            DCXOBJ obj = new DCXOBJ();
            DCXELM el = new DCXELM();

            level = sel.QueryRetrieveLevel;

            el.Init((int)DICOM_TAGS_ENUM.QueryRetrieveLevel);
            el.Value = level;
            obj.insertElement(el);

            el.Init((int)DICOM_TAGS_ENUM.patientName);
            if (sel.patientName != "")
                el.Value = sel.patientName;
            obj.insertElement(el);
            
            if(level == "STUDY" || level == "SERIES" || level == "IMAGE") {
                el.Init((int)DICOM_TAGS_ENUM.StudyDescription);
                obj.insertElement(el);
            }

            if (level == "SERIES" || level == "IMAGE") {
                el.Init((int)DICOM_TAGS_ENUM.SeriesDescription);
                obj.insertElement(el);
            }

            if (level == "IMAGE") {
                el.Init((int)DICOM_TAGS_ENUM.sopClassUid);
                if (sel.sopClassUid != "")
                    el.Value = sel.sopClassUid;
                obj.insertElement(el);

                el.Init((int)DICOM_TAGS_ENUM.sopInstanceUID);
                if (sel.sopInstanceUID != "")
                    el.Value = sel.sopInstanceUID;
                obj.insertElement(el);
            }

            if (level == "STUDY") {
                el.Init((int)DICOM_TAGS_ENUM.studyInstanceUID);
                obj.insertElement(el);
            }

            if (level == "SERIES") {
                el.Init((int)DICOM_TAGS_ENUM.seriesInstanceUID);
                obj.insertElement(el);
            }
            
            return obj;

        }

        static String elementName(int elementNumber)
        {
            DICOM_TAGS_ENUM myEnum = (DICOM_TAGS_ENUM)elementNumber;
            return myEnum.ToString();
        }

        static DCXELM currElem;
        static String stampa(DCXOBJ currObj, int elementNumber)
        {
            try {
                currElem = currObj.getElementByTag(elementNumber);
            } catch (Exception e) { Console.WriteLine(e.Message); } //Tag Not Found
            return elementName(elementNumber) + ": " + currElem.Value+ " | ";
        }


        public static void find(Association ass, Selector sel)
        {
            DCXOBJ obj = fillData(sel);

            // Create the requester object
            DCXREQ req = new DCXREQ();
            // send the query 
            DCXOBJIterator it = req.Query(ass.myAET,
                           ass.TargetAET,
                           ass.TargetIp,
                           ass.TargetPort,
                           "1.2.840.10008.5.1.4.1.2.1.1",
                           obj);
            DCXOBJ currObj = null;
            try
            {
                // Iterate over the query results
                for (; !it.AtEnd(); it.Next())
                {
                    currObj = it.Get();
                    string message = "";
                    
                    message += stampa(currObj, (int)DICOM_TAGS_ENUM.patientName);
                    
                    if (level == "STUDY" || level == "SERIES" || level == "IMAGE")
                        message += stampa(currObj, (int)DICOM_TAGS_ENUM.StudyDescription);

                    if (level == "SERIES" || level == "IMAGE")
                        message += stampa(currObj, (int)DICOM_TAGS_ENUM.SeriesDescription);

                    if (level == "IMAGE")
                    {
                        message += stampa(currObj, (int)DICOM_TAGS_ENUM.sopClassUid);
                        message += stampa(currObj, (int)DICOM_TAGS_ENUM.sopInstanceUID);
                    }

                    if (level == "STUDY")
                        message += stampa(currObj, (int)DICOM_TAGS_ENUM.studyInstanceUID);                   

                    if (level == "SERIES")
                        message += stampa(currObj, (int)DICOM_TAGS_ENUM.seriesInstanceUID);
                    
                    Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }



        public static void moveAndStore(Association ass, Selector sel)
        {
            // Create an object with the query matching criteria (Identifier)
            DCXOBJ query = fillData(sel);

            // Create an accepter to handle the incomming association
            DCXACC accepter = new DCXACC();
            accepter.StoreDirectory = @"C:/Users/daniele/Desktop/provaDicom";
            Directory.CreateDirectory(accepter.StoreDirectory);

            // Create a requester and run the query
            DCXREQ requester = new DCXREQ();

            requester.MoveAndStore(
                ass.myAET, // The AE title that issue the C-MOVE
                ass.TargetAET,     // The PACS AE title
                ass.TargetIp,   // The PACS IP address
                ass.TargetPort,   // The PACS listener port
                ass.myAET, // The AE title to send the
                query,     // The matching criteria
                104,       // The port to receive the results
                accepter); // The accepter to handle the results

        }


    }
}
