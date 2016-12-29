using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OBID;
using OBID.TagHandler;

namespace s2b_core
{
    class ReaderCommunicator
    {
        ////////////////// Variables And Objects //////////////////
        ///////////////////////////////////////////////////////////

        private static ReaderCommunicator _readerCommunicator;
        private readonly FedmIscReader _reader;   // needed for the communication with the reader
        private FedmIscReaderInfo _readerInfo; // TODO write get method to read info without knowing Fedm-Shit
        private readonly Dictionary<string, string> _scannedTags = new Dictionary<string, string>();
        private Task _taskScanner;

        private const int TableId = FedmIscReaderConst.ISO_TABLE;  //the tables used to communicate are ISO-Tables because ISO-Chips are used
        private const int TableSize = 10; // to have max 10 transponders per table (= inventory)
        private const string ReaderType = "USB";
        private const int IsoType = 15693; // tpye of the rfid-chips: ISO15693

        public bool Scanning;       // to check from outside if the Communicator is currently scanning for chips



        /////////////////////// Methods ///////////////////////
        ///////////////////////////////////////////////////////

            // Constuctor
        private ReaderCommunicator()
        {
            try
            {
                _reader = new FedmIscReader();
                Logger.GetInstance().Log("RC: FedmIscReader constructed");
            }
            catch (Exception e)
            {
                Logger.GetInstance().Log("RC: --EXCEPTION caught when generating a new FedmIscReader object: " + e.Message);
            }
           
            //TODO check if constructing was possible?
            _reader.SetTableSize(TableId, TableSize); // set the table used to the tableID and the dependent size
           Logger.GetInstance().Log("RC: Tablesize set");
        }

            // getInstance (call to get object)
        public static ReaderCommunicator GetInstance()
        {
            return _readerCommunicator ?? ( _readerCommunicator = new ReaderCommunicator() );
        }

        // Destructor
        ~ReaderCommunicator()
        {
            if (_reader.Connected) // checks if there is still a connection
            {
                try
                {
                    _reader.DisConnect(); // then disconnects if so
                    Logger.GetInstance().Log("RC: Reader disconnected");
                }
                catch (Exception e)
                {
                    Logger.GetInstance().Log("RC: --EXCEPTION caught when trying to disconnect the reader object: " + e.Message);
                }
            }
        }

        // connects to the reader (parameters may be changed to connect to something other than an usb-connected device on another bus than 255)
        public bool Connect()
        {
            if (_reader.Connected)      // reader already connected
                return true;    

            if (ReaderType.Equals("USB"))  // implemented for changing the type easier
            {
                try
                {
                    _reader.ConnectUSB(0); // 0 to get the first usb reader found is used 
                    Logger.GetInstance().Log($"RC: Called ConnectUSB. Reader connected: {_reader.Connected}");
                }
                catch (Exception e)
                {
                    Logger.GetInstance().Log("RC: --EXCEPTION caught when connecting to an usb reader: " + e.Message);
                }
            }

            if (_reader.Connected) // check if the connection could be established
            {
                try
                {
                    _readerInfo = _reader.ReadReaderInfo();
                    Logger.GetInstance().Log("RC: Received Reader Info: " + _readerInfo);
                }
                catch (Exception e)
                {
                    Logger.GetInstance().Log("RC: --EXCEPTION caught when reading info from connected reader: " + e.Message);
                   
                }

                return true;
            }
            else
                return false;

 
        }

        // disconnects the reader (needs to be down for the internals procedures of the dll)
        public bool Disconnect()
        {
            if (_reader.Connected) // check if the connection could be established
            {
                try
                {
                    _reader.DisConnect();
                    Logger.GetInstance().Log("RC: Reader disconnected from Device");
                }
                catch (Exception e)
                {
                   Logger.GetInstance().Log("RC: --EXCEPTION caught when trying to disconnect the reader object: " + e.Message);
                }

                return true;
            }
            else
                return false;

        }

        // activates the task which is constantly asking the reader for new scanned tags
        public void ActivateScan()
        {
            Scanning = true;

            _taskScanner = new Task(Scanner);    // starts a new task which constantly searches for new tags in the field
            _taskScanner.Start();
            Logger.GetInstance().Log("RC: Started Scanner-Task");
        }

        // deactivates the task which is constantly asking the reader for new scanned tags
        public void DeactivateScan()
        {
            Scanning = false;       // "order" Task to stop
            Logger.GetInstance().Log("RC: Deactivating Scanner-Task");
            _taskScanner.Wait();     // wait until it finally finishes, to not get several tasks running when deactivating and activating fast enough after each other
            Logger.GetInstance().Log("RC: Scanner-Task deactivated");
        }

        /* tasked method which searches for new tasks in the field of the reader
        adds only distinct chips the the list of scanned tags */
        private void Scanner()
        {
            while (Scanning)
            {
                try
                {
                    var tagList = _reader.TagInventory(true, 0x00, 1);

                    Logger.GetInstance().Log("SC: Tagged Inventory");

                    if (tagList.Count > 0)
                    {
                        Logger.GetInstance().Log($"tagList.Count = {tagList.Count}");
                        var listTagHandler = tagList.Values;

                        foreach (FedmIscTagHandler th in listTagHandler)
                        {
                            if (th == null) continue;

                            var tagHandler = th;

                            tagHandler = _reader.TagSelect(tagHandler, 0);
                            Logger.GetInstance().Log("SC: Called TagSelect");

                            if (!( tagHandler is FedmIscTagHandler_ISO15693 )) continue;

                            var scannedTag = (FedmIscTagHandler_ISO15693) tagHandler;

                            lock (_scannedTags)  // lock object because same list is used as return value in asynchronous main thread
                            {
                                if (!_scannedTags.ContainsKey(scannedTag.GetUid()))
                                {
                                        
                                    _scannedTags.Add(scannedTag.GetUid(), scannedTag.ToString());       // info about tag gets saved into list if its not already inside
                                }
                            }
                        }

                        System.Threading.Thread.Sleep(100); // if tags could have been found, wait for 100 ms // TODO maybe switch with event based waking up

                    }
                    else
                    {
                        Logger.GetInstance().Log("SC: No Tags received");
                        System.Threading.Thread.Sleep(200); // if no tags could have been found, wait for 200 ms // TODO maybe switch with event based waking up
                    }
                }
                catch (Exception e)
                {
                   Logger.GetInstance().Log("SC: --EXCEPTION caught while scanning: " + e.Message + e.GetType());
                }
            }
        }

        // returns a dictionary of the scanned tags until this moment since the last time calling the method or starting the scanner
        public Dictionary<string, string> GetScannedTags()
        {
            Dictionary<string, string> returnDictionary;

            lock (_scannedTags)  // lock because it is used by the asynchronous task taskScanner
            {
                returnDictionary = new Dictionary<string, string>(_scannedTags); // return copy of ScannedTags dic, so that the object local one can be reset
                //_scannedTags.Clear();    // clear object-local Dictionary  //TODO ausgeschaltet für testzwecke
            }

            return returnDictionary;
        }
    }
}
