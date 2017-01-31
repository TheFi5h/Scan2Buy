using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess;
using OBID;
using OBID.TagHandler;

namespace Domain
{
    public class ReaderCommunicator : IReaderCommunicator
    {
        ////////////////// Variables And Objects //////////////////
        ///////////////////////////////////////////////////////////

        private static ReaderCommunicator _readerCommunicator;
        private readonly FedmIscReader _reader;   // needed for the communication with the reader
        private FedmIscReaderInfo _readerInfo; // TODO write get method to read info without knowing Fedm-Shit
        private readonly List<TagData> _scannedTags = new List<TagData>();
        private Task _taskScanner;

        private const int TableId = FedmIscReaderConst.ISO_TABLE;  //the tables used to communicate are ISO-Tables because ISO-Chips are used
        private const int TableSize = 10; // to have max 10 transponders per table (= inventory)
        private const string ReaderType = "USB";
        private const int IsoType = 15693; // tpye of the rfid-chips: ISO15693

        public bool Scanning;       // to check from outside if the Communicator is currently scanning for chips


        public delegate void NewTagScannedEventHandler(TagData tagData);  // delegate to let others now, when a new tag was scanned
        public event NewTagScannedEventHandler NewTagScanned;

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

        // GetInstance (call to get object)
        public static IReaderCommunicator GetInstance()
        {
            return _readerCommunicator ?? ( _readerCommunicator = new ReaderCommunicator() );
        }

        // Destructor
        ~ReaderCommunicator()
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

        // Connects to the reader (parameters may be changed to connect to something other than an usb-connected device on another bus than 255)
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

        // Disconnects the reader (needs to be down for the internals procedures of the dll)
        public bool Disconnect()
        {
            if (_reader.Connected) // check if the connection could be established
            {
                try
                {
                    _reader.DisConnect();
                    Logger.GetInstance().Log("RC: Reader disconnected from Device");
                    return true;
                }
                catch (Exception e)
                {
                   Logger.GetInstance().Log("RC: --EXCEPTION caught when trying to disconnect the reader object: " + e.Message);
                }

                return false;
            }
            else
                return false;

        }

        // Activates the task which is constantly asking the reader for new scanned tags
        public void ActivateScan()
        {
            if (Scanning) return;   // Check if the task is already running

            Scanning = true;

            // starts a new task which constantly searches for new tags in the field
            _taskScanner = new Task(Scanner);
            _taskScanner.Start();

            Logger.GetInstance().Log("RC: Started Scanner-Task");
        }

        // Deactivates the task which is constantly asking the reader for new scanned tags
        public void DeactivateScan()
        {
            if (!Scanning) return;      // Check if the task isn't running

            Scanning = false;       // "order" Task to stop
            Logger.GetInstance().Log("RC: Deactivating Scanner-Task");
            _taskScanner.Wait();     // wait until it finally finishes, to not get several tasks running when deactivating and activating fast enough after each other
            Logger.GetInstance().Log("RC: Scanner-Task deactivated");
        }

        // returns a dictionary of the scanned tags until this moment since the last time calling the method or starting the scanner
        public IList<TagData> GetScannedTags()
        {
            List<TagData> returnList;

            lock (_scannedTags)  // lock because it is used by the asynchronous task taskScanner
            {
                returnList = new List<TagData>(_scannedTags);

                _scannedTags.Clear(); // clear object-local Dictionary
            }

            return returnList;
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
                            if (th == null)
                                continue;

                            var tagHandler = _reader.TagSelect(th, 0);
                            Logger.GetInstance().Log("SC: Called TagSelect");

                            // Check if the scanned tag is of the correct type
                            if (!(tagHandler is FedmIscTagHandler_ISO15693))
                                continue;

                            // Create TagData from tag
                            TagData scannedTag = FormatTagHandlerToTagData((FedmIscTagHandler_ISO15693)tagHandler);


                            lock (_scannedTags)  // lock object because same list is used as return value in asynchronous main thread
                            {
                                if (!_scannedTags.Contains(scannedTag))
                                {
                                    _scannedTags.Add(scannedTag);       // info about tag gets saved into list if its not already inside
                                    OnNewTagScanned(scannedTag);
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

        private void OnNewTagScanned(TagData scannedTag)
        {
            // Trigger event if the handler isnt null (if there are subscribers)
            NewTagScanned?.Invoke(scannedTag);
        }

        private TagData FormatTagHandlerToTagData(FedmIscTagHandler_ISO15693 tag)
        {
            return new TagData(tag.GetUid(), tag.GetManufacturerName());       //TODO ToString stuff right?
        }
    }
}
