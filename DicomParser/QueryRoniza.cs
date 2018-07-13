using Avvio;
using rzdcxLib;
using System;
using System.IO;
using System.Windows;

namespace DicomParser
{
    class QueryRoniza
    {
        static DCXOBJ fillData (Selector sel)
        {
            DCXOBJ obj = new DCXOBJ();
            DCXELM el = new DCXELM();

            el.Init((int)DICOM_TAGS_ENUM.QueryRetrieveLevel);
            el.Value = sel.QueryRetrieveLevel;
            obj.insertElement(el);

                el.Init((int)DICOM_TAGS_ENUM.patientName);
                el.Value = "SPIRIDOPOULOU THEODORA";
                obj.insertElement(el);
            el.Init((int)DICOM_TAGS_ENUM.StudyDescription);
            obj.insertElement(el);
            /*
            if (sel.patientID != "") {
                el.Init((int)DICOM_TAGS_ENUM.patientID);
                el.Value = sel.patientID;
                obj.insertElement(el);
            }

            el.Init((int)DICOM_TAGS_ENUM.sopClassUid);
            obj.insertElement(el);

            el.Init((int)DICOM_TAGS_ENUM.sopInstanceUID);
            obj.insertElement(el);

            el.Init((int)DICOM_TAGS_ENUM.studyInstanceUID);
            obj.insertElement(el);

            el.Init((int)DICOM_TAGS_ENUM.seriesInstanceUID);
            obj.insertElement(el);
            */
            return obj;

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
                    DCXELM currElem = currObj.getElementByTag((int)DICOM_TAGS_ENUM.StudyDescription);
                    if (currElem != null) message += " - sopClassUid: " + currElem.Value;
                    /*
                    currElem = currObj.getElementByTag((int)DICOM_TAGS_ENUM.sopInstanceUID);
                    if (currElem != null) message += " - sopInstanceUID: " + currElem.Value;

                    currElem = currObj.getElementByTag((int)DICOM_TAGS_ENUM.studyInstanceUID);
                    if (currElem != null) message += " - studyInstanceUID: " + currElem.Value;

                    currElem = currObj.getElementByTag((int)DICOM_TAGS_ENUM.seriesInstanceUID);
                    if (currElem != null) message += " - seriesInstanceUID: " + currElem.Value;
                    */
                    Console.WriteLine("ciao2");
                    Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }



        public void MoveAndStore(Association ass, Selector sel)
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
